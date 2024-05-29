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

		public const int MaxWeeklyPoint = 4;

		public void AddWeeklyPoint(int point)
		{
			SetWeeklyPoint(weeklyPoint + point);
		}

		public void ResetWeeklyPoint()
		{
			SetWeeklyPoint(0);
		}

		public void SetWeeklyPoint(int point)
		{
			if (point < 0)
				point = 0;

			if (point > MaxWeeklyPoint)
				point = MaxWeeklyPoint;

			weeklyPoint = point;
		}

		public SebamoData Clone()
		{
			var newData = new SebamoData();

			newData.name = name;
			newData.weeklyPoint = weeklyPoint;
			newData.totalPoint = totalPoint;
			newData.penaltyFee = penaltyFee;

			return newData;
		}
	}

	internal class SpreadSheetManager
	{
		private readonly string BaseGetUrl = "https://docs.google.com/spreadsheets/d/1bc9q-co0H9_nmo-AuQtN5dYINCCB-Qv1o42yH3D3vTk/export?format=csv&gid=";
		private readonly string BasePostUrl = "https://script.google.com/macros/s/AKfycbxxmr1HPLRyIWhLdTfxanfzZt4MT5LK1yRfByz7bT7qI3t2GO4F07_QeH4c4LSW7D7vyw/exec";
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

			return new Uri(BaseGetUrl + gid);
		}

		public async Task UpdateSebamoData(GroupType groupType, SebamoData newSebamoData)
		{
			var parameters = MakeParameters(groupType, newSebamoData);	
			var encodedContent = new FormUrlEncodedContent(parameters);

			try
			{
				await client.PostAsync(BasePostUrl, encodedContent);
			}
			catch (HttpRequestException e)
			{
				Console.WriteLine("\nException Caught!");
				Console.WriteLine("Message :{0} ", e.Message);
			}
		}

		private Dictionary<string, string> MakeParameters(GroupType groupType, SebamoData newSebamoData)
		{
			var parameters = new Dictionary<string, string>();
			
			parameters.Add("groupType", groupType.ToString());
			parameters.Add("name", newSebamoData.name);
			parameters.Add("weeklyPoint", newSebamoData.weeklyPoint.ToString());
			parameters.Add("totalPoint", newSebamoData.totalPoint.ToString());
			parameters.Add("penaltyFee", newSebamoData.penaltyFee.ToString());

			return parameters;
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
