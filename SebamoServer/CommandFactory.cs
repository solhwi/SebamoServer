using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SebamoServer
{
	/// <summary>
	/// 입력받은 http request에 맞는 커맨드를 생성해냄
	/// ex. http://localhost:8000/Kaluha?p1=완료&p2=유진
	/// </summary>z
	internal class CommandFactory
	{
		public static Command MakeCommand(HttpListenerRequest request)
		{
			var commandGroupType = GetCommandGroupType(request);
			var commandParameters = GetCommandParameters(request).ToArray();

		    return MakeCommand_Internal(commandGroupType, commandParameters);
		}

		private static Command MakeCommand_Internal(GroupType groupType, string[] commandParameters)
		{
			if (commandParameters == null || commandParameters.Length < 1)
				return null;

			// 1번 인자가 커맨드 타입
			var commandType = GetCommandType(commandParameters[0]);

			switch (commandType)
			{
				case CommandType.Confirm:
				case CommandType.ResetWeekly:
				case CommandType.Help:
				case CommandType.ResetFee:
				case CommandType.ConfirmFee:
				case CommandType.EndWeekly:
					return new Command(groupType, commandType);

				case CommandType.Complete:
				case CommandType.RollbackComplete:
				case CommandType.Rest:

					if (commandParameters.Length < 2)
						return null;

					string name = commandParameters[1];

					if (IsInGroup(groupType, name) == false)
						return null;

					return new NameCommand(groupType, commandType, name);

				case CommandType.UseFee:
				case CommandType.SendMoney:

					if (commandParameters.Length < 2)
						return null;

					string name2 = commandParameters[1];

					if (IsInGroup(groupType, name2) == false)
						return null;

					if (int.TryParse(commandParameters[2], out int fee2) == false)
						return null;

					return new NameAndFeeCommand(groupType, commandType, name2, fee2);
			}

			return null;
		}

		private static CommandType GetCommandType(string commandType)
		{
			switch (commandType)
			{
				case "/완료":
					return CommandType.Complete;

				case "/완료복원":
					return CommandType.RollbackComplete;

				case "/휴식":
					return CommandType.Rest;

				case "/확인":
					return CommandType.Confirm;

				case "/한주초기화":
					return CommandType.ResetWeekly;

				case "/한주끝":
					return CommandType.EndWeekly;

				case "/도움말":
					return CommandType.Help;

				case "/송금":
					return CommandType.SendMoney;

				case "/벌금현황":
					return CommandType.ConfirmFee;

				case "/벌금사용":
					return CommandType.UseFee;

				case "/벌금초기화":
					return CommandType.ResetFee;
			}

			return CommandType.None;
		}

		private static bool IsInGroup(GroupType groupType, string name)
		{
			return Config.IsInGroup(groupType, name);
		}

		private static GroupType GetCommandGroupType(HttpListenerRequest request) 
		{
			if (request.Url == null)
				return GroupType.None;

			string groupTypeStr = request.Url.AbsolutePath.Replace("/", "");

			if (Enum.TryParse<GroupType>(groupTypeStr, out GroupType groupType) == false)
				return GroupType.None;

			return groupType;
		}

		private static IEnumerable<string> GetCommandParameters(HttpListenerRequest request)
		{
			if (request.Url == null)
				yield break;

			string query = request.Url.Query;
			foreach (var p1 in query.Split('&'))
			{
				string[] p2 = p1.Split('=');
				if (p2.Length == 2)
				{
					yield return HttpUtility.UrlDecode(p2[1], Encoding.UTF8);
				}
			}
		}
	}
}
