using BrianSharp.Common;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;

namespace BrianSharp.Evade
{
    public enum CastTypes
    {
        Position,

        Target,

        Self
    }

    public enum SpellTargets
    {
        AllyMinions,

        EnemyMinions,

        AllyWards,

        EnemyWards,

        AllyChampions,

        EnemyChampions
    }

    public enum EvadeTypes
    {
        Blink,

        Dash,

        Invulnerability,

        MovementSpeedBuff,

        Shield,

        SpellShield,

        WindWall
    }

    internal class EvadeSpellData
    {
        #region Fields

        public CastTypes CastType;

        public string CheckSpellName = "";

        public int Delay;

        public EvadeTypes EvadeType;

        public bool FixedRange;

        public float MaxRange;

        public string Name;

        public SpellSlot Slot;

        public int Speed;

        public SpellTargets[] ValidTargets;

        private int dangerLevel;

        #endregion

        #region Public Properties

        public int DangerLevel
        {
            get
            {
                return Helper.GetItem("ESSS_" + Name, "DangerLevel") != null
                    ? Program.Menu["ESSS_" + Name].Cast<Slider>().CurrentValue
                    : dangerLevel;
            }
            set { dangerLevel = value; }
        }

        public bool Enabled
        {
            get
            {
                return Program.Menu["ESSS_" + Name].Cast<CheckBox>().CurrentValue;
                //Helper.GetValue<bool>("ESSS_" + this.Name, "Enabled");
            }
        }

        public bool IsReady
        {
            get
            {
                return (CheckSpellName == ""
                        || ObjectManager.Player.Spellbook.GetSpell(Slot).Name == CheckSpellName)
                       && Player.Instance.Spellbook.GetSpell(Slot).IsReady;
            }
        }

        #endregion
    }
}