using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace BotClient
{
	internal class Program
	{
		private static string serverBaseUrl = "http://localhost:8000";
		private static HttpClient client = new HttpClient();

		private static List<string> testCaseList = new List<string>()
		{
			"/Exp?p1=/완료&p2=솔휘",
			"/Exp?p1=/한주끝"
		};

		public static async Task Main(string[] args)
		{
			foreach(var testCase in testCaseList)
			{
				var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, serverBaseUrl + testCase));
				string message = await response.Content.ReadAsStringAsync();
				Console.WriteLine(message);
			}
		}
	}
}
