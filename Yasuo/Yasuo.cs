using System;
using System.Collections.Generic;
using System.Linq;
using BrianSharp.Common;
using BrianSharp.Evade;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
using SkillShotType = EloBuddy.SDK.Enumerations.SkillShotType;

namespace BrianSharp.Plugin
{
    internal class Yasuo : Helper
    {
        #region Constants

        private const int QRange = 550, Q2Range = 1150, QCirWidth = 275, QCirWidthMin = 250, RWidth = 400;

        #endregion

        public static IsSafeResult IsSafePoint(Vector2 point)
        {
            var result = new IsSafeResult {SkillshotList = new List<Skillshot>()};
            foreach (var skillshot in
                SkillshotDetector.DetectedSkillshots.Where(i => i.Evade && !i.IsSafePoint(point)))
            {
                result.SkillshotList.Add(skillshot);
            }
            result.IsSafe = result.SkillshotList.Count == 0;
            return result;
        }

        internal struct IsSafeResult
        {
            #region Fields

            public bool IsSafe;

            public List<Skillshot> SkillshotList;

            #endregion
        }

        #region Constructors and Destructors

        private static Spell.Skillshot Q, Q2;
        private static Spell.SpellBase W;
        private static Spell.Targeted E;
        private static Spell.Active R;

        private static Menu comboMenu,
            harassMenu,
            clearMenu,
            lastHitMenu,
            fleeMenu,
            miscMenu,
            killStealMenu,
            interruptMenu,
            drawMenu;

        public static void Yasuoo()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 500, SkillShotType.Linear, (int) GetQDelay,
                int.MaxValue, 20);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, (int) GetQ2Delay,
                1500, 90);
            E = new Spell.Targeted(SpellSlot.E, 475);
            R = new Spell.Active(SpellSlot.R, 1300);

            Menu = MainMenu.AddMenu("YasuoYasuoYasuo", "Yasuo");
            Menu.AddLabel("Another port from Rexy");
            Menu.AddLabel("Huehuehuehuehuehue");


            comboMenu = Menu.AddSubMenu("Combo", "Combo");
            AddBool(comboMenu, "Q", "Use Q");
            AddBool(comboMenu, "QStack", "-> Stack Q While Gap (E Gap Must On)", false);
            AddBool(comboMenu, "E", "Use E");
            AddBool(comboMenu, "EDmg", "-> Q3 Circle (Q Must On)");
            AddBool(comboMenu, "EGap", "-> Gap Closer");
            AddSlider(comboMenu, "EGapRange", "-> If Enemy Not In", 300, 1, 475);
            AddBool(comboMenu, "EGapTower", "-> Under Tower", false);
            AddBool(comboMenu, "R", "Use R");
            AddBool(comboMenu, "RDelay", "-> Delay");
            AddSlider(comboMenu, "RHpU", "-> If Enemy Hp <", 60);
            AddSlider(comboMenu, "RCountA", "-> Or Enemy >=", 2, 1, 5);

            harassMenu = Menu.AddSubMenu("Harass", "Harass");
            AddKeybind(harassMenu, "AutoQ", "Auto Q", 'H', KeyBind.BindTypes.PressToggle, true);
            AddBool(harassMenu, "AutoQ3", "-> Use Q3", false);
            AddBool(harassMenu, "AutoQTower", "-> Under Tower", false);
            AddBool(harassMenu, "Q", "Use Q");
            AddBool(harassMenu, "Q3", "-> Use Q3");
            AddBool(harassMenu, "QTower", "-> Under Tower");
            AddBool(harassMenu, "QLastHit", "-> Last Hit (Q1/Q2)");
            clearMenu = Menu.AddSubMenu("Clear", "Clear");

            AddBool(clearMenu, "Q", "Use Q");
            AddBool(clearMenu, "Q3", "-> Use Q3");
            AddBool(clearMenu, "E", "Use E");
            AddBool(clearMenu, "ETower", "-> Under Tower", false);
            AddBool(clearMenu, "Item", "Use Tiamat/Hydra Item");

            lastHitMenu = Menu.AddSubMenu("Last Hit", "LastHit");
            AddBool(lastHitMenu, "Q", "Use Q");
            AddBool(lastHitMenu, "Q3", "-> Use Q3", false);
            AddBool(lastHitMenu, "E", "Use E");
            AddBool(lastHitMenu, "ETower", "-> Under Tower", false);

            fleeMenu = Menu.AddSubMenu("Flee", "Flee");

            AddBool(fleeMenu, "E", "Use E");
            AddBool(fleeMenu, "EStackQ", "-> Stack Q While Dashing");

            killStealMenu = Menu.AddSubMenu("Kill Steal", "KillSteal");

            AddBool(killStealMenu, "Q", "Use Q");
            AddBool(killStealMenu, "E", "Use E");
            AddBool(killStealMenu, "R", "Use R");
            AddBool(killStealMenu, "Ignite", "Use Ignite");

            miscMenu = Menu.AddSubMenu("Misc", "Misc");

            AddKeybind(miscMenu, "StackQ", "Auto Stack Q", 'Z', KeyBind.BindTypes.PressToggle);
            AddBool(miscMenu, "StackQDraw", "-> Draw Text");

            drawMenu = Menu.AddSubMenu("Draw", "Draw");

            AddBool(drawMenu, "Q", "Q Range", false);
            AddBool(drawMenu, "E", "E Range", false);
            AddBool(drawMenu, "R", "R Range", false);

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterruptableSpell += OnPossibleToInterrupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (miscMenu["StackQ"].Cast<KeyBind>().CurrentValue && miscMenu["StackQDraw"].Cast<CheckBox>().CurrentValue)
            {
                var pos = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos.X, pos.Y, Color.Orange, "Auto Stack Q");
            }
            if (drawMenu["Q"].Cast<CheckBox>().CurrentValue && Q.Level > 0)
            {
                Drawing.DrawCircle(
                    Player.Position,
                    Player.IsDashing() ? QCirWidth : (!HaveQ3 ? Q : Q2).Range,
                    Q.IsReady() ? Color.Green : Color.Red);
            }
            if (drawMenu["E"].Cast<CheckBox>().CurrentValue && E.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
            }
            if (drawMenu["R"].Cast<CheckBox>().CurrentValue && R.Level > 0)
            {
                Drawing.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
            }
        }

        #endregion

        #region Properties

        private static float GetQ2Delay
        {
            get { return 0.5f*(1 - Math.Min((Player.AttackSpeedMod - 1)*0.58f, 0.66f)); }
        }

        private static float GetQDelay
        {
            get { return 0.4f*(1 - Math.Min((Player.AttackSpeedMod - 1)*0.58f, 0.66f)); }
        }

        private static bool HaveQ3
        {
            get { return Player.HasBuff("YasuoQ3W"); }
        }

        private static AIHeroClient QCirTarget
        {
            get
            {
                var pos = Player.GetDashInfo().EndPos;
                var target = TargetSelector.GetTarget(QCirWidth, DamageType.Physical, pos);
                return target != null && Player.Distance(pos) < 150 ? target : null;
            }
        }

        #endregion

        #region Methods

        private static void AutoQ()
        {
            if (!harassMenu["AutoQ"].Cast<CheckBox>().CurrentValue || Player.IsDashing()
                || (HaveQ3 && !harassMenu["AutoQ3"].Cast<CheckBox>().CurrentValue)
                || (UnderTower(Player.ServerPosition) && !harassMenu["AutoQTower"].Cast<CheckBox>().CurrentValue))
            {
                return;
            }
            var target = TargetSelector.GetTarget(!HaveQ3 ? QRange : Q2Range, DamageType.Physical);
            if (target == null)
            {
                return;
            }
            (!HaveQ3 ? Q : Q2).Cast(target);
        }

        private static bool CanCastE(Obj_AI_Base target)
        {
            return !target.HasBuff("YasuoDashWrapper");
        }

        private static bool CanCastR(AIHeroClient target)
        {
            return target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback);
        }

        private static bool CastQCir(Obj_AI_Base target)
        {
            return target.IsValidTarget(QCirWidthMin - target.BoundingRadius) && Q.Cast(Game.CursorPos);
        }

        private static void Clear()
        {
            if (clearMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var minionObj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, E.Range)
                        .Where(
                            i =>
                                CanCastE(i) &&
                                (!UnderTower(PosAfterE(i)) || clearMenu["ETower"].Cast<CheckBox>().CurrentValue))
                        .ToList();

                if (minionObj.Any())
                {
                    var obj = minionObj.FirstOrDefault(i => CanKill(i, GetEDmg(i)));
                    if (obj == null && clearMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady(50)
                        && (!HaveQ3 || clearMenu["Q3"].Cast<CheckBox>().CurrentValue))
                        obj = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.ServerPosition, E.Range)
                            .Where(i => i.IsValidTarget(QCirWidth))
                            .FirstOrDefault();

                    if (obj != null && E.Cast(obj))
                    {
                        return;
                    }
                }
            }
            if (clearMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
                (!HaveQ3 || clearMenu["Q3"].Cast<CheckBox>().CurrentValue))
            {
                if (Player.IsDashing())
                {
                    var minionObj =
                        EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.GetDashInfo().EndPos, QCirWidth);

                    if ((minionObj.Any(i => CanKill(i, GetQDmg(i)) || i.Team == GameObjectTeam.Neutral)
                         || minionObj.Count() > 1) && Player.Distance(Player.GetDashInfo().EndPos) < 150
                        && CastQCir(minionObj.FirstOrDefault()))
                    {
                        return;
                    }
                }
                else
                {
                    var minionObj =
                        EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.Position, !HaveQ3 ? QRange : Q2Range);

                    if (minionObj.Any())
                    {
                        if (!HaveQ3)
                        {
                            var obj = minionObj.FirstOrDefault(i => CanKill(i, GetQDmg(i)));
                            if (obj != null && obj.HasBuff("YasuoQ"))
                            {
                                return;
                            }
                        }
                        var qMinHit = Q.MinimumHitChance;
                        Q.MinimumHitChance = HitChance.Medium;
                        var pos = (!HaveQ3 ? Q : Q2).GetPrediction((Obj_AI_Base) minionObj);
                            //.GetLineFarmLocation(minionObj.Cast<Obj_AI_Base>().ToList());
                        Q.MinimumHitChance = qMinHit;
                        if (pos.CollisionObjects.Count() > 0 && (!HaveQ3 ? Q : Q2).Cast(pos.CastPosition))
                        {
                            return;
                        }
                    }
                }
            }
            if (clearMenu["Item"].Cast<CheckBox>().CurrentValue && (Hydra.IsReady() || Tiamat.IsReady()))
            {
                var minionObj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, Hydra.IsReady() ? Hydra.Range : Tiamat.Range);
                if (minionObj.Count() > 2
                    || minionObj.Any(
                        i =>
                            i.MaxHealth >= 1200 &&
                            i.Distance(Player) < (Hydra.IsReady() ? Hydra : Tiamat).Range - 80))
                {
                    if (Tiamat.IsReady())
                    {
                        Tiamat.Cast();
                    }
                    if (Hydra.IsReady())
                    {
                        Hydra.Cast();
                    }
                }
            }
        }


        private static void Fight(Orbwalker.ActiveModes mode)
        {
            if (mode == Orbwalker.ActiveModes.Combo)
            {
                if (comboMenu["R"].Cast<CheckBox>().CurrentValue && R.IsReady())
                {
                    var obj =
                        (from enemy in EntityManager.Heroes.Enemies.Where(i => R.IsInRange(i) && CanCastR(i))
                            //HeroManager.Enemies.Where(i => R.IsInRange(i) && CanCastR(i))
                            let sub = enemy.CountEnemiesInRange(RWidth)
                            where
                                (sub > 1 && Player.GetSpellDamage(enemy, SpellSlot.R) > enemy.Health)
                            orderby sub descending
                            select enemy).ToList();
                    if (obj.Any())
                    {
                        var target = !comboMenu["RDelay"].Cast<CheckBox>().CurrentValue
                            ? obj.FirstOrDefault()
                            : obj.Where(i => TimeLeftR(i)*1000 < 150 + Game.Ping*2).FirstOrDefault();
                        if (target != null && R.Cast(target))
                        {
                            return;
                        }
                    }
                }
                if (comboMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady())
                {
                    if (comboMenu["EDmg"].Cast<CheckBox>().CurrentValue && comboMenu["Q"].Cast<CheckBox>().CurrentValue &&
                        HaveQ3 && Q.IsReady(50))
                    {
                        var target = TargetSelector.GetTarget(QRange, DamageType.Physical);
                        if (target != null)
                        {
                            var obj = GetNearObj(target, true);
                            if (obj != null && E.Cast(obj))
                            {
                                return;
                            }
                        }
                    }
                    if (comboMenu["EGap"].Cast<CheckBox>().CurrentValue)
                    {
                        var target = TargetSelector.GetTarget(QRange, DamageType.Physical)
                                     ?? TargetSelector.GetTarget(Q2Range, DamageType.Physical);
                        if (target != null)
                        {
                            var obj = GetNearObj(target);
                            if (obj != null
                                && (obj.NetworkId != target.NetworkId
                                    ? Player.Distance(target) > comboMenu["EGapRange"].Cast<Slider>().CurrentValue
                                    : !target.IsInAutoAttackRange(Player))
                                && (!UnderTower(PosAfterE(obj)) || comboMenu["EGapTower"].Cast<CheckBox>().CurrentValue)
                                && E.Cast(obj))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            if (comboMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                if (mode == Orbwalker.ActiveModes.Combo
                    || ((!HaveQ3 || comboMenu["Q3"].Cast<CheckBox>().CurrentValue)
                        && (!UnderTower(Player.ServerPosition) || comboMenu["QTower"].Cast<CheckBox>().CurrentValue)))
                {
                    if (Player.IsDashing())
                    {
                        if (QCirTarget != null && CastQCir(QCirTarget))
                        {
                            return;
                        }
                        if (!HaveQ3 && mode == Orbwalker.ActiveModes.Combo &&
                            comboMenu["QStack"].Cast<CheckBox>().CurrentValue &&
                            comboMenu["E"].Cast<CheckBox>().CurrentValue
                            && comboMenu["EGap"].Cast<CheckBox>().CurrentValue &&
                            TargetSelector.GetTarget(200, DamageType.Physical) == null)
                        {
                            var minionObj =
                                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                                    EntityManager.UnitTeam.Enemy, Player.GetDashInfo().EndPos, QCirWidth);
                            if (minionObj.Any() && Player.Distance(Player.GetDashInfo().EndPos) < 150
                                && CastQCir((Obj_AI_Base) (minionObj.Where(i => i.Distance(Player) < Q.Range))))
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        var target = TargetSelector.GetTarget(
                            !HaveQ3 ? QRange : Q2Range,
                            DamageType.Physical);
                        if (target != null)
                        {
                            if (!HaveQ3)
                            {
                                if (target.HasBuff("YasuoQ"))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                var hit = -1;
                                var predPos = new Vector3();
                                foreach (var hero in EntityManager.Heroes.Enemies.Where(i => i.IsValidTarget(Q2Range)))
                                {
                                    var pred = Q2.GetPrediction(hero);
                                    if (pred.HitChance >= Q2.MinimumHitChance && pred.CollisionObjects.Count() > hit)
                                    {
                                        hit = pred.CollisionObjects.Count();
                                        predPos = pred.CastPosition;
                                    }
                                }
                                if (predPos.IsValid())
                                {
                                    if (Q2.Cast(predPos))
                                    {
                                        return;
                                    }
                                }
                                else if (target.HasBuff("YasuoQ2"))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                if (mode == Orbwalker.ActiveModes.Harass && harassMenu["QLastHit"].Cast<CheckBox>().CurrentValue &&
                    TargetSelector.GetTarget(100, DamageType.Physical) == null && !HaveQ3
                    && !Player.IsDashing())
                {
                    var obj =
                        EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.Position, Q.Range)
                            .FirstOrDefault(i => CanKill(i, GetQDmg(i)));
                    if (obj != null)
                    {
                        Q.Cast(obj);
                    }
                }
            }
        }

        private static void Flee()
        {
            if (!fleeMenu["E"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (fleeMenu["EStackQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && !HaveQ3 && Player.IsDashing())
            {
                if (QCirTarget != null && CastQCir(QCirTarget))
                {
                    return;
                }
                var minionObj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.GetDashInfo().EndPos, QCirWidth);
                if (minionObj.Any() && Player.Distance(Player.GetDashInfo().EndPos) < 150
                    && CastQCir(minionObj.FirstOrDefault(i => i.Distance(Player) < Q.Range)))
                {
                    return;
                }
            }
            var obj = GetNearObj();
            if (obj == null || !E.IsReady())
            {
                return;
            }
            E.Cast(obj);
        }

        private static double GetEDmg(Obj_AI_Base target)
        {
            return Player.CalculateDamageOnUnit(
                target,
                DamageType.Magical,
                (float) ((50 + 20*E.Level)*(1 + Math.Max(0, Player.GetBuffCount("YasuoDashScalar")*0.25))
                         + 0.6*Player.FlatMagicDamageMod));
        }

        private static Obj_AI_Base GetNearObj(Obj_AI_Base target = null, bool inQCir = false)
        {
            var pos = target != null
                ? Prediction.Position.PredictUnitPosition(target, (int) (E.CastDelay + E.Handle.SData.MissileSpeed))
                    .To3D()
                : Game.CursorPos;
            var obj = new List<Obj_AI_Base>();
            obj.AddRange(EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                EntityManager.UnitTeam.Enemy, Player.Position, E.Range));
            obj.AddRange(EntityManager.Heroes.Enemies.Where(i => i.IsValidTarget(E.Range)));
            return
                obj.Where(
                    i =>
                        CanCastE(i) && pos.Distance(PosAfterE(i)) < (inQCir ? QCirWidthMin : Player.Distance(pos))
                    )
                    .FirstOrDefault(i => pos.Distance(PosAfterE(i)) < E.Range);
        }

        private static double GetQDmg(Obj_AI_Base target)
        {
            var dmgItem = 0d;
            if (Sheen.IsOwned() && (Sheen.IsReady() || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage;
            }
            if (Trinity.IsOwned() && (Trinity.IsReady() || Player.HasBuff("Sheen")))
            {
                dmgItem = Player.BaseAttackDamage*2;
            }
            var k = 1d;
            var reduction = 0d;
            var dmg = Player.TotalAttackDamage*(Player.Crit >= 0.85f ? (Item.HasItem(3031) ? 1.875 : 1.5) : 1)
                      + dmgItem;
            if (Item.HasItem(3153))
            {
                var dmgBotrk = Math.Max(0.08*target.Health, 10);
                if (target.IsValid())
                {
                    dmgBotrk = Math.Min(dmgBotrk, 60);
                }
                dmg += dmgBotrk;
            }
            if (target.IsValid())
            {
                var hero = (AIHeroClient) target;
                if (Item.HasItem(3047, hero))
                {
                    k *= 0.9d;
                }
                if (hero.ChampionName == "Fizz")
                {
                    reduction += hero.Level > 15
                        ? 14
                        : (hero.Level > 12
                            ? 12
                            : (hero.Level > 9 ? 10 : (hero.Level > 6 ? 8 : (hero.Level > 3 ? 6 : 4))));
                }
                var mastery = hero.Masteries.FirstOrDefault(m => m.Page == MasteryPage.Defense && m.Id == 65);
                if (mastery != null && mastery.Points > 0)
                {
                    reduction += 1*mastery.Points;
                }
            }
            return Player.CalculateDamageOnUnit(target, DamageType.Physical, (int) (20*Q.Level + (dmg - reduction)*k))
                   + (Player.GetBuffCount("ItemStatikShankCharge") == 100
                       ? Player.CalculateDamageOnUnit(
                           target,
                           DamageType.Magical,
                           (int) (100*(Player.Crit >= 0.85f ? (Item.HasItem(3031) ? 2.25 : 1.8) : 1)))
                       : 0);
        }

        private static void KillSteal()
        {
            if (killStealMenu["Ignite"].Cast<CheckBox>().CurrentValue && Player.Spellbook.GetSpell(Ignite).IsReady)
            {
                var target = TargetSelector.GetTarget(600, DamageType.True);
                if (target != null && CastIgnite(target))
                {
                    return;
                }
            }
            if (killStealMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                if (Player.IsDashing())
                {
                    var target = QCirTarget;
                    if (target != null && CanKill(target, GetQDmg(target)) && CastQCir(target))
                    {
                        return;
                    }
                }
                else
                {
                    var target = TargetSelector.GetTarget(
                        !HaveQ3 ? QRange : Q2Range,
                        DamageType.Physical);
                    if (target != null && CanKill(target, GetQDmg(target))
                        && (!HaveQ3) && target.HasBuff("YasuoQ") || target.HasBuff("YasuoQ2"))
                    {
                        return;
                    }
                }
            }
            if (killStealMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (target != null
                    && (CanKill(target, GetEDmg(target))
                        || (killStealMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady(50)
                            && CanKill(target, GetEDmg(target) + GetQDmg(target)))) && E.Cast(target))
                {
                    return;
                }
            }
            if (killStealMenu["R"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target != null && Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
                {
                    R.Cast(target);
                }
            }
        }

        private static void LastHit()
        {
            if (lastHitMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady() && !Player.IsDashing()
                && (!HaveQ3 || lastHitMenu["Q3"].Cast<CheckBox>().CurrentValue))
            {
                var obj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, !HaveQ3 ? QRange : Q2Range)
                        .FirstOrDefault(i => CanKill(i, GetQDmg(i)));
                if (obj != null && (!HaveQ3) && obj.HasBuff("YasuoQ") || obj.HasBuff("YasuoQ2"))
                {
                    return;
                }
            }
            if (lastHitMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                var obj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, E.Range).Where(i =>
                            CanCastE(i)
                            && (!i.IsInAutoAttackRange(Player) || i.Health > Player.GetAutoAttackDamage(i, true))
                            && (!UnderTower(PosAfterE(i)) || lastHitMenu["ETower"].Cast<CheckBox>().CurrentValue))
                        .FirstOrDefault(i => i.IsValidTarget(E.Range));

                if (obj != null)
                {
                    E.Cast(obj);
                }
            }
        }

        private static void OnPossibleToInterrupt(Obj_AI_Base unit, Interrupter.InterruptableSpellEventArgs spell)
        {
            if (Player.IsDead || !interruptMenu["Q"].Cast<CheckBox>().CurrentValue
                || !interruptMenu[unit.CharData.BaseSkinName + "_" + spell.Slot].Cast<CheckBox>().CurrentValue ||
                !HaveQ3)
            {
                return;
            }
            if (E.IsReady() && Q.IsReady(50))
            {
                if (E.IsInRange(unit) && CanCastE(unit) && unit.Distance(PosAfterE(unit)) < QCirWidthMin
                    && E.Cast(unit))
                {
                    return;
                }
                if (unit.IsInRange(unit, E.Range + QCirWidthMin))
                {
                    var obj = GetNearObj(unit, true);
                    if (obj != null && E.Cast(obj))
                    {
                        return;
                    }
                }
            }
            if (!Q.IsReady())
            {
                return;
            }
            if (Player.IsDashing())
            {
                var pos = Player.GetDashInfo().EndPos;
                if (Player.Distance(pos) < 150 && unit.Distance(pos) < QCirWidth)
                {
                    CastQCir(unit);
                }
            }
            else
            {
                Q2.Cast(unit);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (!Equals(Q.CastDelay, GetQDelay))
            {
                Q.CastDelay = (int) GetQDelay;
            }
            if (!Equals(Q2.CastDelay, GetQ2Delay))
            {
                Q2.CastDelay = (int) GetQ2Delay;
            }
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                return;
            }
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Fight(Orbwalker.ActiveModes.Combo);
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Fight(Orbwalker.ActiveModes.Harass);
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    Clear();
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    LastHit();
                    break;
                case Orbwalker.ActiveModes.Flee:
                    Flee();
                    break;
            }
            AutoQ();
            KillSteal();
            StackQ();
        }

        private static Vector3 PosAfterE(Obj_AI_Base target)
        {
            return Player.ServerPosition.Extend(
                target.ServerPosition,
                Player.Distance(target) < 410 ? E.Range : Player.Distance(target) + 65).To3D();
        }

        private static void StackQ()
        {
            if (!miscMenu["StackQ"].Cast<CheckBox>().CurrentValue || !Q.IsReady() || Player.IsDashing() || HaveQ3)
            {
                return;
            }
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null && (!UnderTower(Player.ServerPosition) || !UnderTower(target.ServerPosition)))
            {
                Q.Cast(target);
            }
            else
            {
                var minionObj = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                    EntityManager.UnitTeam.Enemy, Player.ServerPosition, QRange);
                if (!minionObj.Any())
                {
                    return;
                }
                var obj = minionObj.FirstOrDefault(i => CanKill(i, GetQDmg(i)))
                          ?? minionObj.FirstOrDefault(i => i.Distance(Player) < Q.Range);
                if (obj != null)
                {
                    var pred = Q.GetPrediction(obj);

                    if (pred.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
        }

        private static float TimeLeftR(AIHeroClient target)
        {
            var buff = target.Buffs.FirstOrDefault(i => i.Type == BuffType.Knockback || i.Type == BuffType.Knockup);
            return buff != null ? buff.EndTime - Game.Time : -1;
        }

        private static bool UnderTower(Vector3 pos)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(i => i.IsEnemy && !i.IsDead && i.Distance(pos) < 850 + Player.BoundingRadius);
        }

        #endregion
    }


    internal class SpellData
    {
        #region Public Properties

        public string MissileName
        {
            get { return SpellNames.First(); }
        }

        #endregion

        #region Fields

        public string ChampionName;

        public SpellSlot Slot;

        public string[] SpellNames = {};

        #endregion
    }

    internal class Targets
    {
        #region Fields

        public MissileClient Obj;

        public Vector3 Start;

        #endregion
    }
}