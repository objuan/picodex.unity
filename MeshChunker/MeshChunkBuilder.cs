using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace brickgame
{

    public class MeshChunkMaterial
    {
    }

    public enum CellColorType
    {
        Solid,
        SubL1
    }
    public class CellColorFace
    {
        public int index;
        public int[] face_idx = new int[4];
        public Color[] face = new Color[4];
    }

    /// <summary>
    /// [ 2  3 ]  [ 6  7 ]
    /// [ 0  1 ]  [ 4  5 ]
    /// </summary>
    public class CellColor
    {
        // ordine come la mesh
        public static int[,] _faces = new int[,] {
            { 0, 1, 2, 3 }, // front
            { 5, 4, 7, 6 }, // back
            { 2,3,6,7 }, // top
            { 0,1,4,5}, // bottom
            { 4,0,6,2 }, // left
            { 1,5,3,7 } // right
        };
        public CellColorType type;
        public Color[] colors;
        public bool isSubContantColor;
        public bool isSolidColor => colors.Length == 1;
        public Vector2? userValue;

        public Color mainColor => colors[0];

        public float alfa
        {
            get { return colors[0].a; }
            set { colors[0].a = value; }
        }

        CellColorFace[] faces;

        public CellColor(Color color)
        {
            colors = new Color[] { color };
            type = CellColorType.Solid;
        }

        public CellColor(CellColorType type)
        {
            this.type = type;
            if (type == CellColorType.SubL1)
            {
                colors = new Color[8];
                faces = new CellColorFace[6];
                Rebuild();
            }
        }

        public void Rebuild()
        {
            for (int f = 0; f < 6; f++)
            {
                faces[f] = new CellColorFace();
                for (int i=0;i<4;i++)
                {
                    var idx = _faces[f, i];
                    faces[f].index = f;
                    faces[f].face_idx[i] = idx;
                    faces[f].face[i] = colors[idx];
                }
            }
            var c = colors[0];
            isSubContantColor = colors.Count(X => X.Equals(c)) == 8;
        }
        //public void SetColor(iVector3 pos,Color color)
        //{
        //    int idx = pos.x + pos.y *2 + pos.z * 4;
        //    colors[idx] = color;
        //    Rebuild();
        //}
        public void SetColor(int x,int y,int z, Color color)
        {
            int idx = x + y * 2 + z * 4;
            colors[idx] = color;
           // Rebuild();
        }


        /// <summary>
        /// front, back , top, bottom , left, right
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
		public CellColorFace GetFace(int index)
		{
            return faces[index];
        }
    }

    public struct TileInfoFace
    {
        // 6 facce
        public Vector2 offset;
        public Vector2 scale;

    }
    
    public class TileInfo
    {
        public Vector2 main_offset;
        public Vector2 main_scale;
        public Vector2? userValues;

        // 6 facce
        public TileInfoFace[] faces;

    }

    public interface IMaterialSelector
    {
        Material GetMaterial(bool isSolid,bool isBorder);

        // offset + scale
        TileInfo GetMaterialTile(MeshChunkMaterial mat,out Color color);

        void OnSubMaterialsChanged();
    }

    // ======================================================

    public class MeshChunkBuilder : MonoBehaviour
    {
        public class ChunkEntry
        {
            public iVector3 pos;
            public GameObject go;
            public MeshChunk chunk;
            public Vector3 chunk_offset;
            public MultiMesh multiMesh;
            public bool hasMaterial = false;

            public bool cutMode = false;

            public bool mustBuild = true;
        }


        [NonSerialized]
        public iVector3 size;

        [NonSerialized]
        public MeshWorld world;

        [NonSerialized]
        public IMaterialSelector materialSelector;


        public Vector3 localToWorldOffset;

        public Vector3 cellWorldSize;


        //  [NonSerialized]
        public Vector3 chunkWorldSize;

        [NonSerialized]
        public static MeshSource source;

        [NonSerialized]
        public GameObject root;


        public int totalTriangles => chunkList.Sum(X => X.multiMesh.faceLayer_solid.tris.Count +
             ((X.multiMesh.faceLayer_trasp!=null) ? X.multiMesh.faceLayer_trasp.tris.Count : 0));

       // Dictionary<iVector3, ChunkEntry> rebuildChunkList = new Dictionary<iVector3, ChunkEntry>();

        Vector3 lastLeafSize;
        // Dictionary<iVector3, ChunkEntry> chunkList = new Dictionary<iVector3, ChunkEntry>();
        ChunkEntry[] chunkList;

        public Action onInitialized;
        public Action onBeforeRender;

        #region SOURCE

        public static Mesh CreateBoxMesh1(float width, float height, float depth)
        {
            Mesh mesh = new Mesh();
            mesh.name = "BoxMesh";

            // Because the box is centered at the origin, need to divide by two to find the + and - offsets
            width = width / 2.0f;
            height = height / 2.0f;
            depth = depth / 2.0f;

            Vector3[] boxVertices = new Vector3[36];
            Vector3[] boxNormals = new Vector3[36];
            Vector2[] boxUVs = new Vector2[36];

            Vector3 topLeftFront = new Vector3(-width, height, depth);
            Vector3 bottomLeftFront = new Vector3(-width, -height, depth);
            Vector3 topRightFront = new Vector3(width, height, depth);
            Vector3 bottomRightFront = new Vector3(width, -height, depth);

            Vector3 topLeftBack = new Vector3(-width, height, -depth);
            Vector3 topRightBack = new Vector3(width, height, -depth);
            Vector3 bottomLeftBack = new Vector3(-width, -height, -depth);
            Vector3 bottomRightBack = new Vector3(width, -height, -depth);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            // Front face.
            boxVertices[0] = topLeftFront;
            boxNormals[0] = frontNormal;
            boxUVs[0] = textureTopLeft;
            boxVertices[1] = bottomLeftFront;
            boxNormals[1] = frontNormal;
            boxUVs[1] = textureBottomLeft;
            boxVertices[2] = topRightFront;
            boxNormals[2] = frontNormal;
            boxUVs[2] = textureTopRight;
            boxVertices[3] = bottomLeftFront;
            boxNormals[3] = frontNormal;
            boxUVs[3] = textureBottomLeft;
            boxVertices[4] = bottomRightFront;
            boxNormals[4] = frontNormal;
            boxUVs[4] = textureBottomRight;
            boxVertices[5] = topRightFront;
            boxNormals[5] = frontNormal;
            boxUVs[5] = textureTopRight;

            // Back face.
            boxVertices[6] = topLeftBack;
            boxNormals[6] = backNormal;
            boxUVs[6] = textureTopRight;
            boxVertices[7] = topRightBack;
            boxNormals[7] = backNormal;
            boxUVs[7] = textureTopLeft;
            boxVertices[8] = bottomLeftBack;
            boxNormals[8] = backNormal;
            boxUVs[8] = textureBottomRight;
            boxVertices[9] = bottomLeftBack;
            boxNormals[9] = backNormal;
            boxUVs[9] = textureBottomRight;
            boxVertices[10] = topRightBack;
            boxNormals[10] = backNormal;
            boxUVs[10] = textureTopLeft;
            boxVertices[11] = bottomRightBack;
            boxNormals[11] = backNormal;
            boxUVs[11] = textureBottomLeft;

            // Top face.
            boxVertices[12] = topLeftFront;
            boxNormals[12] = topNormal;
            boxUVs[12] = textureBottomLeft;
            boxVertices[13] = topRightBack;
            boxNormals[13] = topNormal;
            boxUVs[13] = textureTopRight;
            boxVertices[14] = topLeftBack;
            boxNormals[14] = topNormal;
            boxUVs[14] = textureTopLeft;
            boxVertices[15] = topLeftFront;
            boxNormals[15] = topNormal;
            boxUVs[15] = textureBottomLeft;
            boxVertices[16] = topRightFront;
            boxNormals[16] = topNormal;
            boxUVs[16] = textureBottomRight;
            boxVertices[17] = topRightBack;
            boxNormals[17] = topNormal;
            boxUVs[17] = textureTopRight;

            // Bottom face. 
            boxVertices[18] = bottomLeftFront;
            boxNormals[18] = bottomNormal;
            boxUVs[18] = textureTopLeft;
            boxVertices[19] = bottomLeftBack;
            boxNormals[19] = bottomNormal;
            boxUVs[19] = textureBottomLeft;
            boxVertices[20] = bottomRightBack;
            boxNormals[20] = bottomNormal;
            boxUVs[20] = textureBottomRight;
            boxVertices[21] = bottomLeftFront;
            boxNormals[21] = bottomNormal;
            boxUVs[21] = textureTopLeft;
            boxVertices[22] = bottomRightBack;
            boxNormals[22] = bottomNormal;
            boxUVs[22] = textureBottomRight;
            boxVertices[23] = bottomRightFront;
            boxNormals[23] = bottomNormal;
            boxUVs[23] = textureTopRight;

            // left face. 
            boxVertices[24] = topRightFront;
            boxNormals[24] = rightNormal;
            boxUVs[24] = textureTopLeft;
            boxVertices[25] = bottomRightFront;
            boxNormals[25] = rightNormal;
            boxUVs[25] = textureBottomLeft;
            boxVertices[26] = bottomRightBack;
            boxNormals[26] = rightNormal;
            boxUVs[26] = textureBottomRight;
            boxVertices[27] = topRightBack;
            boxNormals[27] = rightNormal;
            boxUVs[27] = textureTopRight;
            boxVertices[28] = topRightFront;
            boxNormals[28] = rightNormal;
            boxUVs[28] = textureTopLeft;
            boxVertices[29] = bottomRightBack;
            boxNormals[29] = rightNormal;
            boxUVs[29] = textureBottomRight;

            // right face.
            boxVertices[30] = topLeftFront;
            boxNormals[30] = leftNormal;
            boxUVs[30] = textureTopRight;
            boxVertices[31] = bottomLeftBack;
            boxNormals[31] = leftNormal;
            boxUVs[31] = textureBottomLeft;
            boxVertices[32] = bottomLeftFront;
            boxNormals[32] = leftNormal;
            boxUVs[32] = textureBottomRight;
            boxVertices[33] = topLeftBack;
            boxNormals[33] = leftNormal;
            boxUVs[33] = textureTopLeft;
            boxVertices[34] = bottomLeftBack;
            boxNormals[34] = leftNormal;
            boxUVs[34] = textureBottomLeft;
            boxVertices[35] = topLeftFront;
            boxNormals[35] = leftNormal;
            boxUVs[35] = textureTopRight;

            mesh.vertices = boxVertices;
            mesh.normals = boxNormals;
            mesh.uv = boxUVs;
            //mesh.triangles = CreateIndexBuffer(vertexCount, indexCount, slices);
            mesh.triangles = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 35
            };

            return mesh;
        }

        public static Mesh CreateBoxMesh(float width, float height, float depth)
        {
            Mesh mesh = new Mesh();
            mesh.name = "BoxMesh";

            // Because the box is centered at the origin, need to divide by two to find the + and - offsets
            width = width / 2.0f;
            height = height / 2.0f;
            depth = depth / 2.0f;

            Vector3[] boxVertices = new Vector3[36];
            Vector3[] boxNormals = new Vector3[36];
            Vector2[] boxUVs = new Vector2[36];

            Vector3 topLeftFront = new Vector3(-width, height, depth);
            Vector3 bottomLeftFront = new Vector3(-width, -height, depth);
            Vector3 topRightFront = new Vector3(width, height, depth);
            Vector3 bottomRightFront = new Vector3(width, -height, depth);

            Vector3 topLeftBack = new Vector3(-width, height, -depth);
            Vector3 topRightBack = new Vector3(width, height, -depth);
            Vector3 bottomLeftBack = new Vector3(-width, -height, -depth);
            Vector3 bottomRightBack = new Vector3(width, -height, -depth);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            // Front face.
            boxVertices[0] = topLeftFront;
            boxNormals[0] = frontNormal;
            boxUVs[0] = textureTopLeft;
            boxVertices[1] = bottomLeftFront;
            boxNormals[1] = frontNormal;
            boxUVs[1] = textureBottomLeft;
            boxVertices[2] = topRightFront;
            boxNormals[2] = frontNormal;
            boxUVs[2] = textureTopRight;
            boxVertices[3] = bottomLeftFront;
            boxNormals[3] = frontNormal;
            boxUVs[3] = textureBottomLeft;
            boxVertices[4] = bottomRightFront;
            boxNormals[4] = frontNormal;
            boxUVs[4] = textureBottomRight;
            boxVertices[5] = topRightFront;
            boxNormals[5] = frontNormal;
            boxUVs[5] = textureTopRight;

            // Back face.
            boxVertices[6] = topLeftBack;
            boxNormals[6] = backNormal;
            boxUVs[6] = textureTopRight;
            boxVertices[7] = topRightBack;
            boxNormals[7] = backNormal;
            boxUVs[7] = textureTopLeft;
            boxVertices[8] = bottomLeftBack;
            boxNormals[8] = backNormal;
            boxUVs[8] = textureBottomRight;
            boxVertices[9] = bottomLeftBack;
            boxNormals[9] = backNormal;
            boxUVs[9] = textureBottomRight;
            boxVertices[10] = topRightBack;
            boxNormals[10] = backNormal;
            boxUVs[10] = textureTopLeft;
            boxVertices[11] = bottomRightBack;
            boxNormals[11] = backNormal;
            boxUVs[11] = textureBottomLeft;

            // Top face.
            boxVertices[12] = topLeftFront;
            boxNormals[12] = topNormal;
            boxUVs[12] = textureBottomLeft;
            boxVertices[13] = topRightBack;
            boxNormals[13] = topNormal;
            boxUVs[13] = textureTopRight;
            boxVertices[14] = topLeftBack;
            boxNormals[14] = topNormal;
            boxUVs[14] = textureTopLeft;
            boxVertices[15] = topLeftFront;
            boxNormals[15] = topNormal;
            boxUVs[15] = textureBottomLeft;
            boxVertices[16] = topRightFront;
            boxNormals[16] = topNormal;
            boxUVs[16] = textureBottomRight;
            boxVertices[17] = topRightBack;
            boxNormals[17] = topNormal;
            boxUVs[17] = textureTopRight;

            // Bottom face. 
            boxVertices[18] = bottomLeftFront;
            boxNormals[18] = bottomNormal;
            boxUVs[18] = textureTopLeft;
            boxVertices[19] = bottomLeftBack;
            boxNormals[19] = bottomNormal;
            boxUVs[19] = textureBottomLeft;
            boxVertices[20] = bottomRightBack;
            boxNormals[20] = bottomNormal;
            boxUVs[20] = textureBottomRight;
            boxVertices[21] = bottomLeftFront;
            boxNormals[21] = bottomNormal;
            boxUVs[21] = textureTopLeft;
            boxVertices[22] = bottomRightBack;
            boxNormals[22] = bottomNormal;
            boxUVs[22] = textureBottomRight;
            boxVertices[23] = bottomRightFront;
            boxNormals[23] = bottomNormal;
            boxUVs[23] = textureTopRight;

            // left face. 
            boxVertices[24] = topRightFront;
            boxNormals[24] = rightNormal;
            boxUVs[24] = textureTopLeft;
            boxVertices[25] = bottomRightFront;
            boxNormals[25] = rightNormal;
            boxUVs[25] = textureBottomLeft;
            boxVertices[26] = bottomRightBack;
            boxNormals[26] = rightNormal;
            boxUVs[26] = textureBottomRight;
            boxVertices[27] = topRightBack;
            boxNormals[27] = rightNormal;
            boxUVs[27] = textureTopRight;
            boxVertices[28] = topRightFront;
            boxNormals[28] = rightNormal;
            boxUVs[28] = textureTopLeft;
            boxVertices[29] = bottomRightBack;
            boxNormals[29] = rightNormal;
            boxUVs[29] = textureBottomRight;

            // right face.
            boxVertices[30] = topLeftFront;
            boxNormals[30] = leftNormal;
            boxUVs[30] = textureTopRight;
            boxVertices[31] = bottomLeftBack;
            boxNormals[31] = leftNormal;
            boxUVs[31] = textureBottomLeft;
            boxVertices[32] = bottomLeftFront;
            boxNormals[32] = leftNormal;
            boxUVs[32] = textureBottomRight;
            boxVertices[33] = topLeftBack;
            boxNormals[33] = leftNormal;
            boxUVs[33] = textureTopLeft;
            boxVertices[34] = bottomLeftBack;
            boxNormals[34] = leftNormal;
            boxUVs[34] = textureBottomLeft;
            boxVertices[35] = topLeftFront;
            boxNormals[35] = leftNormal;
            boxUVs[35] = textureTopRight;

            mesh.vertices = boxVertices;
            mesh.normals = boxNormals;
            mesh.uv = boxUVs;
            //mesh.triangles = CreateIndexBuffer(vertexCount, indexCount, slices);
            mesh.triangles = new int[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                29, 30, 31, 32, 33, 34, 35
            };

            return mesh;
        }
        void ElapseSourceMesh(Vector3 cellWorldSize)
        {
            if (source == null || !lastLeafSize.Equals(cellWorldSize))//|| source.v.Length==0)
            {
                lastLeafSize = cellWorldSize;

                Mesh blockMesh = CreateBoxMesh(1, 1, 1);

                source = new MeshSource();

                var bound = blockMesh.bounds;

                // in basso a sinistra
                Vector3 localToCenter = bound.center + new Vector3(bound.size.y / 2, bound.size.y / 2, bound.size.y / 2);

                float freeSpace = 0.00f;
                Vector3 scaleToSize = new Vector3((cellWorldSize.x - freeSpace) / bound.size.x, (cellWorldSize.y - freeSpace) / bound.size.y, (cellWorldSize.z - freeSpace) / bound.size.z);

                var v = blockMesh.vertices.Select(X => new Vector3(
                  (X.x + localToCenter.x) * scaleToSize.x,
                  (X.y + localToCenter.y) * scaleToSize.y,
                  (X.z + localToCenter.z) * scaleToSize.z)).ToArray();

                int subs = blockMesh.subMeshCount;

                for (int s = 0; s < subs; s++)
                {
                    var tris = blockMesh.GetIndices(s);
                    Vector3[] fn = new Vector3[tris.Length / 3];
                    Triangle[] triangles = new Triangle[tris.Length / 3];

                    for (int i = 0, c = 0; i < tris.Length; i += 3, c++)
                    {
                        var v1 = v[tris[i]];
                        var v2 = v[tris[i + 1]];
                        var v3 = v[tris[i + 2]];

                        var FN = -Vector3.Cross(v2 - v1, v2 - v3).normalized;

                        triangles[c] = new Triangle();
                        triangles[c].index = c;
                        triangles[c].v1 = tris[i];
                        triangles[c].v2 = tris[i + 1];
                        triangles[c].v3 = tris[i + 2];
                        triangles[c].N = FN;
                        triangles[c].merged = false;

                        fn[c] = FN;

                        triangles[c].nextCell = iVector3.zero;

                        if (Mathf.Abs(1 - FN.x) < 0.01f) triangles[c].nextCell = new iVector3(1, 0, 0);
                        else if (Mathf.Abs(-1 - FN.x) < 0.01f) triangles[c].nextCell = new iVector3(-1, 0, 0);
                        else if (Mathf.Abs(1 - FN.y) < 0.01f) triangles[c].nextCell = new iVector3(0, 1, 0);
                        else if (Mathf.Abs(-1 - FN.y) < 0.01f) triangles[c].nextCell = new iVector3(0, -1, 0);
                        else if (Mathf.Abs(1 - FN.z) < 0.01f) triangles[c].nextCell = new iVector3(0, 0, 1);
                        else if (Mathf.Abs(-1 - FN.z) < 0.01f) triangles[c].nextCell = new iVector3(0, 0, -1);
                    }

                    source.tris_faces = triangles;
                }

                source.v = v;
                source.n = blockMesh.normals;

                //Vector2 offset = new Vector2(0,0);
                //Vector2 scale = new Vector2(1,1);

                source.uvs = blockMesh.uv.Select(X => new Vector2(1f - X.x,1f- X.y)).ToArray(); // sara la mesh
            }
        }

        public static Vector2 rotate(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        #endregion

        void OnEnable()
        {
            //    chunkList.Clear();
           // rebuildChunkList.Clear();
            OnPreEnable();
        }

        protected virtual void OnPreEnable()
        {
        }

	
		/// <summary>
		/// rebuild only cselected chunks
		/// </summary>
		/// <param name="chunks"></param>
		public void Update()
        {
            if (chunkList == null)
                return;

            //   Debug.Log("update");
            if (world != null)
                world.Update();

            ElapseSourceMesh(cellWorldSize);

            //if (rebuildChunkList.Count > 0)
            foreach(var entry in chunkList)
            {
                if (entry.mustBuild)
                {
                    //var toAll = rebuildChunkList.Values.Where(X => !X.cutMode);
                    //var toCut = rebuildChunkList.Values.Where(X => X.cutMode);

                    if (entry.cutMode)
                        ReBuildChunk_cut(entry);
                    else
                        ReBuildChunk_all(entry);//, rebuildChunkList[chunk]);

                    SaveMesh(entry);

                   // Debug.Log("REBUILD CHUNK  ALL: " + toAll.Count() + "  CUT:" + toCut.Count());

                    /*
                    var options = new ParallelOptions();
                    options.TaskScheduler = TaskScheduler.Default; //Run on background thread scheduler
                    options.MaxDegreeOfParallelism = Environment.ProcessorCount;

                    var ret = Parallel.ForEach(toAll, options, chunkEntry =>
                    {
                        ReBuildChunk_all(chunkEntry);//, rebuildChunkList[chunk]);
                    });

                    // main thread

                    foreach (var entry in toAll)
                    {
                        SaveMesh(entry);
                    }
                    //foreach (var entry in toAll)
                    //            // foreach (var entry in chunkList.Values)
                    //            {
                    //                //entry.cutMode = false;
                    //                ReBuildChunk_all(entry);//, rebuildChunkList[chunk]);
                    //            }
                    // tutti 
                    foreach (var entry in toCut)
                    // foreach (var entry in chunkList.Values)
                    {
                        //entry.cutMode = false;
                        ReBuildChunk_cut(entry);//, rebuildChunkList[chunk]);
                    }
                    */
                    entry.mustBuild = false;
                }
                //rebuildChunkList.Clear();
            }

        }

        private void OnApplicationQuit()
        {
            if (world != null)
                foreach (var layer in world.Layers)
                {
                    layer.OnApplicationQuit();
                }
        }

        public void InvalidateSubMaterials()
        {
            materialSelector.OnSubMaterialsChanged();
        }

        public void Invalidate(MeshChunk chunk, iVector3 chunkInternalPosition, bool cutMode)
        {
            if (chunkList == null)//.Count == 0)
                return;
            try
            {
                Invalidate(world.GetChunk(chunk.chunkIndex), cutMode);

                // prevendo anche i chunk vicini se sono al bordo 

                if (chunkInternalPosition.x == 0 && chunk.chunkIndex.x > 0)
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(-1, 0, 0)), cutMode);
                if (chunkInternalPosition.y == 0 && chunk.chunkIndex.y > 0)
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(0, -1, 0)), cutMode);
                if (chunkInternalPosition.z == 0 && chunk.chunkIndex.z > 0)
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(0, 0, -1)), cutMode);
                if (chunkInternalPosition.x == (chunk.size.x - 1) && chunk.chunkIndex.x < (world.chunksCount.x - 1))
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(1, 0, 0)), cutMode);
                if (chunkInternalPosition.y == (chunk.size.y - 1) && chunk.chunkIndex.y < (world.chunksCount.y - 1))
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(0, 1, 0)), cutMode);
                if (chunkInternalPosition.z == (chunk.size.z - 1) && chunk.chunkIndex.z < (world.chunksCount.z - 1))
                    Invalidate(world.GetChunk(chunk.chunkIndex + new iVector3(0, 0, 1)), cutMode);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void Invalidate(MeshChunk chunk, bool cutMode)
        {
            var entry = chunkList[chunk.ID];
            entry.cutMode = cutMode;
            entry.chunk = chunk;

            //  Debug.Log("invalidate chunk "+ chunk.chunkIndex);

            entry.mustBuild = true;
            entry.cutMode = entry.cutMode & cutMode;

            //if (!rebuildChunkList.ContainsKey(chunk.chunkIndex))//  (X => X.Item1.chunk == chunk);
            //    rebuildChunkList.Add(chunk.chunkIndex, entry);
            //else
            //    rebuildChunkList[chunk.chunkIndex].cutMode = rebuildChunkList[chunk.chunkIndex].cutMode & cutMode;
        }


        public void OnLoad(GameObject _root, Vector3 localToWorldOffset, Vector3 chunkWorldSize, Vector3 cellWorldSize)
        {
            this.root = _root;
            this.localToWorldOffset = localToWorldOffset;
            this.cellWorldSize = cellWorldSize;
            this.chunkWorldSize = chunkWorldSize;
            world = GetComponent<MeshWorldSource>().meshWorld;
        }

        // pulisce la cache
        public void Clear()
        {
        }

        public void Initialize()
        {
            //    Debug.Log(" chunk Initialize");
            root.transform.Clear();
            var chunksCount = world.chunksCount;
            chunkList = new ChunkEntry[chunksCount.x* chunksCount.y* chunksCount.z];// .Clear();
          
           
            List<iVector3> list = new List<iVector3>();
            int ch = 0;
            for (int x = 0; x < chunksCount.x; x++)
            {
                for (int y = 0; y < chunksCount.y; y++)
                {
                    for (int z = 0; z < chunksCount.z; z++)
                    {
                        var chunkIdx = new iVector3(x, y, z);
                        // Debug.Log(" chunk Initialize" + chunkIdx);
                        var go = InitChunk(chunkWorldSize, chunkIdx, cellWorldSize, world.GetChunk(chunkIdx), source);
                        var chunk_offset = localToWorldOffset + new Vector3(chunkWorldSize.x * chunkIdx.x, chunkWorldSize.y * chunkIdx.y, chunkWorldSize.z * chunkIdx.z);

                        var multiMesh = new MultiMesh(materialSelector, chunk_offset, cellWorldSize, source, world.GetChunk(chunkIdx));
                        multiMesh.source = source;

                        var entry = new ChunkEntry()
                        {
                            pos = chunkIdx,
                            go = go,
                            chunk = world.GetChunk(chunkIdx),
                            chunk_offset = chunk_offset,
                            multiMesh = multiMesh,
                            hasMaterial = false
                        };

                        if (entry.chunk.ID != ch)
                            Debug.LogError("must be == ");

                        chunkList[ch++]=  entry;
                    }
                }
            }
            if (onInitialized != null) onInitialized();
        }

        /// <summary>
        /// build all
        /// </summary>
        /// <param name="_root"></param>
        /// <param name="offset"></param>
        /// <param name="chunkWorldSize"></param>
        /// <param name="cellWorldSize"></param>
        public void Build(GameObject _root, MeshWorld world, Vector3 localToWorldOffset, Vector3 chunkWorldSize, Vector3 cellWorldSize)
        {
            if (onBeforeRender != null)
                onBeforeRender();

            //Debug.Log("build all");
            bool blockChanged = false;
            if (!cellWorldSize.Equals(this.cellWorldSize))
            {
                source = null; // e' cambiato il blocco
                blockChanged = true;
            }

            ElapseSourceMesh(cellWorldSize);

           
            //rebuildChunkList.Clear();

            this.root = _root;
            this.world = world;
            this.localToWorldOffset = localToWorldOffset;
            this.cellWorldSize = cellWorldSize;
            this.chunkWorldSize = chunkWorldSize;

            //  ElapseSourceMesh();

            if (root == null)
            {
                if (transform.Find("chunks"))
                {
                    transform.Find("chunks").Clear();
                    root = transform.Find("chunks").gameObject;

                }
                else
                {
                    root = new GameObject("chunks");
                    root.SetParentAtOrigin(gameObject);

                }
                root.transform.localPosition = localToWorldOffset;
            }

            //var s = GetComponent<MeshWorldSource>();
            //world = s.meshWorld;
            world.builder = this;

            // ================
            int total = world.chunksCount.x * world.chunksCount.y * world.chunksCount.z;

            if (chunkList==null || chunkList.Length != total || root.transform.childCount != total || blockChanged)
                Initialize();

            // multi thread
            foreach (var e in chunkList) e.mustBuild = false;

            var options = new ParallelOptions();
            options.TaskScheduler = TaskScheduler.Default; //Run on background thread scheduler
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;

            var ret = Parallel.ForEach(chunkList, options, chunkEntry =>
           {
                // puo' cambiare
                chunkEntry.chunk = world.GetChunk(chunkEntry.pos);
               chunkEntry.multiMesh.chunk = chunkEntry.chunk;

               BuildChunk(chunkEntry);
           });

            // main thread

            foreach (var entry in chunkList)
            {
                //  BuildChunk(entry);
                SaveMesh(entry);
            }

        }



        protected virtual void OnCreateCell(GameObject chunkGO, MeshChunk chunk, MeshChunkCell cell)
        {
        }


        public GameObject InitChunk(Vector3 chunkWorldSize, iVector3 chunkPos, Vector3 cellSize, MeshChunk chunk, MeshSource sourceMesh)
        {

            //   if (chunkObj == null)
            //if (!go.transform.Find("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z))
            //{
            var go = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z);
            go.SetParentAtOrigin(root);
            go.layer = LayerMask.NameToLayer("Chunk");
            go.AddComponent<MeshChunkRef>().position = chunkPos;

            var solid = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z + "_solid");
            solid.SetParentAtOrigin(go);
            solid.layer = LayerMask.NameToLayer("Chunk");

            solid.AddComponent<MeshChunkRef>().position = chunkPos;
            solid.AddComponent<MeshFilter>();
            solid.AddComponent<MeshRenderer>();

            var trx = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z + "_trx");
            trx.SetParentAtOrigin(go);
            trx.layer = LayerMask.NameToLayer("Chunk");

            trx.AddComponent<MeshChunkRef>().position = chunkPos;
            trx.AddComponent<MeshFilter>();
            trx.AddComponent<MeshRenderer>();
            //}
            return go;
        }

        public void BuildChunk(ChunkEntry entry)
        {
            GameObject go = entry.go;

            MultiMesh mesh = entry.multiMesh;
            mesh.begin();

            var size = entry.chunk.size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var cell = entry.chunk.Get(x, y, z);
                        if (cell != null && cell.isVisible)
                        {
                            // Debug.Log("Add "+ chunkPos+" _> " + x + " " +y+" " +z);
                            mesh.AddObject(cell, new iVector3(x, y, z));
                            OnCreateCell(go, entry.chunk, cell);
                        }
                    }
                }
            }

        }

        void SaveMesh(ChunkEntry entry)
        {
            //    Debug.Log("save " + entry.pos);

            var go = entry.go;
            MultiMeshOutput meshes = entry.multiMesh.Elapse();

            var go_solid = go.transform.GetChild(0);
            var go_trx = go.transform.GetChild(1);

            go_solid.GetComponent<MeshFilter>().sharedMesh = meshes.mesh_solid;
            if (go_solid.GetComponent<MeshCollider>())
            {
                go_solid.GetComponent<MeshCollider>().gameObject.SetActive(false); go_solid.GetComponent<MeshCollider>().gameObject.SetActive(true); 
            }

            if (meshes.mesh_trasparent != null)
            {
                go_trx.GetComponent<MeshFilter>().sharedMesh = meshes.mesh_trasparent;
                if (go_trx.GetComponent<MeshCollider>())
                { go_trx.GetComponent<MeshCollider>().gameObject.SetActive(false); go_trx.GetComponent<MeshCollider>().gameObject.SetActive(true); }

            }
            else
                go_trx.GetComponent<MeshFilter>().sharedMesh = null;

            //   MeshChunkCell[] subsMat = mesh.LayersMats;
            if (!entry.hasMaterial)
            {
                entry.hasMaterial = true;
                go.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(true, false);
                //go.GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(null);
                go.transform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.transform.GetChild(0).GetComponent<MeshRenderer>().receiveShadows = false;

                go.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(false, false); 
                //go.GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(null);
                go.transform.GetChild(1).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.transform.GetChild(1).GetComponent<MeshRenderer>().receiveShadows = false;
            }
            // Debug.Log("build " + go.GetComponent<MeshFilter>().sharedMesh.triangles.Length);


        }

        public void ReBuildChunk_all(ChunkEntry entry)
        {
            
          // Debug.Log("REBUILD CHUNK ALL " + entry.pos );

           // if (!entry.cutMode)
            {
                entry.multiMesh.begin();

                var mesh = entry.multiMesh;
                var size = entry.chunk.size;
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        for (int z = 0; z < size.z; z++)
                        {
                            var cell = entry.chunk.Get(x, y, z);
                            if (cell != null && cell.isVisible)
                            {
                                // Debug.Log("Add "+ chunkPos+" _> " + x + " " +y+" " +z);
                                mesh.AddObject(cell, new iVector3(x, y, z));
                            }
                        }
                    }
                }
            }
        }


        public void ReBuildChunk_cut(ChunkEntry entry)
        {
           // Debug.Log("REBUILD CHUNK CUT " + entry.pos );

            entry.multiMesh.clearLayers();


            var faceLayer_solid = entry.multiMesh.faceLayer_solid;
            var faceLayer_trx = entry.multiMesh.faceLayer_trasp;

            var size = entry.chunk.size;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var cell = entry.chunk.Get(x, y, z);
                        if (cell != null && cell.isVisible)
                        {
                            var cellIdx = new iVector3(x, y, z) + entry.chunk.startWorldPosition;

                            if (!cell.isTrasparent() )
                            {
                                for (int i = 0; i < source.tris_faces.Length; i++)
                                {
                                    entry.multiMesh.AddTriangle(faceLayer_solid, source, 1, cell, cell.startVertexIndex, cellIdx, i);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < source.tris_faces.Length; i++)
                                {
                                    entry.multiMesh.AddTriangle(faceLayer_trx, source, 1, cell, cell.startVertexIndex, cellIdx, i);
                                }
                            }

                        }
                    }
                }
            }

            var mesh = entry.go.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            //mesh.SetTriangles(faceLayer_solid.tris, 0);

            //if (faceLayer_trx.tris != null)
            //{
            //    mesh = entry.go.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            //    if (mesh)
            //    {
            //        mesh.SetTriangles(faceLayer_trx.tris, 0);
            //    }
            //}
            //else {
            //    mesh = entry.go.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
            //    mesh.SetTriangles(new int[] { }, 0);
            //}
            //entry.cutMode = false;

        }
    }
}
