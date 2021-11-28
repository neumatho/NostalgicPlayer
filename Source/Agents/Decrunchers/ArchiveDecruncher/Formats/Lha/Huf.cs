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
		private readonly ushort[] left = new ushort[2 * Constants.Nc - 1];
		private readonly ushort[] right = new ushort[2 * Constants.Nc - 1];
		private readonly byte[] cLen = new byte[Constants.Nc];
		private readonly byte[] ptLen = new byte[Constants.Npt];
		private readonly ushort[] cTable = new ushort[4096];
		private readonly ushort[] ptTable = new ushort[256];
		private readonly ushort[] ptCode = new ushort[Constants.Npt];

		private ushort blockSize;
		private int pBit;
		private int np;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadPtLen(short nn, short nBit, short iSpecial)
		{
			int c;

			int n = GetBits((byte)nBit);
			if (n == 0)
			{
				c = GetBits((byte)nBit);

				Array.Clear(ptLen, 0, nn);
				Array.Fill(ptTable, (ushort)c, 0, 256);
			}
			else
			{
				int i = 0;
				while (i < n)
				{
					c = bitBuf >> (16 - 3);
					if (c == 7)
					{
						ushort mask = 1 << (16 - 4);
						while ((mask & bitBuf) != 0)
						{
							mask >>= 1;
							c++;
						}
					}

					FillBuf((byte)(c < 7 ? 3 : c - 3));
					ptLen[i++] = (byte)c;

					if (i == iSpecial)
					{
						c = GetBits(2);
						while (--c >= 0)
							ptLen[i++] = 0;
					}
				}

				while (i < nn)
					ptLen[i++] = 0;

				MakeTable(nn, ptLen, 8, ptTable);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadCLen()
		{
			short c;

			short n = (short)GetBits(Constants.CBit);
			if (n == 0)
			{
				c = (short)GetBits(Constants.CBit);

				Array.Clear(cLen, 0, Constants.Nc);
				Array.Fill(cTable, (ushort)c, 0, 4096);
			}
			else
			{
				short i = 0;

				while (i < n)
				{
					c = (short)ptTable[bitBuf >> (16 - 8)];
					if (c >= Constants.Nt)
					{
						ushort mask = 1 << (16 - 9);

						do
						{
							if ((bitBuf & mask) != 0)
								c = (short)right[c];
							else
								c = (short)left[c];

							mask >>= 1;
						}
						while (c >= Constants.Nt);
					}

					FillBuf(ptLen[c]);

					if (c <= 2)
					{
						if (c == 0)
							c = 1;
						else if (c == 1)
							c = (short)(GetBits(4) + 3);
						else
							c = (short)(GetBits(Constants.CBit) + 20);

						while (--c >= 0)
							cLen[i++] = 0;
					}
					else
						cLen[i++] = (byte)(c - 2);
				}

				while (i < Constants.Nc)
					cLen[i++] = 0;

				MakeTable(Constants.Nc, cLen, 12, cTable);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodeCSt1()
		{
			if (blockSize == 0)
			{
				blockSize = GetBits(16);
				ReadPtLen(Constants.Nt, Constants.TBit, 3);
				ReadCLen();
				ReadPtLen((short)np, (short)pBit, -1);
			}

			blockSize--;

			ushort j = cTable[bitBuf >> 4];

			if (j < Constants.Nc)
				FillBuf(cLen[j]);
			else
			{
				FillBuf(12);

				ushort mask = 1 << (16 - 1);

				do
				{
					if ((bitBuf & mask) != 0)
						j = right[j];
					else
						j = left[j];

					mask >>= 1;
				}
				while (j >= Constants.Nc);

				FillBuf((byte)(cLen[j] - 12));
			}

			return j;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodePSt1()
		{
			ushort j = ptTable[bitBuf >> (16 - 8)];
			if (j < np)
				FillBuf(ptLen[j]);
			else
			{
				FillBuf(8);

				ushort mask = 1 << (16 - 1);

				do
				{
					if ((bitBuf & mask) != 0)
						j = right[j];
					else
						j = left[j];

					mask >>= 1;
				}
				while (j >= np);

				FillBuf((byte)(ptLen[j] - 8));
			}

			if (j != 0)
				j = (ushort)((1 << (j - 1)) + GetBits((byte)(j - 1)));

			return j;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartSt1()
		{
			if (dicBit <= 13)
			{
				np = 14;
				pBit = 4;
			}
			else
			{
				if (dicBit == 16)
					np = 17;
				else
					np = 16;

				pBit = 5;
			}

			InitGetBits();
			blockSize = 0;
		}
	}
}
