using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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
		public int totalPenaltyFee;
		public int usedPenaltyFee;

		public void AddWeeklyPoint(int point)
		{
			SetWeeklyPoint(weeklyPoint + point);
		}

		public void ResetWeeklyPoint()
		{
			SetWeeklyPoint(0);
		}

		/// <summary>
		/// 1. 모자란 위클리 포인트만큼 패널티 요금에 더함
		/// 2. 현재 위클리 포인트를 토탈 포인트로 옮김
		/// </summary>
		public void EndWeekly(GroupType groupType)
		{
			int currentWeeklyPoint = weeklyPoint;
			if (currentWeeklyPoint < Config.MaxWeeklyPoint)
			{
				int penaltyPoint = Config.MaxWeeklyPoint - currentWeeklyPoint;
				int addFee = Config.GetPenaltyFee(groupType, penaltyPoint);

				penaltyFee += addFee;
				totalPenaltyFee += addFee;
			}

			totalPoint += currentWeeklyPoint;
			weeklyPoint = 0;
		}

		public void SendMoney(int money)
		{
			if (penaltyFee - money > 0)
			{
				penaltyFee -= money;
			}
			else
			{
				penaltyFee = 0;
			}
		}

		public void ResetPenaltyFee()
		{
			penaltyFee = 0;
			totalPenaltyFee = 0;
			usedPenaltyFee = 0;
		}

		public void UsePenaltyFee(int fee)
		{
			usedPenaltyFee += fee;
		}

		public void CompleteWeeklyPoint()
		{
            SetWeeklyPoint(Config.MaxWeeklyPoint);
        }

		public void SetWeeklyPoint(int point)
		{
			if (point < 0)
				point = 0;

			if (point > Config.MaxWeeklyPoint)
				point = Config.MaxWeeklyPoint;

			weeklyPoint = point;
		}

		public SebamoData Clone()
		{
			var newData = new SebamoData();

			newData.name = name;
			newData.weeklyPoint = weeklyPoint;
			newData.totalPoint = totalPoint;
			newData.penaltyFee = penaltyFee;
			newData.totalPenaltyFee = totalPenaltyFee;
			newData.usedPenaltyFee = usedPenaltyFee;

			return newData;
		}
	}

	internal class SpreadSheetManager
	{
		private readonly string BaseGetUrl = "https://docs.google.com/spreadsheets/d/1bc9q-co0H9_nmo-AuQtN5dYINCCB-Qv1o42yH3D3vTk/export?format=csv&gid=";
		private readonly string BasePostUrl = "https://script.google.com/macros/s/AKfycbwYTQb-Du53Oikr5ayTuzJZzrI_zIeZBYr5xhoKFSiM6MmyvO9kfb605QV_ZNdjPNuHmA/exec";
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
				string jsonData = JsonConvert.SerializeObject(encodedData, Formatting.None);

				var setting = new JsonSerializerOptions()
				{
					Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
				};

				await client.PostAsJsonAsync(BasePostUrl, jsonData, setting);
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
				data.totalPenaltyFee = (int)sheet["totalPenaltyFee"];
				data.usedPenaltyFee = (int)sheet["usedPenaltyFee"];

				sebamoDictionary.Add(data.name, data);
			}

			return sebamoDictionary;
		}
	}
}
