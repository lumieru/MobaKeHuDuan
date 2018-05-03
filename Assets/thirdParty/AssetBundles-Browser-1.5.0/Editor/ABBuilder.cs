using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundleBrowser;
using AssetBundleBrowser.AssetBundleModel;
using UnityEditor;
using System.IO;
using System;

public class ABBuilder {

    /// <summary>
    /// 遍历Assets/Resources  
    /// 创建一个AssetBundle
    /// 将Reousrces 所有资源加入到Bundle中
    /// 生成一个内存中结构信息
    /// </summary>
    internal static BundleInfo GenBundleMethod()
    {
        var chd = Model.s_RootLevelBundles.GetChild("full");
        if(chd != null)
        {
            return chd;
        }

        //Model.Rebuild();
        var newBundle = AssetBundleBrowser.AssetBundleModel.Model.CreateEmptyBundle(null, "full");
        return newBundle;
    }

    internal static void AddAsset(BundleInfo newBundle)
    {
        var path = Path.Combine(Application.dataPath, "Resources");
        Debug.LogError(path);
        var dirInfo = new DirectoryInfo(path);
        var allFiles = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        var assetInfo = new List<AssetInfo>();
        foreach (var a in allFiles)
        {
            var ap = FullPathToUnityPath(a.FullName);
            var asset = Model.CreateAsset(ap, "full");
            if (asset != null)
            {
                assetInfo.Add(asset);
            }
        }
        Debug.LogError("AddAsset:"+assetInfo.Count);
        Model.MoveAssetToBundle(assetInfo, "full", String.Empty);
        Model.ExecuteAssetMove();
        newBundle.RefreshAssetList();
    }

    public static string FullPathToUnityPath(string full)
    {
        var path = full.Replace("\\", "/").Replace(Application.dataPath, "Assets");
        return path;
    }
}
