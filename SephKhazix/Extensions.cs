﻿using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace SephKhazix
{
    static class Extensions
    {
        internal static AIHeroClient Player = Helper.Khazix;

        internal static bool IsIsolated(this Obj_AI_Base target)
        {
            return !ObjectManager.Get<Obj_AI_Base>().Any(x => x.NetworkId != target.NetworkId && x.Team == target.Team && x.Distance(target) <= 500 && (x.Type == GameObjectType.AIHeroClient || x.Type == GameObjectType.obj_AI_Minion || x.Type == GameObjectType.obj_AI_Turret));
        }

        internal static bool IsValidMinion(this Obj_AI_Minion minion)
        {
            return (minion != null && minion.IsValid && minion.IsVisible && minion.Team != Player.Team && minion.IsHPBarRendered && !minion.CharData.BaseSkinName.ToLower().Contains("ward"));
        }

        internal static bool IsValidAlly(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || unit.Distance(Player) > range || unit.Team != Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValidEnemy(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || !unit.IsHPBarRendered || unit.IsZombie || unit.Distance(Player) > range || unit.Team == Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector2.Distance(unit.ServerPosition.To2D(), Helper.Khazix.ServerPosition.To2D()) <= range;
            }
            return false;
        }

        internal static bool PointUnderEnemyTurret(this Vector2 point)
        {
            var enemyTurrets =
                EntityManager.Turrets.Enemies.Find(t => t.IsEnemy && Vector2.Distance(t.Position.To2D(),point) < 950f);
            return enemyTurrets != null;
        }

        internal static bool PointUnderEnemyTurret(this Vector3 point)
        {
            var enemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Where(t => t.IsEnemy && Vector3.Distance(t.Position, point) < 900f + Helper.Khazix.BoundingRadius);
            return enemyTurrets.Any();
        }

        internal static bool CanKill(this Obj_AI_Base @base, SpellSlot slot, DamageLibrary.SpellStages stage = DamageLibrary.SpellStages.Default)
        {
            return Player.GetSpellDamage(@base, slot, stage) >= @base.Health;
        }

        internal static bool IsCloserWp(this Vector2 point, Obj_AI_Base target)
        {
            var wp = target.Path;
            var lastwp = wp.LastOrDefault();
            var wpc = wp.Count();
            var midwpnum = wpc / 2;
            var midwp = wp[midwpnum];
            var plength = wp[0].Distance(lastwp);
            return (point.Distance(target.ServerPosition, true) <= Player.Distance(target.ServerPosition, true)) || ((plength <= Player.Distance(target.ServerPosition) * 1.2f && point.Distance(lastwp) < Player.Distance(lastwp) || point.Distance(midwp) < Player.Distance(midwp)));
        }

        internal static bool IsCloser(this Vector2 point, Obj_AI_Base target)
        {
            if (Khazix.Config.GetBool("Combo.EAdvanced"))
            {
                return IsCloserWp(point, target);
            }
            return (point.Distance(target.ServerPosition, true) <= Player.Distance(target.ServerPosition, true));
        }


        internal static Vector3 Wts(this Vector3 vect)
        {
            return Drawing.WorldToScreen(vect).To3D();
        }


        //Menu Extensions


        internal static void AddBool(this Menu menu, string name, string displayname, bool @defaultvalue = true)
        {
             menu.Add(name, new CheckBox(displayname, @defaultvalue));
        }

        internal static void AddKeyBind(this Menu menu, string name, string displayname, uint key, KeyBind.BindTypes type)
        {
            menu.Add(name, new KeyBind(displayname, false, type, key));
        }

        internal static void AddSlider(this Menu menu, string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            menu.Add(name, new Slider(displayname, initial, min, max));
        }

        internal static bool IsTargetValid(this AttackableUnit unit,
        float range = float.MaxValue,
        bool checkTeam = true,
        Vector3 from = new Vector3())
        {
            if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable ||
                unit.IsInvulnerable)
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;
            if (@base != null)
            {
                if (@base.HasBuff("kindredrnodeathbuff") && @base.HealthPercent <= 10)
                {
                    return false;
                }
            }

            if (checkTeam && unit.Team == ObjectManager.Player.Team)
            {
                return false;
            }

            var unitPosition = @base != null ? @base.ServerPosition : unit.Position;

            return !(range < float.MaxValue) ||
                   !(Vector2.DistanceSquared(
                       (@from.To2D().IsValid() ? @from : ObjectManager.Player.ServerPosition).To2D(),
                       unitPosition.To2D()) > range * range);
        }

        /*
        internal static bool WCanKill(this Obj_AI_Base minion, bool isQ2 = false)
        {
            var dmg = Player.GetSpellDamage(minion, SpellSlot.W) / 1.3
                            >= HealthPrediction.GetHealthPrediction(
                                minion,
                                (int)(Player.Distance(minion) / Helper.W.Speed) * 1000,
                                (int)Helper.W.Delay * 1000);
            return dmg;
        }
        */


        internal static bool IsBlackListed(this AIHeroClient unit)
        {
            return !Khazix.Config.GetBool("ult" + unit.ChampionName);
        }

        internal static int MinionsInRange(this Obj_AI_Base unit, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Count(x => x.Distance(unit) <= range && x.NetworkId != unit.NetworkId && x.Team == unit.Team);
            return minions;
        }

        internal static int MinionsInRange(this Vector2 pos, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Count(x => x.Distance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }

        internal static int MinionsInRange(this Vector3 pos, float range)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Count(x => x.Distance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }
    }
}
