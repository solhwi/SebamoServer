using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SebamoServer
{
    internal class GameDataManager
    {
        public void Save(GroupType groupType, MyPlayerPacketData packetData)
        {
            string dataPath = GetDataPath(groupType, packetData);
            string jsonData = JsonSerializer.Serialize<MyPlayerPacketData>(packetData);

            File.WriteAllText(dataPath, jsonData);
        }

        public T Load<T>(GroupType groupType, string name) where T : PacketData
        {
            return null;
        }

        private string GetDataPath(GroupType groupType, MyPlayerPacketData packetData)
        {
            string dataPath = $"{Config.GetDataPath(groupType)}/{packetData.playerData.playerName}.json";
            return dataPath;
        }
    }
}
