using System.Linq;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class MeshCollection
{
	Dictionary<string, Func<GameObject>> createMap = new Dictionary<string, Func<GameObject>>();

	Dictionary<string, MeshEntry> map = new Dictionary<string, MeshEntry>();
	//Dictionary<string, List<Func<GameObject>>> list = new Dictionary<string, List<Func<GameObject>>>();
	//Dictionary<string, List<Func<GameObject>>> actual = new Dictionary<string, List<Func<GameObject>>>();

	class MeshEntry
	{
		public List<GameObject> list = new List<GameObject>();
		public List<GameObject> actual = new List<GameObject>();
	}
	public void Init(string name, Func<GameObject> onCreate)
	{
		createMap.Add(name, onCreate);
		map.Add(name, new MeshEntry());
	}
	
	public void Start()
	{
		foreach (var v in map.Values) v.actual.Clear();
	}

	public bool Contains(string name)
	{
		return map.ContainsKey(name);
	}

	public GameObject Add(string name)
	{
		MeshEntry e = map[name];
		if (e.actual.Count < e.list.Count)
		{
			var go = e.list[e.actual.Count];
			e.actual.Add(go);
			return go;
		}
		else
		{
			var go1 = createMap[name].Invoke();
			e.list.Add(go1);
			e.actual.Add(go1);
			return go1;
		}
	}

	public void End()
	{
		foreach (var v in map.Values)
		{
			for(int i=0; i< v.list.Count;i++)
			{
				v.list[i].SetActive(i < v.actual.Count);

			}
		}
	}
}

 
