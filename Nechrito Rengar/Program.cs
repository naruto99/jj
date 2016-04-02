using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using Nechrito_Rengar.Classes;
using Nechrito_Rengar.Classes.Modes;

namespace Nechrito_Rengar
{
    class Program : Logic
    {
        public static readonly int[] BlueSmite = { 3706, 1400, 1401, 1402, 1403 };

        public static readonly int[] RedSmite = { 3715, 1415, 1414, 1413, 1412 };

        private static void Main()
        {
            Loading.OnLoadingComplete += OnGameLoad;
        }
        private static void OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Rengar") return;
            Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">Nechrito Rengar</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\"> Version: 3 (Date: 4/2-16)</font></b>");
            Chat.Print("<b><font color=\"#FFFFFF\">[</font></b><b><font color=\"#00e5e5\">Ported by</font></b><b><font color=\"#FFFFFF\">]</font></b><b><font color=\"#FFFFFF\">Rexy</font></b>");

            MenuConfig.LoadMenu();
            Spells.Initialise();
            Game.OnUpdate += OnTick;
            Obj_AI_Base.OnProcessSpellCast += OnDoCast;
            Obj_AI_Base.OnProcessSpellCast += OnDoCastLc;
            Drawing.OnDraw += Drawing_OnDraw;
       

        }
        private static void OnTick(EventArgs args)
        {
            SmiteCombo();
            Killsteal._Killsteal();
            AutoHp();

            if (RengarHasUlti)
            {
                return;
            }

            if (MenuConfig.BurstModeActive)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Burst.BurstLogic();
                }
            }

            else if (!MenuConfig.BurstModeActive)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    switch (MenuConfig.ComboModeValue)
                    {
                        case 1:
                        {
                            Combo.ComboLogic();
                            break;
                        }
                        case 2:
                        {
                            ApCombo.ApComboLogic();
                            break;
                        }
                    }
                }
            }
        }

        private static void AutoHp()
        {
            var autoHpActive = MenuConfig.Config["AutoHp.Active"].Cast<CheckBox>().CurrentValue;
            var autoHpValue = MenuConfig.Config["AutoHp"].Cast<Slider>().CurrentValue;

            if (!autoHpActive)
            {
                return;
            }
            if (Player.HealthPercent <= autoHpValue)
            {
                if (Spells.W.IsReady() && (int)Player.Mana == 5)
                {
                    Spells.W.Cast();
                }
            }
        }

        private static void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spellName = args.SData;
            if (!sender.IsMe || spellName.IsAutoAttack()) return;

            var hero = args.Target as AIHeroClient;
            if (hero == null) return;
            var target = hero;
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Spells.E.IsReady())
                    {
                        Spells.E.Cast(target);
                    }
                }
            }
        }
        private static void OnDoCastLc(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target is Obj_AI_Minion)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,Player.ServerPosition,600f).FirstOrDefault();
                    {
                        if (minions == null || (int)Player.Mana == 5 && MenuConfig.Passive)
                            return;
                        if(Player.Mana <= 5)
                        {
                            if (Spells.W.IsReady())
                                Spells.W.Cast();
                            
                            if (Spells.E.IsReady() && Player.Mana < 5)
                                Spells.E.Cast(minions);
                            if (Spells.Q.IsReady())
                            {
                                Spells.Q.Cast();
                                CastHydra();
                            }
                                
                        }  
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            var drawcurrentmode = MenuConfig.Config["DrawCurrentMode"].Cast<CheckBox>().CurrentValue;

            if (drawcurrentmode)
            {
                if (MenuConfig.BurstModeActive)
                {
                    Drawing.DrawText(Drawing.Width*0.70f, Drawing.Height*0.95f, System.Drawing.Color.Yellow,
                        "Mode : Burst");
                }
                else if (!MenuConfig.BurstModeActive)
                {
                    switch (MenuConfig.ComboModeValue)
                    {
                        case 1:
                        {
                            Drawing.DrawText(Drawing.Width*0.70f, Drawing.Height*0.95f, System.Drawing.Color.White,
                                "Mode : Combo");
                            break;
                        }
                        case 2:
                        {
                            Drawing.DrawText(Drawing.Width*0.70f, Drawing.Height*0.95f, System.Drawing.Color.Aqua,
                                "Mode : APCombo");
                            break;
                        }
                    }
                }
            }
        }
    }
}
   

