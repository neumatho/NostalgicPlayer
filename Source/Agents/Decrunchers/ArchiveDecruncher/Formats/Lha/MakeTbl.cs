/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.ArchiveDecruncher.Formats.Lha
{
	internal partial class LhaCore
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MakeTable(short nChar, byte[] bitLen, short tableBits, ushort[] table)
		{
			ushort[] count = new ushort[17];	// Count of bitLen
			ushort[] weight = new ushort[17];	// 0x100000ul >> bitLen
			ushort[] start = new ushort[17];	// First code of bitLen

			int avail = nChar;
			int i;

			// Initialize
			for (i = 1; i <= 16; i++)
			{
				count[i] = 0;
				weight[i] = (ushort)(1 << (16 - i));
			}

			// Count
			for (i = 0; i < nChar; i++)
				count[bitLen[i]]++;

			// Calculate first code
			ushort total = 0;
			for (i = 1; i <= 16; i++)
			{
				start[i] = total;
				total += (ushort)(weight[i] * count[i]);
			}

			if ((total & 0xffff) != 0)
				throw new DecruncherException(agentName, Resources.IDS_ARD_ERR_BAD_TABLE);

			// Shift data for make table
			int m = 16 - tableBits;
			for (i = 1; i <= tableBits; i++)
			{
				start[i] >>= m;
				weight[i] >>= m;
			}

			// Initialize
			int j = start[tableBits + 1] >> m;
			int k = 1 << tableBits;

			if (j != 0)
			{
				for (i = j; i < k; i++)
					table[i] = 0;
			}

			// Create table and tree
			for (j = 0; j < nChar; j++)
			{
				k = bitLen[j];
				if (k == 0)
					continue;

				int l = start[k] + weight[k];
				if (k <= tableBits)
				{
					// Code in table
					for (i = start[k]; i < l; i++)
						table[i] = (ushort)j;
				}
				else
				{
					// Code not in table
					ushort[] p = table;
					int pOffset = (i = start[k]) >> m;
					i <<= tableBits;
					int n = k - tableBits;

					// Make tree (n length)
					while (--n >= 0)
					{
						if (p[pOffset] == 0)
						{
							right[avail] = left[avail] = 0;
							p[pOffset] = (ushort)avail++;
						}

						if ((i & 0x8000) != 0)
						{
							pOffset = p[pOffset];
							p = right;
						}
						else
						{
							pOffset = p[pOffset];
							p = left;
						}

						i <<= 1;
					}

					p[pOffset] = (ushort)j;
				}

				start[k] = (ushort)l;
			}
		}
	}
}
