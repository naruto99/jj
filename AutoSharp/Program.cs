using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoSharp.Auto;
using AutoSharp.Utils;
using EloBuddy;
using LeagueSharp;
using LeagueSharp.Common;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Utility = LeagueSharp.Common.Utility;

// ReSharper disable ObjectCreationAsStatement

namespace AutoSharp
{
    class Program
    {
        public static Utility.Map.MapType Map;
        public static Menu Config, options,randomizer;
        private static bool _loaded = false;

        public static int AutoSharpHumanizer => Config["autosharp.humanizer"].Cast<Slider>().CurrentValue;
        public static bool AutoSharpQuit => Config["autosharp.quit"].Cast<CheckBox>().CurrentValue;
        public static bool AutoSharpShop => Config["autosharp.shop"].Cast<CheckBox>().CurrentValue;
        public static bool OptionsHealUp => options["autosharp.options.healup"].Cast<CheckBox>().CurrentValue;
        public static bool OnlyFarm => options["onlyfarm"].Cast<CheckBox>().CurrentValue;
        public static int RecallHp => options["recallhp"].Cast<Slider>().CurrentValue;
        public static int RandomizerMinRand => randomizer["autosharp.randomizer.minrand"].Cast<Slider>().CurrentValue;
        public static int RandomizerMaxRand => randomizer["autosharp.randomizer.maxrand"].Cast<Slider>().CurrentValue;
        public static bool PlayDefensive => randomizer["autosharp.randomizer.playdefensive"].Cast<CheckBox>().CurrentValue;
        public static bool RandomizerAuto => randomizer["autosharp.randomizer.auto"].Cast<CheckBox>().CurrentValue;
        
        public static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (sender == null) return;
            if (args.Target.NetworkId == ObjectManager.Player.NetworkId && (sender is Obj_AI_Turret || sender is Obj_AI_Minion))
            {
                EloBuddy.SDK.Orbwalker.OrbwalkTo(Heroes.Player.Position.Extend(Wizard.GetFarthestMinion().Position, 500).RandomizePosition());
            }
        }

        private static void AntiShrooms2(EventArgs args)
        {
            if (Map == Utility.Map.MapType.SummonersRift && !Heroes.Player.InFountain() &&
                Heroes.Player.HealthPercent < RecallHp)
            {
                if (Heroes.Player.HealthPercent > 0 && Heroes.Player.CountEnemiesInRange(1800) == 0 &&
                    !Turrets.EnemyTurrets.Any(t => t.Distance(Heroes.Player) < 950) &&
                    !Minions.EnemyMinions.Any(m => m.Distance(Heroes.Player) < 950))
                {
                    if (!Heroes.Player.HasBuff("Recall"))
                    {
                        Heroes.Player.Spellbook.CastSpell(SpellSlot.Recall);
                    }
                }
            }

            var turretNearTargetPosition =
                    Turrets.EnemyTurrets.FirstOrDefault(t => t.Distance(Heroes.Player.ServerPosition) < 950);
            if (turretNearTargetPosition != null && turretNearTargetPosition.CountNearbyAllyMinions(950) < 3)
            {
                EloBuddy.SDK.Orbwalker.OrbwalkTo(Heroes.Player.Position.Extend(HeadQuarters.AllyHQ.Position, 950).To3D());
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                if (sender.Owner.IsDead)
                {
                    args.Process = false;
                    return;
                }
                if (Map == Utility.Map.MapType.SummonersRift)
                {
                    if (OnlyFarm && args.Target.IsValid<AIHeroClient>() &&
                        args.Target.IsEnemy)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.InFountain() && args.Slot == SpellSlot.Recall)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.HasBuff("Recall"))
                    {
                        args.Process = false;
                        return;
                    }
                }
                if (Heroes.Player.UnderTurret(true) && args.Target.IsValid<AIHeroClient>())
                {
                    args.Process = false;
                    return;
                }
            }
        }

        private static void OnEnd(GameEndEventArgs args)
        {
            if (AutoSharpQuit)
            {
                Thread.Sleep(3000);
                Game.QuitGame();
            }
        }

        public static void AntiShrooms(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (sender != null && sender.IsMe)
            {
                if (sender.IsDead)
                {
                    args.Process = false;
                    return;
                }
                var turret = Turrets.ClosestEnemyTurret;
                var turret2 = turret as Obj_AI_Base;
                if (Map == Utility.Map.MapType.SummonersRift && Heroes.Player.HasBuff("Recall") && Heroes.Player.CountEnemiesInRange(1800) == 0 &&
                    turret.Distance(Heroes.Player) > 950 && !Minions.EnemyMinions.Any(m => m.Distance(Heroes.Player) < 950))
                {
                    args.Process = false;
                    return;
                }

                if (args.Order == GameObjectOrder.MoveTo)
                {
                    if (args.TargetPosition.IsZero)
                    {
                        args.Process = false;
                        return;
                    }
                    if (!args.TargetPosition.IsValid())
                    {
                        args.Process = false;
                        return;
                    }
                    if (Map == Utility.Map.MapType.SummonersRift && Heroes.Player.InFountain() &&
                        Heroes.Player.HealthPercent < 100)
                    {
                        args.Process = false;
                        return;
                    }
                    if (turret != null && turret.Distance(args.TargetPosition) < 950 &&
                        turret.CountNearbyAllyMinions(950) < 3)
                    {
                        args.Process = false;
                        return;
                    }
                }

                #region BlockAttack

                if (args.Target != null && args.Order == GameObjectOrder.AttackUnit || args.Order == GameObjectOrder.AttackTo)
                {
                    if (OnlyFarm && args.Target.IsValid<AIHeroClient>())
                    {
                        args.Process = false;
                        return;
                    }
                    if (args.Target.IsValid<AIHeroClient>())
                    {
                        if (Minions.AllyMinions.Count(m => m.Distance(Heroes.Player) < 900) <
                            Minions.EnemyMinions.Count(m => m.Distance(Heroes.Player) < 900))
                        {
                            args.Process = false;
                            return;
                        }
                        if (((AIHeroClient) args.Target).UnderTurret(true))
                        {
                            args.Process = false;
                            return;
                        }
                    }
                    if (Heroes.Player.UnderTurret(true) && args.Target.IsValid<AIHeroClient>())
                    {
                        args.Process = false;
                        return;
                    }
                    if (turret != null && turret.Distance(ObjectManager.Player) < 950 && turret.CountNearbyAllyMinions(950) < 3)
                    {
                        args.Process = false;
                        return;
                    }
                    if (Heroes.Player.HealthPercent < RecallHp)
                    {
                        args.Process = false;
                        return;
                    }
                }

                #endregion
            }
            if (sender != null && args.Target != null && args.Target.IsMe)
            {
                if (sender is Obj_AI_Turret || sender is Obj_AI_Minion)
                {
                    var minion = Wizard.GetClosestAllyMinion();
                    if (minion != null)
                    {
                        EloBuddy.SDK.Orbwalker.OrbwalkTo(Heroes.Player.Position.Extend(Wizard.GetClosestAllyMinion().Position, Heroes.Player.LSDistance(minion) + 100).To3D());
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            Game.OnUpdate += AdvancedLoading;
        }

        private static void AdvancedLoading(EventArgs args)
        {
            if (!_loaded)
            {
                if (ObjectManager.Player.Gold > 0)
                {
                    _loaded = true;
                    Utility.DelayAction.Add(new Random().Next(3000, 25000), Init);
                }
            }
        }

        private static void Init()
        {
            Map = Utility.Map.GetMap().Type;
            Config = MainMenu.AddMenu("AutoSharp: " + ObjectManager.Player.ChampionName, "autosharp." + ObjectManager.Player.ChampionName);
            Config.AddLabel("AutoSharp - Ported by Rexy");
            Autoplay.Load();
            Config.Add("autosharp.humanizer", new Slider("Humanize movement by", new Random().Next(125, 350), 125, 350));
            Config.Add("autosharp.quit", new CheckBox("Quit after Game End"));
            Config.Add("autosharp.shop", new CheckBox("AutoShop?"));
            options = Config.AddSubMenu("Options");
            options.Add("autosharp.options.healup", new CheckBox("Take Heals?"));
            options.Add("onlyfarm", new CheckBox("Only Darm", false));
            if (Map == Utility.Map.MapType.SummonersRift)
            {
                options.Add("recallhp", new Slider("Recall if Health% <", 30));
            }

            randomizer = Config.AddSubMenu("Randomizer");
            randomizer.Add("autosharp.randomizer.minrand", new Slider("Min Rand By", 0, 0, 90));
            randomizer.Add("autosharp.randomizer.maxrand", new Slider("Max Rand By", 100, 100, 300));
            randomizer.Add("autosharp.randomizer.playdefensive", new CheckBox("Play Defensive?"));
            randomizer.Add("autosharp.randomizer.auto", new CheckBox("Auto-Adjust? (ALPHA)"));

            Cache.Load();
            Game.OnUpdate += Positioning.OnUpdate;
            Autoplay.Load();
            Game.OnEnd += OnEnd;
            Player.OnIssueOrder += AntiShrooms;
            Game.OnUpdate += AntiShrooms2;
            Spellbook.OnCastSpell += OnCastSpell;
            Obj_AI_Base.OnDamage += OnDamage;



            Utility.DelayAction.Add(
                    new Random().Next(1000, 10000), () =>
                    {
                        new LeagueSharp.Common.AutoLevel(Utils.AutoLevel.GetSequence().Select(num => num - 1).ToArray());
                        LeagueSharp.Common.AutoLevel.Enable();
                        Console.WriteLine("AutoLevel Init Success!");
                    });
        }
    }
}
