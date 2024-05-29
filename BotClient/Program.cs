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
			"/Kahlua?p1=완료&p2=유진",
			"/Kahlua?p1=완료복원&p2=유진",
			"/Kahlua?p1=휴식&p2=유진",
			"/Kahlua?p1=완료복원&p2=유진",
			"/Kahlua?p1=완료복원&p2=유진",
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
