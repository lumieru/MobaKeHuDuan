using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Hdg
{
	public class rdtGuiProperty
	{
		public class ValueChangedEvent
		{
			public rdtTcpMessageComponents.Component Component
			{
				get;
				set;
			}

			public Stack<rdtTcpMessageComponents.Property> Hierarchy
			{
				get;
				set;
			}

			public object OldValue
			{
				get;
				set;
			}

			public object NewValue
			{
				get;
				set;
			}

			public bool UpdateProperty
			{
				get;
				set;
			}

			public int ArrayIndex
			{
				get;
				set;
			}

			public int NewArraySize
			{
				get;
				set;
			}

			public ValueChangedEvent()
			{
				NewArraySize = -1;
				ArrayIndex = -1;
			}

			public ValueChangedEvent(int arrayIndex)
			{
				ArrayIndex = arrayIndex;
				NewArraySize = -1;
			}

			public ValueChangedEvent(object oldValue, object newValue, bool updateProperty, int arrayIndex)
			{
				OldValue = oldValue;
				NewValue = newValue;
				UpdateProperty = updateProperty;
				ArrayIndex = arrayIndex;
				NewArraySize = -1;
			}
		}

		public delegate void ValueChangedHandler(ValueChangedEvent valueChangedEvent);

		public delegate void ComponentValueChangedHandler(ValueChangedEvent valueChangedEvent);

		private rdtExpandedCache m_expandedCache;

		private ComponentValueChangedHandler m_componentValueChangedHandler;

		private Stack<rdtTcpMessageComponents.Property> m_currentHierarchy;

		private rdtTcpMessageComponents.Component m_currentComponent;

		private void DrawList(string label, IList list, ref bool foldout, ValueChangedHandler onValueChanged, string foldoutKey)
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				int oldSize = (list != null) ? list.Count : 0;
				Type elementType = (list != null) ? list.GetType().GetListElementType() : null;
				bool isUserStruct = elementType == typeof(List<rdtTcpMessageComponents.Property>) || elementType == null;
				List<rdtTcpMessageComponents.Property> subProperties = list as List<rdtTcpMessageComponents.Property>;
				if (subProperties != null)
				{
					DrawComponent(subProperties, foldoutKey, onValueChanged);
				}
				else
				{
					EditorGUILayout.BeginHorizontal();
					Space();
					int newSize = EditorGUILayout.IntField("Size", oldSize);
					EditorGUILayout.EndHorizontal();
					if (oldSize != newSize)
					{
						ValueChangedEvent evt = new ValueChangedEvent
						{
							NewArraySize = newSize
						};
						onValueChanged(evt);
					}
					int i;
					for (i = 0; i < oldSize; i++)
					{
						ValueChangedHandler handler = delegate(ValueChangedEvent valueChangedEvent)
						{
							if (isUserStruct)
							{
								valueChangedEvent.UpdateProperty = true;
								valueChangedEvent.ArrayIndex = i;
								onValueChanged(valueChangedEvent);
							}
							else
							{
								list[i] = valueChangedEvent.NewValue;
								valueChangedEvent.OldValue = null;
								valueChangedEvent.NewValue = null;
								valueChangedEvent.UpdateProperty = false;
								onValueChanged(valueChangedEvent);
							}
						};
						Draw("Element " + i, list[i], handler, foldoutKey + ">Element" + i, false);
					}
				}
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
		}

		private Matrix4x4 DrawMatrix(string label, Matrix4x4 matrix, ref bool foldout)
		{
			Matrix4x4 i = matrix;
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				i.m00 = DrawFloat("E00", i.m00);
				i.m01 = DrawFloat("E01", i.m01);
				i.m02 = DrawFloat("E02", i.m02);
				i.m03 = DrawFloat("E03", i.m03);
				i.m10 = DrawFloat("E10", i.m10);
				i.m11 = DrawFloat("E11", i.m11);
				i.m12 = DrawFloat("E12", i.m12);
				i.m13 = DrawFloat("E13", i.m13);
				i.m20 = DrawFloat("E20", i.m20);
				i.m21 = DrawFloat("E21", i.m21);
				i.m22 = DrawFloat("E22", i.m22);
				i.m23 = DrawFloat("E23", i.m23);
				i.m30 = DrawFloat("E30", i.m30);
				i.m31 = DrawFloat("E31", i.m31);
				i.m32 = DrawFloat("E32", i.m32);
				i.m33 = DrawFloat("E33", i.m33);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			return i;
		}

		public rdtGuiProperty(ComponentValueChangedHandler componentValueChangedHandler)
		{
			m_expandedCache = new rdtExpandedCache();
			m_componentValueChangedHandler = componentValueChangedHandler;
		}

		public void DrawComponent(int gameObjInstanceId, rdtTcpMessageComponents.Component component, List<rdtTcpMessageComponents.Property> properties)
		{
			m_currentHierarchy = new Stack<rdtTcpMessageComponents.Property>();
			m_currentComponent = component;
			string foldoutKeyRoot = string.Format("{0}>{1}({2})", gameObjInstanceId.ToString(), component.m_name, component.m_instanceId);
			DrawComponent(properties, foldoutKeyRoot, null);
		}

		private void DrawComponent(List<rdtTcpMessageComponents.Property> properties, string foldoutKeyInit = "", ValueChangedHandler valueChangedHandler = null)
		{
			if (properties != null && properties.Count != 0)
			{
				foreach (rdtTcpMessageComponents.Property property in properties)
				{
					object propValue = property.m_value;
					if (propValue != null || property.m_isArray)
					{
						EditorGUILayout.BeginHorizontal();
						string name = ObjectNames.NicifyVariableName(property.m_name);
						Type obj = (propValue != null) ? propValue.GetType() : null;
						m_currentHierarchy.Push(property);
						string foldoutKey = property.m_name;
						if (!string.IsNullOrEmpty(foldoutKeyInit))
						{
							foldoutKey = foldoutKeyInit + ">" + foldoutKey;
						}
						if (obj == typeof(List<rdtTcpMessageComponents.Property>))
						{
							EditorGUILayout.BeginVertical();
							GUILayout.Label("");
							Rect lastRect = GUILayoutUtility.GetLastRect();
							bool foldout2 = m_expandedCache.IsExpanded(m_currentComponent, foldoutKey);
							foldout2 = EditorGUI.Foldout(lastRect, foldout2, name, true);
							if (foldout2)
							{
								EditorGUI.indentLevel++;
								List<rdtTcpMessageComponents.Property> subProperties = (List<rdtTcpMessageComponents.Property>)propValue;
								DrawComponent(subProperties, foldoutKey, valueChangedHandler);
								EditorGUI.indentLevel--;
							}
							m_expandedCache.SetExpanded(foldout2, m_currentComponent, foldoutKey);
							EditorGUILayout.EndVertical();
						}
						else
						{
							ValueChangedHandler onValueChanged = valueChangedHandler;
							if (onValueChanged == null)
							{
								onValueChanged = delegate(ValueChangedEvent valueChangedEvent)
								{
									valueChangedEvent.Component = m_currentComponent;
									valueChangedEvent.Hierarchy = m_currentHierarchy;
									m_componentValueChangedHandler(valueChangedEvent);
								};
							}
							Draw(name, propValue, onValueChanged, foldoutKey, property.m_isArray);
						}
						m_currentHierarchy.Pop();
						EditorGUILayout.EndHorizontal();
					}
				}
			}
		}

		private void Draw(string propName, object propValue, ValueChangedHandler onValueChanged, string foldoutKey, bool isArray = false)
		{
			bool foldout = m_expandedCache.IsExpanded(m_currentComponent, foldoutKey);
			if (propValue == null && !isArray)
			{
				return;
			}
			Type type = (propValue != null) ? propValue.GetType() : null;
			bool hasSpace = false;
			if (type != null && !type.IsArray && !type.IsGenericList() && type != typeof(Vector4) && type != typeof(Matrix4x4) && type != typeof(Quaternion))
			{
				hasSpace = true;
				EditorGUILayout.BeginHorizontal();
				Space();
			}
			if (type == typeof(float))
			{
				float oldValue = (float)propValue;
				float newValue18 = EditorGUILayout.FloatField(propName, oldValue);
				if (!oldValue.Equals(newValue18))
				{
					ValueChangedEvent evt19 = new ValueChangedEvent(oldValue, newValue18, true, -1);
					onValueChanged(evt19);
				}
			}
			else if (type == typeof(double))
			{
				double oldValue2 = (double)propValue;
				double newValue17 = EditorGUILayout.DoubleField(propName, oldValue2);
				if (!oldValue2.Equals(newValue17))
				{
					ValueChangedEvent evt18 = new ValueChangedEvent(oldValue2, newValue17, true, -1);
					onValueChanged(evt18);
				}
			}
			else if (type == typeof(int))
			{
				int oldValue3 = (int)propValue;
				int newValue16 = EditorGUILayout.IntField(propName, oldValue3);
				if (!oldValue3.Equals(newValue16))
				{
					ValueChangedEvent evt17 = new ValueChangedEvent(oldValue3, newValue16, true, -1);
					onValueChanged(evt17);
				}
			}
			else if (type == typeof(uint))
			{
				int oldValue4 = (int)(uint)propValue;
				int newValue15 = EditorGUILayout.IntField(propName, oldValue4);
				if (!oldValue4.Equals(newValue15))
				{
					ValueChangedEvent evt16 = new ValueChangedEvent(oldValue4, (uint)newValue15, true, -1);
					onValueChanged(evt16);
				}
			}
			else if (type == typeof(Vector2))
			{
				Vector2 oldValue5 = (Vector2)propValue;
				Vector2 newValue14 = EditorGUILayout.Vector2Field(propName, oldValue5);
				if (!oldValue5.Equals(newValue14))
				{
					ValueChangedEvent evt15 = new ValueChangedEvent(oldValue5, newValue14, true, -1);
					onValueChanged(evt15);
				}
			}
			else if (type == typeof(Vector3))
			{
				Vector3 oldValue17 = (Vector3)propValue;
				Vector3 newValue13 = EditorGUILayout.Vector3Field(propName, oldValue17);
				if (oldValue17 != newValue13)
				{
					ValueChangedEvent evt14 = new ValueChangedEvent(oldValue17, newValue13, true, -1);
					onValueChanged(evt14);
				}
			}
			else if (type == typeof(Vector4))
			{
				Vector4 oldValue6 = (Vector4)propValue;
				Vector4 newValue12 = DrawVector4(propName, oldValue6, ref foldout);
				if (!oldValue6.Equals(newValue12))
				{
					ValueChangedEvent evt13 = new ValueChangedEvent(oldValue6, newValue12, true, -1);
					onValueChanged(evt13);
				}
			}
			else if (type == typeof(Matrix4x4))
			{
				Matrix4x4 oldValue7 = (Matrix4x4)propValue;
				Matrix4x4 newValue11 = DrawMatrix(propName, oldValue7, ref foldout);
				if (!oldValue7.Equals(newValue11))
				{
					ValueChangedEvent evt12 = new ValueChangedEvent(oldValue7, newValue11, true, -1);
					onValueChanged(evt12);
				}
			}
			else if (type == typeof(bool))
			{
				bool oldValue8 = (bool)propValue;
				bool newValue10 = EditorGUILayout.Toggle(propName, oldValue8);
				if (!oldValue8.Equals(newValue10))
				{
					ValueChangedEvent evt11 = new ValueChangedEvent(oldValue8, newValue10, true, -1);
					onValueChanged(evt11);
				}
			}
			else if (type != null && type.IsEnum)
			{
				Enum oldValue12 = (Enum)propValue;
				Enum newValue9 = EditorGUILayout.EnumPopup(propName, oldValue12);
				if (!oldValue12.Equals(newValue9))
				{
					ValueChangedEvent evt10 = new ValueChangedEvent(oldValue12, newValue9, true, -1);
					onValueChanged(evt10);
				}
			}
			else if (type == typeof(char))
			{
				string oldValue10 = new string((char)propValue, 1);
				string newValue8 = EditorGUILayout.TextField(propName, oldValue10);
				if (!oldValue10.Equals(newValue8))
				{
					ValueChangedEvent evt9 = new ValueChangedEvent(oldValue10, (newValue8.Length > 0) ? newValue8[0] : ((char)propValue), true, -1);
					onValueChanged(evt9);
				}
			}
			else if (type == typeof(string))
			{
				string oldValue9 = (string)propValue;
				string newValue7 = EditorGUILayout.TextField(propName, oldValue9);
				if (!oldValue9.Equals(newValue7))
				{
					ValueChangedEvent evt8 = new ValueChangedEvent(oldValue9, newValue7, true, -1);
					onValueChanged(evt8);
				}
			}
			else if (type == typeof(Color))
			{
				Color oldValue11 = (Color)propValue;
				Color newValue6 = EditorGUILayout.ColorField(propName, oldValue11);
				if (!oldValue11.Equals(newValue6))
				{
					ValueChangedEvent evt7 = new ValueChangedEvent(oldValue11, newValue6, true, -1);
					onValueChanged(evt7);
				}
			}
			else if (type == typeof(Color32))
			{
				Color32 oldValue13 = (Color32)propValue;
				Color32 newValue5 = EditorGUILayout.ColorField(propName, oldValue13);
				if (!oldValue13.Equals(newValue5))
				{
					ValueChangedEvent evt6 = new ValueChangedEvent(oldValue13, newValue5, true, -1);
					onValueChanged(evt6);
				}
			}
			else if (type == typeof(Quaternion))
			{
				Quaternion oldValue14 = (Quaternion)propValue;
				Vector4 v2 = new Vector4(oldValue14.x, oldValue14.y, oldValue14.z, oldValue14.w);
				v2 = DrawVector4(propName, v2, ref foldout);
				Quaternion newValue4 = new Quaternion(v2.x, v2.y, v2.z, v2.w);
				if (!oldValue14.Equals(newValue4))
				{
					ValueChangedEvent evt5 = new ValueChangedEvent(oldValue14, newValue4, true, -1);
					onValueChanged(evt5);
				}
			}
			else if (type == typeof(Bounds))
			{
				Bounds oldValue15 = (Bounds)propValue;
				Bounds newValue3 = EditorGUILayout.BoundsField(propName, oldValue15);
				if (!oldValue15.Equals(newValue3))
				{
					ValueChangedEvent evt4 = new ValueChangedEvent(oldValue15, newValue3, true, -1);
					onValueChanged(evt4);
				}
			}
			else if (type == typeof(Rect))
			{
				Rect oldValue16 = (Rect)propValue;
				Rect newValue2 = EditorGUILayout.RectField(propName, oldValue16);
				if (!oldValue16.Equals(newValue2))
				{
					ValueChangedEvent evt3 = new ValueChangedEvent(oldValue16, newValue2, true, -1);
					onValueChanged(evt3);
				}
			}
			else if ((type == null & isArray) || type.IsArray)
			{
				Array a = (Array)propValue;
				DrawList(propName, a, ref foldout, onValueChanged, foldoutKey);
			}
			else if (type != null && type.IsGenericList())
			{
				IList list = (IList)propValue;
				DrawList(propName, list, ref foldout, onValueChanged, foldoutKey);
			}
			else if (type == typeof(rdtSerializerButton))
			{
				Vector2 size = GUI.skin.button.CalcSize(new GUIContent(propName));
				if (GUILayout.Button(propName, GUILayout.Width(size.x + 40f)))
				{
					rdtSerializerButton button = new rdtSerializerButton(true);
					ValueChangedEvent evt2 = new ValueChangedEvent(false, button, true, -1);
					onValueChanged(evt2);
				}
			}
			else if (type == typeof(rdtSerializerSlider))
			{
				rdtSerializerSlider slider = (rdtSerializerSlider)propValue;
				float newValue = EditorGUILayout.Slider(propName, slider.Value, slider.LimitMin, slider.LimitMax);
				if (!slider.Value.Equals(newValue))
				{
					rdtSerializerSlider newSlider = new rdtSerializerSlider(newValue, slider.LimitMin, slider.LimitMax);
					ValueChangedEvent evt = new ValueChangedEvent(slider, newSlider, true, -1);
					onValueChanged(evt);
				}
			}
			else
			{
				string typeName = (type != null) ? type.Name : "<null>";
				rdtDebug.Debug("rdtGuiProperty: Unknown type: " + typeName + " (name=" + propName + ", value=" + propValue + ")");
			}
			if (hasSpace)
			{
				EditorGUILayout.EndHorizontal();
			}
			m_expandedCache.SetExpanded(foldout, m_currentComponent, foldoutKey);
		}

		private void Space()
		{
			GUILayout.Space((float)(EditorStyles.foldout.padding.left + EditorStyles.foldout.margin.left - EditorStyles.label.padding.left));
		}

		private float DrawFloat(string label, float value)
		{
			EditorGUILayout.BeginHorizontal();
			Space();
			value = EditorGUILayout.FloatField(label, value);
			EditorGUILayout.EndHorizontal();
			return value;
		}

		private Vector4 DrawVector4(string label, Vector4 value, ref bool foldout)
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label("");
			Rect rect = GUILayoutUtility.GetLastRect();
			foldout = EditorGUI.Foldout(rect, foldout, label, true);
			if (foldout)
			{
				EditorGUI.indentLevel++;
				value.x = DrawFloat("X", value.x);
				value.y = DrawFloat("Y", value.y);
				value.z = DrawFloat("Z", value.z);
				value.w = DrawFloat("W", value.w);
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.EndVertical();
			return value;
		}
	}
}
