﻿using EloBuddy;
using LeagueSharp.Common;

namespace Evade
{
    internal static class Helpers
    {
        public static bool IsSpellShielded(this AIHeroClient unit)
        {
            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellShield))
            {
                return true;
            }

            if (ObjectManager.Player.HasBuffOfType(BuffType.SpellImmunity))
            {
                return true;
            }

            //Sivir E
            if (unit.LastCastedSpellName() == "SivirE" &&
                (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            //Morganas E
            if (unit.LastCastedSpellName() == "BlackShield" &&
                (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            //Nocturnes E
            if (unit.LastCastedSpellName() == "NocturneShit" &&
                (LeagueSharp.Common.Utils.TickCount - unit.LastCastedSpellT()) < 300)
            {
                return true;
            }

            return false;
        }
    }
}