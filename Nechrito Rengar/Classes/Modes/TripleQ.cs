using EloBuddy;
using EloBuddy.SDK;

namespace Nechrito_Rengar.Classes.Modes
{
    internal class TripleQ : Logic
    {
        public static void TripleQLogic()
        {
            var target = TargetSelector.GetTarget(375f + Player.AttackRange + 70, DamageType.Physical);
            if (target != null && target.IsValidTarget() && !target.IsZombie)
            {
                if ((int) Player.Mana == 5)
                {
                    if (Spells.Q.IsReady())
                    {
                        Spells.Q.Cast(target);
                        CastHydra();
                    }

                    if (Player.Mana <= 4)
                    {
                        if (Spells.Q.IsReady())
                            Spells.Q.Cast();
                        if (Spells.W.IsReady())
                        {
                            CastHydra();
                            Spells.W.Cast();
                        }
                        if (Spells.E.IsReady())
                            Spells.E.Cast(target);
                    }
                }
            }
        }
    }
}
