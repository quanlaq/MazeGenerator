using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeMeshGenerator
{
    public float width;
    public float height;

    public MazeMeshGenerator()
    {
        width = 3.75f;
        height = 3.5f;
    }

    public Mesh FromData(int[,] data)
    {
        var maze = new Mesh();

        var newVertices = new List<Vector3>();
        var newUVs = new List<Vector2>();

        maze.subMeshCount = 2;
        var floorTriangles = new List<int>();
        var wallTriangles = new List<int>();

        var rMax = data.GetUpperBound(0);
        var cMax = data.GetUpperBound(1);
        var halfH = height * .5f;

        //4
        for (var i = 0; i <= rMax; i++)
        {
            for (var j = 0; j <= cMax; j++)
            {
                if (data[i, j] == 1) continue;
                // floor
                AddQuad(
                    Matrix4x4.TRS(new Vector3(j * width, 0, i * width), Quaternion.LookRotation(Vector3.up),
                        new Vector3(width, width, 1)), ref newVertices, ref newUVs, ref floorTriangles);

                // ceiling
                AddQuad(
                    Matrix4x4.TRS(new Vector3(j * width, height, i * width), Quaternion.LookRotation(Vector3.down),
                        new Vector3(width, width, 1)), ref newVertices, ref newUVs, ref floorTriangles);

                // walls on sides next to blocked grid cells

                if (i - 1 < 0 || data[i - 1, j] == 1)
                {
                    AddQuad(
                        Matrix4x4.TRS(new Vector3(j * width, halfH, (i - .5f) * width),
                            Quaternion.LookRotation(Vector3.forward), new Vector3(width, height, 1)),
                        ref newVertices, ref newUVs, ref wallTriangles);
                }

                if (j + 1 > cMax || data[i, j + 1] == 1)
                {
                    AddQuad(
                        Matrix4x4.TRS(new Vector3((j + .5f) * width, halfH, i * width),
                            Quaternion.LookRotation(Vector3.left), new Vector3(width, height, 1)), ref newVertices,
                        ref newUVs, ref wallTriangles);
                }

                if (j - 1 < 0 || data[i, j - 1] == 1)
                {
                    AddQuad(
                        Matrix4x4.TRS(new Vector3((j - .5f) * width, halfH, i * width),
                            Quaternion.LookRotation(Vector3.right), new Vector3(width, height, 1)), ref newVertices,
                        ref newUVs, ref wallTriangles);
                }

                if (i + 1 > rMax || data[i + 1, j] == 1)
                {
                    AddQuad(
                        Matrix4x4.TRS(new Vector3(j * width, halfH, (i + .5f) * width),
                            Quaternion.LookRotation(Vector3.back), new Vector3(width, height, 1)), ref newVertices,
                        ref newUVs, ref wallTriangles);
                }
            }
        }

        maze.vertices = newVertices.ToArray();
        maze.uv = newUVs.ToArray();

        maze.SetTriangles(floorTriangles.ToArray(), 0);
        maze.SetTriangles(wallTriangles.ToArray(), 1);

        //5
        maze.RecalculateNormals();

        return maze;
    }

//1, 2
    private void AddQuad(Matrix4x4 matrix, ref List<Vector3> newVertices, ref List<Vector2> newUVs,
        ref List<int> newTriangles)
    {
        var index = newVertices.Count;

        // corners before transforming
        var vert1 = new Vector3(-.5f, -.5f, 0);
        var vert2 = new Vector3(-.5f, .5f, 0);
        var vert3 = new Vector3(.5f, .5f, 0);
        var vert4 = new Vector3(.5f, -.5f, 0);

        newVertices.Add(matrix.MultiplyPoint3x4(vert1));
        newVertices.Add(matrix.MultiplyPoint3x4(vert2));
        newVertices.Add(matrix.MultiplyPoint3x4(vert3));
        newVertices.Add(matrix.MultiplyPoint3x4(vert4));

        newUVs.Add(new Vector2(1, 0));
        newUVs.Add(new Vector2(1, 1));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(0, 0));

        newTriangles.Add(index + 2);
        newTriangles.Add(index + 1);
        newTriangles.Add(index);

        newTriangles.Add(index + 3);
        newTriangles.Add(index + 2);
        newTriangles.Add(index);
    }
}