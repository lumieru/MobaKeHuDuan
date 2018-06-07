#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TestAB : MonoBehaviour {

    [ButtonCallFunc()]
    public bool Move;
    public void MoveMethod()
    {
        BuildLuaAB();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void BuildLuaAB()
    {
        //从外部LuaCode 复制到Asset 目录中的LuaCode
        //删除旧的目录
        //复制整个目录
        //打包一个AB
        //修改每个lua文件名字 xxx.lua.txt 来确保可以被打包

        var destPath = Path.Combine(Application.dataPath, "LuaCode");
        var srcPath = Path.Combine(Application.dataPath, "../LuaCode");
        if (Directory.Exists(destPath))
        {
            Directory.Delete(destPath);
        }

        Directory.CreateDirectory(destPath);
        srcPath = Path.GetFullPath(srcPath);
        destPath = Path.GetFullPath(destPath);

        var dirInfo = new DirectoryInfo(srcPath);
        var luaFiles = dirInfo.GetFiles("*.lua", SearchOption.AllDirectories);
        foreach (var l in luaFiles)
        {
            var dirName = l.DirectoryName;
            var newDir = dirName.Replace(srcPath, destPath);
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }

            var srcFileName = l.FullName;
            
            var destFileName = srcFileName.Replace(srcPath, destPath);
            destFileName += ".txt";
            File.Copy(srcFileName, destFileName);
        }

    }
}

#endif