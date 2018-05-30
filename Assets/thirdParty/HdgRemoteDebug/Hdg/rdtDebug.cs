using System;
using System.Diagnostics;
using UnityEngine;

namespace Hdg
{
	public class rdtDebug
	{
		public enum LogLevel
		{
			Debug,
			Info,
			Warning,
			Error
		}

		public static LogLevel s_logLevel = LogLevel.Info;

		public static void Debug(object instance, string fmt, params object[] args)
		{
			Log(instance, LogLevel.Debug, fmt, args);
		}

		public static void Debug(string fmt, params object[] args)
		{
			Log(LogLevel.Debug, fmt, args);
		}

		public static void Info(object instance, string fmt, params object[] args)
		{
			Log(instance, LogLevel.Info, fmt, args);
		}

		public static void Info(string fmt, params object[] args)
		{
			Log(LogLevel.Info, fmt, args);
		}

		public static void Warning(string fmt, params object[] args)
		{
			Log(LogLevel.Warning, fmt, args);
		}

		public static void Warning(object instance, string fmt, params object[] args)
		{
			Log(instance, LogLevel.Warning, fmt, args);
		}

		public static void Error(string fmt, params object[] args)
		{
			Log(LogLevel.Error, fmt, args);
		}

		public static void Error(object instance, string fmt, params object[] args)
		{
			Log(instance, LogLevel.Error, fmt, args);
		}

		public static void Error(object instance, Exception e, string fmt, params object[] args)
		{
			Log(instance, e, LogLevel.Error, fmt, args);
		}

		public static void Log(object instance, Exception e, LogLevel l, string fmt, params object[] args)
		{
			Exception exception = (e.InnerException != null) ? e.InnerException : e;
			Log(instance, l, fmt + " " + exception + " " + e.StackTrace, args);
		}

		public static void Log(object instance, LogLevel l, string fmt, params object[] args)
		{
			if (l >= s_logLevel)
			{
				string msg = instance.GetType().Name + ": " + string.Format(fmt, args);
				switch (l)
				{
				case LogLevel.Error:
					UnityEngine.Debug.LogError(msg);
					break;
				case LogLevel.Warning:
					UnityEngine.Debug.LogWarning(msg);
					break;
				default:
					UnityEngine.Debug.Log(msg);
					break;
				}
			}
		}

		public static void Log(LogLevel l, string fmt, params object[] args)
		{
			if (l >= s_logLevel)
			{
				string msg = string.Format(fmt, args);
				switch (l)
				{
				case LogLevel.Error:
					UnityEngine.Debug.LogError(msg);
					break;
				case LogLevel.Warning:
					UnityEngine.Debug.LogWarning(msg);
					break;
				default:
					UnityEngine.Debug.Log(msg);
					break;
				}
			}
		}

		[Conditional("DEBUG")]
		public static void Assert(bool condition)
		{
			if (!condition)
			{
				UnityEngine.Debug.LogError("Assert failed");
			}
		}
	}
}
