using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	/// <summary>
	/// 입력받은 커맨드에 적절한 데이터 변경과 메시지를 생성함
	/// </summary>
	internal class CommandExecutor
	{
		private SpreadSheetManager spreadSheetManager = new SpreadSheetManager();

		/// <summary>
		/// message config로 빼기
		/// </summary>
		private Dictionary<string, string> messageFormatDictionary = new Dictionary<string, string>()
		{
			{ "완료", "{0} 님이 하루 치의 성취를 달성하였어요. ({1}/{2})"},
			{ "완료복원", "{0} 님이 하루 치의 성취를 복원하였어요. ({1}/{2})"},
			{ "휴식", "{0} 님이 휴식을 취합니다. ({1}/{2})"},
		};

		private const int MaxWeeklyPoint = 4;

		public async Task<string> MakeResponse(Command command)
		{
			return await UpdateAndReply(command);
		}

		private async Task<string> UpdateAndReply(Command command)
		{
			string commandType = command.commandType;

			if (messageFormatDictionary.TryGetValue(commandType, out var messageFormat) == false)
				return string.Empty;

			switch (commandType)
			{
				case "완료":
				case "완료복원":
				case "휴식":

					if (command is NameCommand nameCommand)
					{
						var rowData = await GetRowSebamoData(nameCommand);
						return string.Format(messageFormat, nameCommand.name, rowData.weeklyPoint, MaxWeeklyPoint);
					}

					break;	
			}

			return string.Empty;
		}

		private async Task<Dictionary<string, SebamoData>> GetSebamoData(Command command)
		{
			return await spreadSheetManager.GetSebamoData(command.groupType);
		}

		private async Task<SebamoData> GetRowSebamoData(NameCommand command)
		{
			var sebamoDataDictionary = await GetSebamoData(command);
			if (sebamoDataDictionary == null)
				return null;

			if (sebamoDataDictionary.TryGetValue(command.name, out var value) == false)
				return null;

			return value;
		}
	}
}
