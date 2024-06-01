using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace SebamoServer
{
	public enum GroupType
	{
		None = -1,
		Kahlua = 0,
		Exp = 1,
	}

	[Serializable()]
	public class EncodedSebamoData
	{
        public string groupType;
        public List<SebamoData> datas = new List<SebamoData>();

		public EncodedSebamoData(GroupType groupType, SebamoData data)
		{
            this.groupType = groupType.ToString();
			this.datas.Add(data.Clone());
        }

		public EncodedSebamoData(GroupType groupType, Dictionary<string, SebamoData> datas)
		{
			this.groupType = groupType.ToString();
			this.datas = datas.Values.ToList();
		}
    }

    [Serializable()]
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

		public void CompleteWeeklyPoint()
		{
            SetWeeklyPoint(MaxWeeklyPoint);
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
		private readonly string BasePostUrl = "https://script.google.com/macros/s/AKfycbzal9aux1EKgIC0d1hYc4FVYF_8Hd7rVkuz3LlErwLc8QReSj-HLhMtDQpqRMu6H6Sdvw/exec";
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
			var parameter = MakeParameter(groupType, newSebamoData);
            await PostSebamoData(parameter);
        }

		public async Task UpdateSebamoData(GroupType groupType, Dictionary<string, SebamoData> newSebamoDataDictionary)
		{
			var parameter = MakeParameter(groupType, newSebamoDataDictionary);
			await PostSebamoData(parameter);
		}

		private async Task PostSebamoData(EncodedSebamoData encodedData)
		{
            try
            {
                await client.PostAsJsonAsync(BasePostUrl, encodedData);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        private EncodedSebamoData MakeParameter(GroupType groupType, SebamoData newSebamoData)
		{
			return new EncodedSebamoData(groupType, newSebamoData);
		}

        private EncodedSebamoData MakeParameter(GroupType groupType, Dictionary<string, SebamoData> newSebamoDataDictionary)
        {
            return new EncodedSebamoData(groupType, newSebamoDataDictionary);
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
