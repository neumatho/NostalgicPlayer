/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha.Containers;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		private static readonly int[][] fixedMethods =
		{
			new [] { 3, 0x01, 0x04, 0x0c, 0x18, 0x30, 0 },				// Old compatible
			new [] { 2, 0x01, 0x01, 0x03, 0x06, 0x0d, 0x1f, 0x4e, 0 }	// 8k buf
		};

		private uint nMax;
		private ushort maxMatch;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartSt0()
		{
			nMax = 286;
			maxMatch = Constants.MaxMatch;
			InitGetBits();
			np = 1 << (Constants.MaxDicBit - 7);
			blockSize = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadyMade(int method)
		{
			int[] tbl = fixedMethods[method];
			int tblOffset = 0;

			int j = tbl[tblOffset++];
			uint weight = (uint)(1 << (16 - j));
			uint code = 0;

			for (int i = 0; i < np; i++)
			{
				while (tbl[tblOffset] == i)
				{
					j++;
					tblOffset++;
					weight >>= 1;
				}

				ptLen[i] = (byte)j;
				ptCode[i] = (ushort)code;
				code += weight;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadTreeC()
		{
			int i = 0;

			while (i < Constants.N1)
			{
				if (GetBits(1) != 0)
					cLen[i] = (byte)(GetBits(Constants.LenField) + 1);
				else
					cLen[i] = 0;

				if ((++i == 3) && (cLen[0] == 1) && (cLen[1] == 1) && (cLen[2] == 1))
				{
					int c = GetBits(Constants.CBit);

					Array.Clear(cLen, 0, Constants.N1);
					Array.Fill(cTable, (ushort)c, 0, 4096);
					return;
				}
			}

			MakeTable(Constants.N1, cLen, 12, cTable);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadTreeP()
		{
			int i = 0;

			while (i < Constants.Np)
			{
				ptLen[i] = (byte)GetBits(Constants.LenField);

				if ((++i == 3) && (ptLen[0] == 1) && (ptLen[1] == 1) && (ptLen[2] == 1))
				{
					int c = GetBits(Constants.MaxDicBit - 7);

					Array.Clear(cLen, 0, Constants.Np);
					Array.Fill(cTable, (ushort)c, 0, 256);
					return;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartFix()
		{
			nMax = 314;
			maxMatch = 60;
			InitGetBits();
			np = 1 << (12 - 6);
			StartCDyn();
			ReadyMade(0);
			MakeTable((short)np, ptLen, 8, ptTable);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodeCSt0()
		{
			if (blockSize == 0)		// Read block head
			{
				blockSize = GetBits(Constants.BufBits);	// Read block blocksize
				ReadTreeC();

				if (GetBits(1) != 0)
					ReadTreeP();
				else
					ReadyMade(1);

				MakeTable(Constants.Np, ptLen, 8, ptTable);
			}

			blockSize--;

			int j = cTable[bitBuf >> 4];
			if (j < Constants.N1)
				FillBuf(cLen[j]);
			else
			{
				FillBuf(12);
				int i = bitBuf;

				do
				{
					if ((short)i < 0)
						j = right[j];
					else
						j = left[j];

					i <<= 1;
				}
				while (j >= Constants.N1);

				FillBuf((byte)(cLen[j] - 12));
			}

			if (j == Constants.N1 - 1)
				j += GetBits(Constants.ExtraBits);

			return (ushort)j;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodePSt0()
		{
			int j = ptTable[bitBuf >> 8];

			if (j < np)
				FillBuf(ptLen[j]);
			else
			{
				FillBuf(8);
				int i = bitBuf;

				do
				{
					if ((short)i < 0)
						j = right[j];
					else
						j = left[j];

					i <<= 1;
				}
				while (j >= np);

				FillBuf((byte)(ptLen[j] - 8));
			}

			return (ushort)((j << 6) + GetBits(6));
		}
	}
}
