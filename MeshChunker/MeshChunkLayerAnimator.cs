using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{
    public enum MeshChunkLayerAnimatorMode
    {
        None,
        PingPong
    }
    public enum MeshChunkLayerAnimatorAxe
    {
        X,  Y,  Z
    }
    public enum MeshChunkLayerAnimatorChannelType
    {
        Position,
        Rotation,
    }

    public class MeshChunkLayerAnimatorChannel
    {
        public MeshChunkLayerAnimatorChannelType type;
        public MeshChunkLayerAnimatorMode mode = MeshChunkLayerAnimatorMode.None;
        public float speed;
        public float from;
        public float to;
        public MeshChunkLayerAnimatorAxe axe;

        public MeshChunkLayerAnimatorChannel GetCopy()
        {
            return new MeshChunkLayerAnimatorChannel()
            {
                axe = axe,
                from = from,
                mode = mode,
                speed = speed,
                to = to,
                type = type
            };
        }
    }

    public class MeshChunkLayerAnimator
    {

        public ScriptValue<bool> enabled;

        public List<MeshChunkLayerAnimatorChannel> channels = new List<MeshChunkLayerAnimatorChannel>();

        public bool HasScriptAnimations
        {
            get
            {
                return channels.Count > 0 || enabled.mode != ScriptValue_AnimatorMode.Fixed;
            }
        }

        public MeshChunkLayerAnimator()
        {
            enabled = new ScriptValue<bool>();
        }

        public string GetDesc()
        {
            var list =  string.Join(",", channels.Select(X => X.type));
            return enabled.mode != ScriptValue_AnimatorMode.Fixed ? "(EN) ": "" + list;
        }

        public void CopyFrom(MeshChunkLayerAnimator anim)
        {
            enabled.CopyFrom(anim.enabled);
            channels.Clear();
            foreach (var ch in anim.channels)
            {
                channels.Add(ch.GetCopy());
            }
        }

        public void PreUpdate(MeshChunkLayer layer)
        {
            bool isChanged = false;
            isChanged |= enabled.PreUpdate();
            if (isChanged)
                layer.world.Invalidate();
        }

        public bool Update(MeshChunkLayer layer)
        {
            bool isChanged = false;
            //isChanged|= enabled.PreUpdate();
            //if (isChanged)
            //    layer.world.Invalidate();

            //if (isChanged)
            //    layer.world.RebuildAllLayerCells(true);

            // layer.visible = enabled.value;
            //if (isChanged)
            //{
            //    layer.world.BeginChange();

            //    layer.world.RebuildAllLayerCells(true);
            //}

           return isChanged ;
        }
    }

   
}
