﻿using Newtonsoft.Json;
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
        public void Save(MyPlayerPacketData packetData)
        {
            var groupType = GetGroupType(packetData.playerData.playerGroup);
            string playerName = packetData.playerData.playerName;

            string dataPath = GetDataPath(groupType, playerName);
            string jsonData = JsonConvert.SerializeObject(packetData);

            File.WriteAllText(dataPath, jsonData);
        }

        private void TryInitializeDataFile(string dataPath, GroupType groupType, string name)
        {
			if (File.Exists(dataPath) == false)
            {
                string jsonData = GetDefaultMyPlayerPacketData(groupType, name);

				File.WriteAllText(dataPath, jsonData);
			}
		}

        private string GetDefaultMyPlayerPacketData(GroupType groupType, string name)
        {
            var data = new MyPlayerPacketData();
            data.playerData = new PlayerPacketData();

			data.playerData.playerName = name;
            data.playerData.playerGroup = groupType.ToString();

            return JsonConvert.SerializeObject(data);
		}

        public MyPlayerPacketData LoadMyPacketData(GroupType groupType, string playerName)
        {
			string dataPath = GetDataPath(groupType, playerName);

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

				string dataPath = GetDataPath(groupType, name);

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

        private string GetDataPath(GroupType groupType, string name)
        {
            string dataPath = $"{Config.GetDataPath(groupType)}/{name}.json";
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
