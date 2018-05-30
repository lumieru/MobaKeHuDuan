using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternals;
using UnityEngine;
using System.IO;

namespace Hdg
{
	public class ConnectionWindow : EditorWindow
	{
		private bool m_debug;

		private bool m_automaticRefresh = true;

		private bool m_forceRefresh;

		[NonSerialized]
		private bool m_waitingForGameObjects;

		private rdtTcpMessageComponents? m_components;

		private rdtClient m_client;

		private double m_lastTime;

		private rdtServerAddress m_currentServer;

		private Vector2 m_componentsScrollPos;

		private double m_gameObjectRefreshTimer;

		private double m_componentRefreshTimer;

		private rdtGuiSplit m_split;

		private rdtGuiTree<rdtTcpMessageGameObjects.Gob> m_tree;

		private bool m_updatingTree;

		private rdtSerializerRegistry m_serializerRegistry = new rdtSerializerRegistry();

		private rdtExpandedCache m_expandedCache = new rdtExpandedCache();

		private const float WIDE_MODE_SIZE_THRESHOLD = 330f;

		private const float LABEL_ADJUST_SIZE_THRESHOLD = 350f;

		private GUIStyle m_normalFoldoutStyle;

		private GUIStyle m_toggleStyle;

		private bool m_isProSkin;

		private rdtGuiProperty m_propertyGui;

		private rdtClientEnumerateServers m_serverEnum;

		private ServersMenu m_serversMenu;

		[NonSerialized]
		private rdtTcpMessageComponents.Component? m_pendingExpandComponent;

		[NonSerialized]
		private List<rdtTcpMessageGameObjects.Gob> m_gameObjects;

		[NonSerialized]
		private bool m_clearFocus;

		private static ConnectionWindow s_instance;

		private Texture2D m_sceneIcon;

		public static ConnectionWindow Instance
		{
			get
			{
				return s_instance;
			}
		}

		[MenuItem("Window/Hdg Remote Debug")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow<ConnectionWindow>(false, "Remote Debug", true);
		}

		public ConnectionWindow()
		{
			base.minSize = new Vector2(400f, 100f);
		}

		public void RestartServerEnumerator()
		{
			if (m_serverEnum != null)
			{
				rdtDebug.Debug("Stopping server enumerator");
				m_serverEnum.Stop();
			}
			m_serverEnum = new rdtClientEnumerateServers();
		}

		public void Connect(rdtServerAddress address)
		{
			Disconnect(true);
			m_expandedCache.Clear();
			m_pendingExpandComponent = null;
			m_components = null;
			m_currentServer = address;
			m_client = new rdtClient();
			m_propertyGui = new rdtGuiProperty(OnComponentValueChanged);
			m_client.Connect(address.IPAddress, address.m_port);
			m_client.AddCallback(typeof(rdtTcpMessageGameObjects), OnMessageGameObjects);
			m_client.AddCallback(typeof(rdtTcpMessageComponents), OnMessageGameObjectComponents);
		}

		private void InitStyles()
		{
			if (m_isProSkin == EditorGUIUtility.isProSkin && m_toggleStyle != null && m_normalFoldoutStyle != null && (UnityEngine.Object)m_toggleStyle.normal.background != (UnityEngine.Object)null)
			{
				return;
			}
			m_isProSkin = EditorGUIUtility.isProSkin;
			m_toggleStyle = new GUIStyle(EditorStyles.toggle);
			m_toggleStyle.overflow.top = -2;
			m_normalFoldoutStyle = new GUIStyle(EditorStyles.foldout);
			m_normalFoldoutStyle.overflow.top = -2;
			m_normalFoldoutStyle.active.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onActive.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onFocused.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.onFocused.background = EditorStyles.foldout.onNormal.background;
			m_normalFoldoutStyle.focused.textColor = EditorStyles.foldout.normal.textColor;
			m_normalFoldoutStyle.focused.background = EditorStyles.foldout.normal.background;
		}

		private void Disconnect(bool resetServer = true)
		{
			if (resetServer)
			{
				m_currentServer = null;
			}
			if (m_client != null)
			{
				m_client.Stop();
			}
			m_tree.Clear();
			m_components = null;
			m_waitingForGameObjects = false;
		}

		private void OnEnable()
		{
			m_debug = EditorPrefs.GetBool("Hdg.RemoteDebug.Debug", false);
			rdtDebug.s_logLevel = ((!m_debug) ? rdtDebug.LogLevel.Info : rdtDebug.LogLevel.Debug);
			rdtDebug.Debug("OnEnable");
			s_instance = this;
			m_split = new rdtGuiSplit(200f, 100f, this);
			m_tree = new rdtGuiTree<rdtTcpMessageGameObjects.Gob>();
			m_tree.SelectedNodesChanged += OnTreeSelectionChanged;
			m_tree.SelectedNodesDeleted += OnTreeSelectionDeleted;
			EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(OnPlaymodeStateChanged));
			RestartServerEnumerator();
			m_automaticRefresh = EditorPrefs.GetBool("Hdg.RemoteDebug.AutomaticRefresh", false);
			m_serversMenu = new ServersMenu(OnServerSelected, this);
			m_isProSkin = EditorGUIUtility.isProSkin;
			m_expandedCache.Clear();
		}

		private void OnDisable()
		{
			rdtDebug.Debug("OnDisable");
			EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(OnPlaymodeStateChanged));
			s_instance = null;
			m_serversMenu.Destroy();
			Disconnect(false);
			if (m_serverEnum != null)
			{
				m_serverEnum.Stop();
			}
		}

		private void OnMessageGameObjects(rdtTcpMessage message)
		{
			m_waitingForGameObjects = false;
			rdtTcpMessageGameObjects i = (rdtTcpMessageGameObjects)message;
			m_gameObjects = i.m_allGobs;
			m_updatingTree = true;
			List<rdtTcpMessageGameObjects.Gob> selectionData = (from x in m_tree.SelectedNodes
			where x.HasData
			select x.Data).ToList();
			List<string> selectionNoData = (from x in m_tree.SelectedNodes
			where !x.HasData
			select x.Name).ToList();
			BuildTree();
			if (selectionData.Count > 0 || selectionNoData.Count > 0)
			{
				List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> selectionNodesData = (from x in selectionData
				select m_tree.FindNode(x) into x
				where x != null
				select x).ToList();
				List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> selectionNodesNoData = (from x in selectionNoData
				select m_tree.FindNode(x) into x
				where x != null
				select x).ToList();
				m_tree.SelectedNodes.AddRange(selectionNodesData);
				m_tree.SelectedNodes.AddRange(selectionNodesNoData);
			}
			m_updatingTree = false;
			base.Repaint();
		}

		private void BuildTree()
		{
			m_tree.Clear();
			List<rdtTcpMessageGameObjects.Gob> list = (from x in m_gameObjects
			where !x.m_hasParent
			select x).ToList();
			Dictionary<int, rdtTcpMessageGameObjects.Gob> existing = new Dictionary<int, rdtTcpMessageGameObjects.Gob>();
			List<rdtTcpMessageGameObjects.Gob> nonRoots = (from x in (from x in m_gameObjects
			where x.m_hasParent
			select x).Where(delegate(rdtTcpMessageGameObjects.Gob x)
			{
				if (existing.ContainsKey(x.m_instanceId))
				{
					return false;
				}
				existing.Add(x.m_instanceId, x);
				return true;
			})
			orderby x.m_parentInstanceId
			select x).ToList();
			if ((UnityEngine.Object)m_sceneIcon == (UnityEngine.Object)null)
			{
				m_sceneIcon = EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");
			}
			List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> sceneRoots = new List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>();
			foreach (rdtTcpMessageGameObjects.Gob item in list)
			{
				rdtTcpMessageGameObjects.Gob r = item;
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node root = sceneRoots.FirstOrDefault((rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node x) => x.Name.Equals(r.m_scene));
				if (root == null)
				{
					root = m_tree.AddNode(r.m_scene);
					sceneRoots.Add(root);
					root.Icon = m_sceneIcon;
					root.IsBold = true;
				}
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = root.AddNode(r, r.m_enabled);
				AddChildren(node, nonRoots);
			}
		}

		private void AddChildren(rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node parentNode, List<rdtTcpMessageGameObjects.Gob> nonRoots)
		{
			int firstChildIndex;
			for (firstChildIndex = 0; firstChildIndex < nonRoots.Count && nonRoots[firstChildIndex].m_parentInstanceId != parentNode.Data.m_instanceId; firstChildIndex++)
			{
			}
			if (firstChildIndex < nonRoots.Count && nonRoots.Count != 0)
			{
				List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node> children = new List<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>();
				int count = 0;
				int j = firstChildIndex;
				while (j < nonRoots.Count)
				{
					rdtTcpMessageGameObjects.Gob g = nonRoots[j];
					if (g.m_parentInstanceId != parentNode.Data.m_instanceId)
					{
						break;
					}
					rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node2 = parentNode.AddNode(g, g.m_enabled);
					children.Add(node2);
					j++;
					count++;
				}
				nonRoots.RemoveRange(firstChildIndex, count);
				for (int i = 0; i < children.Count; i++)
				{
					rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = children[i];
					AddChildren(node, nonRoots);
				}
			}
		}

		private void Update()
		{
			if (EditorApplication.isCompiling && m_client != null && m_client.IsConnected)
			{
				Disconnect(false);
			}
			double time = EditorApplication.timeSinceStartup;
			double delta = time - m_lastTime;
			m_lastTime = time;
			UpdateServers(delta);
			if (m_client != null)
			{
				bool connected = m_client.IsConnected;
				bool connecting = m_client.IsConnecting;
				m_client.Update(delta);
				if (m_client.IsConnected != connected || ((!m_client.IsConnected && !m_client.IsConnecting) & connecting))
				{
					OnConnectionStatusChanged();
				}
				if (!m_automaticRefresh && !m_forceRefresh)
				{
					return;
				}
				m_gameObjectRefreshTimer -= delta;
				if (m_gameObjectRefreshTimer <= 0.0)
				{
					m_forceRefresh = false;
					RefreshGameObjects();
					m_gameObjectRefreshTimer = (double)Settings.GAMEOBJECT_UPDATE_TIME;
				}
				if (m_tree.SelectedNodes.Count == 1)
				{
					m_componentRefreshTimer -= delta;
					if (m_componentRefreshTimer <= 0.0)
					{
						RefreshComponents();
						m_componentRefreshTimer = (double)Settings.COMPONENT_UPDATE_TIME;
					}
				}
			}
		}

		private void RefreshComponents()
		{
			if (m_tree.SelectedNodes.Count != 0)
			{
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node node = ((ObservableList<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>)m_tree.SelectedNodes)[0];
				if (node.HasData)
				{
					rdtTcpMessageGetComponents message = default(rdtTcpMessageGetComponents);
					message.m_instanceId = node.Data.m_instanceId;
					m_client.EnqueueMessage((rdtTcpMessage)(object)message);
				}
			}
		}

		private void RefreshGameObjects()
		{
			if (m_client != null && m_client.IsConnected && !m_waitingForGameObjects)
			{
				rdtDebug.Debug(this, "Refreshing GameObject list from the server");
				m_client.EnqueueMessage((rdtTcpMessage)(object)default(rdtTcpMessageGetGameObjects));
				m_waitingForGameObjects = true;
			}
		}

		private void OnGUI()
		{
			if (m_clearFocus)
			{
				m_clearFocus = false;
				UnityEngine.GUI.FocusControl(null);
			}
			InitStyles();
			DrawToolbar();
			Draw();
			ProcessInput();
		}

        private string luaFileName = "testSend.lua";
        private void DrawSendLua()
        {
            EditorGUILayout.LabelField("lua文件", GUILayout.MaxWidth(80));
            luaFileName = EditorGUILayout.TextField(luaFileName);
            if (GUILayout.Button("发送Lua文件"))
            {
                SendLuaCode();
            } 
        } 
        private void SendLuaCode()
        {
            if (m_client != null && m_client.IsConnected && !m_waitingForGameObjects)
            {
                rdtDebug.Debug(this, "Refreshing GameObject list from the server");
                var path = Path.Combine(Application.dataPath, "../"+luaFileName);
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);
                    var luaMsg = new Hdg.rdtTcpMessageSendLuaCode();
                    luaMsg.luaCode = content;
                    m_client.EnqueueMessage(luaMsg);
                }
            }
        }
		private void DrawToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			m_serversMenu.Show(m_currentServer);
			bool num = m_client != null && m_client.IsConnecting;
			bool isConnected = m_client != null && m_client.IsConnected;
			string serverName = num ? "Connecting" : ((isConnected && m_currentServer != null) ? m_currentServer.ToString() : "Not connected");
			if (isConnected && m_currentServer != null && m_debug)
			{
				serverName = serverName + " - Server Version " + m_currentServer.m_serverVersion;
			}
			GUILayout.Label(serverName, EditorStyles.toolbarButton);
			m_tree.Filter = UnityEditorInternals.GUI.ToolbarSearchField(m_tree.Filter, GUILayout.Width(250f));
            DrawSendLua();

			GUILayout.FlexibleSpace();
			bool prevEnabled = UnityEngine.GUI.enabled;
			bool autoRefresh = GUILayout.Toggle(m_automaticRefresh, "Automatic Refresh", EditorStyles.toolbarButton);
			if (autoRefresh != m_automaticRefresh)
			{
				m_automaticRefresh = autoRefresh;
				EditorPrefs.SetBool("Hdg.RemoteDebug.AutomaticRefresh", m_automaticRefresh);
			}
			UnityEngine.GUI.enabled = !m_waitingForGameObjects;
			if (!m_waitingForGameObjects)
			{
				UnityEngine.GUI.enabled = !m_automaticRefresh;
			}
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
			{
				RefreshGameObjects();
				RefreshComponents();
			}
			UnityEngine.GUI.enabled = prevEnabled;
			if (GUILayout.Button("About", EditorStyles.toolbarButton))
			{
				ShowAbout();
			}
			GUILayout.EndHorizontal();
		}

		private void ShowAbout()
		{
			string msg = "\r\n    Hdg Remote Debug\r\n    Version {0}.{1}.{2} {3} {4}\r\n\r\n    http://www.horsedrawngames.com\r\n    info@horsedrawngames.com\r\n\r\n    (c) 2017 Horse Drawn Games Pty Ltd";
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Version version = executingAssembly.GetName().Version;
			object[] assemblyInfoVersionAttr = executingAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			string beta = "";
			if (assemblyInfoVersionAttr.Length != 0)
			{
				beta = ((AssemblyInformationalVersionAttribute)assemblyInfoVersionAttr[0]).InformationalVersion;
			}
			object[] assemblyConfigurationAttr = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
			string configuration = "";
			if (assemblyConfigurationAttr.Length != 0)
			{
				configuration = ((AssemblyConfigurationAttribute)assemblyConfigurationAttr[0]).Configuration;
			}
			EditorUtility.DisplayDialog("About", string.Format(msg, version.Major, version.Minor, version.Build, beta, configuration), "Ok");
		}

		private void Draw()
		{
			EditorGUILayout.BeginHorizontal();
			bool windowHasFocus = (UnityEngine.Object)EditorWindow.focusedWindow == (UnityEngine.Object)this;
			Rect rect = EditorGUILayout.GetControlRect(false, 1f, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.Width(m_split.SeparatorPosition));
			m_tree.Draw(rect, windowHasFocus);
			m_split.Draw();
			m_componentsScrollPos = EditorGUILayout.BeginScrollView(m_componentsScrollPos);
			float inspectorWidth = base.position.width - m_split.SeparatorPosition;
			EditorGUIUtility.wideMode = (inspectorWidth >= 330f);
			EditorGUIUtility.labelWidth = 0f;
			EditorGUIUtility.fieldWidth = 0f;
			if (inspectorWidth > 350f)
			{
				EditorGUIUtility.labelWidth = (inspectorWidth - 350f) * 0.5f + EditorGUIUtility.labelWidth;
			}
			DrawComponents();
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndHorizontal();
		}

		private void ProcessInput()
		{
			Event evt = Event.current;
			if (evt.isMouse && evt.type == EventType.MouseUp && m_pendingExpandComponent.HasValue)
			{
				UnityEngine.GUI.FocusControl(null);
				bool expanded = m_expandedCache.IsExpanded(m_pendingExpandComponent.Value, null);
				m_expandedCache.SetExpanded(!expanded, m_pendingExpandComponent.Value, null);
				m_pendingExpandComponent = null;
				base.Repaint();
			}
		}

		private void DrawComponents()
		{
			if (m_components.HasValue)
			{
				if (m_components.Value.m_instanceId == 0)
				{
					EditorGUILayout.LabelField("GameObject was not found.");
				}
				else
				{
					DrawGameObjectTitle();
					rdtGuiLine.DrawHorizontalLine();
					if (m_tree.SelectedNodes.Count <= 1)
					{
						foreach (rdtTcpMessageComponents.Component component in m_components.Value.m_components)
						{
							if (!DrawComponentTitle(component, null))
							{
								rdtGuiLine.DrawHorizontalLine();
							}
							else
							{
								m_propertyGui.DrawComponent(m_components.Value.m_instanceId, component, component.m_properties);
								EditorGUILayout.Space();
								rdtGuiLine.DrawHorizontalLine();
							}
						}
						EditorGUILayout.Space();
					}
				}
			}
		}

		private void DrawGameObjectTitle()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			bool enabled = EditorGUILayout.ToggleLeft("Enabled", m_components.Value.m_enabled);
			if (enabled != m_components.Value.m_enabled)
			{
				rdtTcpMessageComponents components = m_components.Value;
				components.m_enabled = enabled;
				m_components = components;
				((ObservableList<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>)m_tree.SelectedNodes)[0].Enabled = enabled;
				OnGameObjectChanged();
			}
			if (m_tree.SelectedNodes.Count == 1)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Tag", GUILayout.Width(30f));
				string newTag = EditorGUILayout.TagField(GUIContent.none, m_components.Value.m_tag, GUILayout.MinWidth(50f));
				if (newTag != m_components.Value.m_tag)
				{
					rdtTcpMessageComponents components2 = m_components.Value;
					components2.m_tag = newTag;
					m_components = components2;
					OnGameObjectChanged();
				}
				EditorGUILayout.LabelField("Layer", GUILayout.Width(40f));
				int newLayer = EditorGUILayout.LayerField(GUIContent.none, m_components.Value.m_layer, GUILayout.MinWidth(50f));
				if (newLayer != m_components.Value.m_layer)
				{
					rdtTcpMessageComponents components3 = m_components.Value;
					components3.m_layer = newLayer;
					m_components = components3;
					OnGameObjectChanged();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		private void OnComponentValueChanged(rdtGuiProperty.ValueChangedEvent valueChangedEvent)
		{
			if (valueChangedEvent.UpdateProperty)
			{
				rdtTcpMessageComponents.Property prop = valueChangedEvent.Hierarchy.Pop();
				prop.m_value = valueChangedEvent.NewValue;
				valueChangedEvent.Hierarchy.Push(prop);
			}
			OnPropertyChanged(valueChangedEvent, null);
		}

		private bool DrawComponentTitle(rdtTcpMessageComponents.Component component, Component unityComponent = null)
		{
			bool wasExpanded = m_expandedCache.IsExpanded(component, null);
			bool isExpanded = wasExpanded;
			Rect lastRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(18f));
			GUILayout.Space(4f);
			Rect rect = GUILayoutUtility.GetRect(13f, 16f, GUILayout.ExpandWidth(false));
			if (component.m_properties != null && component.m_properties.Count > 0)
			{
				isExpanded = EditorGUI.Foldout(rect, wasExpanded, GUIContent.none, m_normalFoldoutStyle);
				if (isExpanded != wasExpanded)
				{
					m_expandedCache.SetExpanded(isExpanded, component, null);
				}
			}
			bool enabled = true;
			int toggleWidth = m_toggleStyle.normal.background.width;
			if (component.m_canBeDisabled)
			{
				enabled = EditorGUILayout.Toggle(component.m_enabled, m_toggleStyle, GUILayout.Width((float)toggleWidth));
			}
			else
			{
				EditorGUILayout.LabelField("", GUILayout.Width((float)toggleWidth));
			}
			string name = ObjectNames.NicifyVariableName(component.m_name);
			if (m_debug)
			{
				name = name + ":" + component.m_instanceId;
			}
			EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
			if (component.m_canBeDisabled && enabled != component.m_enabled)
			{
				component.m_enabled = enabled;
				rdtGuiProperty.ValueChangedEvent evt2 = new rdtGuiProperty.ValueChangedEvent
				{
					Component = component
				};
				OnPropertyChanged(evt2, null);
			}
			EditorGUILayout.EndHorizontal();
			if (component.m_properties != null && component.m_properties.Count > 0)
			{
				Event evt = Event.current;
				if (lastRect.Contains(evt.mousePosition) && evt.isMouse)
				{
					if (evt.type == EventType.MouseDown)
					{
						m_pendingExpandComponent = component;
						evt.Use();
					}
					else if (evt.type == EventType.MouseUp && m_pendingExpandComponent.HasValue && m_pendingExpandComponent.Value.m_instanceId != component.m_instanceId)
					{
						m_pendingExpandComponent = null;
					}
				}
			}
			return isExpanded;
		}

		private void OnMessageGameObjectComponents(rdtTcpMessage message)
		{
			m_components = (rdtTcpMessageComponents)message;
			List<rdtTcpMessageComponents.Component> components = m_components.Value.m_components;
			for (int j = 0; j < components.Count; j++)
			{
				rdtTcpMessageComponents.Component c = components[j];
				if (c.m_properties == null)
				{
					rdtDebug.Debug(this, "Component '{0}' has no properties", c.m_name);
				}
				else
				{
					for (int i = 0; i < c.m_properties.Count; i++)
					{
						rdtTcpMessageComponents.Property p = c.m_properties[i];
						p.Deserialise(m_serializerRegistry);
						c.m_properties[i] = p;
					}
					components[j] = c;
				}
			}
			base.Repaint();
		}

		private void OnTreeSelectionChanged()
		{
			if (!m_updatingTree)
			{
				m_clearFocus = true;
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.SelectedNodeCollection selected = m_tree.SelectedNodes;
				if (selected.Count == 0 || selected.Any((rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node x) => !x.HasData))
				{
					m_pendingExpandComponent = null;
					m_components = null;
				}
				else
				{
					RefreshComponents();
				}
				base.Repaint();
			}
		}

		private void OnTreeSelectionDeleted()
		{
			rdtTcpMessageDeleteGameObjects msg = default(rdtTcpMessageDeleteGameObjects);
			IEnumerable<int> selectedIds = from x in m_tree.SelectedNodes
			select x.Data.m_instanceId;
			msg.m_instanceIds = selectedIds.ToList();
			m_client.EnqueueMessage((rdtTcpMessage)(object)msg);
			m_tree.SelectedNodes.Clear();
			m_gameObjectRefreshTimer = 0.10000000149011612;
			m_forceRefresh = true;
		}

		private List<rdtTcpMessageComponents.Property> CloneAndSerialize(Stack<rdtTcpMessageComponents.Property> hierarchy, bool serialiseValues = true)
		{
			List<rdtTcpMessageComponents.Property> list = new List<rdtTcpMessageComponents.Property>();
			rdtTcpMessageComponents.Property topProperty = hierarchy.Peek();
			rdtTcpMessageComponents.Property property = topProperty.Clone();
			if (serialiseValues)
			{
				property.m_value = m_serializerRegistry.Serialize(topProperty.m_value);
			}
			bool top = true;
			foreach (rdtTcpMessageComponents.Property item in hierarchy)
			{
				if (top)
				{
					top = false;
				}
				else
				{
					rdtTcpMessageComponents.Property parent = item.Clone();
					List<rdtTcpMessageComponents.Property> properties = new List<rdtTcpMessageComponents.Property>();
					properties.Add(property);
					parent.m_value = properties;
					property = parent;
				}
			}
			list.Add(property);
			return list;
		}

		private void OnPropertyChanged(rdtGuiProperty.ValueChangedEvent valueChangedEvent, Component unityComponent = null)
		{
			if (m_client != null)
			{
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node selected = ((ObservableList<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>)m_tree.SelectedNodes)[0];
				rdtTcpMessage message;
				if (valueChangedEvent.NewArraySize != -1)
				{
					rdtTcpMessageSetArraySize i = default(rdtTcpMessageSetArraySize);
					i.m_gameObjectInstanceId = selected.Data.m_instanceId;
					i.m_componentName = valueChangedEvent.Component.m_name;
					i.m_componentInstanceId = valueChangedEvent.Component.m_instanceId;
					i.m_size = valueChangedEvent.NewArraySize;
					Stack<rdtTcpMessageComponents.Property> hierarchy2 = valueChangedEvent.Hierarchy;
					if (hierarchy2 != null && hierarchy2.Count > 0)
					{
						i.m_properties = CloneAndSerialize(hierarchy2, false);
					}
					message = (rdtTcpMessage)(object)i;
				}
				else
				{
					rdtTcpMessageUpdateComponentProperties j = default(rdtTcpMessageUpdateComponentProperties);
					j.m_arrayIndex = valueChangedEvent.ArrayIndex;
					j.m_gameObjectInstanceId = selected.Data.m_instanceId;
					j.m_componentName = valueChangedEvent.Component.m_name;
					j.m_componentInstanceId = valueChangedEvent.Component.m_instanceId;
					j.m_enabled = valueChangedEvent.Component.m_enabled;
					Stack<rdtTcpMessageComponents.Property> hierarchy = valueChangedEvent.Hierarchy;
					if (hierarchy != null && hierarchy.Count > 0)
					{
						j.m_properties = CloneAndSerialize(hierarchy, true);
					}
					message = (rdtTcpMessage)(object)j;
				}
				m_client.EnqueueMessage(message);
				RefreshComponents();
			}
		}

		private void OnGameObjectChanged()
		{
			if (m_client != null)
			{
				rdtGuiTree<rdtTcpMessageGameObjects.Gob>.SelectedNodeCollection selected = m_tree.SelectedNodes;
				for (int i = 0; i < selected.Count; i++)
				{
					rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node gob = ((ObservableList<rdtGuiTree<rdtTcpMessageGameObjects.Gob>.Node>)selected)[i];
					rdtTcpMessageUpdateGameObjectProperties message = default(rdtTcpMessageUpdateGameObjectProperties);
					message.m_instanceId = gob.Data.m_instanceId;
					message.m_enabled = m_components.Value.m_enabled;
					message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateEnabled, true);
					message.m_layer = m_components.Value.m_layer;
					message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateLayer, selected.Count == 1);
					message.m_tag = m_components.Value.m_tag;
					message.SetFlag(rdtTcpMessageUpdateGameObjectProperties.Flags.UpdateTag, selected.Count == 1);
					m_client.EnqueueMessage((rdtTcpMessage)(object)message);
				}
			}
		}

		private void OnConnectionStatusChanged()
		{
			rdtDebug.Debug(this, "OnConnectionStatusChanged");
			if (m_client == null || !m_client.IsConnected)
			{
				m_currentServer = null;
			}
			m_pendingExpandComponent = null;
			m_components = null;
			m_expandedCache.Clear();
			m_tree.Clear();
			if (!m_automaticRefresh && m_client != null && m_client.IsConnected)
			{
				RefreshGameObjects();
			}
			base.Repaint();
		}

		private void UpdateServers(double delta)
		{
			m_serverEnum.Update(delta);
			m_serversMenu.Servers = m_serverEnum.Servers;
		}

		private void OnServerSelected(rdtServerAddress server)
		{
			if (server != null)
			{
				Connect(server);
			}
			else
			{
				Disconnect(true);
			}
		}

		[DidReloadScripts]
		private static void OnUnityReloadedAssemblies()
		{
			if (!((UnityEngine.Object)s_instance == (UnityEngine.Object)null))
			{
				s_instance.OnUnityReloadedAssembliesImp();
			}
		}

		private void OnUnityReloadedAssembliesImp()
		{
			rdtDebug.Debug(this, "OnUnityReloadedAssemblies");
			rdtDebug.s_logLevel = ((!m_debug) ? rdtDebug.LogLevel.Info : rdtDebug.LogLevel.Debug);
			if (m_currentServer != null)
			{
				if (m_currentServer.IPAddress == null)
				{
					m_currentServer = null;
				}
				else
				{
					Connect(m_currentServer);
				}
			}
		}

		private void OnPlaymodeStateChanged()
		{
			Disconnect(true);
		}
	}
}
