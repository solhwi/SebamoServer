using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace BotClient
{
	internal class Program
	{
		public enum TestCaseType
		{
			CompleteYujin,
		}

		private static string serverBaseUrl = "http://localhost:8000";
		private static HttpClient client = new HttpClient();

		private static Dictionary<TestCaseType, string> testCaseDictionary = new Dictionary<TestCaseType, string>()
		{
			{ TestCaseType.CompleteYujin, "/Kahlua?p1=완료&p2=유진" },
		};

		public static async Task Main(string[] args)
		{
			if (testCaseDictionary.TryGetValue(TestCaseType.CompleteYujin, out string testParameters))
			{
				var message = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, serverBaseUrl + testParameters));
				string str = await message.Content.ReadAsStringAsync();
				Console.WriteLine(str);
			}
		}
	}
}
