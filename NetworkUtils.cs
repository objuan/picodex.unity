using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public  class NetworkUtils
{

	public static string LoadStreamFile(string full_path)
	{
		//var localizedText = new Dictionary<string, string>();
		//	string filePath; 
		//filePath = Path.Combine(FolderPath + "/", fileName); 
		string dataAsJson = "";

		//Debug.Log("LOAD :" + full_path);

		if (full_path.Contains("://") || full_path.Contains(":///"))
		{
		//	var full_path1 = "jar:file://" + Application.dataPath + "!/assets/" + "Roads/" + new FileInfo(full_path).Name;

		//	Debug.Log("<< " + full_path1);

			try
			{
				UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(full_path);
				var ret = www.SendWebRequest();
				while (ret.isDone == false || www.isDone == false)
				{
					System.Threading.Thread.Sleep(10);
				}
				if (www.isNetworkError || www.isHttpError)
				{
					Debug.LogWarning($"Network error whilst downloading [{full_path}] Error: [{www.error}]");
					return "";
				}

				//yield return www.SendWebRequest();
				//dataAsJson = www.downloadHandler.text;

				Debug.Log("DONE :" + dataAsJson);

				//yield return www.downloadHandler.text;
				return www.downloadHandler.text;

			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				return "";
			}
		}
		else
		{
			dataAsJson = File.ReadAllText(full_path);
			return dataAsJson;
		}
		//	return dataAsJson;
		//LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

		//for (int i = 0; i < loadedData.items.Length; i++)
		//{
		//	localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
		//	Debug.Log("KEYS:" +loadedData.items[i].key);
		//}

	}
}

