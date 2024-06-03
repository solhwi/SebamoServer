using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	public class MessageFactory
	{
		public static IMessage MakeMessage(Command command, Dictionary<string, SebamoData> oldDataDictionary, Dictionary<string, SebamoData> newDataDictionary)
		{ 
			switch (command.commandType)
			{
				case CommandType.Complete:
					return new CompleteMessage();

				case CommandType.RollbackComplete:
					return new RollBackCompleteMessage();

				case CommandType.Rest:
					return new RestMessage();

				case CommandType.Confirm:
					return new ConfirmMessage(oldDataDictionary);

				case CommandType.ResetWeekly:
					return new ResetWeeklyMessage();

				case CommandType.EndWeekly:
					return new EndWeeklyMessage(command, oldDataDictionary, newDataDictionary);

				case CommandType.Help:
					return new HelpMessage();

				case CommandType.SendMoney:
					return new SendMoneyMessage();

				case CommandType.ConfirmFee:
					return new ConfirmFeeMessage(command, oldDataDictionary);

				case CommandType.UseFee:

					if (command is NameAndFeeCommand nafCommand)
					{
						return new UseFeeMessage(nafCommand, newDataDictionary);
					}

					return null;
					
				case CommandType.ResetFee:
					return new ResetFeeMessage();
		}

			return null;
		}
	}
}
