﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    
    [Header("Map Seed")]
    public int seed;

    [Header("Noise Values")]
    public float heightScale = 3f;
    public float frequency;
    public float amplitude;

    public float lacunarity;
    public float persistance;

    public int octaves;

    public bool useFalloff;
    public float fallOffValueA = 3;
    public float fallOffValueB = 2.2f;
    float[,] falloffMap;
    public AnimationCurve heightCurve;


    PerlinNoise noise;
    float[,] noiseValues;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;

    private void Awake()
    {
        
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        uv = new Vector2[(mapWidth + 1) * (mapHeight + 1)];
    }

    private void Update()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapWidth + 1, mapHeight + 1, fallOffValueA, fallOffValueB);
        noise = new PerlinNoise(seed.GetHashCode(), frequency, amplitude, lacunarity, persistance, octaves);

        CreateMap();
        UpdateMesh();
    }

    private void CreateMap()
    {
        vertices = new Vector3[(mapWidth + 1) * (mapHeight + 1)];
        noiseValues = noise.GetNoiseValues(mapWidth + 1, mapHeight + 1);

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
                float y = heightCurve.Evaluate(noiseValues[x, z]) * heightScale;
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2(x / (float)mapWidth, z / (float)mapHeight);
                i++;
            }
        }

        triangles = new int[mapWidth * mapHeight * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + mapWidth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + mapWidth + 1;
                triangles[tris + 5] = vert + mapWidth + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
    }
}
