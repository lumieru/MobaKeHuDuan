using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AssetBundles;
using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class ABLoader : SerializedMonoBehaviour
{
    [SerializeField]
    public Dictionary<string, string> resToAB;
    [ButtonCallFunc()]
    public bool GenResToAB;
    public void GenResToABMethod()
    {
        var abDir = Path.Combine(Application.dataPath, "../AssetBundles/" +AssetBundles.Utility.GetPlatformName());
        var dirInfo = new DirectoryInfo(abDir);
        var manifest = dirInfo.GetFiles("*.manifest");
        resToAB = new Dictionary<string, string>();
        foreach(var m in manifest)
        {
            HandleManifest(m);
        }

    }
    private void HandleManifest(FileInfo file)
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
                    if (resToAB.ContainsKey(resName))
                    {
                        Debug.LogError("Duplicate:" + resName);
                    }
                    else
                    {
                        resToAB.Add(resName, bundleName);
                    }
                }
            }else if(state == 2)
            {
                break;
            }
        }
    }

    public static ABLoader Instance;
    private AssetBundleManager abm;
    private bool initYet = false;

    private void Awake()
    {
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
        abm = new AssetBundleManager();
        abm.UseSimulatedUri();
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
        var abName = resToAB[path];
        var async = abm.GetBundleAsync(abName);
        yield return async;
        var ab = async.AssetBundle;
        //var ab = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "../AssetBundles/StandaloneWindows/test"));
        var go = ab.LoadAsset<GameObject>(path);
        ret[0] = go;
    }

    public string ResPathToAbPath(string resPath)
    {
        return Path.Combine("assets/resources", resPath+".prefab").ToLower().Replace("\\", "/");
    }
}
