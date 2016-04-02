using EloBuddy;
using EloBuddy.SDK;

namespace Nechrito_Rengar.Classes.Modes
{
    class Combo : Logic
    {
        public static void ComboLogic()
        {
            var target = TargetSelector.GetTarget(Spells.E.Range - 80, DamageType.Physical);
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                if ((int)Player.Mana == 5)
                {
                    if (Spells.E.IsReady())
                    { Spells.E.Cast(target);}
                       CastYoumoo();
                    if (Smite != SpellSlot.Unknown
                    && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                    {
                        Player.Spellbook.CastSpell(Smite, target);
                    }
                    if (Spells.Q.IsReady() && !Spells.E.IsReady() && (Player.Distance(target.Position) <= Player.AttackRange + 30) && (target.IsValidTarget() && !target.IsZombie))
                    { Spells.Q.Cast();}
                    if (Spells.W.IsReady() && (Player.Distance(target.Position) <= Player.AttackRange + 30) && (target.IsValidTarget() && !target.IsZombie))
                    {
                        CastHydra();
                        Spells.W.Cast();
                    }       
                }
                else if ((int)Player.Mana <= 4)
                {
                    if (Spells.Q.IsReady() && (Player.Distance(target.Position) <= Player.AttackRange) && (target.IsValidTarget() && !target.IsZombie))
                    {Spells.Q.Cast();}
                    if (Spells.W.IsReady() && (Player.Distance(target.Position) <= Player.AttackRange + 30) && (target.IsValidTarget() && !target.IsZombie))
                    {
                        CastHydra();
                        Spells.W.Cast();
                    }
                    if (Spells.E.IsReady() && (target.IsValidTarget() && !target.IsZombie))
                    { Spells.E.Cast(target);}
                }
            }
        }
    }
}
