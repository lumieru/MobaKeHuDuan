using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyLib;

public class MobaModelLoader : MonoBehaviour {
    private GameObject model;
	
    public void LoadModel(int modelId)
    {
        var udata = Util.GetUnitData(true, modelId, 0);
        model = Object.Instantiate<GameObject>(Resources.Load<GameObject>(udata.ModelName));
        var scale = model.transform.localScale;
        model.transform.parent = transform;
        Util.InitGameObject(model);
        model.transform.localScale = scale;
        var attri = GetComponent<NpcAttribute>();
        MyEventSystem.myEventSystem.PushLocalEvent(attri.GetLocalId(), MyEvent.EventType.UpdateModel);
    }
}
