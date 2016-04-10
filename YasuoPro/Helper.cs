using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace YasuoPro
{
    internal class Helper
    {
        internal const float LaneClearWaitTimeMod = 2f;

        internal static AIHeroClient Yasuo
        {
            get { return Player.Instance; }
        }
        public static Obj_AI_Base ETarget;
        internal static Obj_Shop shop = ObjectManager.Get<Obj_Shop>().FirstOrDefault(x => x.IsAlly);
        internal static bool DontDash = false;
        internal static ItemManager.Item Hydra, Tiamat, Blade, Bilgewater, Youmu;
        /* Credits to Brian for Q Skillshot values */

        public static Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 500, SkillShotType.Linear, GetQ1Delay,
            int.MaxValue, 20);

        public static Spell.Skillshot Q2 = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, GetQ2Delay, 1500,
            90);

        public static Spell.Active W = new Spell.Active(SpellSlot.W, 450);
        public static Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 475);
        public static Spell.Active R = new Spell.Active(SpellSlot.R, 1250);
        public static Spell.Targeted Ignite;
        internal static Vector2 DashPosition;

        internal string[] DangerousSpell =
        {
            "syndrar", "veigarprimordialburst", "dazzle", "leblancchaosorb",
            "judicatorreckoning", "iceblast", "disintegrate"
        };

        public static SpellSlot IgniteSlot
        {
            get { return Yasuo.GetSpellSlotFromName("summonerdot"); }
        }

        private static int GetQDelay
        {
            get { return (int) (1 - Math.Min((Yasuo.AttackSpeedMod - 1)*0.0058552631578947, 0.6675)); }
        }

        private static int GetQ1Delay
        {
            get { return (int) (0.4*GetQDelay); }
        }

        private static int GetQ2Delay
        {
            get { return (int) (0.5*GetQDelay); }
        }

        internal int Qrange
        {
            get { return TornadoReady ? (int)(Q2.Range) : (int)(Q.Range); }
        }

        internal int Qdelay
        {
            get { return (int)(0.250 - (Math.Min(BonusAttackSpeed, 0.66) * 0.250)); }
        }

        internal int BonusAttackSpeed
        {
            get { return (int)((1/Player.Instance.AttackDelay) - 0.658); }
        }

        internal int Erange
        {
            get { return (int)E.Range; }
        }

        internal int Rrange
        {
            get { return (int)R.Range; }
        }

        internal bool TornadoReady
        {
            get { return Yasuo.HasBuff("yasuoq3w"); }
        }

        internal static int DashCount
        {
            get
            {
                var bc = Yasuo.GetBuffCount("yasuodashscalar");
                return bc;
            }
        }

        internal IEnumerable<AIHeroClient> KnockedUp
        {
            get
            {
                var KnockedUpEnemies = new List<AIHeroClient>();
                foreach (var hero in EntityManager.Heroes.Enemies)
                {
                    if (hero.IsValidEnemy(R.Range))
                    {
                        var knockup =
                            hero.Buffs.Find(
                                x =>
                                    (x.Type == BuffType.Knockup &&
                                     (x.EndTime - Game.Time) <=
                                     (GetSliderFloat("Combo.knockupremainingpct")/100)*(x.EndTime - x.StartTime)) ||
                                    x.Type == BuffType.Knockback);
                        if (knockup != null)
                        {
                            KnockedUpEnemies.Add(hero);
                        }
                    }
                }
                return KnockedUpEnemies;
            }
        }

        internal static bool isHealthy
        {
            get
            {
                return Yasuo.IsInvulnerable || Yasuo.HasBuffOfType(BuffType.Invulnerability) ||
                       Yasuo.HasBuffOfType(BuffType.SpellShield) || Yasuo.HasBuffOfType(BuffType.SpellImmunity) ||
                       Yasuo.HealthPercent > GetSliderFloat("Misc.Healthy") ||
                       Yasuo.HasBuff("yasuopassivemovementshield") && Yasuo.HealthPercent > 30;
            }
        }

        internal static bool Debug
        {
            get { return GetBool("Misc.Debug"); }
        }

        internal FleeType FleeMode
        {
            get
            {
                var GetFM = GetSliderInt("Flee.Mode");
                if (GetFM == 0)
                {
                    return FleeType.ToNexus;
                }
                if (GetFM == 1)
                {
                    return FleeType.ToAllies;
                }
                return FleeType.ToCursor;
            }
        }

        internal void InitSpells()
        {
            if (IgniteSlot != SpellSlot.Unknown)
            {
                Ignite = new Spell.Targeted(IgniteSlot, 600);
            }
        }

        internal bool UseQ(AIHeroClient target, HitChance minhc = HitChance.Medium, bool UseQ1 = true, bool UseQ2 = true)
        {
            if (target == null)
            {
                return false;
            }

            var tready = TornadoReady;

            if ((tready && !UseQ2) || !tready && !UseQ1)
            {
                return false;
            }

            if (tready && Yasuo.IsDashing())
            {
                if (GetBool("Combo.NoQ2Dash") || ETarget == null ||
                    !(ETarget is AIHeroClient) && ETarget.CountEnemiesInRange(120) < 1)
                {
                    return false;
                }
            }

            var sp = tready ? Q2 : Q;
            var pred = sp.GetPrediction(target);

            if (pred.HitChance >= minhc)
            {
                return sp.Cast(pred.CastPosition);
            }

            return false;
        }

        internal static bool GetBool(string name)
        {
            return YasuoMenu.Config[name].Cast<CheckBox>().CurrentValue;
        }

        internal static bool GetKeyBind(string name)
        {
            return YasuoMenu.Config[name].Cast<KeyBind>().CurrentValue;
        }

        internal static int GetSliderInt(string name)
        {
            return YasuoMenu.Config[name].Cast<Slider>().CurrentValue;
        }

        internal static float GetSliderFloat(string name)
        {
            return YasuoMenu.Config[name].Cast<Slider>().CurrentValue;
        }

        internal static Vector2 GetDashPos(Obj_AI_Base @base)
        {
            var predictedposition = Yasuo.ServerPosition.Extend(@base.Position,
                Yasuo.Distance(@base) + 475 - Yasuo.Distance(@base));
            DashPosition = predictedposition;
            return predictedposition;
        }

        internal static double GetProperEDamage(Obj_AI_Base target)
        {
            double dmg = Yasuo.GetSpellDamage(target, SpellSlot.E);
            float amplifier = 0;
            if (DashCount == 0)
            {
                amplifier = 0;
            }
            else if (DashCount == 1)
            {
                amplifier = 0.25f;
            }
            else if (DashCount == 2)
            {
                amplifier = 0.50f;
            }
            dmg += dmg*amplifier;
            return dmg;
        }

        internal static HitChance GetHitChance(string search)
        {
            var hitchance = YasuoMenu.Config[search].Cast<Slider>().CurrentValue;
            switch (hitchance)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.Dashing;
            }
            return HitChance.Medium;
        }

        internal UltMode GetUltMode()
        {
            switch (GetSliderInt("Combo.UltMode"))
            {
                case 0:
                    return UltMode.Health;
                case 1:
                    return UltMode.Priority;
                case 2:
                    return UltMode.EnemiesHit;
            }
            return UltMode.Priority;
        }

        internal void InitItems()
        {
            Hydra = new ItemManager.Item(3074, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Tiamat = new ItemManager.Item(3077, 225f, ItemManager.ItemCastType.RangeCast, 1, 2);
            Blade = new ItemManager.Item(3153, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Bilgewater = new ItemManager.Item(3144, 450f, ItemManager.ItemCastType.TargettedCast, 1);
            Youmu = new ItemManager.Item(3142, 185f, ItemManager.ItemCastType.SelfCast, 1, 3);
        }

        internal enum FleeType
        {
            ToNexus,
            ToAllies,
            ToCursor
        }

        internal enum UltMode
        {
            Health,
            Priority,
            EnemiesHit
        }
    }
}