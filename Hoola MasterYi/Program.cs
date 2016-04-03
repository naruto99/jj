using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Events;
using Color = System.Drawing.Color;
using MainMenu = EloBuddy.SDK.Menu.MainMenu;
using Menu = EloBuddy.SDK.Menu.Menu;


namespace HoolaMasterYi
{
    public class Program
    {
        private static Menu _menu;
        private static readonly AIHeroClient Playerrr = ObjectManager.Player;
        private static readonly Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q,600);
        private static readonly Spell.Active W = new Spell.Active(SpellSlot.W);
        private static readonly Spell.Active E = new Spell.Active(SpellSlot.E);
        private static readonly Spell.Active R = new Spell.Active(SpellSlot.R);
        

        private static bool Dq
        {
            get { return _menu["DQ"].Cast<EloBuddy.SDK.Menu.Values.CheckBox>().CurrentValue; }
        }
        
        static void Main()
        {
            Loading.OnLoadingComplete += OnGameLoad;
        }

        static void OnGameLoad(EventArgs args)
        {
            Chat.Print("Hoola Master Yi - Loaded Successfully, Good Luck! :)");
            Chat.Print("Ported by Rexy");
            OnMenuLoad();

            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += DetectSpell;
            Obj_AI_Base.OnSpellCast += OnDoCast;
            Obj_AI_Base.OnPlayAnimation += OnPlay;
            Spellbook.OnCastSpell += OnCast;
            Obj_AI_Base.OnSpellCast += OnDoCastJc;
            Obj_AI_Base.OnSpellCast += BeforeAttack;
            Obj_AI_Base.OnSpellCast += DetectBlink;
            Drawing.OnDraw += OnDraw;
        }

        private static void DetectBlink(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsMe || args.SData.IsAutoAttack()) return;

                if (Spelldatabase.List.Contains(args.SData.Name.ToLower()) &&
                    (((Playerrr.Distance(args.End) >= Q.Range))))
                    Q.Cast((Obj_AI_Base)args.Target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private static void DetectSpell(EventArgs args)
        {
            try
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target.IsDashing() && target.IsValidTarget(Q.Range) && (((Playerrr.Distance(target.Path.Last()) >= Q.Range))) && Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    Q.Cast(target);
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
                if (!Dq)
                {
                    return;
                }
                if (Dq && Q.IsReady())
                {
                    Drawing.DrawCircle(Playerrr.Position, Q.Range, Color.LimeGreen);
                }
                else if (Dq && !Q.IsReady())
                {
                    Drawing.DrawCircle(Playerrr.Position, Q.Range, Color.IndianRed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        static void BeforeAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (!sender.IsMe || !args.Target.IsValid || !Orbwalker.IsAutoAttacking) return;

                var target = (AIHeroClient) args.Target;

                if (target.IsValidTarget(400f))
                {
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                    {
                        R.Cast();
                        CastYoumoo();
                        E.Cast();
                    }
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
                    {
                        CastYoumoo();
                        E.Cast();
                    }
                }
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
                    {
                        foreach (Obj_AI_Minion minion in EntityManager.MinionsAndMonsters.GetLaneMinions())
                        {
                            if (minion.IsValidTarget(400f))
                            {
                                E.Cast();
                            }
                        }
                    }
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
                    {
                        foreach (Obj_AI_Minion mobb in EntityManager.MinionsAndMonsters.GetJungleMonsters())
                        {
                            if (mobb.IsValidTarget(400f))
                            {
                                E.Cast();
                            }
                        }
                    }
                }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        private static void OnCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            try
            {
                if (args.Slot == SpellSlot.R) CastYoumoo();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
        }

        private static void OnPlay(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            try
            {
                if (!sender.IsMe) return;
                if (args.Animation.Contains("Spell2"))
                {
                    Orbwalker.ResetAutoAttack();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
        }

        static void UseCastItem(int t)
        {
            try
            {
                for (var i = 0; i < t; i = i + 1)
                {
                    if (HasItem)
                        Core.DelayAction(CastItem, i);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        static void CastItem()
        {
            try
            {
                if (Item.CanUseItem(ItemId.Tiamat_Melee_Only))
                    Item.UseItem(ItemId.Tiamat_Melee_Only);
                if (Item.CanUseItem(ItemId.Ravenous_Hydra_Melee_Only))
                    Item.UseItem(ItemId.Ravenous_Hydra_Melee_Only);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        static void CastYoumoo()
        {
            try
            {
                if (Item.CanUseItem(ItemId.Youmuus_Ghostblade))
                    Item.UseItem(ItemId.Youmuus_Ghostblade);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        static void CastBotrk(AIHeroClient target)
        {
            try
            {
                if (Item.CanUseItem(ItemId.Blade_of_the_Ruined_King))
                    Item.UseItem(ItemId.Blade_of_the_Ruined_King, target);
                if (Item.CanUseItem(ItemId.Bilgewater_Cutlass))
                    Item.UseItem(ItemId.Bilgewater_Cutlass, target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        static bool HasItem
        {
            get
            {
                return Item.CanUseItem(ItemId.Tiamat_Melee_Only) || Item.CanUseItem(ItemId.Ravenous_Hydra_Melee_Only);
            }
        }


        private static void OnDoCast(Obj_AI_Base sender,  GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (!sender.IsMe || !args.Target.IsValid && !Orbwalker.IsAutoAttacking) return;

                if (args.Target != null && args.Target.IsValid)
                {
                    var target = (AIHeroClient)args.Target;
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                    {
                        CastBotrk(target);
                        UseCastItem(300);
                        Core.DelayAction(() => W.Cast(), 1);
                    }
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
                    {
                        CastBotrk(target);
                        UseCastItem(300);
                        Core.DelayAction(() => W.Cast(), 1);
                    }
                }
                if (args.Target is Obj_AI_Minion && args.Target.IsValid)
                {
                    var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                        Playerrr.ServerPosition, 400f);//MinionManager.GetMinions(ItemData.Ravenous_Hydra_Melee_Only.Range);
                    if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
                    {
                        if (minions != null)
                        {
                            UseCastItem(300);
                            Core.DelayAction(() => W.Cast(), 1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

        }
        private static void OnDoCastJc(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            try
            {
                if (!sender.IsMe || !args.Target.IsValid && !Orbwalker.IsAutoAttacking) return;

                if (args.Target is Obj_AI_Minion && args.Target.IsValid)
                {
                    foreach (Obj_AI_Minion mmobs in EntityManager.MinionsAndMonsters.GetJungleMonsters())
                    {
                        if (mmobs.IsValidTarget(400f))
                        {
                            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
                            {
                                if (Q.IsReady()) Q.Cast(mmobs);
                                if (!Q.IsReady())
                                {
                                    UseCastItem(300);
                                    Core.DelayAction(() => W.Cast(), 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

        }
        private static void OnMenuLoad()
        {
            try
            {
                _menu = MainMenu.AddMenu("Hoola Master Yi", "hoolamasteryi");
                _menu.AddLabel("Hoola Master Yi ported by Rexy");
                _menu.AddSeparator();
                _menu.Add("DQ", new EloBuddy.SDK.Menu.Values.CheckBox("Draw Q"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

        }

        static void Killsteal()
        {
            try
            {
                if (Q.IsReady())
                {
                    var targets = EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsZombie);
                    foreach (var target in targets)
                    {
                        if (target.IsValidTarget(Q.Range) && target.Health < Playerrr.GetSpellDamage(target, SpellSlot.Q) && (!target.HasBuff("kindrednodeathbuff") || !target.HasBuff("Undying Rage") || !target.HasBuff("JudicatorIntervention")) && (!target.IsValidTarget(Playerrr.GetAutoAttackRange()) || !Orbwalker.CanAutoAttack))
                            Q.Cast(target);
                    }
                }
                if (
                    (Item.CanUseItem(ItemId.Bilgewater_Cutlass) ||
                     Item.CanUseItem(ItemId.Blade_of_the_Ruined_King)))
                {
                    var targets =
                        EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(550) && !x.IsZombie);
                    foreach (var target in targets)
                    {
                        if (target.Health < Playerrr.GetItemDamage(target, ItemId.Bilgewater_Cutlass)) Item.UseItem(ItemId.Bilgewater_Cutlass, target);
                        if (target.Health < Playerrr.GetItemDamage(target, ItemId.Blade_of_the_Ruined_King)) Item.UseItem(ItemId.Blade_of_the_Ruined_King, target);
                    }
                }
                if (
                    (Item.CanUseItem(ItemId.Tiamat_Melee_Only) ||
                     Item.CanUseItem(ItemId.Ravenous_Hydra_Melee_Only)))
                {
                    var targets =
                        EntityManager.Heroes.Enemies.Where(
                            x => x.IsValidTarget(400f) && !x.IsZombie);
                    foreach (var target in targets)
                    {
                        if (target.Health < Playerrr.GetItemDamage(target, ItemId.Tiamat_Melee_Only)) Item.UseItem(ItemId.Tiamat_Melee_Only, target);
                        if (target.Health < Playerrr.GetItemDamage(target, ItemId.Ravenous_Hydra_Melee_Only)) Item.UseItem(ItemId.Ravenous_Hydra_Melee_Only, target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                Killsteal();
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo) Combo();
                if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass) Harass();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private static void Combo()
        {
            try
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (Q.IsReady() && target.IsValidTarget(Q.Range)) Q.Cast(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        static void Harass()
        {
            try
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (Q.IsReady() && target.IsValidTarget(Q.Range)) Q.Cast(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
    }
}