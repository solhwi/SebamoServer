using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	internal class GameListener : Listener
	{
		public GameListener(int port) : base(port)
		{

		}

		protected async override Task<string> RequestProcess(HttpListenerRequest request)
		{
			await base.RequestProcess(request);

			return string.Empty;
		}
	}
}
