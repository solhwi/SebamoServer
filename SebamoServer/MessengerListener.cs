using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	internal class MessengerListener : Listener
	{
		private CommandExecutor commandExecutor = new CommandExecutor();

		public MessengerListener(int port) : base(port)
		{
		}

		protected async override Task<string> RequestProcess(HttpListenerRequest request)
		{
			await base.RequestProcess(request);

			var command = CommandFactory.MakeCommand(request);
			if (command == null)
				return string.Empty;

			return await commandExecutor.MakeResponse(command);
		}
	}
}
