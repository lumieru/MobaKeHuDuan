using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hdg
{
	public static class rdtTypeExtensions
	{
		private static List<FieldInfo> s_fields = new List<FieldInfo>(256);

		public static bool IsUserStruct(this Type type)
		{
			if (!type.IsPrimitive && !type.IsEnum)
			{
				return type.IsValueType;
			}
			return false;
		}

		public static bool IsGenericList(this Type type)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
			}
			return false;
		}

		public static bool IsReference(this Type type)
		{
			if (!type.IsValueType && type != typeof(string) && !type.IsArray)
			{
				return !type.IsGenericList();
			}
			return false;
		}

		public static Type GetListElementType(this Type type)
		{
			if (!type.IsArray)
			{
				if (!type.IsGenericList())
				{
					return null;
				}
				return type.GetGenericArguments()[0];
			}
			return type.GetElementType();
		}

		public static List<FieldInfo> GetAllFields(this Type t)
		{
			s_fields.Clear();
			GetAllFieldsImp(t);
			return s_fields;
		}

		public static FieldInfo GetFieldInHierarchy(this Type t, string name)
		{
			if (t == null)
			{
				return null;
			}
			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo field = t.GetField(name, flags);
			if (field == null)
			{
				field = t.BaseType.GetFieldInHierarchy(name);
			}
			return field;
		}

		private static void GetAllFieldsImp(Type t)
		{
			if (t != null)
			{
				BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				FieldInfo[] fields = t.GetFields(flags);
				s_fields.AddRange(fields);
				GetAllFieldsImp(t.BaseType);
			}
		}
	}
}
