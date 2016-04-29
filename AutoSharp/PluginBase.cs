using System;
using System.Drawing;
using AutoSharp.Utils;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core.UI.IMenu;
using Spell = LeagueSharp.Common.Spell;

namespace AutoSharp
{

    #region

    #endregion

    /// <summary>
    ///     PluginBase class
    /// </summary>
    public abstract class PluginBase
    {
        /// <summary>
        ///     Init BaseClass
        /// </summary>
        protected PluginBase()
        {
            InitPluginEvents();
        }

        /// <summary>
        ///     Champion Name
        /// </summary>
        public string ChampionName { get; set; }
        
        /// <summary>
        ///     ActiveMode
        /// </summary>
        public static Orbwalker.ActiveModes ActiveMode { get; set; }
        
        /// <summary>
        ///     ComboMode
        /// </summary>
        public bool ComboMode
        {
            get { return true; }
        }

        /// <summary>
        ///     HarassMode
        /// </summary>
        public bool HarassMode
        {
            get { return false; }
        }

        /// <summary>
        ///     HarassMana
        /// </summary>
        public bool HarassMana
        {
            get { return Player.ManaPercent > ConfigSLValue("HarassMana").Cast<Slider>().CurrentValue; }
        }

        /// <summary>
        ///     UsePackets
        /// </summary>
        public bool UsePackets
        {
            get { return false; /* 4.21 ConfigValue<bool>("UsePackets"); */ }
        }

        /// <summary>
        ///     Player Object
        /// </summary>
        public AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        /// <summary>
        ///     AttackRange
        /// </summary>
        public float AttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Target); }
        }

        /// <summary>
        ///     Target
        /// </summary>
        public AIHeroClient Target
        {
            get { return TargetSelector.GetTarget(1200, DamageType.Magical); }
        }

        /// <summary>
        ///     OrbwalkerTarget
        /// </summary>
        public AttackableUnit OrbwalkerTarget
        {
            get { return TargetSelector.GetTarget(Player.GetAutoAttackRange(),DamageType.Physical); }
        }

        /// <summary>
        ///     Q
        /// </summary>
        public Spell Q { get; set; }

        /// <summary>
        ///     W
        /// </summary>
        public Spell W { get; set; }

        /// <summary>
        ///     E
        /// </summary>
        public Spell E { get; set; }

        /// <summary>
        ///     R
        /// </summary>
        public Spell R { get; set; }

        /// <summary>
        ///     Config
        /// </summary>
        public static EloBuddy.SDK.Menu.Menu Config { get; set; }

        /// <summary>
        ///     ComboConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu ComboConfig { get; set; }

        /// <summary>
        ///     HarassConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu HarassConfig { get; set; }

        /// <summary>
        ///     MiscConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu MiscConfig { get; set; }

        /// <summary>
        ///     ManaConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu ManaConfig { get; set; }

        /// <summary>
        ///     DrawingConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu DrawingConfig { get; set; }

        /// <summary>
        ///     InterruptConfig
        /// </summary>
        public EloBuddy.SDK.Menu.Menu InterruptConfig { get; set; }

        /// <summary>
        ///     ConfigValue
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="item">string</param>
        /// <remarks>
        ///     Helper for
        /// </remarks>
        /// <returns></returns>
        public ValueBase ConfigCBValue(string item)
        {
            return Config["autosharp." + ObjectManager.Player.ChampionName + "." + item].Cast<CheckBox>();
        }

        public ValueBase ConfigSLValue(string item)
        {
            return Config["autosharp." + ObjectManager.Player.ChampionName + "." + item].Cast<Slider>();
        }

        /// <summary>
        ///     OnProcessPacket
        /// </summary>
        /// <remarks>
        ///     override to Implement OnProcessPacket logic
        /// </remarks>
        /// <param name="args"></param>
        public virtual void OnProcessPacket(GamePacketEventArgs args) { }

        /// <summary>
        ///     OnSendPacket
        /// </summary>
        /// <remarks>
        ///     override to Implement OnSendPacket logic
        /// </remarks>
        /// <param name="args"></param>
        public virtual void OnSendPacket(GamePacketEventArgs args) { }

        /// <summary>
        ///     OnPossibleToInterrupt
        /// </summary>
        /// <remarks>
        ///     override to Implement SpellsInterrupt logic
        /// </remarks>
        /// <param name="unit">Obj_AI_Base</param>
        /// <param name="spell">InterruptableSpell</param>
        public virtual void OnPossibleToInterrupt(AIHeroClient unit, Interrupter2.InterruptableTargetEventArgs spell) { }

        /// <summary>
        ///     OnEnemyGapcloser
        /// </summary>
        /// <remarks>
        ///     override to Implement AntiGapcloser logic
        /// </remarks>
        /// <param name="gapcloser">ActiveGapcloser</param>
        public virtual void OnEnemyGapcloser(ActiveGapcloser gapcloser) { }

        /// <summary>
        ///     OnUpdate
        /// </summary>
        /// <remarks>
        ///     override to Implement Update logic
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnUpdate(EventArgs args) { }

        /// <summary>
        ///     OnBeforeAttack
        /// </summary>
        /// <remarks>
        ///     override to Implement OnBeforeAttack logic
        /// </remarks>
        /// <param name="args">Orbwalking.BeforeAttackEventArgs</param>
        public virtual void OnBeforeAttack(Obj_AI_Base a,Orbwalker.PreAttackArgs args) { }

        /// <summary>
        ///     OnAfterAttack
        /// </summary>
        /// <remarks>
        ///     override to Implement OnAfterAttack logic
        /// </remarks>
        /// <param name="unit">unit</param>
        /// <param name="target">target</param>
        public virtual void OnAfterAttack(AttackableUnit unit, AttackableUnit target) { }

        /// <summary>
        ///     OnLoad
        /// </summary>
        /// <remarks>
        ///     override to Implement class Initialization
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnLoad(EventArgs args) { }

        /// <summary>
        ///     OnDraw
        /// </summary>
        /// <remarks>
        ///     override to Implement Drawing
        /// </remarks>
        /// <param name="args">EventArgs</param>
        public virtual void OnDraw(EventArgs args) { }

        /// <summary>
        ///     ComboMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement ComboMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void ComboMenu(Menu config) { }

        /// <summary>
        ///     HarassMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement HarassMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void HarassMenu(Menu config) { }

        /// <summary>
        ///     ManaMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement ManaMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void ManaMenu(Menu config) { }

        /// <summary>
        ///     MiscMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement MiscMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void MiscMenu(Menu config) { }

        /// <summary>
        ///     MiscMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement Interrupt Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void InterruptMenu(Menu config) { }

        /// <summary>
        ///     DrawingMenu
        /// </summary>
        /// <remarks>
        ///     override to Implement DrawingMenu Config
        /// </remarks>
        /// <param name="config">Menu</param>
        public virtual void DrawingMenu(Menu config) { }

        #region Private Stuff

        /// <summary>
        ///     PluginEvents Initialization
        /// </summary>
        private void InitPluginEvents()
        {
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += OnPossibleToInterrupt;
            //Game.OnGameSendPacket += OnSendPacket;
            //Game.OnGameProcessPacket += OnProcessPacket;
            OnLoad(new EventArgs());
        }
        #endregion
    }
}