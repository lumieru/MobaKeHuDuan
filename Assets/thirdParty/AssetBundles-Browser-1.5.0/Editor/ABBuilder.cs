using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundleBrowser;
using AssetBundleBrowser.AssetBundleModel;
using UnityEditor;
using System.IO;
using System;

public class ABBuilder
{
    private static AssetBundleManageTab mgTab;

    public static float abNum = 11;
    internal static void SplitAB(AssetBundleManageTab tab)
    {
        Model.ForceShare = true;

        mgTab = tab;
        DeleteAll();

        //重新初始化Full信息
        var chd1 = Model.s_RootLevelBundles.GetChild("full");
        if(chd1 == null)
        {
            GenFullBundle(tab);
        }
        var abAssetList = CollectAllAsset();
        if(abAssetList == null)
        {
            return;
        }
        //return;
        var chd = Model.s_RootLevelBundles.GetChild("full");
        if (chd != null)
        {
            var bd = chd as BundleDataInfo;
            /*
            //收集full
            var con = bd.m_ConcreteAssets;
            var dep = bd.m_DependentAssets;

            var allAsset = new List<AssetInfo>();
            allAsset.AddRange(con);
            allAsset.AddRange(dep);
            */
            var allAsset = abAssetList;
            Debug.LogError("AbAssetList:"+abAssetList.Count);

            //删除
            Model.HandleBundleDelete(new List<BundleInfo>() { bd});
            mgTab.m_BundleTree.ReloadAndSelect(new List<int>());

            //切割Full
            var eachNum = allAsset.Count / abNum;
            var abList = new List<List<AssetInfo>>();
            var curNum = 0;
            var curCount = 0;
            var curList = new List<AssetInfo>();
            abList.Add(curList);
            foreach(var a in allAsset)
            {
                curList.Add(a);
                curCount++;
                if(curCount > eachNum)
                {
                    curCount = 0;
                    curList = new List<AssetInfo>();
                    abList.Add(curList);
                }
            }
            Debug.LogError("ABNum:"+abList.Count);

            EditorUtility.DisplayProgressBar("BuildAB", "", 0);
            var lsNb = new List<BundleInfo>();
            var curAbName = 0;
            foreach(var l in abList)
            {
                var ab = CreateBundle("ab_" + curAbName);
                EditorUtility.DisplayProgressBar("BuildAB:" + curAbName, "", curAbName / (abList.Count+1) );
                mgTab.m_BundleTree.ReloadAndSelect(ab.nameHashCode, false);
                AddAssetToAB(ab, l);
                curAbName++;
                lsNb.Add(ab);
            }
            mgTab.UpdateSelectedBundles(lsNb);
            EditorUtility.ClearProgressBar();

        }
        Model.ForceShare = false;
    }

    /// <summary>
    /// List 前面资源 不会依赖于后面的资源
    /// </summary>
    private static List<AssetInfo> CollectAllAsset()
    {
        var chd = Model.s_RootLevelBundles.GetChild("full");
        if (chd != null)
        {
            //收集full
            var bd = chd as BundleDataInfo;
            var con = bd.m_ConcreteAssets;
            var dep = bd.m_DependentAssets;

            var allAsset = new List<AssetInfo>();
            allAsset.AddRange(con);
            allAsset.AddRange(dep);
            Debug.LogError("AllAsset:"+allAsset.Count+":"+con.Count+":"+dep.Count);
            //List中的Asset和Dependency中的Asset没有统一

            var abAssetList = new List<AssetInfo>();
            //每个资源所依赖的外部资源
            //每个资源被哪些资源依赖

            //找到没有依赖的资源 --> AssetList
            //减去这些已经加入资源 依赖这些资源 依赖-1
            //接着遍历所有资源 寻找依赖数 = 0 放入AssetList中
            foreach(var a in allAsset)
            {
                a.GetDependencies();
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("AllAssetList:"+allAsset.Count);
            foreach(var a in allAsset)
            {
                a.InitReverseDep();
            }
            foreach(var a in allAsset)
            {
                sb.AppendLine("\"\""+a.fullAssetName);
                foreach(var d in a.depAssets)
                {
                    sb.AppendLine("\t"+"\\\\"+d.fullAssetName);
                    foreach(var r in d.m_ReverseDep)
                    {
                        sb.AppendLine("\t\t" + "//" + r.fullAssetName);
                    }
                }
            }

            foreach(var a in allAsset)
            {
                sb.AppendLine("++" + a.fullAssetName);
                foreach(var r in a.m_ReverseDep)
                {
                    sb.AppendLine("\t"+"||"+r.fullAssetName);
                }
            }
            

            EditorUtility.DisplayProgressBar("SortAsset", "", 0);
            var sortCount = 0;
            var allCount = allAsset.Count;
            while (allAsset.Count > 0 && sortCount < allCount+2)
            {
                var changeCount = 0;
                for(var i = 0; i < allAsset.Count;)
                {
                    var a = allAsset[i];
                    if(a.depCount <= 0)
                    {
                        EditorUtility.DisplayProgressBar("SortAsset" + a.displayName, a.displayName, (float)i / (float)(allAsset.Count+1) );
                        abAssetList.Add(a);
                        a.RemoveDep();
                        allAsset.RemoveAt(i);
                        changeCount++;
                    }else
                    {
                        i++;
                    }
                }
                
                EditorUtility.DisplayProgressBar("SortAsset" + sortCount++, ""+sortCount, sortCount/allCount );
            }
            EditorUtility.ClearProgressBar();

            sb.AppendLine("AllAssetLeft:"+allAsset.Count);
            if(allAsset.Count > 0)
            {
                foreach(var a in allAsset)
                {
                    sb.Append("##"+a.fullAssetName + ":" + a.depCount+"\n");
                    foreach(var d in a.depAssets)
                    {
                        sb.Append(".."+d.fullAssetName+"\n");
                    }
                }
                foreach(var a in abAssetList)
                {
                    sb.AppendLine("__:"+a.fullAssetName);
                    foreach(var r in a.m_ReverseDep)
                    {
                        sb.AppendLine("--"+r.fullAssetName);
                    }
                }
                File.WriteAllText(Path.Combine(Application.dataPath, "../result.txt"), sb.ToString());
                return null;
            }
            return abAssetList;
        }
        return null;
    }

    private static void DeleteAll()
    {
        var lb = new List<BundleInfo>();
        foreach (var c in Model.s_RootLevelBundles.m_Children)
        {
            lb.Add(c.Value);
        }
        Model.HandleBundleDelete(lb);
        mgTab.m_BundleTree.ReloadAndSelect(new List<int>());
    }

    private static BundleInfo CreateBundle(string abName)
    {
        var newBundle = AssetBundleBrowser.AssetBundleModel.Model.CreateEmptyBundle(null, abName);
        return newBundle;
    }
    private static void AddAssetToAB(BundleInfo bundle, List<AssetInfo> assetInfo)
    {
        Model.MoveAssetToBundle(assetInfo, bundle.displayName, String.Empty);
        Model.ExecuteAssetMove();
        bundle.RefreshAssetList();
    }

    /// <summary>
    /// 生成FullBundle
    /// </summary>
    /// <param name="tab"></param>
    internal static void GenFullBundle(AssetBundleManageTab tab)
    {
        var nb = GenBundleMethod();
        tab.m_BundleTree.ReloadAndSelect(nb.nameHashCode, false);
        AddAsset(nb);
        var bd = nb as BundleDataInfo;
        bd.GatherAllDep();

        var lsNb = new List<BundleInfo>();
        lsNb.Add(nb);
        tab.UpdateSelectedBundles(lsNb);
    }
    /// <summary>
    /// 遍历Assets/Resources  
    /// 创建一个AssetBundle
    /// 将Reousrces 所有资源加入到Bundle中
    /// 生成一个内存中结构信息
    /// </summary>
    internal static BundleInfo GenBundleMethod()
    {
        var chd = Model.s_RootLevelBundles.GetChild("full");
        if (chd != null)
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
        Debug.LogError("AddAsset:" + assetInfo.Count);
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
