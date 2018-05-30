using UnityEditor;
using UnityEngine;

namespace Hdg
{
	public static class Preferences
	{
		[PreferenceItem("Remote Debug")]
		public static void OnGUI()
		{
			EditorGUILayout.Space();
			int broadcastPort2 = EditorPrefs.GetInt("Hdg.RemoteDebug.BroadcastPort", 12000);
			broadcastPort2 = EditorGUILayout.IntField("Server broadcast port", broadcastPort2);
			if (GUI.changed)
			{
				EditorPrefs.SetInt("Hdg.RemoteDebug.BroadcastPort", broadcastPort2);
				if ((bool)ConnectionWindow.Instance)
				{
					ConnectionWindow.Instance.RestartServerEnumerator();
				}
			}
			bool debug2 = EditorPrefs.GetBool("Hdg.RemoteDebug.Debug", false);
			debug2 = EditorGUILayout.Toggle("Debug mode", debug2);
			if (GUI.changed)
			{
				EditorPrefs.SetBool("Hdg.RemoteDebug.Debug", debug2);
				rdtDebug.s_logLevel = ((!debug2) ? rdtDebug.LogLevel.Info : rdtDebug.LogLevel.Debug);
			}
		}
	}
}
