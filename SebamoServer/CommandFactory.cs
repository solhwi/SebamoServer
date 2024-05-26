using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SebamoServer
{
	public class Command
	{
		public readonly GroupType groupType = GroupType.None;
		public readonly string commandType = string.Empty;

		public Command(GroupType groupType, string commandType)
		{
			this.groupType = groupType;
			this.commandType = commandType;
		}
	}

	public class NameCommand : Command
	{
		public readonly string name = string.Empty;

		public NameCommand(GroupType groupType, string commandType, string name) : base(groupType, commandType)
		{
			this.name = name;
		}
	}

	public class FeeCommand : Command
	{
		public readonly int fee = 0;

		public FeeCommand(GroupType groupType, string commandType, int fee) : base(groupType, commandType)
		{
			this.fee = fee;
		}
	}

	public class NameAndFeeCommand : NameCommand
	{
		public readonly int fee = 0;

		public NameAndFeeCommand(GroupType groupType, string commandType, string name, int fee) : base(groupType, commandType, name)
		{
			this.fee = fee;
		}
	}


	/// <summary>
	/// 입력받은 http request에 맞는 커맨드를 생성해냄
	/// ex. http://localhost.com:8000/Kaluha?p1=완료&p2=유진
	/// </summary>
	internal class CommandFactory
	{
		/// <summary>
		/// 요기 group config로 빼기
		/// </summary>
		private static Dictionary<GroupType, string[]> groupMemberNameDictionary = new Dictionary<GroupType, string[]>()
		{ 
			{ GroupType.Kahlua, new string[] {"유진", "민준", "주훈", "나경", "솔휘"} },
			{ GroupType.Exp, new string[] {"동현", "상훈", "지홍", "지현", "솔휘", "강욱"} },
		};

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
			var commandType = commandParameters[0];

			switch (commandType)
			{
				case "확인":
				case "초기화":
				case "도움말":
				case "벌금초기화":
				case "벌금현황":
				case "한주끝":
					return new Command(groupType, commandType);

				case "완료":
				case "완료복원":
				case "휴식":

					if (commandParameters.Length < 2)
						return null;

					string name = commandParameters[1];

					if (IsInGroup(groupType, name) == false)
						return null;

					return new NameCommand(groupType, commandType, name);

				case "벌금사용":

					if (commandParameters.Length < 2)
						return null;

					string feeStr = commandParameters[1];

					if (int.TryParse(feeStr, out int fee) == false)
						return null;

					return new FeeCommand(groupType, commandType, fee);

				case "송금":

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

		private static bool IsInGroup(GroupType groupType, string name)
		{
			if (groupMemberNameDictionary.TryGetValue(groupType, out string[] names))
			{
				return names.Contains(name);
			}

			return false;
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
				yield return HttpUtility.UrlDecode(p2[1], Encoding.UTF8);
			}
		}
	}
}
