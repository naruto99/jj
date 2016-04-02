using EloBuddy.SDK;
using EloBuddy;

namespace Nechrito_Rengar.Classes.Modes
{
    class ApCombo : Logic
    {
        public static void ApComboLogic()
        {
            var target = TargetSelector.GetTarget(Spells.E.Range - 80, DamageType.Physical);
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                if ((int)Player.Mana == 5)
                {
                    if (Spells.Q.IsReady())
                    {   Spells.Q.Cast();}
                    if (Spells.W.IsReady())
                    {   Spells.W.Cast();}
                    if (Smite != SpellSlot.Unknown
                   && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                    {
                        Player.Spellbook.CastSpell(Smite, target);
                    }
                    if (Spells.W.IsReady())
                    {   Spells.W.Cast();}
                    if (Spells.E.IsReady())
                    { Spells.E.Cast(target);}
                }
                else if ((int)Player.Mana <= 4)
                {
                    if (Spells.Q.IsReady())
                    { Spells.Q.Cast();}
                    if (Smite != SpellSlot.Unknown
                   && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                    {
                        Player.Spellbook.CastSpell(Smite, target);
                    }
                    if (Spells.W.IsReady())
                    {Spells.W.Cast();}
                    if (Spells.W.IsReady())
                    { Spells.W.Cast();}
                    if (Spells.E.IsReady())
                    { Spells.E.Cast(target);}
                }
            }
        }

    }
}
