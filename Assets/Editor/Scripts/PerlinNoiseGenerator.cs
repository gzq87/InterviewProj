using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.IO;
using System.Drawing;
using Unity.Mathematics;

public class PerlinNoiseGenerator
{
    private static int[] p = new int[512];
    private static double[] fadeTable = new double[256];

    public PerlinNoiseGenerator()
    {
        Init();
    }

    private void Init()
    {
        System.Random rand = new System.Random(1); // 使用固定种子以获得可重复的结果
        for (int i = 0; i < 256; i++)
        {
            p[i] = i;
            fadeTable[i] = Fade(i / 255.0);
        }
        for (int i = 0; i < 256; i++)
        {
            int j = rand.Next(256);
            int k = p[i];
            p[i] = p[j];
            p[j] = k;
        }
        for (int i = 0; i < 256; i++)
        {
            p[i + 256] = p[i];
        }
    }

    private double Fade(double t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10); // 6t^5 - 15t^4 + 10t^3
    }

    private double Lerp(double t, double a, double b)
    {
        return a + t * (b - a);
    }

    private double Grad(int hash, double x, double y)
    {
        int h = hash & 15; // 取低四位
        double u = h < 8 ? x : y,
               v = h < 4 ? y : -x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public double Noise(double x, double y)
    {
        int X = (int)Math.Floor(x) & 255, // 求模256
            Y = (int)Math.Floor(y) & 255;
        x -= Math.Floor(x);
        y -= Math.Floor(y);
        double u = fadeTable[(int)(x * 255)],
               v = fadeTable[(int)(y * 255)];
        int A = p[X] + Y, AA = p[A], AB = p[A + 1],
            B = p[X + 1] + Y, BA = p[B], BB = p[B + 1];

        return Lerp(v, Lerp(u, Grad(p[AA], x, y), Grad(p[BA], x - 1, y)),
                           Lerp(u, Grad(p[AB], x, y - 1), Grad(p[BB], x - 1, y - 1)));
    }

    public Texture2D GeneratePerlinNoiseImage(int width = 512, int height = 512, double scale = 0.01)
    {
        Color32[] colors = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double nx = x * scale;
                double ny = y * scale;
                double value = (this.Noise(nx, ny) + 1) / 2.0; // 将噪声值映射到 [0, 1] 范围
                Color32 color = new Color32((byte)(value * 255), (byte)(value * 255), (byte)(value * 255), 255);
                colors[x + y * width] = color;
            }
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels32(colors);
        texture.Apply();
        return texture;
    }

    [MenuItem("Tools/GeneratePerlinNoiseImage")]
    public static void Generate() {
        int width = 1024;
        int height = 1024;
        double scale = 0.01;


        var generator = new PerlinNoiseGenerator();
        Texture2D noiseImage = generator.GeneratePerlinNoiseImage(width, height, scale);
        byte[] bytesPNG = noiseImage.EncodeToPNG();
        var path = Application.dataPath + "/PerlinNoise.png";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllBytes(path, bytesPNG);
        AssetDatabase.Refresh();
    }
}