using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour
{
    public float width = 0.5f;
    public BezierSpline spline;

    public float duration;

    private float progress;

    public bool lookForward;
    public Material material;

    public enum SplineWalkerMode
    {
        Once,
        Loop,
        PingPong
    }

    public SplineWalkerMode mode;

    private bool goingForward = true;

    GameObject plane;
    private void Start()
    {
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = spline.GetPoint(0);
        plane.transform.rotation = Quaternion.Euler(90, 0, 0);
        plane.name = "road";
        plane.GetComponent<MeshRenderer>().material = material;
        MeshFilter roadMeshFilter = plane.GetComponent<MeshFilter>();
        roadMeshFilter.mesh = CreateBezierMesh(0.01f, width);
    }

    private void Update()
    {
        plane.transform.position = spline.transform.position; //spline.transform.position + (spline.GetPoint(0f) - spline.transform.position);
        MeshFilter roadMeshFilter = plane.GetComponent<MeshFilter>();
        roadMeshFilter.mesh = CreateBezierMesh(0.0005f, width);
        if (goingForward)
        {
            progress += Time.deltaTime / duration;
            if (progress > 1f)
            {
                if (mode == SplineWalkerMode.Once)
                {
                    progress = 1f;
                }
                else if (mode == SplineWalkerMode.Loop)
                {
                    progress -= 1f;
                }
                else
                {
                    progress = 2f - progress;
                    goingForward = false;
                }
            }
        }
        else
        {
            progress -= Time.deltaTime / duration;
            if (progress < 0f)
            {
                progress = -progress;
                goingForward = true;
            }
        }

        Vector3 position = spline.GetPoint(progress);
        transform.localPosition = position;
        if (lookForward)
        {
            transform.LookAt(position + spline.GetDirection(progress));
        }
    }


    //
    public Mesh CreateBezierMesh(float multiplier, float roadWidth)
    {
        List<Vector2> pointsList = new List<Vector2>();


        for (float i = 0; i < 1.25f; i += multiplier)
        {
            pointsList.Add(ToVector2(spline.GetPoint(i)));
        }

        // Iterate to get the distance of startPoint and endPoint traveled by the road
        float distance = 0;
        for (int i = pointsList.Count - 1; i > 1; i--)
        {
            distance += Vector3.Distance(pointsList[i], pointsList[i - 1]);
        }

        Vector2[] points = pointsList.ToArray();
        // vertices = 2 * number of points
        // triangles = (2 * (number of points - 1) * 3) 
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if (i > 0)
            {
                forward += points[i] - points[i - 1];
            }
            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);

            verts[vertIndex] = points[i] + left * roadWidth;
            verts[vertIndex + 1] = points[i] - left * roadWidth;

            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);


            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        return mesh;
    }
    public static Vector2 ToVector2(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    public static Vector3 Quadratic(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
    }
    public static Vector2 Quadratic(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (1.0f - t) * (1.0f - t) * p0 + 2.0f * (1.0f - t) * t * p1 + t * t * p2;
    }
}