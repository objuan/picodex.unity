using brickgame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace filotto
{


    public class GOAnimator 
    {
        public class KeyPoint
        {
            public Vector3 pos = Vector3.zero;
            public Quaternion rot = Quaternion.identity;
            public Vector3 scale= Vector3.zero;
            public float time;
        }

		private List<KeyPoint> points = new List<KeyPoint>();
     

        public void AddKey(KeyPoint key)
        {
            points.Add(key);
        }
        public void AddKey(Transform trx,float timeSecs)
        {
            points.Add( new KeyPoint() { pos = trx.position, rot = trx.rotation , scale = trx.localScale, time= timeSecs });
        }

        public void AddKeyMid(Transform from,Transform to,Vector3 offset,  float timeSecs)
        {
            points.Add(new KeyPoint() { pos = offset +  Vector3.Lerp(from.position , to.position,0.5f), 
                rot = Quaternion.Slerp(from.rotation, to.rotation,0.5f),
                scale = Vector3.Lerp(from.localScale, to.localScale, 0.5f), time = timeSecs });
        }
        public void Add(BezierCurve curve,int sampleCount, float timeSecs)
        {
            for (int i = 1; i <= sampleCount; i++)
            {
                var node = curve.GetAt((float)i / sampleCount);

                points.Add(new KeyPoint() { pos = node.position, rot = node.rotation,  time = timeSecs / sampleCount });
            }
        }


        public IEnumerator Animate(Transform trx, Transform movingTarget=null)
        {
            Vector3 end_pos = points.Last().pos;
            Quaternion end_rot = points.Last().rot;

            if (movingTarget != null)
            {
                end_pos = movingTarget.transform.position;
                end_rot = movingTarget.transform.rotation;
            }

            Vector3 pos = trx.transform.position;
            Quaternion rot = trx.transform.rotation;
            Vector3 scale = trx.transform.localScale;

            int current = 0;
            float start_time = Time.time;
            float dt = 0;
            while (current < points.Count)
            {
                var key = points[current];
                dt = Time.time - start_time;
                if (dt > key.time)
                {
                    // dt -= key.time;
                    start_time = Time.time - (dt - key.time);
                     current++;
                    pos = trx.transform.position;
                    rot = trx.transform.rotation;
                    scale = trx.transform.localScale;
                }
             
                if (current < points.Count)
                {
                    float factor = (dt) / key.time;

                  //  Debug.Log("" + current + " " + dt + " " + factor);

                    Vector3 off = Vector3.zero;
					if (movingTarget != null)
					{
						off = movingTarget.transform.position - end_pos;
					}

					trx.transform.position =Vector3.Lerp(pos, key.pos+ off, factor);
                  //  trx.transform.rotation = Quaternion.Slerp(rot, key.rot, factor);

                    if (key.scale != Vector3.zero)
                        trx.transform.localScale = Vector3.Lerp(scale, key.scale, factor);
                }
                //time = Time.time;
                yield return null;
            }
        }

   

    }
}
