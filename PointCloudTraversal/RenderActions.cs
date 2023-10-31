using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace PointCloudTraversal
{
    internal class RenderActions
    {
        internal static void RenderPointCloud(double percentage, (float, float, float)?[] points, HelixViewport3D viewport)
        {
            int pointsCount = points.Length;
            double counterMax = pointsCount * percentage;

            HashSet<int> randomNumbers = Utilities.GenerateRandom((int)counterMax, 0, pointsCount);
            HashSet<Point3D> pointsHash = new HashSet<Point3D>();
            Point3D point3D = new Point3D();

            for (int i = 0; i < points.Length; i++)
            {
                if (randomNumbers.Contains(i))
                {
                    point3D.X = points[i].Value.Item1;
                    point3D.Y = points[i].Value.Item2;
                    point3D.Z = points[i].Value.Item3;
                    pointsHash.Add(point3D);
                }
            }

            PointsVisual3D cloudPoints = new PointsVisual3D { Color = Colors.LightBlue, Size = 1 };
            Point3DCollection pointsCollection = new Point3DCollection(pointsHash);
            cloudPoints.Points = pointsCollection;
            viewport.Children.Add(cloudPoints);
        }
    }
}
