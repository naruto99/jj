using System;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
//Added Elobuddy for my Buddy references


namespace Kalista_Airlines
{
    internal class Program
    {
        private static Menu _menu;

        private static bool FlyActive
        {
            get { return _menu["Fly"].Cast<CheckBox>().CurrentValue; }
        }

        private static int FlySpeed
        {
            get { return _menu["Flyspeed"].Cast<Slider>().CurrentValue; }
        }

        private static int LastAaTick
        {
            get { return Orbwalker.LastAutoAttack; }
        }

        private static void Main()
        {
            //Intializing Game
            Loading.OnLoadingComplete += GameOnLoadedAndIntialized;
        }

        private static void GameOnLoadedAndIntialized(EventArgs args)
        {
            try
            {
                MenuIntializer();
                ChatCredit();
                Game.OnUpdate += IntializeLogicExecuter;
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void MenuIntializer()
        {
            try
            {
                _menu = MainMenu.AddMenu("Kalista Airlines", "Kalista.Airlines");
                _menu.AddLabel("Kalista Airlines");
                _menu.AddLabel("Updated and modified by Rexy");
                _menu.Add("Fly", new CheckBox("Use FlyHack", false));
                _menu.Add("Flyspeed", new Slider("Fly Speed (Adjust Utill it works)", 250, 0, 1000));
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void IntializeLogicExecuter(EventArgs args)
        {
            try
            {
                ComboLogic();
                LaneLogic();
                JungleLogic();
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void ComboLogic()
        {
            try
            {
                if (FlyActive && Player.Instance.AttackSpeedMod >= 2.5)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        var target = TargetSelector.GetTarget(
                            ObjectManager.Player.GetAutoAttackRange(),
                            DamageType.Physical);
                        if (target.IsValidTarget(ObjectManager.Player.GetAutoAttackRange()))
                        {
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                >= LastAaTick + 1)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            }
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                > LastAaTick + ObjectManager.Player.AttackDelay*1000 - 250)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void LaneLogic()
        {
            try
            {
                if (FlyActive && Player.Instance.AttackSpeedMod >= 2.5)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    {
                        var target =
                            EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsVisible)
                                .OrderByDescending(x => x.Health)
                                .LastOrDefault(x => x != null);
                        if (target.IsValidTarget(ObjectManager.Player.GetAutoAttackRange()))
                        {
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                >= LastAaTick + 1)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            }
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                > LastAaTick + ObjectManager.Player.AttackDelay*1000 - 250)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void JungleLogic()
        {
            try
            {
                if (FlyActive && Player.Instance.AttackSpeedMod >= 2.5)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    {
                        var target =
                            EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.ServerPosition,
                                ObjectManager.Player.GetAutoAttackRange())
                                .Where(x => x.IsVisible)
                                .OrderByDescending(x => x.MaxHealth)
                                .LastOrDefault(x => x != null);
                        if (target.IsValidTarget(ObjectManager.Player.GetAutoAttackRange()))
                        {
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                >= LastAaTick + 1)
                            {
                                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                            }
                            if (Game.Time*(1000 - FlySpeed) - Game.Ping
                                > LastAaTick + ObjectManager.Player.AttackDelay*1000 - 250)
                            {
                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            }
                        }
                        else
                        {
                            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }

        private static void ChatCredit()
        {
            try
            {
                Chat.Print("Kalista Airlines | Loaded", Color.Aqua);
                Chat.Print("Kalista Airlines | Modified and updated by Rexy", Color.Aqua);
            }
            catch (Exception e)
            {
                //Anti Fps Drop
                Console.WriteLine(e);
            }
        }
    }
}