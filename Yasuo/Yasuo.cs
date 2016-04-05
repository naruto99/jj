using System;
using System.Collections.Generic;
using System.Linq;
using BrianSharp.Common;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
using SkillShotType = EloBuddy.SDK.Enumerations.SkillShotType;

namespace BrianSharp
{
    internal class Yasuo : Helper
    {
        #region Constants

        private const int QRange = 550, Q2Range = 1150, QCirWidth = 275, QCirWidthMin = 250, RWidth = 400;

        #endregion

        public static IsSafeResult IsSafePoint(Vector2 point)
        {
            var result = new IsSafeResult {SkillshotList = new List<Spell.Skillshot>()};
            
            result.IsSafe = result.SkillshotList.Count == 0;
            return result;
        }

        internal struct IsSafeResult
        {
            #region Fields

            public bool IsSafe;

            public List<Spell.Skillshot> SkillshotList;

            #endregion
        }

        public static bool CastQCir(Obj_AI_Base target)
        {
            return target.IsValidTarget(QCirWidthMin - target.BoundingRadius) && _q.Cast(Game.CursorPos);
        }

        #region Constructors and Destructors

        private static Spell.Skillshot _q, _q2;
        private static Spell.Targeted _e;
        private static Spell.Active _r;

        public static SpellSlot Flash, Smite, Ignite;


        public static Item Tiamat, Hydra, Youmuu, Zhonya, Seraph, Sheen, Iceborn, Trinity;
        public static Menu Menu;

        public static void Yasuoo()
        {
            _q = new Spell.Skillshot(SpellSlot.Q, 500, SkillShotType.Linear);
            _q2 = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, (int) GetQ2Delay,
                1500, 90);
            _e = new Spell.Targeted(SpellSlot.E, 475);
            _r = new Spell.Active(SpellSlot.R, 1300);

            Menu = MainMenu.AddMenu("YasuoXD", "Yasuo");
            Menu.AddLabel("Another port from Rexy");
            Menu.AddLabel("Huehuehuehuehuehue");


            Menu.AddLabel("Combo");
            AddBool(Menu, "Q", "Use Q");
            AddBool(Menu, "QStack", "-> Stack Q While Gap (E Gap Must On)", false);
            AddBool(Menu, "E", "Use E");
            AddBool(Menu, "EDmg", "-> Q3 Circle (Q Must On)");
            AddBool(Menu, "EGap", "-> Gap Closer");
            AddSlider(Menu, "EGapRange", "-> If Enemy Not In", 300, 1, 475);
            AddBool(Menu, "EGapTower", "-> Under Tower", false);
            AddBool(Menu, "R", "Use R");
            AddBool(Menu, "RDelay", "-> Delay");
            AddSlider(Menu, "RHpU", "-> If Enemy Hp <", 60);
            AddSlider(Menu, "RCountA", "-> Or Enemy >=", 2, 1, 5);

            Menu.AddLabel("Harass");
            AddBool(Menu, "HarQ", "Use Q");
            AddBool(Menu, "HarQ3", "-> Use Q3");
            AddBool(Menu, "HarQTower", "-> Under Tower");
            AddBool(Menu, "HarQLastHit", "-> Last Hit (Q1/Q2)");
            

            Menu.AddLabel("Clear");
            AddBool(Menu, "ClQ", "Use Q");
            AddBool(Menu, "ClQ3", "-> Use Q3");
            AddBool(Menu, "ClE", "Use E");
            AddBool(Menu, "ClETower", "-> Under Tower", false);
            AddBool(Menu, "ClItem", "Use Tiamat/Hydra Item");

            Menu.AddLabel("Last Hit");
            AddBool(Menu, "LhQ", "Use Q");
            AddBool(Menu, "LhQ3", "-> Use Q3", false);
            AddBool(Menu, "LhE", "Use E");
            AddBool(Menu, "LhETower", "-> Under Tower", false);

            Menu.AddLabel("Flee");

            AddBool(Menu, "FlE", "Use E");
            AddBool(Menu, "FlEStackQ", "-> Stack Q While Dashing");

            Menu.AddLabel("Draw");

            AddBool(Menu, "DrQ", "Q Range", false);
            AddBool(Menu, "DrE", "E Range", false);
            AddBool(Menu, "DrR", "R Range", false);

            Tiamat = new Item(ItemId.Tiamat_Melee_Only);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only);
            Youmuu = new Item(ItemId.Youmuus_Ghostblade);
            Zhonya = new Item(ItemId.Zhonyas_Hourglass);
            Seraph = new Item(ItemId.Seraphs_Embrace);
            Sheen = new Item(ItemId.Sheen);
            Iceborn = new Item(ItemId.Iceborn_Gauntlet);
            Trinity = new Item(ItemId.Trinity_Force);
            Flash = Player.GetSpellSlotFromName("summonerflash");
            foreach (var spell in
                Player.Spellbook.Spells.Where(
                    i =>
                        i.Name.ToLower().Contains("smite")
                        && (i.Slot == SpellSlot.Summoner1 || i.Slot == SpellSlot.Summoner2)))
            {
                Smite = spell.Slot;
            }
            Ignite = Player.GetSpellSlotFromName("summonerdot");


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (Menu["DrQ"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(Player.Position,_e.Range,Color.White);
            }
            if (Menu["DrE"].Cast<CheckBox>().CurrentValue)
            {
                Drawing.DrawCircle(Player.Position, _e.Range, Color.White);
            }
           if (Menu["DrR"].Cast<CheckBox>().CurrentValue)
{
                Drawing.DrawCircle(Player.Position, _r.Range,Color.White);
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling())
            {
                return;
            }
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Fight();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Fight();
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
        }
        #endregion

        #region Properties

        private static float GetQ2Delay
        {
            get { return 0.5f*(1 - Math.Min((Player.AttackSpeedMod - 1)*0.58f, 0.66f)); }
        }

        private static bool HaveQ3
        {
            get { return Player.HasBuff("YasuoQ3W"); }
        }

        public static AIHeroClient QCirTarget
        {
            get
            {
                var pos = Player.GetDashInfo().EndPos.To2D();
                var target = TargetSelector.GetTarget(QCirWidth, DamageType.Physical);
                return target != null && Player.Distance(pos) < 150 ? target : null;
            }
        }

        #endregion

        #region Methods

        private static bool CanCastE(Obj_AI_Base target)
        {
            return !target.HasBuff("YasuoDashWrapper");
        }

        private static bool CanCastR(AIHeroClient target)
        {
            return target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback);
        }

        private static void Clear()
        {
            if (Menu["ClE"].Cast<CheckBox>().CurrentValue && _e.IsReady())
            {
                var minionObj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, _e.Range)
                        .Where(
                            i =>
                                CanCastE(i) &&
                                (!UnderTower(PosAfterE(i)) || Menu["ClETower"].Cast<CheckBox>().CurrentValue))
                        .ToList();

                if (minionObj.Any())
                {
                    var obj = minionObj.FirstOrDefault(i => CanKill(i, GetEDmg(i)));
                    if (obj == null && Menu["ClQ"].Cast<CheckBox>().CurrentValue && _q.IsReady(50)
                        && (!HaveQ3 || Menu["ClQ3"].Cast<CheckBox>().CurrentValue))
                        obj = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.ServerPosition, _e.Range)
                            .Where(i => i.IsValidTarget(QCirWidth))
                            .FirstOrDefault();

                    if (obj != null && _e.Cast(obj))
                    {
                        return;
                    }
                }
            }
            if (Menu["ClQ"].Cast<CheckBox>().CurrentValue && _q.IsReady() &&
                (!HaveQ3 || Menu["ClQ3"].Cast<CheckBox>().CurrentValue))
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
                        var qMinHit = _q.MinimumHitChance;
                        _q.MinimumHitChance = HitChance.Medium;
                        var pos = (!HaveQ3 ? _q : _q2).GetPrediction((Obj_AI_Base) minionObj);
                        _q.MinimumHitChance = qMinHit;
                        if (pos.CollisionObjects.Count() > 0 && (!HaveQ3 ? _q : _q2).Cast(pos.CastPosition))
                        {
                            return;
                        }
                    }
                }
            }
            if (Menu["ClItem"].Cast<CheckBox>().CurrentValue && (Hydra.IsReady() || Tiamat.IsReady()))
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


        private static void Fight()
        {
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                if (Menu["R"].Cast<CheckBox>().CurrentValue && _r.IsReady())
                {
                    var obj =
                        (from enemy in EntityManager.Heroes.Enemies.Where(i => _r.IsInRange(i) && CanCastR(i))
                            let sub = enemy.CountEnemiesInRange(RWidth)
                            where
                                (sub > 1 && Player.GetSpellDamage(enemy, SpellSlot.R) > enemy.Health)
                            orderby sub descending
                            select enemy).ToList();
                    if (obj.Any())
                    {
                        var target = TargetSelector.GetTarget(_r.Range, DamageType.Physical);
                        if (target.IsValidTarget(_r.Range))
                        {
                            _r.Cast();
                        }
                    }
                }
                if (Menu["E"].Cast<CheckBox>().CurrentValue && _e.IsReady())
                {
                    if (Menu["EDmg"].Cast<CheckBox>().CurrentValue && Menu["Q"].Cast<CheckBox>().CurrentValue &&
                        HaveQ3 && _q.IsReady(50))
                    {
                        var target = TargetSelector.GetTarget(QRange, DamageType.Physical);
                        if (target != null)
                        {
                            var obj = GetNearObj(target, true);
                            if (obj != null && _e.Cast(obj))
                            {
                                return;
                            }
                        }
                    }
                    if (Menu["EGap"].Cast<CheckBox>().CurrentValue)
                    {
                        var target = TargetSelector.GetTarget(QRange, DamageType.Physical)
                                     ?? TargetSelector.GetTarget(Q2Range, DamageType.Physical);
                        if (target != null)
                        {
                            var obj = GetNearObj(target);
                            if (obj != null
                                && (obj.NetworkId != target.NetworkId
                                    ? Player.Distance(target) > Menu["EGapRange"].Cast<Slider>().CurrentValue
                                    : !target.IsInAutoAttackRange(Player))
                                && (!UnderTower(PosAfterE(obj)) || Menu["EGapTower"].Cast<CheckBox>().CurrentValue)
                                && _e.Cast(obj))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && Menu["Q"].Cast<CheckBox>().CurrentValue && _q.IsReady())
            {
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo
                    || ((!HaveQ3 || Menu["Q3"].Cast<CheckBox>().CurrentValue)
                        && (!UnderTower(Player.ServerPosition) || Menu["QTower"].Cast<CheckBox>().CurrentValue)))
                {
                    if (Player.IsDashing())
                    {
                        if (QCirTarget != null && CastQCir(QCirTarget))
                        {
                            return;
                        }
                        if (!HaveQ3 && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo &&
                            Menu["QStack"].Cast<CheckBox>().CurrentValue &&
                            Menu["E"].Cast<CheckBox>().CurrentValue
                            && Menu["EGap"].Cast<CheckBox>().CurrentValue &&
                            TargetSelector.GetTarget(200, DamageType.Physical) == null)
                        {
                            var minionObj =
                                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                                    EntityManager.UnitTeam.Enemy, Player.GetDashInfo().EndPos, QCirWidth);
                            if (minionObj.Any() && Player.Distance(Player.GetDashInfo().EndPos) < 150
                                && CastQCir((Obj_AI_Base) (minionObj.Where(i => i.Distance(Player) < _q.Range))))
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
                                    var pred = _q2.GetPrediction(hero);
                                    if (pred.HitChancePercent >= 85  && pred.CollisionObjects.Count() > hit)
                                    {
                                        hit = pred.CollisionObjects.Count();
                                        predPos = pred.CastPosition;
                                    }
                                }
                                if (predPos.IsValid())
                                {
                                    if (_q2.Cast(predPos))
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
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && _q.IsReady())
                {
                    var qTarget = TargetSelector.GetTarget(QRange, DamageType.Physical);
                    if (qTarget.IsValidTarget(QRange))
                    {
                        CastQCir(qTarget);
                    }
                }

                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass && Menu["HarQLastHit"].Cast<CheckBox>().CurrentValue &&
                    TargetSelector.GetTarget(100, DamageType.Physical) == null && !HaveQ3
                    && !Player.IsDashing())
                {
                    var obj =
                        EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                            EntityManager.UnitTeam.Enemy, Player.Position, _q.Range)
                            .FirstOrDefault(i => CanKill(i, GetQDmg(i)));
                    if (obj != null)
                    {
                        CastQCir(obj);
                    }
                }
            }
        }

        private static void Flee()
        {
            if (!Menu["FlE"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (Menu["FlEStackQ"].Cast<CheckBox>().CurrentValue && _q.IsReady() && !HaveQ3 && Player.IsDashing())
            {
                if (QCirTarget != null && CastQCir(QCirTarget))
                {
                    return;
                }
                var minionObj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.GetDashInfo().EndPos, QCirWidth);
                if (minionObj.Any() && Player.Distance(Player.GetDashInfo().EndPos) < 150
                    && CastQCir(minionObj.FirstOrDefault(i => i.Distance(Player) < _q.Range)))
                {
                    return;
                }
            }
            var obj = GetNearObj();
            if (obj == null || !_e.IsReady())
            {
                return;
            }
            _e.Cast(obj);
        }

        private static double GetEDmg(Obj_AI_Base target)
        {
            return Player.CalculateDamageOnUnit(
                target,
                DamageType.Magical,
                (float) ((50 + 20*_e.Level)*(1 + Math.Max(0, Player.GetBuffCount("YasuoDashScalar")*0.25))
                         + 0.6*Player.FlatMagicDamageMod));
        }

        private static Obj_AI_Base GetNearObj(Obj_AI_Base target = null, bool inQCir = false)
        {
            var pos = target != null
                ? Prediction.Position.PredictUnitPosition(target, (int) (_e.CastDelay + _e.Handle.SData.MissileSpeed))
                    .To3D()
                : Game.CursorPos;
            var obj = new List<Obj_AI_Base>();
            obj.AddRange(EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                EntityManager.UnitTeam.Enemy, Player.Position, _e.Range));
            obj.AddRange(EntityManager.Heroes.Enemies.Where(i => i.IsValidTarget(_e.Range)));
            return
                obj.Where(
                    i =>
                        CanCastE(i) && pos.Distance(PosAfterE(i)) < (inQCir ? QCirWidthMin : Player.Distance(pos))
                    )
                    .FirstOrDefault(i => pos.Distance(PosAfterE(i)) < _e.Range);
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
            return Player.CalculateDamageOnUnit(target, DamageType.Physical, (int) (20*_q.Level + (dmg - reduction)*k))
                   + (Player.GetBuffCount("ItemStatikShankCharge") == 100
                       ? Player.CalculateDamageOnUnit(
                           target,
                           DamageType.Magical,
                           (int) (100*(Player.Crit >= 0.85f ? (Item.HasItem(3031) ? 2.25 : 1.8) : 1)))
                       : 0);
        }

        private static void LastHit()
        {
            if (Menu["LhQ"].Cast<CheckBox>().CurrentValue && _q.IsReady() && !Player.IsDashing()
                && (!HaveQ3 || Menu["LhQ3"].Cast<CheckBox>().CurrentValue))
            {
                var obj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, !HaveQ3 ? QRange : Q2Range)
                        .FirstOrDefault(i => CanKill(i, GetQDmg(i)));
                if (obj != null && (!HaveQ3) && obj.HasBuff("YasuoQ"))
                {
                    return;
                }
            }
            if (Menu["LhE"].Cast<CheckBox>().CurrentValue && _e.IsReady())
            {
                var obj =
                    EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                        EntityManager.UnitTeam.Enemy, Player.Position, _e.Range).Where(i =>
                            CanCastE(i)
                            && (!i.IsInAutoAttackRange(Player) || i.Health > Player.GetAutoAttackDamage(i, true))
                            && (!UnderTower(PosAfterE(i)) || Menu["LhETower"].Cast<CheckBox>().CurrentValue))
                        .FirstOrDefault(i => i.IsValidTarget(_e.Range));

                if (obj != null)
                {
                    _e.Cast(obj);
                }
            }
        }

        private static Vector3 PosAfterE(Obj_AI_Base target)
        {
            return Player.ServerPosition.Extend(
                target.ServerPosition,
                Player.Distance(target) < 410 ? _e.Range : Player.Distance(target) + 65).To3D();
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

        public string ChampionName = "Yasuo";


        public string[] SpellNames = {};

        #endregion
    }
}