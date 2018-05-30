using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hdg
{
	public class rdtSerializerRegistry
	{
		private delegate object ConvertObjectDelegate(object objIn, rdtSerializerRegistry registry);

		private Dictionary<Type, ConvertObjectDelegate> m_converters = new Dictionary<Type, ConvertObjectDelegate>();

		private HashSet<Type> m_failures = new HashSet<Type>();

		private HashSet<Type> m_referenceFailures = new HashSet<Type>();

		private HashSet<Type> m_unknownPrimitives = new HashSet<Type>();

		private HashSet<string> m_skipProperties = new HashSet<string>();

		private HashSet<string> m_skipTypes = new HashSet<string>();

		private Dictionary<string, HashSet<string>> m_skipPropertiesPerType = new Dictionary<string, HashSet<string>>();

		private Dictionary<string, HashSet<string>> m_includePropertiesPerType = new Dictionary<string, HashSet<string>>();

		private HashSet<string> m_dontReadProperties = new HashSet<string>();

		private object NotHandledConversion(object objIn, rdtSerializerRegistry r)
		{
			return null;
		}

		public rdtSerializerRegistry()
		{
			m_converters.Add(typeof(Vector2), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector2((Vector2)objIn));
			m_converters.Add(typeof(Vector3), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector3((Vector3)objIn));
			m_converters.Add(typeof(Vector4), (object objIn, rdtSerializerRegistry r) => new rdtSerializerVector4((Vector4)objIn));
			m_converters.Add(typeof(Quaternion), (object objIn, rdtSerializerRegistry r) => new rdtSerializerQuaternion((Quaternion)objIn));
			m_converters.Add(typeof(Color), (object objIn, rdtSerializerRegistry r) => new rdtSerializerColor((Color)objIn));
			m_converters.Add(typeof(Color32), (object objIn, rdtSerializerRegistry r) => new rdtSerializerColor32((Color32)objIn));
			m_converters.Add(typeof(Rect), (object objIn, rdtSerializerRegistry r) => new rdtSerializerRect((Rect)objIn));
			m_converters.Add(typeof(Bounds), (object objIn, rdtSerializerRegistry r) => new rdtSerializerBounds((Bounds)objIn));
			m_converters.Add(typeof(Matrix4x4), (object objIn, rdtSerializerRegistry r) => new rdtSerializerMatrix4x4((Matrix4x4)objIn));
			m_converters.Add(typeof(List<>), rdtSerializerContainerArray.Serialize);
			m_converters.Add(typeof(Dictionary<, >), NotHandledConversion);
			m_converters.Add(typeof(Array), rdtSerializerContainerArray.Serialize);
			InitSkipProperties();
		}

		public void AddUnknownPrimitive(Type type)
		{
			if (!m_unknownPrimitives.Contains(type))
			{
				rdtDebug.Warning("Remote Debug: Tried to serialise an unknown primitive type '{0}' ({1})", type, type.FullName);
				m_unknownPrimitives.Add(type);
			}
		}

		public object Serialize(object obj)
		{
			if (obj != null && !obj.Equals(null))
			{
				object serializedObj = obj;
				Type typeToConvert = obj.GetType();
				if (typeof(rdtSerializerInterface).IsAssignableFrom(typeToConvert))
				{
					return serializedObj;
				}
				if (typeToConvert.IsArray)
				{
					typeToConvert = typeof(Array);
				}
				else if (typeToConvert.IsGenericType)
				{
					typeToConvert = typeToConvert.GetGenericTypeDefinition();
				}
				ConvertObjectDelegate converter;
				if (m_converters.TryGetValue(typeToConvert, out converter))
				{
					serializedObj = converter(obj, this);
				}
				else if (typeToConvert.IsUserStruct() || typeToConvert.IsReference())
				{
					serializedObj = ReadAllFields(obj);
				}
				if (serializedObj != null)
				{
					Type serialisedObjType = serializedObj.GetType();
					if (!serialisedObjType.IsSerializable && !typeof(rdtSerializerInterface).IsAssignableFrom(serialisedObjType))
					{
						if (!m_failures.Contains(serialisedObjType))
						{
							rdtDebug.Warning("Remote Debug: Object '{0}' (type {1}) is not serializable!", serializedObj, serialisedObjType.Name);
							m_failures.Add(serialisedObjType);
						}
						return null;
					}
				}
				return serializedObj;
			}
			return null;
		}

		public object Deserialize(object obj)
		{
			if (obj != null && !obj.Equals(null))
			{
				object deserializedObj = obj;
				rdtSerializerInterface serializer = obj as rdtSerializerInterface;
				if (serializer != null)
				{
					deserializedObj = serializer.Deserialize(this);
				}
				List<rdtTcpMessageComponents.Property> subProperties = deserializedObj as List<rdtTcpMessageComponents.Property>;
				if (subProperties != null)
				{
					for (int i = 0; i < subProperties.Count; i++)
					{
						rdtTcpMessageComponents.Property p = subProperties[i];
						p.Deserialise(this);
						subProperties[i] = p;
					}
				}
				return deserializedObj;
			}
			return null;
		}

		private void AddField(List<rdtTcpMessageComponents.Property> allFields, string name, object value, rdtTcpMessageComponents.Property.Type type, RangeAttribute rangeAttribute, bool isArrayOrList)
		{
			object serializedValue;
			if (rangeAttribute != null && value is float)
			{
				serializedValue = new rdtSerializerSlider((float)value, rangeAttribute.min, rangeAttribute.max);
			}
			else
			{
				serializedValue = Serialize(value);
				if (value != null)
				{
					if (serializedValue == null)
					{
						return;
					}
					if (serializedValue.Equals(null))
					{
						return;
					}
				}
			}
			rdtTcpMessageComponents.Property property = default(rdtTcpMessageComponents.Property);
			property.m_isArray = (serializedValue is rdtSerializerContainerArray | isArrayOrList);
			property.m_name = name;
			property.m_value = serializedValue;
			property.m_type = type;
			allFields.Add(property);
		}

		private bool CanAddMember(object owner, MemberInfo memberInfo, Type memberType)
		{
			bool num = memberInfo.IsDefined(typeof(ObsoleteAttribute), false);
			bool hide = memberInfo.IsDefined(typeof(HideInInspector), false);
			bool isMonoBehaviour = memberType.IsSubclassOf(typeof(Component));
			if (num | hide | isMonoBehaviour)
			{
				return false;
			}
			return true;
		}

		public List<rdtTcpMessageComponents.Property> ReadAllFields(object owner)
		{
			Type ownerType = owner.GetType();
			string ownerTypeName = ownerType.Name;
			if (m_skipTypes.Contains(ownerTypeName))
			{
				return null;
			}
			List<rdtTcpMessageComponents.Property> allFields = new List<rdtTcpMessageComponents.Property>();
			PropertyInfo[] props = ownerType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			if (!(owner is MonoBehaviour) && !m_dontReadProperties.Contains(ownerTypeName))
			{
				foreach (PropertyInfo p in props)
				{
					string pName;
					RangeAttribute range2;
					object value2;
					bool isArrayOrList2;
					if (p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0 && !p.PropertyType.IsEnum)
					{
						pName = p.Name;
						bool checkSkip2 = !HasIncludePerType(ownerTypeName);
						if ((checkSkip2 || IncludeMember(ownerTypeName, pName)) && (!checkSkip2 || !SkipMember(ownerTypeName, pName)))
						{
							MethodInfo getMethod = p.GetGetMethod();
							if (getMethod != null && getMethod.IsPublic)
							{
								MethodInfo setMethod = p.GetSetMethod();
								if (setMethod != null && setMethod.IsPublic && CanAddMember(owner, p, p.PropertyType))
								{
									range2 = null;
									value2 = p.GetValue(owner, null);
									Type propType = p.PropertyType;
									isArrayOrList2 = (propType.IsGenericList() || propType.IsArray);
									if (isArrayOrList2)
									{
										Type elementType2 = propType.GetListElementType();
										if (!elementType2.IsSubclassOf(typeof(Component)) && (elementType2.IsSerializable || elementType2.IsUserStruct()))
										{
											goto IL_0171;
										}
										continue;
									}
									goto IL_0171;
								}
							}
						}
					}
					continue;
					IL_0171:
					AddField(allFields, pName, value2, rdtTcpMessageComponents.Property.Type.Property, range2, isArrayOrList2);
				}
			}
			FieldInfo[] fields = ownerType.GetAllFields().ToArray();
			foreach (FieldInfo f in fields)
			{
				bool serialize = f.IsDefined(typeof(SerializeField), false);
				RangeAttribute range = null;
				string fName;
				object value;
				bool isArrayOrList;
				if ((f.IsPublic || serialize) && !f.FieldType.IsEnum)
				{
					fName = f.Name;
					bool checkSkip = !HasIncludePerType(ownerTypeName);
					if ((checkSkip || IncludeMember(ownerTypeName, fName)) && (!checkSkip || !SkipMember(ownerTypeName, fName)) && CanAddMember(owner, f, f.FieldType))
					{
						value = f.GetValue(owner);
						Type fieldType = f.FieldType;
						isArrayOrList = (fieldType.IsGenericList() || fieldType.IsArray);
						if (isArrayOrList)
						{
							Type elementType = fieldType.GetListElementType();
							if (!elementType.IsSubclassOf(typeof(Component)) && (elementType.IsSerializable || elementType.IsUserStruct()))
							{
								goto IL_0287;
							}
							continue;
						}
						goto IL_0287;
					}
				}
				continue;
				IL_0287:
				AddField(allFields, fName, value, rdtTcpMessageComponents.Property.Type.Field, range, isArrayOrList);
			}
			MethodInfo[] methods = ownerType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo i in methods)
			{
				if (i.IsDefined(typeof(ButtonAttribute), false))
				{
					rdtTcpMessageComponents.Property property = default(rdtTcpMessageComponents.Property);
					property.m_name = i.Name;
					property.m_value = new rdtSerializerButton(false);
					property.m_type = rdtTcpMessageComponents.Property.Type.Method;
					allFields.Add(property);
				}
			}
			return allFields;
		}

		private object MakeNewList(IList oldValue, Type listType, int arraySize)
		{
			object newValue = null;
			Type elementType = listType.GetListElementType();
			if (oldValue == null)
			{
				IList list;
				if (listType.IsArray)
				{
					list = Array.CreateInstance(elementType, arraySize);
				}
				else
				{
					list = (IList)typeof(List<>).MakeGenericType(elementType).GetConstructor(Type.EmptyTypes).Invoke(null);
					for (int m = 0; m < arraySize; m++)
					{
						list.Add(null);
					}
				}
				for (int l = 0; l < arraySize; l++)
				{
					object dummyValue3 = list[l] = Activator.CreateInstance(elementType);
				}
				return list;
			}
			if (listType.IsArray)
			{
				Array oldArray = oldValue as Array;
				Array newArray = Array.CreateInstance(elementType, arraySize);
				Array.Copy(oldArray, newArray, Mathf.Min(arraySize, oldArray.Length));
				for (int k = oldArray.Length; k < arraySize; k++)
				{
					object dummyValue2 = Activator.CreateInstance(elementType);
					newArray.SetValue(dummyValue2, k);
				}
				return newArray;
			}
			if (arraySize < oldValue.Count)
			{
				int diff2 = oldValue.Count - arraySize;
				for (int j = 0; j < diff2; j++)
				{
					oldValue.RemoveAt(oldValue.Count - 1);
				}
			}
			else if (arraySize > oldValue.Count)
			{
				int diff = arraySize - oldValue.Count;
				for (int i = 0; i < diff; i++)
				{
					object dummyValue = Activator.CreateInstance(elementType);
					oldValue.Add(dummyValue);
				}
			}
			return oldValue;
		}

		public void SetArraySize(object owner, List<rdtTcpMessageComponents.Property> allFields, int arraySize)
		{
			if (arraySize >= 0)
			{
				List<rdtTcpMessageComponents.Property> fields = allFields;
				rdtTcpMessageComponents.Property p = fields[0];
				Type type = owner.GetType();
				if (!p.m_isArray)
				{
					fields = (p.m_value as List<rdtTcpMessageComponents.Property>);
					if (fields == null)
					{
						rdtDebug.Error(this, "Expected to find a list of properties at {0}, but found {1} while trying to set array size", p.m_name, (p.m_value != null) ? p.m_value.GetType().Name : "<null>");
						return;
					}
				}
				if (p.m_type == rdtTcpMessageComponents.Property.Type.Property)
				{
					PropertyInfo prop = type.GetProperty(p.m_name);
					if (prop != null)
					{
						if (p.m_isArray)
						{
							Type propType = prop.PropertyType;
							IList oldValue2 = prop.GetValue(owner, null) as IList;
							object newValue2 = MakeNewList(oldValue2, propType, arraySize);
							prop.SetValue(owner, newValue2, null);
						}
						else
						{
							object child2 = prop.GetValue(owner, null);
							SetArraySize(child2, fields, arraySize);
							prop.SetValue(owner, child2, null);
						}
					}
				}
				else if (p.m_type == rdtTcpMessageComponents.Property.Type.Field)
				{
					FieldInfo field = type.GetField(p.m_name);
					if (field != null)
					{
						if (p.m_isArray)
						{
							Type fieldType = field.FieldType;
							IList oldValue = field.GetValue(owner) as IList;
							object newValue = MakeNewList(oldValue, fieldType, arraySize);
							field.SetValue(owner, newValue);
						}
						else
						{
							object child = field.GetValue(owner);
							SetArraySize(child, fields, arraySize);
							field.SetValue(owner, child);
						}
					}
				}
				else
				{
					rdtDebug.Error(this, "Unexpected property type {0} when setting array size on {1}", p.m_type.ToString(), p.m_name);
				}
			}
		}

		public void WriteAllFields(object realOwner, List<rdtTcpMessageComponents.Property> allFields, int arrayIndex = -1)
		{
			bool isArrayOrList = false;
			object owner = realOwner;
			Type ownerType2 = owner.GetType();
			if ((ownerType2.IsArray || ownerType2.IsGenericList()) && arrayIndex != -1)
			{
				isArrayOrList = true;
				owner = ((IList)owner)[arrayIndex];
			}
			rdtDebug.Debug(this, "WriteAllFields");
			ownerType2 = owner.GetType();
			for (int i = 0; i < allFields.Count; i++)
			{
				rdtTcpMessageComponents.Property property = allFields[i];
				object packedValue = property.m_value;
				object deserializedValue = Deserialize(packedValue);
				if (deserializedValue is rdtSerializerSlider)
				{
					deserializedValue = ((rdtSerializerSlider)deserializedValue).Value;
				}
				if (property.m_type == rdtTcpMessageComponents.Property.Type.Property)
				{
					PropertyInfo prop = ownerType2.GetProperty(property.m_name);
					if (prop != null)
					{
						List<rdtTcpMessageComponents.Property> subProperties2 = property.m_value as List<rdtTcpMessageComponents.Property>;
						if (subProperties2 != null)
						{
							object oldValue2 = prop.GetValue(owner, null);
							WriteAllFields(oldValue2, subProperties2, arrayIndex);
							prop.SetValue(owner, oldValue2, null);
						}
						else
						{
							try
							{
								rdtDebug.Debug(this, "Setting property {0} to {1}", property.m_name, deserializedValue.ToString());
								prop.SetValue(owner, deserializedValue, null);
							}
							catch (Exception ex2)
							{
								rdtDebug.Warning(this, "Property '{0}' could not be set: {1}!", property.m_name, ex2.Message);
							}
						}
					}
				}
				else if (property.m_type == rdtTcpMessageComponents.Property.Type.Field)
				{
					FieldInfo field = ownerType2.GetFieldInHierarchy(property.m_name);
					if (field != null)
					{
						List<rdtTcpMessageComponents.Property> subProperties = property.m_value as List<rdtTcpMessageComponents.Property>;
						if (subProperties != null)
						{
							object oldValue = field.GetValue(owner);
							WriteAllFields(oldValue, subProperties, arrayIndex);
							field.SetValue(owner, oldValue);
						}
						else
						{
							try
							{
								rdtDebug.Debug(this, "Setting field {0} to {1}", property.m_name, deserializedValue.ToString());
								field.SetValue(owner, deserializedValue);
							}
							catch (ArgumentException argException)
							{
								rdtDebug.Error(this, "'{0}' could not be assigned: {1}!", property.m_name, argException.Message);
							}
						}
					}
				}
				else
				{
					MethodInfo method = ownerType2.GetMethod(property.m_name);
					if (method != null)
					{
						try
						{
							if (((rdtSerializerButton)property.m_value).Pressed)
							{
								method.Invoke(owner, null);
							}
						}
						catch (Exception ex)
						{
							string msg = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
							string callstack = (ex.InnerException != null) ? ex.InnerException.StackTrace : ex.StackTrace;
							object[] args = new object[2]
							{
								property.m_name,
								msg
							};
							rdtDebug.Error("RemoteDebugServer: Method '{0}' failed: {1}", args);
							rdtDebug.Error(callstack);
						}
					}
				}
			}
			if (isArrayOrList)
			{
				((IList)realOwner)[arrayIndex] = owner;
			}
		}

		private void InitSkipProperties()
		{
			m_skipTypes.Add("ParticleSystemRenderer");
			m_skipProperties.Add("hideFlags");
			m_skipProperties.Add("useGUILayout");
			m_skipProperties.Add("tag");
			m_skipProperties.Add("name");
			m_skipProperties.Add("enabled");
			m_skipProperties.Add("m_CachedPtr");
			m_skipProperties.Add("m_InstanceID");
			AddSkipForType("Rigidbody2D", "position", "rotation", "freezeRotation");
			string[] skipForRigidBody3D = new string[4]
			{
				"position",
				"rotation",
				"freezeRotation",
				"useConeFriction"
			};
			AddSkipForType("Rigidbody", skipForRigidBody3D);
			string[] skipForCollider = new string[4]
			{
				"material",
				"sharedMaterial",
				"density",
				"sharedMesh"
			};
			AddSkipForType("BoxCollider", skipForCollider);
			AddSkipForType("BoxCollider2D", skipForCollider);
			AddSkipForType("CircleCollider2D", skipForCollider);
			AddSkipForType("SphereCollider", skipForCollider);
			AddSkipForType("PolygonCollider2D", skipForCollider);
			AddSkipForType("MeshCollider", skipForCollider);
			AddSkipForType("CapsuleCollider", skipForCollider);
			AddSkipForType("EdgeCollider2D", skipForCollider);
			AddSkipForType("WheelCollider", skipForCollider);
			AddSkipForType("TerrainCollider", skipForCollider);
			AddSkipForType("TerrainCollider", "isTrigger", "terrainData");
			AddSkipForType("CharacterController", skipForCollider);
			AddSkipForType("CharacterController", "isTrigger", "contactOffset");
			string[] skipForCloth = new string[5]
			{
				"capsuleColliders",
				"sphereColliders",
				"solverFrequency",
				"useContinuousCollision",
				"useVirtualParticles"
			};
			AddSkipForType("Cloth", skipForCloth);
			string[] skipForJoint2D = new string[3]
			{
				"breakForce",
				"breakTorque",
				"connectedBody"
			};
			AddSkipForType("HingeJoint2D", skipForJoint2D);
			AddSkipForType("FixedJoint2D", skipForJoint2D);
			AddSkipForType("SpringJoint2D", skipForJoint2D);
			AddSkipForType("DistanceJoint2D", skipForJoint2D);
			AddSkipForType("FrictionJoint2D", skipForJoint2D);
			AddSkipForType("RelativeJoint2D", skipForJoint2D);
			AddSkipForType("SliderJoint2D", skipForJoint2D);
			AddSkipForType("WheelJoint2D", skipForJoint2D);
			AddSkipForType("TargetJoint2D", "enableCollision");
			AddSkipForType("TargetJoint2D", skipForJoint2D);
			string[] skipForJoint = new string[1]
			{
				"connectedBody"
			};
			AddSkipForType("CharacterJoint", skipForJoint);
			AddSkipForType("ConfigurableJoint", skipForJoint);
			AddSkipForType("FixedJoint", skipForJoint);
			AddSkipForType("HingeJoint", skipForJoint);
			AddSkipForType("SpringJoint", skipForJoint);
			AddSkipForType("ReflectionProbe", "bakedTexture", "customBakedTexture");
			AddSkipForType("Skybox", "material");
			AddSkipForType("NavMeshAgent", "velocity", "nextPosition");
			AddSkipForType("AudioSource", "clip", "outputAudioMixerGroup");
			AddSkipForType("AudioLowPassFilter", "customCutoffCurve");
			AddSkipForType("AudioReverbZone", "reverbDelay", "reflectionsDelay");
			AddSkipForType("LensFlare", "flare");
			AddSkipForType("Projector", "material");
			AddSkipForType("EventSystem", "m_FirstSelected", "firstSelectedGameObject");
			AddSkipForType("EventTrigger", "m_Delegates");
			AddSkipForType("Canvas", "worldCamera");
			AddSkipForType("TouchInputModule", "forceModuleActive");
			AddSkipForType("Light", "flare", "cookie");
			string[] skipForLayoutGroups = new string[2]
			{
				"m_Padding",
				"padding"
			};
			AddSkipForType("GridLayoutGroup", skipForLayoutGroups);
			AddSkipForType("HorizontalLayoutGroup", skipForLayoutGroups);
			AddSkipForType("VerticalLayoutGroup", skipForLayoutGroups);
			AddSkipForType("TextMesh", "font");
			AddSkipForType("Animation", "clip");
			AddSkipForType("Animator", "runtimeAnimatorController", "avatar", "bodyPosition", "bodyRotation", "playbackTime");
			AddSkipForType("NetworkView", "observed", "viewID");
			AddSkipForType("Terrain", "terrainData", "materialTemplate");
			AddSkipForType("NavMeshAgent", "path");
			AddSkipForType("OffMeshLink", "startTransform", "endTransform");
			string[] skipForNetworkManager = new string[9]
			{
				"m_SpawnPrefabs",
				"m_ConnectionConfig",
				"m_GlobalConfig",
				"m_Channels",
				"m_PlayerPrefab",
				"client",
				"matchInfo",
				"matchMaker",
				"matches"
			};
			AddSkipForType("NetworkManager", skipForNetworkManager);
			AddSkipForType("NetworkLobbyManager", skipForNetworkManager);
			AddSkipForType("NetworkLobbyManager", "m_LobbyPlayerPrefab", "m_GamePlayerPrefab");
			AddSkipForType("NetworkTransform", "m_ClientMoveCallback3D", "m_ClientMoveCallback2D");
			AddSkipForType("NetworkTransformVisualizer", "m_VisualizerPrefab");
			AddSkipForType("GUIText", "material", "font");
			AddSkipForType("GUITexture", "texture", "border");
			string[] skipForUIBehaviour = new string[30]
			{
				"m_OnClick",
				"m_TargetGraphic",
				"m_AnimationTriggers",
				"m_SpriteState",
				"m_OnCullStateChanged",
				"m_Template",
				"m_CaptionText",
				"m_CaptionImage",
				"m_Options",
				"m_OnValueChanged",
				"m_ItemText",
				"m_ItemImage",
				"m_Sprite",
				"m_Material",
				"m_TextComponent",
				"m_Placeholder",
				"m_OnEndEdit",
				"m_OnValidateInput",
				"m_Texture",
				"m_HandleRect",
				"m_FontData",
				"m_Group",
				"m_AsteriskChar",
				"m_FillRect",
				"onValueChanged",
				"graphic",
				"m_HorizontalScrollbar",
				"m_Content",
				"m_VerticalScrollbar",
				"m_Viewport"
			};
			AddSkipForType("Navigation", "m_SelectOnUp", "m_SelectOnDown", "m_SelectOnLeft", "m_SelectOnRight");
			AddSkipForType("Button", skipForUIBehaviour);
			AddSkipForType("Dropdown", skipForUIBehaviour);
			AddSkipForType("Image", skipForUIBehaviour);
			AddSkipForType("InputField", skipForUIBehaviour);
			AddSkipForType("RawImage", skipForUIBehaviour);
			AddSkipForType("Scrollbar", skipForUIBehaviour);
			AddSkipForType("ScrollRect", skipForUIBehaviour);
			AddSkipForType("Selectable", skipForUIBehaviour);
			AddSkipForType("Slider", skipForUIBehaviour);
			AddSkipForType("Text", skipForUIBehaviour);
			AddSkipForType("Toggle", skipForUIBehaviour);
			AddDontReadProperties("ColorBlock");
			AddDontReadProperties("Navigation");
			string[] includeForTransform = new string[3]
			{
				"localPosition",
				"localEulerAngles",
				"localScale"
			};
			AddIncludeForType("Transform", includeForTransform);
			string[] includeForRectTransform = new string[6]
			{
				"anchoredPosition",
				"anchorMax",
				"anchorMin",
				"offsetMax",
				"offsetMin",
				"pivot"
			};
			AddIncludeForType("RectTransform", includeForTransform);
			AddIncludeForType("RectTransform", includeForRectTransform);
			string[] includeForRenderer = new string[4]
			{
				"shadowCastingMode",
				"receiveShadows",
				"useLightProbes",
				"reflectionProbeUsage"
			};
			AddIncludeForType("MeshRenderer", includeForRenderer);
			AddIncludeForType("SpriteRenderer", includeForRenderer);
			AddIncludeForType("SpriteRenderer", "color", "flipX", "flipY");
			string[] includeForParticleSystemRenderer = new string[11]
			{
				"alignment",
				"cameraVelocityScale",
				"lengthScale",
				"maxParticleSize",
				"minParticleSize",
				"normalDirection",
				"pivot",
				"renderMode",
				"sortingFudge",
				"sortMode",
				"velocityScale"
			};
			AddIncludeForType("ParticleSystemRenderer", includeForRenderer);
			AddIncludeForType("ParticleSystemRenderer", includeForParticleSystemRenderer);
			AddIncludeForType("TrailRenderer", includeForRenderer);
			AddIncludeForType("TrailRenderer", "autodestruct", "endWidth", "startWidth", "time");
			AddIncludeForType("SkinnedMeshRenderer", includeForRenderer);
			AddIncludeForType("SkinnedMeshRenderer", "quality", "updateWhenOffscreen", "localBounds");
			AddIncludeForType("LineRenderer", includeForRenderer);
			AddIncludeForType("LineRenderer", "useWorldSpace");
			AddIncludeForType("BillboardRenderer", includeForRenderer);
			string[] includeForCamera = new string[14]
			{
				"clearFlags",
				"backgroundColor",
				"cullingMask",
				"orthographic",
				"orthographicSize",
				"fov",
				"nearClipPlane",
				"farClipPlane",
				"rect",
				"depth",
				"renderingPath",
				"useOcclusionCulling",
				"hdr",
				"targetDisplay"
			};
			AddIncludeForType("Camera", includeForCamera);
			AddIncludeForType("MeshFilter");
			AddIncludeForType("NetworkAnimator");
			AddIncludeForType("NetworkIdentity", "m_ServerOnly", "m_LocalPlayerAuthority");
			AddIncludeForType("CanvasRenderer");
		}

		private void AddDontReadProperties(string typeName)
		{
			m_dontReadProperties.Add(typeName);
		}

		private void AddSkipForType(string typeName, params string[] properties)
		{
			HashSet<string> set;
			if (!m_skipPropertiesPerType.TryGetValue(typeName, out set))
			{
				set = new HashSet<string>();
				m_skipPropertiesPerType.Add(typeName, set);
			}
			set.UnionWith(properties);
		}

		private void AddIncludeForType(string typeName, params string[] properties)
		{
			HashSet<string> set;
			if (!m_includePropertiesPerType.TryGetValue(typeName, out set))
			{
				set = new HashSet<string>();
				m_includePropertiesPerType.Add(typeName, set);
			}
			set.UnionWith(properties);
		}

		private bool HasIncludePerType(string ownerTypeName)
		{
			return m_includePropertiesPerType.ContainsKey(ownerTypeName);
		}

		private bool IncludeMember(string ownerTypeName, string memberInfoName)
		{
			HashSet<string> includePerType = null;
			if (!m_includePropertiesPerType.TryGetValue(ownerTypeName, out includePerType))
			{
				return true;
			}
			if (includePerType.Contains(memberInfoName))
			{
				return true;
			}
			return false;
		}

		private bool SkipMember(string ownerTypeName, string memberInfoName)
		{
			if (m_skipProperties.Contains(memberInfoName))
			{
				return true;
			}
			HashSet<string> skipPerType = null;
			if (m_skipPropertiesPerType.TryGetValue(ownerTypeName, out skipPerType) && skipPerType.Contains(memberInfoName))
			{
				return true;
			}
			return false;
		}
	}
}
