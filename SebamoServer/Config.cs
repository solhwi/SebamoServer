using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{
	public class Config
	{
		public const int MaxWeeklyPoint = 4;

		public const char ReplySeparator = '|';

		private static readonly Dictionary<GroupType, int[]> feeDictionary = new Dictionary<GroupType, int[]>()
		{
			{ GroupType.Kahlua, new int[] { 3000, 5000, 10000, 30000 } },
			{ GroupType.Exp, new int[] { 1000, 3000, 5000, 10000 } }
		};

		private static Dictionary<GroupType, string[]> groupMemberNameDictionary = new Dictionary<GroupType, string[]>()
		{
			{ GroupType.Kahlua, new string[] {"유진", "민준", "주훈", "나경", "솔휘"} },
			{ GroupType.Exp, new string[] {"동현", "상훈", "지홍", "지현", "솔휘", "강욱"} },
		};

		private static Dictionary<GroupType, string> groupAccountDictionary = new Dictionary<GroupType, string>()
		{
			{ GroupType.Kahlua, "케이뱅크 100191000123" },
			{ GroupType.Exp, "카카오뱅크 79795861723" }
		};

		private static Dictionary<GroupType, string> groupDataPathDictionary = new Dictionary<GroupType, string>()
		{
			{ GroupType.Kahlua, $"{Directory.GetCurrentDirectory()}/Resources/Kahlua" },
			{ GroupType.Exp,  $"{Directory.GetCurrentDirectory()}/Resources/Exp" },
		};

		public static string ScriptRootPath = $"{Directory.GetCurrentDirectory()}/Resources/Default";

		public static IEnumerable<string> GetNamesWithoutMe(GroupType groupType, string myName)
		{
			if (groupMemberNameDictionary.TryGetValue(groupType, out string[] names))
			{ 
				foreach(var name in names)
				{
					if (name == myName)
						continue;

					yield return name;
				}
			}
		}

		public static string GetDataRootPath(GroupType groupType)
		{
			return groupDataPathDictionary[groupType];
		}

		public static bool IsInGroup(GroupType groupType, string name)
		{
			if (groupMemberNameDictionary.TryGetValue(groupType, out string[] names))
			{
				return names.Contains(name);
			}

			return false;
		}

		public static int GetPenaltyFee(GroupType groupType, int penaltyPoint)
		{
			if (feeDictionary.TryGetValue(groupType, out int[] array))
			{
				if (0 < penaltyPoint && penaltyPoint <= MaxWeeklyPoint)
				{
					return array[penaltyPoint - 1];
				}

			}

			return 0;
		}

		public static string GetAccountString(GroupType groupType)
		{
			if (groupAccountDictionary.TryGetValue(groupType, out string accountStr))
			{
				return accountStr;
			}

			return string.Empty;
		}

		public static int GetMaxWeeklyPoint(GroupType groupType)
		{
			if (feeDictionary.TryGetValue(groupType, out int[] array))
			{
				return array.Length;
			}

			return 0;
		}
	}
}

	
