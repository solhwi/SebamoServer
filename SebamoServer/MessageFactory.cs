using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	public class MessageFactory
	{
		public static IMessage MakeMessage(CommandType type, Dictionary<string, SebamoData> dataDictionary)
		{ 
			switch (type)
			{
				case CommandType.Complete:
					return new CompleteMessage();

				case CommandType.RollbackComplete:
					return new RollBackCompleteMessage();

				case CommandType.Rest:
					return new RestMessage();

				case CommandType.Confirm:
					return new ConfirmMessage();

				case CommandType.ResetWeekly:
					return new ResetWeeklyMessage();

				case CommandType.EndWeekly:
					return new EndWeeklyMessage();

				case CommandType.Help:
					return new HelpMessage();

				case CommandType.SendMoney:
					return new SendMoneyMessage();

				case CommandType.ConfirmFee:
					return new ConfirmFeeMessage();

				case CommandType.UseFee:
					return new UseFeeMessage();

				case CommandType.ResetFee:
					return new ResetFeeMessage();
		}

			return null;
		}
	}
}
