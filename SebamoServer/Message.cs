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
			return string.Format(format, data.name, data.weeklyPoint, SebamoData.MaxWeeklyPoint);
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
			return string.Format(format, data.name, data.weeklyPoint, SebamoData.MaxWeeklyPoint);
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
			return string.Format(format, data.name, data.weeklyPoint, SebamoData.MaxWeeklyPoint);
		}
	}

	public class ConfirmMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class ResetWeeklyMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class EndWeeklyMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class HelpMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class SendMoneyMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class ConfirmFeeMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class UseFeeMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}

	public class ResetFeeMessage : IMessage
	{
		public string GetFormat()
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(SebamoData data)
		{
			throw new NotImplementedException();
		}
	}
}
