using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	internal class Listener
	{
		private HttpListener listener = new HttpListener();
		private const string serverUrl = "http://localhost";

		private int requestCount = 0;
		private bool isRunning = false;

		public Listener(int port)
		{
			listener.Prefixes.Add($"{serverUrl}:{port}/");
		}

		public void Start()
		{
			isRunning = true;
			listener.Start();
		}

		public void Close()
		{
			isRunning = false;
			listener.Close();
		}

		public async Task Listen()
		{
			while (isRunning)
			{
				Console.WriteLine($"Start Listening... {GetType()}");

				HttpListenerContext ctx = await listener.GetContextAsync();

				HttpListenerRequest req = ctx.Request;
				HttpListenerResponse res = ctx.Response;

				var responseMessage = await RequestProcess(req);
				await ResponseProcess(res, responseMessage);
			}
		}

		protected async virtual Task<string> RequestProcess(HttpListenerRequest request)
		{
			await Task.Yield();

			Console.WriteLine($"Request #: {++requestCount}");
			Console.WriteLine(request.Url?.ToString());
			Console.WriteLine(request.HttpMethod);
			Console.WriteLine(request.UserHostName);
			Console.WriteLine(request.UserAgent);
			Console.WriteLine(request.UserHostAddress);
			Console.WriteLine();

			return string.Empty; 
		}

		private async Task ResponseProcess(HttpListenerResponse response, string responseMessage)
		{
			Console.WriteLine(responseMessage);

			byte[] data = Encoding.UTF8.GetBytes(responseMessage);

			response.ContentEncoding = Encoding.UTF8;
			response.ContentLength64 = data.LongLength;

			await response.OutputStream.WriteAsync(data, 0, data.Length);
			response.Close();
		}
	}
}
