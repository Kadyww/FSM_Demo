using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private readonly float cellSize;

    public readonly Node[] nodes;
    private readonly int width;

    public Grid(int width, float cellSize)
    {
        this.width = width;
        this.cellSize = cellSize;
        nodes = new Node[width * width];
        CreateGrid();
    }

    private Vector3 Origin => new(-width * cellSize / 2, 0, -width * cellSize / 2);

    private void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                Vector3 pos = Origin + new Vector3(x * cellSize, 0, y * cellSize);
                nodes[x * width + y] = new Node(x, y, pos);
            }
        }
    }

    public Node GetNode(int x, int y)
    {
        return x < 0 || x >= width || y < 0 || y >= width ? null : nodes[x * width + y];
    }

    public Node GetNode(Vector3 position)
    {
        position /= cellSize;
        int x = Mathf.FloorToInt(position.x + width / 2f);
        int y = Mathf.FloorToInt(position.z + width / 2f);
        return GetNode(x, y);
    }

    public HashSet<Node> GetBoundaryNodes(int boundarySize)
    {
        HashSet<Node> boundaryNodes = new();
        for (int x = 0; x < width; x++)
        {
            // Bottom boundary
            for (int y = 0; y < boundarySize; y++)
                boundaryNodes.Add(GetNode(x, y));

            // Top boundary
            for (int y = width - boundarySize; y < width; y++)
                boundaryNodes.Add(GetNode(x, y));
        }

        for (int y = 0; y < width; y++)
        {
            // Left boundary
            for (int x = 0; x < boundarySize; x++)
                boundaryNodes.Add(GetNode(x, y));

            // Right boundary
            for (int x = width - boundarySize; x < width; x++)
                boundaryNodes.Add(GetNode(x, y));
        }

        return boundaryNodes;
    }

    public List<Node> GetNodesInRadius(Node centerNode, int radius)
    {
        List<Node> nodesInRadius = new();
        int minX = Mathf.Max(0, centerNode.X - radius); // Clamp to grid boundaries
        int maxX = Mathf.Min(width - 1, centerNode.X + radius);
        int minY = Mathf.Max(0, centerNode.Y - radius);
        int maxY = Mathf.Min(width - 1, centerNode.Y + radius);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Node node = GetNode(x, y); // Direct access since node is guaranteed to exist
                int dx = Mathf.Abs(x - centerNode.X);
                int dy = Mathf.Abs(y - centerNode.Y);

                if (dx + dy <= radius) // Optimized distance check (Manhattan distance)
                    nodesInRadius.Add(node);
            }
        }

        return nodesInRadius;
    }
}

public class Node
{
    public Node(int x, int y, Vector3 position)
    {
        X = x;
        Y = y;
        Position = position;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public Vector3 Position { get; set; }

    public int GCost { get; set; } // Cost from start
    public int HCost { get; set; } // Estimated cost to goal
    public int FCost => GCost + HCost;

    public Node Parent { get; set; } // Node that led to this one
}

public class AStarPathfinder
{
    private static Grid grid;

    public AStarPathfinder(Grid _grid)
    {
        grid = _grid;
    }

    public List<Node> FindPath(Node start, Node goal)
    {
        List<Node> openList = new() {start};
        HashSet<Node> closedList = new();

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == goal)
                return RetracePath(start, goal);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode, neighbor);
                if (tentativeGCost >= neighbor.GCost && openList.Contains(neighbor)) continue;
                neighbor.GCost = tentativeGCost;
                neighbor.HCost = CalculateDistance(neighbor, goal);
                neighbor.Parent = currentNode;

                if (!openList.Contains(neighbor))
                    openList.Add(neighbor);
            }
        }

        return null; // No path found
    }

    private static Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowestFCostNode = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
            if (nodes[i].FCost < lowestFCostNode.FCost)
                lowestFCostNode = nodes[i];
        return lowestFCostNode;
    }

    private static List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Node n = grid.GetNode(node.X + x, node.Y + y);
                if (n != null && (x != 0 || y != 0))
                    neighbors.Add(n);
            }
        }

        return neighbors;
    }

    private static int CalculateDistance(Node a, Node b)
    {
        return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    private static List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new();
        Node currentNode = end;
        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }
}