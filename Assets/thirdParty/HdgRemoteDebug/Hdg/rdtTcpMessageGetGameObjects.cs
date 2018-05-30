using System.IO;
using System.Runtime.InteropServices;

namespace Hdg
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct rdtTcpMessageGetGameObjects : rdtTcpMessage
	{
		public void Write(BinaryWriter w)
		{
		}

		public void Read(BinaryReader r)
		{
		}
	}
}
