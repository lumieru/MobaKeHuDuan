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
            var con = File.ReadAllText(newPath);
            filePath = filePath + ".lua";
            return System.Text.Encoding.UTF8.GetBytes(con);
            //return ABLoader.Instance.LoadLua(filePath);
        });
        //InitLua();
    }

    public void InitLua()
    {
        luaEnv.DoString(@"
            require 'Main'
        ");
        luaEnv.Global.Get("DoFile", out RequireFile);
        luaEnv.Global.Get("DoModule", out DoModule);
        startYet = true;
    }

    public static void LoadAndDoFile(string filePath)
    {
        var newPath = Path.Combine(Application.dataPath, "../LuaCode/" + filePath + ".lua");
        var con = File.ReadAllText(newPath);
        var bytes = System.Text.Encoding.UTF8.GetBytes(con);
        luaEnv.DoString(bytes);
    }

    [CSharpCallLua]
    public delegate void S_VDel(string s1);
    [CSharpCallLua]
    public delegate LuaTable S_TDel(string s1);
    public static S_VDel RequireFile;
    public static S_TDel DoModule;

    public static LuaEnv luaEnv;
}
