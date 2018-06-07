using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using MiniJSON;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class HotUpdateManager : MonoBehaviour {
    public static HotUpdateManager Instance;
    private void Awake()
    {
        Instance = this;
    }
#if UNITY_EDITOR
    [ButtonCallFunc()]
    public bool GenCrc;
    public void GenCrcMethod()
    {
        var abDir = Path.Combine(Application.dataPath, "../AssetBundles/" + AssetBundles.Utility.GetPlatformName());
        var dirInfo = new DirectoryInfo(abDir);
        var manifest = dirInfo.GetFiles("*.manifest", SearchOption.TopDirectoryOnly);
        var abCrc = new Dictionary<string, string>();
        foreach(var m in manifest)
        {
            /*
            var dirName = m.DirectoryName;
            var abName = Path.Combine(dirName, fn);
            */
            var fn = Path.GetFileNameWithoutExtension(m.FullName);
            var crc = HandleManifest(m);
            abCrc.Add(fn, crc);
        }
        var json = Json.Serialize(abCrc);
        File.WriteAllText(abDir + "/abCrc.json", json);
    }

    private string HandleManifest(FileInfo file)
    {
        var lines = File.ReadAllLines(file.FullName);
        var state = 0;
        foreach (var l in lines)
        {
            if(state == 0)
            {
                if (l.StartsWith("CRC:"))
                {
                    var crc = l.Substring(5);
                    return crc;
                }
            }
        }
        return string.Empty;
    }
#endif	

    private void Start()
    {
        //StartCoroutine(CheckUpdate());
    }
    /// <summary>
    /// ab 比较 CrC
    /// </summary>
    public IEnumerator CheckUpdate()
    {
        var platform = AssetBundles.Utility.GetPlatformName();

        var url = "http://" + ClientApp.Instance.QueryServerIP + ":" + 9090 + "/StandaloneWindows/abCrc.json";
        Log.Net("HttpReq: " + url);
        var w = new WWW(url);
        yield return w;
        if (string.IsNullOrEmpty(w.error))
        {
            //服务器上ABCrc
            var jsonContent = w.text;
            Log.Sys("HttpResult: " + jsonContent);
            var serverDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            //本地存储获得AbCrc信息
            var localPath = string.Format("{0}/../AssetBundles/StandaloneWindows/abCrc.json", Application.dataPath);
            var localFile = File.ReadAllText(localPath);
            var localDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(localFile);

            Log.Net(jsonContent);
            Log.Net(localFile);

            //新增的AB 减少的AB 以及变化的AB
            var keys = serverDict.Keys;
            var serverHash = new HashSet<string>(keys);

            var keys2 = localDict.Keys;
            var localHash = new HashSet<string>(keys2);

            //新增
            var copyServer = new HashSet<string>(keys);
            copyServer.ExceptWith(localHash);

            var changed = new HashSet<string>();
            //变化的
            serverHash.IntersectWith(localHash);
            foreach(var h in serverHash)
            {
                var serverCrc = serverDict[h];
                if(serverCrc != localDict[h])
                {
                    changed.Add(h);
                }
            }

            var toDownload = new List<string>();
            toDownload.AddRange(copyServer);
            toDownload.AddRange(changed);

            Debug.LogError("ToDownload:"+toDownload.Count);
            foreach(var ab in toDownload)
            {
                var url2 = "http://" + ClientApp.Instance.QueryServerIP + ":" + 9090 + "/StandaloneWindows/"+ab;
                var url3 = "http://" + ClientApp.Instance.QueryServerIP + ":" + 9090 + "/StandaloneWindows/"+ab+".manifest";
                var http = new WWW(url2);
                yield return http;
                var bytes = http.bytes;
                var localPath2 = string.Format("{0}/../AssetBundles/StandaloneWindows/"+ab, Application.dataPath);
                File.WriteAllBytes(localPath2, bytes);

                Debug.LogError(localPath2);

                var http3 = new WWW(url3);
                yield return http3;
                var bytes3 = http3.bytes;
                var localPath3 = string.Format("{0}/../AssetBundles/StandaloneWindows/"+ab+".manifest", Application.dataPath);
                File.WriteAllBytes(localPath3, bytes3);

                Debug.LogError(localPath3);
            }

            File.WriteAllText(localPath, jsonContent);
        }
        else
        {
            Debug.LogError("HotError:"+w.error);
        }
    }


}
