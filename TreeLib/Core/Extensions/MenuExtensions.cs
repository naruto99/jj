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
        public static void AddBool(this Menu menu, string name, string displayName, bool value = true)
        {
            menu.Add(name, new CheckBox(displayName, value));
        }

        public static void AddHitChance(this Menu menu, string name, string displayName, HitChance defaultHitChance)
        {
            menu.Add(name, new Slider(displayName, (int) defaultHitChance - 3, 0, 3));
        }

        public static void AddSlider(this Menu menu,
            string name,
            string displayName,
            int value,
            int min = 0,
            int max = 100)
        {
            menu.Add(name, new Slider(displayName, value, min, max));
        }

        public static void AddInfo(this Menu menu, string text)
        {
            menu.AddLabel(text);
        }

        public static void AddKeyBind(this Menu menu,
            string name,
            string displayName,
            uint key,
            KeyBind.BindTypes type = KeyBind.BindTypes.HoldActive,
            bool defaultValue = false)
        {
            menu.Add(name, new KeyBind(displayName, defaultValue, type, key));
        }

        public static Menu AddSpell(this Menu menu, SpellSlot spell, List<Orbwalker.ActiveModes> modes)
        {
            var spellMenu = menu.AddSubMenu(spell.ToString(), spell.ToString());
            foreach (var mode in modes)
            {
                spellMenu.AddBool(mode.ToString() + spell, "Use in " + mode);
            }

            return spellMenu;
        }
    }
}