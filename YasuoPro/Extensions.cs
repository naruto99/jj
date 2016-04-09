using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using Evade;
using LeagueSharp.Common;
using SharpDX;
using KeyBind = EloBuddy.SDK.Menu.Values.KeyBind;
using Slider = EloBuddy.SDK.Menu.Values.Slider;

namespace YasuoPro
{
    internal static class Extensions
    {
        internal static AIHeroClient Player = Helper.Yasuo;

        internal static bool IsDashable(this Obj_AI_Base unit, float range = 475)
        {
            if (unit == null || unit.Team == Player.Team || unit.Distance(Player) > range || !unit.IsValid ||
                unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }

            if (Helper.GetBool("Misc.SafeE"))
            {
                var point = Helper.GetDashPos(unit);
                if (!Program.IsSafe(point).IsSafe)
                {
                    return false;
                }
            }

            var minion = unit as Obj_AI_Minion;
            return !unit.HasBuff("YasuoDashWrapper") && (unit is AIHeroClient || minion.IsValidMinion());
        }

        internal static bool IsValidMinion(this Obj_AI_Minion minion, float range = 50000)
        {
            var name = minion.CharData.BaseSkinName.ToLower();
            return (minion != null && Player.Distance(minion) <= range && minion.IsValid && minion.IsVisible &&
                    minion.Team != Player.Team && minion.IsHPBarRendered && !name.Contains("ward") &&
                    !name.Contains("gangplankbarrel"));
        }

        internal static bool IsValidAlly(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || unit.Distance(Player) > range || unit.Team != Player.Team || !unit.IsValid ||
                unit.IsDead || !unit.IsVisible || unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValidEnemy(this Obj_AI_Base unit, float range = 50000)
        {
            if (unit == null || !unit.IsHPBarRendered || unit.IsZombie || unit.Distance(Player) > range ||
                unit.Team == Player.Team || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable)
            {
                return false;
            }
            return true;
        }

        internal static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector2.Distance(unit.ServerPosition.To2D(), Helper.Yasuo.ServerPosition.To2D()) <= range;
            }
            return false;
        }

        internal static bool PointUnderEnemyTurret(this Vector2 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Find(t => t.IsEnemy && Vector2.Distance(t.Position.To2D(), Point) < 950f);
            return EnemyTurrets != null;
        }

        internal static bool PointUnderEnemyTurret(this Vector3 Point)
        {
            var EnemyTurrets =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(t => t.IsEnemy && Vector3.Distance(t.Position, Point) < 900f + Helper.Yasuo.BoundingRadius);
            return EnemyTurrets.Any();
        }

        internal static bool CanKill(this Obj_AI_Base @base, SpellSlot slot)
        {
            if (slot == SpellSlot.E)
            {
                return Helper.GetProperEDamage(@base) >= @base.Health;
            }
            return DamageLibrary.GetSpellDamage(Player, @base, slot) >= @base.Health;
        }

        internal static bool IsCloserWP(this Vector2 point, Obj_AI_Base target)
        {
            var wp = target.GetWaypoints();
            var lastwp = wp.LastOrDefault();
            var wpc = wp.Count();
            var midwpnum = wpc/2;
            var midwp = wp[midwpnum];
            var plength = wp[0].Distance(lastwp);
            return (point.Distance(target.ServerPosition, true) <= Player.Distance(target.ServerPosition, true)) ||
                   ((plength <= Player.Distance(target.ServerPosition)*1.2f &&
                     point.Distance(lastwp.To3D()) < Player.Distance(lastwp.To3D()) ||
                     point.Distance(midwp.To3D()) < Player.Distance(midwp)));
        }

        internal static bool IsCloser(this Vector2 point, Obj_AI_Base target)
        {
            if (Helper.GetBool("Combo.EAdvanced"))
            {
                return IsCloserWP(point, target);
            }
            return (point.Distance(target.ServerPosition, true) <= Player.Distance(target.ServerPosition, true));
        }

        internal static bool IsCloser(this Obj_AI_Base @base, Obj_AI_Base target)
        {
            return Helper.GetDashPos(@base).Distance(target.ServerPosition) < Player.Distance(target.ServerPosition);
        }

        internal static Vector3 WTS(this Vector3 vect)
        {
            return EloBuddy.SDK.Extensions.To3D(Drawing.WorldToScreen(vect));
        }

        //Menu Extensions

        internal static void AddBool(string name, string displayname, bool @defaultvalue = true)
        {
            YasuoMenu.Config.Add(name, new CheckBox(displayname, @defaultvalue));
        }

        internal static void AddKeyBind(string name, string displayname, uint key, KeyBind.BindTypes type)
        {
            YasuoMenu.Config.Add(name, new KeyBind(displayname, false, type, key));
        }

        internal static void AddSlider(string name, string displayname, int initial = 0, int min = 0, int max = 100)
        {
            YasuoMenu.Config.Add(name, new Slider(displayname, initial, min, max));
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
                       (EloBuddy.SDK.Extensions.IsValid(@from.To2D()) ? @from : ObjectManager.Player.ServerPosition)
                           .To2D(),
                       unitPosition.To2D()) > range*range);
        }

        internal static bool QCanKill(this Obj_AI_Base minion, bool isQ2 = false)
        {
            //var hpred =
            //  HealthPrediction.GetHealthPrediction(minion, 0, 500 + Game.Ping / 2);
            // return hpred < 0.95 * Player.GetSpellDamage(minion, SpellSlot.Q) && hpred > 0;
            var qspell = isQ2 ? Helper.Q2 : Helper.Q;
            var dmg = DamageLibrary.GetSpellDamage(Player, minion, SpellSlot.Q)/1.3
                      >= HealthPrediction.GetHealthPrediction(
                          minion,
                          (int) (Player.Distance(minion)/qspell.Speed)*1000,
                          qspell.CastDelay*1000);
            return dmg;
        }

        internal static bool ECanKill(this Obj_AI_Base minion)
        {
            var espell = Helper.E;
            return Helper.GetProperEDamage(minion)/1.2
                   >= HealthPrediction.GetHealthPrediction(
                       minion,
                       (int) (Player.Distance(minion)/espell.Handle.SData.MissileSpeed)*1000,
                       espell.CastDelay*1000);
        }

        internal static bool isBlackListed(this AIHeroClient unit)
        {
            return !Helper.GetBool("ult" + unit.ChampionName);
        }

        internal static int MinionsInRange(this Obj_AI_Base unit, float range)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(x => x.Distance(unit) <= range && x.NetworkId != unit.NetworkId && x.Team == unit.Team);
            return minions;
        }

        internal static int MinionsInRange(this Vector2 pos, float range)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(x => x.Distance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }

        internal static int MinionsInRange(this Vector3 pos, float range)
        {
            var minions =
                ObjectManager.Get<Obj_AI_Minion>()
                    .Count(x => x.Distance(pos) <= range && (x.IsEnemy || x.Team == GameObjectTeam.Neutral));
            return minions;
        }
    }
}