﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace BrianSharp.Evade
{
    public enum SkillShotType
    {
        SkillshotCircle,

        SkillshotLine,

        SkillshotMissileLine,

        SkillshotCone,

        SkillshotMissileCone,

        SkillshotRing,

        SkillshotArc
    }

    public enum DetectionType
    {
        RecvPacket,

        ProcessSpell
    }

    public struct SafePathResult
    {
        #region Constructors and Destructors

        public SafePathResult(bool isSafe, FoundIntersection intersection)
        {
            IsSafe = isSafe;
            Intersection = intersection;
        }

        #endregion

        #region Fields

        public FoundIntersection Intersection;

        public bool IsSafe;

        #endregion
    }

    public struct FoundIntersection
    {
        #region Constructors and Destructors

        public FoundIntersection(float distance, int time, Vector2 point, Vector2 comingFrom)
        {
            Distance = distance;
            ComingFrom = comingFrom;
            Valid = point.IsValid();
            Point = point + Configs.GridSize*(ComingFrom - point).Normalized();
            Time = time;
        }

        #endregion

        #region Fields

        public Vector2 ComingFrom;

        public float Distance;

        public Vector2 Point;

        public int Time;

        public bool Valid;

        #endregion
    }

    public class Skillshot
    {
        #region Constructors and Destructors

        public Skillshot(
            DetectionType detectionType,
            SpellData spellData,
            int startT,
            Vector2 start,
            Vector2 end,
            Obj_AI_Base unit)
        {
            DetectionType = detectionType;
            SpellData = spellData;
            StartTick = startT;
            Start = start;
            End = end;
            Direction = (end - start).Normalized();
            Unit = unit;
            switch (spellData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    Circle = new Geometry.Polygon.Circle(CollisionEnd, spellData.Radius, 22);
                    break;
                case SkillShotType.SkillshotLine:
                case SkillShotType.SkillshotMissileLine:
                    Rectangle = new Geometry.Polygon.Rectangle(Start, CollisionEnd, spellData.Radius);
                    break;
                case SkillShotType.SkillshotCone:
                    Sector = new Geometry.Polygon.Sector(
                        start,
                        CollisionEnd - start,
                        spellData.Radius*(float) Math.PI/180,
                        spellData.Range,
                        22);
                    break;
                case SkillShotType.SkillshotRing:
                    Ring = new Geometry.Polygon.Ring(CollisionEnd, spellData.Radius, spellData.RingRadius, 22);
                    break;
                case SkillShotType.SkillshotArc:
                    Arc = new Geometry.Polygon.Arc(
                        start,
                        end,
                        Configs.SkillShotsExtraRadius + (int) ObjectManager.Player.BoundingRadius,
                        22);
                    break;
            }
            UpdatePolygon();
        }

        #endregion

        #region Properties

        private Vector2 CollisionEnd
        {
            get
            {
                return collisionEnd.IsValid()
                    ? collisionEnd
                    : (SpellData.RawRange == 20000
                        ? GetGlobalMissilePosition(0)
                          + Direction*SpellData.MissileSpeed
                          *(0.5f + SpellData.Radius*2/ObjectManager.Player.MoveSpeed)
                        : End);
            }
        }

        #endregion

        #region Fields

        public Geometry.Polygon.Arc Arc;

        public Geometry.Polygon.Circle Circle;

        public DetectionType DetectionType;

        public Vector2 Direction;

        public Vector2 End;

        public bool ForceDisabled;

        public Geometry.Polygon Polygon;

        public Geometry.Polygon.Rectangle Rectangle;

        public Geometry.Polygon.Ring Ring;

        public Geometry.Polygon.Sector Sector;

        public SpellData SpellData;

        public Vector2 Start;

        public int StartTick;

        private bool cachedValue;

        private int cachedValueTick;

        private Vector2 collisionEnd;

        private int helperTick;

        private int lastCollisionCalc;

        #endregion

        #region Public Properties

        public bool Evade
        {
            get
            {
                if (ForceDisabled)
                {
                    return false;
                }
                if (Core.GameTickCount - cachedValueTick < 100)
                {
                    return cachedValue;
                }
                cachedValue = Program.Menu["ESS_" + SpellData.MenuItemName].Cast<CheckBox>().CurrentValue;
                cachedValueTick = Core.GameTickCount;
                return cachedValue;
            }
        }

        public bool IsActive
        {
            get
            {
                return SpellData.MissileAccel != 0
                    ? Core.GameTickCount <= StartTick + 5000
                    : Core.GameTickCount
                      <= StartTick + SpellData.Delay + SpellData.ExtraDuration
                      + 1000*(Start.Distance(End)/SpellData.MissileSpeed);
            }
        }

        public Vector2 Perpendicular
        {
            get { return Direction.Perpendicular(); }
        }

        public Obj_AI_Base Unit { get; set; }

        #endregion

        #region Public Methods and Operators

        public Vector2 GetMissilePosition(int time)
        {
            var t = Math.Max(0, Core.GameTickCount + time - StartTick - SpellData.Delay);
            int x;
            if (SpellData.MissileAccel == 0)
            {
                x = t*SpellData.MissileSpeed/1000;
            }
            else
            {
                var t1 = (SpellData.MissileAccel > 0
                    ? SpellData.MissileMaxSpeed
                    : SpellData.MissileMinSpeed - SpellData.MissileSpeed)*1000f
                         /SpellData.MissileAccel;
                x = t <= t1
                    ? (int)
                        (t*SpellData.MissileSpeed/1000d
                         + 0.5d*SpellData.MissileAccel*Math.Pow(t/1000d, 2))
                    : (int)
                        (t1*SpellData.MissileSpeed/1000d
                         + 0.5d*SpellData.MissileAccel*Math.Pow(t1/1000d, 2)
                         + (t - t1)/1000d
                         *(SpellData.MissileAccel < 0
                             ? SpellData.MissileMaxSpeed
                             : SpellData.MissileMinSpeed));
            }
            return Start + Direction*(int) Math.Max(0, Math.Min(CollisionEnd.Distance(Start), x));
        }

        public bool IsAboutToHit(int time, Obj_AI_Base unit)
        {
            if (SpellData.Type != SkillShotType.SkillshotMissileLine)
            {
                return !IsSafePoint(unit.ServerPosition.To2D())
                       && SpellData.ExtraDuration + SpellData.Delay
                       + (int) (1000*Start.Distance(End)/SpellData.MissileSpeed)
                       - (Core.GameTickCount - StartTick) <= time;
            }
            var project = unit.ServerPosition.To2D()
                .ProjectOn(GetMissilePosition(0), GetMissilePosition(time));
            return project.IsOnSegment && unit.Distance(project.SegmentPoint) < SpellData.Radius;
        }

        public SafePathResult IsSafePath(List<Vector2> path, int timeOffset, int speed = -1, int delay = 0)
        {
            var distance = 0f;
            timeOffset += Game.Ping/2;
            speed = speed == -1 ? (int) ObjectManager.Player.MoveSpeed : speed;
            var allIntersections = new List<FoundIntersection>();
            for (var i = 0; i <= path.Count - 2; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var segmentIntersections = new List<FoundIntersection>();
                for (var j = 0; j <= Polygon.Points.Count - 1; j++)
                {
                    var sideStart = Polygon.Points[j];
                    var sideEnd = Polygon.Points[j == (Polygon.Points.Count - 1) ? 0 : j + 1];
                    var intersection = from.Intersection(to, sideStart, sideEnd);
                    if (intersection.Intersects)
                    {
                        segmentIntersections.Add(
                            new FoundIntersection(
                                distance + intersection.Point.Distance(from),
                                (int) ((distance + intersection.Point.Distance(from))*1000/speed),
                                intersection.Point,
                                from));
                    }
                }
                allIntersections.AddRange(segmentIntersections.OrderBy(o => o.Distance));
                distance += from.Distance(to);
            }
            if (SpellData.Type == SkillShotType.SkillshotMissileLine
                || SpellData.Type == SkillShotType.SkillshotMissileCone
                || SpellData.Type == SkillShotType.SkillshotArc)
            {
                if (IsSafePoint(ObjectManager.Player.ServerPosition.To2D()))
                {
                    if (allIntersections.Count == 0)
                    {
                        return new SafePathResult(true, new FoundIntersection());
                    }
                    if (SpellData.DontCross)
                    {
                        return new SafePathResult(false, allIntersections[0]);
                    }
                    for (var i = 0; i <= allIntersections.Count - 1; i = i + 2)
                    {
                        var enterIntersection = allIntersections[i];
                        var enterIntersectionProjection =
                            enterIntersection.Point.ProjectOn(Start, End).SegmentPoint;
                        if (i == allIntersections.Count - 1)
                        {
                            return
                                new SafePathResult(
                                    (End.Distance(GetMissilePosition(enterIntersection.Time - timeOffset))
                                     + 50 <= End.Distance(enterIntersectionProjection))
                                    && ObjectManager.Player.MoveSpeed < SpellData.MissileSpeed,
                                    allIntersections[0]);
                        }
                        var exitIntersection = allIntersections[i + 1];
                        var exitIntersectionProjection =
                            exitIntersection.Point.ProjectOn(Start, End).SegmentPoint;
                        if (GetMissilePosition(enterIntersection.Time - timeOffset).Distance(End) + 50
                            > enterIntersectionProjection.Distance(End)
                            && GetMissilePosition(exitIntersection.Time + timeOffset).Distance(End)
                            <= exitIntersectionProjection.Distance(End))
                        {
                            return new SafePathResult(false, allIntersections[0]);
                        }
                    }
                    return new SafePathResult(true, allIntersections[0]);
                }
                if (allIntersections.Count == 0)
                {
                    return new SafePathResult(false, new FoundIntersection());
                }
                if (allIntersections.Count > 0)
                {
                    var exitIntersection = allIntersections[0];
                    var exitIntersectionProjection = exitIntersection.Point.ProjectOn(Start, End).SegmentPoint;
                    if (GetMissilePosition(exitIntersection.Time + timeOffset).Distance(End)
                        <= exitIntersectionProjection.Distance(End))
                    {
                        return new SafePathResult(false, allIntersections[0]);
                    }
                }
            }
            if (allIntersections.Count == 0)
            {
                return new SafePathResult(false, new FoundIntersection());
            }
            if (IsSafePoint(ObjectManager.Player.ServerPosition.To2D()) && SpellData.DontCross)
            {
                return new SafePathResult(false, allIntersections[0]);
            }
            var timeToExplode = (SpellData.DontAddExtraDuration ? 0 : SpellData.ExtraDuration)
                                + SpellData.Delay
                                + (int) (1000*Start.Distance(End)/SpellData.MissileSpeed)
                                - (Core.GameTickCount - StartTick);
            return !IsSafePoint(path.PositionAfter(timeToExplode, speed, delay))
                ? new SafePathResult(false, allIntersections[0])
                : new SafePathResult(
                    IsSafePoint(path.PositionAfter(timeToExplode, speed, timeOffset)),
                    allIntersections[0]);
        }

        public bool IsSafePoint(Vector2 point)
        {
            return Polygon.IsOutside(point);
        }

        public void OnUpdate()
        {
            if (SpellData.CollisionObjects.Count() > 0 && SpellData.CollisionObjects != null
                && Core.GameTickCount - lastCollisionCalc > 50)
            {
                lastCollisionCalc = Core.GameTickCount;
                collisionEnd = this.GetCollisionPoint();
            }
            if (SpellData.Type == SkillShotType.SkillshotMissileLine)
            {
                Rectangle = new Geometry.Polygon.Rectangle(
                    GetMissilePosition(0),
                    CollisionEnd,
                    SpellData.Radius);
                UpdatePolygon();
            }
            if (SpellData.MissileFollowsUnit && Unit.IsVisible)
            {
                End = Unit.ServerPosition.To2D();
                Direction = (End - Start).Normalized();
                UpdatePolygon();
            }
            if (SpellData.SpellName == "SionR")
            {
                if (helperTick == 0)
                {
                    helperTick = StartTick;
                }
                SpellData.MissileSpeed = (int) Unit.MoveSpeed;
                if (Unit.IsValidTarget(float.MaxValue, false))
                {
                    if (!Unit.HasBuff("SionR") && Core.GameTickCount - helperTick > 600)
                    {
                        StartTick = 0;
                    }
                    else
                    {
                        StartTick = Core.GameTickCount - SpellData.Delay;
                        Start = Unit.ServerPosition.To2D();
                        End = Unit.ServerPosition.To2D() + 1000*Unit.Direction.To2D().Perpendicular();
                        Direction = (End - Start).Normalized();
                        UpdatePolygon();
                    }
                }
                else
                {
                    StartTick = 0;
                }
            }
            if (SpellData.FollowCaster)
            {
                Circle.Center = Unit.ServerPosition.To2D();
                UpdatePolygon();
            }
        }

        #endregion

        #region Methods

        private Vector2 GetGlobalMissilePosition(int time)
        {
            return Start
                   + Direction
                   *(int)
                       Math.Max(
                           0,
                           Math.Min(
                               End.Distance(Start),
                               (float) Math.Max(0, Core.GameTickCount + time - StartTick - SpellData.Delay)
                               *SpellData.MissileSpeed/1000));
        }

        private void UpdatePolygon()
        {
            switch (SpellData.Type)
            {
                case SkillShotType.SkillshotCircle:
                    Circle.UpdatePolygon();
                    Polygon = Circle;
                    break;
                case SkillShotType.SkillshotLine:
                case SkillShotType.SkillshotMissileLine:
                    Rectangle.UpdatePolygon();
                    Polygon = Rectangle;
                    break;
                case SkillShotType.SkillshotCone:
                    Sector.UpdatePolygon();
                    Polygon = Sector;
                    break;
                case SkillShotType.SkillshotRing:
                    Ring.UpdatePolygon();
                    Polygon = Ring;
                    break;
                case SkillShotType.SkillshotArc:
                    Arc.UpdatePolygon();
                    Polygon = Arc;
                    break;
            }
        }

        #endregion
    }
}