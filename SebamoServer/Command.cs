using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	public enum CommandType
	{
		None = -1,
		Complete, // 완료
		RollbackComplete, // 완료복원
		Rest, // 휴식
		Confirm, // 확인
		ResetWeekly, // 주간 초기화
		EndWeekly, // 한주끝
		Help, // 도움말
		SendMoney, // 송금
		ConfirmFee, // 벌금현황
		UseFee, // 벌금사용
		ResetFee, // 벌금초기화
	}

	public class Command
	{
		public readonly GroupType groupType = GroupType.None;
		public readonly CommandType commandType = CommandType.None;

		public Command(GroupType groupType, CommandType commandType)
		{
			this.groupType = groupType;
			this.commandType = commandType;
		}
	}

	public class NameCommand : Command
	{
		public readonly string name = string.Empty;

		public NameCommand(GroupType groupType, CommandType commandType, string name) : base(groupType, commandType)
		{
			this.name = name;
		}
	}

	public class NameAndFeeCommand : NameCommand
	{
		public readonly int fee = 0;

		public NameAndFeeCommand(GroupType groupType, CommandType commandType, string name, int fee) : base(groupType, commandType, name)
		{
			this.fee = fee;
		}
	}
}
