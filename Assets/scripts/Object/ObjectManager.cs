
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace MyLib
{
    public class ObjectManager : MonoBehaviour
    {
        Dictionary<int, GameObject> fakeObjects = new Dictionary<int,GameObject>();
        public static ObjectManager objectManager;
        public int myPlayerServerId = Util.LocalMyId;
        public GameObject myPlayerGameObj = null;

        /*
		 * 玩家服务器Id--->玩家实体
         * 
         * View 普通的怪物等实体
         * LocalId  playerId
         * ViewId
		 */
        public List<KBEngine.KBNetworkView> photonViewList = new List<KBEngine.KBNetworkView>();

        public List<KBEngine.KBNetworkView> GetNetView()
        {
            return photonViewList;
        }
        /*
		 * Monster Killed By Player
		 */
        //TODO: 使用MyEventSystem 来发送事件
        public VoidDelegate killEvent;

        /// <summary>
        /// 获得玩家实体对象
        /// 获得服务器怪物实体
        /// </summary>
        public KBEngine.KBNetworkView GetPlayerOrMonsterNetView(int playerId)
        {
            foreach (KBEngine.KBNetworkView view in photonViewList)
            {
                if (view != null && view.GetServerID() == playerId)
                {
                    return view;
                }
            }
            return null;
        }

        public GameObject GetFakeObj(int localId)
        {
            GameObject g;
            if (fakeObjects.TryGetValue(localId, out g))
            {
                return g;
            }
            return null;
        }

        //对象不一定是服务器对象因此通过localId来区分
        public GameObject GetLocalPlayer(int localId)
        {
            foreach (KBEngine.KBNetworkView p in photonViewList)
            {
                if (p.GetLocalId() == localId)
                {
                    return p.gameObject;
                }
            }
            return null;
        }

        public int GetMyProp(CharAttribute.CharAttributeEnum prop)
        {
            if (myPlayerServerId != Util.NotInitServerID)
            {
                var vi = GetPlayerOrMonsterNetView(myPlayerServerId);
                if (vi != null)
                {
                    return vi.GetComponent<CharacterInfo>().GetProp(prop);
                }
            }
            return 0;
        }

        public CharacterInfo GetMyData()
        {
            var vi = GetPlayerOrMonsterNetView(myPlayerServerId);
            if (vi != null)
            {
                return vi.GetComponent<CharacterInfo>();
            }
            return null;
        }

        public NpcAttribute GetMyAttr()
        {
            var myplayer = GetMyPlayer();
            if (myplayer != null)
            {
                return myplayer.GetComponent<NpcAttribute>();
            }
            return null;
        }

        public IEnumerator<GameObject> GetAllPlayer()
        {
            foreach (var p in photonViewList)
            {
                if (p.IsPlayer)
                {
                    yield return p.gameObject;
                }
            }
        }

        public GameObject GetMyPlayer()
        {
            return myPlayerGameObj;
        }

        //获得玩家自身或者其它玩家的属性数据 或者网络NPC的 NetView
        public GameObject GetPlayer(int playerId)
        {
            var view = GetPlayerOrMonsterNetView(playerId);
            if (view != null)
            {
                return view.gameObject;
            }
            return null;
        }



        //登录的时候 从SelectChar中获取等级数据
        //平时从 CharacterInfo 数据源获取数据
        //因为CharacterInfo 数据的初始化是在CreateMyPlayer 之后的
        //CharacterInfo SetProps Level 这里的值也要修改
        int GetMyLevel()
        {
            return 1;
        }

        /*
		 * SaveGame selectChar has My PlayerID
		 * But Here just Return -1 As My Id
		 */
        public int GetMyServerID()
        {
            return myPlayerServerId;
        }

        /*
		 * Local Allocated NpcID
		 */
        public int GetMyLocalId()
        {
            var view = GetPlayerOrMonsterNetView(myPlayerServerId);
            if (view != null)
            {
                var lid = view.GetLocalId();
                return lid;
            }
            return -1;
        }

     


        private Vector3 GetMyInitRot()
        {
            var dir = SaveGame.saveGame.bindSession.Direction;
            return Quaternion.Euler(new Vector3(0, dir, 0)) * Vector3.forward;
        }

        public string GetMyName()
        {
            return UserInfo.UserName;
        }




        public int GetMyJob()
        {
            Log.Sys("GetMyJob: " + ServerData.Instance.playerInfo.Roles.Job);
            return (int)ServerData.Instance.playerInfo.Roles.Job;
        }

        void Awake()
        {
            objectManager = this;
            DontDestroyOnLoad(this.gameObject);
        }

        //增加一个玩家实体对象
        public void AddObject(long unitId, KBEngine.KBNetworkView view)
        {
            photonViewList.Add(view);
        }

        /*
		 * PlayerId MySelf self is -1
		 * TODO:如果WorldManager正在进入新的场景，则缓存当前的服务器推送的Player,等待彻底进入场景再初始化Player
		 * TODO:CScene 进入场景之后解开缓存的Player数据
		 */
        private void AddPlayer(int unitId, int player)
        {
            if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter)
            {
            } else
            {
                throw new SystemException("正在切换场景，增加新的Player失败");
            }
        }




        public void DestroyByLocalId(int localId)
        {
            var keys = photonViewList.Where(f => true).ToArray();
            foreach (KBEngine.KBNetworkView v in keys)
            {
                if (v.GetLocalId() == localId)
                {
                    photonViewList.Remove(v);
                    DestroyFakeObj(v.GetLocalId());
                    GameObject.Destroy(v.gameObject);
                    break;
                }
            }
        }

        //删除Player和PhotonView
        public void DestroyPlayer(int playerID)
        {
            var player = playerID;
            if (myPlayerServerId == playerID)
            {
                myPlayerServerId = Util.LocalMyId;
            }

            //摧毁某个玩家所有的PhotonView对象  Destroy Fake object Fist Or Send Event ?
            //删除玩家控制的怪物实体
            var keys = photonViewList.Where(f => true).ToArray();
            foreach (KBEngine.KBNetworkView v in keys)
            {
                if (v == null)
                {
                    photonViewList.Remove(v);
                } else
                {
                    if (v.GetServerID() == player)
                    {
                        photonViewList.Remove(v);
                        DestroyFakeObj(v.GetLocalId());
                        GameObject.Destroy(v.gameObject);
                        break;
                    }
                }
            }
            MyEventSystem.PushEventStatic(MyEvent.EventType.RemovePlayer);
        }

        /// <summary>
        /// 摧毁自己玩家对象和单人副本怪物对象
        /// </summary>
        public void DestroyMySelf()
        {
            MyEventSystem.myEventSystem.PushEvent(MyEvent.EventType.PlayerLeaveWorld);
            //删除我自己玩家
            DestroyPlayer(myPlayerServerId);
        }


        /*
		 * 显示私聊人物信息
		 */
        public void ShowCharInfo(GCLoadMChatShowInfo info)
        {
        }

        //加载对应模型的基础职业骨架 FakeObject
        public GameObject NewFakeObject(int localId)
        {
            //每次显示都要初始化一下FakeObj的装备信息
            if (fakeObjects.ContainsKey(localId))
            {
                //fakeObjects [localId].GetComponent<NpcEquipment> ().InitDefaultEquip();
                fakeObjects [localId].GetComponent<NpcEquipment>().InitFakeEquip();
                return fakeObjects [localId];
            }

            var player = GetLocalPlayer(localId);
            var job = player.GetComponent<NpcAttribute>().ObjUnitData.job;
            Log.Sys("DialogPlayer is " + job.ToString());
            //var fakeObject = Instantiate (Resources.Load<GameObject> ("DialogPlayer/" + job.ToString ())) as GameObject;
            var fakeObject = SelectChar.ConstructChar(job);
            fakeObject.name = fakeObject.name + "_fake";


            fakeObject.SetActive(false);
            fakeObjects [localId] = fakeObject;
            fakeObject.GetComponent<NpcEquipment>().SetFakeObj();
            fakeObject.GetComponent<NpcEquipment>().SetLocalId(localId);
            fakeObject.GetComponent<NpcEquipment>().InitFakeEquip();

            Util.SetLayer(fakeObject, GameLayer.PlayerCamera);
            return fakeObject;
        }

        //当玩家对象被删除的时候,删除对应的玩家的FakeObj
        public void DestroyFakeObj(int localId)
        {
            var fake = GetFakeObj(localId);
            if (fake != null)
            {
                fakeObjects.Remove(localId);
                GameObject.Destroy(fake);
            }
        }

        /// <summary> 
        /// 副本内 :: 我方玩家构建流程 
        /// 
        /// 角色的初始化位置不同
        /// </summary>
        public GameObject CreateMyPlayerInCopy()
        {
            var player = CreateMyPlayerInternal();
            SetStartPointPosition(player);
            return player;
        }

        GameObject CreateMyPlayerInternal()
        {
            var job = MyLib.Job.WARRIOR;
            var udata = Util.GetUnitData(true, (int)job, GetMyLevel());

            //var player = Instantiate(Resources.Load<GameObject>(udata.ModelName)) as GameObject;
            var player = new GameObject("Player_Me");
			
            NetDebug.netDebug.AddConsole("Init Player tag layer transform");
            NGUITools.AddMissingComponent<NpcAttribute>(player);
            NGUITools.AddMissingComponent<MobaMePlayerAI>(player);
            player.tag = "Player";
            player.layer = (int)GameLayer.Npc;
            player.transform.parent = transform;
			

            //设置自己玩家的View属性
            var view = player.GetComponent<KBEngine.KBNetworkView>();
            view.SetServerID(myPlayerServerId);
			
            var npcAttr = player.GetComponent<NpcAttribute>();
            player.GetComponent<NpcAttribute>().SetObjUnitData(udata);
            player.GetComponent<NpcEquipment>().InitDefaultEquip();
            player.GetComponent<NpcEquipment>().InitPlayerEquipmentFromBackPack();
            npcAttr.InitName();
			
            player.name = "player_me_"+myPlayerServerId;
			
            ObjectManager.objectManager.AddObject(SaveGame.saveGame.selectChar.PlayerId, view);

            myPlayerGameObj = player;
            return player;
        }

        void SetStartPointPosition(GameObject player)
        {
            if (NetworkUtil.IsNet())
            {
                var pos = NetworkUtil.GetStartPos();
                player.transform.position = pos;
            } else
            {
                var startPoint = GameObject.Find("PlayerStart");
                player.transform.position = startPoint.transform.position;
                player.transform.forward = startPoint.transform.forward;
            }
        }

        void SetCityStartPos(GameObject player)
        {
            SetStartPointPosition(player);
        }

        ///<summary>
        /// 主城内
        /// 
        /// 我方玩家构建流程 野外 
        /// 	可能从副本退出    从BackPack 初始化装备
        /// 	也可能是刚登陆    需要等待BackPack 初始化结束 通知 穿戴装备
        /// 从副本退出则初始位置在刚才进入副本的位置
        /// 初次登陆的初始位置在登陆的属性中
        /// 
        /// 角色的初始化位置不同
        ///</summary> 
        public GameObject CreateMyPlayerInCity()
        {
            NetDebug.netDebug.AddConsole("LoginMyPlayer");
            var player = CreateMyPlayerInternal();
            SetCityStartPos(player);

            NetDebug.netDebug.AddConsole("ObjectManager Init Player Over");
            return player;
        }

        class MonsterInit
        {
            public UnitData unitData;
            public SpawnTrigger spawn;
            public GameObject spawnObj;

            public MonsterInit(UnitData ud, SpawnTrigger sp)
            {
                unitData = ud;
                spawn = sp;
            }

            public MonsterInit(UnitData ud, GameObject obj)
            {
                unitData = ud;
                spawnObj = obj;
            }
        }

        /// <summary>
        /// 创建其它玩家
        /// 玩家所在场景
        /// </summary>
        /// <param name="ainfo">Ainfo.</param>
        public void CreateOtherPlayer(AvatarInfo ainfo)
        {
            if (WorldManager.worldManager.station == WorldManager.WorldStation.Enter)
            {
                Log.Sys("CreateOtherPlayer: " + ainfo);
                var oldPlayer = GetPlayer(ainfo.Id);
                if (oldPlayer != null)
                {
                    Debug.LogError("PlayerExists: " + ainfo);
                    return;
                }

                if(myPlayerServerId == ainfo.Id)
                {
                    Debug.LogError("CreateMeAgain");
                    return;
                }

                var kbplayer = ainfo.Id;

                var udata = Util.GetUnitData(true, (int)ainfo.PlayerModelInGame, 1);
                //var player = GameObject.Instantiate(Resources.Load<GameObject>(udata.ModelName)) as GameObject;
                var player = new GameObject("Player_Other");

                var attr = NGUITools.AddMissingComponent<NpcAttribute>(player);
                //状态机类似 之后可能需要修改为其它玩家状态机
                NGUITools.AddMissingComponent<MobaOtherPlayer>(player);

                player.tag = "Player";
                player.layer = (int)GameLayer.Npc;

                NGUITools.AddMissingComponent<SkillInfoComponent>(player);
                player.GetComponent<NpcAttribute>().SetObjUnitData(udata);
                player.GetComponent<NpcEquipment>().InitDefaultEquip();

                player.name = "player_" + ainfo.Id;
                player.transform.parent = gameObject.transform;

                var netview = player.GetComponent<KBEngine.KBNetworkView>();
                netview.SetServerID(kbplayer);
                

                AddPlayer(kbplayer, kbplayer);
                AddObject(netview.GetServerID(), netview);
                attr.Init();
                var sync = player.GetComponent<MyLib.ISyncInterface>();
                sync.SetPositionAndDir(ainfo);
                sync.SetLevel(ainfo);
            }
        }

        /// <summary>
        /// 进入游戏更新自己的ServerID
        /// </summary>
        /// <param name="id"></param>
        public void RefreshMyServerId(int id)
        {
            Log.Sys("RefreshMyServerId: " + id);
            myPlayerServerId = id;
            if (myPlayerGameObj != null)
            {
                myPlayerGameObj.GetComponent<KBEngine.KBNetworkView>().SetServerID(myPlayerServerId);
                var startPos = NetworkUtil.GetStartPos();
                GetMyPlayer().transform.position = startPos;
            }
        }

        /// <summary>
        /// 伤害区域
        /// Moba中的移动小兵
        /// </summary>
        /// <param name="unitData"></param>
        /// <param name="spawn"></param>
        /// <param name="info"></param>
        public void CreateSpawnZoneEntity(UnitData unitData, EntityInfo info)
        {
            Log.Sys("CreateSpawnZoneEntity: "+unitData+" info "+info);
            //TODO: 这里可能有BUG
            if (info != null)
            {
                var oldMon = ObjectManager.objectManager.GetPlayer(info.Id);
                if (oldMon != null)
                {
                    var nv = oldMon.GetComponent<KBEngine.KBNetworkView>();
                    if (nv != null)
                    {
                        ObjectManager.objectManager.DestroyByLocalId(nv.GetLocalId());
                    }
                }
            }

            var Resource = Resources.Load<GameObject>(unitData.ModelName);
            GameObject g = Instantiate(Resource) as GameObject;
            NpcAttribute npc = NGUITools.AddMissingComponent<NpcAttribute>(g);

            g.transform.parent = transform;
            g.tag = GameTag.Enemy;
            g.layer = (int)GameLayer.Npc;
            if (info != null)
            {
                g.name += "_" + info.Id;
            }

            var type = Type.GetType("MyLib." + unitData.AITemplate);
            var t = typeof(NGUITools);
            var m = t.GetMethod("AddMissingComponent");
            Log.AI("Monster Create Certain AI  " + unitData.AITemplate + " " + type);
            var geMethod = m.MakeGenericMethod(type);
            geMethod.Invoke(null, new object[]{ g });// as AIBase;


            var netView = g.GetComponent<KBEngine.KBNetworkView>();
            //服务器返回的ViewId
            //Owner 客户端怪物 服务器怪物
            //Id ViewId
            if (info != null)
            {
                netView.SetServerID(info.Id);
            } else
            {
                netView.SetServerID(Util.NotInitServerID);
            }

            netView.IsPlayer = false;

            npc.SetObjUnitData(unitData);
            AddObject(netView.GetServerID(), netView);

            var sync = npc.GetComponent<MonsterSync>();
            if (sync != null)
            {
                sync.InitSetPos(NetworkUtil.FloatPos(info.X, info.Y, info.Z));
                sync.SyncAttribute(info);
            }

        }
      
    }
}