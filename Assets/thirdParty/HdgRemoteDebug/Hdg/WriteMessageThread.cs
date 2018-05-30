using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Hdg
{
	public class WriteMessageThread
	{
		private enum State
		{
			Idle,
			Writing,
			LostConnection,
			Max
		}

		private Stream m_stream;

		private BinaryWriter m_writer;

		private Queue<rdtTcpMessage> m_messageQueue = new Queue<rdtTcpMessage>();

		private bool m_run;

		private Action[] m_stateDelegates;

		private State m_state;

		private rdtTcpMessage m_currentMessage;

		private AutoResetEvent m_event = new AutoResetEvent(false);

		private Thread m_thread;

		private string m_name;

		public bool IsConnected
		{
			get
			{
				return m_state != State.LostConnection;
			}
		}

		public WriteMessageThread(Stream stream, string name)
		{
			m_name = name;
			m_stateDelegates = new Action[3];
			m_stateDelegates[0] = OnIdle;
			m_stateDelegates[1] = OnWriting;
			m_stateDelegates[2] = OnLostConnection;
			m_stream = stream;
			m_writer = new BinaryWriter(m_stream);
			m_run = true;
			m_thread = new Thread(ThreadFunc);
			m_thread.Name = m_name + " rdtWriteMessageThread";
			m_thread.Start();
		}

		public void Stop()
		{
			m_run = false;
			m_event.Set();
			m_thread.Join();
		}

		public void EnqueueMessage(rdtTcpMessage message)
		{
			lock (m_messageQueue)
			{
				m_messageQueue.Enqueue(message);
			}
			m_event.Set();
		}

		private void ThreadFunc()
		{
			while (m_run)
			{
				if (m_stateDelegates[(int)m_state] != null)
				{
					m_stateDelegates[(int)m_state]();
				}
			}
			rdtDebug.Debug(this, "Exited");
		}

		private void OnIdle()
		{
			m_currentMessage = null;
			lock (m_messageQueue)
			{
				if (m_messageQueue.Count > 0)
				{
					m_currentMessage = m_messageQueue.Dequeue();
				}
			}
			if (m_currentMessage != null)
			{
				m_state = State.Writing;
			}
			else
			{
				m_event.WaitOne();
			}
		}

		private void OnWriting()
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (BinaryWriter bw = new BinaryWriter(ms))
					{
						string name = m_currentMessage.GetType().FullName;
						bw.Write(name);
						m_currentMessage.Write(bw);
						byte[] bytes = ms.ToArray();
						m_writer.Write(bytes.Length);
						m_writer.Write(bytes);
						m_writer.Flush();
					}
				}
				m_state = State.Idle;
			}
			catch (IOException ioe)
			{
				rdtDebug.Log(this, ioe, rdtDebug.LogLevel.Debug, "{0} lost connection", m_name);
				m_state = State.LostConnection;
			}
			catch (ObjectDisposedException)
			{
				rdtDebug.Debug(this, "{0} object disposed, lost connection", m_name);
				m_state = State.LostConnection;
			}
			catch (Exception e)
			{
				rdtDebug.Error(this, e, "{0} Unknown exception", m_name);
				m_state = State.LostConnection;
			}
		}

		private void OnLostConnection()
		{
			m_run = false;
		}
	}
}
