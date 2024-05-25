using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;

namespace SebamoServer
{
	public enum GroupType
	{
		None = -1,
		Kahlua = 0,
		Exp = 1,
	}

	public class SebamoData
	{
		public string name;
		public int weeklyPoint;
		public int totalPoint;
		public int penaltyFee;
	}

	internal class SpreadSheetManager
	{
		private readonly string BaseUrl = "https://docs.google.com/spreadsheets/d/1bc9q-co0H9_nmo-AuQtN5dYINCCB-Qv1o42yH3D3vTk/export?format=csv&gid=";

		private Dictionary<GroupType, string> groupGidDictionary = new Dictionary<GroupType, string>()
		{
			{ GroupType.Kahlua, "0"}, 
			{ GroupType.Exp, "2020924495"},
		};

		private HttpClient client = new HttpClient();

		private Uri GetUri(GroupType groupType)
		{
			if (groupGidDictionary.TryGetValue(groupType, out string gid) == false)
				return null;

			return new Uri(BaseUrl + gid);
		}

		public async Task<Dictionary<string, SebamoData>> GetSebamoData(GroupType groupType)
		{
			try
			{
				var uri = GetUri(groupType);
				string rawData = await client.GetStringAsync(uri);
				return MakeSebamoData(rawData);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("\nException Caught!");
				Console.WriteLine("Message :{0} ", e.Message);

				return null;
			}
		}

		private Dictionary<string, SebamoData> MakeSebamoData(string rawText)
		{
			var sheetList = CSVReader.Read(rawText);
			if (sheetList == null || sheetList.Count == 0)
				return null;

			Dictionary<string, SebamoData> sebamoDictionary = new Dictionary<string, SebamoData>();

			foreach(var sheet in sheetList)
			{
				var data = new SebamoData();

				data.name = (string)sheet["name"];
				data.weeklyPoint = (int)sheet["weeklyPoint"];
				data.totalPoint = (int)sheet["totalPoint"];
				data.penaltyFee = (int)sheet["penaltyFee"];

				sebamoDictionary.Add(data.name, data);
			}

			return sebamoDictionary;
		}
	}
}
