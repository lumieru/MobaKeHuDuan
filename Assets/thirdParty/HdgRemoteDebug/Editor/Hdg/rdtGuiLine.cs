using UnityEditor;
using UnityEngine;

namespace Hdg
{
	public static class rdtGuiLine
	{
		public static void DrawHorizontalLine()
		{
			Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.MaxHeight(1f), GUILayout.ExpandWidth(true));
			Color prevCol = GUI.color;
			GUI.color = Color.white;
			Color colour = EditorGUIUtility.isProSkin ? new Color(0.2784314f, 0.2784314f, 0.2784314f, 1f) : new Color(0.3647059f, 0.3647059f, 0.3647059f, 255f);
			EditorGUI.DrawRect(rect, colour);
			GUI.color = prevCol;
		}
	}
}
