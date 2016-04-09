using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using Evade;
using LeagueSharp.Common;
using SharpDX;

namespace YasuoPro
{
    internal static class YasuoEvade
    {
        public static int TickCount
        {
            get { return (int) (Game.Time*1000f); }
        }

        internal static void Evade()
        {
            if (!Helper.GetBool("Evade.Enabled"))
            {
                return;
            }

            foreach (var skillshot in Program.DetectedSkillshots.ToList())
            {
                if (skillshot.Dodged)
                {
                    if (Helper.Debug)
                    {
                        Chat.Print(skillshot.SpellData.SpellName + " Dodged already");
                    }
                    continue;
                }

                //Avoid trying to evade while dashing
                if (Helper.Yasuo.IsDashing())
                {
                    return;
                }

                if (Helper.GetBool("Evade.OnlyDangerous") && !skillshot.SpellData.IsDangerous)
                {
                    continue;
                }

                if ((skillshot.SpellData.Type == SkillShotType.SkillshotCircle ||
                     (skillshot.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall)) &&
                     !SpellSlot.E.IsReady()))
                {
                    continue;
                }


                if (((Program.NoSolutionFound ||
                      !Program.IsSafePath(Helper.Yasuo.GetWaypoints(), 250).IsSafe &&
                      !Program.IsSafe(Helper.Yasuo.Position.LSTo2D()).IsSafe)))
                {
                    Helper.DontDash = true;
                    if (skillshot.IsAboutToHit(700, Helper.Yasuo) &&
                        skillshot.SpellData.Type != SkillShotType.SkillshotCircle && Helper.GetBool("Evade.UseW"))
                    {
                        if (skillshot.SpellData.CollisionObjects.Contains(CollisionObjectTypes.YasuoWall) &&
                            skillshot.Evade(SpellSlot.W)
                            && skillshot.SpellData.DangerValue >= Helper.GetSliderInt("Evade.MinDangerLevelWW"))
                        {
                            var castpos = Helper.Yasuo.ServerPosition.LSExtend(skillshot.MissilePosition.To3D(), 50);
                            if (TickCount - skillshot.StartTick >=
                                skillshot.SpellData.setdelay + Helper.GetSliderInt("Evade.Delay"))
                            {
                                var WCasted = Helper.W.Cast(castpos);
                                Program.DetectedSkillshots.Remove(skillshot);
                                skillshot.Dodged = WCasted;
                                if (WCasted)
                                {
                                    if (Helper.Debug)
                                    {
                                        Chat.Print("Blocked " + skillshot.SpellData.SpellName + " with Windwall ");
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    if (skillshot.IsAboutToHit(500, Helper.Yasuo) && skillshot.Evade(SpellSlot.E) && !skillshot.Dodged &&
                        Helper.GetBool("Evade.UseE") &&
                        skillshot.SpellData.DangerValue >= Helper.GetSliderInt("Evade.MinDangerLevelE"))
                    {
                        var evadetarget =
                            ObjectManager
                                .Get<Obj_AI_Base>()
                                .Where(
                                    x =>
                                        x.IsDashable() && !Helper.GetDashPos(x).PointUnderEnemyTurret() &&
                                        Program.IsSafe(x.ServerPosition.LSTo2D()).IsSafe &&
                                        Program.IsSafePath(x.GeneratePathTo(), 0, 1200, 250).IsSafe)
                                .MinOrDefault(x => x.LSDistance(Helper.shop));

                        if (evadetarget != null)
                        {
                            Helper.E.Cast(evadetarget);
                            Program.DetectedSkillshots.Remove(skillshot);
                            skillshot.Dodged = true;
                            if (Helper.Debug)
                            {
                                Chat.Print("Evading " + skillshot.SpellData.SpellName + " " + "using E to " +
                                           evadetarget.BaseSkinName);
                            }
                        }
                    }
                }
            }

            Helper.DontDash = false;
        }

        private static List<Vector2> GeneratePathTo(this Obj_AI_Base unit)
        {
            var path = new List<Vector2>();
            path.Add(Helper.Yasuo.ServerPosition.LSTo2D());
            path.Add(Helper.GetDashPos(unit));
            return path;
        }
    }
}