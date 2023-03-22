using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{
    /// <summary>
    /// usa solo un chunk
    /// 
    /// </summary>
    [ExecuteInEditMode]
    public class MeshSurfaceBuilder : MonoBehaviour
    {
        [NonSerialized]
        public MeshWorld world;

        [NonSerialized]
        public MeshChunkCell[] surfaceCells;

        //public Mesh filter;

        //  [NonSerialized]
        // params

        //  public GameObject blockMeshObject;

        [NonSerialized]
        public bool usePhysic=false;

        [NonSerialized]
        public iVector3 size;
    
    
        [NonSerialized]
        public IMaterialSelector materialSelector;

        public Vector3 localToWorldOffset;

        public Vector3 cellWorldSize;

      //  [NonSerialized]
        public  Vector3 chunkWorldSize;

        [NonSerialized]
        public MeshSource source;

        [NonSerialized]
        public GameObject root;
       
        Vector3 lastLeafSize;

        public Action<GameObject> OnBuild;

        #region SOURCE
        void ElapseSourceMesh(Vector3 cellWorldSize)
        {
            if (source == null || !lastLeafSize.Equals(cellWorldSize))//|| source.v.Length==0)
            {
                lastLeafSize = cellWorldSize;

                Mesh blockMesh = MeshChunkBuilder.CreateBoxMesh(1, 1, 1);

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

                source.uvs = blockMesh.uv.Select(X => new Vector2(1f - X.x, 1-X.y)).ToArray(); // sara la mesh
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

		private void OnEnable()
		{
                Build();
		}

        private void Start()
        {
             Build();

            //if (usePhysic)
            //{
            //    foreach (var filter in GetComponentsInChildren<MeshFilter>())
            //    {
            //        filter.gameObject.AddComponent<MeshCollider>().convex = true;
            //        filter.gameObject.AddComponent<Rigidbody>();
            //        //  filter.gameObject.AddComponent<Body>();
            //    }
            //}
        }

        /// <summary>
        /// build all
        /// </summary>
        /// <param name="_root"></param>
        /// <param name="offset"></param>
        /// <param name="chunkWorldSize"></param>
        /// <param name="cellWorldSize"></param>
        public void Build()
        {
            //Debug.Log("build all");

            if (world == null  || surfaceCells == null)
                return;

            Debug.Log("build surface");
            
            if (!transform.Find("surface"))
            {
                GameObject go = new GameObject("surface");
                go.SetParentAtOrigin(gameObject);
            }


            float leafSize = world.leafSize;
            var chunkSize = world.worldSize;
        
            this.root = transform.Find("surface").gameObject;

         
            this.chunkWorldSize = new Vector3(leafSize * chunkSize.x, leafSize * chunkSize.y, leafSize * chunkSize.z);
            this.cellWorldSize = new Vector3(leafSize , leafSize, leafSize);

            ElapseSourceMesh(cellWorldSize);

            // ================

            iVector3 min = new iVector3(9999, 9999, 9999);
            foreach (var p in surfaceCells)
                min = min.Min(p.worldPosition);

            // PIVOT on 

            this.localToWorldOffset = min.ToVector3() * leafSize;
            var chunk = new MeshChunk(world, new iVector3(0, 0, 0) , surfaceCells);

            BuildChunk(null, min, cellWorldSize, chunk, source);


        }


        protected virtual void OnCreateCell(GameObject chunkGO , MeshChunk chunk, MeshChunkCell cell )
        { 
        }


        public GameObject BuildChunk(GameObject chunkObj, iVector3 chunkPos,Vector3 cellSize, MeshChunk chunk, MeshSource sourceMesh)
        {
            //   Debug.Log("build " + chunkPos);

           
            var chunk_offset = localToWorldOffset;// + new Vector3(chunkWorldSize.x * chunkPos.x, chunkWorldSize.y * chunkPos.y, chunkWorldSize.z * chunkPos.z);

            var offset = new Vector3(0, 0, 0);// root.transform.TransformPoint(new Vector3(0, 0, 0)) - localToWorldOffset;

            GameObject go = chunkObj;
            if (chunkObj == null)
            {
                go = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z);
                go.SetParentAtOrigin(root);
                go.layer = LayerMask.NameToLayer("ChunkSurface");
                go.AddComponent<MeshChunkRef>().position = chunkPos;

                var solid = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z+"_solid");
                solid.SetParentAtOrigin(go);
                solid.layer = LayerMask.NameToLayer("ChunkSurface");

                solid.AddComponent<MeshChunkRef>().position = chunkPos;
                solid.AddComponent<MeshFilter>();
                solid.AddComponent<MeshRenderer>();

                var trx = new GameObject("chunk_" + chunkPos.x + "_" + chunkPos.y + "_" + chunkPos.z + "_trx");
                trx.SetParentAtOrigin(go);
                trx.layer = LayerMask.NameToLayer("ChunkSurface");

                trx.AddComponent<MeshChunkRef>().position = chunkPos;
                trx.AddComponent<MeshFilter>();
                trx.AddComponent<MeshRenderer>();
            }
          
            MultiMesh mesh = new MultiMesh(materialSelector,chunk_offset, cellSize, sourceMesh, chunk);
            mesh.source = sourceMesh;
 
           var size = chunk.size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var cell = chunk.Get(x, y, z);
                        if (cell != null && cell.isVisible)
                        {
                            // Debug.Log("Add "+ chunkPos+" _> " + x + " " +y+" " +z);
                            mesh.AddObject(cell, new iVector3(x, y, z));
                            OnCreateCell(go, chunk, cell);
                        }
                    }
                }
            }

            MultiMeshOutput meshes = mesh.Elapse();

            go.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh = meshes.mesh_solid;
            if (meshes.mesh_trasparent!=null)
                go.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = meshes.mesh_trasparent;
            else
                go.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh = null;

            go.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(true, false); ;
            //go.GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(null);
            go.transform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.GetChild(0).GetComponent<MeshRenderer>().receiveShadows = false;
   
            go.transform.GetChild(1).GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(false, false); ;
            //go.GetComponent<MeshRenderer>().sharedMaterial = materialSelector.GetMaterial(null);
            go.transform.GetChild(1).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.GetChild(1).GetComponent<MeshRenderer>().receiveShadows = false;

            // collisions

            // Debug.Log("build " + go.GetComponent<MeshFilter>().sharedMesh.triangles.Length);

            if (usePhysic && GetComponentsInChildren<Rigidbody>().Count()==0)
            {
                foreach (var filter in GetComponentsInChildren<MeshFilter>())
                {
                    filter.gameObject.AddComponent<MeshCollider>().convex = true;
                    var body = filter.gameObject.AddComponent<Rigidbody>();

                    body.velocity = new Vector3(0, 1, 0);// * game.config.airBlocksBreakSpeed;

                    //  filter.gameObject.AddComponent<Body>();
                }
            }

            if (OnBuild != null) OnBuild(go);
            return go;
        }

     

    }
}
