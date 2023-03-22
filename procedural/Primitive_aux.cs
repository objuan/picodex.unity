namespace ProceduralPrimitives
{
    using System;
	using System.Collections.Generic;
	using UnityEngine;

    /// <summary>
    /// Defines static creation methods that procedurally create 3D primitive shapes like spheres, boxes, cones, planes, and cylinders.
    /// 
    /// DEPRECATED!
    /// 
    /// Please use the extension methods in MeshExtensions and GameObjectExtensions to programatically create primitives now.
    /// This is being left in as a reference for how things used to work, and to aid in backwards compatibility with old versions.
    /// </summary>
    public static partial class Primitive
    {
           public static Mesh CreateCylinderSideMesh(float bottomRadius, float topRadius, float length, int slices, int stacks)
        {
            // if both the top and bottom have a radius of zero, just return null, because invalid
            if (bottomRadius <= 0 && topRadius <= 0)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            mesh.name = "CylinderMesh";
            float sliceStep = (float) Math.PI*2.0f/slices;
            float heightStep = length/stacks;
            float radiusStep = (topRadius - bottomRadius)/stacks;
            float currentHeight = -length/2;
            int vertexCount = (stacks + 1)*slices + 2; //cone = stacks * slices + 1
            int triangleCount = (stacks + 1)*slices*2; //cone = stacks * slices * 2 + slices
            int indexCount = triangleCount*3;
            float currentRadius = bottomRadius;

            Vector3[] cylinderVertices = new Vector3[vertexCount];
            Vector3[] cylinderNormals = new Vector3[vertexCount];
            Vector2[] cylinderUVs = new Vector2[vertexCount];

            // Start at the bottom of the cylinder            
            int currentVertex = 0;
            cylinderVertices[currentVertex] = new Vector3(0, currentHeight, 0);
            cylinderNormals[currentVertex] = Vector3.down;
            currentVertex++;
            for (int i = 0; i <= stacks; i++)
            {
                float sliceAngle = 0;
                for (int j = 0; j < slices; j++)
                {
                    float x = currentRadius*(float) Math.Cos(sliceAngle);
                    float y = currentHeight;
                    float z = currentRadius*(float) Math.Sin(sliceAngle);

                    Vector3 position = new Vector3(x, y, z);
                    cylinderVertices[currentVertex] = position;
                    cylinderNormals[currentVertex] = Vector3.Normalize(position);
                    cylinderUVs[currentVertex] =
                        new Vector2((float) (Math.Sin(cylinderNormals[currentVertex].x)/Math.PI + 0.5f),
                            (float) (Math.Sin(cylinderNormals[currentVertex].y)/Math.PI + 0.5f));

                    currentVertex++;

                    sliceAngle += sliceStep;
                }
                currentHeight += heightStep;
                currentRadius += radiusStep;
            }
            cylinderVertices[currentVertex] = new Vector3(0, length/2, 0);
            cylinderNormals[currentVertex] = Vector3.up;
            currentVertex++;

            mesh.vertices = cylinderVertices;
            mesh.normals = cylinderNormals;
            mesh.uv = cylinderUVs;
            //  mesh.triangles = CreateIndexBuffer(vertexCount, indexCount, slices);

            int[] indices = new int[indexCount];
            int currentIndex = 0;

            // Middle sides of shape
            for (int i = 1; i < vertexCount - slices - 1; i++)
            {
                indices[currentIndex++] = i + slices;
                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;

                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;
            }
            mesh.triangles = indices;

            return mesh;
        }

        public static Mesh PolyMesh( float radius, int n)
        {
            //polyCollider = GetComponent<PolygonCollider2D>();

            // MeshFilter mf = GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
           // mf.mesh = mesh;

            //verticies
            List<Vector3> verticiesList = new List<Vector3> { };
            float x;
            float y;
            for (int i = 0; i < n; i++)
            {
                x = radius * Mathf.Sin((2 * Mathf.PI * i) / n);
                y = radius * Mathf.Cos((2 * Mathf.PI * i) / n);
                verticiesList.Add(new Vector3(x, y, 0f));
            }
            Vector3[] verticies = verticiesList.ToArray();

            //triangles
            List<int> trianglesList = new List<int> { };
            for (int i = 0; i < (n - 2); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + 2);
            }
            int[] triangles = trianglesList.ToArray();

            //normals
            List<Vector3> normalsList = new List<Vector3> { };
            for (int i = 0; i < verticies.Length; i++)
            {
                normalsList.Add(-Vector3.forward);
            }
            Vector3[] normals = normalsList.ToArray();

            //initialise
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.normals = normals;

            return mesh;
            //polyCollider
            //polyCollider.pathCount = 1;

            //List<Vector2> pathList = new List<Vector2> { };
            //for (int i = 0; i < n; i++)
            //{
            //    pathList.Add(new Vector2(verticies[i].x, verticies[i].y));
            //}
            //Vector2[] path = pathList.ToArray();

            //polyCollider.SetPath(0, path);
        }
    }
}