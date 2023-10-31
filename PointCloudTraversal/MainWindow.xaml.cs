using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Linq;

namespace PointCloudTraversal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static (float, float, float)?[] Points { get; private set; }
        internal static float[] Bounds { get; private set; }
        internal (float, float, float)? PointA { get; set; }
        internal (float, float, float)? PointB { get; set; }
        internal PointsVisual3D StartingPoint { get; set; }
        internal PointsVisual3D FinishingPoint { get; set; }
        internal List<LinesVisual3D> PathBoxes { get; set; }
        internal List<LinesVisual3D> StartingBox { get; set; }
        internal List<LinesVisual3D> FinishingBox { get; set; }
        internal int OctreeDepth { get; set; }
        internal OctreeNode RootNode { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            inputPercentage.Text = "0.01";
            OctreeDepth = 6;
            inputOctreeDepth.Text = $"{OctreeDepth}";

            buttonRender.IsEnabled = false;
            buttonPointAPick.IsEnabled = false;
            buttonPointBPick.IsEnabled = false;
            buttonCalculateOctree.IsEnabled = false;
            buttonCalculatePath.IsEnabled = false;

            StartingPoint = new PointsVisual3D { Color = Colors.Green, Size = 5 };
            FinishingPoint = new PointsVisual3D { Color = Colors.Red, Size = 5 };
            PathBoxes = new List<LinesVisual3D>();
            StartingBox = new List<LinesVisual3D>();
            FinishingBox = new List<LinesVisual3D>();

            PointA = null;
            PointB = null;
        }

        private void MenuFileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Lidar point cloud data|*.las";

            if (dialog.ShowDialog() == true)
            {
                Points = LasReader.ReadPointCloud(dialog.FileName);
                Bounds = LasReader.ReadPointCloudBounds(dialog.FileName);
                RootNode = new OctreeNode(Bounds[0], Bounds[1], Bounds[2], Bounds[3], Bounds[4], Bounds[5], 0);
                RootNode.PointsArray = Points.ToArray();
                buttonRender.IsEnabled = true;
            }
        }

        private void SettingsButtonRender_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double percentage = Convert.ToDouble(inputPercentage.Text);
                buttonRender.IsEnabled = false;
                viewport.Children.Clear();
                RenderActions.RenderPointCloud(percentage, Points, viewport);
                buttonRender.IsEnabled = true;
                buttonPointAPick.IsEnabled = true;
                buttonPointBPick.IsEnabled = true;
                buttonCalculateOctree.IsEnabled = true;
            }
            catch
            {
                MessageBox.Show($"Percentage value has to be between 0 and 1.");
            }
        }

        private void SettingsButtonPointAPick_Click(object sender, RoutedEventArgs e)
        {
            int? pointIndex = null;

            if (PointA != null)
            {
                StartingPoint.Points.Clear();
                viewport.Children.Remove(StartingPoint);
            }

            if (checkboxPointARandom.IsChecked == true)
            {
                Random random = new Random();
                pointIndex = random.Next(0, Points.Length);
            }
            else if (inputPointAIndex.Text == null)
            {
                MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                return;
            }
            else
            {
                try
                {
                    pointIndex = Convert.ToInt32(inputPointAIndex.Text);
                    if (pointIndex < 0 || pointIndex > Points.Length)
                    {
                        MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                }
            }

            if (pointIndex != null)
            {
                PointA = (Points[pointIndex.Value].Value.Item1, Points[pointIndex.Value].Value.Item2, Points[pointIndex.Value].Value.Item3);
                StartingPoint.Points.Add(new Point3D(PointA.Value.Item1, PointA.Value.Item2, PointA.Value.Item3));
                viewport.Children.Add(StartingPoint);
                inputPointAIndex.Text = $"{pointIndex}";
            }
        }

        private void SettingsButtonPointBPick_Click(object sender, RoutedEventArgs e)
        {
            int? pointIndex = null;

            if (PointB != null)
            {
                viewport.Children.Remove(FinishingPoint);
                FinishingPoint.Points.Clear();
            }

            if (checkboxPointBRandom.IsChecked == true)
            {
                Random random = new Random();
                pointIndex = random.Next(0, Points.Length);
            }
            else if (inputPointBIndex.Text == null)
            {
                MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                return;
            }
            else
            {
                try
                {
                    pointIndex = Convert.ToInt32(inputPointBIndex.Text);
                    if (pointIndex < 0 || pointIndex > Points.Length)
                    {
                        MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show($"Enter point index between 0 and {Points.Length}");
                }
            }

            if (pointIndex != null)
            {
                PointB = (Points[pointIndex.Value].Value.Item1, Points[pointIndex.Value].Value.Item2, Points[pointIndex.Value].Value.Item3);
                FinishingPoint.Points.Add(new Point3D(PointB.Value.Item1, PointB.Value.Item2, PointB.Value.Item3));
                viewport.Children.Add(FinishingPoint);
                inputPointBIndex.Text = $"{pointIndex}";
            }
        }

        private void SettingsButtonCalculateOctree_Click(object sender, RoutedEventArgs e)
        {
            buttonCalculateOctree.IsEnabled = false;
            RootNode.PointsArray = Points.ToArray();
            OctreeActions.Nodes.Clear();

            try
            {
                int depth = Convert.ToInt32(inputOctreeDepth.Text);
                OctreeActions.RecursiveDivide(RootNode, depth, null);

                buttonCalculateOctree.IsEnabled = true;
                buttonCalculatePath.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Depth must be a positive integer: {ex.Message}");
            }
        }

        private void SettingsButtonCalculatePath_Click(object sender, RoutedEventArgs e)
        {
            if (PointA == null || PointB == null)
            {
                MessageBox.Show($"Point A and point B must be set");
                return;
            }

            if (PathBoxes.Count != 0)
            {
                foreach (var line in PathBoxes)
                {
                    viewport.Children.Remove(line);
                }
                PathBoxes.Clear();

                foreach (var line in StartingBox)
                {
                    viewport.Children.Remove(line);
                }

                foreach (var line in FinishingBox)
                {
                    viewport.Children.Remove(line);
                }
            }

            List<OctreeNode> nodes = OctreeActions.CalculatePath(PointA, PointB);

            var startingBoxLines = nodes[0].GenerateBoundingBox(Colors.Red, 2);
            foreach (var line in startingBoxLines)
            {
                StartingBox.Add(line);
                viewport.Children.Add(line);
            }

            int? index = null;
            if (nodes.Count == 1)
            {
                index = 0;
            }
            else
            {
                index = nodes.Count - 1;
            }

            var finishingBoxLines = nodes[index.Value].GenerateBoundingBox(Colors.LightGreen, 2);
            foreach (var line in finishingBoxLines)
            {
                FinishingBox.Add(line);
                viewport.Children.Add(line);
            }

            if (nodes.Count != 1)
            {
                for (int i = 1; i < nodes.Count - 1; i++)
                {
                    var nodeLines = nodes[i].GenerateBoundingBox(Colors.LightPink, 2);
                    foreach (var line in nodeLines)
                    {
                        PathBoxes.Add(line);
                        viewport.Children.Add(line);
                    }
                }
            }
        }
    }
}
