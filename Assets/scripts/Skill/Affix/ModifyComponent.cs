using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class ModifyComponent : MonoBehaviour
{
    private List<AffixSpawn> allAffixes = new List<AffixSpawn>();
       
    private void Awake()
    {
    }
    // Use this for initialization
    void Start () {
		
	}
	
    public void AddBuff(AffixSpawn affix)
    {
        affix.modify = this;
        allAffixes.Add(affix);
        affix.OnEnter();
    }
    public void RemoveBuff(string buffName)
    {
        AffixSpawn buff = null;
        foreach (var a in allAffixes)
        {
            if (a.AffixName == buffName)
            {
                allAffixes.Remove(a);
                buff = a;
                break;
            }
        }
        if (buff != null)
        {
            buff.OnExit();
            GameObject.Destroy(buff.gameObject);
        }
    }
}
