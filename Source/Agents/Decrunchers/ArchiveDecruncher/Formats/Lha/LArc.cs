/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		private int flag;
		private int flagCnt;
		private int matchPos;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodeCLzs()
		{
			if (GetBits(1) != 0)
				return GetBits(8);

			matchPos = GetBits(11);
			return (ushort)(GetBits(4) + 0x100);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodePLzs()
		{
			return (ushort)((loc - matchPos - Constants.Magic0) & 0x7ff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartLzs()
		{
			InitGetBits();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodeCLz5()
		{
			if (flagCnt == 0)
			{
				flagCnt = 8;
				flag = stream.ReadByte();
			}

			flagCnt--;
			int c = stream.ReadByte();

			if ((flag & 1) == 0)
			{
				matchPos = c;
				c = stream.ReadByte();
				matchPos += (c & 0xf0) << 4;
				c &= 0x0f;
				c += 0x100;
			}

			flag >>= 1;

			return (ushort)c;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodePLz5()
		{
			return (ushort)((loc - matchPos - Constants.Magic5) & 0xfff);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartLz5()
		{
			flagCnt = 0;
		}
	}
}
