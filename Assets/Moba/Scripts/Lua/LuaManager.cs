using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;

public class LuaManager : MonoBehaviour
{
    public bool startYet = false;

    public static LuaManager Instance;
    private void Awake()
    {
        Instance = this;
        luaEnv = new LuaEnv();
        luaEnv.AddLoader((ref string filePath) =>
        {
            var newPath = Path.Combine(Application.dataPath, "../LuaCode/" + filePath + ".lua");
            //Debug.LogError("filePath:"+filePath+":"+newPath);
            var con = File.ReadAllText(newPath);
            return System.Text.Encoding.UTF8.GetBytes(con);
        });

        luaEnv.DoString(@"
            require 'Main'
        ");
        InitLua();
        startYet = true;
    }

    private void InitLua()
    {
        luaEnv.Global.Get("DoFile", out DoFile);
        luaEnv.Global.Get("DoModule", out DoModule);
    }

    [CSharpCallLua]
    public delegate void S_VDel(string s1);
    [CSharpCallLua]
    public delegate LuaTable S_TDel(string s1);
    public static S_VDel DoFile;
    public static S_TDel DoModule;
    public static LuaEnv luaEnv;
}
