using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp.Common;
using TreeLib.Extensions;
using Champion = TreeLib.Objects.Champion;
using Menu = EloBuddy.SDK.Menu.Menu;

namespace TreeLib.Managers
{
    public static class ManaManager
    {
        public enum ManaMode
        {
            Combo,
            Harass,
            Farm,
            None
        }

        private static Menu _menu;

        private static readonly Dictionary<ManaMode, Dictionary<SpellSlot, int>> ManaDictionary =
            new Dictionary<ManaMode, Dictionary<SpellSlot, int>>();

        private static ManaMode CurrentMode
        {
            get
            {
                switch (Champion.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        return ManaMode.Combo;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        return ManaMode.Harass;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        return ManaMode.Farm;
                }

                return ManaMode.None;
            }
        }

        public static void Initialize(Menu menu)
        {
            _menu = menu.AddSubMenu("Mana Manager");
            _menu.AddBool("Enabled", "Enabled", false);
        }

        public static void SetManaCondition(this Spell spell, ManaMode mode, int value)
        {
            if (!ManaDictionary.ContainsKey(mode))
            {
                ManaDictionary.Add(mode, new Dictionary<SpellSlot, int>());
            }

            ManaDictionary[mode].Add(spell.Slot, value);
            var m = mode.ToString();

            _menu.AddSubMenu(m).AddBool(m + "Enabled", "Enabled in " + m);

            _menu.AddSubMenu(m)
                .AddSlider(ObjectManager.Player.ChampionName + spell.Slot + "Mana", spell.Slot + " Mana Percent", value);
        }

        public static bool HasManaCondition(this Spell spell)
        {
            if (!_menu["Enabled"].Cast<CheckBox>().CurrentValue)
            {
                return false;
            }

            var mode = CurrentMode;

            if (mode == ManaMode.None || !ManaDictionary.ContainsKey(mode) ||
                !_menu[mode + "Enabled"].Cast<CheckBox>().CurrentValue)
            {
                return false;
            }

            var currentMode = ManaDictionary[mode];

            if (!currentMode.ContainsKey(spell.Slot))
            {
                return false;
            }

            return ObjectManager.Player.ManaPercent < currentMode[spell.Slot];
        }
    }
}