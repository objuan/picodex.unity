using UnityEngine;
using System.IO;
using System.Linq;

// file locale , uso System.IO
class FileCache
{
	static bool winMode => Application.platform != RuntimePlatform.Android;

	public static void Initialize()
	{
		if (!Directory.Exists(Application.persistentDataPath))
			Directory.CreateDirectory(Application.persistentDataPath);

		if (!Directory.Exists(Application.persistentDataPath + "/levels"))
			Directory.CreateDirectory(Application.persistentDataPath + "/levels");

		if (!Directory.Exists(Application.persistentDataPath + "/bricks"))
			Directory.CreateDirectory(Application.persistentDataPath + "/bricks");

		Debug.Log("cache folder " + Application.persistentDataPath + "/levels");
		Debug.Log("cache folder " + Application.persistentDataPath + "/bricks");
	}

	public static bool IsNetworkPath(string full_path)
	{
		return full_path.Contains("://") || full_path.Contains(":///");
	}

	public static string GetFile(string path)
	{
		if (winMode)
			return null;
			/*
			var localPath = "";
			if (IsNetworkPath(path))
			{
				int i = path.IndexOf("/assets/");
				if (i != -1)
				{
					localPath = path.Substring(i + 8);
				}
				else
					return localPath;
			}
			else
				localPath = path;
			*/
			var cachePath = Application.persistentDataPath + "/" + path;
		
		Debug.Log("cache path " + cachePath);

		if (File.Exists(cachePath))
			return cachePath;
		else
			return null;
	}
	public static System.Collections.Generic.IEnumerable<string> GetFiles(string folder, string filter)
	{
		var cachePath = Application.persistentDataPath + "/" + folder;
		if (Directory.Exists(cachePath))
			return new DirectoryInfo(cachePath).GetFiles(filter).Select( X => (X.FullName.Substring(Application.persistentDataPath.Length+1).Replace("\\","/")));
		else
			return null;
	}

	public static void WriteAllText(string path, string content)
	{
		var cachePath = Application.persistentDataPath + "/" + path;
		if (!Directory.Exists(Path.GetDirectoryName(cachePath)))
			Directory.CreateDirectory(Path.GetDirectoryName(cachePath));

		Debug.Log("SAVE CACHE: "+ cachePath);
		File.WriteAllText(cachePath, content);
	}
}


public class PxFile
{
	static bool initialized = false;

	static bool useWriteCacheDebug = false;

	static bool useWriteCache = useWriteCacheDebug || !winMode;

	static bool winMode => Application.platform != RuntimePlatform.Android;

	
	////public static bool IsNetworkPath(string full_path) {
	//	return full_path.Contains("://") || full_path.Contains(":///");
	//}

	//static string basePath ="";

	public static string FullPath(string localPath)
	{
		return Application.streamingAssetsPath + "/" + localPath; 
	}

	
	public static string Normalize(string localPath)
	{
		//	Debug.Log("Normalize  " + path);
		var path = localPath;// Application.streamingAssetsPath + "/" + localPath;
		return "\\" + localPath;
		if (!winMode)
		{
			/*
			 * int i = path.IndexOf("/assets/");
			if (i != -1)
			{
				basePath = path.Substring(0, i+8);
				//Debug.Log("BASE PATH = " + basePath);
				return "\\"+ path.Substring(i+8);
			}
			else
				return path;
			*/
			return "\\" + localPath;
		}
		else
			return path;
	}
	public static string DeNormalize(string path)
	{
	//	Debug.Log("DeNormalize  " + path);
		if (path.StartsWith("\\"))
			path = path.Substring(1);
		return path;
	}
	

	static void _Initialize()
	{
		if (initialized) return;
		initialized = true;
		//if (Application.isPlaying)
		BetterStreamingAssets.Initialize();
		Debug.Log("Win Mode " + winMode);

		FileCache.Initialize();

	}

 

	public static System.Collections.Generic.IEnumerable<FileInfo> GetFiles(string folder,string filter)
	{
		_Initialize();

		var cacheFiles = FileCache.GetFiles(folder, filter);
	
		//	DirectoryInfo
	//	if (!IsNetworkPath(folder))
		//	return new DirectoryInfo(folder).GetFiles(filter);
	//	else
		{
			//Debug.Log("1.."+folder);
			//Debug.Log("2.." + Normalize(folder));

			var files = BetterStreamingAssets.GetFiles(Normalize(folder), filter, SearchOption.TopDirectoryOnly).Select( X => DeNormalize(X));

			if (cacheFiles != null)
			{
				var ll = files.ToList();
				foreach (var f in cacheFiles)
				{
					if (!ll.Contains(f))
						ll.Add(f);
				}
				files = ll.ToArray();
			}
			//foreach (var p in files)
			//{
			//		Debug.Log(p);
			//}

			var list =  files.Select(X => new FileInfo(X));

			//foreach (var p in list)
			//{
			//	Debug.Log(p.FullName);
			//}

			return list;
		
		}
	}
	public static bool Exists(string path)
	{
		_Initialize();
		var cacheFile = FileCache.GetFile(path);
		if (cacheFile != null)
			return File.Exists(cacheFile);

		path = Normalize(path);
		Debug.Log("Exists " + path);

		return BetterStreamingAssets.FileExists(path);

	}

	public static void Delete(string path)
	{
		_Initialize();
		var cacheFile = FileCache.GetFile(path);
		if (cacheFile != null)
		{
			Debug.Log("DELETE: " + cacheFile);
			if (File.Exists(cacheFile))
				File.Delete(cacheFile);
		}
		else
		{
			if (winMode)
			{

				var full = FullPath(path);
				Debug.Log("DELETE: " + full);
				if (File.Exists(full))
					File.Delete(full);
				/*if (!IsNetworkPath(full))
					File.Delete(full);
				else
				{
					//BetterStreamingAssets.de

				}
				*/
			}
		}
    }
    public static string ReadAllText(string path)
    {
		_Initialize();
		var cacheFile = FileCache.GetFile(path);
		if (cacheFile != null)
			return File.ReadAllText(cacheFile);
		else
			return BetterStreamingAssets.ReadAllText(Normalize(path));
		/*else if (!IsNetworkPath(path))
			return File.ReadAllText(path);
        else
        {
            return LoadStreamFile(path);
        }
		*/
	}
	public static byte[] ReadAllBytes(string path)
	{
		_Initialize();
		var cacheFile = FileCache.GetFile(path);
		if (cacheFile != null)
			return File.ReadAllBytes(cacheFile);
		else
			return BetterStreamingAssets.ReadAllBytes(Normalize(path));
		/*else if (!IsNetworkPath(path))
			return File.ReadAllBytes(path);
		else
		{
			return LoadStreamBytes(path);
		}
		*/
	}
	public static string[] ReadAllLines(string path)
    {
		_Initialize();
		var cacheFile = FileCache.GetFile(path);
		if (cacheFile != null)
			return File.ReadAllLines(cacheFile);
		else
			return BetterStreamingAssets.ReadAllLines(Normalize(path));
		/*if (!IsNetworkPath(path))
			return File.ReadAllLines(path);
        else
        {
            return LoadStreamFile(path).Split('\n');
        }
		*/
	}
	public static void WriteAllText(string path,string content)
	{
		_Initialize();
		if (useWriteCache)
		{
			FileCache.WriteAllText(path, content);
		}
		else
		{
			var full = FullPath(path);

			Debug.Log("SAVE: " + full);
			if (winMode)
			//if (!IsNetworkPath(full))
				File.WriteAllText(full, content);
		}
	}
	// ==================
	/*

	static string LoadStreamFile(string full_path)
	{
		//var localizedText = new Dictionary<string, string>();
		//	string filePath; 
		//filePath = Path.Combine(FolderPath + "/", fileName); 
		string dataAsJson = "";

		Debug.Log("LOAD :" + full_path);

		if (IsNetworkPath(full_path))
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

				Debug.Log("Load OK");

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

	static byte[] LoadStreamBytes(string full_path)
	{
		if (IsNetworkPath(full_path))
		{
			Debug.Log("LOAD B:" + full_path);
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
					return new byte[] { };
				}

				Debug.Log("Load OK");

				return www.downloadHandler.data;

			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
				return new byte[] { };
			}
		}
		else
		{
			var b = File.ReadAllBytes(full_path);
			return b;
		}
		//	return dataAsJson;
		//LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

		//for (int i = 0; i < loadedData.items.Length; i++)
		//{
		//	localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
		//	Debug.Log("KEYS:" +loadedData.items[i].key);
		//}

	}
	*/
}

