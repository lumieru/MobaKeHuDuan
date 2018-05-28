using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaComponent : MonoBehaviour {

    public XLua.LuaTable table;
    public bool isModule = false;
    public string luaFile = "Test";
    private void Awake()
    {
        if (isModule)
        {
            table = LuaManager.DoModule(luaFile);
        }
        else
        {
            LuaManager.RequireFile(luaFile);
        }
    }

}
