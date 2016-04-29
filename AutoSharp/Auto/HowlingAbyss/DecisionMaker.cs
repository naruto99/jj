using System;
using System.Linq;
using AutoSharp.Auto.SummonersRift;
using AutoSharp.Utils;
using EloBuddy;
using EloBuddy.SDK;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.HowlingAbyss
{
    internal static class DecisionMaker
    {
        private static int _lastUpdate = 0;
        public static void OnUpdate(EventArgs args)
        {
            if (Environment.TickCount - _lastUpdate < 150) return;
            _lastUpdate = Environment.TickCount;

            var player = Heroes.Player;

            if (Decisions.ImSoLonely())
            {
                return;
            }

            if (Program.OptionsHealUp && Decisions.HealUp())
            {
                return;
            }

            if (player.UnderTurret(true) && Wizard.GetClosestEnemyTurret().CountNearbyAllyMinions(700) <= 3 && Wizard.GetClosestEnemyTurret().CountAlliesInRange(700) == 0)
            {
                Orbwalker.ActiveModesFlags = Orbwalker.ActiveModes.Harass;
                Player.IssueOrder(GameObjectOrder.MoveTo, player.Position.Extend(HeadQuarters.AllyHQ.Position.RandomizePosition(), 800).To3D());
                return;
            }

            if (Heroes.Player.InFountain())
            {
                Shopping.Shop();
                Wizard.AntiAfk();
            }

            if (Decisions.Farm())
            {
                return;
            }
            Decisions.Fight();

            if (Orbwalker.OrbwalkPosition.IsZero || Orbwalker.OrbwalkPosition == Game.CursorPos)
            {
                Decisions.ImSoLonely();
            }
        }
    }
}
