using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{

    public class CellCoordinate
    {
        public iVector3 chunk;
        public iVector3 localCoord;
    }

	public class MeshWorldSource : MonoBehaviour
	{
		public MeshWorld meshWorld;
	}

	public  class TempLayer
    {
        /// <summary>
        /// from 0 to 9. first is 0 , last is 9
        /// </summary>
        public int order;
        public string name;
        public MeshChunkLayer layer;
    }
    public enum MergedViewType
    {
        Normal=0,
    //    Ghost,
        Cut
        // Bonus

    }
    public class MeshWorld
    {
        public object Tag=null;

        public MeshChunk[] chunks;
        List<MeshChunkLayer> layers = new List<MeshChunkLayer>();
        iVector3[] worldCellIndexList;
        MeshChunkCell[] merged_cells;
        MergedViewType[] merged_view_types;

        Dictionary<iVector3,MeshChunkCell> subFaceCells = new Dictionary<iVector3, MeshChunkCell>();

        long[] merged_cells_ids;

        // data 
        public IList<MeshChunkLayer> Layers  => layers;

        public IEnumerable<MeshChunkCell> SubFaceCells => subFaceCells.Values;


        // called after the layers, no save
        List<TempLayer> tempLayers = new List<TempLayer>();
        // public IList<TempLayer> TempLayers => tempLayers;
        bool mustRebuildCells = true;
        public bool playActive = true;
        public iVector3 chunksCount;
        public iVector3 worldSize;
        public iVector3 chunkSize;
        public int size;
        public int size2;
        public int merge_size;
        public int merge_size2;
        public float leafSize;

        public float leafWorldSize => root.localToWorldMatrix.lossyScale.x * leafSize;

        public Vector3 pivot_pos = new Vector3(0, 0, 0);
        public Quaternion pivot_rot = Quaternion.identity;

        public Transform root;

        public bool modEnabled = false;
        public MeshChunkBuilder builder;
        int forceVisibleLayer;

        // public BoundsOctree<MeshChunkCell> tree;

        public iVector3 maxCoordinate;

        public Vector3 realSize;// => new Vector3(leafSize * worldSize.x, leafSize * worldSize.y, leafSize * worldSize.z);


        public Bounds bounds;

        public float boundRay;

        public Action<iVector3> OnInvalidate;


        public MeshWorld()
        {
        }

        public MeshWorld(Transform root, iVector3 worldSize, iVector3 chunkSize, float leafSize)
        {
            Initialize(root, worldSize, chunkSize, leafSize);
        }

        public void Initialize(Transform root, iVector3 worldSize, iVector3 chunkSize, float leafSize)
        {
            this.worldSize = worldSize;
            this.chunkSize = chunkSize;
           
            this.leafSize = leafSize;
            //  this.origin = origin;
            this.root = root;
            realSize = new Vector3(leafSize * worldSize.x, leafSize * worldSize.y, leafSize * worldSize.z);
            maxCoordinate = worldSize + new iVector3(-1, -1, -1);
            bounds = new Bounds(new Vector3(leafSize * 0.5f * worldSize.x, leafSize * 0.5f * worldSize.y, leafSize * worldSize.z * 0.5f),
                realSize);

            boundRay = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.5f;

            Resize(worldSize, leafSize);

            if (layers.Count == 0)
                AddLayer();

   
        }

        public void Update()
        {
            // bool isChanged = false;

            foreach (var layer in Layers)
            {
                layer.PreUpdate();

            }
            if (playActive || Application.isPlaying)
            {
                foreach (var layer in Layers)
                {
                    layer.Update();
                }
            }

            if (mustRebuildCells)
            {
                RebuildAllLayerCells();
                RebuildMesh();
            }
            //if (isChanged)
            //    CommitChange(false);
        }


        public MeshChunkLayer AddLayer()
        {
            MeshChunkLayer layer = new MeshChunkLayer();
            layers.Add(layer);
            layer.Initialize(this, layers.Count);
           // RebuildAllLayerCells();
            return layer;
        }

        public void RemoveLayer(int index)
        {
            layers.RemoveAt(index);
            Invalidate();
        }

        public void RemoveLayer(MeshChunkLayer layer)
        {
            int idx = layers.IndexOf(layer);
           if (idx != -1) RemoveLayer(idx);
        }

        public MeshChunkLayer Layer(int index) => layers[index];

        /// <summary>
        /// order from 0 to 9 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public MeshChunkLayer AddTempLayer(string name,int order)
        {
            MeshChunkLayer layer = new MeshChunkLayer();
            tempLayers.Add(new TempLayer() { layer = layer, name = name, order = order });
            layer.Initialize(this, tempLayers.Count);

            tempLayers.Sort((X, Y) => X.order.CompareTo(Y.order));
            return layer;
        }
        public void ClearTempLayers()
        {
            tempLayers.Clear();
            Invalidate();
        }
        public void RemoveTempLayer(string name)
        {
            var find = tempLayers.FirstOrDefault(X => X.name == name);
            if (find != null)
            {
                tempLayers.Remove(find);
                tempLayers.Sort((X, Y) => X.order.CompareTo(Y.order));
                Invalidate();
            }
        }
        public void RemoveTempLayer(MeshChunkLayer layer)
        {
            var find = tempLayers.FirstOrDefault(X => X.layer == layer);
            if (find != null)
            {
                tempLayers.Remove(find);
                tempLayers.Sort((X, Y) => X.order.CompareTo(Y.order));
                Invalidate();
            }
        }
        public MeshChunkLayer GetTempLayer(string name)
        {
            var find = tempLayers.FirstOrDefault(X => X.name == name);
            return find != null ?  find.layer : null;
        }

        // public MeshChunkLayer TempLayer(int index) => tempLayers[index];

        void Resize(iVector3 newWorldSize, float leafSize)
        {

            worldSize = newWorldSize;
            this.leafSize = leafSize;
            realSize = new Vector3(leafSize * worldSize.x, leafSize * worldSize.y, leafSize * worldSize.z);

            Debug.Log("Reshape world : " + worldSize + " ck: " + chunkSize + " leaf: " + leafSize);

            chunksCount = new iVector3(worldSize.x / chunkSize.x , worldSize.y / chunkSize.y ,  worldSize.z / chunkSize.z );
            if (worldSize.x > chunksCount.x * chunkSize.x) chunksCount.x++;
            if (worldSize.y > chunksCount.y * chunkSize.y) chunksCount.y++;
            if (worldSize.z > chunksCount.z * chunkSize.z) chunksCount.z++;

            size = chunksCount.x;
            size2 = chunksCount.x * chunksCount.y;
            chunks = new MeshChunk[chunksCount.x * chunksCount.y * chunksCount.z];
            worldCellIndexList = new iVector3[worldSize.x* worldSize.y* worldSize.z];
            for (int x = 0, j = 0; x < worldSize.x; x++)
                for (int y = 0; y < worldSize.y; y++)
                    for (int z = 0; z < worldSize.z; z++)
                        worldCellIndexList[j++] = new iVector3(x, y, z);

            int ch = 0;
            for (int x = 0; x < chunksCount.x; x++)
                for (int y = 0; y < chunksCount.y; y++)
                    for (int z = 0; z < chunksCount.z; z++)
                        chunks[x + y * size + z * size2] = new MeshChunk(ch++,this, new iVector3(x, y, z), chunkSize);
           
            merged_cells = new MeshChunkCell[worldSize.x * worldSize.y * worldSize.z];
            merged_view_types = new  MergedViewType[worldSize.x * worldSize.y * worldSize.z];

            merged_cells_ids = new long[worldSize.x * worldSize.y * worldSize.z];
            subFaceCells.Clear();
            merge_size = worldSize.x;
            merge_size2 = worldSize.x * worldSize.y;

            builder = GameObject.FindObjectOfType<MeshChunkBuilder>();

           //tree = new BoundsOctree<MeshChunkCell>(worldSize.x * chunkSize.x, new Vector3(worldSize.x, worldSize.y, worldSize.z) / 2, leafSize, 1); // origin

            // pivot in mezzo

            pivot_pos = -bounds.center;

            foreach (var layer in layers)
                layer.Resize(newWorldSize, leafSize, true);
            foreach (var layer in tempLayers)
                layer.layer.Resize(newWorldSize, leafSize, false);

        }

        public IEnumerable<MeshChunkCell> Cells()
        {
            return merged_cells.Where( X => X!=null);
        }

        public static bool intersect(Vector3 c, float radius, Bounds box, ref Vector3 hitVector)
        {
            // get box closest point to sphere center by clamping
            var x = Mathf.Max(box.min.x, Mathf.Min(c.x, box.max.x));
            var y = Mathf.Max(box.min.y, Mathf.Min(c.y, box.max.y));
            var z = Mathf.Max(box.min.z, Mathf.Min(c.z, box.max.z));

            hitVector = new Vector3((x - c.x), (y - c.y), (z - c.z));

            return hitVector.magnitude < radius;
        }

        public Vector3 localToWorldPoint(Vector3 v)
        {
            var localtoWorld = root.localToWorldMatrix;// * buildTrx.inverse;
            return localtoWorld.MultiplyPoint(v);
        }

        public Vector3 localToWorldVector(Vector3 v)
        {
            var localtoWorld = root.localToWorldMatrix;// * buildTrx.inverse;
            return localtoWorld.MultiplyVector(v);
        }
        public Vector3 worldToLocalPoint(Vector3 v)
        {
            var worldToLocal = root.localToWorldMatrix.inverse; // buildTrx * ..
            return worldToLocal.MultiplyPoint(v);
        }
        public Vector3 worldToCellPoint(Vector3 v)
        {
            var worldToLocal = root.localToWorldMatrix.inverse; // buildTrx * ..
            return worldToLocal.MultiplyPoint(v)/ leafSize;
        }

        public Vector3 GetLocalToCell(Vector3 localPosition)
        {
            return  localPosition / leafSize;
        }

        public bool GetCellIndexAt(Vector3 localPosition, ref iVector3 index)
        {
            iVector3 p = new iVector3((int)(localPosition.x / leafSize), (int)(localPosition.y / leafSize), (int)(localPosition.z / leafSize));

            if (p.x < 0 || p.y < 0 || p.z < 0)
                return false;
            if (p.x >= worldSize.x || p.y >= worldSize.y || p.z >= worldSize.z) return false;

            index = p;

            //  Debug.Log("cell at " + localPosition +"=> "+ index);

            return true;
        }

        public Vector3 GetLocal_Clamped(Vector3 localPosition)
        {
            return new Vector3(Mathf.Clamp(localPosition.x, 0, realSize.x - 1), Mathf.Clamp(localPosition.y, 0, realSize.y - 1), Mathf.Clamp(localPosition.z, 0, realSize.z - 1));
        }

        public iVector3 GetCellIndexAt_Clamped(Vector3 localPosition)
        {
            iVector3 p = new iVector3((int)(localPosition.x / leafSize), (int)(localPosition.y / leafSize), (int)(localPosition.z / leafSize));

            p = new iVector3(Mathf.Clamp(p.x,0, worldSize.x -1), Mathf.Clamp(p.y, 0, worldSize.y - 1), Mathf.Clamp(p.z, 0, worldSize.z - 1));
        
            //  Debug.Log("cell at " + localPosition +"=> "+ index);

            return p;
        }
        public bool GetCellIndexAtFromWorld(Vector3 worldPosition, ref iVector3 index)
        {
            Vector3 local = worldToLocalPoint(worldPosition);
            return GetCellIndexAt(local, ref index);
        }

        public Vector3 GetCellLocalCenter(iVector3 cellIndex)
        {
            Vector3 pos = new Vector3(leafSize * 0.5f + cellIndex.x * leafSize, leafSize * 0.5f + cellIndex.y * leafSize, leafSize * 0.5f + cellIndex.z * leafSize);
            return (pos);
        }
        public Vector3 GetCellLocal(iVector3 cellIndex)
        {
            Vector3 pos = new Vector3( cellIndex.x * leafSize,cellIndex.y * leafSize+ leafSize * 0.5f,  cellIndex.z * leafSize );
            return (pos);
        }
        public Vector3 GetCellLocal(Vector3 cellIndex)
        {
            Vector3 pos = new Vector3(cellIndex.x * leafSize, cellIndex.y * leafSize + leafSize * 0.5f, cellIndex.z * leafSize);
            return (pos);
        }
        public Vector3 GetCellWorldCenter(iVector3 cellIndex)
        {
            Vector3 pos = new Vector3(leafSize * 0.5f + cellIndex.x * leafSize, leafSize * 0.5f + cellIndex.y * leafSize, leafSize * 0.5f + cellIndex.z * leafSize);
            return localToWorldPoint(pos);
        }

        public iRectangle GetCellRectangleFromLocal(Bounds localBounds)
        {
            var min = new iVector3(localBounds.min);// + new Vector3(1,1,1));
            var max = new iVector3(localBounds.max);// - new Vector3(1, 1, 1));
            min.x = Mathf.Max(min.x, 0);
            min.y = Mathf.Max(min.y, 0);
            min.z = Mathf.Max(min.z, 0);
            max.x = Mathf.Min(max.x, worldSize.x - 1);
            max.y = Mathf.Min(max.y, worldSize.y - 1);
            max.z = Mathf.Min(max.z, worldSize.z - 1);
            return new iRectangle(min, max);
        }


        public void Clear(bool clearChunks, bool clearLayers =true)
        {
            //  chunks = new MeshChunk[chunksCount.x * chunksCount.y * chunksCount.z];
            if (clearChunks)
            {
                foreach (var chunk in chunks)
                {
                    chunk.Clear();
                }
            }
          
            if (clearLayers)
            {
                while (layers.Count > 1) layers.RemoveAt(layers.Count - 1);
                layers[0].Clear();
            }

            //tree = new BoundsOctree<MeshChunkCell>(worldSize.x * chunkSize.x, new Vector3(worldSize.x, worldSize.y, worldSize.z) / 2, leafSize, 1); // origin +
            builder.Clear();
        }

        public bool EmptyCell(iVector3 cell_index)
        {
            var cell = GetCell(cell_index);
            if (cell != null)
            {
                cell.isVisible = false;
                if (modEnabled)
                    builder.Invalidate(cell.chunk, cell_index - cell.chunk.startWorldPosition, true);
                else
                    InvalidateMerged(cell);
              //  tree.Remove(cell);
                //if (cell.bonus)
                //    GameObject.Destroy(cell.bonus);
                return true;
            }
            else return false;
        }

        public bool RefillCell(iVector3 cell_index)
        {
            var c = GetCell(cell_index);
            c.isVisible = true;
            SetMergedChunkCell(cell_index,c,true,true);
            return true;
        }

        //public void HitCell(iVector3 cell_index)
        //{
        //    var cell = GetCell(cell_index);
        //    if (cell != null)
        //    {
               
        //    }
        //}

        /// <summary>
        /// se ce ne' solo uno visibile lo forzo
        /// </summary>
        /// <returns></returns>
        public int ForcedVisibleLayer()
        {
			int vc = 0;
			foreach (var l in layers)
				vc += l.visible ? 1 : 0;
			if (vc == 1)
				return layers.First(X => X.visible).layerIndex;
			else
				return -1;
        }

        public void Invalidate()
        {
         //   RebuildAllCells();
            mustRebuildCells = true;
        }
        public void InvalidateMerged(MeshChunkCell cell)
        {
            //   RebuildAllCells();
            CellCoordinate coord;
            if (worldToChunk(cell.worldPosition, out coord))
                builder.Invalidate(cell.chunk, coord.localCoord, false);

        }

        /// <summary>
        /// rifà tutto
        /// </summary>
        public void RebuildAllLayerCells()//bool updateMesh=true)
        {
            mustRebuildCells = false;
            Clear(true,false);

            int forceLayer = ForcedVisibleLayer();
            foreach(var idx in worldCellIndexList)
            {
                BuildChunkCellMerged(idx, forceLayer,false,false);
            }

            //if (updateMesh)
            //     RebuildMesh();
        }

        public void RebuildMesh(bool clearCache=false)
		{
           // if (mustRebuildCells)
            //    RebuildAllCells(false);

            // forse rebuild
            // RebuildAllCells(false);

            //if (clearCache)
            //    root.transform.Clear();

            builder.Build(root.gameObject, this, new Vector3(0, 0, 0), new Vector3(leafSize * chunkSize.x, leafSize * chunkSize.y, leafSize * chunkSize.z), new Vector3(leafSize, leafSize, leafSize));
            
        }

        public void BuildChunkCellMerged( iVector3 cell_index,int forceVisibleLayer,bool enableCutMode,bool fireEvents)
        {
            // valuto la cella del chunk come fuzione della info dei layers
            MeshChunkCell chunkCell = null;
            int c = layers.Count;
            for (int i = 0; i < c; i++)
            {
                var layer = layers[i];
                if ((forceVisibleLayer==-1 && layer.active) || (forceVisibleLayer>=0  && forceVisibleLayer==i))
                {
                    var cell = layer.GetCell(cell_index);
                    if (cell != null)
                    {
                        if (chunkCell == null)
                        {
                            if (layer.operation == LayerOperation.Additive || forceVisibleLayer>=0)
                                chunkCell = cell;
                        }
                        else
                        {
                            // merge ?? 
                            if (layer.operation == LayerOperation.Additive || forceVisibleLayer >= 0)
                                chunkCell = cell;
                            else
                                chunkCell = null;
                        }
                    }
                }
            }

            // TEMP DOPO TUTTO
         
            chunkCell = WriteTempCell(cell_index, chunkCell);

            // set on all

            SetMergedChunkCell(cell_index, chunkCell, enableCutMode, fireEvents);
        }

        public MeshChunkCell WriteTempCell(iVector3 cell_index, MeshChunkCell prevValue)
        {
            MeshChunkCell chunkCell = prevValue;
            // TEMP DOPO TUTTO
            for (int i = 0; i < tempLayers.Count; i++)
            {
                var layer = tempLayers[i].layer;
                if (!layer.active)
                    continue;
                if (layer.operation == LayerOperation.Copy)
                {
                    // rimane il precedente
                }
                else
                {
                    var cell = layer.GetCell(cell_index);
                    if (cell != null)
                    {
                        //cell.isTempLayer = true;
                        if (layer.operation == LayerOperation.Additive)
                            chunkCell = cell;
                        else if (layer.operation == LayerOperation.Substract)
                            chunkCell = null;
                        else if (layer.operation == LayerOperation.FilterLogicAnd)
                        {
							if (chunkCell!=null)
							{
							//	NOP
								  chunkCell = cell;
							}
							//else
							//	chunkCell = null;
						}
                        else if (layer.operation == LayerOperation.AddIfEmpty)
                        {
                            if (chunkCell == null)
                            {
                                chunkCell = cell;
                            }
                        }
						else if (layer.operation == LayerOperation.AddLogicAnd)
						{
                            if (chunkCell != null)
                            {
                                //	NOP
                                chunkCell = cell;
                            }
                        }
					}
                    else
                    {
                        if (layer.operation == LayerOperation.FilterLogicAnd)
                        {
                            if (chunkCell != null) chunkCell = null;
                        }
                        //else if (layer.operation == LayerOperation.SubstractLogicAnd)
                        //{
                        //    if (chunkCell != null) chunkCell = null;
                        //}
                    }
                }
            }
            return chunkCell;
        }

		/// <summary>
		/// layer event
		/// </summary>
		public void OnSetCellByLayer(MeshChunkLayer layer, iVector3 cell_index, bool enableCutMode)
		{
			// valuto la cella del chunk come funzione della info dei layers, non invalida tutto 

			BuildChunkCellMerged(cell_index, forceVisibleLayer, enableCutMode, modEnabled);

		}

		/// <summary>
		/// scrittura finale
		/// </summary>
		void SetMergedChunkCell(iVector3 cell_index, MeshChunkCell cell,bool enableCutMode, bool fireEvents)
        {
            MeshChunkCell removedCell = null;
            CellCoordinate coord;
            if (worldToChunk(cell_index, out coord))
            {
                var chunk = chunks[coord.chunk.x + coord.chunk.y * size + coord.chunk.z * size2];

                removedCell = chunk.Get(coord.localCoord);

                // e' cambiato ?? 

                bool isChanged = chunk.Set(coord.localCoord, cell);

                merged_cells[cell_index.x + cell_index.y * merge_size + cell_index.z * merge_size2] = cell;

                if (isChanged && fireEvents)//|| modEnabled)
                {
                    builder.Invalidate(chunk, coord.localCoord, enableCutMode && cell == null);
                    if (OnInvalidate != null) OnInvalidate(coord.localCoord);
                }

                // sub faces  manage
                var prev_isSub = subFaceCells.ContainsKey(cell_index);
                bool isSub = (cell != null && !cell.cellColor().isSolidColor);
                if (isSub != prev_isSub)
                {
                    if (isSub) subFaceCells.Add(cell_index, cell);
                    else subFaceCells.Remove(cell_index);
                    
                   // builder.InvalidateSubMaterials();
                }
                else
                {
                    if (isSub) 
                        subFaceCells[cell_index] = cell;
                   // builder.InvalidateSubMaterials();
                }
                
                //if (!commitChunkList.Contains(chunk))
                //    commitChunkList.Add(chunk);

               // if (true)
                {
                    if (cell != null && !cell.bounds.HasValue)
                    {
                        // var bounds = new Bounds(new Vector3(origin.x + leafSize * cell_index.x, origin.y + leafSize * cell_index.y, origin.z + leafSize * cell_index.z), new Vector3(leafSize, leafSize, leafSize));
                        
                        // in local coordinate

                        var bounds = new Bounds(new Vector3(leafSize * 0.5f + leafSize * cell_index.x, leafSize * 0.5f + leafSize * cell_index.y, leafSize * 0.5f + leafSize * cell_index.z), new Vector3(leafSize, leafSize, leafSize));

                        cell.bounds = bounds;
                        // add or replace ?? 
                        //if (removedCell != null)
                        //    tree.Remove(removedCell);
                        //tree.Add(cell, bounds);
                    }
                    //else if (removedCell != null)
                    //{
                    //    tree.Remove(removedCell);
                    //    //if (removedCell.bonus)
                    //    //    GameObject.Destroy(removedCell.bonus);
                    //}
                }
            }
        }

        //public bool IsLocalValid(Vector3 localPos)
        //{
        //    if (localPos.x < 0 || localPos.y < 0 || localPos.z < 0) return false;
        //    if (localPos.x >= realSize.x || localPos.y >= realSize.y || localPos.z >= realSize.z) return false;
        //    return true;
        //}
        public bool IsValid(Vector3 cellPos)
        {
            if (cellPos.x < 0 || cellPos.y < 0 || cellPos.z < 0) return false;
            if (cellPos.x >= worldSize.x || cellPos.y >= worldSize.y || cellPos.z >= worldSize.z) return false;
            return true;

        }
        public bool IsValid(iVector3 cellPos)
        {
            if (cellPos.x < 0 || cellPos.y < 0 || cellPos.z < 0) return false;
            if (cellPos.x >= worldSize.x || cellPos.y >= worldSize.y || cellPos.z >= worldSize.z) return false;
            return true;

        }

        public bool IsOnBound(iVector3 cellPos)
        {
            if (cellPos.x == 0 || cellPos.y == 0 || cellPos.z == 0
                || cellPos.x == worldSize.x - 1 || cellPos.y == worldSize.y - 1 || cellPos.z == worldSize.z - 1) return true;
            else
                return false;

        }
        public bool IsFull(iVector3 cell_index)
        {
            CellCoordinate coord;
            if (worldToChunk(cell_index, out coord))
            {
                //var coord = worldToChunk(cell_index);
                var chunk = chunks[coord.chunk.x + coord.chunk.y * size + coord.chunk.z * size2];
                var c = chunk.Get(coord.localCoord);
                return c != null && c.isExisting;
            }
            else return true;
        }


        public MeshChunkCell GetCell(iVector3 cell_index)
        {
            CellCoordinate coord;
            if (worldToChunk(cell_index, out coord))
            {
                //  var coord = worldToChunk(cell_index);
                var chunk = chunks[coord.chunk.x + coord.chunk.y * size + coord.chunk.z * size2];
                return chunk.Get(coord.localCoord);
            }
            else
                return null;
        }

        public MeshChunkCell GetCell(int x, int y, int z)
        {
            CellCoordinate coord;
            if (worldToChunk(new iVector3(x, y, z), out coord))
            {
                //var coord = worldToChunk(new iVector3(x, y, z));
                var chunk = chunks[coord.chunk.x + coord.chunk.y * size + coord.chunk.z * size2];
                return chunk.Get(coord.localCoord);
            }
            else
                return null;
        }

        public bool GetCellPosFromLocal(Vector3 pos, ref iVector3 cellPos)
        {
            var p = new iVector3((int)(pos.x + 0.001f), (int)(pos.y + 0.001f), (int)(pos.z + 0.001f));
            if (p.x < 0 || p.y < 0 || p.z < 0)
                return false;
            if (p.x >= worldSize.x || p.y >= worldSize.y || p.z >= worldSize.z) return false;
            cellPos = p;
            return true;
        }

        public MeshChunk GetChunk(iVector3 index)
        {
            return chunks[index.x + index.y * size + index.z * size2];
        }
        public MeshChunk GetChunk(int x,int y,int z)
        {
            return chunks[x + y * size + z * size2];
        }
        public MeshChunkCell GetCellMerged(iVector3 worldIndex)
        {
            return merged_cells[worldIndex.x + worldIndex.y * merge_size + worldIndex.z * merge_size2];
        }
        public MeshChunkCell GetCellMerged(iVector3 worldIndex, iVector3 offset)
        {
            var p = worldIndex + offset;
            if (p.x >= 0 && p.y >= 0 && p.z >= 0 && p.x < worldSize.x && p.y < worldSize.y && p.z < worldSize.z)
                return merged_cells[p.x + p.y * merge_size + p.z * merge_size2];
            else
                return null;
        }

        
        public MergedViewType GetCellMergedView(iVector3 worldIndex)
        {
            return merged_view_types[worldIndex.x + worldIndex.y * merge_size + worldIndex.z * merge_size2];
        }
        public void SetCellMergedView(int x, int y, int z, MergedViewType view)
        {
            merged_view_types[x + y * merge_size + z * merge_size2] = view;
        }
        public void ClearCellMergedView()
        {
            for (int i = 0; i < merged_view_types.Length; i++) merged_view_types[i] = MergedViewType.Normal;
        }

        public MeshChunkCell GetCellMerged(int x,int y,int z)
        {
            if (x < 0 || y < 0 || z < 0)
                return null;
            if (x >= worldSize.x || y >= worldSize.y ||z >= worldSize.z) return null;

            return merged_cells[x + y * merge_size + z * merge_size2];
        }
        public void GetCellLayerMerged( int y,List<MeshChunkCell> cells)
        {
            for (int x = 0; x < worldSize.x; x++)
                for (int z = 0; z < worldSize.z; z++)
                {
                    var cell = GetCellMerged(x, y, z);
                    if (cell != null)
                        cells.Add(cell);
                }
	    }

        public bool worldToChunk(iVector3 worldPos,out CellCoordinate cellCoord)
        {
            cellCoord = null;
            if (worldPos.x < 0 || worldPos.y < 0 || worldPos.z < 0)
                return false; //throw new Exception("bad world coordinate <:" + worldPos);
            if (worldPos.x >= worldSize.x || worldPos.y >= worldSize.y || worldPos.z >= worldSize.z)
                return false;
                //throw new Exception("bad world coordinate >:" + worldPos);

            CellCoordinate coord = new CellCoordinate();
            coord.chunk = new iVector3((int)((float)worldPos.x / chunkSize.x), (int)((float)worldPos.y / chunkSize.y), (int)((float)worldPos.z / chunkSize.z));
            coord.localCoord = new iVector3(worldPos.x - coord.chunk.x * chunkSize.x, worldPos.y - coord.chunk.y * chunkSize.y, worldPos.z - coord.chunk.z * chunkSize.z);
            //return coord;
            cellCoord = coord;
            return true;
        }

        public void BeginChange()
        {
            modEnabled = true;
            forceVisibleLayer = ForcedVisibleLayer();
        }

        public void CommitChange(bool forceRedraw=false)
        {
            modEnabled = false;
            if (forceRedraw)
                RebuildMesh();
        }

        #region COLLIDE

        /// <summary>
        /// in local coordinate
        /// </summary>
        public void GetColliding(List<MeshChunkCell> list, Bounds bounds )
        {
           // Debug.Log("COLL "+ bounds);

            iVector3 start = (bounds.min.Div(leafSize)).ToiVector3().Max(iVector3.zero);
            iVector3 end = (bounds.max + new Vector3(1, 1, 1)).Div(leafSize).ToiVector3().Min(worldSize);

            for(int x= start.x;x< end.x;x++)
                for (int y = start.y; y < end.y; y++)
                    for (int z = start.z; z < end.z; z++)
					{
                        var c = merged_cells[x + y * merge_size + z * merge_size2];
                        if (c != null)
                            list.Add(c);
                    }

        }

        /// <summary>
        /// /// in local coordinate
        /// </summary>
        public void GetColliding(List<MeshChunkCell> list, Ray ray)
        {
         //   Debug.Log("COLL ray " + ray);

            float dist,dist_f;
            if (bounds.IntersectRay(ray,out dist))
            {
                var startPoint = ray.GetPoint(dist + leafSize * 0.25f) ;
                var invRay = new Ray(ray.origin + ray.direction * 1000, -ray.direction);
                if (bounds.IntersectRay(invRay, out dist_f))
                {
                    var endPoint = invRay.GetPoint(dist_f + leafSize*0.5f); // rimane dentro

                    // to brick coordinate
                    startPoint = startPoint.Div(leafSize);
                    endPoint = endPoint.Div(leafSize);
                    var distance = (endPoint - startPoint).magnitude;

                 //   Debug.Log("hit  " + startPoint + " dist " + distance);

                    if (distance > 0)
                    {
                        // traverse
                        var dir = (endPoint-startPoint );
                        float size = dir.magnitude * 5;
                        var step = dir.normalized / 5;
                        iVector3 oldP = new iVector3(-1, -1, -1);
                        for (int i = 0; i <= (int)size; i++)
                        {
                            var p = startPoint + step * i;
                            iVector3 l = p.ToiVector3();
                            if (!l.Equals(oldP) 
                                && l.x >=0  && l.y >= 0 && l.z >= 0
                                && l.x <= maxCoordinate.x && l.y <= maxCoordinate.y && l.z <= maxCoordinate.z)
                            {
                                oldP = l;
                               // Debug.Log("test " + l);
                                var c = merged_cells[l.x + l.y * merge_size + l.z * merge_size2];
                                if (c != null)
                                    list.Add(c);
                            }
                        }
                    }
                }

            }


			//var dir = to - from;
			//float size = dir.magnitude * 5;
			//var step = dir.normalized / 5;
			//iVector3 oldP = new iVector3(-1, -1, -1);
			//for (int i = 0; i <= (int)size; i++)
			//{
			//    var p = from + step * i;
			//    iVector3 l = new iVector3(p);
			//    if (!l.Equals(oldP))
			//    {
			//        oldP = l;
			//        DrawPoint(mesh, layer, tool, new iVector3(p), addMode ? ToolBrushWriteMode.AddFG : ToolBrushWriteMode.Del);
			//    }
			//}

		//	Debug.Log("COLL ray " + ray);
        }

        public MeshChunkPhysicHit[] CollideAll(BoundingSphere wsphere)
        {
            List<MeshChunkPhysicHit> hitList = new List<MeshChunkPhysicHit>();

            var worldToLocal = root.localToWorldMatrix.inverse; // buildTrx * ..

            //var localRadius = worldToLocal.MultiplyVector(new Vector3(wsphere.radius, wsphere.radius, wsphere.radius));

            //BoundingSphere localSphere = new BoundingSphere(worldToLocal.MultiplyPoint(wsphere.position), localRadius.x);
            BoundingSphere local_sphere = new BoundingSphere(worldToLocal.MultiplyPoint(wsphere.position), worldToLocal.ExtractScale().x * wsphere.radius);

            List<MeshChunkCell> list = new List<MeshChunkCell>();
            GetColliding(list, new Bounds(local_sphere.position, new Vector3(local_sphere.radius * 2, local_sphere.radius * 2, local_sphere.radius * 2)));
            foreach (var c in list)
            {
                Vector3 hitv = new Vector3();
                if (intersect(local_sphere.position, local_sphere.radius, c.bounds.Value, ref hitv))
                {
                    MeshChunkPhysicHit hit = new MeshChunkPhysicHit();
                    hit.cell = c;
                    hit.cellIndex = c.worldPosition;
                    hit.point = worldToLocal.inverse.MultiplyPoint(local_sphere.position + hitv);
                    if (Mathf.Abs(hitv.x) > Mathf.Abs(hitv.y) && Mathf.Abs(hitv.x) > Mathf.Abs(hitv.z))
                        hit.normal = new Vector3(-Mathf.Sign(hitv.x), 0, 0);
                    else if (Mathf.Abs(hitv.y) > Mathf.Abs(hitv.z))
                        hit.normal = new Vector3(0, -Mathf.Sign(hitv.y), 0);
                    else
                        hit.normal = new Vector3(0, 0, -Mathf.Sign(hitv.z));
                    hit.normal = worldToLocal.inverse.MultiplyVector(hit.normal);
                    hitList.Add(hit);
                }
            }
            return hitList.ToArray();
        }


        public bool Collide(BoundingSphere wsphere, out MeshChunkPhysicHit hit)
        {
            var worldToLocal = root.localToWorldMatrix.inverse; // buildTrx * ..


            BoundingSphere local_sphere = new BoundingSphere(worldToLocal.MultiplyPoint(wsphere.position), worldToLocal.ExtractScale().x * wsphere.radius);

            hit = new MeshChunkPhysicHit();

            List<MeshChunkCell> list = new List<MeshChunkCell>();
            GetColliding(list, new Bounds(local_sphere.position, new Vector3(local_sphere.radius * 2, local_sphere.radius * 2, local_sphere.radius * 2)));
            if (list.Count > 0)
            {
                MeshChunkCell bestCell = null;
                float bestDist = 999999;
                Vector3 hitVector = Vector3.zero;
                foreach (var c in list)
                {
                    //Debug.Log("dist=" + c.bounds.center);

                    Vector3 hitv = new Vector3();
                    if (intersect(local_sphere.position, local_sphere.radius, c.bounds.Value, ref hitv))
                    {
                        //Debug.Log("hit=" + c.bounds.center);

                        if (hitv.magnitude < bestDist)
                        {
                            bestDist = hitv.magnitude;
                            bestCell = c;
                            hitVector = hitv;
                        }
                    }

                }
                if (bestCell != null)
                {
                    hit.cell = bestCell;
                    hit.cellIndex = bestCell.worldPosition;
                    hit.point = worldToLocal.inverse.MultiplyPoint(local_sphere.position + hitVector);
                    if (Mathf.Abs(hitVector.x) > Mathf.Abs(hitVector.y) && Mathf.Abs(hitVector.x) > Mathf.Abs(hitVector.z))
                        hit.normal = new Vector3(-Mathf.Sign(hitVector.x), 0, 0);
                    else if (Mathf.Abs(hitVector.y) > Mathf.Abs(hitVector.z))
                        hit.normal = new Vector3(0, -Mathf.Sign(hitVector.y), 0);
                    else
                        hit.normal = new Vector3(0, 0, -Mathf.Sign(hitVector.z));

                    hit.normal = worldToLocal.inverse.MultiplyVector(hit.normal);
                    return true;
                }
                else
                    return false;
            }
            //int layerMask = 1 << LayerMask.NameToLayer("Chunk");

            //foreach (RaycastHit rhit in  Physics.RaycastAll(ray, 9999999, layerMask))
            //{
            //    int y = 0;
            //  //  Debug.Log("hit  = " + rhit.collider.GetComponent<MeshChunkRef>().position);


            //    hit = new MeshChunkPhysicHit();
            //    hit.point = rhit.point;

            //    return true;
            //}

            return false;
        }

        public bool Collide(BoxCollider collider, out MeshChunkPhysicHit hit)
        {
            hit = new MeshChunkPhysicHit();

            List<MeshChunkCell> list = new List<MeshChunkCell>();
            GetColliding(list, collider.bounds);
            if (list.Count > 0)
            {
                MeshChunkCell bestCell = null;
                float bestDist = 999999;
                //foreach (var c in list)
                //{
                //    float dist;
                //   // if (c.bounds.co(ray, out dist))
                //    {
                //        if (dist < bestDist)
                //        {
                //            bestDist = dist;
                //            bestCell = c;
                //        }
                //    }
                //}
                //if (bestCell != null)
                //{
                //    hit.point = ray.GetPoint(bestDist);
                //    return true;
                //}
                //else
                //    return false;
            }

            return false;
        }


        static Vector3[] dirs = new Vector3[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1), new Vector3(0, 0, -1) };

        int GetFace(float x,float y)
		{
            return( Mathf.Sign(x) > 0 ? 1 : 0)+ 2 * (Mathf.Sign(y) > 0 ? 1 : 0);

        }
        public bool RaycastFace(Ray worldRay, out MeshChunkPhysicHit hit, out int faceIdx, out int subColorIdx)
        {
            faceIdx = -1;
            subColorIdx = -1;
            if (Raycast(worldRay, out hit))
            {
                var N = hit.normal;
               
                Vector3 centerPoint = N* leafSize*0.5f;
                centerPoint = ( hit.cell.worldPosition.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f)) * leafSize + centerPoint;

                var worldToLocal = root.localToWorldMatrix.inverse;
                var localHitPoint = worldToLocal.MultiplyPoint( hit.point);

                var dir = (localHitPoint - centerPoint);

                if (N.z > 0.5f) { faceIdx = 1; subColorIdx = GetFace(-dir.x, dir.y); } // back
                else if (N.z < -0.5f) { faceIdx = 0; subColorIdx = GetFace(dir.x, dir.y); } // front

                else if (N.x > 0.5f) { faceIdx = 5; subColorIdx = GetFace(dir.z, dir.y); } // right
                else if (N.x < -0.5f) { faceIdx = 4; subColorIdx = GetFace(-dir.z, dir.y); } // left

                else if (N.y > 0.5f) { faceIdx = 2; subColorIdx = GetFace(dir.x, dir.z); } // top
                else if (N.y < -0.5f) { faceIdx = 3; subColorIdx = GetFace(dir.x, dir.z); } // bottom


                //Debug.Log("w:" + localHitPoint + "cp:"+ centerPoint+" dir:" +dir);
                //float dist = 0;
                //new Plane(N, hit.point).Raycast(ray, dist);
                //var hitPoint = ray.get
                return true;
            }
            else
                return false;
        }

        public bool Raycast(Ray worldRay, out MeshChunkPhysicHit hit)
        {
            hit = new MeshChunkPhysicHit();

            //Ray ray = worldRay;
            var worldToLocal = root.localToWorldMatrix.inverse; 
            Ray localRay = new Ray(worldToLocal.MultiplyPoint(worldRay.origin), worldToLocal.MultiplyVector(worldRay.direction));
         
            // worldRay
            List<MeshChunkCell> list = new List<MeshChunkCell>();
            GetColliding(list, localRay);

            if (list.Count > 0)
            {
                MeshChunkCell bestCell = null;
                float bestDist = 999999;
                foreach (var c in list)
                {
                    float dist;
                    if (GetCellMergedView(c.worldPosition)  == MergedViewType.Normal && c.bounds.Value.IntersectRay(localRay, out dist))
                    {
                        if (dist < bestDist)// && !c.isTempLayer)
                        {
                            bestDist = dist;
                            bestCell = c;
                        }
                    }
                }
                if (bestCell != null)
                {
                   // Debug.Log("hit " + bestCell.worldPosition +" "+ bestCell.cellColor());
                    hit.cell = bestCell;
                    hit.cellIndex = bestCell.worldPosition;
                    hit.point = worldRay.GetPoint(bestDist * root.localToWorldMatrix.lossyScale.x);
                    var hitVector = -worldRay.direction;
                    float best = 99999;
                    foreach (var dir in dirs)
                    {
                        float d = Mathf.Abs(Vector3.Distance(hit.point, bestCell.position + dir));
                        if (d < best)
                        {
                            best = d;
                            hit.normal = dir;
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
#endregion

#region FIND
        /// <summary>
        ///  solo l'intorno immediato
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="onlyVisible"></param>
        /// <returns></returns>
        public MeshChunkCell[] GetNearest(MeshChunkCell _from,bool onlyVisible=true)
        {
            List<MeshChunkCell> l = new List<MeshChunkCell>();
            var from = _from.worldPosition;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        if (from.x + x >= 0 && from.y + y >= 0 && from.z + z >= 0
                            && from.x + x < worldSize.x &&   from.y + y < worldSize.y &&   from.z + z < worldSize.z)
                        {
                            var cell = GetCell(from.x + x, from.y + y, from.z + z);
                            if (cell != null && ((onlyVisible && cell.isExisting) || (!onlyVisible)))
                                l.Add(cell);
                        }

                    }
                }
            }
            return l.ToArray();
        }

        // get neibourn axis linked
        public MeshChunkCell[] GetAxisLinked(iVector3 from, bool onlyVisible = true)
        {
            List<MeshChunkCell> list = new List<MeshChunkCell>();

            if (from.x > 0)
            {
                var cell = GetCell(from.x - 1, from.y, from.z);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }
            if (from.x < worldSize.x - 2)
            {
                var cell = GetCell(from.x + 1, from.y, from.z);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }

            if (from.y > 0)
            {
                var cell = GetCell(from.x, from.y - 1, from.z);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }
            if (from.y < worldSize.y - 2)
            {
                var cell = GetCell(from.x, from.y + 1, from.z);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }

            if (from.z > 0)
            {
                var cell = GetCell(from.x, from.y, from.z - 1);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }
            if (from.z < worldSize.z - 2)
            {
                var cell = GetCell(from.x, from.y, from.z + 1);
                if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
                {
                    list.Add(cell);
                }
            }

            //for (int x = -1; x <= 1; x += 2)
            //{
            //    var cell = GetCell(cellPos.x + x, cellPos.y, cellPos.z);
            //    if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
            //        l.Add(cell);
            //}

            //for (int y = -1; y <= 1; y += 2)
            //{
            //    var cell = GetCell(cellPos.x , cellPos.y+y, cellPos.z);
            //    if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
            //        l.Add(cell);

            //}
            //for (int z = -1; z <= 1; z += 2)
            //{
            //    var cell = GetCell(cellPos.x, cellPos.y , cellPos.z + z);
            //    if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
            //        l.Add(cell);

            //}
            return list.ToArray();
        }

        /// <summary>
        /// only visible
        /// </summary>
        public void _GetNear(MeshChunkCell _from, Func<MeshChunkCell, MeshChunkCell, bool> cond, List<MeshChunkCell> list)
        {
            //  var layer = layers[from.z];
            var from = _from.worldPosition;


            if (from.x > 0)
            {
                var next = GetCell(from.x - 1, from.y, from.z);

                if (next!=null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }
            if (from.x < worldSize.x-1)
            {
                var next = GetCell(from.x + 1, from.y, from.z);
                if (next != null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }

            if (from.y > 0)
            {
                var next = GetCell(from.x, from.y-1, from.z);
                if (next != null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }
            if (from.y < worldSize.y - 1)
            {
                var next = GetCell(from.x, from.y+1, from.z);
                if (next != null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }

            if (from.z > 0)
            {
                var next = GetCell(from.x, from.y, from.z-1);
                if (next != null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }
            if (from.z < worldSize.z - 1)
            {
                var next = GetCell(from.x, from.y, from.z+1);
                if (next != null && next.isVisible && 
                    cond(GetCell(from), next)
                    && !list.Contains(next))
                {
                    list.Add(next);
                    _GetNear(next, cond, list);
                }
            }
        }

        /// <summary>
        /// only visible
        /// </summary>
        /// <returns></returns>
        public List<MeshChunkCell> GetNear(MeshChunkCell from, Func<MeshChunkCell, MeshChunkCell, bool> cond)
        {
            var list = new List<MeshChunkCell>();
            list.Add(from);
            _GetNear(from, cond, list);
            return list.Distinct().ToList();
        }

    
    }
    #endregion
}
