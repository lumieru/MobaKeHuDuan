using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

public class TestFunc : MonoBehaviour {

    [ButtonCallFunc()]
    public bool T;
	// Use this for initialization
	public void TMethod () {

        var pattern = @"class\s+(\w+)";
        var reg = new Regex(pattern);

        var dir1 = Path.Combine(Application.dataPath, "Moba");
        var dir2 = Path.Combine(Application.dataPath, "scripts");
        var dirInfo = new DirectoryInfo(dir1);
        var allCS = dirInfo.GetFiles("*.cs", SearchOption.AllDirectories);
        dirInfo = new DirectoryInfo(dir2);
        var allCS2 = dirInfo.GetFiles("*.cs", SearchOption.AllDirectories);

        var allFiles = new List<FileInfo>();
        allFiles.AddRange(allCS);
        allFiles.AddRange(allCS2);

        var fileTypes = new HashSet<string>();
        foreach (var f in allFiles)
        {
            var lines = File.ReadAllLines(f.FullName);
            foreach (var l in lines)
            {
                var matchs = reg.Matches(l);
                if (matchs.Count > 0)
                {
                    var className = matchs[0].Groups[1].ToString();
                    Debug.LogError(className);
                    break;
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
