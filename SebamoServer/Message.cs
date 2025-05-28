using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	public interface IMessage
	{
		string GetFormat();
		string ReadMessage(SebamoData data);
	}

	public class CompleteMessage : IMessage
	{
		public string GetFormat()
		{
			return "{0} 님이 하루 치의 성취를 달성하였어요. ({1}/{2})";
		}

		public string ReadMessage(SebamoData data)
		{
			string format = GetFormat();
			return string.Format(format, data.name, data.weeklyPoint, Config.MaxWeeklyPoint);
		}
	}

	public class RollBackCompleteMessage : IMessage
	{
		public string GetFormat()
		{
			return "{0} 님이 하루 치의 성취를 복원하였어요. ({1}/{2})";
		}

		public string ReadMessage(SebamoData data)
		{
			string format = GetFormat();
			return string.Format(format, data.name, data.weeklyPoint, Config.MaxWeeklyPoint);
		}
	}

	public class RestMessage : IMessage
	{
		public string GetFormat()
		{
			return "{0} 님이 휴식을 취합니다. ({1}/{2})";
		}

		public string ReadMessage(SebamoData data)
		{
			string format = GetFormat();
			return string.Format(format, data.name, data.weeklyPoint, Config.MaxWeeklyPoint);
		}
	}

	public class ConfirmMessage : IMessage
	{
		private readonly Dictionary<string, SebamoData> dataDictionary = new Dictionary<string, SebamoData>();
		private const string toDoStr = "* 이번 주 To Do *\n\n";

		public ConfirmMessage(Dictionary<string, SebamoData> dataDictionary)
		{
			this.dataDictionary = dataDictionary;
		}

		public string GetFormat()
		{
			return "{0} ({1}/{2})\n";
		}

		public string ReadMessage(SebamoData data)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(toDoStr);

			foreach (var value in dataDictionary.Values)
			{
				string format = GetFormat();
				sb.Append(string.Format(format, value.name, value.weeklyPoint, Config.MaxWeeklyPoint));
			}

			return sb.ToString();
		}
	}

	public class ResetWeeklyMessage : IMessage
	{
		public string GetFormat()
		{
			return string.Empty;
		}

		public string ReadMessage(SebamoData data)
		{
			return "한 주 기록이 초기화되었습니다. 누적 벌금에는 영향이 가지 않습니다.";
		}
	}

	public class HelpMessage : IMessage
	{
		public string GetFormat()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("❓ 도움말\n\n");
			sb.Append("/완료 (이름) \n > (이름)의 일일 미션 완료를 기록합니다.\n\n");
			sb.Append("/완료복원 (이름) \n > 완료한 (이름)의 일일 미션을 일일 미션을 없던 일로 합니다.\n\n");
			sb.Append("/확인 \n > 본인 및 다른 사람의 진행 정도를 알려줍니다.\n\n");
			sb.Append("/한주초기화 \n > 주간 일일 미션들을 초기화할 수 있습니다. \n⚠️ 총무만 사용 가능합니다.\n\n");
			sb.Append("/한주끝 \n > 한 주의 기록을 초기화하고, 벌금을 환산합니다. \n⚠️ 총무만 사용 가능합니다.\n\n");
			sb.Append("/송금 (이름) (금액) \n > (이름)으로 (금액)만큼 송금합니다.\n\n");
			sb.Append("/휴식 (이름) \n > (이름)을 금주 휴식 처리합니다.\n\n");
			sb.Append("/벌금현황 \n > 전체 벌금표를 조회합니다.\n\n");
			sb.Append("/벌금사용 (이름) (금액) \n > 벌금을 사용하여 누적 금액에서 제합니다.\n\n");
			sb.Append("/벌금초기화 \n > 전체 벌금을 초기화합니다. \n⚠️ 총무만 사용 가능합니다.");

			return sb.ToString();
		}

		public string ReadMessage(SebamoData data)
		{
			return GetFormat();
		}
	}

	public class SendMoneyMessage : IMessage
	{
		public string GetFormat()
		{
			return "{0} 님의 벌금 잔액은 {1} 입니다.";
		}

		public string ReadMessage(SebamoData data)
		{
			string format = GetFormat();
			return string.Format(format, data.name, data.penaltyFee);
		}
	}

	public class ConfirmFeeMessage : IMessage
	{
		private readonly Command command = null;
		private readonly Dictionary<string, SebamoData> dataDictionary = new Dictionary<string, SebamoData>();
		
		private const string alertMessage = "💰 벌금표 💰\n\n";

		public ConfirmFeeMessage(Command command, Dictionary<string, SebamoData> dataDictionary)
		{
			this.command = command;
			this.dataDictionary = dataDictionary;
		}

		public string GetFormat()
		{
			return string.Empty;
		}

		public string ReadMessage(SebamoData data)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(alertMessage);

			int totalFee = GetAllTotalFee();

			foreach (var value in dataDictionary.Values)
			{
				sb.Append($"{value.name} (잔액 : {value.penaltyFee} 원)\n");
				sb.Append($"> 누적 금액 : {value.totalPenaltyFee}원\n");

				float percentage = value.totalPenaltyFee * 100.0f / totalFee;
				sb.Append($"> 공헌 비율 : {percentage.ToString("0.00")} %\n\n");
			}

			int usedFee = GetAllUsedFee();

			sb.Append($"총 누적 금액 : {totalFee}원\n");

			sb.Append($"사용 금액 : {usedFee}원\n");
			sb.Append($"남은 금액 : {totalFee - usedFee}원\n");

			string accountStr = Config.GetAccountString(command.groupType);
			sb.Append($"{accountStr} 으로 보내시면 됩니다.");

			return sb.ToString();
		}

		private int GetAllUsedFee()
		{
			int totalFee = 0;

			foreach (var value in dataDictionary.Values)
			{
				totalFee += value.usedPenaltyFee;
			}

			return totalFee;
		}

		private int GetAllTotalFee()
		{
			int totalFee = 0;

			foreach(var  value in dataDictionary.Values)
			{
				totalFee += value.totalPenaltyFee;
			}

			return totalFee;
		}
	}

	public class UseFeeMessage : IMessage
	{
		private NameAndFeeCommand command;
		private ConfirmFeeMessage confirmFeeMessage = null;

		public UseFeeMessage(NameAndFeeCommand command, Dictionary<string, SebamoData> newDataDictionary)
		{
			this.command = command;
			confirmFeeMessage = new ConfirmFeeMessage(command, newDataDictionary);
		}

		public string GetFormat()
		{
			return "(주의) {0} 님이 {1}원, 여태 총 {2}원 만큼 벌금을 사용했습니다.";
		}

		public string ReadMessage(SebamoData data)
		{
			if (command == null)
				return string.Empty;

			StringBuilder sb = new StringBuilder();

			string format = GetFormat();
			string message = string.Format(format, command.name, command.fee, data.usedPenaltyFee);

			sb.Append(message);
			sb.Append(Config.ReplySeparator);
			sb.Append(confirmFeeMessage.ReadMessage(data));

			return sb.ToString();
		}
	}

	public class ResetFeeMessage : IMessage
	{
		public string GetFormat()
		{
			return string.Empty;
		}

		public string ReadMessage(SebamoData data)
		{
			return "벌금 기록이 모두 초기화되었습니다.";
		}
	}

	public class EndWeeklyMessage : IMessage
	{
		private ConfirmMessage confirmMessage = null;
		private ConfirmFeeMessage confirmFeeMessage = null;

		public EndWeeklyMessage(Command command, Dictionary<string, SebamoData> oldDataDictionary, Dictionary<string, SebamoData> newDataDictionary)
		{
			confirmMessage = new ConfirmMessage(oldDataDictionary);
			confirmFeeMessage = new ConfirmFeeMessage(command, newDataDictionary);
		}

		public string GetFormat()
		{
			return "한 주 기록이 초기화되었습니다. 누적 벌금은 위와 같습니다.";
		}

		public string ReadMessage(SebamoData data)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(confirmMessage.ReadMessage(data));
			sb.Append(Config.ReplySeparator);
			sb.Append(confirmFeeMessage.ReadMessage(data));
			sb.Append(Config.ReplySeparator);
			sb.Append(GetFormat());

			return sb.ToString();
		}
	}

}
