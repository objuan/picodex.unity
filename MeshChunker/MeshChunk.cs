using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{

    [Serializable]
    public class MeshChunk : IMeshChunk
    {
        [NonSerialized]
        public MeshWorld world;

        public int ID;
        public iVector3 chunkIndex;
        public iVector3 startWorldPosition { get; set; }
        MeshChunkCell[] cells;
        int size1;
        int size2;
        public iVector3 size { get; set; }

        public MeshChunkCell[] Cells { get => cells; }

        //  BoundsOctree<MeshChunkCell> tree;

        public MeshChunk(int ID,MeshWorld world, iVector3 chunkIndex, iVector3 size)
        {
            this.world = world;
            this.ID = ID;
            startWorldPosition = new iVector3(chunkIndex.x * size.x, chunkIndex.y * size.y, chunkIndex.z * size.z);
            this.chunkIndex = chunkIndex;
            this.size = size;
            cells = new MeshChunkCell[size.x* size.y* size.z];
            size1 = size.x;
            size2 = size.x*size.y;

         
        }
        public MeshChunk(MeshWorld world, iVector3 chunkIndex, MeshChunkCell[] cellList)
        {
            this.world = world;
            iVector3 min = new iVector3(0,0,0);
            if (cellList.Length > 0)
            {
                min = new iVector3(9999, 9999, 9999);
                iVector3 max = new iVector3(-9999, -9999, -9999);
                foreach (var p in cellList)
                {
                    min = min.Min(p.worldPosition);
                    max = max.Max(p.worldPosition);
                }
                size = max - min + 1;
            }
            else
                size = new iVector3(0, 0, 0);

            startWorldPosition = new iVector3(chunkIndex.x * size.x, chunkIndex.y * size.y, chunkIndex.z * size.z);
            this.chunkIndex = chunkIndex;
            // this.size = size;
            cells = new MeshChunkCell[size.x * size.y * size.z];
            size1 = size.x;
            size2 = size.x * size.y;

            foreach (var p in cellList)
            {
                Set(p.worldPosition - min, p);
            }
            
        }
        public IMeshChunkCell GetCellMerged(int x, int y, int z)
        {
            return world.GetCellMerged(x, y, z);
        }


        public void Clear()
        {
            cells = new MeshChunkCell[size.x * size.y * size.z];
            
        }

        public MeshChunkCell Get(iVector3 index)
        {
         //   Debug.Log(index);
            var idx = index.x + index.y * size1+ index.z * size2;
            return cells[idx];

        }
        public MeshChunkCell Get(int x,int y,int z)
        {
            if (x >= 0 && y >= 0 && z >= 0
                && x < size.x && y < size.y && z < size.z)
            {
                var idx = x + y * size1 + z * size2;
                return cells[idx];
            }
            else
                return null;
        }

        /// <summary>
        /// return true is is changed 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool Set(iVector3 index, MeshChunkCell cell)
        {
            bool isChanged = false;
            var idx = index.x + index.y * size1 + index.z * size2;

            isChanged = cells[idx] != cell;
            //if ((cell == null && cells[idx] != null) || (cell != null && cells[idx] == null))
            //    isChanged = true;
            //else
            //{
            //    // e' cambiata la cella ?? 
            //    if (cell != null)
            //    {
            //        isChanged = (cell != cells[idx]);
            //        {
            //            isChanged = (!cell.Equals(cells[idx]));
            //        }

            //    }
            //}

            cells[idx] = cell;

            if (cell != null)
            {
                cell.Initialize( this , index);
            //    cell.chunkInternalPosition = index;
            }
            return isChanged;
        }

        public void ComputeCollisions()
        {
            
        }


    }
}
