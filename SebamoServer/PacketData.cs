﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SebamoServer
{

	[System.Serializable]
	public class PlayerPacketData : PacketData
	{
		public PlayerSyncPacketData syncData;

		public string[] hasItems; // 가진 아이템
		public int[] hasItemCounts; // 가진 아이템 개수

		public string[] appliedBuffItems; // 적용 중인 버프 아이템
	}

	[System.Serializable]
	public class PlayerSyncPacketData : PacketData
	{
		public bool isMine;

		public string playerName; // 플레이어 이름

		public int playerTileIndex; // 현재 타일 인덱스
		public int hasDiceCount; // 가진 주사위 개수

		public string[] equippedItems; // 장착 중인 아이템

	}

	[System.Serializable]
	public class PacketData
	{

	}
}
