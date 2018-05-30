using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Hdg
{
	internal class ServerBroadcaster
	{
		private Thread m_thread;

		private rdtUdpMessageHello m_message;

		private bool m_run;

		public ServerBroadcaster()
		{
			m_message = new rdtUdpMessageHello();
			m_message.m_deviceName = SystemInfo.deviceName;
			m_message.m_deviceType = SystemInfo.deviceType.ToString();
			m_message.m_devicePlatform = Application.platform.ToString();
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
			m_message.m_serverVersion = string.Format("{0}.{1}.{2} {3} {4}", version.Major, version.Minor, version.Build, beta, configuration);
			m_message.m_serverPort = Settings.DEFAULT_SERVER_PORT;
			m_run = true;
			m_thread = new Thread(ThreadFunc);
			m_thread.Name = "rdtServerBroadcaster";
			m_thread.Start();
		}

		public void Stop()
		{
			rdtDebug.Debug("Stopping server broadcaster thread");
			m_run = false;
			m_thread.Join();
		}

		private void ThreadFunc()
		{
			while (m_run)
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, Settings.DEFAULT_BROADCAST_PORT);
				try
				{
					using (MemoryStream ms = new MemoryStream())
					{
						using (BinaryWriter bw = new BinaryWriter(ms))
						{
							m_message.Write(bw);
							byte[] data = ms.ToArray();
							UdpClient udpClient = new UdpClient();
							udpClient.EnableBroadcast = true;
							udpClient.Connect(endPoint);
							udpClient.Send(data, data.Length);
							udpClient.Close();
						}
					}
				}
				catch (SocketException e2)
				{
					Exception ex2 = (e2.InnerException != null) ? e2.InnerException : e2;
					rdtDebug.Error(this, e2, "Couldn't broadcast hello message: {0}", ex2.Message);
					break;
				}
				catch (Exception e)
				{
					Exception ex = (e.InnerException != null) ? e.InnerException : e;
					rdtDebug.Error(this, e, "Error broadcasting hello message: {0}", ex.Message);
				}
				Thread.Sleep(Settings.BROADCAST_TIME * 1000);
			}
			rdtDebug.Debug(this, "Finishing broadcast thread");
		}
	}
}
