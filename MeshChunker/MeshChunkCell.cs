using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{
    public enum MeshChunkCellType
    {
        Empty,
        Filled,
       // Bonus
    }
    public struct CellColorAttribute
    {
        public float cut;
        public float luminosity;
    }
    public class MeshChunkCellRef 
    {
        public MeshWorld world;
        public iVector3 worldPosition;

        public MeshChunkCell Value => world.GetCellMerged(worldPosition);
        public Vector3 position => Value.position;

        public MeshChunkCellRef(MeshChunkCell cell)
		{
            world = cell.world;
            worldPosition = cell.worldPosition;
        }
    }
    public class MeshChunkCell : IMeshChunkCell
    {
        //[JsonIgnore]
        //public MeshChunkViewType viewType = MeshChunkViewType.Normal;

        [JsonIgnore]
        /// <summary>
        /// in brick coordinate
        /// </summary>
        public Bounds? bounds;

        // runtime

        public int meshMaterialIdx;

       // public int startVertexIndex;

        /// <summary>
        /// for break events
        /// </summary>
        [JsonIgnore]
        public bool isVisible { get; set; } = true;

        public virtual bool isExisting { get; } = true;

        [JsonIgnore]
        public int startVertexIndex { get; set; }
        // ==============

        [JsonIgnore]
        public MeshWorld world;// => chunk.world;

        [JsonIgnore]
        public  MeshChunk chunk;

        [JsonIgnore]
        public iVector3 chunkInternalPosition;

        public iVector3 worldPosition;//=> iVector3.Add(chunk.startWorldPosition,chunkInternalPosition);

        // in game world coordinate 
        public Vector3 position => chunk.world.localToWorldPoint(bounds.Value.center);
     
        //public virtual string materialKey() { return ""; }

        public virtual MeshChunkMaterial cellMaterial() { return null; }

      //  public virtual MeshChunkCellType cellType() { return MeshChunkCellType.Empty; }

        public virtual CellColor cellColor() { return null; }

        public virtual CellColorAttribute cellAttribute() { return default(CellColorAttribute); }

        public virtual bool isTrasparent() { return false; }

        // utility
        public virtual void Initialize(MeshChunk chunk,iVector3 chunkInternalPosition)
		{
            this.chunk = chunk; ;
            this.chunkInternalPosition = chunkInternalPosition;
        }
        public bool IsInternal()
        {
            var nearList = chunk.world.GetNearest(this,true);
            return nearList.Length == 6;
        }

        public virtual MeshChunkCell Clone()
        {
            return null;
        }
     
    }
}
