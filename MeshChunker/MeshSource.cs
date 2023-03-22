using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{
    //public enum FaceCode
    //{
    //    Undefined=0,
    //    X_P,X_N,Y_P,Y_N,Z_P,Z_N
    //}
    #region triangle

    [Serializable]
    public class Triangle
    {
        public int index;
        public int v1;
        public int v2;
        public int v3;
      //  public List<Triangle>[] near = new List<Triangle>[3];

        public int this[int index]
        {
            get
            {
                return index==0 ? v1 : ((index==1) ? v2: v3);
            }
            set
            {
                if (index == 0) v1 = value;
                else if (index == 1) v2 = value;
                else  v3 = value;
            }
        }
        public Vector3 N;
       
        public iVector3 nextCell ;

        [NonSerialized]
        public bool merged;

        [NonSerialized]
        public bool discarded;

        public float Area(Vector3[] v)
        {
            var a = (v[v1] - v[v2]).magnitude;
            var b = (v[v2] - v[v3]).magnitude;
            var c = (v[v3] - v[v1]).magnitude;
            var p = (a + b + c) / 2;
            return Mathf.Sqrt( p * (p-a) * (p-b) * (p-c));
        }

        public int OtherIndex(int v1, int v2)
        {
            var b = new bool[3].Select(X => false).ToList();
            if (this.v1 == v1 || this.v1 == v2) b[0] = true;
            if (this.v2 == v1 || this.v2 == v2) b[1] = true;
            if (this.v3 == v1 || this.v3 == v2) b[2] = true;
            return b.IndexOf(false);
        }

        public bool Merge(Vector3[] v,Triangle tri_other, int v1, int v2)
        {
            if (merged || tri_other.merged) return false;

            float origArea_base = Area(v);
            float origArea = Area(v) + tri_other.Area(v);

            int newVertex = tri_other[tri_other.OtherIndex(v1, v2)];

            int otherIdx = OtherIndex(v1, v2);

         //   Debug.Log("Area  " + origArea+" new:" + newVertex+" other "+ otherIdx);

            // cambio successivo successivo
            int changeIdx = otherIdx + 1;if (changeIdx == 3) changeIdx = 0;
            var oldIdx = this[changeIdx];
            this[changeIdx] = newVertex;
            float area1 = Area(v);

            //  Debug.Log("Merge1 " + area1+" " +this.ToString());

            if (Mathf.Abs(area1 - origArea) < 0.0000001)
            {
             
                // check equilateo 
                //float d1 = (v[this.v1] - v[this.v2]).magnitude;
                //float d2 = (v[this.v2] - v[this.v3]).magnitude;
                //float d3 = (v[this.v3] - v[this.v1]).magnitude;
                //if (!(Mathf.Abs(d1 - d2) < 0.000001 && Mathf.Abs(d2 - d3) < 0.000001))
                {
                    Debug.Log("Merge1 " + area1 + " " + this.ToString());
                    merged = true;
                    tri_other.merged = true;
                    tri_other.discarded = true;
                    return true;
                }
            }
            this[changeIdx] = oldIdx;

            changeIdx = otherIdx - 1; if (changeIdx <0) changeIdx = 2;
            oldIdx = this[changeIdx];
            this[changeIdx] = newVertex;
            float area2 = Area(v);
           
            //int chagedIdx = keepIdx + 1; if (chagedIdx == 3) chagedIdx = 0;
            //int newIdx = tri.Other(v1,v2);

        //    Debug.Log("Merge2 " + area1 + " " + this.ToString());

            if (Mathf.Abs(area2 - origArea) < 0.0000001)
            {
                //  Debug.Log("FIND  !!!!!!!!!!!!!!!!!! ");
                //float d1 = (v[this.v1] - v[this.v2]).magnitude;
                //float d2 = (v[this.v1] - v[this.v3]).magnitude;
                //float d3 = (v[this.v3] - v[this.v1]).magnitude;
                //if (!(Mathf.Abs(d1 - d2) < 0.000001 && Mathf.Abs(d2 - d3) < 0.000001))
                {
                    Debug.Log("Merge2 " + area1 + " " + this.ToString());
                    merged = true;
                    tri_other.merged = true;
                    tri_other.discarded = true;
                    return true;
                }
            }

            this[changeIdx] = oldIdx;
            return false;
        }

        public override string ToString()
        {
            return "tri " + index+" ("+v1 + " " + v2 + " " + v3+") "+N;
        }
    }
    #endregion

    #region mesh_source

    [Serializable]
    public class MeshSource
    {
        public Vector3[] v;
        public Vector2[] uvs;
        public Vector3[] n;

        //    public Triangle[] tris_border;
        public Triangle[] tris_faces;

        Dictionary<int, List<Triangle>> lati;

        int foundCount;

        public int GetTriCount()
        {
            return tris_faces.Length;
         //   return (layer == 0) ? tris_border.Length : tris_faces.Length;
            // return new int[] { tris[idx * 3], tris[idx * 3+1], tris[idx * 3+2] };
        }

        public Triangle GetTri(int idx)
        {
            return tris_faces[idx];
           // return (layer==0) ? tris_border[idx] : tris_faces[idx];
            // return new int[] { tris[idx * 3], tris[idx * 3+1], tris[idx * 3+2] };
        }

        public void Optimize()
        {
            // tolgo quadtrati 

            //Optimize_findinside(tris0);
            //// tris0 = tris0.Where(X => !X.discarded).ToArray();

            // Optimize(tris0);
            //tris0 = tris0.Where(X => !X.discarded).ToArray();

            //Optimize(tris1);
            //tris1 = tris1.Where(X => !X.discarded).ToArray();
            //Optimize(tris1);
        }

        bool Optimize_findinside(Triangle[] tris)
        {
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i].merged = false;
                tris[i].discarded = false;
            }
            // trova lati in comune
            lati = new Dictionary<int, List<Triangle>>();
            foreach (var tri in tris)
            {
                Add(tri, tri.v1, tri.v2);
                Add(tri, tri.v2, tri.v3);
                Add(tri, tri.v3, tri.v1);
            }
            // -------------
            foreach (var key in lati.Keys)
            {
                List<Triangle> nearTris = lati[key];
                if (nearTris.Count == 2)
                {
                    int v1 = key & ((1 << 16) - 1);
                    int v2 = (key >> 16) & ((1 << 16) - 1);

                    if (nearTris[0].N == nearTris[1].N) // stessa normale
                    {
                        // cerco un altro che ha gli altri lati condivisi
                       
                       // Triangle newTri = nearTris[0];
                    }
                }
            }
            return false;
        }

        bool Optimize(Triangle[] tris)
        {
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i].merged = false;
                tris[i].discarded = false;
            }
            bool find = false;

            foundCount = 0;
             // trova lati in comune
            lati = new Dictionary<int, List<Triangle>>();
            foreach (var tri in tris)
            {
                Add(tri, tri.v1, tri.v2);
                Add(tri, tri.v2, tri.v3);
                Add(tri, tri.v3, tri.v1);
            }
            foreach (var key in lati.Keys)
            {
                List<Triangle> nearTris = lati[key];
                if (nearTris.Count == 2)
                {
                    int v1 = key & ((1 << 16) - 1);
                    int v2 = (key >> 16) & ((1 << 16) - 1);

                    if (nearTris[0].N == nearTris[1].N) // stessa normale
                    {
                   //     Debug.Log("Co-planar " + v1 + "-" + v2 + " => " + string.Join(",", nearTris.Select(X => "" + X.index)));
                      //  Debug.Log(nearTris[0]);
                   //     Debug.Log(nearTris[1]);

                        Triangle newTri = nearTris[0];
                        if (newTri.Merge(v, nearTris[1], v1, v2))
                        {
                            //Debug.Log("merged  " + newTri+"-"+ nearTris[1]);
                            find = true;
                        }

                      //  Debug.Log("res  " + newTri);
                    }

                }
            }

            Debug.Log("======================");
            Debug.Log(" found = "+ foundCount);
            return false;
        }

        void Add(Triangle tri,int v1,int v2)
        {
            //if (v1 == 0 || v2 == 0)
            //    Debug.Log("Zero "+ v1+" " +v2);

            if (v2 < v1) // dal piu' piccolo al iu' grande
            {
                var aus = v2; v2 = v1;v1 = aus;
            }
            int key = v1 + (v2 << 16);
            if (!lati.ContainsKey(key)) lati.Add(key, new List<Triangle>());
            lati[key].Add(tri);

        }
    }
    #endregion


}
