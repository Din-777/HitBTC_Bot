using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace HitBTC
{
	public class HitBTCSocketAPI
	{
		private WebSocket socket;

		public delegate void SocketOpenedHandler();
		public event SocketOpenedHandler Opened;

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);
			socket.Open();
			socket.Opened += Socket_Opened;
		}

		public virtual void Socket_Opened(object sender, EventArgs e)
		{
			if (Opened != null) Opened();
		}
	}
}
