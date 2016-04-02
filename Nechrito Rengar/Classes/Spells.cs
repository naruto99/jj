using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Nechrito_Rengar.Classes
{
    class Spells
    {
        private static readonly AIHeroClient Player = ObjectManager.Player;
        public static SpellSlot Ignite;
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static void Initialise()
        {
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W,300);
            E = new Spell.Skillshot(SpellSlot.E,900 ,SkillShotType.Linear,(int)0.125f,1500,70);
            R = new Spell.Active(SpellSlot.R);
            Ignite = Player.GetSpellSlotFromName("SummonerDot");
        }


    }
}