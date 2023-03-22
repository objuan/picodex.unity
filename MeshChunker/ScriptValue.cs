using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace brickgame
{
    public class ScriptValue_AnimKey<T>
    {
        public float time;
        public T value;

        public void SetValue(bool value)
        {
            this.value = (T)Convert.ChangeType(value, typeof(T));
        }
        public void SetValue(float value)
        {
            this.value = (T)Convert.ChangeType(value, typeof(T));
        }

        public bool toBool()
        {
            return (bool)Convert.ChangeType(value, typeof(bool));
        }

        public float toFloat()
        {
            return (float)Convert.ChangeType(value, typeof(float));
        }
        public ScriptValue_AnimKey<T> Clone()
        {
            return new ScriptValue_AnimKey<T>() { time = time, value = value };
        }
    }
    public enum ScriptValue_AnimatorMode
    {
        Fixed,
        PingPong,
        Repeat
    }
    public enum ScriptValue_SnapMode
    {
        None,
        IntegerUp,
        IntegerDown,
        Half
    }

    public interface IScriptValue
    {
    }

    public class ScriptValue<T> : IScriptValue
    {
        public bool animEnabled = true;
        public ScriptValue_AnimatorMode mode = ScriptValue_AnimatorMode.Fixed;
        public ScriptValue_SnapMode snap = ScriptValue_SnapMode.None;
        public List<ScriptValue_AnimKey<T>> keyList = new List<brickgame.ScriptValue_AnimKey<T>>();

        public bool initialized = false;
        public T start_value;
        public T value;

		// runtime;
		int currentKey = 0;
        int animDirection = 1;
        float keyStartTime;
        float keyTotalTime;
        float f_keyStartValue;

        public ScriptValue()
        {
            if (typeof(T) == typeof(bool))
                SetValue(true);

        }

        public void CopyFrom(ScriptValue<T> val)
        {
            initialized = false;
            start_value = val.start_value;
            value = val.value;
            mode = val.mode;
            animEnabled = val.animEnabled;
            snap = val.snap;
            keyList.Clear();
            keyList.AddRange(val.keyList.Select ( X => X.Clone()));

        }
        //public ScriptValue(T start)
        //{
        //    SetStartValue(start);
        //}
        public void reset()
        {
            if (animEnabled && mode != ScriptValue_AnimatorMode.Fixed)
            {
                this.value = start_value;
                keyStartTime = Time.time;
                keyTotalTime = Time.time;
                f_keyStartValue = (float)Convert.ChangeType(start_value, typeof(float));
            }
        }

        //public ScriptValue(int v)
        //{
        //    value = (T)Convert.ChangeType(v, typeof(T));
        //    start_value = value;
        //}
        //public ScriptValue(float v)
        //{
        //    value = (T)Convert.ChangeType(v, typeof(T));
        //    start_value = value;
        //}

        public bool toBool()
		{
            return (bool)Convert.ChangeType(value, typeof(bool));
        }

        public float toFloat()
        {
            return (float)Convert.ChangeType(value, typeof(float));
        }

        public void SetStartValue(T value)
        {
            if (!initialized)
            {
                initialized = true;
                start_value = value;
            }
            this.value = start_value;
            keyStartTime = Time.time;
            // if (typeof(T) == typeof(float))
            keyTotalTime = Time.time;
            f_keyStartValue = (float)Convert.ChangeType(start_value, typeof(float));
            //if (typeof(T) == typeof(bool))
            //    snap = ScriptValue_SnapMode.IntegerUp;
        }
        //public void SetValue(T value)
        //{
        //    this.value = value;
        //}
        public void SetValue(bool value)
        {
            this.value = (T)Convert.ChangeType(value, typeof(T));
        }
        public void SetValue(float value)
        {
            this.value = (T)Convert.ChangeType(value, typeof(T));
        }
        T Clamp(T t)
        {
            float val =  (float)Convert.ChangeType(t, typeof(float));

         //   Debug.Log(t);
            if (snap == ScriptValue_SnapMode.IntegerDown)
                return (T)Convert.ChangeType((int)val, typeof(T));
            else if (snap == ScriptValue_SnapMode.IntegerUp)
                return (T)Convert.ChangeType((int)(val + 0.55f), typeof(T));
            else if(snap == ScriptValue_SnapMode.Half)
			{
                var f = (float)(Math.Round(val * 2, MidpointRounding.AwayFromZero) / 2);
                return (T)Convert.ChangeType(val, typeof(T));
            }
            else 
                return t;
        }

        public bool PreUpdate()
        {
            if (animEnabled && mode != ScriptValue_AnimatorMode.Fixed && keyList.Count>=2)
            {
                if (currentKey+ animDirection >= keyList.Count || currentKey + animDirection<0) 
                    currentKey = 0;

                var oldValue = this.value;

                // var current = value;
                var targetKey = keyList[currentKey+ animDirection]; 
                float dt = Time.time - keyStartTime;
                float targetTime = (animDirection > 0) ? keyList[currentKey + animDirection].time - keyList[currentKey ].time 
                    : keyList[currentKey].time - keyList[currentKey + animDirection].time;
                if (dt < targetTime)
                {
                    if (value is float)
                    {
                        if (typeof(T) != typeof(bool))
                        {
                            var target = targetKey.toFloat();
                            this.value = Clamp((T)Convert.ChangeType(Mathf.Lerp(f_keyStartValue, target, dt / targetTime), typeof(T)));

                          //  Debug.Log("anim f:" + currentKey + " dt:" + dt + " =" + this.value + " .. " + f_keyStartValue + " - " + target +" gt:"+ (Time.time- keyTotalTime));
                        }
                    }
                    else
                    {
                        var target = targetKey.toBool();
                      //  Debug.Log("anim b:" + currentKey + " dt:" + dt + " =" + this.value + " .. " + f_keyStartValue + " - " + target + " gt:" + (Time.time - keyTotalTime));
                    }
                }
                else
                {
                    currentKey += animDirection;

                    if (currentKey == keyList.Count - 1)
                    {
                        if (mode == ScriptValue_AnimatorMode.Repeat)
                        {
                            currentKey = 0;
                            this.value = keyList[0].value;
                            keyTotalTime = Time.time;
                        }
                        if (mode == ScriptValue_AnimatorMode.PingPong)
                        {
                            animDirection = -1;
                            currentKey = keyList.Count - 1;
                            this.value = keyList[keyList.Count() - 1].value;
                            //currentKey = currentKey - 2;
                        }
                    }
                    else if (currentKey == 0)
                    {
                        if (mode == ScriptValue_AnimatorMode.PingPong)
                        {
                            animDirection = 1;
                            currentKey = 0;
                            this.value = keyList[0].value;
                            keyTotalTime = Time.time;
                        }
                    }
                    else
                    {
                        if (typeof(T) == typeof(bool))
                        {
                            this.value = keyList[currentKey+ animDirection].value;
                        }
                    }
                    
                    keyStartTime = Time.time;
                    f_keyStartValue = (float)Convert.ChangeType(value, typeof(float));
                }
                bool ch = false;
                if (value is float)
                    ch = (float)Convert.ChangeType(value, typeof(float)) == toFloat();
                if (value is bool)
                    ch = (bool)Convert.ChangeType(value, typeof(bool)) == toBool();
               // Debug.Log("ch "+ch+" "+ oldValue + "-> " + this.value);
                return ch;
            }
            else
                return false;
        }

        //public static implicit operator ScriptValue<T>(int d)
        //{
        //    return new ScriptValue<T>(d);
        //}
        //public static implicit operator ScriptValue<T>(float f)
        //{
        //    return new ScriptValue<T>(f);
        //}

        public static implicit operator int(ScriptValue<T> d)
		{
			return Convert.ToInt32(d.value);
		}
		public static implicit operator float(ScriptValue<T> f)
		{
			return Convert.ToSingle(f.value);
		}
		//	public static ScriptValue<T> operator =(ScriptValue<T> a) => a;

	}



 
}
