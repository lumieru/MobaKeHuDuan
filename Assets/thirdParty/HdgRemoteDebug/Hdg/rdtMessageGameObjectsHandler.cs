using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hdg
{
	public class rdtMessageGameObjectsHandler
	{
		private RemoteDebugServer m_server;

		private bool m_dontDestroyOnLoadBadObject;

		private List<GameObject> m_gameObjects;

		private List<rdtTcpMessageGameObjects.Gob> m_allGobs;

		private List<rdtTcpMessageComponents.Component> m_components;

		private List<Component> m_unityComponents;

		public rdtMessageGameObjectsHandler(RemoteDebugServer server)
		{
			m_gameObjects = new List<GameObject>(2048);
			m_allGobs = new List<rdtTcpMessageGameObjects.Gob>(2048);
			m_components = new List<rdtTcpMessageComponents.Component>(16);
			m_unityComponents = new List<Component>(16);
			m_server = server;
			m_server.AddCallback(typeof(rdtTcpMessageGetGameObjects), OnRequestGameObjects);
			m_server.AddCallback(typeof(rdtTcpMessageGetComponents), OnRequestGameObjectComponents);
			m_server.AddCallback(typeof(rdtTcpMessageUpdateComponentProperties), OnUpdateComponentProperties);
			m_server.AddCallback(typeof(rdtTcpMessageUpdateGameObjectProperties), OnUpdateGameObjectProperties);
			m_server.AddCallback(typeof(rdtTcpMessageSetArraySize), OnSetArraySize);
			m_server.AddCallback(typeof(rdtTcpMessageDeleteGameObjects), OnDeleteGameObjects);
            m_server.AddCallback(typeof(rdtTcpMessageSendLuaCode), OnSendLuaCode);
		}

		private void OnUpdateGameObjectProperties(rdtTcpMessage message)
		{
			rdtTcpMessageUpdateGameObjectProperties i = (rdtTcpMessageUpdateGameObjectProperties)message;
			GameObject gob = FindGameObject(i.m_instanceId);
			if (!((UnityEngine.Object)gob == (UnityEngine.Object)null))
			{
				if (i.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateEnabled))
				{
					gob.SetActive(i.m_enabled);
				}
				if (i.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateLayer))
				{
					gob.layer = i.m_layer;
				}
				if (i.HasFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateTag))
				{
					gob.tag = i.m_tag;
				}
			}
		}

		private void OnUpdateComponentProperties(rdtTcpMessage message)
		{
			rdtDebug.Debug(this, "OnUpdateComponentProperties");
			rdtTcpMessageUpdateComponentProperties i = (rdtTcpMessageUpdateComponentProperties)message;
			GameObject gob = FindGameObject(i.m_gameObjectInstanceId);
			if (!((UnityEngine.Object)gob == (UnityEngine.Object)null))
			{
				Component component = FindComponent(gob, i.m_componentInstanceId);
				if ((UnityEngine.Object)component == (UnityEngine.Object)null)
				{
					rdtDebug.Error(this, "Tried to update component with id {0} (name={1}) but couldn't find it!", i.m_componentInstanceId, i.m_componentName);
				}
				else
				{
					if (component is Behaviour)
					{
						((Behaviour)component).enabled = i.m_enabled;
					}
					else if (component is Renderer)
					{
						((Renderer)component).enabled = i.m_enabled;
					}
					else if (component is Collider)
					{
						((Collider)component).enabled = i.m_enabled;
					}
					if (i.m_properties != null)
					{
						m_server.SerializerRegistry.WriteAllFields(component, i.m_properties, i.m_arrayIndex);
						Graphic g = component as Graphic;
						if ((bool)g)
						{
							g.SetAllDirty();
						}
					}
				}
			}
		}

		private void OnSetArraySize(rdtTcpMessage message)
		{
			rdtDebug.Debug(this, "rdtTcpMessageSetArraySize");
			rdtTcpMessageSetArraySize i = (rdtTcpMessageSetArraySize)message;
			GameObject gob = FindGameObject(i.m_gameObjectInstanceId);
			if (!((UnityEngine.Object)gob == (UnityEngine.Object)null) && i.m_size >= 0)
			{
				Component component = FindComponent(gob, i.m_componentInstanceId);
				if ((UnityEngine.Object)component == (UnityEngine.Object)null)
				{
					rdtDebug.Error(this, "Tried to set array size on component with id {0} (name={1}) but couldn't find it!", i.m_componentInstanceId, i.m_componentName);
				}
				else
				{
					m_server.SerializerRegistry.SetArraySize(component, i.m_properties, i.m_size);
				}
			}
		}

		private void OnDeleteGameObjects(rdtTcpMessage message)
		{
			rdtTcpMessageDeleteGameObjects j = (rdtTcpMessageDeleteGameObjects)message;
			for (int i = 0; i < j.m_instanceIds.Count; i++)
			{
				GameObject gob = FindGameObject(j.m_instanceIds[i]);
				if (!((UnityEngine.Object)gob == (UnityEngine.Object)null))
				{
					UnityEngine.Object.Destroy(gob);
				}
			}
		}

        private void OnSendLuaCode(rdtTcpMessage message)
        {
            rdtDebug.Debug(this, "OnSendLuaCode");
            var j = (rdtTcpMessageSendLuaCode)message;
            var luaCode = j.luaCode;
            LuaManager.luaEnv.DoString(luaCode);
        }

		private void OnRequestGameObjectComponents(rdtTcpMessage message)
		{
			rdtTcpMessageGetComponents j = (rdtTcpMessageGetComponents)message;
			GameObject gob = (j.m_instanceId != 0) ? FindGameObject(j.m_instanceId) : null;
			rdtTcpMessageComponents msg = default(rdtTcpMessageComponents);
			msg.m_instanceId = (((UnityEngine.Object)gob != (UnityEngine.Object)null) ? j.m_instanceId : 0);
			msg.m_components = new List<rdtTcpMessageComponents.Component>();
			msg.m_layer = (((UnityEngine.Object)gob != (UnityEngine.Object)null) ? gob.layer : 0);
			msg.m_tag = (((UnityEngine.Object)gob != (UnityEngine.Object)null) ? gob.tag : "");
			msg.m_enabled = ((UnityEngine.Object)gob != (UnityEngine.Object)null && gob.activeInHierarchy);
			if ((bool)gob)
			{
				m_components.Clear();
				gob.GetComponents(m_unityComponents);
				if (m_unityComponents.Count > m_components.Capacity)
				{
					m_components.Capacity = m_unityComponents.Count;
				}
				for (int i = 0; i < m_unityComponents.Count; i++)
				{
					Component c = m_unityComponents[i];
					if ((UnityEngine.Object)c == (UnityEngine.Object)null)
					{
						rdtDebug.Debug(this, "Component is null, skipping");
					}
					else
					{
						List<rdtTcpMessageComponents.Property> properties = m_server.SerializerRegistry.ReadAllFields(c);
						if (properties == null)
						{
							rdtDebug.Debug(this, "Properties are null, skipping");
						}
						else
						{
							rdtTcpMessageComponents.Component component = default(rdtTcpMessageComponents.Component);
							if (c is Behaviour)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Behaviour)c).enabled;
							}
							else if (c is Renderer)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Renderer)c).enabled;
							}
							else if (c is Collider)
							{
								component.m_canBeDisabled = true;
								component.m_enabled = ((Collider)c).enabled;
							}
							else
							{
								component.m_canBeDisabled = false;
								component.m_enabled = true;
							}
							Type type = c.GetType();
							component.m_name = type.Name;
							component.m_assemblyName = type.AssemblyQualifiedName;
							component.m_instanceId = c.GetInstanceID();
							component.m_properties = properties;
							m_components.Add(component);
						}
					}
				}
			}
			msg.m_components = m_components;
			m_unityComponents.Clear();
			m_server.EnqueueMessage((rdtTcpMessage)(object)msg);
		}

		private void OnRequestGameObjects(rdtTcpMessage message)
		{
			rdtTcpMessageGameObjects i = default(rdtTcpMessageGameObjects);
			m_gameObjects.Clear();
			int total = 0;
			for (int m = 0; m < SceneManager.sceneCount; m++)
			{
				Scene scene = SceneManager.GetSceneAt(m);
				if (scene.isLoaded && scene.IsValid())
				{
					total += scene.rootCount;
					if (total > m_gameObjects.Capacity)
					{
						m_gameObjects.Capacity = scene.rootCount;
					}
					GameObject[] gobs = scene.GetRootGameObjects();
					m_gameObjects.AddRange(gobs);
				}
			}
			List<GameObject> gameObjects = m_gameObjects;
			List<GameObject> ddol = m_server.DontDestroyOnLoadObjects;
			if (!m_dontDestroyOnLoadBadObject)
			{
				int l = 0;
				while (l < ddol.Count)
				{
					if (!((UnityEngine.Object)ddol[l] == (UnityEngine.Object)null))
					{
						l++;
						continue;
					}
					rdtDebug.Log(rdtDebug.LogLevel.Warning, "A null GameObject was found in the DontDestroyOnLoadObjects list! Please ensure only DontDestroyOnLoad objects are added to the server.");
					m_dontDestroyOnLoadBadObject = true;
					break;
				}
			}
			for (int k = 0; k < ddol.Count; k++)
			{
				GameObject obj = ddol[k];
				if (!((UnityEngine.Object)obj == (UnityEngine.Object)null) && !gameObjects.Contains(obj))
				{
					gameObjects.Add(obj);
				}
			}
			int count = gameObjects.Count;
			if (count > m_allGobs.Capacity)
			{
				m_allGobs.Capacity = count;
			}
			m_allGobs.Clear();
			for (int j = 0; j < count; j++)
			{
				GameObject g = gameObjects[j];
				if (!((UnityEngine.Object)g == (UnityEngine.Object)null) && g.hideFlags == HideFlags.None && g.transform.hideFlags == HideFlags.None)
				{
					AddGameObject(g, m_allGobs);
				}
			}
			i.m_allGobs = m_allGobs;
			m_server.EnqueueMessage((rdtTcpMessage)(object)i);
			m_gameObjects.Clear();
		}

		private void AddGameObject(GameObject g, List<rdtTcpMessageGameObjects.Gob> list)
		{
			rdtTcpMessageGameObjects.Gob gob = default(rdtTcpMessageGameObjects.Gob);
			Scene scene = g.scene;
			object scene2;
			if (!scene.IsValid())
			{
				scene2 = "<no scene>";
			}
			else
			{
				scene = g.scene;
				scene2 = scene.name;
			}
			gob.m_scene = (string)scene2;
			gob.m_name = g.name;
			gob.m_instanceId = g.GetInstanceID();
			Transform parent = g.transform.parent;
			gob.m_hasParent = ((UnityEngine.Object)parent != (UnityEngine.Object)null);
			if (gob.m_hasParent)
			{
				gob.m_parentInstanceId = parent.gameObject.GetInstanceID();
			}
			gob.m_enabled = g.activeInHierarchy;
			list.Add(gob);
			for (int i = 0; i < g.transform.childCount; i++)
			{
				Transform child = g.transform.GetChild(i);
				AddGameObject(child.gameObject, list);
			}
		}

		public GameObject FindGameObject(int instanceId)
		{
			for (int k = 0; k < SceneManager.sceneCount; k++)
			{
				Scene scene = SceneManager.GetSceneAt(k);
				if (scene.isLoaded && scene.IsValid())
				{
					if (scene.rootCount > m_gameObjects.Capacity)
					{
						m_gameObjects.Capacity = scene.rootCount;
					}
					m_gameObjects.Clear();
					scene.GetRootGameObjects(m_gameObjects);
					int count = m_gameObjects.Count;
					for (int i = 0; i < count; i++)
					{
						GameObject parent = m_gameObjects[i];
						GameObject gob = FindGameObject(instanceId, parent);
						if ((UnityEngine.Object)gob != (UnityEngine.Object)null)
						{
							return gob;
						}
					}
				}
			}
			List<GameObject> ddol = m_server.DontDestroyOnLoadObjects;
			for (int j = 0; j < ddol.Count; j++)
			{
				GameObject parent2 = ddol[j];
				if (!((UnityEngine.Object)parent2 == (UnityEngine.Object)null))
				{
					GameObject gob2 = FindGameObject(instanceId, parent2);
					if ((UnityEngine.Object)gob2 != (UnityEngine.Object)null)
					{
						return gob2;
					}
				}
			}
			return null;
		}

		private GameObject FindGameObject(int instanceId, GameObject parent)
		{
			if (parent.GetInstanceID() == instanceId)
			{
				return parent;
			}
			for (int i = 0; i < parent.transform.childCount; i++)
			{
				GameObject child = parent.transform.GetChild(i).gameObject;
				GameObject gob = FindGameObject(instanceId, child);
				if ((UnityEngine.Object)gob != (UnityEngine.Object)null)
				{
					return gob;
				}
			}
			return null;
		}

		private Component FindComponent(GameObject gob, int instanceId)
		{
			gob.GetComponents(m_unityComponents);
			for (int i = 0; i < m_unityComponents.Count; i++)
			{
				Component component = m_unityComponents[i];
				if (component.GetInstanceID() == instanceId)
				{
					return component;
				}
			}
			return null;
		}
	}
}
