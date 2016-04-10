using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Dash = EloBuddy.SDK.Events.Dash;
using Gapcloser = EloBuddy.SDK.Events.Gapcloser;
using HitChance = EloBuddy.SDK.Enumerations.HitChance;
using Interrupter = EloBuddy.SDK.Events.Interrupter;
using TargetSelector = EloBuddy.SDK.TargetSelector;

namespace YasuoPro
{
    internal class Yasuo : Helper
    {
        public AIHeroClient CurrentTarget = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
        public bool Fleeing;

        public Yasuo()
        {
            Loading.OnLoadingComplete += OnLoad;
        }

        private void OnLoad(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Yasuo")
            {
                return;
            }

            Chat.Print("<font color='#1d87f2'>YasuoPro by Seph Loaded. Good Luck!</font>");
            Chat.Print("<font color='#1d87f2'>YasuoPro Ported by Rexy</font>");
            InitItems();
            InitSpells();
            YasuoMenu.Init();
            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.Initialize();
            }
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnGapClose;
            Interrupter.OnInterruptableSpell += OnInterruptable;
            Obj_AI_Base.OnProcessSpellCast += TargettedDanger.SpellCast;
        }

        private void OnUpdate(EventArgs args)
        {
            if (Yasuo.IsDead || EloBuddy.SDK.Extensions.IsRecalling(Yasuo))
            {
                return;
            }

            CastUlt();

            if (GetBool("Evade.WTS"))
            {
                TargettedDanger.OnUpdate();
            }

            if (GetBool("Misc.AutoStackQ") && !TornadoReady && !CurrentTarget.IsValidEnemy(Q.Range))
            {
                var closest =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                        x => EloBuddy.SDK.Extensions.IsValidTarget(x, Q.Range))
                        .OrderByDescending(x => x.Distance(Yasuo))
                        .LastOrDefault(x => x != null);

                if (closest == null)
                {
                    return;
                }

                var pred = Q.GetPrediction(closest);
                if (pred.HitChance >= HitChance.Low)
                {
                    Q.Cast(closest.ServerPosition);
                }
            }

            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.OnUpdate();
            }

            Fleeing = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee;

            if (GetBool("Killsteal.Enabled") && !Fleeing)
            {
                Killsteal();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Waveclear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LhSkills();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
        }

        private void CastUlt()
        {
            if (!R.IsReady())
            {
                return;
            }
            if (GetBool("Combo.UseR") && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
            {
                CastR(GetSliderInt("Combo.RMinHit"));
            }

            if (GetBool("Misc.AutoR") && !Fleeing)
            {
                CastR(GetSliderInt("Misc.RMinHit"));
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (Debug)
            {
                Drawing.DrawCircle(EloBuddy.SDK.Extensions.To3D(DashPosition), Yasuo.BoundingRadius, Color.Chartreuse);
            }


            if (Yasuo.IsDead || GetBool("Drawing.Disable"))
            {
                return;
            }

            TargettedDanger.OnDraw(args);

            if (GetBool("Misc.Walljump") && Game.MapId == GameMapId.SummonersRift)
            {
                WallJump.OnDraw();
            }

            var pos = Yasuo.Position.WTS();

            Drawing.DrawText(pos.X - 25, pos.Y - 25, isHealthy ? Color.Green : Color.Red, "Healthy: " + isHealthy);

            var drawq = GetBool("Drawing.DrawQ");
            var drawe = GetBool("Drawing.DrawE");
            var drawr = GetBool("Drawing.DrawR");

            if (drawq)
            {
                Drawing.DrawCircle(Yasuo.Position, Qrange, Color.White);
            }
            if (drawe)
            {
                Drawing.DrawCircle(Yasuo.Position, E.Range, Color.White);
            }
            if (drawr)
            {
                Drawing.DrawCircle(Yasuo.Position, R.Range, Color.White);
            }
        }

        private void Combo()
        {
            CurrentTarget = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            CastQ(CurrentTarget);

            if (GetBool("Combo.UseE"))
            {
                CastE(CurrentTarget);
            }

            if (GetBool("Combo.UseIgnite"))
            {
                CastIgnite();
            }

            if (GetBool("Items.Enabled"))
            {
                if (GetBool("Items.UseTIA"))
                {
                    Tiamat.Cast(null);
                }
                if (GetBool("Items.UseHDR"))
                {
                    Hydra.Cast(null);
                }
                if (GetBool("Items.UseBRK") && CurrentTarget != null)
                {
                    Blade.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseBLG") && CurrentTarget != null)
                {
                    Bilgewater.Cast(CurrentTarget);
                }
                if (GetBool("Items.UseYMU"))
                {
                    Youmu.Cast(null);
                }
            }
        }

        private void CastQ(AIHeroClient target)
        {
            if (Q.IsReady() && target.IsValidEnemy(Qrange))
            {
                UseQ(target, GetHitChance("Hitchance.Q"), GetBool("Combo.UseQ"), GetBool("Combo.UseQ2"));
                return;
            }

            if (GetBool("Combo.StackQ") && !target.IsValidEnemy(Qrange) && !TornadoReady)
            {
                var bestmin =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsValidMinion(Qrange))
                        .OrderByDescending(x => x.Distance(Yasuo))
                        .LastOrDefault(x => x != null);

                if (bestmin != null)
                {
                    var pred = Q.GetPrediction(bestmin);

                    if (pred.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(bestmin.ServerPosition);
                    }
                }
            }
        }

        private void CastE(AIHeroClient target)
        {
            if (target != null)
            {
                if (E.IsReady() && isHealthy && target.Distance(Yasuo) >= 0.30*Yasuo.AttackRange)
                {
                    if (DashCount >= 1 && GetDashPos(target).IsCloser(target) && target.IsDashable() &&
                        (GetBool("Combo.ETower") || GetKeyBind("Misc.TowerDive") ||
                         !GetDashPos(target).PointUnderEnemyTurret()))
                    {
                        ETarget = target;
                        E.Cast(target);
                        return;
                    }

                    if (DashCount == 0)
                    {
                        var bestminion =
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                                x =>
                                    x.IsDashable() && GetDashPos(x).IsCloser(target) &&
                                    (GetBool("Combo.ETower") || GetKeyBind("Misc.TowerDive") ||
                                     !GetDashPos(x).PointUnderEnemyTurret()))
                                .OrderByDescending(x => Vector2.Distance(GetDashPos(x), target.ServerPosition.To2D()))
                                .LastOrDefault(x => x != null);

                        if (bestminion != null)
                        {
                            ETarget = bestminion;
                            E.Cast(bestminion);
                        }

                        else if (target.IsDashable() && GetDashPos(target).IsCloser(target) &&
                                 (GetBool("Combo.ETower") || GetKeyBind("Misc.TowerDive") ||
                                  !GetDashPos(target).PointUnderEnemyTurret()))
                        {
                            ETarget = target;
                            E.Cast(target);
                        }
                    }


                    else
                    {
                        var minion =
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                                x =>
                                    x.IsDashable() && GetDashPos(x).IsCloser(target) &&
                                    (GetBool("Combo.ETower") || GetKeyBind("Misc.TowerDive") ||
                                     !GetDashPos(x).PointUnderEnemyTurret()))
                                .OrderByDescending(x => GetDashPos(x).Distance(target.ServerPosition))
                                .LastOrDefault(x => x != null);
                        if (minion != null && GetDashPos(minion).IsCloser(target))
                        {
                            ETarget = minion;
                            E.Cast(minion);
                        }
                    }
                }
            }
        }

        private void CastR(float minhit = 1)
        {
            var ultmode = GetUltMode();

            IOrderedEnumerable<AIHeroClient> ordered = null;


            if (ultmode == UltMode.Health)
            {
                ordered =
                    KnockedUp.OrderBy(x => x.Health)
                        .ThenByDescending(x => TargetSelector.GetPriority(x))
                        .ThenByDescending(x => EloBuddy.SDK.Extensions.CountEnemiesInRange(x, 350));
            }

            if (ultmode == UltMode.Priority)
            {
                ordered =
                    KnockedUp.OrderByDescending(x => TargetSelector.GetPriority(x))
                        .ThenBy(x => x.Health)
                        .ThenByDescending(x => EloBuddy.SDK.Extensions.CountEnemiesInRange(x, 350));
            }

            if (ultmode == UltMode.EnemiesHit)
            {
                ordered =
                    KnockedUp.OrderByDescending(x => EloBuddy.SDK.Extensions.CountEnemiesInRange(x, 350))
                        .ThenByDescending(x => TargetSelector.GetPriority(x))
                        .ThenBy(x => x.Health);
            }

            if ((GetBool("Combo.OnlyifMin") && ordered.Count() < minhit) ||
                (ordered.Count() == 1 && ordered.FirstOrDefault().HealthPercent < GetSliderInt("Combo.MinHealthUlt")))
            {
                return;
            }

            if (GetBool("Combo.RPriority"))
            {
                var best =
                    ordered.Find(
                        x =>
                            !x.isBlackListed() && TargetSelector.GetPriority(x) == 5 &&
                            (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") ||
                             !x.Position.To2D().PointUnderEnemyTurret()));
                if (best != null && Yasuo.HealthPercent/best.HealthPercent <= 1)
                {
                    R.Cast();
                    return;
                }
            }

            if (GetBool("Combo.UltOnlyKillable"))
            {
                var killable =
                    ordered.FirstOrDefault(
                        x =>
                            !x.isBlackListed() && x.Health <= DamageLibrary.GetSpellDamage(Yasuo, x, SpellSlot.R) &&
                            x.HealthPercent >= GetSliderInt("Combo.MinHealthUlt") &&
                            (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") ||
                             !x.Position.To2D().PointUnderEnemyTurret()));
                if (killable != null && !killable.IsInRange(Q.Range))
                {
                    R.Cast();
                    return;
                }
            }

            if (ordered.Count() >= minhit)
            {
                var best2 =
                    ordered.FirstOrDefault(
                        x =>
                            !x.isBlackListed() &&
                            (GetBool("Combo.UltTower") || GetKeyBind("Misc.TowerDive") ||
                             !x.Position.To2D().PointUnderEnemyTurret()));
                if (best2 != null)
                {
                    R.Cast();
                }
            }
        }

        private void Flee()
        {
            if (GetBool("Flee.UseQ2") && !Dash.IsDashing(Yasuo) && SpellSlot.Q.IsReady() && TornadoReady)
            {
                var qtarg = TargetSelector.GetTarget(Q2.Range, DamageType.Physical);
                if (qtarg != null)
                {
                    Q2.Cast(qtarg.ServerPosition);
                }
            }

            if (FleeMode == FleeType.ToCursor)
            {
                if (E.IsReady())
                {
                    var dashtarg =
                        EntityManager.Heroes.Enemies.Where(x => x.IsDashable())
                            .OrderByDescending(x => GetDashPos(x).Distance(Game.CursorPos)).LastOrDefault(x => x != null);

                    if (dashtarg != null &&
                        GetDashPos(dashtarg).Distance(Game.CursorPos) < Yasuo.Distance(Game.CursorPos))
                    {
                        E.Cast(dashtarg);

                        if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady)
                        {
                            Q.Cast(dashtarg);
                        }
                    }
                }
            }

            if (FleeMode == FleeType.ToNexus)
            {
                var nexus = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
                if (nexus != null)
                {
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
                    {
                        Orbwalker.MoveTo(nexus.Position);
                    }
                    var bestminion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsDashable())
                            .OrderByDescending(x => GetDashPos(x).Distance(nexus.Position))
                            .LastOrDefault(x => x != null);
                    if (bestminion != null &&
                        GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position))
                    {
                        E.Cast(bestminion);
                        if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady)
                        {
                            Q.Cast(bestminion.ServerPosition);
                        }
                    }
                }
            }

            if (FleeMode == FleeType.ToAllies)
            {
                Obj_AI_Base bestally =
                    EntityManager.Heroes.Allies.Where(x => !x.IsMe && x.CountEnemiesInRange(300) == 0)
                        .OrderByDescending(x => x.Distance(Yasuo))
                        .LastOrDefault(x => x != null);
                if (bestally == null)
                {
                    bestally =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(x => x.IsValidAlly(3000))
                            .MinOrDefault(x => x.Distance(Yasuo));
                }

                if (bestally != null)
                {
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
                    {
                        Orbwalker.MoveTo(bestally.ServerPosition);
                    }
                    if (E.IsReady())
                    {
                        var besttarget =
                            EntityManager.Heroes.Enemies.Where(x => x.IsDashable())
                                .OrderByDescending(x => GetDashPos(x).Distance(bestally.ServerPosition))
                                .LastOrDefault(x => x != null);
                        if (besttarget != null)
                        {
                            E.Cast(besttarget);
                            if (GetBool("Flee.StackQ") && SpellSlot.Q.IsReady() && !TornadoReady)
                            {
                                Q.Cast(besttarget.ServerPosition);
                            }
                        }
                    }
                }

                else
                {
                    var nexus = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
                    if (nexus != null)
                    {
                        Orbwalker.MoveTo(nexus.Position);
                        var bestminion =
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsDashable())
                                .OrderByDescending(x => GetDashPos(x).Distance(nexus.Position))
                                .LastOrDefault(x => x != null);

                        if (bestminion != null &&
                            GetDashPos(bestminion).Distance(nexus.Position) < Yasuo.Distance(nexus.Position))
                        {
                            E.Cast(bestminion);
                        }
                    }
                }
            }
        }

        private void CastIgnite()
        {
            var target =
                EntityManager.Heroes.Enemies.Find(
                    x =>
                        x.IsValidEnemy(Ignite.Range) &&
                        Yasuo.GetSummonerSpellDamage(x, DamageLibrary.SummonerSpells.Ignite) >= x.Health);
            if (Ignite.IsReady() && target != null)
            {
                Ignite.Cast(target);
            }
        }

        private void Waveclear()
        {
            if (SpellSlot.Q.IsReady() && !Dash.IsDashing(Yasuo))
            {
                if (!TornadoReady && GetBool("Waveclear.UseQ"))
                {
                    var minion =
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(x =>
                            x.IsValidMinion(Q
.Range) &&
                            ((x.IsDashable() &&
                              (x.Health - DamageLibrary.GetSpellDamage(Yasuo, x, SpellSlot.Q) >=
                               GetProperEDamage(x))) ||
                             (x.Health - DamageLibrary.GetSpellDamage(Yasuo, x, SpellSlot.Q) >= 0.15*x.MaxHealth ||
                              x.QCanKill()))).OrderByDescending(x => x.MaxHealth).LastOrDefault(x => x != null);

                        
                    if (minion != null)
                    {
                        Q.Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Waveclear.UseQ2"))
                {
                    var minions =
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(x =>
                            x.Distance(Yasuo) > Yasuo.AttackRange && x.IsValidMinion(Q2.Range) &&
                            ((x.IsDashable() &&
                              x.Health - DamageLibrary.GetSpellDamage(Yasuo, x, SpellSlot.Q) >=
                              0.85*GetProperEDamage(x)) ||
                             (x.Health - DamageLibrary.GetSpellDamage(Yasuo, x, SpellSlot.Q) >= 0.10*x.MaxHealth) ||
                             x.CanKill(SpellSlot.Q)));

                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.LSTo2D()).ToList(),
                            Q2.Width, Q2.Range);
                    if (pred.MinionsHit >= GetSliderInt("Waveclear.Qcount"))
                    {
                        Q2.Cast(pred.Position.To3D());
                    }
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("Waveclear.UseE") && (!GetBool("Waveclear.Smart") || isHealthy))
            {
                var minions =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(x =>
                        x.IsDashable() &&
                        ((GetBool("Waveclear.UseENK") &&
                          (!GetBool("Waveclear.Smart") || x.Health - GetProperEDamage(x) > GetProperEDamage(x)*3)) ||
                         x.ECanKill()) &&
                        (GetBool("Waveclear.ETower") || GetKeyBind("Misc.TowerDive") ||
                         !GetDashPos(x).PointUnderEnemyTurret()));
                Obj_AI_Minion minion = null;
                minion = minions.MaxOrDefault(x => GetDashPos(x).MinionsInRange(200));
                if (minion != null)
                {
                    E.Cast(minion);
                }
            }

            if (GetBool("Waveclear.UseItems"))
            {
                if (GetBool("Waveclear.UseTIA"))
                {
                    Tiamat.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Tiamat.Cast(null, true);
                }
                if (GetBool("Waveclear.UseHDR"))
                {
                    Hydra.minioncount = GetSliderInt("Waveclear.MinCountHDR");
                    Hydra.Cast(null, true);
                }
                if (GetBool("Waveclear.UseYMU"))
                {
                    Youmu.minioncount = GetSliderInt("Waveclear.MinCountYOU");
                    Youmu.Cast(null, true);
                }
            }
        }

        private void Killsteal()
        {
            if (SpellSlot.Q.IsReady() && GetBool("Killsteal.UseQ"))
            {
                var targ = EntityManager.Heroes.Enemies.Find(x => x.CanKill(SpellSlot.Q) && x.IsInRange(Qrange));
                if (targ != null)
                {
                    UseQ(targ, GetHitChance("Hitchance.Q"));
                    return;
                }
            }

            if (SpellSlot.E.IsReady() && GetBool("Killsteal.UseE"))
            {
                var targ = EntityManager.Heroes.Enemies.Find(x => x.CanKill(SpellSlot.E) && x.IsInRange(E.Range));
                if (targ != null)
                {
                    E.Cast(targ);
                    return;
                }
            }

            if (SpellSlot.R.IsReady() && GetBool("Killsteal.UseR"))
            {
                var targ = KnockedUp.Find(x => x.CanKill(SpellSlot.R) && x.IsValidEnemy(R.Range) && !x.isBlackListed());
                if (targ != null)
                {
                    R.Cast();
                    return;
                }
            }

            if (GetBool("Killsteal.UseIgnite"))
            {
                CastIgnite();
                return;
            }

            if (GetBool("Killsteal.UseItems"))
            {
                if (Tiamat.item.IsReady())
                {
                    var targ =
                        EntityManager.Heroes.Enemies.Find(
                            x =>
                                x.IsValidEnemy(Tiamat.item.Range) &&
                                x.Health <= Yasuo.GetItemDamage(x, ItemId.Tiamat_Melee_Only));
                    if (targ != null)
                    {
                        Tiamat.Cast(null);
                    }
                }
                if (Hydra.item.IsReady())
                {
                    var targ =
                        EntityManager.Heroes.Enemies.Find(
                            x =>
                                x.IsValidEnemy(Hydra.item.Range) &&
                                x.Health <= Yasuo.GetItemDamage(x, ItemId.Tiamat_Melee_Only));
                    if (targ != null)
                    {
                        Hydra.Cast(null);
                    }
                }
                if (Blade.item.IsReady())
                {
                    var targ = EntityManager.Heroes.Enemies.Find(
                        x =>
                            x.IsValidEnemy(Blade.item.Range) &&
                            x.Health <= Yasuo.GetItemDamage(x, ItemId.Blade_of_the_Ruined_King));
                    if (targ != null)
                    {
                        Blade.Cast(targ);
                    }
                }
                if (Bilgewater.item.IsReady())
                {
                    var targ = EntityManager.Heroes.Enemies.Find(
                        x =>
                            x.IsValidEnemy(Bilgewater.item.Range) &&
                            x.Health <= Yasuo.GetItemDamage(x, ItemId.Bilgewater_Cutlass));
                    if (targ != null)
                    {
                        Bilgewater.Cast(targ);
                    }
                }
            }
        }

        private void Harass()
        {
            //No harass under enemy turret to avoid aggro
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q2.Range, DamageType.Physical);
            if (SpellSlot.Q.IsReady() && target != null && target.IsInRange(Qrange))
            {
                UseQ(target, GetHitChance("Hitchance.Q"), GetBool("Harass.UseQ"), GetBool("Harass.UseQ2"));
            }

            if (target != null && isHealthy && GetBool("Harass.UseE") && E.IsReady() && target.IsInRange(E.Range*3) &&
                !target.Position.To2D().PointUnderEnemyTurret())
            {
                if (target.IsInRange(E.Range))
                {
                    ETarget = target;
                    E.Cast(target);
                    return;
                }

                var minion =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                        x => x.IsDashable() && !x.ServerPosition.To2D().PointUnderEnemyTurret())
                        .OrderByDescending(x => GetDashPos(x).Distance(target.ServerPosition))
                        .FirstOrDefault(x => x != null);

                if (minion != null && GetBool("Harass.UseEMinion") && GetDashPos(minion).IsCloser(target))
                {
                    ETarget = minion;
                    E.Cast(minion);
                }
            }
        }

        private void Mixed()
        {
            if (GetBool("Harass.InMixed"))
            {
                Harass();
            }
            LhSkills();
        }

        private void LhSkills()
        {
            if (SpellSlot.Q.IsReady() && !Dash.IsDashing(Yasuo))
            {
                if (!TornadoReady && GetBool("Farm.UseQ"))
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .FirstOrDefault(x => x.IsValidMinion(Q.Range) && x.QCanKill());
                    if (minion != null)
                    {
                        Q.Cast(minion.ServerPosition);
                    }
                }

                else if (TornadoReady && GetBool("Farm.UseQ2"))
                {
                    var minions =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                x =>
                                    x.Distance(Yasuo) > Yasuo.AttackRange && x.IsValidMinion(Q2.Range) && (x.QCanKill()));
                    var pred =
                        MinionManager.GetBestLineFarmLocation(minions.Select(m => m.ServerPosition.To2D()).ToList(),
                            Q2.Width, Q2.Range);
                    if (pred.MinionsHit >= GetSliderInt("Farm.Qcount"))
                    {
                        Q2.Cast(pred.Position.To3D());
                    }
                }
            }

            if (E.IsReady() && GetBool("Farm.UseE"))
            {
                var minion =
                    ObjectManager.Get<Obj_AI_Minion>()
                        .FirstOrDefault(
                            x =>
                                x.IsDashable() && x.ECanKill() &&
                                (GetBool("Waveclear.ETower") || GetKeyBind("Misc.TowerDive") ||
                                 !GetDashPos(x).PointUnderEnemyTurret()));
                if (minion != null)
                {
                    E.Cast(minion);
                }
            }
        }

        private void OnGapClose(AIHeroClient target, Gapcloser.GapcloserEventArgs args)
        {
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }
            if (GetBool("Misc.AG") && TornadoReady && Yasuo.Distance(args.End) <= 500)
            {
                var pred = Q2.GetPrediction(args.Sender);
                if (pred.HitChance >= GetHitChance("Hitchance.Q"))
                {
                    Q2.Cast(pred.CastPosition);
                }
            }
        }

        private void OnInterruptable(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
        {
            if (Yasuo.ServerPosition.PointUnderEnemyTurret())
            {
                return;
            }
            if (GetBool("Misc.Interrupter") && TornadoReady && Yasuo.Distance(sender.ServerPosition) <= 500)
            {
                if (args.EndTime >= Q2.CastDelay)
                {
                    Q2.Cast(sender.ServerPosition);
                }
            }
        }
    }
}