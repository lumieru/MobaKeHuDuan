using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

namespace Hdg
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct rdtTcpMessageSendLuaCode : rdtTcpMessage
    {
        public string luaCode;
        public void Write(BinaryWriter w)
        {
            w.Write(luaCode);
        }

        public void Read(BinaryReader r)
        {
            luaCode = r.ReadString();
        }
    }
}