using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [Header("Prefabs")] public GameObject playerPrefab;

    public GameObject wallPrefab;
    public Creature[] enemyPrefabs;

    [Header("Map Settings")] public int mapWidth = 50;

    public float cellSize = 1;
    public int center_area_size = 10;
    public int min_nest_size = 5;
    public int max_nest_size = 10;
    public float min_wall_scale = 2;
    public float max_wall_scale = 4;
    public int min_wall_distance = 2;
    public int max_wall_distance = 4;
    public int min_path_width = 2;
    public int max_path_width = 4;
    public int cave_boundary_size = 2;

    [Header("Enemy Settings")] public int num_of_nests = 3;

    public int min_num_of_enemies = 5;
    public int max_num_of_enemies = 10;

    private Grid grid;

    private static void BakeNavMesh(Transform root)
    {
        var navMeshSurface = root.gameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.All;
        // Ground layer and Obstacle layer
        int groundLayer = LayerMask.NameToLayer("Ground");
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        navMeshSurface.layerMask = (1 << groundLayer) | (1 << obstacleLayer);
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.BuildNavMesh();
    }

    public void Generate()
    {
        // Clear the map
        if (GameObject.Find("Map"))
            DestroyImmediate(GameObject.Find("Map"));

        Transform root = new GameObject("Map").transform;

        grid = new Grid(mapWidth, cellSize);

        List<Node> caveNodes = new List<Node>();
        List<Node> open_field_nodes = new List<Node>();
        if (caveNodes == null) throw new ArgumentNullException("Nodes are null");
        caveNodes.AddRange(grid.nodes);

        Node centerNode = grid.GetNode(mapWidth / 2, mapWidth / 2);
        List<Node> centerArea = grid.GetNodesInRadius(centerNode, center_area_size);
        // Remove center area from open nodes
        foreach (Node node in centerArea)
        {
            caveNodes.Remove(node);
            open_field_nodes.AddUnique(node);
        }

        // RemoveBoundaryNodes
        HashSet<Node> boundaryNodes = grid.GetBoundaryNodes(max_nest_size + 2);
        foreach (Node node in boundaryNodes) caveNodes.Remove(node);

        // Create nests
        var nests = new List<Nest>();
        for (int i = 0; i < num_of_nests; i++)
        {
            int nestSize = Random.Range(min_nest_size, max_nest_size);
            Node nestNode = caveNodes[Random.Range(0, caveNodes.Count)];
            var nestObject = new GameObject("Nest " + i)
            {
                transform =
                {
                    position = nestNode.Position,
                    parent = root
                }
            };
            nests.Add(nestObject.AddComponent<Nest>());
            nests[i].Initialize(nestSize, nestNode.Position, cellSize);
            List<Node> nestNodes = grid.GetNodesInRadius(nestNode, nestSize);
            foreach (Node node in nestNodes)
            {
                caveNodes.Remove(node);
                open_field_nodes.AddUnique(node);
                List<Node> neighbors = grid.GetNodesInRadius(node, cave_boundary_size + max_nest_size);
                foreach (Node neighbor in neighbors) caveNodes.Remove(neighbor);
            }

            if (caveNodes.Count <= 0)
            {
                Debug.LogWarning("No valid nodes for nest, " + i + " out of " + num_of_nests + " nests created");
                break;
            }
        }

        var pathfinder = new AStarPathfinder(grid);
        // Create paths
        foreach (Nest nest in nests)
        {
            List<Node> path = pathfinder.FindPath(grid.GetNode(nest.Center), centerNode);
            if (path == null)
            {
                Debug.LogWarning("No path found for nest");
                continue;
            }

            foreach (Node node in path)
            {
                List<Node> nodes_in_radius = grid.GetNodesInRadius(node, Random.Range(min_path_width, max_path_width));
                foreach (Node n in nodes_in_radius) open_field_nodes.AddUnique(n);
            }
        }

        // Spawn walls
        HashSet<Node> boundary_nodes = new HashSet<Node>();
        foreach (Node node in open_field_nodes)
        {
            List<Node> neighbors = grid.GetNodesInRadius(node, 1);
            foreach (Node neighbor in neighbors)
                if (!open_field_nodes.Contains(neighbor))
                {
                    int wall_distance = Random.Range(min_wall_distance, max_wall_distance);
                    List<Node> nodes_in_radius = grid.GetNodesInRadius(node, wall_distance);
                    foreach (Node n in nodes_in_radius)
                        if (!open_field_nodes.Contains(n))
                            boundary_nodes.Add(n);
                    break;
                }
        }

        foreach (Node node in boundary_nodes)
        {
            float height = Random.Range(min_wall_scale, max_wall_scale) * cellSize;
            var scale = new Vector3(cellSize, height, cellSize);
            Instantiate(wallPrefab, node.Position, Quaternion.identity, root).transform.localScale = scale;
        }

        BakeNavMesh(root);

        // Spawn enemies
        SpawnEnemies(nests, root);
    }

    private void SpawnEnemies(List<Nest> nests, Transform root)
    {
        foreach (Nest nest in nests)
        {
            int num_of_enemies = Random.Range(min_num_of_enemies, max_num_of_enemies);
            Creature enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            List<Vector3> pointsInCircle = new();
            float circleCellSize = 2 / Mathf.Sqrt(2); // Diagonal of cell = safeDistance

            for (int i = 0; i < num_of_enemies; i++)
            {
                Vector3 pointInCircle;
                bool validPoint;

                do
                {
                    float angle = 2f * Mathf.PI * i / num_of_enemies;
                    float distance = Random.Range(0f, nest.Radius);
                    pointInCircle = transform.position + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

                    var cell = new Vector2(Mathf.FloorToInt(pointInCircle.x / circleCellSize), Mathf.FloorToInt(pointInCircle.z / circleCellSize));
                    var jitter = new Vector3(Random.Range(-circleCellSize / 2f, circleCellSize / 2f), 0f,
                        Random.Range(-circleCellSize / 2f, circleCellSize / 2f));
                    pointInCircle = new Vector3(cell.x * circleCellSize, 0f, cell.y * circleCellSize) + jitter;

                    validPoint = true;
                    foreach (Vector3 existingPoint in pointsInCircle)
                        if (Vector3.Distance(pointInCircle, existingPoint) < 3)
                        {
                            validPoint = false;
                            break;
                        }
                } while (!validPoint);

                pointsInCircle.Add(pointInCircle);
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Vector3 spawnPosition = nest.Center + pointInCircle;
#if UNITY_EDITOR
                var enemy = (Creature) PrefabUtility.InstantiatePrefab(enemyPrefab, root);
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = rotation;
                enemy.gameObject.SetActive(false);
#else
                var enemy = Instantiate(enemyPrefab, spawnPosition, rotation, root);
                enemy.gameObject.SetActive(false);
#endif
                enemy.SetNest(nest);
                nest.AddCreature(enemy);
            }
        }
    }
}