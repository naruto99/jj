using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace BrianSharp.Evade
{
    public enum CollisionObjectTypes
    {
        Minion,

        Champion,

        YasuoWall
    }

    internal class FastPredResult
    {
        #region Fields

        public Vector2 CurrentPos;

        public bool IsMoving;

        public Vector2 PredictedPos;

        #endregion
    }

    internal class DetectedCollision
    {
        #region Fields

        public float Diff;

        public float Distance;

        public Vector2 Position;

        public CollisionObjectTypes Type;

        public Obj_AI_Base Unit;

        #endregion
    }

    internal static class Collisions
    {
        #region Static Fields

        private static Vector2 wallCastedPos;

        private static int wallCastT;

        #endregion

        #region Public Methods and Operators

        public static Vector2 GetCollisionPoint(this Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);
            skillshot.ForceDisabled = false;
            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:
                        collisions.AddRange(
                            from minion in
                                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both,
                                    EntityManager.UnitTeam.Enemy, @from.To3D(), 1200)
                            let pred =
                                FastPrediction(
                                    @from,
                                    minion,
                                    Math.Max(
                                        0,
                                        skillshot.SpellData.Delay - (Core.GameTickCount - skillshot.StartTick)),
                                    skillshot.SpellData.MissileSpeed)
                            let pos = pred.PredictedPos
                            let w =
                                skillshot.SpellData.RawRadius + (!pred.IsMoving ? minion.BoundingRadius - 15 : 0)
                                - pos.Distance(@from, skillshot.End, true)
                            where w > 0
                            select
                                new DetectedCollision
                                {
                                    Position =
                                        pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint
                                        + skillshot.Direction*30,
                                    Unit = minion,
                                    Type = CollisionObjectTypes.Minion,
                                    Distance = pos.Distance(@from),
                                    Diff = w
                                });
                        break;
                    case CollisionObjectTypes.Champion:
                        collisions.AddRange(
                            from hero in EntityManager.Heroes.Allies.Where(i => i.IsValidTarget(1200) && !i.IsMe)
                            let pred =
                                FastPrediction(
                                    @from,
                                    hero,
                                    Math.Max(
                                        0,
                                        skillshot.SpellData.Delay - (Core.GameTickCount - skillshot.StartTick)),
                                    skillshot.SpellData.MissileSpeed)
                            let pos = pred.PredictedPos
                            let w = skillshot.SpellData.RawRadius + 30 - pos.Distance(@from, skillshot.End, true)
                            where w > 0
                            select
                                new DetectedCollision
                                {
                                    Position =
                                        pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint
                                        + skillshot.Direction*30,
                                    Unit = hero,
                                    Type = CollisionObjectTypes.Minion,
                                    Distance = pos.Distance(@from),
                                    Diff = w
                                });
                        break;
                    case CollisionObjectTypes.YasuoWall:
                        if (
                            !EntityManager.Heroes.Allies.Any(
                                i => i.IsValidTarget(float.MaxValue) && i.ChampionName == "Yasuo"))
                        {
                            break;
                        }
                        var wall =
                            ObjectManager.Get<GameObject>()
                                .FirstOrDefault(
                                    i =>
                                        i.IsValid &&
                                        Regex.IsMatch(i.Name, "_w_windwall.\\.troy", RegexOptions.IgnoreCase));
                        if (wall == null)
                        {
                            break;
                        }
                        var wallWidth = 300 + 50*Convert.ToInt32(wall.Name.Substring(wall.Name.Length - 6, 1));
                        var wallDirection = (wall.Position.To2D() - wallCastedPos).Normalized().Perpendicular();
                        var wallStart = wall.Position.To2D() + wallWidth/2f*wallDirection;
                        var wallEnd = wallStart - wallWidth*wallDirection;
                        var wallPolygon = new Geometry.Polygon.Rectangle(wallStart, wallEnd, 75);
                        var intersections = new List<Vector2>();
                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                wallPolygon.Points[i].Intersection(
                                    wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0],
                                    from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }
                        if (intersections.Count > 0)
                        {
                            var intersection = intersections.OrderBy(i => i.Distance(@from)).ToList()[0];
                            var collisionT = Core.GameTickCount
                                             + Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay
                                                 - (Core.GameTickCount - skillshot.StartTick)) + 100
                                             + (1000*intersection.Distance(from))/skillshot.SpellData.MissileSpeed;
                            if (collisionT - wallCastT < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }
                                return intersection;
                            }
                        }
                        break;
                }
            }
            return collisions.Count > 0 ? collisions.OrderBy(i => i.Distance).ToList()[0].Position : new Vector2();
        }

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        #endregion

        #region Methods

        public static List<Vector2> CutPath(this List<Vector2> path, Obj_AI_Base unit, float delay, float speed = 0)
        {
            if (speed == 0)
            {
                speed = unit.MoveSpeed;
            }

            var dist = speed*delay/1000;
            return CutPath(path, dist);
        }

        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;

            if (path.Count > 0)
            {
                result.Add(path.First());
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance*(path[i + 1] - path[i]).Normalized());
                    break;
                }
                result.Add(path[i + 1]);
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> {path.Last()};
        }

        private static FastPredResult FastPrediction(Vector2 from, Obj_AI_Base unit, int delay, int speed)
        {
            var d = (delay/1000f + (from.Distance(unit)/speed))*unit.MoveSpeed;
            var path = unit.Path;
            var pathTo2D = path.PathToVector2();
            return path.Length > d
                ? new FastPredResult
                {
                    IsMoving = true,
                    CurrentPos = unit.ServerPosition.To2D(),
                    PredictedPos = pathTo2D.CutPath((int) d)[0]
                }
                : (path.Count() == 0
                    ? new FastPredResult
                    {
                        IsMoving = false,
                        CurrentPos = unit.ServerPosition.To2D(),
                        PredictedPos = unit.ServerPosition.To2D()
                    }
                    : new FastPredResult
                    {
                        IsMoving = false,
                        CurrentPos = path[path.Count() - 1].To2D(),
                        PredictedPos = path[path.Count() - 1].To2D()
                    });
        }

        public static List<Vector2> CutPPath(List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;

            if (path.Count > 0)
            {
                result.Add(path.First());
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance*(path[i + 1] - path[i]).Normalized());
                    break;
                }
                result.Add(path[i + 1]);
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> {path.Last()};
        }

        public static List<Vector2> PathToVector2(this Vector3[] path)
        {
            return path.Select(p => p.To2D()).ToList();
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Team != ObjectManager.Player.Team || args.SData.Name != "YasuoWMovingWall")
            {
                return;
            }
            wallCastT = Core.GameTickCount;
            wallCastedPos = sender.ServerPosition.To2D();
        }

        #endregion
    }
}