using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AssetBundles;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class ABLoader : SerializedMonoBehaviour
{
    //[ButtonCallFunc()]
    //public bool GenResToAB;
    [Button]
    public void GenResToABMethod()
    {
        var abDir = Path.Combine(Application.dataPath, "../AssetBundles/" +AssetBundles.Utility.GetPlatformName());
        var dirInfo = new DirectoryInfo(abDir);
        var manifest = dirInfo.GetFiles("*.manifest");
        var resToAB2 = new Dictionary<string, string>();
        foreach(var m in manifest)
        {
            HandleManifest(m, resToAB2);
        }

        resToAB.Clear();
        foreach(var r in resToAB2)
        {
            resToAB.Add(new ResPair() {key= r.Key, value=r.Value });
        }
    }

    //[SerializeField]
    //public Dictionary<string, string> resToAB;
    [System.Serializable]
    public class ResPair {
        public string key;
        public string value;
    }
    public List<ResPair> resToAB;
    private Dictionary<string, string> kvPair;

    private void HandleManifest(FileInfo file, Dictionary<string, string> res)
    {
        var bundleName = Path.GetFileNameWithoutExtension(file.Name);
        var lines = File.ReadAllLines(file.FullName);
        var state = 0;
        foreach(var l in lines)
        {
            if(state == 0)
            {
                if (l.Contains("Assets:"))
                {
                    state = 1;
                }
            }
            else if(state == 1)
            {
                if (l.Contains("Dependencies:"))
                {
                    state = 2;
                }else
                {
                    var resName = l.Substring(2).ToLower();
                    if (res.ContainsKey(resName))
                    {
                        Debug.LogError("Duplicate:" + resName);
                    }
                    else
                    {
                        res.Add(resName, bundleName);
                    }
                }
            }else if(state == 2)
            {
                break;
            }
        }
    }

    public static ABLoader Instance;
    public AssetBundleManager abm;
    private bool initYet = false;

    private void Awake()
    {
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
        abm = new AssetBundleManager();
        abm.UseSimulatedUri();
        kvPair = new Dictionary<string, string>();
        foreach(var k in resToAB)
        {
            kvPair.Add(k.key, k.value);
        }
    }
    private IEnumerator Start()
    {
        var async = abm.InitializeAsync();
        yield return async;
        initYet = async.Success;
    }
    /// <summary>
    /// Resources.Load
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator LoadPrefab(string path, GameObject[] ret)
    {
        path = ResPathToAbPath(path);
        var abName = kvPair[path];
        Log.Net("LoadPrefab:"+path);
        var async = abm.GetBundleAsync(abName);
        yield return async;
        var ab = async.AssetBundle;
        Log.Net("FinishLoadAB:" + abName+":"+ab);
        var container = abm.GetContainer(ab.name);
        var go = ab.LoadAsset<GameObject>(path);
        ret[0] = go;
        AssetBundleMemoryManager.Instance.AddAB(container);
    }

    public bool hasScene(string sceneName)
    {
        var sceneFile = sceneName + ".unity";
        var scenePath = string.Empty;
        foreach(var n in kvPair)
        {
            var isScene = n.Key.EndsWith(sceneFile);
            if (isScene)
            {
                scenePath = n.Key;
                break;
            }
        }
        Log.Normal("HasScene:"+scenePath);
        return !string.IsNullOrEmpty(scenePath);
    }

    public IEnumerator LoadScene(string sceneName)
    {
        var sceneFile = sceneName + ".unity";
        var scenePath = "";
        foreach(var n in kvPair)
        {
            var isScene = n.Key.EndsWith(sceneFile);
            if (isScene)
            {
                scenePath = n.Key;
                break;
            }
        }
        var abName = kvPair[scenePath];
        var async = abm.GetBundleAsync(abName);
        yield return async;
        var ab = async.AssetBundle;
        Log.Net("FinishLoadAB:" + abName + ":" + ab);
        var container = abm.GetContainer(ab.name);
        AssetBundleMemoryManager.Instance.AddAB(container);
    }



    public string ResPathToAbPath(string resPath)
    {
        return Path.Combine("assets/resources", resPath+".prefab").ToLower().Replace("\\", "/");
    }
}
