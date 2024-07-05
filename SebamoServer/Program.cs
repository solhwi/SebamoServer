// ImplicitUsings 활성화 상태

using System.Net;
using System.Text;

namespace SebamoServer
{
	internal class Program
	{
		private static MessengerListener messengerListener = null;
		private static GameListener gameListener = null;

		private const int MessengerServerPort = 8000;
		private const int GameServerPort = 8001;

		private static async Task Main(string[] args)
		{
			messengerListener = new MessengerListener(MessengerServerPort);
			gameListener = new GameListener(GameServerPort);

			messengerListener.Start();
			gameListener.Start();

			var messengerTask = messengerListener.Listen();
			var gameTask = gameListener.Listen();

			messengerTask.GetAwaiter().GetResult();
			gameTask.GetAwaiter().GetResult();

			messengerListener.Close();
			gameListener.Close();
		}
	}
}