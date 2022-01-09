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
		private readonly short[] child = new short[Constants.TreeSize];
		private readonly short[] parent = new short[Constants.TreeSize];
		private readonly short[] block = new short[Constants.TreeSize];
		private readonly short[] edge = new short[Constants.TreeSize];
		private readonly short[] stock = new short[Constants.TreeSize];
		private readonly short[] sNode = new short[Constants.TreeSize / 2];

		private readonly ushort[] freq = new ushort[Constants.TreeSize];

		private ushort totalP;
		private int avail;
		private int n1;
		private int mostP;
		private int nn;
		private uint nextCount;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void StartCDyn()
		{
			int i, j;

			n1 = (nMax >= 256 + maxMatch - Constants.Threshold + 1) ? 512 : (int)nMax - 1;

			for (i = 0; i < Constants.TreeSizeC; i++)
			{
				stock[i] = (short)i;
				block[i] = 0;
			}

			for (i = 0, j = (int)nMax * 2 - 2; i < nMax; i++, j--)
			{
				freq[j] = 1;
				child[j] = (short)~i;
				sNode[i] = (short)j;
				block[j] = 1;
			}

			avail = 2;
			edge[1] = (short)(nMax - 1);
			i = (int)nMax * 2 - 2;

			while (j >= 0)
			{
				int f = freq[j] = (ushort)(freq[i] + freq[i - 1]);
				child[j] = (short)i;
				parent[i] = parent[i - 1] = (short)j;

				if (f == freq[j + 1])
					edge[block[j] = block[j + 1]] = (short)j;
				else
					edge[block[j] = stock[avail++]] = (short)j;

				i -= 2;
				j--;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void StartPDyn()
		{
			freq[Constants.RootP] = 1;
			child[Constants.RootP] = ~Constants.NChar;
			sNode[Constants.NChar] = Constants.RootP;
			edge[block[Constants.RootP]] = stock[avail++] = Constants.RootP;
			mostP = Constants.RootP;
			totalP = 0;
			nn = 1 << dicBit;
			nextCount = 64;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void DecodeStartDyn()
		{
			nMax = 286;
			maxMatch = Constants.MaxMatch;
			InitGetBits();
			StartCDyn();
			StartPDyn();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Reconst(int start, int end)
		{
			int i, j, b = 0;

			for (i = j = start; i < end; i++)
			{
				int k = child[i];
				if (k < 0)
				{
					freq[j] = (ushort)((freq[i] + 1) / 2);
					child[j] = (short)k;
					j++;
				}

				b = block[i];
				if (edge[b] == i)
					stock[--avail] = (short)b;
			}

			j--;
			i = end - 1;
			int l = end - 2;
			uint f;

			while (i >= start)
			{
				while (i >= l)
				{
					freq[i] = freq[j];
					child[i] = child[j];
					i--;
					j--;
				}

				f = (uint)(freq[l] + freq[l + 1]);

				int k;
				for (k = start; f < freq[k]; k++)
				{
				}

				while (j >= k)
				{
					freq[i] = freq[j];
					child[i] = child[j];
					i--;
					j--;
				}

				freq[i] = (ushort)f;
				child[i] = (short)(l + 1);
				i--;
				l -= 2;
			}

			f = 0;
			for (i = start; i < end; i++)
			{
				j = child[i];
				if (j < 0)
					sNode[~j] = (short)i;
				else
					parent[j] = parent[j - 1] = (short)i;

				uint g = freq[i];
				if (g == f)
					block[i] = (short)b;
				else
				{
					edge[b = block[i] = stock[avail++]] = (short)i;
					f = g;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private short SwapInc(short p)
		{
			short b = block[p];
			short q = edge[b];
			bool doIt = false;

			if (q != p)		// Swap for leader
			{
				short r = child[p];
				short s = child[q];
				child[p] = s;
				child[q] = r;

				if (r >= 0)
					parent[r] = parent[r - 1] = q;
				else
					sNode[~r] = q;

				if (s >= 0)
					parent[s] = parent[s - 1] = p;
				else
					sNode[~s] = p;

				p = q;
				doIt = true;
			}

			if (doIt || (b == block[p + 1]))
			{
				edge[b]++;
				if (++freq[p] == freq[p - 1])
					block[p] = block[p - 1];
				else
					edge[block[p] = stock[avail++]] = p;	// Create block
			}
			else if (++freq[p] == freq[p - 1])
			{
				stock[--avail] = b;							// Delete block
				block[p] = block[p - 1];
			}

			return parent[p];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateC(int p)
		{
			if (freq[Constants.RootC] == 0x8000)
				Reconst(0, (int)nMax * 2 - 1);

			freq[Constants.RootC]++;
			short q = sNode[p];

			do
			{
				q = SwapInc(q);
			}
			while (q != Constants.RootC);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void UpdateP(int p)
		{
			if (totalP == 0x8000)
			{
				Reconst(Constants.RootP, mostP + 1);
				totalP = freq[Constants.RootP];
				freq[Constants.RootP] = 0xffff;
			}

			int q = sNode[p + Constants.NChar];
			while (q != Constants.RootP)
				q = SwapInc((short)q);

			totalP++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MakeNewNode(int p)
		{
			int r = mostP + 1;
			int q = r + 1;

			sNode[~(child[r] = child[mostP])] = (short)r;
			child[q] = (short)~(p + Constants.NChar);
			child[mostP] = (short)q;
			freq[r] = freq[mostP];
			freq[q] = 0;
			block[r] = block[mostP];

			if (mostP == Constants.RootP)
			{
				freq[Constants.RootP] = 0xffff;
				edge[block[Constants.RootP]]++;
			}

			parent[r] = parent[q] = (short)mostP;
			edge[block[q] = stock[avail++]] = sNode[p + Constants.NChar] = (short)(mostP = (short)q);
			UpdateP(p);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodeCDyn()
		{
			int c = child[Constants.RootC];
			short buf = (short)bitBuf;
			short cnt = 0;

			do
			{
				c = child[c - (buf < 0 ? 1 : 0)];
				buf <<= 1;

				if (++cnt == 16)
				{
					FillBuf(16);
					buf = (short)bitBuf;
					cnt = 0;
				}
			}
			while (c > 0);

			FillBuf((byte)cnt);
			c = ~c;
			UpdateC(c);

			if (c == n1)
				c += GetBits(8);

			return (ushort)c;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ushort DecodePDyn()
		{
			while (count > nextCount)
			{
				MakeNewNode((int)(nextCount / 64));
				if ((nextCount += 64) >= nn)
					nextCount = 0xffffffff;
			}

			int c = child[Constants.RootP];
			short buf = (short)bitBuf;
			short cnt = 0;

			while (c > 0)
			{
				c = child[c - (buf < 0 ? 1 : 0)];
				buf <<= 1;

				if (++cnt == 16)
				{
					FillBuf(16);
					buf = (short)bitBuf;
					cnt = 0;
				}
			}

			FillBuf((byte)cnt);
			c = ~c - Constants.NChar;
			UpdateP(c);

			return (ushort)((c << 6) + GetBits(6));
		}
	}
}
