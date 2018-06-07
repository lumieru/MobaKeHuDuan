
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

using MyLib;
using UnityEngine;
using System.Collections;
using KBEngine;
using System;

public class ClientApp : UnityEngine.MonoBehaviour
{
    public static KBEngineApp gameapp = null;
    public static ClientApp Instance;
    public enum ClientState
    {
        Idle,
        InInit,
        FinishInit,
    }
    public ClientState state = ClientState.Idle;
    public int testPort = 20000;
    public string remoteServerIP;
    public int remotePort = 10001;
    public float syncFreq = 0.1f;

    public int remoteUDPPort = 10001;
    public int UDPListenPort = 10002;

    public int ServerHttpPort = 12002;
    public int remoteKCPPort = 6060;

    public bool testAI = false;

    public string QueryServerIP
    {
        get;
        set;
    }
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 初始化连接服务器
    /// CheckUpdate
    /// AssetBundle Loader初始化
    /// 加载LuaAB
    /// </summary>
    public void AfterInitServer()
    {
        StartCoroutine(InitBeforeLogin());
    }
    private IEnumerator InitBeforeLogin()
    {
        state = ClientState.InInit;
        yield return HotUpdateManager.Instance.StartCoroutine(HotUpdateManager.Instance.CheckUpdate());
        yield return StartCoroutine(ABLoader.Instance.InitLoader());
        yield return StartCoroutine(ABLoader.Instance.LoadLuaAb());
        LuaManager.Instance.InitLua();
        state = ClientState.FinishInit;
        Debug.LogError("InitAllFinish");
    }

    public void StartServer() {
        Debug.Log("StartServer");
    }
    // Use this for initialization
    void Start()
    {
        UnityEngine.MonoBehaviour.print("client app start");
        gameapp = new KBEngineApp(this);
    }


    void OnDestroy()
    {
        UnityEngine.MonoBehaviour.print("clientapp destroy");
        if (KBEngineApp.app != null)
        {
            UnityEngine.MonoBehaviour.print("client app over " +  " over = ");
        }
    }

    void Update()
    {
        KBEUpdate();
    }

    //处理网络数据
    void KBEUpdate()
    {
        //处理网络回调
        gameapp.UpdateMain();
    }
    public bool IsPause = false;
    public void OnApplicationPause(bool pauseStatus) {
        IsPause = pauseStatus;
        if(pauseStatus && MyLib.ServerData.Instance != null){
            //ChuMeng.DemoServer.demoServer.GetThread().CloseServerSocket();
            MyLib.ServerData.Instance.SaveUserData();
        }
        if (pauseStatus)
        {
            var act = WorldManager.worldManager.GetActive();
            if (act != null && !act.IsCity)
            {
                WorldManager.ReturnCity();
            }
            StatisticsManager.Instance.QuitGame();
        }
    }

    [ButtonCallFunc()]
    public bool PauseTest;

    public void PauseTestMethod()
    {
        OnApplicationPause(true);
    }

}
