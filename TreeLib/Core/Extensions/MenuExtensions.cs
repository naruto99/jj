using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace TreeLib.Extensions
{
    public static class MenuExtensions
    {
        public static void AddBool(this Menu Menu, string name, string displayName, bool value = true)
        {
            Menu.Add(name, new CheckBox(displayName, value));
        }

        public static void AddHitChance(this Menu Menu, string name, string displayName, HitChance defaultHitChance)
        {
            Menu.Add(name, new Slider(displayName, (int) defaultHitChance - 3, 0, 3));
        }

        public static void AddSlider(this Menu Menu,
            string name,
            string displayName,
            int value,
            int min = 0,
            int max = 100)
        {
            Menu.Add(name, new Slider(displayName, value, min, max));
        }

        public static void AddInfo(this Menu Menu, string text)
        {
            Menu.AddLabel(text);
        }

        public static void AddKeyBind(this Menu Menu,
            string name,
            string displayName,
            uint key,
            KeyBind.BindTypes type = KeyBind.BindTypes.HoldActive,
            bool defaultValue = false)
        {
            Menu.Add(name, new KeyBind(displayName, defaultValue, type, key));
        }

        public static Menu AddSpell(this Menu Menu, SpellSlot spell, List<Orbwalker.ActiveModes> modes)
        {
            var spellMenu = Menu.AddSubMenu(spell.ToString(), spell.ToString());
            foreach (var mode in modes)
            {
                spellMenu.AddBool(mode.ToString() + spell, "Use in " + mode);
            }

            return spellMenu;
        }
    }
}