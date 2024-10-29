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
    internal class GameDataManager
    {
        public void Save(PacketData packetData)
        {
            var groupType = GetGroupType(packetData.playerGroup);
            string playerName = packetData.playerName;
			string dataPath = string.Empty;

			if (packetData is MyPlayerPacketData)
			{
				dataPath = GetUserDataPath(groupType, playerName);
			}
			else if (packetData is TilePacketData)
			{
				dataPath = GetTileDataPath(groupType);
			}

			string jsonData = JsonConvert.SerializeObject(packetData);
            File.WriteAllText(dataPath, jsonData);
        }

		private void TryInitializeDataFile(string dataPath, GroupType groupType, string name)
        {
			if (File.Exists(dataPath) == false)
            {
                string jsonData = GetDefaultPlayerPacketData(groupType, name);

				File.WriteAllText(dataPath, jsonData);
			}
		}

        private void TryInitializeTileDataFile(string dataPath, GroupType groupType)
        {
			if (File.Exists(dataPath) == false)
			{
				string jsonData = GetDefaultTilePacketData();

				File.WriteAllText(dataPath, jsonData);
			}
		}

        private string GetDefaultPlayerPacketData(GroupType groupType, string name)
        {
			string defaultJsonPath = $"{Config.ScriptRootPath}/DefaultPlayerData.json";
			string jsonData = File.ReadAllText(defaultJsonPath);

			var data = JsonConvert.DeserializeObject<MyPlayerPacketData>(jsonData);
            if (data == null)
                return string.Empty;

			data.playerData.playerName = name;
            data.playerData.playerGroup = groupType.ToString();

            return JsonConvert.SerializeObject(data);
		}

        private string GetDefaultTilePacketData()
        {
            string defaultJsonPath = $"{Config.ScriptRootPath}/DefaultTileData.json";
            return File.ReadAllText(defaultJsonPath);
		}

		public MyPlayerPacketData LoadMyPacketData(GroupType groupType, string playerName)
        {
			string dataPath = GetUserDataPath(groupType, playerName);

			TryInitializeDataFile(dataPath, groupType, playerName);

			string jsonData = File.ReadAllText(dataPath);
            if (jsonData == string.Empty)
                return null;

			return JsonConvert.DeserializeObject<MyPlayerPacketData>(jsonData);
		}

        public PlayerPacketDataCollection LoadOtherPacket(GroupType groupType, string playerName)
        {
			var names = Config.GetNamesWithoutMe(groupType, playerName).ToArray();

			PlayerPacketDataCollection collection = new PlayerPacketDataCollection();
            collection.playerDatas = new PlayerPacketData[names.Length];

			for(int i = 0; i < names.Length; i++)
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

        public TilePacketData LoadTilePacket(GroupType groupType)
        {
            string dataPath = GetTileDataPath(groupType);

            TryInitializeTileDataFile(dataPath, groupType);

			string jsonData = File.ReadAllText(dataPath);
			if (jsonData == string.Empty)
				return null;

			return JsonConvert.DeserializeObject<TilePacketData>(jsonData);
		}

		private string GetUserDataPath(GroupType groupType, string name)
        {
            string dataPath = $"{Config.GetDataRootPath(groupType)}/{name}.json";
            return dataPath;
        }

        private string GetTileDataPath(GroupType groupType)
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
