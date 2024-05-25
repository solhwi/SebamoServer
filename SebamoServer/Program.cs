// ImplicitUsings 활성화 상태

using System.Net;
using System.Text;

namespace SebamoServer
{
	internal class Program
	{
		private static CommandExecutor commandExecutor = new CommandExecutor();

		private static HttpListener listener = new HttpListener();
		private static string serverUrl = "http://localhost:8000/";

		private static bool isRunning = false;
		private static int requestCount = 0;

		private static async Task Main(string[] args)
		{
			isRunning = true;
			
			listener.Prefixes.Add(serverUrl);
			listener.Start();

			var listenTask = Listen();
			listenTask.GetAwaiter().GetResult();

			listener.Close();
			isRunning = false;
		}

		private static async Task Listen()
		{
			while(isRunning)
			{
				Console.WriteLine("Start Listening...");

				HttpListenerContext ctx = await listener.GetContextAsync();

				HttpListenerRequest req = ctx.Request;
				HttpListenerResponse res = ctx.Response;

				var responseMessage = await RequestProcess(req);
				await ResponseProcess(res, responseMessage);
			}
		}

		private static async Task<string> RequestProcess(HttpListenerRequest request)
		{
			Console.WriteLine($"Request #: {++requestCount}");
			Console.WriteLine(request.Url?.ToString());
			Console.WriteLine(request.HttpMethod);
			Console.WriteLine(request.UserHostName);
			Console.WriteLine(request.UserAgent);
			Console.WriteLine();

			var command = CommandFactory.MakeCommand(request);
			if (command == null)
				return string.Empty;

			return await commandExecutor.MakeResponse(command);
		}

		private static async Task ResponseProcess(HttpListenerResponse response, string responseMessage)
		{
			byte[] data = Encoding.UTF8.GetBytes(responseMessage);

			response.ContentEncoding = Encoding.UTF8;
			response.ContentLength64 = data.LongLength;

			await response.OutputStream.WriteAsync(data, 0, data.Length);
			response.Close();
		}
	}
}