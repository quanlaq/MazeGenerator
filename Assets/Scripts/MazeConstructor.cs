using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    public bool showDebug;

    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;

    public int[,] data
    {
        get;
        private set;
    }
    
    public float HallWidth
    {
        get; private set;
    }
    public float HallHeight
    {
        get; private set;
    }

    public int StartRow
    {
        get; private set;
    }
    public int StartCol
    {
        get; private set;
    }

    public int GoalRow
    {
        get; private set;
    }
    public int GoalCol
    {
        get; private set;
    }

    private MazeDataGenerator _dataGenerator;
    private MazeMeshGenerator _meshGenerator;

    private void Awake()
    {
        _dataGenerator = new MazeDataGenerator();
        _meshGenerator = new MazeMeshGenerator();
    }
    
    public void DisposeOldMaze()
    {
        var objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (var go in objects) {
            Destroy(go);
        }
    }

    public void GenerateNewMaze(int sizeRows, int sizeCols, TriggerEventHandler startCallback = null,
        TriggerEventHandler goalCallback = null)
    {
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        DisposeOldMaze();

        data = _dataGenerator.FromDimensions(sizeRows, sizeCols);

        FindStartPosition();
        FindGoalPosition();

        // store values used to generate this mesh
        HallWidth = _meshGenerator.width;
        HallHeight = _meshGenerator.height;

        DisplayMaze();

        PlaceStartTrigger(startCallback);
        PlaceGoalTrigger(goalCallback);
    }

    private void DisplayMaze()
    {
        var go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = _meshGenerator.FromData(data);

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};
    }
    
    private void FindStartPosition()
    {
        var maze = data;
        var rMax = maze.GetUpperBound(0);
        var cMax = maze.GetUpperBound(1);

        for (var i = 0; i <= rMax; i++)
        {
            for (var j = 0; j <= cMax; j++)
            {
                if (maze[i, j] != 0) continue;
                StartRow = i;
                StartCol = j;
                return;
            }
        }
    }

    private void FindGoalPosition()
    {
        var maze = data;
        var rMax = maze.GetUpperBound(0);
        var cMax = maze.GetUpperBound(1);

        // loop top to bottom, right to left
        for (var i = rMax; i >= 0; i--)
        {
            for (var j = cMax; j >= 0; j--)
            {
                if (maze[i, j] != 0) continue;
                GoalRow = i;
                GoalCol = j;
                return;
            }
        }
    }
    
    private void PlaceStartTrigger(TriggerEventHandler callback)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(StartCol * HallWidth, .5f, StartRow * HallWidth);
        go.name = "Start Trigger";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = startMat;

        var tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }

    private void PlaceGoalTrigger(TriggerEventHandler callback)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(GoalCol * HallWidth, .5f, GoalRow * HallWidth);
        go.name = "Treasure";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;

        var tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }

    private void OnGUI()
    {
        if (!showDebug)
        {
            return;
        }

        var maze = data;
        var rMax = maze.GetUpperBound(0);
        var cMax = maze.GetUpperBound(1);
//        print(rMax + " " + cMax);
        var str = "";

        for (var i = rMax; i >= 0; i--)
        {
            for (var j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    str += "....";
                }
                else
                {
                    str += "==";
                }
            }

            str += '\n';
        }
        GUI.Label(new Rect(20, 20, 500, 500), str);
    }
}
