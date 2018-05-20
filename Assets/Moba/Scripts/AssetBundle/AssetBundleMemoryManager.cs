using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundles;
using System;

public class AssetBundleMemoryManager : MonoBehaviour
{
    /// <summary>
    /// 最大内存
    /// </summary>
    public int maxMemory = 10;
    public static AssetBundleMemoryManager Instance;
    private void Awake()
    {
        Instance = this;
        var func = FunctionalComparer<AssetBundleManager.AssetBundleContainer>.Create((a, b) =>
        {
            if (a.lastUsedTime > b.lastUsedTime)
            {
                return 1;
            }
            if (a.lastUsedTime < b.lastUsedTime)
            {
                return -1;
            }
            return 0;
        });
        leastRU = new C5.IntervalHeap<AssetBundleManager.AssetBundleContainer>(func);

    }

    private HashSet<AssetBundleManager.AssetBundleContainer> tranversed = new HashSet<AssetBundleManager.AssetBundleContainer>();
    public void AddAB(AssetBundles.AssetBundleManager.AssetBundleContainer container)
    {
        tranversed.Clear();
        container.lastUsedTime = Time.time;
        UpdateDenpencyTime(container);
        CheckMemory();
    }

    /// <summary>
    /// 每个AB 在使用的时候 都会刷新LRU
    /// </summary>
    /// <param name="container"></param>
    private void UpdateDenpencyTime(AssetBundleManager.AssetBundleContainer container)
    {
        tranversed.Add(container);
        container.InitReverse();
        leastRU.Add(ref container.handler, container);

        var abm = ABLoader.Instance.abm;
        foreach(var dep in container.Dependencies)
        {
            var c = abm.GetContainer(dep);
            c.lastUsedTime = container.lastUsedTime;
            if(!tranversed.Contains(c))
            {
                UpdateDenpencyTime(c);
            }
        }
    }

    private void CheckMemory()
    {
        var loaded = ABLoader.Instance.abm.GetLoadedABCount();
        if(loaded > maxMemory)
        {
            ReleaseMemory();
        }
    }
    /// <summary>
    /// 释放AB的策略
    /// 如何记录AB状态
    /// </summary>
    private void ReleaseMemory()
    {
        var minEle = leastRU.FindMin();
        //没有反向依赖才可以释放
        if (minEle.reverseDep.Count == 0)
        {
            leastRU.DeleteMin();
            ABLoader.Instance.abm.UnloadContainer(minEle);
        }
    }

    private C5.IntervalHeap<AssetBundleManager.AssetBundleContainer> leastRU;

    public class FunctionalComparer<T> : IComparer<T>
    {
        private Func<T, T, int> comparer;
        public FunctionalComparer(Func<T, T, int> comparer)
        {
            this.comparer = comparer;
        }
        public static IComparer<T> Create(Func<T, T, int> comparer)
        {
            return new FunctionalComparer<T>(comparer);
        }
        public int Compare(T x, T y)
        {
            return comparer(x, y);
        }
    }
}
