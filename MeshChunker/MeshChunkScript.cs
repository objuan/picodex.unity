using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace brickgame
{

    public enum ScriptParamType
	{
        Position
	}

    public class MeshChunkScript // : MonoBehaviour
    {
        public virtual string GetName() { return "Script"; }
        public virtual string GetDescription() { return "Script"; }

        public bool created = false;
        public bool isChanged = true;

        LayerScriptParam GetParam(List<LayerScriptParam> pars,string name,Action<LayerScriptParam> onLoad, Action<LayerScriptParam> onSave)
        {
            var f = pars.FirstOrDefault(X => X.name == name);
            if (f==null)
			{
                var p  = new LayerScriptParam() { name = name };
                pars.Add(p);
                f = pars.FirstOrDefault(X => X.name == name);
            }
            f.onLoad = onLoad;
            f.onSave = onSave;

            return f;
        }

        public void AddParam(string name, iVector3 v, List<LayerScriptParam> pars)
		{
            var prop = GetType().GetProperty(name);
            var field = GetType().GetField(name);
            if (field == null && prop == null) Debug.LogError("property not found: " + name);
            var par = GetParam(pars, name,(load) =>
            {
                if (prop != null) prop.SetValue(this, load.ivector);
                if (field != null) field.SetValue(this, load.ivector);
            }
            ,(save) =>
            {
                if (prop != null) save.ivector = (iVector3)prop.GetValue(this);
                if (field != null) save.ivector = (iVector3)field.GetValue(this);
            });
            
            par.type = LayerScriptParamType.Type_iVector3;
            par.onLoad(par);
        }
        public void AddParam(string name, Vector3 v, List<LayerScriptParam> pars)
        {
            var prop = GetType().GetProperty(name);
            var field = GetType().GetField(name);
            if (field == null && prop == null) Debug.LogError("property not found: " + name);
            var par = GetParam(pars, name, (load) =>
            {
                if (prop != null) prop.SetValue(this, load.vvector);
                if (field != null) field.SetValue(this, load.vvector);
            }
            , (save) =>
            {
                if (prop != null) save.vvector = (Vector3)prop.GetValue(this);
                if (field != null) save.vvector = (Vector3)field.GetValue(this);
            });

            par.type = LayerScriptParamType.Type_Vector3;
            par.onLoad(par);
        }
        public void AddParam(string name,  float v, List<LayerScriptParam> pars)
        {
            var prop = GetType().GetProperty(name);
            var field = GetType().GetField(name);
            if (field == null && prop == null) Debug.LogError("property not found: " + name);
            var par = GetParam(pars, name, (load) =>
            {
                if (prop != null) prop.SetValue(this, load.f_value.value);
                if (field != null) field.SetValue(this, load.f_value.value);
            }
            , (save) =>
            {
                if (prop != null) save.f_value.SetStartValue((float)prop.GetValue(this));
                if (field != null) save.f_value.SetStartValue((float)field.GetValue(this));
            });
            par.type = LayerScriptParamType.Type_float;
            par.onLoad(par);
        }
        public void AddParam(string name, int v, List<LayerScriptParam> pars, LayerScriptParamType type = LayerScriptParamType.Type_int)
        {
            var prop = GetType().GetProperty(name);
            var field = GetType().GetField(name);
            if (field == null && prop == null) Debug.LogError("property not found: "+ name);
            var par = GetParam(pars, name, (load) =>
            {
                if (prop != null) prop.SetValue(this, (int)load.f_value.value);
                if (field != null) field.SetValue(this, (int)load.f_value.value);
            }
            , (save) =>
            {
                if (prop != null) save.f_value.SetStartValue((int)prop.GetValue(this));
                if (field != null) save.f_value.SetStartValue((int)field.GetValue(this));
            });
            par.type = type;
            par.onLoad(par);
        }
        public void AddParam(string name, bool v, List<LayerScriptParam> pars)
        {
            var prop = GetType().GetProperty(name);
            var field = GetType().GetField(name);
            if (field == null && prop == null) Debug.LogError("property not found: " + name);
            var par = GetParam(pars, name, (load) =>
            {
                if (prop != null) prop.SetValue(this, (int)load.f_value.value == 1);
                if (field != null) field.SetValue(this, (int)load.f_value.value == 1);
            }
            , (save) =>
            {
                if (prop != null) save.f_value.SetStartValue((bool)prop.GetValue(this) ? 1 : 0);
                if (field != null) save.f_value.SetStartValue((bool)field.GetValue(this) ? 1 : 0);
            });
            par.type =   LayerScriptParamType.Type_Bool;
            par.onLoad(par);
        }

        public void Invalidate()
        {
            isChanged = true;
        }

        public void LoadPars(List<LayerScriptParam> pars)
        {
            foreach (var p in pars)
                p.onLoad(p);
            Invalidate();
        }
        
        public virtual void OnCreate(MeshChunkLayer layer, List<LayerScriptParam> pars) {  }

        public virtual void OnDestroy(MeshChunkLayer layer, List<LayerScriptParam> pars) {  }

        public virtual void OnRender(MeshChunkLayer layer) { }

        public virtual void OnUpdate(MeshChunkLayer layer) { }

        public void PreUpdate(MeshChunkLayer layer, List<LayerScriptParam> pars, ScriptValue<bool> enabled)
        {
            if (!created)
            {
                // OnDestroy(layer, pars);

                OnCreate(layer, pars);
                created = true;

                // tolgo quelli non usati
                foreach (var toRemove in pars.Where(X => X.onLoad == null).ToList())
                {
                    pars.Remove(toRemove);
                }
            }

            if ( !layer.play)
                return;

			bool changed = false;

            changed |= enabled.PreUpdate();

            foreach (var p in pars)
			{
                if (p.type != LayerScriptParamType.Type_iVector3 && p.type != LayerScriptParamType.Type_Vector3)
                    changed|= p.f_value.PreUpdate();
            }
            if (changed)
            {
                isChanged = true;

                LoadPars(pars);
            }
		}

        /// <summary>
        /// return true if changed
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public void Update(MeshChunkLayer layer, List<LayerScriptParam> pars)
        {
          

            OnUpdate(layer);
            if (isChanged)
			{
            
                OnRender(layer);

                isChanged = false;
               // created = true;
            }
            
         }

    }

}
