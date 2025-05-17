using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace SebamoServer
{
    internal static class GameDataManager
    {
		private static object lockObj = new object();

		public static void EndWeekly(GroupType groupType, Dictionary<string, SebamoData> sebamoDictionary)
		{
			string? firstPlayerName = sebamoDictionary.Keys.FirstOrDefault();
			if (firstPlayerName == null)
				return;

			var myPacket = LoadMyPacketData(groupType, firstPlayerName);
			if (sebamoDictionary.TryGetValue(firstPlayerName, out var data))
			{
				myPacket.playerData.hasDiceCount += data.GetNewDiceCount();
				Save(myPacket);
			}

			var otherPackets = LoadOtherPacket(groupType, firstPlayerName);
			foreach (var otherPacket in otherPackets.playerDatas)
			{
				if (sebamoDictionary.TryGetValue(otherPacket.playerName, out data))
				{
					otherPacket.hasDiceCount += data.GetNewDiceCount();
				}

				Save(otherPacket);
			}
		}

        public static void Save(PacketData packetData)
        {
			lock (lockObj)
			{
				var groupType = GetGroupType(packetData.playerGroup);
				string playerName = packetData.playerName; 
				string dataPath = string.Empty;

				if (packetData is TilePacketData)
				{
					dataPath = GetTileDataPath(groupType);
				}
				else if (packetData is PlayerPacketData || packetData is MyPlayerPacketData)
				{
					dataPath = GetUserDataPath(groupType, playerName);
				}

				string jsonData = JsonConvert.SerializeObject(packetData);
				File.WriteAllText(dataPath, jsonData);
			}
        }

		private static void TryInitializeDataFile(string dataPath, GroupType groupType, string name)
        {
			lock (lockObj)
			{
				if (File.Exists(dataPath) == false)
				{
					string jsonData = GetDefaultPlayerPacketData(groupType, name);

					File.WriteAllText(dataPath, jsonData);
				}
			}
		}

		private static void TryInitializeTileDataFile(string dataPath, GroupType groupType)
        {
			lock (lockObj)
			{
				if (File.Exists(dataPath) == false)
				{
					string jsonData = GetDefaultTilePacketData();

					File.WriteAllText(dataPath, jsonData);
				}
			}
		}

        private static string GetDefaultPlayerPacketData(GroupType groupType, string name)
        {
			lock (lockObj)
			{
				string defaultJsonPath = $"{Config.ScriptRootPath}/DefaultPlayerData.json";
				string jsonData = File.ReadAllText(defaultJsonPath);

				var data = JsonConvert.DeserializeObject<PlayerSaveData>(jsonData);
				if (data == null)
					return string.Empty;

				data.data.playerData.playerName = name;
				data.data.playerName = name;

				data.data.playerData.playerGroup = groupType.ToString();
				data.data.playerGroup = groupType.ToString();

				return JsonConvert.SerializeObject(data.data);
			}
		}

        private static string GetDefaultTilePacketData()
        {
			lock (lockObj)
			{
				string defaultJsonPath = $"{Config.ScriptRootPath}/DefaultTileData.json";
				return File.ReadAllText(defaultJsonPath);
			}
		}

		public static MyPlayerPacketData LoadMyPacketData(GroupType groupType, string playerName)
        {
			lock (lockObj)
			{
				string dataPath = GetUserDataPath(groupType, playerName);

				TryInitializeDataFile(dataPath, groupType, playerName);

				string jsonData = File.ReadAllText(dataPath);
				if (jsonData == string.Empty)
					return null;

				var data = JsonConvert.DeserializeObject<MyPlayerPacketData>(jsonData);
				return data;
			}
		}

        public static PlayerPacketDataCollection LoadOtherPacket(GroupType groupType, string playerName)
        {
			lock (lockObj)
			{
				var names = Config.GetNamesWithoutMe(groupType, playerName).ToArray();

				PlayerPacketDataCollection collection = new PlayerPacketDataCollection();
				collection.playerDatas = new PlayerPacketData[names.Length];

				for (int i = 0; i < names.Length; i++)
				{
					string name = names[i];

					string dataPath = GetUserDataPath(groupType, name);

					TryInitializeDataFile(dataPath, groupType, name);

					string jsonData = File.ReadAllText(dataPath);
					if (jsonData == string.Empty)
						continue;

					var packetData = JsonConvert.DeserializeObject<MyPlayerPacketData>(jsonData);
					if (packetData == null)
						continue;

					collection.playerDatas[i] = packetData.playerData;
				}

				return collection;
			}
		}

        public static TilePacketData LoadTilePacket(GroupType groupType)
        {
			lock (lockObj)
			{
				string dataPath = GetTileDataPath(groupType);

				TryInitializeTileDataFile(dataPath, groupType);

				string jsonData = File.ReadAllText(dataPath);
				if (jsonData == string.Empty)
					return null;

				return JsonConvert.DeserializeObject<TilePacketData>(jsonData);
			}
		}

		private static string GetUserDataPath(GroupType groupType, string name)
        {
            string dataPath = $"{Config.GetDataRootPath(groupType)}/{name}.json";
            return dataPath;
        }

        private static string GetTileDataPath(GroupType groupType)
        {
			string dataPath = $"{Config.GetDataRootPath(groupType)}/tile.json";
			return dataPath;
		}

		private static GroupType GetGroupType(string groupName)
		{
			if (Enum.TryParse(groupName, out GroupType groupType) == false)
				return GroupType.None;

			return groupType;
		}
	}
}
