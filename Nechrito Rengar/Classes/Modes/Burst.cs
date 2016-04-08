using EloBuddy;
using EloBuddy.SDK;

namespace Nechrito_Rengar.Classes.Modes
{
    class Burst : Logic
    {
        public static void BurstLogic()
        {
            var target = TargetSelector.SelectedTarget;
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                if ((int)Player.Mana == 5 && (Player.Distance(target.Position) <= 900f))
                {
                    if (Spells.Q.IsReady())
                    {
                        Spells.Q.Cast();
                        Spells.E.Cast(target);
                    }
                    CastYoumoo();
                    if (Smite != SpellSlot.Unknown
                   && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                        Player.Spellbook.CastSpell(Smite, target);
                    if (Spells.W.IsReady())
                    {
                        CastHydra();
                        Spells.W.Cast();
                    }
                    else if ((int)Player.Mana <= 4)
                    {
                        if (Spells.E.IsReady())
                            Spells.E.Cast(target);
                        if (Spells.Q.IsReady())
                            Spells.Q.Cast();
                            CastYoumoo();
                        if (Smite != SpellSlot.Unknown
                       && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                        {
                            Player.Spellbook.CastSpell(Smite, target);
                        }
                        if (Spells.W.IsReady())
                        {
                            CastHydra();
                            Spells.W.Cast();
                        }
                    }
                }
            }
        }
    }
}
