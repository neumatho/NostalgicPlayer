/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
{
	/// <summary>
	/// Discrete Cosine Transform (DCT) for subband synthesis
	/// </summary>
	internal class Dct64
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Dct64(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void DoDct64(Memory<Real> out0, Memory<Real> out1, Memory<Real> samples)
		{
			Real[] bufs = new Real[64];

			{
				Span<Real> b1 = samples.Span;
				int b1Offset = 0;
				Real[] bs = bufs;
				int bsOffset = 0;
				Real[] cosTab = lib.tabInit.Pnts[0];
				int cosTabOffset = 16;
				Span<Real> b2 = b1;
				int b2Offset = 32;

				for (c_int i = 15; i >= 0; i--)
					bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];

				for (c_int i = 15; i >= 0; i--)
					bs[bsOffset++] = Helpers.Real_Mul(b2[--b2Offset] - b1[b1Offset++], cosTab[--cosTabOffset]);

				b1 = bufs.AsSpan();
				b1Offset = 0;
				cosTab = lib.tabInit.Pnts[1];
				cosTabOffset = 8;
				b2 = b1;
				b2Offset = b1Offset + 16;

				{
					for (c_int i = 7; i >= 0; i--)
						bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];

					for (c_int i = 7; i >= 0; i--)
						bs[bsOffset++] = Helpers.Real_Mul(b2[--b2Offset] - b1[b1Offset++], cosTab[--cosTabOffset]);

					b2Offset += 32;
					cosTabOffset += 8;

					for (c_int i = 7; i >= 0; i--)
						bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];

					for (c_int i = 7; i >= 0; i--)
						bs[bsOffset++] = Helpers.Real_Mul(b1[b1Offset++] - b2[--b2Offset], cosTab[--cosTabOffset]);

					b2Offset += 32;
				}

				bs = bufs;
				bsOffset = 0;
				cosTab = lib.tabInit.Pnts[2];
				b2 = b1;
				b2Offset = b1Offset + 8;

				for (c_int j = 2; j != 0; j--)
				{
					for (c_int i = 3; i >= 0; i--)
						bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];

					for (c_int i = 3; i >= 0; i--)
						bs[bsOffset++] = Helpers.Real_Mul(b2[--b2Offset] - b1[b1Offset++], cosTab[i]);

					b2Offset += 16;

					for (c_int i = 3; i >= 0; i--)
						bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];

					for (c_int i = 3; i >= 0; i--)
						bs[bsOffset++] = Helpers.Real_Mul(b1[b1Offset++] - b2[--b2Offset], cosTab[i]);

					b2Offset += 16;
				}

				b1 = bufs.AsSpan();
				b1Offset = 0;
				cosTab = lib.tabInit.Pnts[3];
				b2 = b1;
				b2Offset = b1Offset + 4;

				for (c_int j = 4; j != 0; j--)
				{
					bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];
					bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];
					bs[bsOffset++] = Helpers.Real_Mul(b2[--b2Offset] - b1[b1Offset++], cosTab[1]);
					bs[bsOffset++] = Helpers.Real_Mul(b2[--b2Offset] - b1[b1Offset++], cosTab[0]);

					b2Offset += 8;

					bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];
					bs[bsOffset++] = b1[b1Offset++] + b2[--b2Offset];
					bs[bsOffset++] = Helpers.Real_Mul(b1[b1Offset++] - b2[--b2Offset], cosTab[1]);
					bs[bsOffset++] = Helpers.Real_Mul(b1[b1Offset++] - b2[--b2Offset], cosTab[0]);

					b2Offset += 8;
				}

				bs = bufs;
				bsOffset = 0;
				cosTab = lib.tabInit.Pnts[4];

				for (c_int j = 8; j != 0; j--)
				{
					Real v0 = b1[b1Offset++];
					Real v1 = b1[b1Offset++];

					bs[bsOffset++] = v0 + v1;
					bs[bsOffset++] = Helpers.Real_Mul(v0 - v1, cosTab[0]);

					v0 = b1[b1Offset++];
					v1 = b1[b1Offset++];

					bs[bsOffset++] = v0 + v1;
					bs[bsOffset++] = Helpers.Real_Mul(v1 - v0, cosTab[0]);
				}
			}

			{
				Real[] b1;
				int b1Offset;
				c_int i;

				for (b1 = bufs, b1Offset = 0, i = 8; i != 0; i--, b1Offset += 4)
					b1[b1Offset + 2] += b1[b1Offset + 3];

				for (b1 = bufs, b1Offset = 0, i = 4; i != 0; i--, b1Offset += 8)
				{
					b1[b1Offset + 4] += b1[b1Offset + 6];
					b1[b1Offset + 6] += b1[b1Offset + 5];
					b1[b1Offset + 5] += b1[b1Offset + 7];
				}

				for (b1 = bufs, b1Offset = 0, i = 2; i != 0; i--, b1Offset += 16)
				{
					b1[b1Offset + 8] += b1[b1Offset + 12];
					b1[b1Offset + 12] += b1[b1Offset + 10];
					b1[b1Offset + 10] += b1[b1Offset + 14];
					b1[b1Offset + 14] += b1[b1Offset + 9];
					b1[b1Offset + 9] += b1[b1Offset + 13];
					b1[b1Offset + 13] += b1[b1Offset + 11];
					b1[b1Offset + 11] += b1[b1Offset + 15];
				}
			}

			Span<Real> o0 = out0.Span;
			Span<Real> o1 = out1.Span;

			o0[0x10 * 16] = Helpers.Real_Scale_Dct64(bufs[0]);
			o0[0x10 * 15] = Helpers.Real_Scale_Dct64(bufs[16 + 0] + bufs[16 + 8]);
			o0[0x10 * 14] = Helpers.Real_Scale_Dct64(bufs[8]);
			o0[0x10 * 13] = Helpers.Real_Scale_Dct64(bufs[16 + 8] + bufs[16 + 4]);
			o0[0x10 * 12] = Helpers.Real_Scale_Dct64(bufs[4]);
			o0[0x10 * 11] = Helpers.Real_Scale_Dct64(bufs[16 + 4] + bufs[16 + 12]);
			o0[0x10 * 10] = Helpers.Real_Scale_Dct64(bufs[12]);
			o0[0x10 * 9] = Helpers.Real_Scale_Dct64(bufs[16 + 12] + bufs[16 + 2]);
			o0[0x10 * 8] = Helpers.Real_Scale_Dct64(bufs[2]);
			o0[0x10 * 7] = Helpers.Real_Scale_Dct64(bufs[16 + 2] + bufs[16 + 10]);
			o0[0x10 * 6] = Helpers.Real_Scale_Dct64(bufs[10]);
			o0[0x10 * 5] = Helpers.Real_Scale_Dct64(bufs[16 + 10] + bufs[16 + 6]);
			o0[0x10 * 4] = Helpers.Real_Scale_Dct64(bufs[6]);
			o0[0x10 * 3] = Helpers.Real_Scale_Dct64(bufs[16 + 6] + bufs[16 + 14]);
			o0[0x10 * 2] = Helpers.Real_Scale_Dct64(bufs[14]);
			o0[0x10 * 1] = Helpers.Real_Scale_Dct64(bufs[16 + 14] + bufs[16 + 1]);
			o0[0x10 * 0] = Helpers.Real_Scale_Dct64(bufs[1]);

			o1[0x10 * 0] = Helpers.Real_Scale_Dct64(bufs[1]);
			o1[0x10 * 1] = Helpers.Real_Scale_Dct64(bufs[16 + 1] + bufs[16 + 9]);
			o1[0x10 * 2] = Helpers.Real_Scale_Dct64(bufs[9]);
			o1[0x10 * 3] = Helpers.Real_Scale_Dct64(bufs[16 + 9] + bufs[16 + 5]);
			o1[0x10 * 4] = Helpers.Real_Scale_Dct64(bufs[5]);
			o1[0x10 * 5] = Helpers.Real_Scale_Dct64(bufs[16 + 5] + bufs[16 + 13]);
			o1[0x10 * 6] = Helpers.Real_Scale_Dct64(bufs[13]);
			o1[0x10 * 7] = Helpers.Real_Scale_Dct64(bufs[16 + 13] + bufs[16 + 3]);
			o1[0x10 * 8] = Helpers.Real_Scale_Dct64(bufs[3]);
			o1[0x10 * 9] = Helpers.Real_Scale_Dct64(bufs[16 + 3] + bufs[16 + 11]);
			o1[0x10 * 10] = Helpers.Real_Scale_Dct64(bufs[11]);
			o1[0x10 * 11] = Helpers.Real_Scale_Dct64(bufs[16 + 11] + bufs[16 + 7]);
			o1[0x10 * 12] = Helpers.Real_Scale_Dct64(bufs[7]);
			o1[0x10 * 13] = Helpers.Real_Scale_Dct64(bufs[16 + 7] + bufs[16 + 15]);
			o1[0x10 * 14] = Helpers.Real_Scale_Dct64(bufs[15]);
			o1[0x10 * 15] = Helpers.Real_Scale_Dct64(bufs[16 + 15]);
		}
	}
}
