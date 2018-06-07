
/*
Author: liyonghelpme
Email: 233242872@qq.com
*/

/*
Author: liyonghelpme
Email: 233242872@qq.com
*/
using MyLib;
using System.Reflection;
using System.Collections.Generic;

namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Collections; 
	using System.Collections.Generic;
	using System.Text;
    using System.Threading;
	using System.Text.RegularExpressions;
	
	using MessageID = System.UInt16;
	using MessageLength = System.UInt32;

	public delegate void Callback();

	public class KBEngineApp : IMainLoop
	{
		public static KBEngineApp app = null;
		
        public  void queueInUpdate(System.Action cb) {
        }
        public void removeUpdate(System.Action cb) {
        }
		
		
		public Queue<System.Action> pendingCallbacks = new Queue<Action>();

		ClientApp client;
        public KBEngineApp(ClientApp c)
        {
			client = c;
			app = this;
        }

	
		public void queueInLoop(System.Action cb) {
			lock (this) {
				pendingCallbacks.Enqueue(cb);
			}
		}

		public void UpdateMain() {
			lock (this) {
			    while (pendingCallbacks.Count > 0)
			    {
			        var cb = pendingCallbacks.Dequeue();
			        try
			        {
			            cb();
			        }
			        catch (Exception ex)
			        {
			            Debug.LogError(ex.ToString());
			        }
			    }
			}
		}

	}
} 
