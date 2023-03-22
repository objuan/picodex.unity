using System;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;


public class TimingDelete : MonoBehaviour
{
    public float time = 1;

    public Func<IEnumerator> OnEnd;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        if (OnEnd != null)
            yield return OnEnd();

        Destroy(gameObject);
    }
}
 
