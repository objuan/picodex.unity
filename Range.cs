
using Newtonsoft.Json;
using System;
using UnityEngine;

[System.Serializable]
public class FloatRange
{ 
	public float min=1;
	public float max=10;

	public FloatRange()
	{

	}
	public FloatRange(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}



