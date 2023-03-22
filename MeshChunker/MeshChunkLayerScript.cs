using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace brickgame
{
    public enum LayerScriptParamType
	{
        Type_iVector3,
        Type_float,
        Type_int,
        Type_Material,
        Type_Axe,
        Type_Vector3,
        Type_Bool
    }

    [Serializable]
    public class LayerScriptParam
    {
        public string name;
        public LayerScriptParamType type;

        public iVector3 ivector;
        public float[] vector = new float[3];
  
        public ScriptValue<float> f_value = new ScriptValue<float>();

        [JsonIgnore]
        public Vector3 vvector
        {
            get { return new Vector3(vector[0], vector[1], vector[2]); }
            set { vector[0] = value.x; vector[1] = value.y; vector[2] = value.z; }
        }
 
        [JsonIgnore]
        public Action<LayerScriptParam> onLoad;
        [JsonIgnore]
        public Action<LayerScriptParam> onSave;

        public void reset()
        {
            f_value.reset();
        }
    }

    public class LayerScript
    {
        public ScriptValue<bool> fenabled;

        public string referenceName;

        public List<LayerScriptParam> pars = new List<LayerScriptParam>();

        [JsonIgnore]
        bool created = false;

		[JsonIgnore]
        MeshChunkScript scriptObject;

        [JsonIgnore]
        public MeshChunkScript ScriptObject {
            get 
            {
                if (!created || scriptObject.GetType().AssemblyQualifiedName != referenceName)
                    scriptObject = Create();
                return scriptObject;
              }
        }

        public LayerScript()
        {
            fenabled = new ScriptValue<bool>();
          //  fenabled.SetStartValue(true);
        }

        public void Invalidate()
        {
            fenabled.reset();
            foreach (var p in pars)
                p.onLoad(p);
            foreach (var p in pars)
                p.reset();
            ScriptObject.Invalidate();
        }
        public void Reset()
        {
            foreach (var p in pars)
                p.reset();
         //   ScriptObject.Invalidate();
        }
        public void PreUpdate(MeshChunkLayer layer)
        {
           // if (enabled.value)
            {
                ScriptObject.PreUpdate(layer, pars, fenabled);
            }
        }
        public void Update(MeshChunkLayer layer)
        {
            if (fenabled.value)
            {
                ScriptObject.Update(layer, pars);
            }
            //if (firstFrame)
            //{
            //    firstFrame = false;
            //    ScriptObject.OnCreate(layer, pars);
            //    foreach (var p in pars)
            //        p.onLoad(p);
            //}
        }
        public void OnApplicationQuit(MeshChunkLayer layer)
        {
            ScriptObject.OnDestroy(layer,pars);

         //   firstFrame = true;
            //foreach (var p in pars)
            //    p.onSave(p);
        }

        MeshChunkScript Create()
        {
            try
            {
                scriptObject = (MeshChunkScript)Type.GetType(referenceName).GetConstructor(new Type[] { }).Invoke(new object[] { });
                created = true;
                //scriptObject.fi
                return scriptObject;
            }
            catch (Exception e) {
                Debug.LogError(e);
                return new MeshChunkScript();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MeshChunkLayerScript
    {
        List<LayerScript> scriptList = new List<LayerScript>();

		public IEnumerable<LayerScript> ScriptList { get => scriptList;  }

        public bool HasScriptAnimations
		{
			get
			{
                return ScriptList.Sum(X => X.pars.Sum(Y => X.fenabled.mode != ScriptValue_AnimatorMode.Fixed ? 1 : 0 +  Y.f_value.mode !=  ScriptValue_AnimatorMode.Fixed ? 1 : 0))>0;

            }
		}

        public string GetDesc()
        {
            var list = string.Join(",", scriptList.Select(X => X.ScriptObject.GetName()));
            return list;
        }

        public void Reset()
        {
            foreach (var s in scriptList)
                s.Reset();
        }
        public void PreUpdate(MeshChunkLayer layer)
        {
            foreach (var s in scriptList)
                s.PreUpdate(layer);
        }   
        public bool Update(MeshChunkLayer layer)
        {
          
            int isChanged = scriptList.Sum(X => X.ScriptObject.isChanged ? 1 : 0);
            if (isChanged > 0)
            {
                layer.Clear();

                // no events
                //layer.world.BeginChange();

                // ridisegno tutto 
                foreach (var s in scriptList) s.ScriptObject.isChanged = true;

            }

            foreach (var s in scriptList)
                s.Update(layer);

            //if (isChanged > 0)
            //{
            //    layer.world.Invalidate();
            //    //if (isChanged > 0)
            //   // layer.world.CommitChange(true);
            //}
            return isChanged>0;
        }
        public void OnApplicationQuit(MeshChunkLayer layer)
        {
            foreach (var s in scriptList)
                s.OnApplicationQuit(layer);
        }

        public void New()
        {
            scriptList.Add(new LayerScript());
        }

        public void Delete(LayerScript s)
        {
            scriptList.Remove(s);
        }
        //public void CopyFrom(MeshChunkLayerAnimator anim)
        //{
        //    channels.Clear();
        //    foreach (var ch in anim.channels)
        //    {
        //        channels.Add(ch.GetCopy());
        //    }
        //}
    }


}
