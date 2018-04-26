using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AssetBundles;

public class ABLoader : MonoBehaviour {

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
        var async = abm.GetBundleAsync("test");
        yield return async;
        var ab = async.AssetBundle;
        //var ab = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "../AssetBundles/StandaloneWindows/test"));
        var go = ab.LoadAsset<GameObject>(path);
        ret[0] = go;
    }

    public string ResPathToAbPath(string resPath)
    {
        return Path.Combine("assets/resources", resPath+".prefab").ToLower();
    }
}
