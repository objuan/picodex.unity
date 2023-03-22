using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{

    
    public class MeshChunkRef : MonoBehaviour
    {
       
        public iVector3 position;
     //   [NonSerialized]
      //  public List<int[]> originalIndices=null;

        private void Start()
        {
           // var mesh = GetComponent<MeshFilter>().mesh;
            //originalIndices = new List<int[]>();

            //for (int i=0;i< mesh.subMeshCount;i++)
            //    originalIndices.Add(GetComponent<MeshFilter>().mesh.GetIndices(i));
        }
    }
}
