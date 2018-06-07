using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLua : MonoBehaviour {

    public string luaFile = "TestHot";
    [ButtonCallFunc()]
    public bool T;
    public void TMethod()
    {
        LuaManager.LoadAndDoFile(luaFile);
    }

    [ButtonCallFunc()]
    public bool DoFile;
    public void DoFileMethod()
    {
        LuaManager.RequireFile(luaFile);
    }
    public void Update()
    {
        
    }
}
