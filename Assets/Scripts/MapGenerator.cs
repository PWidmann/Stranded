﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Seed")]
    public int seed;

    [Header("Map Size")]
    public int mapWidth;
    public int mapHeight;

    [Header("Water Height")]
    public float waterHeight = 0.7f;

    [Header("Noise Values")]
    public float heightScale = 3f;
    public float frequency;
    public float amplitude;
    public float lacunarity;
    public float persistance;
    public int octaves;
    public bool useFalloff;
    public bool useFlatShading;
    public float fallOffValueA = 3;
    public float fallOffValueB = 2.2f;
    public AnimationCurve heightCurve;  

    [Header("GameObjects")]
    public GameObject waterPrefab;
    public GameObject playerPrefab;
    
    // Mesh generation
    private NavMeshSurface levelMeshSurface;
    private MeshCollider meshCollider;
    private PerlinNoise noise;
    private float[,] noiseValues;
    private float[,] falloffMap;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    // Object spawn check
    Vector3 playerSpawnPosition;
    RaycastHit hitInfo;
    Ray ray;

    private void Awake()
    {
        levelMeshSurface = GetComponent<NavMeshSurface>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        uv = new Vector2[(mapWidth + 1) * (mapHeight + 1)];

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapWidth + 1, mapHeight + 1, fallOffValueA, fallOffValueB);
        noise = new PerlinNoise(seed.GetHashCode(), frequency, amplitude, lacunarity, persistance, octaves);

        CreateTerrainMesh();
        FlatShading();
        ApplyMesh();
        levelMeshSurface.BuildNavMesh();
        meshCollider.sharedMesh = mesh;

        SpawnWater();
        SpawnPlayer();
    }

    private void Update()
    {
        
    }

    private void CreateTerrainMesh()
    {
        vertices = new Vector3[(mapWidth + 1) * (mapHeight + 1)];
        noiseValues = noise.GetNoiseValues(mapWidth*2 + 1, mapHeight*2 + 1);

        for (int x = 0; x <= mapWidth; x++)
        {
            for (int y = 0; y <= mapHeight; y++)
            {
                if (useFalloff)
                {
                    // SUBSTRACT FALLOFF MAP VALUES
                    noiseValues[x, y] = Mathf.Clamp01(noiseValues[x, y] - falloffMap[x, y]);
                }
            }
        }

        for (int i = 0, z = 0; z <= mapHeight; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                float y = Mathf.Floor(heightCurve.Evaluate(noiseValues[x, z]) * heightScale);

                
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2(x / (float)mapWidth, z / (float)mapHeight);
                i++;
            }
        }

        triangles = new int[mapWidth * mapHeight * 12];

        int vert = 0;
        int tris = 0;

        // Create triangle quads
        for (int z = 0; z < mapHeight; z+=2)
        {
            for (int x = 0; x < mapWidth; x += 2)
            {
                // Bottom triangle
                triangles[tris + 0] = vert + 0 + (z * mapWidth/2);
                triangles[tris + 1] = vert + mapWidth + 2 + (z * mapWidth / 2);
                triangles[tris + 2] = vert + 2 + (z * mapWidth / 2);

                // Right triangle
                triangles[tris + 3] = vert + 2 + (z * mapWidth / 2);
                triangles[tris + 4] = vert + mapWidth + 2 + (z * mapWidth / 2);
                triangles[tris + 5] = vert + (mapWidth + 1) * 2 + 2 + (z * mapWidth / 2);

                // Top triangle
                triangles[tris + 6] = vert + (mapWidth + 1) * 2 + 2 + (z * mapWidth / 2);
                triangles[tris + 7] = vert + mapWidth + 2 + (z * mapWidth /2);
                triangles[tris + 8] = vert + (mapWidth + 1) * 2 + 0 + (z * mapWidth / 2);

                // Left triangle
                triangles[tris + 9] = vert + (mapWidth + 1) * 2 + 0 + (z * mapWidth / 2);
                triangles[tris + 10] = vert + mapWidth + 2 + (z * mapWidth / 2);
                triangles[tris + 11] = vert + 0 + (z * mapWidth / 2);

                vert += 2;
                tris += 12;
            }
            vert += 2;
        }
    }

    private void ApplyMesh()
    {
        mesh.Clear();

        SmoothRectangularEdges();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
    }

    void FlatShading()
    {
        // Duplicate vertices for each triangle to prevent smooth edges
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uv[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uv = flatShadedUvs;
    }

    void SmoothRectangularEdges()
    {
        Vector3[] rectBatch = new Vector3[12];

        for (int v = 0; v < vertices.Length; v+=12)
        {
            for (int i = 0; i < 12; i++)
            {
                rectBatch[i] = vertices[i + v];
            }

            //North/South edge
            if (rectBatch[0].y == rectBatch[2].y && rectBatch[0].y != rectBatch[8].y && rectBatch[8].y == rectBatch[5].y)
            {
                // Put middlepoints of quad to average height between the two heights

                float y = (vertices[v + 0].y + vertices[v + 8].y) / 2;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
            }

            //East/West edge
            if (rectBatch[0].y == rectBatch[8].y && rectBatch[2].y == rectBatch[5].y && rectBatch[0].y != rectBatch[2].y)
            {
                // Put middlepoints of quad to average height between the two heights

                float y = (vertices[v + 0].y + vertices[v + 2].y) / 2;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
            }
        }
    }

    void SpawnWater()
    {
        Instantiate(waterPrefab, new Vector3(mapWidth/2, waterHeight, mapHeight/2), Quaternion.identity);
    }

    void SpawnPlayer()
    {
        //Look for possible spawn position in Z direction from map border
        Vector3 checkPosition = new Vector3(mapWidth/2, 5f, 0);

        for (int z = 0; z < mapHeight; z++)
        {
            checkPosition.z = z;

            if (Physics.Raycast(checkPosition, Vector3.down, out hitInfo, maxDistance: 50))
            {
                if (hitInfo.collider.CompareTag("Terrain"))
                {
                    // When you find land, add one unit and instantiate player
                    checkPosition.z = z + 1;
                    playerSpawnPosition = new Vector3(checkPosition.x, 1, checkPosition.z);
                    Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
                    break;
                }
            }
        }

        
    }
}
