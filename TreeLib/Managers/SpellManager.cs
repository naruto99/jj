using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using TreeLib.Extensions;
using Menu = EloBuddy.SDK.Menu.Menu;
using Spell = EloBuddy.SDK.Spell;

namespace TreeLib.Managers
{
    public static class SpellManager
    {
        public static Spell.Targeted Ignite;
        public static Spell.Targeted Smite;
        internal static Menu Menu;

        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = MainMenu.AddMenu("Summoners", "Summoners");
            Menu.AddBool("SmiteManagerEnabled", "Load Smite Manager");
            Menu.AddBool("IgniteManagerEnabled", "Load Ignite Manager");

            var smite = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(h => h.Name.ToLower().Contains("smite"));

            if (smite != null && !smite.Slot.Equals(SpellSlot.Unknown))
            {
                Smite = new Spell.Targeted(smite.Slot, 500);

                if (Menu["SmiteManagerEnabled"].Cast<CheckBox>().CurrentValue)
                {
                    SmiteManager.Initialize();
                }
            }

            var igniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            if (!igniteSlot.Equals(SpellSlot.Unknown))
            {
                Ignite = new Spell.Targeted(igniteSlot, 600);

                if (Menu["IgniteManagerEnabled"].Cast<CheckBox>().CurrentValue)
                {
                    IgniteManager.Initialize();
                }
            }
        }
    }
}