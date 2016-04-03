using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using SharpDX;
using Color = System.Drawing.Color;

namespace SephKhazix
{
    class Khazix : Helper
    {

        static void Main()
        {
            Khazix K6 = new Khazix();
        }

        public Khazix()
        {
            Loading.OnLoadingComplete += OnLoad;
        }

        void OnLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Khazix")
            {
                return;
            }
            Chat.Print("<font color='#1d87f2'>SephKhazix Loaded </font>");
            Chat.Print("<font color='#1d87f2'>Ported by Rexy </font>");
            Init();
            GenerateMenu();
            Game.OnUpdate += OnUpdate;
            Game.OnUpdate += DoubleJump;
            Drawing.OnDraw += OnDraw;
            Spellbook.OnCastSpell += SpellCast;
            Orbwalker.OnPreAttack += BeforeAttack;
        }

        void Init()
        {
            InitSkills();
            Khazix = ObjectManager.Player;

            foreach (var t in ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy))
            {
                EnemyTurretPositions.Add(t.ServerPosition);
            }

            var shop = ObjectManager.Get<Obj_Shop>().FirstOrDefault(o => o.IsAlly);
            if (shop != null)
            {
                NexusPosition = shop.Position;
            }

            HeroList = EntityManager.Heroes.AllHeroes;
        }


        void OnUpdate(EventArgs args)
        {
            if (Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }

            EvolutionCheck();

            if (Config.GetBool("Kson"))
            {
                KillSteal();
            }

            if (Config.GetKeyBind("Harass.Key"))
            {
                Harass();
            }
            switch (Orbwalker.ActiveModesFlags)
            {
                    case Orbwalker.ActiveModes.Combo:
                {
                    Combo();
                    break;
                }
                    case Orbwalker.ActiveModes.Harass:
                {
                    Harass();
                    break;
                }
                    case Orbwalker.ActiveModes.LaneClear:
                {
                    Waveclear();
                    break;
                }
                    case Orbwalker.ActiveModes.LastHit:
                {
                    Lh();
                    break;
                }
            }
        }


        void Harass()
        {
            if (Config.GetBool("UseQHarass") && Q.IsReady())
            {
                var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (enemy.IsValidEnemy())
                {
                    Q.Cast(enemy);
                }
            }
            if (Config.GetBool("UseWHarass") && W.IsReady())
            {
                AIHeroClient target = TargetSelector.GetTarget(950, DamageType.Physical);
                var autoWi = Config.GetBool("Harass.AutoWI");
                var autoWd = Config.GetBool("Harass.AutoWD");
                var hitchance = HitChance.Medium;
                if (target != null && W.IsReady())
                {
                    if (!EvolvedW && Khazix.Distance(target) <= W.Range)
                    {
                        var predw = W.GetPrediction(target);
                        if (predw.HitChance == hitchance)
                        {
                            W.Cast(predw.CastPosition);
                        }
                    }
                    else if (EvolvedW && target.IsValidTarget(W.Range + 200))
                    {
                            var pred = W.GetPrediction(target);
                            if ((pred.HitChance == HitChance.Immobile && autoWi) || (pred.HitChance == HitChance.Dashing && autoWd) || pred.HitChance >= hitchance)
                            {
                                CastWe(target, pred.UnitPosition.To2D());
                        }
                    }
                }
            }
        }


        void Lh()
        {
            var allMinions =
                EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.IsEnemy && m.IsValidTarget(Q.Range));
            if (Config.GetBool("UseQFarm") && Q.IsReady())
            {
                foreach (var minn in allMinions)
                {
                    {
                        if (Vector3.Distance(minn.ServerPosition, Khazix.ServerPosition) >
                            Khazix.GetAutoAttackRange() && Khazix.Distance(minn) <= Q.Range && Khazix.GetSpellDamage(minn,SpellSlot.Q) >= minn.Health)
                        {
                            Q.Cast(minn);
                            return;
                        }
                    }
                }
            }
            if (Config.GetBool("UseWFarm") && W.IsReady())
            {
                var wMinions =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,Khazix.ServerPosition,W.Range).FirstOrDefault();

                    var wPred = W.GetPrediction(wMinions);
                    if (wPred.HitChancePercent >= 75)
                    {
                        if (!EvolvedW)
                        {
                            if (Khazix.Distance(wMinions) <= W.Range)
                            {
                                W.Cast(wMinions);
                            }
                        }

                        if (EvolvedW)
                        {
                            if (Khazix.Distance(wMinions) <= W.Range)
                            {
                                W.Cast(wMinions);
                            }
                        }
                    }
            }

            if (Config.GetBool("UseEFarm") && E.IsReady())
            {
                var minyan =
                    EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Khazix.ServerPosition,
                        E.Range).FirstOrDefault();
                    if (Khazix.Distance(minyan) <= E.Range)
                        E.Cast(minyan);
            }


            if (Config.GetBool("UseItemsFarm"))
            {
                var minyonlar =
                    EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Khazix.ServerPosition,
                        Hydra.Range).FirstOrDefault();

                if (Hydra.IsReady() && Khazix.Distance(minyonlar) <= Hydra.Range)
                {
                    Item.UseItem(3074);
                }
                if (Tiamat.IsReady() && Khazix.Distance(minyonlar) <= Tiamat.Range)
                {
                    Item.UseItem(3077);
                }
            }
        }

        void Waveclear()
        {

            if (Config.GetBool("UseQFarm") && Q.IsReady())
            {
                var minion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Khazix.ServerPosition, Q.Range).FirstOrDefault();
                if (minion != null && minion.IsValidTarget(Q.Range)) {
                    Q.Cast(minion);
                }
                else if (minion == null || !minion.IsValid)
                {
                    if (minion.IsValidTarget(Q.Range))
                    {
                            Q.Cast(minion);
                    }
                }
            }

            if (Config.GetBool("UseWFarm") && W.IsReady() && Khazix.HealthPercent <= Config.GetSlider("Farm.WHealth"))
            {
                var minnn = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Khazix.ServerPosition,
                    W.Range).FirstOrDefault();
                var distcheck = EvolvedW ? Khazix.Distance(minnn) <= We.Range : Khazix.Distance(minnn) <= W.Range;
                if (distcheck)
                {
                    W.Cast(minnn);
                }
            }

            if (Config.GetBool("UseEFarm") && E.IsReady())
            {
                var minyon =
                    EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Khazix.ServerPosition,
                        E.Range).FirstOrDefault();
                if (Khazix.Distance(minyon) <= E.Range)
                {
                    E.Cast(minyon);
                }
            }


            if (Config.GetBool("UseItemsFarm"))
            {
                var mins = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,Khazix.ServerPosition,Hydra.Range).FirstOrDefault();

                    if (Hydra.IsReady() && Khazix.Distance(mins) <= Hydra.Range)
                    {
                        Item.UseItem(3074);
                    }
                    if (Tiamat.IsReady() && Khazix.Distance(mins) <= Tiamat.Range)
                    {
                        Item.UseItem(3077);
                    }
                    if (Titanic.IsReady() && Khazix.Distance(mins) <= Titanic.Range)
                    {
                        Item.UseItem(3748);
                    }
                }

        if (Config.GetBool("UseQFarm") && Q.IsReady())
            {
                var minion = EntityManager.MinionsAndMonsters.GetJungleMonsters(Khazix.ServerPosition, Q.Range).FirstOrDefault();
                if (minion != null && minion.IsValidTarget(Q.Range)) {
                    Q.Cast(minion);
                }
                else if (minion == null || !minion.IsValid)
                {
                    if (minion.IsValidTarget(Q.Range))
                    {
                            Q.Cast(minion);
                    }
                }
            }

            if (Config.GetBool("UseWFarm") && W.IsReady() && Khazix.HealthPercent <= Config.GetSlider("Farm.WHealth"))
            {
                var minnn = EntityManager.MinionsAndMonsters.GetJungleMonsters(Khazix.ServerPosition,
                    W.Range).FirstOrDefault();
                var distcheck = EvolvedW ? Khazix.Distance(minnn) <= We.Range : Khazix.Distance(minnn) <= W.Range;
                if (distcheck)
                {
                    W.Cast(minnn);
                }
            }

            if (Config.GetBool("UseEFarm") && E.IsReady())
            {
                var minyon =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(Khazix.ServerPosition,
                        E.Range).FirstOrDefault();
                if (Khazix.Distance(minyon) <= E.Range)
                {
                    E.Cast(minyon);
                }
            }


            if (Config.GetBool("UseItemsFarm"))
            {
                var mins = EntityManager.MinionsAndMonsters.GetJungleMonsters(Khazix.ServerPosition,Hydra.Range).FirstOrDefault();

                    if (Hydra.IsReady() && Khazix.Distance(mins) <= Hydra.Range)
                    {
                        Item.UseItem(3074);
                    }
                    if (Tiamat.IsReady() && Khazix.Distance(mins) <= Tiamat.Range)
                    {
                        Item.UseItem(3077);
                    }
                    if (Titanic.IsReady() && Khazix.Distance(mins) <= Titanic.Range)
                    {
                        Item.UseItem(3748);
                    }
                }
        }


        void Combo()
        {
            var target = EntityManager.Heroes.Enemies.Find(x => x.IsValidEnemy(W.Range) && !x.IsZombie);

            if ((target != null))
            {
                var dist = Khazix.Distance(target);

                // Normal abilities
                if (Q.IsReady() && dist <= Q.Range && Config.GetBool("UseQCombo"))
                {
                    Q.Cast(target);
                }

                if (W.IsReady() && !EvolvedW && dist <= W.Range && Config.GetBool("UseWCombo"))
                {
                    var pred = W.GetPrediction(target);
                    if (pred.HitChancePercent >= 75)
                    {
                        W.Cast(pred.CastPosition);
                    }
                }

                if (E.IsReady() && dist <= E.Range && Config.GetBool("UseECombo") && dist > Q.Range + (0.7 * Khazix.MoveSpeed))
                {
                    var pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                }

                // Use EQ AND EW Synergy
                if ((dist <= E.Range + Q.Range + (0.7 * Khazix.MoveSpeed) && dist > Q.Range && E.IsReady() &&
                    Config.GetBool("UseEGapclose")) || (dist <= E.Range + W.Range && dist > Q.Range && E.IsReady() && W.IsReady() &&
                    Config.GetBool("UseEGapcloseW")))
                {
                    var pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                    if (Config.GetBool("UseRGapcloseW") && R.IsReady())
                    {
                        R.Cast();
                    }
                }


                // Ult Usage
                if (R.IsReady() && !Q.IsReady() && !W.IsReady() && !E.IsReady() &&
                    Config.GetBool("UseRCombo"))
                {
                    R.Cast();
                }
                // Evolved

                if (W.IsReady() && EvolvedW && dist <= We.Range && Config.GetBool("UseWCombo"))
                {
                    var pred = We.GetPrediction(target);
                    if (pred.HitChancePercent >= 75)
                    {
                        CastWe(target, pred.UnitPosition.To2D());
                    }
                    if (pred.HitChance >= HitChance.Collision)
                    {
                        Obj_AI_Base[] pCollision = pred.CollisionObjects;
                        var x = pCollision.FirstOrDefault(predCollisionChar => predCollisionChar.Distance(target) <= 30);
                        if (x != null)
                        {
                            W.Cast(x.Position);
                        }
                    }
                }

                if (dist <= E.Range + (0.7 * Khazix.MoveSpeed) && dist > Q.Range &&
                    Config.GetBool("UseECombo") && E.IsReady())
                {
                    var pred = E.GetPrediction(target);
                    if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                    {
                        E.Cast(pred.CastPosition);
                    }
                }

                if (Config.GetBool("UseItems"))
                {
                    UseItems(target);
                }
            }
        }


         void KillSteal()
        {
                var target = EntityManager.Heroes.Enemies
                    .Find(x => x.IsValidTarget() && x.Distance(Khazix.Position) < 1375f && !x.IsZombie);

                if (target != null && target.IsInRange(Ignite.Range))
                {
                    if (Config.GetBool("UseIgnite") &&
                        Ignite.State == SpellState.Ready)
                    {
                        double igniteDmg = Khazix.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite);
                        if (igniteDmg > target.Health)
                        {
                            Ignite.Cast(target);
                            return;
                        }
                    }

                    if (Config.GetBool("Safety.autoescape") && !IsHealthy)
                    {
                        var ally =
                            HeroList.FirstOrDefault(h => h.HealthPercent > 40 && h.CountEnemiesInRange(400) == 0 && !h.ServerPosition.PointUnderEnemyTurret());
                        if (ally != null && ally.IsValid)
                        {
                            E.Cast(ally.ServerPosition);
                            return;
                        }
                        var objAiturret = EnemyTurretPositions.Where(x => Vector3.Distance(Khazix.ServerPosition, x) <= 900f);
                        if (objAiturret.Any() || Khazix.CountEnemiesInRange(500) >= 1)
                        {
                            var bestposition = Khazix.ServerPosition.Extend(NexusPosition, E.Range);
                            E.Cast(bestposition.To3D());
                            return;
                        }
                    }

                    if (Config.GetBool("UseQKs") && Q.IsReady() &&
                        Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= Q.Range)
                    {
                        double qDmg = GetQDamage(target);
                        if (!Jumping && target.Health <= qDmg)
                        {
                            Q.Cast(target);
                            return;
                        }
                    }

                    if (Config.GetBool("UseEKs") && E.IsReady() &&
                        Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= E.Range && Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) > Q.Range)
                    {
                        double eDmg = Khazix.GetSpellDamage(target, SpellSlot.E);
                        if (!Jumping && target.Health < eDmg)
                        {
                            Core.DelayAction(delegate
                                {
                                    var pred = E.GetPrediction(target);
                                    if (target.IsValid && !target.IsDead)
                                    {
                                        if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                        {
                                            E.Cast(pred.CastPosition);
                                        }
                                    }
                                }, Game.Ping + Config.GetSlider("EDelay"));
                        }
                    }

                    if (W.IsReady() && !EvolvedW && Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range &&
                        Config.GetBool("UseWKs"))
                    {
                        double wDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                        if (target.Health <= wDmg)
                        {
                            var pred = W.GetPrediction(target);
                            if (pred.HitChancePercent >= 70)
                            {
                                W.Cast(pred.CastPosition);
                                return;
                            }
                        }
                    }

                    if (W.IsReady() && EvolvedW &&
                            Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range &&
                            Config.GetBool("UseWKs"))
                    {
                        double wDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                        var pred = W.GetPrediction(target);
                        if (target.Health <= wDmg && pred.HitChancePercent >= 70)
                        {
                            CastWe(target, pred.UnitPosition.To2D());
                            return;
                        }

                        if (pred.HitChance >= HitChance.Collision)
                        {
                            var pCollision = pred.CollisionObjects;
                            var x =
                                pCollision
                                    .FirstOrDefault(predCollisionChar => Vector3.Distance(predCollisionChar.ServerPosition, target.ServerPosition) <= 30);
                            if (x != null)
                            {
                                W.Cast(x.Position);
                                return;
                            }
                        }
                    }


                    // Mixed's EQ KS
                    if (Q.IsReady() && E.IsReady() &&
                        Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= E.Range + Q.Range
                        && Config.GetBool("UseEQKs"))
                    {
                        double qDmg = GetQDamage(target);
                        double eDmg = Khazix.GetSpellDamage(target, SpellSlot.E);
                        if ((target.Health <= qDmg + eDmg))
                        {
                            Core.DelayAction(delegate
                            {
                                var pred = E.GetPrediction(target);
                                if (target.IsValidTarget() && !target.IsZombie && ShouldJump(pred.CastPosition))
                                {
                                    if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                    {
                                        E.Cast(pred.CastPosition);
                                    }
                                }
                            },Config.GetSlider("EDelay"));
                        }
                    }

                    // MIXED EW KS
                    if (W.IsReady() && E.IsReady() && !EvolvedW &&
                        Vector3.Distance(Khazix.ServerPosition, target.ServerPosition) <= W.Range + E.Range
                        && Config.GetBool("UseEWKs"))
                    {
                        double wDmg = Khazix.GetSpellDamage(target, SpellSlot.W);
                        if (target.Health <= wDmg)
                        {
                            Core.DelayAction(delegate
                            {
                                var pred = E.GetPrediction(target);
                                if (target.IsValid && !target.IsDead && ShouldJump(pred.CastPosition))
                                {
                                    if (Config.GetBool("Ksbypass") || ShouldJump(pred.CastPosition))
                                    {
                                        E.Cast(pred.CastPosition);
                                    }
                                }
                            }, Config.GetSlider("EDelay"));
                        }
                    }

                    if (Tiamat.IsReady() &&
                        Vector2.Distance(Khazix.ServerPosition.To2D(), target.ServerPosition.To2D()) <= Tiamat.Range &&
                        Config.GetBool("UseTiamatKs"))
                    {
                        double tiamatdmg = Khazix.GetItemDamage(target,ItemId.Tiamat_Melee_Only);
                        if (target.Health <= tiamatdmg)
                        {
                            Tiamat.Cast();
                            return;
                        }
                    }
                    if (Hydra.IsReady() &&
                        Vector2.Distance(Khazix.ServerPosition.To2D(), target.ServerPosition.To2D()) <= Hydra.Range &&
                        Config.GetBool("UseTiamatKs"))
                    {
                        double hydradmg = Khazix.GetItemDamage(target, ItemId.Ravenous_Hydra_Melee_Only);
                        if (target.Health <= hydradmg)
                        {
                            Hydra.Cast();
                        }
                    }
                }
            }

        internal bool ShouldJump(Vector3 position)
        {
            if (!Config.GetBool("Safety.Enabled") || Override)
            {
                return true;
            }
            if (Config.GetBool("Safety.TowerJump") && position.PointUnderEnemyTurret())
            {
                return false;
            }
            else if (Config.GetBool("Safety.Enabled"))
            {
                if (Khazix.HealthPercent < Config.GetSlider("Safety.MinHealth"))
                {
                    return false;
                }

                if (Config.GetBool("Safety.CountCheck"))
                {
                    var enemies = position.CountEnemiesInRange(400);
                    var allies = position.CountAlliesInRange(400);

                    var ec = enemies;
                    var ac = allies;
                    float setratio = Config.GetSlider("Safety.Ratio") / 5;


                    if (ec != 0 && !((float)ac / ec >= setratio))
                    {
                        return false;
                    }
                }
                return true;
            }
            return true;
        }



        internal void CastWe(Obj_AI_Base unit, Vector2 unitPosition, int minTargets = 0)
        {
            var points = new List<Vector2>();
            var hitBoxes = new List<int>();

            Vector2 startPoint = Khazix.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (unitPosition - startPoint).Normalized();

            foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies)
            {
                if (enemy.IsValidTarget() && enemy.NetworkId != unit.NetworkId)
                {
                    var pos = W.GetPrediction(enemy);
                    if (pos.HitChancePercent >= 75)
                    {
                        points.Add(pos.UnitPosition.To2D());
                        hitBoxes.Add((int)enemy.BoundingRadius + 275);
                    }
                }
            }

            var posiblePositions = new List<Vector2>();

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    posiblePositions.Add(unitPosition + originalDirection.Rotated(0));
                if (i == 1)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(Wangle));
                if (i == 2)
                    posiblePositions.Add(startPoint + originalDirection.Rotated(-Wangle));
            }


            if (startPoint.Distance(unitPosition) < 900)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = posiblePositions[i];
                    Vector2 direction = (pos - startPoint).Normalized().Perpendicular();
                    float k = ((float)2 / 3 * (unit.BoundingRadius + W.Width));
                    posiblePositions.Add(startPoint - k * direction);
                    posiblePositions.Add(startPoint + k * direction);
                }
            }

            var bestPosition = new Vector2();
            int bestHit = -1;

            foreach (Vector2 position in posiblePositions)
            {
                int hits = CountHits(position, points, hitBoxes);
                if (hits > bestHit)
                {
                    bestPosition = position;
                    bestHit = hits;
                }
            }

            if (bestHit <= minTargets)
                return;

            W.Cast(bestPosition.To3D());
        }

        int CountHits(Vector2 position, List<Vector2> points, List<int> hitBoxes)
        {
            int result = 0;

            Vector2 startPoint = Khazix.ServerPosition.To2D();
            Vector2 originalDirection = W.Range * (position - startPoint).Normalized();
            Vector2 originalEndPoint = startPoint + originalDirection;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                for (int k = 0; k < 3; k++)
                {
                    var endPoint = new Vector2();
                    if (k == 0)
                        endPoint = originalEndPoint;
                    if (k == 1)
                        endPoint = startPoint + originalDirection.Rotated(Wangle);
                    if (k == 2)
                        endPoint = startPoint + originalDirection.Rotated(-Wangle);

                    if (point.Distance(startPoint, endPoint, true, true) <
                        (W.Width + hitBoxes[i]) * (W.Width + hitBoxes[i]))
                    {
                        result++;
                        break;
                    }
                }
            }
            return result;
        }


        void DoubleJump(EventArgs args)
        {
            if (!E.IsReady() || !EvolvedE || !Config.GetBool("djumpenabled") || Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }

            var targets = HeroList.Where(x => x.IsValidTarget() && !x.IsInvulnerable && !x.IsZombie);

            if (Q.IsReady() && E.IsReady())
            {
                var checkQKillable = targets.FirstOrDefault(x => Vector3.Distance(Khazix.ServerPosition, x.ServerPosition) < Q.Range - 25 && GetQDamage(x) > x.Health);

                if (checkQKillable != null)
                {
                    Jumping = true;
                    Jumppoint1 = GetJumpPoint(checkQKillable);
                    E.Cast(Jumppoint1);
                    Q.Cast(checkQKillable);
                    Core.DelayAction(() =>
                    {
                        if (E.IsReady())
                        {
                            Jumppoint2 = GetJumpPoint(checkQKillable, false);
                            E.Cast(Jumppoint2);
                        }
                        Jumping = false;
                    }, Config.GetSlider("JEDelay") + Game.Ping);
                }
            }
        }


        Vector3 GetJumpPoint(AIHeroClient qtarget, bool firstjump = true)
        {
            if (Khazix.ServerPosition.PointUnderEnemyTurret())
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range).To3D();
            }

            if (Config.GetSlider("jumpmode") == 0)
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range).To3D();
            }

            if (firstjump && Config.GetBool("jcursor"))
            {
                return Game.CursorPos;
            }

            if (!firstjump && Config.GetBool("jcursor2"))
            {
                return Game.CursorPos;
            }

            Vector3 position = new Vector3();
            var jumptarget = IsHealthy
                  ? HeroList
                      .FirstOrDefault(x => x.IsValidTarget() && !x.IsZombie && x != qtarget &&
                              Vector3.Distance(Khazix.ServerPosition, x.ServerPosition) < E.Range)
                  :
              HeroList
                  .FirstOrDefault(x => x.IsAlly && !x.IsZombie && !x.IsDead && !x.IsMe &&
                          Vector3.Distance(Khazix.ServerPosition, x.ServerPosition) < E.Range);

            if (jumptarget != null)
            {
                position = jumptarget.ServerPosition;
            }
            if (jumptarget == null)
            {
                return Khazix.ServerPosition.Extend(NexusPosition, E.Range).To3D();
            }
            return position;
        }

        void SpellCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!EvolvedE || !Config.GetBool("save"))
            {
                return;
            }

            if (args.Slot.Equals(SpellSlot.Q) && args.Target is AIHeroClient && Config.GetBool("djumpenabled"))
            {
                var target = (AIHeroClient)args.Target;
                var qdmg = GetQDamage(target);
                var dmg = (Khazix.GetAutoAttackDamage(target) * 2) + qdmg;
                if (target.Health < dmg && target.Health > qdmg)
                { //save some unnecessary q's if target is killable with 2 autos + Q instead of Q as Q is important for double jumping
                    args.Process = false;
                }
            }
        }

        void BeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (args.Target.Type == GameObjectType.AIHeroClient)
            {
                if (Config.GetBool("Safety.noaainult") && IsInvisible)
                {
                    args.Process = false;
                    return;
                }
                if (Config.GetBool("djumpenabled") && Config.GetBool("noauto"))
                {
                    if (args.Target.Health < GetQDamage((AIHeroClient)args.Target) &&
                        Khazix.ManaPercent > 15)
                    {
                        args.Process = false;
                    }
                }
            }
        }

        void OnDraw(EventArgs args)
        {
            if (Config.GetBool("Drawings.Disable") || Khazix.IsDead || Khazix.IsRecalling())
            {
                return;
            }
            if (Config.GetBool("Debugon"))
            {
                var isolatedtargs = GetIsolatedTargets();
                foreach (var x in isolatedtargs)
                {
                    var heroposwts = Drawing.WorldToScreen(x.Position);
                    Drawing.DrawText(heroposwts.X, heroposwts.Y, Color.White, "Isolated");
                }
            }

            if (Config.GetBool("jumpdrawings") && Jumping)
            {
                var playerPosition = Drawing.WorldToScreen(Khazix.Position);
                var jump1 = Drawing.WorldToScreen(Jumppoint1).To3D();
                var jump2 = Drawing.WorldToScreen(Jumppoint2).To3D();
                Drawing.DrawCircle(jump1,250,Color.White);
                Drawing.DrawCircle(jump2, 250, Color.White);
                Drawing.DrawLine(playerPosition.X, playerPosition.Y, jump1.X, jump1.Y, 10, Color.DarkCyan);
                Drawing.DrawLine(jump1.X, jump1.Y, jump2.X, jump2.Y, 10, Color.DarkCyan);
            }

            var drawq = Config.GetBool("DrawQ");
            var draww = Config.GetBool("DrawW");
            var drawe = Config.GetBool("DrawE");

            if (drawq)
            {
                Drawing.DrawCircle(Khazix.Position,Q.Range,Color.White);
            }
            if (draww)
            {
                Drawing.DrawCircle(Khazix.Position, W.Range, Color.White);
            }

            if (drawe)
            {
                Drawing.DrawCircle(Khazix.Position, E.Range, Color.White);
            }

        }
    }
}


