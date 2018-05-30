using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternals
{
	public static class GUI
	{
		private static MethodInfo s_toolbarSearchField;

		public static string ToolbarSearchField(string text, params GUILayoutOption[] options)
		{
			if (s_toolbarSearchField == null)
			{
				s_toolbarSearchField = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(string),
					typeof(GUILayoutOption[])
				}, null);
			}
			object[] args = new object[2]
			{
				text,
				options
			};
			string result = "";
			if (s_toolbarSearchField != null)
			{
				result = (string)s_toolbarSearchField.Invoke(null, args);
			}
			return result;
		}
	}
}
