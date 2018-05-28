using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public static class HotfixConfig
{
    [Hotfix]
    public static List<Type> by_property
    {
        get
        {
            var allTypes = Assembly.Load("Assembly-CSharp").GetTypes();

            //return allTypes.Select((a)=>a).ToList();

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
                        //Debug.LogError(className);
                        //break;
                        if(!fileTypes.Contains(className))
                            fileTypes.Add(className);
                    }
                }
            }

            var retList = new List<Type>();
            foreach(var t in allTypes)
            {
                if (fileTypes.Contains(t.Name))
                {
                    retList.Add(t);
                }
            }
            Debug.LogError("Types:"+allTypes.Length+":"+retList.Count+":"+fileTypes.Count);

            var sb = new StringBuilder();
            sb.AppendLine(retList.Count.ToString());
            foreach(var t in retList)
            {
                sb.AppendLine(t.FullName);
            }
            File.WriteAllText(Path.Combine(Application.dataPath, "../HotTypes.txt"), sb.ToString());
            return retList;
        }
    }
}
