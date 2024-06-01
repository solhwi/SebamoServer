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

	
