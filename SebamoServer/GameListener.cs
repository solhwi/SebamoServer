using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SebamoServer
{
	internal class GameListener : Listener
	{
		private static GameDataManager dataManager = new GameDataManager();

		public GameListener(int port) : base(port)
		{

		}

		protected async override Task<string> RequestProcess(HttpListenerRequest request)
		{
			await base.RequestProcess(request);

			var packetData = MakePacketData(request);
			if (packetData == null)
				return string.Empty;

			return JsonConvert.SerializeObject(packetData);
		}

		public static PacketData MakePacketData(HttpListenerRequest request)
		{
			var commandGroupType = GetCommandGroupType(request);

			if (request.HttpMethod == "GET")
			{
				var commandParameters = GetGetParameters(request).ToArray();
				return MakeGetPacketData(commandGroupType, commandParameters);
			}
			else if (request.HttpMethod == "POST")
			{
				return MakePostPacketData(request);
			}

			return null;
		}

		private static PacketData MakeGetPacketData(GroupType groupType, string[] commandParameters)
		{
			if (commandParameters == null || commandParameters.Length < 1)
				return null;

			// 1번 인자가 이름
			string userName = commandParameters[0];

			if (Config.IsInGroup(groupType, userName) == false)
				return null;

			string commandType = commandParameters[1];
			if (commandType == "My")
			{
				return dataManager.LoadMyPacketData(groupType, userName);
			}
			else if (commandType == "Other")
			{
				return dataManager.LoadOtherPacket(groupType, userName);
			}
			else if (commandType == "Tile")
			{
				return dataManager.LoadTilePacket(groupType);
			}

			return null;
		}

		private static PacketData MakePostPacketData(HttpListenerRequest request)
		{
			string jsonData = GetPostJsonData(request);

			PacketData packetData = JsonConvert.DeserializeObject<PacketData>(jsonData);
			if (packetData == null)
				return null;

			if (packetData.IsTile())
			{
				packetData = JsonConvert.DeserializeObject<TilePacketData>(jsonData);
			}
			else
			{
				packetData = JsonConvert.DeserializeObject<MyPlayerPacketData>(jsonData);
			}

			dataManager.Save(packetData);
			return packetData;
		}

		private static GroupType GetGroupType(string groupName)
		{
			if (Enum.TryParse(groupName, out GroupType groupType) == false)
				return GroupType.None;

			return groupType;
		}

		private static string GetPostJsonData(HttpListenerRequest request)
		{
			if (!request.HasEntityBody)
			{
				return string.Empty;
			}
			using (Stream body = request.InputStream) // here we have data
			{
				using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
				{
					return reader.ReadToEnd();
				}
			}
		}

		private static GroupType GetCommandGroupType(HttpListenerRequest request)
		{
			if (request.Url == null)
				return GroupType.None;

			string groupTypeStr = request.Url.AbsolutePath.Replace("/", "");
			return GetGroupType(groupTypeStr);
		}

		private static IEnumerable<string> GetGetParameters(HttpListenerRequest request)
		{
			if (request.Url == null)
				yield break;

			string query = request.Url.Query;
			foreach (var p1 in query.Split('&'))
			{
				string[] p2 = p1.Split('=');
				yield return HttpUtility.UrlDecode(p2[1], Encoding.UTF8);
			}
		}
	}
}
