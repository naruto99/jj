using EloBuddy.SDK;
using System.Linq;
using EloBuddy;

namespace Nechrito_Rengar.Classes
{
    class Killsteal
    {
       public static void _Killsteal()
        {
            if (Spells.W.IsReady())
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Spells.W.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Player.Instance.GetSpellDamage(target,SpellSlot.W))
                        Spells.W.Cast(target);
                }
            }
            if (Spells.E.IsReady())
            {
                var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Spells.E.Range) && !x.IsZombie);
                foreach (var target in targets)
                {
                    if (target.Health < Player.Instance.GetSpellDamage(target, SpellSlot.E))
                        Spells.E.Cast(target);
                }
            }
        }
    }
}
