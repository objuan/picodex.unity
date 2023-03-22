using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{

    public class MultiMeshLayer
    {
        //   public MeshChunkCell cell;
        public string cellDesc;
        public List<int> tris = new List<int>();
    }

    public class MultiMeshOutput
    {
        public Mesh mesh_solid;
        public Mesh mesh_trasparent;
    }
    public interface IMeshChunk
    {
        iVector3 startWorldPosition { get; }
        iVector3 size { get; }

        IMeshChunkCell GetCellMerged(int x, int y, int z);
    }
    public interface IMeshChunkCell
    {
        int startVertexIndex { get; set; }
        bool isVisible { get; }
        bool isTrasparent();
        MeshChunkMaterial cellMaterial();
    }
    public class MultiMesh
    {
        protected IMaterialSelector materialSelector;

        //public MultiMeshLayer borderLayer;
        public MultiMeshLayer faceLayer_solid;
        public MultiMeshLayer faceLayer_trasp;
        public MeshSource source;
        public IMeshChunk chunk;
        // MeshWorld world;
        protected Vector3 chunk_offset;
        protected Vector3 cell_size;
        protected iVector3 chunkStartWorldPosition;
        protected int startVertexIndex;

        public FastArray_Vector3 vertices ;
        public FastArray_Vector3 normals;
        public FastArray_Color vcolors;
        public FastArray_Vector2 uvs1; // Main uv
        public FastArray_Vector2 uvs2; // sub face uv
        public FastArray_Vector2 uvs3;  // special , cut

        MultiMeshOutput output;

        protected Dictionary<int, Vector3[]> vertexCache = new Dictionary<int, Vector3[]>();
    
        public MultiMesh( IMaterialSelector materialSelector,Vector3 chunk_offset, Vector3 cell_size, MeshSource source, IMeshChunk chunk)//,MeshWorld world)
        {
            this.materialSelector = materialSelector;
            this.chunk_offset = chunk_offset;
            this.cell_size = cell_size;
            this.source = source;
            this.chunk = chunk;
           // this.world = world;
            chunkStartWorldPosition = chunk.startWorldPosition;

            int N_MAX = source.v.Length * (chunk.size.x * chunk.size.y * chunk.size.z);

            vertices = new FastArray_Vector3(N_MAX);
            normals = new FastArray_Vector3(N_MAX);
            vcolors = new FastArray_Color(N_MAX);
            uvs1 = new FastArray_Vector2(N_MAX);
            uvs2 = new FastArray_Vector2(N_MAX);
            uvs3 = new FastArray_Vector2(N_MAX);
            //  borderLayer = new MultiMeshLayer();
            faceLayer_solid = new MultiMeshLayer();
            faceLayer_trasp = new MultiMeshLayer();

            output = new MultiMeshOutput();
            output.mesh_solid = new Mesh();
            output.mesh_trasparent = new Mesh();
        }

        public void begin()
        {
            vertices.Clear();
            normals.Clear();
            vcolors.Clear();
            uvs1.Clear();
            uvs2.Clear();
            uvs3.Clear();
            faceLayer_solid = new MultiMeshLayer();
            faceLayer_trasp = new MultiMeshLayer();
        }
        public void clearLayers()
        {
            faceLayer_solid = new MultiMeshLayer();
            faceLayer_trasp = new MultiMeshLayer();
        }
        /// <summary>
        /// logica di add
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="source"></param>
        /// <param name="cell"></param>
        /// <param name="startVertexIndex"></param>
        /// <param name="cellIdx"></param>
        /// <param name="triIdx"></param>
        public  virtual void AddTriangle(MultiMeshLayer layer , MeshSource source, int layerIdx,IMeshChunkCell cell,int startVertexIndex, iVector3 cellIdx, int triIdx)
        {
            // controllo se e' in vista
            Triangle tri = source.GetTri( triIdx);

            IMeshChunkCell nearCell = chunk.GetCellMerged(cellIdx.x + tri.nextCell.x, cellIdx.y + tri.nextCell.y, cellIdx.z + tri.nextCell.z);

            // salto se è adiacente solido, altrimenti lo metto 
          //  if (nearCell != null && nearCell.isVisible && !nearCell.isTrasparent())
             if (nearCell != null && nearCell.isVisible && !cell.isTrasparent( )&& !nearCell.isTrasparent() )
            {
                return;
            }
         
            layer.tris.Add(startVertexIndex + tri.v1);
            layer.tris.Add(startVertexIndex + tri.v2);
            layer.tris.Add(startVertexIndex + tri.v3);
        }

        protected static Vector2[,] face_uv_mapping = new Vector2[,]
        {
            // front
            { new Vector2(0,0) , new Vector2(0,1 )  , new Vector2(1,0 ) ,
              new Vector2(0,1) , new Vector2(1,1 )  , new Vector2(1,0 ) , new Vector2(-1,-1)}, // ultimo e' il flip
            // back
             { new Vector2(0,0) , new Vector2(1,0  )  , new Vector2(0,1  ) ,
               new Vector2(0,1) , new Vector2(1,0 )  , new Vector2(1,1 ) , new Vector2(1,-1)}, // ultimo e' il flip
            // top
             { new Vector2(0,0) , new Vector2(1,1  )  , new Vector2(0,1  ) ,
               new Vector2(0,0) , new Vector2(1,0 )  , new Vector2(1,1 ) , new Vector2(1,-1)}, // ultimo e' il flip
              // bottom
             { new Vector2(0,0) , new Vector2(0,1  )  , new Vector2(1,1  ) ,
               new Vector2(0,0) , new Vector2(1,1 )  , new Vector2(1,0 ) , new Vector2(1,-1)}, // ultimo e' il flip
                 // lewft
             { new Vector2(0,1) , new Vector2(0,0  )  , new Vector2(1,0  ) ,
               new Vector2(1,1) , new Vector2(0,1 )  , new Vector2(1,0 ) , new Vector2(-1,1)}, // ultimo e' il flip
                  // right
             { new Vector2(0,1) , new Vector2(1,0  )  , new Vector2(0,0  ) ,
               new Vector2(1,1) , new Vector2(1,0 )  , new Vector2(0,1 ) , new Vector2(1,1)}, // ultimo e' il flip
        };

        // la mesh di riferimento ha la Z invertita
        protected static int[] cellToFace = new int[] { 1, 0, 2, 3, 5, 4 };
        protected static Vector2[] emptyTxt = new Vector2[] { Vector2 .zero, Vector2.zero , Vector2.zero , Vector2.zero , Vector2.zero , Vector2.zero };
        protected static Vector2 noTxt = new Vector2(0.001f, 0.001f);
        protected static Vector2[] bulkTxt = new Vector2[] { noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt ,
                                                    noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt ,
                                                    noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt , noTxt };
        public virtual void AddObject( IMeshChunkCell cell, iVector3 chunk_cellIdx)
        {
            int key = chunk_cellIdx.x + chunk_cellIdx.y * 50 + chunk_cellIdx.z * (50*50);
            startVertexIndex = vertices.count;

			if (vertexCache.ContainsKey(key))
			{
				vertices.AddRange(vertexCache[key]);
			}
			else
			{
                var offset = chunk_offset + new Vector3(cell_size.x * chunk_cellIdx.x, cell_size.y * chunk_cellIdx.y, cell_size.z * chunk_cellIdx.z);
                
                int l = source.v.Length;
                var vv = new Vector3[l];
                for (int i = 0; i < l; i++)
                    vv[i] = source.v[i] + offset;
                vertexCache.Add(key, vv);

                vertices.AddRange(vv);
            }

            normals.AddRange(source.n);

            //vertices.AddRange(source.v.Select( X => X + offset));

            var cellWorldIndex = chunkStartWorldPosition + chunk_cellIdx;

            Color color;
            TileInfo tileInfo = materialSelector.GetMaterialTile(cell.cellMaterial(),out color);
            Vector2 off = tileInfo.main_offset;
            Vector2 scale = tileInfo.main_scale;

            uvs1.AddRange(source.uvs, scale.x, scale.y, off.x, off.y);

			if (tileInfo.userValues.HasValue)
			{
				for (int i = 0; i < source.v.Length; i++)
					uvs3.Add(tileInfo.userValues.Value);
			}

			// facce ?? 
			if (tileInfo.faces != null)
            {
                //off = tileInfo.sub_color_offset;
                //scale = tileInfo.sub_color_scale;
                // uvs.AddRange(source.uvs.Select( X => off + X * scale) );
                for (int f = 0; f < 6; f++)
                {
					//try { 
     //                   var faceuv1 = tileInfo.faces[cellToFace[f]];
     //                   // 2 triangoli per faccia, 4 vertici per faccia
     //               }
     //               catch (System.Exception se)
     //               {
     //                   int y = 0;
     //               }
                    var faceuv = tileInfo.faces[cellToFace[f]];
                    if (f< 6)
                    {
                        var flip = face_uv_mapping[f, 6];
                        for (int v = 0; v < 6; v++)
                        {
                            var map = face_uv_mapping[f, v];
                            if (flip.x == -1) map = new Vector2(1f- map.x, map.y);
                            if (flip.y == -1) map = new Vector2( map.x, 1f - map.y);

                            var uv = faceuv.offset + faceuv.scale * map;
                            uvs2.Add(uv);
                        }
                    }
                    else
                    {
                        uvs2.AddRange(emptyTxt);
                    }

                }

                for (int i = 0; i < source.v.Length; i++)
                    vcolors.Add(color);

                // uvs2.AddRange(source.uvs.Select(X => X * scale + off));
            }
            else
            {
                uvs2.AddRange(bulkTxt);
             
                for (int i = 0; i < source.v.Length; i++)
                    vcolors.Add(color);
            }
             //   uvs2.AddRange(source.uvs);

            // cell.cellColor();

             cell.startVertexIndex = startVertexIndex;


            if (!cell.isTrasparent())
            {
                for (int i = 0; i < source.tris_faces.Length; i++)
                {
                    AddTriangle(faceLayer_solid, source, 1, cell, startVertexIndex, cellWorldIndex, i);
                }
            }
            else
            {
                for (int i = 0; i < source.tris_faces.Length; i++)
                {
                    AddTriangle(faceLayer_trasp, source, 1, cell, startVertexIndex, cellWorldIndex, i);
                }
            }
        }


        // solid / trasparent
        public MultiMeshOutput Elapse()
        {
            for (int i = 0; i < 2; i++)
            {
               
                var mesh = (i == 0) ? output.mesh_solid : output.mesh_trasparent;

                if (i == 1 && faceLayer_trasp.tris.Count == 0 && mesh.triangles.Length == 0)
                    break;

                mesh.Clear();
                mesh.SetVertices(vertices.values, 0, vertices.count);
                mesh.SetUVs(0, uvs1.values, 0, uvs1.count);
                mesh.SetUVs(1, uvs2.values, 0, uvs2.count);
                if (uvs3.count>0)
                    mesh.SetUVs(3, uvs3.values, 0, uvs3.count);
                mesh.SetColors(vcolors.values, 0, vcolors.count);
                mesh.SetNormals(normals.values, 0, normals.count);


                mesh.subMeshCount = 1;
                mesh.SetTriangles( (i==0) ? faceLayer_solid.tris.ToArray() : faceLayer_trasp.tris.ToArray(), 0);


                //Debug.Log();

               // mesh.RecalculateNormals();
                //mesh.RecalculateBounds();

            }
            return output;
        }

    //    public void Update_bo(MultiMeshOutput mesh_out)
    //    {
    //        for (int i = 0; i < 2; i++)
    //        {
    //            if (i == 1 && faceLayer_trasp.tris.Count == 0)
				//{
    //                mesh_out.mesh_trasparent = null;
    //                break;
    //            }
                    
    //            var mesh = (i==0) ?  mesh_out.mesh_solid : mesh_out.mesh_trasparent;
               
    //            mesh.SetVertices(vertices.values , 0, vertices.count);
    //            mesh.SetUVs(0, uvs1.values, 0, uvs1.count);
    //            mesh.SetUVs(1, uvs2.values, 0, uvs2.count);
    //            mesh.SetColors(vcolors.values, 0, vcolors.count);
              
    //            mesh.subMeshCount = 1;
    //            mesh.SetTriangles((i == 0) ? faceLayer_solid.tris.ToArray() : faceLayer_trasp.tris.ToArray(), 0);
                

    //            mesh.RecalculateNormals();
    //            mesh.RecalculateBounds();

    //        }
    //    }
    }


}
