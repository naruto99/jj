using EloBuddy;
using EloBuddy.SDK;
using System;
using System.Collections.Generic;
using SharpDX;
using System.Linq;
using EloBuddy.SDK.Enumerations;

namespace SephKhazix
{
    class Helper
    {
        internal static AIHeroClient Khazix = ObjectManager.Player;

        internal static KhazixMenu Config;

        internal Item Hydra, Tiamat, Blade, Bilgewater, Youmu, Titanic;

        internal Spell.Targeted Q, Ignite;
        internal Spell.Skillshot W, We, E;
        internal Spell.Active R;

        internal const float Wangle = 22 * (float) Math.PI / 180;

        internal static bool EvolvedQ, EvolvedW, EvolvedE;

        internal static List<AIHeroClient> HeroList;
        internal static List<Vector3> EnemyTurretPositions = new List<Vector3>();
        internal static Vector3 NexusPosition;
        internal static Vector3 Jumppoint1, Jumppoint2;
        internal static bool Jumping;

        internal void InitSkills()
        {
            
            Q = new Spell.Targeted(SpellSlot.Q,325);
            W = new Spell.Skillshot(SpellSlot.W,1000,SkillShotType.Linear,(int)0.225,(int)828.5,80);
            We = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, (int)0.225, (int)828.5, 100);
            E = new Spell.Skillshot(SpellSlot.E,700,SkillShotType.Circular,(int)0.25,1000,100);
            R = new Spell.Active(SpellSlot.R);
            var slot = Khazix.GetSpellSlotFromName("summonerdot");
            if (slot != SpellSlot.Unknown)
            {
                Ignite = new Spell.Targeted(slot, 600);
            }


            Hydra = new Item(3074,225f);
            Tiamat = new Item(3077, 225f);
            Blade = new Item(3153, 450f);
            Bilgewater = new Item(3144, 450f);
            Youmu = new Item(3142, 185f);
            Titanic = new Item(3748, 225f);

        }

        internal void EvolutionCheck()
        {
            if (!EvolvedQ && Khazix.HasBuff("khazixqevo"))
            {
                Q.Range = 375;
                EvolvedQ = true;
            }
            if (!EvolvedW && Khazix.HasBuff("khazixwevo"))
            {
                EvolvedW = true;
                W = new Spell.Skillshot(SpellSlot.W,1000,SkillShotType.Linear,(int)0.225,(int)828.5,100);
            }

            else if (!EvolvedE && Khazix.HasBuff("khazixeevo"))
            {
                E.Range = 1000;
                EvolvedE = true;
            }
        }

        internal void UseItems(Obj_AI_Base target)
        {
            if (Hydra.IsReady() && Khazix.Distance(target) <= Hydra.Range)
            {
                Hydra.Cast();
            }
            if (Tiamat.IsReady() && Khazix.Distance(target) <= Tiamat.Range)
            {
                Tiamat.Cast();
            }
            if (Titanic.IsReady() && Khazix.Distance(target) <= Tiamat.Range)
            {
                Tiamat.Cast();
            }
            if (Blade.IsReady() && Khazix.Distance(target) <= Blade.Range)
            {
                Blade.Cast(target);
            }
            if (Youmu.IsReady() && Khazix.Distance(target) <= Youmu.Range)
            {
                Youmu.Cast(target);
            }
            if (Bilgewater.IsReady() && Khazix.Distance(target) <= Bilgewater.Range)
            {
                Bilgewater.Cast(target);
            }
        }

        internal double GetQDamage(Obj_AI_Base target)
        {
            if (Q.Range < 326)
            {
                return 0.984 * Khazix.GetSpellDamage(target, SpellSlot.Q, target.IsIsolated() ? DamageLibrary.SpellStages.Empowered : DamageLibrary.SpellStages.Default);
            }
            if (Q.Range > 325)
            {
                var isolated = target.IsIsolated();
                if (isolated)
                {
                    return 0.984 * Khazix.GetSpellDamage(target, SpellSlot.Q,DamageLibrary.SpellStages.Empowered);
                }
                return Khazix.GetSpellDamage(target, SpellSlot.Q);
            }
            return 0;
        }

        internal List<AIHeroClient> GetIsolatedTargets()
        {
            var validtargets = HeroList.Where(h => h.IsValidTarget(E.Range) && h.IsIsolated()).ToList();
            return validtargets;
        }

        internal KhazixMenu GenerateMenu()
        {
            Config = new KhazixMenu();
            return Config;
        }

        internal bool IsHealthy
        {
            get
            {
                return Khazix.HealthPercent >= Config.GetSliderFloat("Safety.MinHealth");
            }
        }

        internal bool Override
        {
            get
            {
                return Config.GetKeyBind("Safety.Override");
            }
        }

        internal bool IsInvisible
        {
            get
            {
                return Khazix.HasBuff("khazixrstealth");
            }
        }

    }
}

