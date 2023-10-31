using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace PointCloudTraversal
{
    internal class OctreeNode
    {
        internal float MinX { get; set; }
        internal float MaxX { get; set; }
        internal float MinY { get; set; }
        internal float MaxY { get; set; }
        internal float MinZ { get; set; }
        internal float MaxZ { get; set; }
        internal byte Depth { get; set; }
        internal float? GCost { get; set; }
        internal float? HCost { get; set; }
        internal float? FCost { get; set; }
        internal OctreeNode Parent { get; set; }
        internal (float, float, float) Center { get; set; }
        internal (float, float, float)?[] PointsArray { get; set; }
        internal OctreeNode[] Children { get; set; }

        internal OctreeNode(float minX, float maxX, float minY, float maxY, float minZ, float maxZ, byte parentDepth)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            MinZ = minZ;
            MaxZ = maxZ;
            Depth = (byte)(parentDepth + 1);
            Center = ((float)(minX + (maxX - minX) / 2), (float)(minY + (maxY - minY) / 2), (float)(minZ + (maxZ - minZ) / 2));
            FCost = null;
            GCost = null;
            HCost = null;
            Parent = null;
        }

        internal static float DistanceBetweenPoints((float, float, float) pointA, (float, float, float) pointB)
        {
            return (float)Math.Sqrt(Math.Pow(pointB.Item1 - pointA.Item1, 2) + Math.Pow(pointB.Item2 - pointA.Item2, 2) + Math.Pow(pointB.Item3 - pointA.Item3, 2));
        }

        internal void CalculateCosts(OctreeNode parent, (float, float, float) targetPoint)
        {
            GCost = parent.GCost + DistanceBetweenPoints(parent.Center, Center);
            HCost = DistanceBetweenPoints(targetPoint, Center);
            FCost = GCost + HCost;
        }

        internal bool CurrentNodeContainsPoint((float, float, float) point)
        {
            return point.Item1 >= MinX && point.Item1 <= MaxX
                && point.Item2 >= MinY && point.Item2 <= MaxY
                && point.Item3 >= MinZ && point.Item3 <= MaxZ;
        }

        internal (float, float, float)?[] PickOutPoints((float, float, float)?[] parentPoints)
        {
            List<(float, float, float)?> childPoints = new List<(float, float, float)?>();
            for (int i = 0; i < parentPoints.Length; i++)
            {
                if (parentPoints[i] != null)
                {
                    if (CurrentNodeContainsPoint(((float, float, float))parentPoints[i]))
                    {
                        childPoints.Add(parentPoints[i]);
                        parentPoints[i] = null;
                    }
                }
            }

            PointsArray = childPoints.ToArray();

            return parentPoints;
        }

        internal void Divide(int? maxDepth, int? minPointCount)
        {
            if ((maxDepth != null && Depth < maxDepth) || (minPointCount != null && PointsArray.Length > minPointCount))
            {
                Children = new OctreeNode[8];

                Children[0] = (new OctreeNode(MinX, Center.Item1, MinY, Center.Item2, MinZ, Center.Item3, Depth));
                Children[1] = (new OctreeNode(MinX, Center.Item1, Center.Item2, MaxY, MinZ, Center.Item3, Depth));
                Children[2] = (new OctreeNode(Center.Item1, MaxX, Center.Item2, MaxY, MinZ, Center.Item3, Depth));
                Children[3] = (new OctreeNode(Center.Item1, MaxX, MinY, Center.Item2, MinZ, Center.Item3, Depth));
                Children[4] = (new OctreeNode(MinX, Center.Item1, MinY, Center.Item2, Center.Item3, MaxZ, Depth));
                Children[5] = (new OctreeNode(MinX, Center.Item1, Center.Item2, MaxY, Center.Item3, MaxZ, Depth));
                Children[6] = (new OctreeNode(Center.Item1, MaxX, Center.Item2, MaxY, Center.Item3, MaxZ, Depth));
                Children[7] = (new OctreeNode(Center.Item1, MaxX, MinY, Center.Item2, Center.Item3, MaxZ, Depth));
            }
        }

        internal IEnumerable<LinesVisual3D> GenerateBoundingBox(Color? color, double thickness = 1)
        {
            if (color == null)
            {
                color = Colors.Black;
            }

            List<Point3DCollection> lineEndPairs = new List<Point3DCollection>()
            {
                new Point3DCollection {
                    new Point3D(MinX, MinY, MinZ),
                    new Point3D(MinX, MaxY, MinZ)},
                new Point3DCollection {
                    new Point3D(MinX, MaxY, MinZ),
                    new Point3D(MaxX, MaxY, MinZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MaxY, MinZ),
                    new Point3D(MaxX, MinY, MinZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MinY, MinZ),
                    new Point3D(MinX, MinY, MinZ)},
                new Point3DCollection {
                    new Point3D(MinX, MinY, MaxZ),
                    new Point3D(MinX, MaxY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MinX, MaxY, MaxZ),
                    new Point3D(MaxX, MaxY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MaxY, MaxZ),
                    new Point3D(MaxX, MinY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MinY, MaxZ),
                    new Point3D(MinX, MinY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MinX, MinY, MinZ),
                    new Point3D(MinX, MinY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MinX, MaxY, MinZ),
                    new Point3D(MinX, MaxY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MaxY, MinZ),
                    new Point3D(MaxX, MaxY, MaxZ)},
                new Point3DCollection {
                    new Point3D(MaxX, MinY, MinZ),
                    new Point3D(MaxX, MinY, MaxZ)},
            };

            foreach (Point3DCollection pair in lineEndPairs)
            {
                yield return new LinesVisual3D
                {
                    Points = pair,
                    Color = (Color)color,
                    Thickness = thickness,
                };
            }
        }
    }
}
