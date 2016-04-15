using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace ElRengar_Revamped
{
    internal class Program
    {
        public static Menu Menu;
        private static readonly int[] BlueSmite = {3706, 1400, 1401, 1402, 1403};
        private static readonly int[] RedSmite = {3715, 1415, 1414, 1413, 1412};

        private static readonly Spell.Active Q = new Spell.Active(SpellSlot.Q,
            (uint) (EloBuddy.Player.Instance.GetAutoAttackRange() + 100));

        private static readonly Spell.Active W = new Spell.Active(SpellSlot.W,
            (uint) (500 + EloBuddy.Player.Instance.BoundingRadius));

        private static readonly Spell.Skillshot E = new Spell.Skillshot(SpellSlot.E,
            1000 + (uint) (EloBuddy.Player.Instance.BoundingRadius), SkillShotType.Linear, (int) 0.25, 1500, 70);

        private static readonly Spell.Active R = new Spell.Active(SpellSlot.R, 2000);
        public static int LastSwitch;
        protected static SpellSlot Ignite;
        protected static SpellSlot Smite;
        public static int LastAutoAttack, Lastrengarq;
        public static int LastQ, LastE, LastW, LastSpell;
        public static Obj_AI_Base SelectedEnemy;
        public static Item Youmuuu = new Item(ItemId.Youmuus_Ghostblade);

        /// <summary>
        ///     Gets Youmuus Ghostblade
        /// </summary>
        /// <value>
        ///     Youmuus Ghostblade
        /// </value>
        public static Item Youmuu = new Item(ItemId.Youmuus_Ghostblade);

        /// <summary>
        ///     Gets Ravenous Hydra
        /// </summary>
        /// <value>
        ///     Ravenous Hydra
        /// </value>
        public static Item Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);

        /// <summary>
        ///     Gets Tiamat Item
        /// </summary>
        /// <value>
        ///     Tiamat Item
        /// </value>
        public static Item Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);

        /// <summary>
        ///     Gets Titanic Hydra
        /// </summary>
        /// <value>
        ///     Titanic Hydra
        /// </value>
        public static Item Titanic = new Item(ItemId.Titanic_Hydra);

        public static int Ferocity
        {
            get { return (int) ObjectManager.Player.Mana; }
        }

        public static bool HasPassive
        {
            get { return EloBuddy.Player.Instance.Buffs.Any(x => x.Name.ToLower().Contains("rengarpassivebuff")); }
        }

        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool RengarR
        {
            get { return Player.Buffs.Any(x => x.Name.ToLower().Contains("RengarR")); }
        }

        private static IEnumerable<AIHeroClient> Enemies
        {
            get { return EntityManager.Heroes.Enemies; }
        }

        private static int ComboPriority
        {
            get { return Menu["Combo.Prio"].Cast<Slider>().CurrentValue; }
        }

        private static bool AutoHpActive
        {
            get { return Menu["Heal.AutoHeal"].Cast<CheckBox>().CurrentValue; }
        }

        private static int AutoHpValue
        {
            get { return Menu["Heal.HP"].Cast<Slider>().CurrentValue; }
        }

        private static bool KillStealOn
        {
            get { return Menu["Killsteal.On"].Cast<CheckBox>().CurrentValue; }
        }

        private static bool BetaCastQOn
        {
            get { return Menu["Beta.Cast.Q"].Cast<CheckBox>().CurrentValue; }
        }

        public static float IgniteDamage(AIHeroClient target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite);
        }

        public static bool IsActive(string menuItem)
        {
            return Menu[menuItem].Cast<CheckBox>().CurrentValue;
        }

        protected static void SmiteCombo()
        {
            if (BlueSmite.Any(id => Item.HasItem(id)))
            {
                Smite = Player.GetSpellSlotFromName("s5_summonersmiteplayerganker");
                return;
            }

            if (RedSmite.Any(id => Item.HasItem(id)))
            {
                Smite = Player.GetSpellSlotFromName("s5_summonersmiteduel");
                return;
            }

            Smite = Player.GetSpellSlotFromName("summonersmite");
        }

        public static void OnClick(WndEventArgs args)
        {
            if (args.Msg != (uint) WindowMessages.LeftButtonDown)
            {
                return;
            }
            var unit2 =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(
                        a =>
                            (a.IsValid()) && !(a.IsMinion || a.IsMonster || a is Obj_AI_Turret) && a.IsEnemy &&
                            a.Distance(Game.CursorPos) < a.BoundingRadius + 80
                            && a.IsValidTarget());
            if (unit2 != null)
            {
                SelectedEnemy = unit2;
            }
        }

        private static void AddBool(string name, string displayName, bool value = true)
        {
            Menu.Add(name, new CheckBox(displayName, value));
        }

        private static void Main()
        {
            Loading.OnLoadingComplete += OnLoad;
        }

        public static void OnLoad(EventArgs args)
        {
            if (Player.ChampionName != "Rengar")
            {
                return;
            }

            try
            {
                Ignite = Player.GetSpellSlotFromName("summonerdot");
                Chat.Print(
                    "[00:01] <font color='#f9eb0b'>HEEEEEEY!</font> ElRengar Revamped Loaded");
                Chat.Print(
                    "[00:01] <font color='#f9eb0b'>ElRengar Revamped</font> Ported by Rexy");

                Menu = MainMenu.AddMenu("ElRengar", "ElRengar");
                Menu.AddLabel("Ported by Rexy");

                Menu.AddSeparator();

                Menu.AddLabel("Modes");
                Menu.AddLabel("Summoner spells");
                AddBool("Combo.Use.Ignite", "Use Ignite");
                AddBool("Combo.Use.Smite", "Use Smite");

                Menu.AddSeparator();

                Menu.AddLabel("Combo");
                AddBool("Combo.Use.Q", "Use Q");
                AddBool("Combo.Use.W", "Use W");
                AddBool("Combo.Use.E", "Use E");
                AddBool("Combo.Switch.E", "Switch E prio to Q after E cast");
                AddBool("Combo.Use.E.OutOfRange", "Use E when out of range");

                Menu.AddSeparator();

                Menu.AddLabel("0 => E || 1 => W || 2 => Q");
                Menu.Add("Combo.Prio", new Slider("Combo Priority", 2, 0, 2));
                Menu.Add("Combo.Switch", new KeyBind("Switch Priority", false, KeyBind.BindTypes.HoldActive, 'T'));
                Menu.Add("Combo.TripleQ", new KeyBind("Triple Q", false, KeyBind.BindTypes.HoldActive, 'A'));

                Menu.AddSeparator();

                Menu.AddLabel("Clear");
                AddBool("Clear.Use.Q", "Use Q");
                AddBool("Clear.Use.W", "Use W");
                AddBool("Clear.Use.E", "Use E");
                AddBool("Clear.Save.Ferocity", "Save LaneClear Ferocity");

                Menu.AddSeparator();

                AddBool("Jungle.Use.Q", "Use Q");
                AddBool("Jungle.Use.W", "Use W");
                AddBool("Jungle.Use.E", "Use E");
                AddBool("Jungle.Save.Ferocity", "Save LaneClear Ferocity");

                Menu.AddSeparator();

                Menu.AddLabel("Heal");
                AddBool("Heal.AutoHeal", "Auto heal your self");
                Menu.Add("Heal.HP", new Slider("Self heal at >= ", 25));

                Menu.AddSeparator();

                Menu.AddLabel("Killsteal");
                AddBool("Killsteal.On", "Active");
                AddBool("Killsteal.Use.W", "Use W");

                Menu.AddSeparator();

                Menu.AddLabel("Beta options");
                AddBool("Beta.Cast.Q", "Use beta q");
                AddBool("Beta.Cast.Youmuu", "Use Youmuu");
                Menu.Add("Beta.Cast.Q.Delay", new Slider("Cast Q delay", 500, 100, 2000));
                Menu.Add("Beta.searchrange", new Slider("Search range", 2000, 1000, 2500));
                Menu.Add("Beta.searchrange.Q", new Slider("Q cast range", 1000, 500, 1500));

                Menu.AddSeparator();

                Menu.AddLabel("Misc");
                AddBool("Misc.Drawings.Off", "Turn Drawings off", false);
                AddBool("Misc.Drawings.Exclamation", "Draw exlamation mark range");
                AddBool("Misc.Drawings.Prioritized", "Draw Prioritiy");
                AddBool("Misc.Drawings.W", "Draw W Range");
                AddBool("Misc.Drawings.E", "Draw E Range");
                AddBool("Misc.Mastery", "Mastery badge on kill");

                Game.OnUpdate += OnUpdate;
                Drawing.OnDraw += OnDraw;
                Dash.OnDash += OnDash;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Orbwalker.OnPostAttack += AfterAttack;
                Orbwalker.OnPreAttack += BeforeAttack;
                Game.OnWndProc += OnClick;
                Game.OnNotify += OnNotify;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnNotify(GameNotifyEventArgs args)
        {
            if (!IsActive("Misc.Mastery"))
            {
                return;
            }

            if (args.EventId == GameEventId.OnChampionKill)
            {
                Chat.Say("/masterybadge");
            }
        }

        private static void BeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            try
            {
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo && !HasPassive && Q.IsReady()
                    && !(ComboPriority == 0
                         || ComboPriority == 1 && Ferocity == 5))
                {
                    var x = Prediction.Position.PredictUnitPosition(args.Target as Obj_AI_Base,
                        (int) (Player.AttackCastDelay + 0.04));
                    if (Player.Position.To2D().Distance(x)
                        >= Player.BoundingRadius + Player.AttackRange + args.Target.BoundingRadius)
                    {
                        args.Process = false;
                        Q.Cast();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void AfterAttack(AttackableUnit target, EventArgs args)
        {
            try
            {
                var enemy = target as Obj_AI_Base;
                if (!target.IsMe || enemy == null || !(target is AIHeroClient))
                {
                    return;
                }

                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.Cast();
                    }
                }
                CastItems(enemy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Heal()
        {
            try
            {
                if (Player.IsRecalling() || Player.IsInShopRange() || Ferocity <= 4 || RengarR)
                {
                    return;
                }

                if (AutoHpActive
                    && (Player.Health/Player.MaxHealth)*100
                    <= AutoHpValue && W.IsReady())
                {
                    W.Cast();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void KillstealHandler()
        {
            try
            {
                if (!KillStealOn)
                {
                    return;
                }

                var target =
                    Enemies.FirstOrDefault(
                        x => x.IsValidTarget(W.Range) && x.Health < Player.GetSpellDamage(x, SpellSlot.W));

                if (target != null)
                {
                    W.Cast();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe)
                {
                    switch (args.SData.Name.ToLower())
                    {
                        case "RengarR":
                            if (Item.CanUseItem(3142))
                            {
                                Core.DelayAction(() => Item.UseItem(3142), 2000);
                            }
                            break;

                        case "RengarQ":
                            LastQ = Environment.TickCount;
                            break;

                        case "RengarE":
                            LastE = Environment.TickCount;
                            break;

                        case "RengarW":
                            LastW = Environment.TickCount;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            try
            {
                if (!sender.IsMe)
                {
                    return;
                }

                var target = TargetSelector.GetTarget(1500f, DamageType.Physical);
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                {
                    if (Ferocity == 5)
                    {
                        switch (ComboPriority)
                        {
                            case 0:
                                if (E.IsReady() && target.IsValidTarget(E.Range))
                                {
                                    var pred = E.GetPrediction(target);
                                    E.Cast(pred.CastPosition);
                                }
                                break;
                            case 2:
                                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    Q.Cast();
                                }

                                if (target.IsValidTarget(Q.Range))
                                {
                                    Core.DelayAction(() =>
                                    {
                                        if (target.IsValidTarget(W.Range))
                                        {
                                            W.Cast();
                                        }

                                        E.Cast(target);
                                        CastItems(target);
                                    }, 50);
                                }

                                break;
                        }
                    }
                }

                switch (ComboPriority)
                {
                    case 0:
                        if (E.IsReady() && target.IsValidTarget(E.Range))
                        {
                            var pred = E.GetPrediction(target);
                            E.Cast(pred.CastPosition);
                        }
                        break;

                    case 2:
                        if (BetaCastQOn && RengarR)
                        {
                            Q.Cast();
                        }
                        break;
                }

                if (e.Duration - 100 - Game.Ping/2 > 0)
                {
                    Core.DelayAction(() => { CastItems(target); }, e.Duration - 100 - Game.Ping/2);
                }
            }

            catch (Exception es)
            {
                Console.WriteLine(es);
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead)
                {
                    return;
                }

                switch (Orbwalker.ActiveModesFlags)
                {
                    case Orbwalker.ActiveModes.Combo:
                        Combo();
                        break;

                    case Orbwalker.ActiveModes.LaneClear:
                        Laneclear();
                        break;

                    case Orbwalker.ActiveModes.JungleClear:
                        Jungleclear();
                        break;
                }


                SwitchCombo();
                SmiteCombo();
                Heal();
                KillstealHandler();

                if (Menu["Combo.TripleQ"].Cast<KeyBind>().CurrentValue)
                {
                    Orbwalker.OrbwalkTo(Game.CursorPos);

                    var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                    if (!target.IsValidTarget())
                    {
                        return;
                    }

                    Orbwalker.OrbwalkTo(Game.CursorPos);

                    if (RengarR)
                    {
                        if (Ferocity == 5 && Player.Distance(target) <= Q.Range)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        Q.Cast();
                    }

                    if (Ferocity <= 4)
                    {
                        if (Player.Distance(target) <= Q.Range)
                        {
                            Q.Cast();
                        }
                        if (Player.Distance(target) <= W.Range)
                        {
                            W.Cast();
                        }
                        if (Player.Distance(target) <= E.Range)
                        {
                            E.Cast(target);
                        }
                    }
                }

                if (BetaCastQOn && ComboPriority == 2)
                {
                    if (IsActive("Beta.Cast.Youmuu") && !Item.HasItem(3142))
                    {
                        return;
                    }

                    var searchrange = Menu["Beta.searchrange"].Cast<Slider>().CurrentValue;
                    var target =
                        ObjectManager.Get<AIHeroClient>()
                            .FirstOrDefault(h => h.IsEnemy && h.IsValidTarget(searchrange));
                    if (!target.IsValidTarget())
                    {
                        return;
                    }

                    if (Ferocity == 5 && RengarR)
                    {
                        if (target.Distance(Player.ServerPosition)
                            <= Menu["Beta.searchrange.Q"].Cast<Slider>().CurrentValue)
                        {
                            Core.DelayAction(() => Q.Cast(), Menu["Beta.Cast.Q.Delay"].Cast<Slider>().CurrentValue);
                        }
                    }
                }

                R.Range = (uint) (1000 + R.Level*1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SwitchCombo()
        {
            try
            {
                var switchTime = Core.GameTickCount - LastSwitch;
                if (Menu["Combo.Switch"].Cast<KeyBind>().CurrentValue && switchTime >= 350)
                {
                    switch (ComboPriority)
                    {
                        case 0:
                            Menu["Combo.Switch"].Cast<Slider>().CurrentValue = 2;
                            LastSwitch = Core.GameTickCount;
                            break;
                        case 1:
                            Menu["Combo.Switch"].Cast<Slider>().CurrentValue = 0;
                            LastSwitch = Core.GameTickCount;
                            break;

                        default:
                            Menu["Combo.Switch"].Cast<Slider>().CurrentValue = 0;
                            LastSwitch = Core.GameTickCount;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnDraw(EventArgs args)
        {
            try
            {
                var drawW = Menu["Misc.Drawings.W"].Cast<CheckBox>().CurrentValue;
                var drawE = Menu["Misc.Drawings.E"].Cast<CheckBox>().CurrentValue;
                var drawExclamation = Menu["Misc.Drawings.Exclamation"].Cast<CheckBox>().CurrentValue;
                //Exclamation mark

                var drawSearchRange = Menu["Beta.Search.Range"].Cast<CheckBox>().CurrentValue;
                var searchrange = Menu["Beta.searchrange"].Cast<Slider>().CurrentValue;

                var drawsearchrangeQ = Menu["Beta.Search.QCastRange"].Cast<CheckBox>().CurrentValue;
                var searchrangeQCastRange = Menu["Beta.searchrange.Q"].Cast<Slider>().CurrentValue;

                if (SelectedEnemy.IsValidTarget() && SelectedEnemy.IsVisible && !SelectedEnemy.IsDead)
                {
                    Drawing.DrawText(
                        Drawing.WorldToScreen(SelectedEnemy.Position).X - 40,
                        Drawing.WorldToScreen(SelectedEnemy.Position).Y + 10,
                        Color.White,
                        "Selected Target");
                }

                if (IsActive("Misc.Drawings.Off"))
                {
                    return;
                }

                if (IsActive("Beta.Cast.Q"))
                {
                    if (drawSearchRange && R.Level > 0)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, searchrange, Color.Orange);
                    }

                    if (drawsearchrangeQ && R.Level > 0)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, searchrangeQCastRange, Color.Orange);
                    }
                }

                if (RengarR && drawExclamation)
                {
                    if (R.Level > 0)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, 1450f, Color.DeepSkyBlue);
                    }
                }

                if (drawW)
                {
                    if (W.Level > 0)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, Color.Purple);
                    }
                }

                if (drawE)
                {
                    if (E.Level > 0)
                    {
                        Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, Color.White);
                    }
                }

                if (Menu["Misc.Drawings.Prioritized"].Cast<CheckBox>().CurrentValue)
                {
                    switch (ComboPriority)
                    {
                        case 0:
                            Drawing.DrawText(
                                Drawing.Width*0.70f,
                                Drawing.Height*0.95f,
                                Color.Yellow,
                                "Prioritized spell: E");
                            break;
                        case 1:
                            Drawing.DrawText(
                                Drawing.Width*0.70f,
                                Drawing.Height*0.95f,
                                Color.White,
                                "Prioritized spell: W");
                            break;
                        case 2:
                            Drawing.DrawText(
                                Drawing.Width*0.70f,
                                Drawing.Height*0.95f,
                                Color.White,
                                "Prioritized spell: Q");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Combo()
        {
            try
            {
                var target = TargetSelector.SelectedTarget ?? TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target.IsValidTarget() == false)
                {
                    return;
                }

                if (Ferocity <= 4)
                {
                    if (Q.IsReady() && IsActive("Combo.Use.Q")
                        && Player.Distance(target) <= Q.Range)
                    {
                        Q.Cast();
                    }

                    if (!RengarR)
                    {
                        CastItems(target);

                        if (!HasPassive)
                        {
                            if (E.IsReady() && IsActive("Combo.Use.E"))
                            {
                                CastE(target);
                            }
                        }
                        else
                        {
                            if (E.IsReady() && IsActive("Combo.Use.E"))
                            {
                                if (!Player.IsDashing())
                                {
                                    return;
                                }

                                CastE(target);
                            }
                        }
                    }

                    if (W.IsReady() && IsActive("Combo.Use.W"))
                    {
                        CastW();
                    }
                }

                if (Ferocity == 5)
                {
                    switch (ComboPriority)
                    {
                        case 0:
                            if (!RengarR)
                            {
                                if (E.IsReady() && !HasPassive)
                                {
                                    CastE(target);

                                    if (IsActive("Combo.Switch.E") && Environment.TickCount - LastE >= 500
                                        && Core.GameTickCount - LastSwitch >= 350)
                                    {
                                        Menu["Combo.Prio"].Cast<Slider>().CurrentValue = 2;
                                        LastSwitch = Core.GameTickCount;
                                    }
                                }
                            }
                            else
                            {
                                if (E.IsReady() && IsActive("Combo.Use.E"))
                                {
                                    if (!Player.IsDashing())
                                    {
                                        return;
                                    }

                                    CastE(target);
                                }
                            }
                            break;
                        case 1:
                            if (IsActive("Combo.Use.W") && W.IsReady())
                            {
                                CastW();
                            }
                            break;
                        case 2:
                            if (Q.IsReady() && IsActive("Combo.Use.Q") &&
                                Player.CountEnemiesInRange(Player.AttackRange + Player.BoundingRadius + 100) != 0)
                            {
                                Q.Cast();
                            }
                            break;
                    }

                    if (!RengarR)
                    {
                        if (IsActive("Combo.Use.E.OutOfRange"))
                        {
                            CastE(target);
                        }
                    }
                }

                #region Summoner spells

                if (Youmuuu.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Youmuuu.Cast();
                }

                if (IsActive("Combo.Use.Smite") && !RengarR && Smite != SpellSlot.Unknown
                    && Player.Spellbook.CanUseSpell(Smite) == SpellState.Ready && !target.IsZombie)
                {
                    Player.Spellbook.CastSpell(Smite, target);
                }

                if (IsActive("Combo.Use.Ignite") && target.IsValidTarget(600f) && IgniteDamage(target) >= target.Health)
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                }

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Jungleclear()
        {
            try
            {
                var minion =
                    EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.ServerPosition, W.Range)
                        .OrderByDescending(x => x.MaxHealth)
                        .LastOrDefault(x => x != null);

                if (minion == null)
                {
                    return;
                }

                CastItems(minion);

                if (Ferocity == 5 && IsActive("Jungle.Save.Ferocity"))
                {
                    if (minion.IsValidTarget(W.Range) && !HasPassive)
                    {
                        CastItems(minion);
                    }
                    return;
                }

                if (IsActive("Jungle.Use.Q") && Q.IsReady()
                    && minion.IsValidTarget(Q.Range + 100))
                {
                    Q.Cast();
                }

                if (IsActive("Jungle.Use.W") && W.IsReady()
                    && minion.IsValidTarget(W.Range) && !HasPassive)
                {
                    W.Cast();
                }

                if (IsActive("Jungle.Use.E") && E.IsReady()
                    && minion.IsValidTarget(E.Range))
                {
                    E.Cast(minion.Position);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Laneclear()
        {
            try
            {
                var minion =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsValidTarget(W.Range))
                        .OrderByDescending(x => x.Health)
                        .LastOrDefault(x => x != null);
                if (minion == null)
                {
                    return;
                }

                if (Player.Spellbook.IsAutoAttacking)
                {
                    return;
                }
                if (Ferocity == 5 && IsActive("Clear.Save.Ferocity"))
                {
                    if (minion.IsValidTarget(W.Range))
                    {
                        CastItems(minion);
                    }
                    return;
                }

                if (IsActive("Clear.Use.Q") && Q.IsReady()
                    && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (IsActive("Clear.Use.W") && W.IsReady()
                    && minion.IsValidTarget(W.Range))
                {
                    CastItems(minion);
                    W.Cast();
                }

                if (IsActive("Clear.Use.E") && Player.GetSpellDamage(minion, SpellSlot.E) > minion.Health
                    && E.IsReady() && minion.IsValidTarget(E.Range))
                {
                    E.Cast(minion.Position);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool CastItems(Obj_AI_Base target)
        {
            if (Player.IsDashing() || RengarR)
            {
                return false;
            }

            var units =
                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                    EntityManager.UnitTeam.Enemy, Player.ServerPosition, 385)
                    .Count(o => o.IsMinion || o.IsMonster);
            var heroes = Player.CountEnemiesInRange(385);
            var count = units + heroes;

            var tiamat = Tiamat;
            if (tiamat.IsReady() && count > 0 && tiamat.Cast())
            {
                return true;
            }

            var hydra = Hydra;
            if (Hydra.IsReady() && count > 0 && hydra.Cast())
            {
                return true;
            }

            var youmuus = Youmuu;
            if (Youmuu.IsReady() && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo
                && youmuus.Cast())
            {
                return true;
            }

            var titanic = Titanic;
            return titanic.IsReady() && count > 0 && titanic.Cast();
        }

        #region Methods

        private static void CastE(Obj_AI_Base target)
        {
            try
            {
                if (!E.IsReady() || !target.IsValidTarget(E.Range))
                {
                    return;
                }

                var prediction = E.GetPrediction(target);
                if (prediction.HitChance >= HitChance.High)
                {
                    E.Cast(prediction.CastPosition);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void CastW()
        {
            try
            {
                if (!W.IsReady() || Environment.TickCount - LastE <= 200)
                {
                    return;
                }

                if (GetWHits().Item1 > 0)
                {
                    W.Cast();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static Tuple<int, List<AIHeroClient>> GetWHits()
        {
            try
            {
                var hits =
                    EntityManager.Heroes.Enemies.Where(
                        e =>
                            e.IsValidTarget() && e.Distance(Player) < 450f
                            || e.Distance(Player) < 450f && e.IsFacing(Player)).ToList();

                return new Tuple<int, List<AIHeroClient>>(hits.Count, hits);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new Tuple<int, List<AIHeroClient>>(0, null);
        }

        #endregion
    }
}