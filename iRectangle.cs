
using Newtonsoft.Json;
using System;
using UnityEngine;

[System.Serializable]
public struct iRectangle
{
    public iVector3 start;
    public iVector3 end;

    public iVector3 size => end - start;

    public iRectangle(iVector3 start, iVector3 end)
    {
        this.start = start;
        this.end = end;
    }
    public iRectangle(Vector3 start, Vector3 end)
    {
        this.start = new iVector3(start);
        this.end = new iVector3(end);
    }

}



