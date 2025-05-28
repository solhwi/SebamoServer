using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	/// <summary>
	/// 입력받은 커맨드에 적절한 행동을 취하고 응답을 돌려줌
	/// </summary>
	internal class CommandExecutor
	{
		private SpreadSheetManager spreadSheetManager = new SpreadSheetManager();

		private Dictionary<CommandType, Func<Command, Task<SebamoData>>> taskDictionary = null;
		
		private Dictionary<string, SebamoData> oldDataDictionary = new Dictionary<string, SebamoData>();
		private Dictionary<string, SebamoData> newDataDictionary = new Dictionary<string, SebamoData>();

		public CommandExecutor()
		{
			taskDictionary = new Dictionary<CommandType, Func<Command, Task<SebamoData>>>
			{
				{ CommandType.Complete, OnComplete },
				{ CommandType.RollbackComplete, OnRollbackComplete },
				{ CommandType.Rest, OnRest },
				{ CommandType.ResetWeekly, OnResetWeekly },
				{ CommandType.EndWeekly, OnEndWeekly },
				{ CommandType.SendMoney, OnSendMoney },
				{ CommandType.UseFee, OnUseFee },
				{ CommandType.ResetFee, OnResetFee },
			};
		}

		public async Task<string> MakeResponse(Command command)
		{
			oldDataDictionary = await GetSebamoData(command);
			newDataDictionary.Clear();

			return await ProcessResponse(command);
		}

		private async Task<string> ProcessResponse(Command command)
		{
			SebamoData data = null;
			if (taskDictionary.TryGetValue(command.commandType, out var doTask))
			{
				data = await doTask(command);
			}

			return MessageFactory.MakeMessage(command, oldDataDictionary, newDataDictionary)
				.ReadMessage(data)
				.Replace(" ", Config.TrimSeparator)
				.Replace("\n", Config.LineSeparator);
		}

		private async Task<Dictionary<string, SebamoData>> GetSebamoData(Command command)
		{
			return await spreadSheetManager.GetSebamoData(command.groupType);
		}

		private SebamoData GetRowSebamoData(NameCommand command)
		{
			if (oldDataDictionary.TryGetValue(command.name, out var value) == false)
				return null;

			return value.Clone();
		}

		private async Task<SebamoData> OnComplete(Command command)
		{
			if (command is NameCommand nameCommand)
			{
				newDataDictionary = Clone(oldDataDictionary);

				var rowData = GetRowSebamoData(nameCommand);
				if (rowData != null)
				{
					rowData.AddWeeklyPoint(1);

					newDataDictionary[rowData.name] = rowData;

					await spreadSheetManager.UpdateSebamoData(command.groupType, rowData);
				}
				
				return rowData;
			}

			return null;
		}

		private async Task<SebamoData> OnRollbackComplete(Command command)
		{
			if (command is NameCommand nameCommand)
			{
				newDataDictionary = Clone(oldDataDictionary);

				var rowData = GetRowSebamoData(nameCommand);
				if (rowData != null)
				{
					rowData.AddWeeklyPoint(-1);

					newDataDictionary[rowData.name] = rowData;

					await spreadSheetManager.UpdateSebamoData(command.groupType, rowData);
				}

				return rowData;
			}

			return null;
		}

		private async Task<SebamoData> OnRest(Command command)
		{
			if (command is NameCommand nameCommand)
			{
				newDataDictionary = Clone(oldDataDictionary);

				var rowData = GetRowSebamoData(nameCommand);
				if (rowData != null)
				{
					rowData.CompleteWeeklyPoint();

					newDataDictionary[rowData.name] = rowData;

					await spreadSheetManager.UpdateSebamoData(command.groupType, rowData);
				}

				return rowData;
			}

			return null;
		}

		private async Task<SebamoData> OnResetWeekly(Command command)
		{
			if (oldDataDictionary != null)
			{
				newDataDictionary.Clear();
				foreach (var sebamoData in oldDataDictionary.Values)
				{
					var newSebamoData = sebamoData.Clone();
					newSebamoData.ResetWeeklyPoint();

					newDataDictionary.Add(newSebamoData.name, newSebamoData);
				}

				await spreadSheetManager.UpdateSebamoData(command.groupType, newDataDictionary);
			}

			return null;
		}

		private async Task<SebamoData> OnEndWeekly(Command command)
		{
			if (oldDataDictionary != null)
			{
				newDataDictionary.Clear();
				foreach (var sebamoData in oldDataDictionary.Values)
				{
					var newSebamoData = sebamoData.Clone();
					newSebamoData.EndWeekly(command.groupType);

					newDataDictionary.Add(newSebamoData.name, newSebamoData);
				}

				GameDataManager.EndWeekly(command.groupType, oldDataDictionary);
				await spreadSheetManager.UpdateSebamoData(command.groupType, newDataDictionary);
			}

			return null;
		}

		private async Task<SebamoData> OnSendMoney(Command command)
		{
			if (command is NameAndFeeCommand nameAndFeeCommand)
			{
				newDataDictionary = Clone(oldDataDictionary);

				var rowData = GetRowSebamoData(nameAndFeeCommand);
				if (rowData != null)
				{
					var newSebamoData = rowData.Clone();
					newSebamoData.SendMoney(nameAndFeeCommand.fee);

					newDataDictionary[newSebamoData.name] = newSebamoData;

					await spreadSheetManager.UpdateSebamoData(command.groupType, newSebamoData);

					return newSebamoData;
				}
			}

			return null;
		}

		private async Task<SebamoData> OnUseFee(Command command)
		{
			if (command is NameAndFeeCommand nameAndFeeCommand)
			{
				newDataDictionary = Clone(oldDataDictionary);

				var rowData = GetRowSebamoData(nameAndFeeCommand);
				if (rowData != null)
				{
					var newSebamoData = rowData.Clone();
					newSebamoData.UsePenaltyFee(nameAndFeeCommand.fee);

					newDataDictionary[newSebamoData.name] = newSebamoData;

					await spreadSheetManager.UpdateSebamoData(command.groupType, newSebamoData);

					return newSebamoData;
				}
			}

			return null;
		}

		private async Task<SebamoData> OnResetFee(Command command)
		{
			if (oldDataDictionary != null)
			{
				newDataDictionary.Clear();
				foreach (var sebamoData in oldDataDictionary.Values)
				{
					var newSebamoData = sebamoData.Clone();
					newSebamoData.ResetPenaltyFee();

					newDataDictionary.Add(newSebamoData.name, newSebamoData);
				}

				await spreadSheetManager.UpdateSebamoData(command.groupType, newDataDictionary);
			}

			return null;
		}

		private Dictionary<string, SebamoData> Clone(Dictionary<string, SebamoData> src)
		{
			var dic = new Dictionary<string, SebamoData>();

            foreach (var iter in src)
            {
				dic.Add(iter.Key, iter.Value);
            }

			return dic;
        }
	}
}
