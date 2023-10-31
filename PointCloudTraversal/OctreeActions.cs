using System.Collections.Generic;
using System.Linq;

namespace PointCloudTraversal
{
    internal class OctreeActions
    {
        internal static List<OctreeNode> Nodes = new List<OctreeNode>();

        internal static List<OctreeNode> CalculatePath((float, float, float)? startingPoint, (float, float, float)? finishingPoint)
        {
            OctreeNode[] nodesArray = new OctreeNode[Nodes.Count];
            Nodes.CopyTo(nodesArray);
            List<OctreeNode> nodes = nodesArray.ToList();
            List<OctreeNode> openNodes = new List<OctreeNode>();
            List<OctreeNode> closedNodes = new List<OctreeNode>();
            List<OctreeNode> path = new List<OctreeNode>();
            OctreeNode startingNode = null;
            OctreeNode targetNode = null;
            OctreeNode currentNode;

            foreach (var node in nodes)
            {
                if (startingNode != null && targetNode != null)
                {
                    break;
                }
                else if (node.CurrentNodeContainsPoint((startingPoint.Value.Item1, startingPoint.Value.Item2, startingPoint.Value.Item3)))
                {
                    startingNode = node;
                }
                else if (node.CurrentNodeContainsPoint((finishingPoint.Value.Item1, finishingPoint.Value.Item2, finishingPoint.Value.Item3)))
                {
                    targetNode = node;
                }
            }

            if (startingNode != null && targetNode == null)
            {
                path.Add(startingNode);
                return path;
            }

            nodes.Remove(startingNode);

            startingNode.GCost = 0;
            startingNode.HCost = 0;
            startingNode.FCost = 0;
            currentNode = startingNode;

            while (true)
            {
                float lowestFCost = float.MaxValue;
                foreach (var node in openNodes)
                {
                    if (node.FCost != null && node.FCost < lowestFCost)
                    {
                        lowestFCost = (float)node.FCost;
                        currentNode = node;
                    }
                }

                openNodes.Remove(currentNode);
                closedNodes.Add(currentNode);

                if (currentNode.CurrentNodeContainsPoint(targetNode.Center))
                {
                    OctreeNode node = currentNode;
                    while (node != startingNode)
                    {
                        path.Add(node);
                        node = node.Parent;
                    }

                    path.Add(startingNode);
                    return path;
                }

                (float, float, float)[] neighboringPoints = new (float, float, float)[6];
                neighboringPoints[0] = (currentNode.Center.Item1 + (currentNode.MaxX - currentNode.MinX), currentNode.Center.Item2, currentNode.Center.Item3);
                neighboringPoints[1] = (currentNode.Center.Item1 - (currentNode.MaxX - currentNode.MinX), currentNode.Center.Item2, currentNode.Center.Item3);
                neighboringPoints[2] = (currentNode.Center.Item1, currentNode.Center.Item2 + (currentNode.MaxY - currentNode.MinY), currentNode.Center.Item3);
                neighboringPoints[3] = (currentNode.Center.Item1, currentNode.Center.Item2 - (currentNode.MaxY - currentNode.MinY), currentNode.Center.Item3);
                neighboringPoints[4] = (currentNode.Center.Item1, currentNode.Center.Item2, currentNode.Center.Item3 + (currentNode.MaxZ - currentNode.MinZ));
                neighboringPoints[5] = (currentNode.Center.Item1, currentNode.Center.Item2, currentNode.Center.Item3 - (currentNode.MaxZ - currentNode.MinZ));

                foreach (var node in nodes)
                {
                    foreach (var neighborPoint in neighboringPoints)
                    {
                        if (node.CurrentNodeContainsPoint(neighborPoint) && !closedNodes.Contains(node))
                        {
                            if (!openNodes.Contains(node))
                            {
                                openNodes.Add(node);
                                node.Parent = currentNode;
                                node.CalculateCosts(node.Parent, targetNode.Center);
                            }

                            if (node.FCost != null)
                            {
                                float oldFCost = node.FCost.Value;
                                node.CalculateCosts(currentNode, targetNode.Center);
                                if (node.FCost.Value < oldFCost)
                                {
                                    node.Parent = currentNode;
                                }
                                else
                                {
                                    node.CalculateCosts(node.Parent, targetNode.Center);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void RecursiveDivide(OctreeNode parentNode, int? maxDepth, int? minPointCount)
        {
            if (maxDepth == null && minPointCount == null)
            {
                throw new System.Exception("Either maxDepth or minPointCount must be provided.");
            }
            else if (maxDepth != null && minPointCount != null)
            {
                throw new System.Exception("maxDepth and minPointCount cannot be provided both at the same time.");
            }

            parentNode.Divide(maxDepth, minPointCount);
            if (parentNode.Depth == maxDepth)
            {
                Nodes.Add(parentNode);
            }
            if (parentNode.Children != null)
            {
                foreach (var node in parentNode.Children)
                {
                    parentNode.PointsArray = node.PickOutPoints(parentNode.PointsArray);
                    if (node.PointsArray.Length != 0)
                    {
                        RecursiveDivide(node, maxDepth, minPointCount);
                    }
                }

                parentNode.PointsArray = null;
            }
        }
    }
}
