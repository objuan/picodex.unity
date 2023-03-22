
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace filotto
{


    public class AnimatorSpaceSym
    {
        public float currentValue;
        public float timeFactor;


        float targetValue;
        float lastValue;
     
        float sym_blend = 0;
        float start_time;
        float deltaTime;
        float startValue;
        AnimationCurve distanceToTimeCurve;
        AnimationCurve factorToSymCurve;
        float max_step = -1;
        public AnimatorSpaceSym(AnimationCurve distanceToTimeCurve, AnimationCurve factorToSymCurve,float startValue)
        {
            this.distanceToTimeCurve = distanceToTimeCurve;
            this.factorToSymCurve = factorToSymCurve;
            lastValue = targetValue = currentValue = startValue;
            timeFactor = 1;
        }

        public bool Update(float target)
        {
			float d = Mathf.Abs(target- currentValue);

			if (Mathf.Abs(lastValue- target) > 0.01f)
			{
				// var sym_cam_sistance = Mathf.Abs(target_camera_z - current_camera_z);
				lastValue = target;
				timeFactor = 0.5f;
			}

			if (d > 0.001f)
			{
				float speed = 2f;
				float space_todo = Math.Min(d, speed * Time.deltaTime);

				currentValue = currentValue + (target - currentValue) * space_todo;

				timeFactor += speed * 0.5f * Time.deltaTime;
				timeFactor = Mathf.Min(timeFactor, 1);
				return true;
			}
			else
			{
				timeFactor = 1;
				currentValue = target;
				return true;
			}

			/*
			this.targetValue = target;
			if (Mathf.Abs(lastValue - targetValue) > 0.01f)
			{
				// var sym_cam_sistance = Mathf.Abs(target_camera_z - current_camera_z);
				deltaTime = distanceToTimeCurve.Evaluate(Mathf.Abs(targetValue - currentValue));// symToSlow_Time;
				sym_blend = 1;
				start_time = Time.time;
				startValue = currentValue;
				//  Debug.Log("SEfloat DISfloat  start:" + current_camera_z + " from: " + last_target_camera_z + "->" + target_camera_z + " delta time" + deltaTime);
				lastValue = targetValue;
			}

			if (sym_blend > 0)
			{
				// blending, sia di spazio che ti tempo
				var f = Mathf.Clamp((Time.time - start_time) / deltaTime, 0, 1); // 0-1
				sym_blend = 1f - f;
				try
				{
					timeFactor = 1f;// factorToSymCurve.Evaluate(f);
					currentValue = Mathf.Lerp(startValue, targetValue, f);

				}
				catch (Exception e)
				{
					currentValue = targetValue;
					sym_blend = 0;
				}

				// Debug.Log("SEfloat BLEND  =" + timeFactor + " cam:" + current_camera_z);

				//GO.Instance<FGameSession>().symulationFactor = timeFactor;
				//camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, -current_camera_z + playerPlane.Value.distance);

				//ballSpeedBonusFactor = factor;
				//yield return null;
				return true;
			}
			else
			{
				currentValue = targetValue;
				return false;
			}
			*/
		}
    }

    public class AnimatorSpaceSymVector
    {
        public Vector3 currentValue;
        public float timeFactor;

        Vector3 targetValue;
        Vector3 lastValue;
        bool sym_blend = false;
        float start_time;
        float deltaTime;
        Vector3 startValue;
        float max_step;
        AnimationCurve distanceToTimeCurve;
        AnimationCurve factorToSymCurve;
        AnimationCurve factorToDistanceCurve;
        public AnimatorSpaceSymVector(AnimationCurve distanceToTimeCurve, AnimationCurve factorToSymCurve, AnimationCurve factorToDistanceCurve, Vector3 startValue)
        {
            this.distanceToTimeCurve = distanceToTimeCurve;
            this.factorToSymCurve = factorToSymCurve;
            this.factorToDistanceCurve = factorToDistanceCurve;
            lastValue = targetValue = currentValue = startValue;
        }

        public bool Update(Vector3 target)
        {
			float d = Vector3.Distance(target, currentValue);

			if (Vector3.Distance(lastValue, target) > 0.01f)
			{
				// var sym_cam_sistance = Mathf.Abs(target_camera_z - current_camera_z);
				lastValue = target;
				timeFactor = 0.5f;
			}

			if (d > 0.001f)
			{
				float speed = 2f;
				float space_todo = Math.Min(d, speed * Time.deltaTime);

				currentValue = currentValue + (target - currentValue).normalized * space_todo;

				timeFactor += speed * 0.5f*  Time.deltaTime;
				timeFactor = Mathf.Min(timeFactor,1);

				//deltaTime = Mathf.Min(1, d);// distanceToTimeCurve.Evaluate(d);
				//float f = Mathf.Min(deltaTime, Mathf.Min(0.02f, Time.deltaTime)) / deltaTime;

				//var newValue = Vector3.Lerp(currentValue, target, f);// factorToDistanceCurve.Evaluate(f));
				//float dist = Vector3.Distance(newValue, currentValue);
				//max_step = Mathf.Max(dist, max_step);

				//currentValue = newValue;
				//timeFactor = 1;// 0.5f + ( 1  - f) / 2;
				//float f1 = dist / max_step;
				//timeFactor = factorToSymCurve.Evaluate(f1);// * 0.5f;// factorToSymCurve.Evaluate( f / max_f);// 0.5f + ( 1  - f) / 2;

				//Debug.Log("blend " + f1 + "d:" + d + " f:" + f + " dt:" + Time.deltaTime);

				return true;
			}
			else
			{
				timeFactor = 1;
				   currentValue = target;
				return true;
			}
			//symToSlow_Time;

			/*
			this.targetValue = target;
			if (Vector3.Distance(lastValue, targetValue) > 0.01f)
			{
				// var sym_cam_sistance = Mathf.Abs(target_camera_z - current_camera_z);
				deltaTime = distanceToTimeCurve.Evaluate(Vector3.Distance(targetValue, currentValue));// symToSlow_Time;
				sym_blend = true;
				start_time = Time.time;
				startValue = currentValue;
				//  Debug.Log("SEfloat DISfloat  start:" + current_camera_z + " from: " + last_target_camera_z + "->" + target_camera_z + " delta time" + deltaTime);
				lastValue = targetValue;
			}

			if (sym_blend)
			{
				// blending, sia di spazio che ti tempo
				var f = Mathf.Max (0.1f, (Time.time - start_time) / deltaTime); // 0-1
				sym_blend = f <= 1;
				f = Mathf.Min(1, f);

				timeFactor = factorToSymCurve.Evaluate(f);

			//	float ev =  f * timeFactor;
				float ev = factorToDistanceCurve.Evaluate(f) ;
				var old = currentValue;
				currentValue = Vector3.Lerp(startValue, targetValue, ev);

				float speed = (currentValue - old).magnitude;

				//Debug.Log("blend " + f + "d:" + ev+" tf:"+ timeFactor+" sp:"+ speed);

				return true;
			}
			else
			{
				currentValue = targetValue;
				return false;
			}

			*/
		}
    }
}
