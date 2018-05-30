using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Hdg
{
	public class rdtGuiTree<T>
	{
		public class SelectedNodeCollection : ObservableList<Node>
		{
		}

		public class Node
		{
			private rdtExpandedCache m_expandedCache;

			public T Data
			{
				get;
				private set;
			}

			public string Name
			{
				get;
				private set;
			}

			public List<Node> Children
			{
				get;
				private set;
			}

			public Node Parent
			{
				get;
				private set;
			}

			public bool Enabled
			{
				get;
				set;
			}

			public bool HasData
			{
				get;
				private set;
			}

			public Texture2D Icon
			{
				get;
				set;
			}

			public bool IsBold
			{
				get;
				set;
			}

			public int Depth
			{
				get;
				set;
			}

			public bool Expanded
			{
				get
				{
					int hash = HasData ? Data.GetHashCode() : Name.GetHashCode();
					return m_expandedCache.IsExpanded(hash, null);
				}
				set
				{
					int hash = HasData ? Data.GetHashCode() : Name.GetHashCode();
					m_expandedCache.SetExpanded(value, hash, null);
				}
			}

			public Node(string name, Node parent, rdtExpandedCache expandedCache)
			{
				Depth = ((parent != null) ? (parent.Depth + 1) : 0);
				Parent = parent;
				HasData = false;
				Name = name;
				m_expandedCache = expandedCache;
				Enabled = true;
				Children = new List<Node>();
			}

			public Node(T data, Node parent, rdtExpandedCache expandedCache)
			{
				Depth = ((parent != null) ? (parent.Depth + 1) : 0);
				HasData = true;
				m_expandedCache = expandedCache;
				Enabled = true;
				Parent = parent;
				Data = data;
				Children = new List<Node>();
				Name = Data.ToString();
			}

			public Node AddNode(T data, bool enabled = true)
			{
				Node node = new Node(data, this, m_expandedCache);
				node.Enabled = enabled;
				Children.Add(node);
				return node;
			}
		}

		private const float ROW_HEIGHT = 16f;

		private const float INDENT_WIDTH = 13f;

		private const float FOLDOUT_WIDTH = 12f;

		private const float BASE_INDENT = 2f;

		private const float ICON_WIDTH = 16f;

		private const float SCROLLBAR_WIDTH = 16f;

		private const float TREE_BOTTOM_MARGIN = 2f;

		private const float SPACE_BETWEEN_ICON_AND_TEXT = 2f;

		private GUIContent m_tempContent = new GUIContent();

		private Vector2 m_scrollPosition;

		private Node m_root;

		private SelectedNodeCollection m_selectedNodes;

		private GUIStyle m_foldoutStyle;

		private GUIStyle m_lineStyle;

		private GUIStyle m_disabledLineStyle;

		private GUIStyle m_boldLineStyle;

		private GUIStyle m_selectionStyle;

		private GUIStyle m_noDataStyle;

		private Rect m_scrollViewRect;

		private GUIContent m_content = new GUIContent();

		private bool m_hasFocus;

		private rdtExpandedCache m_expandedCache;

		private string m_filter;

		private List<Node> m_visibleNodes;

		private bool m_visibleNodesDirty;

		public rdtExpandedCache ExpandedCache
		{
			get
			{
				return m_expandedCache;
			}
		}

		public SelectedNodeCollection SelectedNodes
		{
			get
			{
				return m_selectedNodes;
			}
		}

		public string Filter
		{
			get
			{
				return m_filter;
			}
			set
			{
				m_visibleNodesDirty |= !string.Equals(m_filter, value);
				string prevFilter = m_filter;
				m_filter = value;
				if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(prevFilter))
				{
					EnsureVisible(SelectedNodes);
				}
			}
		}

        public event Action SelectedNodesChanged;
        /*
    {
        [CompilerGenerated]
        add
        {
            Action action = this.SelectedNodesChanged;
            Action action2;
            do
            {
                action2 = action;
                Action value2 = (Action)Delegate.Combine(action2, value);
                action = Interlocked.CompareExchange<Action>(ref this.SelectedNodesChanged, value2, action2);
            }
            while ((object)action != action2);
        }
        [CompilerGenerated]
        remove
        {
            Action action = this.SelectedNodesChanged;
            Action action2;
            do
            {
                action2 = action;
                Action value2 = (Action)Delegate.Remove(action2, value);
                action = Interlocked.CompareExchange<Action>(ref this.SelectedNodesChanged, value2, action2);
            }
            while ((object)action != action2);
        }
    }
    */

        public event Action SelectedNodesDeleted;
        /*
		{
			[CompilerGenerated]
			add
			{
				Action action = this.SelectedNodesDeleted;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref this.SelectedNodesDeleted, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = this.SelectedNodesDeleted;
				Action action2;
				do
				{
					action2 = action;
					Action value2 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref this.SelectedNodesDeleted, value2, action2);
				}
				while ((object)action != action2);
			}
		}
        */

		public rdtGuiTree()
		{
			m_expandedCache = new rdtExpandedCache();
			m_root = new Node("root", null, m_expandedCache);
			m_root.Depth = -1;
			m_selectedNodes = new SelectedNodeCollection();
			m_selectedNodes.ListChanged += OnSelectedNodesChanged;
			m_visibleNodesDirty = true;
		}

		private void OnSelectedNodesChanged(ObservableList<Node> obj)
		{
			if (this.SelectedNodesChanged != null)
			{
				this.SelectedNodesChanged();
			}
		}

		public Node FindNode(T data)
		{
			return Enumerable.FirstOrDefault<Node>((IEnumerable<Node>)BuildFlatList(m_root.Children, true), (Func<Node, bool>)((Node x) => x.Data.Equals(data)));
		}

		public Node FindNode(string name)
		{
			return Enumerable.FirstOrDefault<Node>((IEnumerable<Node>)BuildFlatList(m_root.Children, true), (Func<Node, bool>)((Node x) => x.Name.Equals(name)));
		}

		public Node AddNode(T data, bool enabled = true)
		{
			Node node = new Node(data, m_root, m_expandedCache);
			node.Enabled = enabled;
			m_root.Children.Add(node);
			m_visibleNodesDirty = true;
			return node;
		}

		public Node AddNode(string name)
		{
			Node node = new Node(name, m_root, m_expandedCache);
			node.Enabled = true;
			m_root.Children.Add(node);
			m_visibleNodesDirty = true;
			return node;
		}

		public void Clear()
		{
			m_root.Children.Clear();
			SelectedNodes.Clear();
			m_visibleNodesDirty = true;
		}

		public void Draw(Rect rect, bool windowHasFocus)
		{
			RefreshVisibleNodes();
			if (Event.current.type == EventType.Repaint)
			{
				m_scrollViewRect = rect;
			}
			m_hasFocus &= windowHasFocus;
			InitStyles();
			ProcessKeyboardInput();
			Vector2 visibleSize = GetTotalSize();
			Rect viewRect = new Rect(0f, 0f, rect.width, visibleSize.y);
			if (viewRect.height > rect.height)
			{
				viewRect.width -= 16f;
			}
			m_scrollPosition = GUI.BeginScrollView(rect, m_scrollPosition, viewRect);
			int first;
			int last;
			GetFirstLastRowVisible(out first, out last);
			for (int i = first; i <= last; i++)
			{
				DrawNode(m_visibleNodes[i], i, viewRect.width);
			}
			GUI.EndScrollView();
			Event evt = Event.current;
			switch (evt.type)
			{
			case EventType.Used:
				m_hasFocus = true;
				break;
			case EventType.MouseDown:
				m_hasFocus = m_scrollViewRect.Contains(Event.current.mousePosition);
				if (m_hasFocus)
				{
					SelectedNodes.Clear();
					evt.Use();
				}
				break;
			}
		}

		private void EnsureVisible(List<Node> nodes)
		{
			if (nodes.Count != 0)
			{
				for (int i = 0; i < nodes.Count; i++)
				{
					for (Node parent = nodes[i].Parent; parent != m_root; parent = parent.Parent)
					{
						SetNodeExpanded(parent, true);
					}
				}
				RefreshVisibleNodes();
				int index = m_visibleNodes.IndexOf(nodes[0]);
				float top = (float)index * 16f;
				float bottom = top + 16f;
				if (bottom >= m_scrollPosition.y + m_scrollViewRect.height)
				{
					m_scrollPosition.y = bottom - m_scrollViewRect.height;
				}
				else if (top <= m_scrollPosition.y)
				{
					m_scrollPosition.y = (float)index * 16f;
				}
			}
		}

		private void DrawNode(Node node, int rowIndex, float maxWidth)
		{
			bool selected = m_selectedNodes.Contains(node);
			bool wasExpanded = node.Expanded;
			bool isExpanded = wasExpanded;
			m_content.text = node.Name;
			Rect rowRect = new Rect(0f, GetTopPixelForRow(rowIndex), maxWidth, 16f);
			if (!node.HasData)
			{
				Color color = GUI.color;
				GUI.color *= new Color(1f, 1f, 1f, 0.9f);
				GUI.Label(rowRect, GUIContent.none, m_noDataStyle);
				GUI.color = color;
			}
			Event evt = Event.current;
			if (evt.type == EventType.Repaint)
			{
				if (selected)
				{
					m_selectionStyle.Draw(rowRect, false, false, true, m_hasFocus);
				}
				float contentIndent = GetContentIndent(node);
				rowRect.x += contentIndent;
				rowRect.width -= contentIndent;
				GUIStyle style = node.IsBold ? m_boldLineStyle : (node.Enabled ? m_lineStyle : m_disabledLineStyle);
				style.padding.left = 2;
				if ((bool)node.Icon)
				{
					style.padding.left += 16;
				}
				style.Draw(rowRect, node.Name, false, false, selected, m_hasFocus);
				Texture2D icon = node.Icon;
				if ((bool)icon)
				{
					Rect pos = rowRect;
					pos.width = 16f;
					pos.height = 16f;
					GUI.DrawTexture(pos, icon);
				}
			}
			if (node.Children.Count > 0 && string.IsNullOrEmpty(Filter))
			{
				float foldoutIndent = GetFoldoutIndent(node);
				bool expanded = GUI.Toggle(new Rect(foldoutIndent, rowRect.y, 12f, rowRect.height), node.Expanded, GUIContent.none, m_foldoutStyle);
				if (expanded != node.Expanded)
				{
					SetNodeExpanded(node, expanded);
				}
			}
			bool isMouseDown = evt.isMouse && evt.type == EventType.MouseDown && evt.button == 0;
			if ((rowRect.Contains(evt.mousePosition) & isMouseDown) && isExpanded == wasExpanded)
			{
				if ((evt.modifiers & EventModifiers.Control) != 0)
				{
					if (SelectedNodes.Contains(node))
					{
						SelectedNodes.Remove(node);
					}
					else
					{
						SelectedNodes.Add(node);
					}
				}
				else if ((evt.modifiers & EventModifiers.Shift) != 0)
				{
					int nodeIndex = m_visibleNodes.IndexOf(node);
					int maxIndex = -1;
					int maxDelta = -1;
					for (int j = 0; j < SelectedNodes.Count; j++)
					{
						Node i = ((ObservableList<Node>)SelectedNodes)[j];
						int index = m_visibleNodes.IndexOf(i);
						int delta = Mathf.Abs(nodeIndex - index);
						if (delta > maxDelta)
						{
							maxDelta = delta;
							maxIndex = index;
						}
					}
					if (maxIndex != -1)
					{
						int start = maxIndex;
						int end = nodeIndex;
						if (maxIndex > nodeIndex)
						{
							start = nodeIndex;
							end = maxIndex;
						}
						List<Node> newSelection = m_visibleNodes.GetRange(start, end - start + 1);
						SelectedNodes.ReplaceAll(newSelection);
					}
				}
				else
				{
					SelectedNodes.ReplaceAll(node);
				}
				evt.Use();
			}
		}

		private void InitStyles()
		{
			if (m_foldoutStyle != null && m_lineStyle != null && m_boldLineStyle != null && m_selectionStyle != null)
			{
				return;
			}
			m_selectionStyle = new GUIStyle("PR Label");
			m_lineStyle = new GUIStyle("PR Label");
			Texture2D background = m_lineStyle.hover.background;
			m_lineStyle.onNormal.background = background;
			m_lineStyle.onActive.background = background;
			m_lineStyle.onFocused.background = background;
			m_lineStyle.alignment = TextAnchor.MiddleLeft;
			m_boldLineStyle = new GUIStyle(m_lineStyle);
			m_boldLineStyle.font = EditorStyles.boldLabel.font;
			m_boldLineStyle.fontStyle = EditorStyles.boldLabel.fontStyle;
			m_disabledLineStyle = new GUIStyle("PR DisabledLabel");
			m_disabledLineStyle.alignment = TextAnchor.MiddleLeft;
			m_foldoutStyle = new GUIStyle("IN Foldout");
			m_noDataStyle = new GUIStyle("ProjectBrowserTopBarBg");
		}

		private void ProcessKeyboardInput()
		{
			Event evt = Event.current;
			if (evt.isKey && m_selectedNodes.Count != 0 && evt.type == EventType.KeyDown && GUIUtility.keyboardControl == 0)
			{
				int selectedIndex = m_visibleNodes.IndexOf(((ObservableList<Node>)m_selectedNodes)[m_selectedNodes.Count - 1]);
				if (selectedIndex == -1 || m_visibleNodes.Count == 0)
				{
					Node node = (m_visibleNodes.Count > 0) ? m_visibleNodes[0] : null;
					SelectedNodes.ReplaceAll(node);
					EnsureVisible(SelectedNodes);
				}
				else
				{
					bool ensureVisible = true;
					switch (evt.keyCode)
					{
					case KeyCode.UpArrow:
						if (selectedIndex > 0)
						{
							SelectedNodes.ReplaceAll(m_visibleNodes[selectedIndex - 1]);
						}
						evt.Use();
						break;
					case KeyCode.DownArrow:
						if (selectedIndex < m_visibleNodes.Count - 1)
						{
							SelectedNodes.ReplaceAll(m_visibleNodes[selectedIndex + 1]);
						}
						evt.Use();
						break;
					case KeyCode.LeftArrow:
						evt.Use();
						ProcessLeftArrow(m_visibleNodes);
						break;
					case KeyCode.RightArrow:
						evt.Use();
						ProcessRightArrow(m_visibleNodes);
						break;
					case KeyCode.Home:
						SelectedNodes.ReplaceAll(m_visibleNodes[0]);
						evt.Use();
						break;
					case KeyCode.End:
						SelectedNodes.ReplaceAll(m_visibleNodes[m_visibleNodes.Count - 1]);
						evt.Use();
						break;
					case KeyCode.PageUp:
					{
						int numRows = (int)(m_scrollViewRect.height / 16f);
						int index = Mathf.Max(selectedIndex - numRows, 0);
						SelectedNodes.ReplaceAll(m_visibleNodes[index]);
						evt.Use();
						break;
					}
					case KeyCode.PageDown:
					{
						int numRows2 = (int)(m_scrollViewRect.height / 16f);
						int index2 = Mathf.Min(selectedIndex + numRows2, m_visibleNodes.Count - 1);
						SelectedNodes.ReplaceAll(m_visibleNodes[index2]);
						evt.Use();
						break;
					}
					case KeyCode.Delete:
						if (this.SelectedNodesDeleted != null)
						{
							this.SelectedNodesDeleted();
						}
						evt.Use();
						break;
					default:
						ensureVisible = false;
						break;
					}
					if (ensureVisible)
					{
						EnsureVisible(SelectedNodes);
					}
				}
			}
		}

		private void ProcessLeftArrow(List<Node> flatList)
		{
			if (string.IsNullOrEmpty(Filter))
			{
				if (m_selectedNodes.Count == 1)
				{
					Node node = ((ObservableList<Node>)m_selectedNodes)[0];
					if (node.Expanded)
					{
						SetNodeExpanded(node, false);
					}
					else if (node.Parent != m_root)
					{
						SelectedNodes.ReplaceAll(node.Parent);
					}
					else
					{
						int k = flatList.IndexOf(node) - 1;
						Node j;
						while (true)
						{
							if (k >= 0)
							{
								j = flatList[k];
								if (j.Children.Count <= 0)
								{
									k--;
									continue;
								}
								break;
							}
							return;
						}
						SelectedNodes.ReplaceAll(j);
					}
				}
				else
				{
					for (int i = 0; i < m_selectedNodes.Count; i++)
					{
						SetNodeExpanded(((ObservableList<Node>)m_selectedNodes)[i], false);
					}
				}
			}
		}

		private void ProcessRightArrow(List<Node> flatList)
		{
			if (string.IsNullOrEmpty(Filter))
			{
				if (m_selectedNodes.Count == 1)
				{
					Node node = ((ObservableList<Node>)m_selectedNodes)[0];
					if (node.Children.Count == 0 || node.Expanded)
					{
						int k = flatList.IndexOf(node) + 1;
						Node j;
						while (true)
						{
							if (k < flatList.Count)
							{
								j = flatList[k];
								if (j.Children.Count <= 0)
								{
									k++;
									continue;
								}
								break;
							}
							return;
						}
						SelectedNodes.ReplaceAll(j);
					}
					else if (node.Children.Count > 0)
					{
						SetNodeExpanded(node, true);
					}
				}
				else
				{
					for (int i = 0; i < m_selectedNodes.Count; i++)
					{
						SetNodeExpanded(((ObservableList<Node>)m_selectedNodes)[i], true);
					}
				}
			}
		}

		private List<Node> BuildFlatList(List<Node> nodes, bool allNodes = false)
		{
			List<Node> flatList = new List<Node>();
			for (int j = 0; j < nodes.Count; j++)
			{
				Node i = nodes[j];
				bool hasFilter = !string.IsNullOrEmpty(Filter);
				if (allNodes || !hasFilter || i.Name.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					flatList.Add(i);
				}
				if ((allNodes || i.Expanded) | hasFilter)
				{
					flatList.AddRange(BuildFlatList(i.Children, false));
				}
			}
			return flatList;
		}

		private void RefreshVisibleNodes()
		{
			if (!m_visibleNodesDirty)
			{
				if (m_visibleNodes == null)
				{
					m_visibleNodes = new List<Node>();
				}
			}
			else
			{
				m_visibleNodesDirty = false;
				m_visibleNodes = BuildFlatList(m_root.Children, false);
			}
		}

		private float GetFoldoutIndent(Node node)
		{
			if (!string.IsNullOrEmpty(Filter))
			{
				return 2f;
			}
			return 2f + (float)node.Depth * 13f;
		}

		private float GetContentIndent(Node node)
		{
			return GetFoldoutIndent(node) + 12f;
		}

		private Vector2 GetTotalSize()
		{
			return new Vector2(1f, (float)m_visibleNodes.Count * 16f + 2f);
		}

		private void GetFirstLastRowVisible(out int first, out int last)
		{
			first = Mathf.Max(Mathf.FloorToInt(m_scrollPosition.y / 16f), 0);
			last = first + Mathf.CeilToInt(m_scrollViewRect.height / 16f);
			last = Mathf.Min(last, m_visibleNodes.Count - 1);
		}

		private float GetTopPixelForRow(int row)
		{
			return (float)row * 16f;
		}

		private GUIContent GetContent(string text)
		{
			m_tempContent.text = text;
			m_tempContent.tooltip = string.Empty;
			return m_tempContent;
		}

		private void SetNodeExpanded(Node node, bool expanded)
		{
			m_visibleNodesDirty |= (node.Expanded != expanded);
			node.Expanded = expanded;
		}
	}
}
