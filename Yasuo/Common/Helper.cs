﻿using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace BrianSharp.Common
{
    internal class Helper : Program
    {
        #region Enums

        public enum SmiteType
        {
            Grey,

            Purple,

            Red,

            Blue,

            None
        }

        #endregion

        #region Public Methods and Operators

        public static void AddBool(Menu subMenu, string item, string display, bool state = true)
        {
            subMenu.Add("_" + subMenu.DisplayName + "_" + item, new CheckBox(display, state));
            //return subMenu.AddItem(new MenuItem("_" + subMenu.Name + "_" + item, display, true).SetValue(state));
        }

        public static void AddKeybind(
            Menu subMenu,
            string item,
            string display,
            uint key,
            KeyBind.BindTypes type = KeyBind.BindTypes.HoldActive,
            bool state = false)
        {
            subMenu.Add("_" + subMenu.DisplayName + "_" + item, new KeyBind(display, state, type, key));
        }

        public static void AddSlider(Menu subMenu, string item, string display, int cur, int min = 1, int max = 100)
        {
            subMenu.Add("_" + subMenu.DisplayName + "_" + item, new Slider(display, cur, min, max));
        }

        public static void AddText(Menu subMenu, string item, string display)
        {
            subMenu.AddLabel(display);
        }

        public static bool CanKill(Obj_AI_Base target, double subDmg)
        {
            return target.Health < subDmg;
        }

        public static bool CastFlash(Vector3 pos)
        {
            return Player.Spellbook.GetSpell(Flash).IsReady && pos.IsValid() && Player.Spellbook.CastSpell(Flash, pos);
        }

        public static bool CastIgnite(AIHeroClient target)
        {
            return Player.Spellbook.GetSpell(Ignite).IsReady && target.IsValidTarget(600)
                   && target.Health + 5 < Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite)
                   && Player.Spellbook.CastSpell(Ignite, target);
        }

        public static ValueBase GetItem(string subMenu, string item)
        {
            return Menu["_" + subMenu + "_" + item];
        }

        public static bool IsPet(Obj_AI_Minion obj)
        {
            var pets = new[]
            {
                "annietibbers", "elisespiderling", "heimertyellow", "heimertblue", "leblanc",
                "malzaharvoidling", "shacobox", "shaco", "yorickspectralghoul", "yorickdecayedghoul",
                "yorickravenousghoul", "zyrathornplant", "zyragraspingplant"
            };
            return pets.Contains(obj.CharData.BaseSkinName.ToLower());
        }

        #endregion
    }
}