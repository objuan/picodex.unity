using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{
    public enum LayerOperation
    {
        Additive,
        Substract,
        AddIfEmpty,
        FilterLogicAnd,
        AddLogicAnd,
        Copy,
      //  Select
    }
    public enum MeshChunkLayerAxe
    {
        //   Undefined,
        X=0, Y, Z
    }
    public class MeshChunkLayer
    {
        public MeshWorld world;

        // PROPS
        public bool _edit_visible = true;
        public bool _visible = true;
        public bool _play = true;
        public LayerOperation _operation = LayerOperation.Additive;
        public string name = "Layer";
        public MeshChunkLayerAnimator animator = new MeshChunkLayerAnimator();
        public MeshChunkLayerScript script = new MeshChunkLayerScript();
        // 

        public int layerIndex => world.Layers.IndexOf(this);
        MeshChunkCell[] cells;

        public iVector3 chunksCount;
        public iVector3 worldSize;

        public int size;
        public int size2;
        public float leafSize;

        public float leafWorldSize => root.localToWorldMatrix.lossyScale.x * leafSize;

        public Vector3 pivot_pos = new Vector3(0, 0, 0);
        public Quaternion pivot_rot = Quaternion.identity;

        Transform root;

     //   public iVector3 maxCoordinate => worldSize + new iVector3(-1, -1, -1);

        public Vector3 realSize;

        public Bounds bounds;

        public bool active
        {
            get => visible && ((animator!=null) ? animator.enabled.value : true);
          
        }
        public bool visible
        {
            get => _visible;
			set
			{
				if (_visible != value)
				{
					_visible = value; OnChanged();
				}
			}
		}
        public bool play
        {
            get => _play;
            set
            {
                if (_play != value)
                {
                    _play = value; OnChanged();
                }
            }
        }

        public LayerOperation operation { get => _operation;
            set
            {
               // Debug.Log("value"+ value);
                if (_operation != value)
                {
                    _operation = value; 
                    OnChanged();
                }
            }
        }

        public MeshChunkLayer()
        {
        }

        public void Initialize(MeshWorld world, int layerIndex)
        {
            this.world = world;
            this.worldSize = world.worldSize;
            this.leafSize = world.leafSize;
            this.root = world.root;

            size = worldSize.x;
            size2 = worldSize.x * worldSize.y;
            cells = new MeshChunkCell[size2 * worldSize.z];
            pivot_pos = -bounds.center;

            realSize = new Vector3(leafSize * worldSize.x, leafSize * worldSize.y, leafSize * worldSize.z);
            bounds = new Bounds(new Vector3(leafSize * 0.5f * worldSize.x, leafSize * 0.5f * worldSize.y, leafSize * worldSize.z * 0.5f), realSize);
            // Resize(worldSize, leafSize,false);
        }

        public void PreUpdate()
        {
            if (animator != null)
                animator.PreUpdate(this);
            if (script != null)
                script.PreUpdate(this);
        }
        public void Update()
        {
            if (animator!=null)
                animator.Update(this);
            if (script!=null)
                script.Update(this);
        }
        public void OnApplicationQuit()
        {
            if (script != null)
                script.OnApplicationQuit(this);
        }

        void OnChanged()
        {
            this.world.Invalidate();
            //this.world.RebuildAllLayerCells();
        }

        /// <summary>
        /// preserva i lvalore vecchio 
        /// </summary>
        /// <param name="newWorldSize"></param>
        /// <param name="leafSize"></param>
        public void Resize(iVector3 newWorldSize,float leafSize,bool saveOld)
        {
            if (!newWorldSize.Equals(worldSize))
            {
                Debug.Log("Reshape layer " + layerIndex + " " + newWorldSize + " leaf: " + leafSize + " saveOld " + saveOld);

                if (saveOld)
                {
                    var old_cells = cells;
                    var old_size = size;
                    var old_size2 = size2;

                    size = newWorldSize.x;
                    size2 = newWorldSize.x * newWorldSize.y;
                    cells = new MeshChunkCell[size2 * newWorldSize.z];

                    for (int x = 0; x < worldSize.x; x++)
                        for (int y = 0; y < worldSize.y; y++)
                            for (int z = 0; z < worldSize.z; z++)
                            {
                                if (x < newWorldSize.x && y < newWorldSize.y && z < newWorldSize.z)
                                    cells[x + y * size + z * size2] = old_cells[x + y * old_size + z * old_size2];
                            }
                }
                else
                {
                    size = newWorldSize.x;
                    size2 = newWorldSize.x * newWorldSize.y;
                    cells = new MeshChunkCell[size2 * newWorldSize.z];
                }
                worldSize = newWorldSize;
                this.leafSize = leafSize;
                pivot_pos = -bounds.center;
                realSize = new Vector3(leafSize * worldSize.x, leafSize * worldSize.y, leafSize * worldSize.z);
                bounds = new Bounds(new Vector3(leafSize * 0.5f * worldSize.x, leafSize * 0.5f * worldSize.y, leafSize * worldSize.z * 0.5f), realSize);
            }
        }

        public IEnumerable<MeshChunkCell> Cells()
        {
            return cells;
        }

        public IEnumerable<iVector3> GetCellIndexes()
        {
            var size = world.worldSize;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        yield return new iVector3(x, y, z);
                    }
                }
            }
        }

        public bool GetCellPosFromLocal(Vector3 pos, ref iVector3 cellPos)
        {
            var p = new iVector3((int)(pos.x + 0.5f), (int)(pos.y + 0.5f), (int)(pos.z + 0.5f));
            if (p.x < 0 || p.y < 0 || p.z < 0)
                return false;
            if (p.x >= worldSize.x || p.y >= worldSize.y || p.z >= worldSize.z) return false;
            cellPos = p;
            return true;
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

        public Vector3 GetCellWorldCenter(iVector3 cellIndex)
        {
            Vector3 pos = new Vector3(leafSize * 0.5f + cellIndex.x * leafSize, leafSize * 0.5f + cellIndex.y * leafSize, leafSize * 0.5f + cellIndex.z * leafSize);
            return localToWorldPoint(pos);
        }

   
        public void Clear()
        {
            //var list = cells.ToArray();
            cells = new MeshChunkCell[size2 * worldSize.z];

            //world.RebuildAllLayerCells(false);

            world.Invalidate();
            //foreach (var cell in list)
            //{
            //    if (cell!=null && cell.chunk!=null)
            //        world.OnSetCellByLayer(this, cell.worldPosition,false);
            //}
        }

        public bool EmptyCell(iVector3 cell_index)
        {
            var cell = GetCell(cell_index);
            if (cell != null)
            {
                cell.isVisible = false;
                //if (modEnabled)
                //    builder.Invalidate(cell.chunk, true);
                //tree.Remove(cell);
                //if (cell.bonus)
                //    GameObject.Destroy(cell.bonus);
                return true;
            }
            else return false;
        }


        public void SetCell(int x, int y, int z, MeshChunkCell cell)
        {
            SetCell(new iVector3(x, y, z), cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell_index"></param>
        /// <param name="cell"></param>
        public void SetCell(iVector3 cell_index, MeshChunkCell cell)
        {
            cells[cell_index.x + cell_index.y * size + cell_index.z * size2] = cell;
            world.OnSetCellByLayer(this, cell_index,true);
         
        }
        public void _SetCell(iVector3 cell_index, MeshChunkCell cell)
        {
            cells[cell_index.x + cell_index.y * size + cell_index.z * size2] = cell;

        }
        public bool IsValid(iVector3 worldPos)
        {
            if (worldPos.x < 0 || worldPos.y < 0 || worldPos.z < 0) return false;
            if (worldPos.x >= worldSize.x || worldPos.y >= worldSize.y || worldPos.z >= worldSize.z) return false;
            return true;

        }

        public bool IsOnBound(iVector3 worldPos)
        {
            if (worldPos.x == 0 || worldPos.y == 0 || worldPos.z == 0
                || worldPos.x == worldSize.x-1 || worldPos.y == worldSize.y-1 || worldPos.z== worldSize.z-1) return true;
           else
                return false;
        }

        public bool IsFull(iVector3 cell_index)
        {
            var c = GetCell(cell_index);
            return c!=null && c.isVisible;
        }
        public iVector3 GetCell2DIndex(iVector2 cell_index, MeshChunkLayerAxe axe, int axeIndex)
        {
            if (axe == MeshChunkLayerAxe.X)
                return new iVector3(axeIndex, cell_index.y, cell_index.x);
            else if (axe == MeshChunkLayerAxe.Y)
                return new iVector3(cell_index.x, axeIndex, cell_index.y);
            else
                return new iVector3(cell_index.x, cell_index.y, axeIndex); ;
        }

        public MeshChunkCell GetCell2D(iVector2 cell_index, MeshChunkLayerAxe axe,int axeIndex )
        {
            return GetCell(GetCell2DIndex(cell_index,axe,axeIndex));
        }

        public MeshChunkCell GetCell(iVector3 cell_index)
        {
            return cells[cell_index.x + cell_index.y * size + cell_index.z * size2];
        }

        public MeshChunkCell GetCell(int x, int y, int z)
        {
            return cells[x + y * size + z * size2];
        }


        // ===============================================================================
   
        /// <summary>
        /// flip.xyz , 0 = no, 1 = si 
        /// </summary>
        public void CopyTo(iRectangle source, MeshChunkLayer target, iVector3 delta,iVector3 flip)
        {
         //   var diff = to - source.end;
          //  Debug.Log("delta " + delta);

            MathHelper.OrderMinMax(ref source.start, ref source.end);

            var start = source.start.Max(iVector3.zero );
            var end = source.end.Min(world.worldSize - 1);

            for (int x = start.x; x <= end.x; x++)
                for (int y = start.y; y <= end.y; y++)
                    for (int z = start.z; z <= end.z; z++)
                    {
                        var src_p = new iVector3(x, y, z);
                        var src_cell = GetCell(src_p);

                        var dst_p = src_p;
                        if (flip.x > 0) dst_p = new iVector3(start.x + (end.x- dst_p.x), dst_p.y, dst_p.z);
                        if (flip.y > 0) dst_p = new iVector3(dst_p.x, start.y + (end.y - dst_p.y),  dst_p.z);
                        if (flip.z > 0) dst_p = new iVector3(dst_p.x, dst_p.y,start.z + (end.z - dst_p.z));

                        dst_p = dst_p + delta;

                        if (src_cell != null && target.IsValid(dst_p))
                            target.SetCell(dst_p, src_cell.Clone());
                    }
        }

        public void ClearSelected(iVector3[] source)
        {
            foreach(var c in source)
                SetCell(c, null);
        }

        /// <summary>
        /// flip.xyz , 0 = no, 1 = si 
        /// </summary>
        public void CopyTo(iVector3[] source, MeshChunkLayer target, iVector3 delta, iVector3 flip)
        {
            //   var diff = to - source.end;
            //  Debug.Log("delta " + delta);
            iVector3 min = new iVector3(9999, 9999, 9999);
            iVector3 max = new iVector3(-9999, -9999, -9999);
            foreach (var p in source)
            {
                min = min.Min(p);
                max = max.Max(p);
            }

            foreach (var p in source)
            {
                var src_p = p;// new iVector3(x, y, z);
                var src_cell = GetCell(src_p);

                var dst_p = src_p;
                if (flip.x > 0) dst_p = new iVector3(min.x + (max.x - dst_p.x), dst_p.y, dst_p.z);
                if (flip.y > 0) dst_p = new iVector3(dst_p.x, min.y + (max.y - dst_p.y), dst_p.z);
                if (flip.z > 0) dst_p = new iVector3(dst_p.x, dst_p.y, min.z + (max.z - dst_p.z));

                dst_p = dst_p + delta;

                if (src_cell != null && target.IsValid(dst_p))
                    target.SetCell(dst_p, src_cell.Clone());
            }
        }


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
                            if (cell != null && ((onlyVisible && cell.isVisible) || (!onlyVisible)))
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
}
