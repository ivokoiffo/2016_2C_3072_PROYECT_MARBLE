using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Geometry;
using TGC.Core.Utils;

namespace TGC.Group.Model.Utils
{
    static class CheckpointHelper
    {
        public static List<TgcArrow> lastCheckPointArrows;
        public static List<Checkpoint> checkpoints;
        public static GameModel EjemploAlumno { get; set; }
        public static Vector3 origenMapa = new Vector3(0, 150, 0);
        public static void renderAll()
        {
            checkpoints.ForEach(c => c.render());
        }

        public static void GenerateGraph()
        {
            List<Checkpoint> checkPoints = new List<Checkpoint>(checkpoints);
            foreach (Checkpoint checkPoint in checkpoints)
            {
                List<Checkpoint> unCheckedCheckpoints = checkPoints;
                unCheckedCheckpoints = unCheckedCheckpoints.FindAll(c => checkPoint.hasDirectSightWith(c));
                checkPoint.Neighbors =
                    new List<Checkpoint>(
                        unCheckedCheckpoints.FindAll(c => checkPoint.hasDirectSightWith(c))
                            .FindAll(
                                neighbor =>
                                            !unCheckedCheckpoints.Any(
                                                c =>
                                                    c != neighbor &&
                                                    neighbor.hasDirectSightWith(c) &&
                                                    Math.Abs(AngleBetweenInXandZ(checkPoint, neighbor, c)) < 15 &&
                                                    DistanceBetweenInXandZ(checkPoint, c) <
                                                    DistanceBetweenInXandZ(checkPoint, neighbor))

                            ));
            }
        }

        public static float DistanceBetweenInXandZ(Checkpoint checkPoint, Checkpoint otherCheckpoint)
        {
            Vector3 positionOne = new Vector3(checkPoint.Position.X, 0f, checkPoint.Position.Z);
            Vector3 positionTwo = new Vector3(otherCheckpoint.Position.X, 0f, otherCheckpoint.Position.Z);
            return Vector3.Length(positionOne - positionTwo);
        }

        public static float AngleBetweenInXandZ(Checkpoint checkPointBase, Checkpoint otherCheckpoint, Checkpoint otherCheckpoint2)
        {
            Vector3 vector1 = otherCheckpoint.Position - checkPointBase.Position;
            Vector3 vector2 = otherCheckpoint2.Position - checkPointBase.Position;
            vector1.Normalize();
            vector2.Normalize();
            return Geometry.RadianToDegree(Convert.ToSingle(Math.Acos(Vector3.Dot(vector1, vector2))));
        }

        public static List<TgcArrow> PrepareClosestCheckPoint(Vector3 position, Checkpoint lastCheckPoint, out Checkpoint updatedChekPoint)
        {
            updatedChekPoint = GetClosestCheckPoint(position);
            if (lastCheckPoint != updatedChekPoint)
            {
                lastCheckPoint = updatedChekPoint;
                lastCheckPointArrows = updatedChekPoint.Neighbors.Select(c =>
                {
                    TgcArrow arrow = new TgcArrow();
                    arrow.PStart = lastCheckPoint.Position;
                    arrow.PEnd = c.Position;
                    arrow.BodyColor = Color.Black;
                    arrow.HeadColor = Color.White;
                    arrow.Thickness = 0.4f;
                    arrow.HeadSize = new Vector2(8, 10);

                    arrow.updateValues();
                    return arrow;
                }).ToList();
            }

            return lastCheckPointArrows;

        }
        public static Checkpoint GetClosestCheckPoint(Vector3 position)
        {
            if(checkpoints.Count == 0)
            {
                return null;
            }

            List<Checkpoint> checkPoints = new List<Checkpoint>(checkpoints);
            

            return checkPoints.Aggregate((checkPointMin, aCheckpoint) => (checkPointMin == null || Vector3.Length(position - aCheckpoint.Position) < (Vector3.Length(position - checkPointMin.Position)) ? aCheckpoint : checkPointMin));
        }

        public static void DestroyLinkBetween(int IdCheckpoint, int IdNeighbor)
        {
            var checkpoint1 = checkpoints.Find(c => c.id == IdCheckpoint);
            checkpoint1.DeleteNeighbor(IdNeighbor);
        }

        public static void BuildCheckpoints()
        {
            checkpoints = new List<Checkpoint>();
            checkpoints.Add(new Checkpoint(new Vector3(1208f, 0f, 480f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(879f, 0f, 480f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(588f, 0f, 480f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(588f, 0f, 947f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(180f, 0f, 947f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(180f, 0f, 1630f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(712f, 0f, 1630f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(1203f, 0f, 1630f) + origenMapa));
            checkpoints.Add(new Checkpoint(new Vector3(1203f, 0f, 1063f) + origenMapa));

            GenerateGraph();
        }

    }
}
