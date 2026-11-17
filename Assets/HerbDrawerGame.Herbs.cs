using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed partial class HerbDrawerGame
{
    private enum HerbForm { Berry, Date, Seed, Tuber, Slice, Chunk, Twig, Bark, Peel }

    private readonly List<Mesh> herbMeshes = new List<Mesh>();
    private readonly List<Texture2D> herbTextures = new List<Texture2D>();

    private void BuildHerbPile(Transform tray, string herb)
    {
        (HerbForm form, Color face, Color edge, Vector3 size) = herb switch
        {
            "枸杞" => (HerbForm.Berry, Tone(183, 57, 32), Tone(112, 31, 20), new Vector3(.105f, .038f, .049f)),
            "人參" => (HerbForm.Slice, Tone(229, 202, 150), Tone(172, 135, 86), new Vector3(.155f, .024f, .087f)),
            "當歸" => (HerbForm.Slice, Tone(207, 166, 99), Tone(130, 91, 52), new Vector3(.161f, .027f, .091f)),
            "黃耆" => (HerbForm.Slice, Tone(224, 194, 120), Tone(158, 118, 64), new Vector3(.195f, .023f, .064f)),
            "甘草" => (HerbForm.Slice, Tone(211, 181, 111), Tone(108, 64, 43), new Vector3(.088f, .026f, .079f)),
            "陳皮" => (HerbForm.Peel, Tone(169, 88, 41), Tone(210, 168, 107), new Vector3(.186f, .040f, .122f)),
            "紅棗" => (HerbForm.Date, Tone(119, 39, 27), Tone(69, 23, 17), new Vector3(.134f, .078f, .089f)),
            "白朮" => (HerbForm.Slice, Tone(222, 198, 153), Tone(141, 104, 79), new Vector3(.145f, .033f, .117f)),
            "茯苓" => (HerbForm.Chunk, Tone(231, 217, 186), Tone(178, 165, 140), new Vector3(.105f, .060f, .096f)),
            "川芎" => (HerbForm.Slice, Tone(200, 173, 117), Tone(112, 83, 58), new Vector3(.123f, .031f, .111f)),
            "白芍" => (HerbForm.Slice, Tone(224, 209, 177), Tone(182, 159, 132), new Vector3(.104f, .025f, .090f)),
            "熟地" => (HerbForm.Chunk, Tone(63, 42, 31), Tone(35, 22, 20), new Vector3(.128f, .048f, .103f)),
            "桂枝" => (HerbForm.Twig, Tone(123, 71, 45), Tone(183, 128, 79), new Vector3(.177f, .028f, .035f)),
            "桔梗" => (HerbForm.Slice, Tone(225, 205, 155), Tone(154, 120, 79), new Vector3(.147f, .031f, .074f)),
            "杜仲" => (HerbForm.Bark, Tone(98, 64, 56), Tone(172, 142, 117), new Vector3(.173f, .036f, .083f)),
            "薏仁" => (HerbForm.Seed, Tone(229, 214, 180), Tone(110, 82, 59), new Vector3(.063f, .034f, .046f)),
            "麥冬" => (HerbForm.Tuber, Tone(216, 182, 112), Tone(155, 120, 72), new Vector3(.109f, .037f, .046f)),
            "丹參" => (HerbForm.Twig, Tone(130, 65, 48), Tone(182, 139, 88), new Vector3(.149f, .042f, .052f)),
            _ => throw new System.ArgumentException("Missing herb appearance", nameof(herb))
        };

        Texture2D texture = HerbFinish(herb, form, face, edge);
        Material material = Finish(Color.white, texture);
        material.SetFloat("_Smoothness", form == HerbForm.Date || herb == "熟地" ? .27f : .10f);
        Mesh mesh = HerbMesh(herb, form);
        int columns = form == HerbForm.Seed || form == HerbForm.Berry ? 6 : 4;
        int rows = columns == 6 ? 4 : 3;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = row * columns + column;
                float variance = Mathf.PerlinNoise(index * .72f, .36f);
                Vector3 position = new Vector3((column - (columns - 1) * .5f) * (.47f / (columns - 1))
                    + Mathf.Sin(index * 5.1f) * .018f, .055f + size.y * .46f + variance * .022f,
                    (row - (rows - 1) * .5f) * (.38f / (rows - 1)) + Mathf.Cos(index * 3.6f) * .019f);
                Transform piece = Group(herb, tray, position);
                piece.localScale = size * Mathf.Lerp(.78f, 1.21f, variance);
                piece.localRotation = Quaternion.Euler(variance * 20 - 10, index * 137.5f, variance * 17 - 9);
                piece.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer renderer = piece.gameObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }
    }

    private Mesh HerbMesh(string herb, HerbForm form)
    {
        const int rings = 20;
        const int sides = 28;
        int stride = sides + 1;
        var vertices = new Vector3[(rings + 1) * stride];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[rings * sides * 6];
        int triangle = 0;
        for (int ring = 0; ring <= rings; ring++)
        {
            float v = (float)ring / rings;
            float theta = v * Mathf.PI;
            for (int side = 0; side <= sides; side++)
            {
                float u = (float)side / sides;
                float angle = u * 2 * Mathf.PI;
                Vector3 point = new Vector3(Mathf.Sin(theta) * Mathf.Cos(angle), Mathf.Cos(theta),
                    Mathf.Sin(theta) * Mathf.Sin(angle));
                float wrinkle = Mathf.Sin(angle * 9 + theta * 4) * Mathf.Sin(theta * 12 + angle * 3);
                float uneven = 1 + .057f * Mathf.Sin(angle * 5 + theta * 2) + .031f * Mathf.Cos(angle * 11 - theta * 3);
                switch (form)
                {
                    case HerbForm.Berry:
                    case HerbForm.Date:
                        point *= 1 + wrinkle * .047f + .03f * Mathf.Cos(theta * 15);
                        break;
                    case HerbForm.Tuber:
                        point.y *= Mathf.Lerp(.78f, 1.08f, 1 - Mathf.Abs(point.x));
                        point *= 1 + .034f * wrinkle;
                        break;
                    case HerbForm.Seed:
                        point.y -= Mathf.Exp(-point.z * point.z * 56) * Mathf.Max(0, point.y) * .28f;
                        break;
                    case HerbForm.Slice:
                        float radius = Mathf.Pow(Mathf.Max(0, Mathf.Sin(theta)), .56f) * uneven;
                        point = new Vector3(Mathf.Cos(angle) * radius, SignedPower(point.y, .31f), Mathf.Sin(angle) * radius);
                        point.y += .10f * Mathf.Sin(point.x * 4 + point.z * 3);
                        break;
                    case HerbForm.Chunk:
                        point = new Vector3(SignedPower(point.x, .48f), SignedPower(point.y, .41f), SignedPower(point.z, .53f));
                        point *= 1 + wrinkle * (herb == "熟地" ? .13f : .065f);
                        break;
                    case HerbForm.Twig:
                        point = new Vector3(SignedPower(point.x, .45f), point.y * .83f, point.z);
                        point.y += .18f * Mathf.Sin(point.x * 2);
                        break;
                    case HerbForm.Bark:
                    case HerbForm.Peel:
                        point.x = SignedPower(point.x, form == HerbForm.Bark ? .49f : .76f) * uneven;
                        point.z *= uneven;
                        point.y = point.y * .32f + (point.x * point.x * 1.35f + point.z * point.z * .64f)
                            * (form == HerbForm.Peel ? 1.1f : .48f) - .55f;
                        break;
                }
                int current = ring * stride + side;
                vertices[current] = point * .5f;
                uv[current] = form == HerbForm.Slice ? new Vector2(point.x, point.z) * .45f + Vector2.one * .5f : new Vector2(u, v);
                if (ring == rings || side == sides)
                    continue;
                triangles[triangle++] = current;
                triangles[triangle++] = current + 1;
                triangles[triangle++] = current + stride;
                triangles[triangle++] = current + 1;
                triangles[triangle++] = current + stride + 1;
                triangles[triangle++] = current + stride;
            }
        }
        var mesh = new Mesh { name = herb, vertices = vertices, uv = uv, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        herbMeshes.Add(mesh);
        return mesh;
    }

    private Texture2D HerbFinish(string herb, HerbForm form, Color face, Color edge)
    {
        const int resolution = 96;
        var pixels = new Color32[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / (resolution - 1);
                float v = (float)y / (resolution - 1);
                float grain = Mathf.PerlinNoise(u * 22, v * 19);
                float fleck = Mathf.PerlinNoise(u * 82 + 8, v * 84);
                float stripe = Mathf.PerlinNoise(u * 38, v * 6 + grain * .55f);
                float amount;
                switch (form)
                {
                    case HerbForm.Slice:
                        float radius = Vector2.Distance(new Vector2(u, v), Vector2.one * .5f);
                        float rings = .5f + .5f * Mathf.Sin(radius * 86 + grain * 2.1f);
                        float angle = Mathf.Atan2(v - .5f, u - .5f);
                        float fibers = Mathf.Pow(.5f + .5f * Mathf.Sin(angle * 21 + radius * 20), 6);
                        amount = Mathf.Clamp01(.05f + rings * .16f + fibers * .15f + Mathf.SmoothStep(.0f, .85f, Mathf.InverseLerp(.36f, .47f, radius)));
                        break;
                    case HerbForm.Peel:
                        amount = v > .52f ? .85f + fleck * .15f : .1f + Mathf.Pow(fleck, 3) * .49f;
                        break;
                    case HerbForm.Bark:
                        amount = stripe > .73f ? .88f : .16f + grain * .22f;
                        break;
                    case HerbForm.Twig:
                        amount = .14f + stripe * .34f + Mathf.Pow(fleck, 4) * .30f;
                        break;
                    case HerbForm.Seed:
                        amount = .08f + Mathf.Pow(Mathf.Abs(Mathf.Cos(u * Mathf.PI * 2)), 28) * .68f;
                        break;
                    default:
                        float folds = Mathf.Pow(.5f + .5f * Mathf.Sin(u * 71 + v * 12 + grain * 3), 4);
                        amount = Mathf.Clamp01(.08f + folds * .36f + grain * .21f);
                        break;
                }
                Color color = Color.Lerp(face, edge, amount) * (.87f + .2f * fleck);
                color.a = 1;
                pixels[y * resolution + x] = color;
            }
        }
        var texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, true) { name = herb + " surface" };
        texture.SetPixels32(pixels);
        texture.Apply(true, true);
        herbTextures.Add(texture);
        return texture;
    }

    private static float SignedPower(float value, float power)
    {
        return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), power);
    }

    private static Color Tone(byte red, byte green, byte blue)
    {
        return new Color32(red, green, blue, 255);
    }
}
