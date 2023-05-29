/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers;
using Single = Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers.Single;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
{
	/// <summary>
	/// The layer 2 decoder
	/// </summary>
	internal class Layer2
	{
		private static readonly c_int[,,] translate = new c_int[3, 2, 16]
		{
			{
				{ 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 0 },
				{ 0, 2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }
			},
			{
				{ 0, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
				{ 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
			},
			{
				{ 0, 3, 3, 3, 3, 3, 3, 0, 0, 0, 1, 1, 1, 1, 1, 0 },
				{ 0, 3, 3, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }
			}
		};

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Layer2(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// The layer12_table is already in real format (fixed or float),
		/// just needs a little scaling in the MMX/SSE case
		/// </summary>
		/********************************************************************/
		public void Init_Layer12_Stuff(Mpg123_Handle fr, Func<Mpg123_Handle, Real[], int, Memory<Real>> init_Table)
		{
			for (c_int k = 0; k < 27; k++)
			{
				Memory<Real> table = init_Table(fr, fr.Muls[k], k);
				table.Span[0] = 0.0f;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Memory<Real> Init_Layer12_Table(Mpg123_Handle fr, Real[] table, c_int m)
		{
			c_int i;

			for (i = 0; i < 63; i++)
				table[i] = L12Tabs.Layer12_Table[m, i];

			return table.AsMemory(i);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Do_Layer2(Mpg123_Handle fr)
		{
			c_int clip = 0;
			c_int stereo = fr.Stereo;
			Memory<Real> fraction = fr.Layer2;
			c_uint[] bit_Alloc = new c_uint[64];
			c_int[] scale = new c_int[192];
			Single single = fr.Single;

			II_Select_Table(fr);

			fr.JsBound = (fr.Mode == Mode.Joint_Stereo) ? (fr.Mode_Ext << 2) + 4 : fr.II_SbLimit;

			if (fr.JsBound > fr.II_SbLimit)
				fr.JsBound = fr.II_SbLimit;

			// TODO: What happens with mono mixing, actually?
			if ((stereo == 1) || (single == Single.Mix))	// Also, mix not really handled
				single = Single.Left;

			if (II_Step_One(bit_Alloc, scale, fr) != 0)
				return clip;

			for (c_int i = 0; i < Constant.Scale_Block; i++)
			{
				II_Step_Two(bit_Alloc, fraction, scale, fr, i >> 2);

				if (fr.Bits_Avail < 0)
					return clip;

				for (c_int j = 0; j < 3; j++)
				{
					if (single != Single.Stereo)
						clip += fr.Synth_Mono(GetLayerBuffer(fraction, (int)single, j), fr);
					else
						clip += fr.Synth_Stereo(GetLayerBuffer(fraction, 0, j), GetLayerBuffer(fraction, 1, j), fr);
				}
			}

			return clip;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<Real> GetLayerBuffer(Memory<Real> buffer, int index1, int index2)
		{
			return buffer.Slice(index1 * 4 * Constant.SBLimit + index2 * Constant.SBLimit);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int II_Step_One(c_uint[] bit_Alloc, c_int[] scale, Mpg123_Handle fr)
		{
			c_int stereo = fr.Stereo - 1;
			c_int sbLimit = fr.II_SbLimit;
			c_int jsBound = fr.JsBound;
			c_int sbLimit2 = fr.II_SbLimit << stereo;
			Al_Table[] alloc1 = fr.alloc;
			int alloc1Offset = 0;
			c_uint[] scfsi_Buf = new c_uint[64];
			c_uint[] scfsi = scfsi_Buf;
			int scfsiOffset;
			c_int sc, step;

			// Count the bits needed for getbits_fast()
			c_uint needBits = 0;
			c_uint[] scale_Bits = { 18, 12, 6, 12 };

			c_uint[] bitA = bit_Alloc;
			int bitAOffset = 0;

			if (stereo != 0)
			{
				for (c_int i = jsBound; i != 0; i--, alloc1Offset += (1 << step))
				{
					step = alloc1[alloc1Offset].Bits;
					bitA[bitAOffset] = (c_uint)(c_char)lib.getBits.GetBits_(fr, step);
					bitA[bitAOffset + 1] = (c_uint)(c_char)lib.getBits.GetBits_(fr, step);
					needBits += (c_uint)(((bitA[bitAOffset] != 0 ? 1 : 0) + (bitA[bitAOffset + 1] != 0 ? 1 : 0)) * 2);
					bitAOffset += 2;
				}

				for (c_int i = sbLimit - jsBound; i != 0; i--, alloc1Offset += (1 << step))
				{
					step = alloc1[alloc1Offset].Bits;
					bitA[bitAOffset] = (c_uint)(c_char)lib.getBits.GetBits_(fr, step);
					bitA[bitAOffset + 1] = bitA[bitAOffset];
					needBits += (c_uint)((bitA[bitAOffset] != 0 ? 1 : 0) * 2 * 2);
					bitAOffset += 2;
				}

				bitAOffset = 0;
				scfsiOffset = 0;

				if (fr.Bits_Avail < needBits)
					return -1;

				for (c_int i = sbLimit2; i != 0; i--)
				{
					if (bitA[bitAOffset++] != 0)
						scfsi[scfsiOffset++] = (c_uint)(c_char)lib.getBits.GetBits_Fast(fr, 2);
				}
			}
			else
			{
				for (c_int i = sbLimit; i != 0; i--, alloc1Offset += (1 << step))
				{
					step = alloc1[alloc1Offset].Bits;
					bitA[bitAOffset] = (c_uint)(c_char)lib.getBits.GetBits_(fr, step);

					if (bitA[bitAOffset] != 0)
						needBits += 2;

					++bitAOffset;
				}

				bitAOffset = 0;
				scfsiOffset = 0;

				if (fr.Bits_Avail < needBits)
					return -1;

				for (c_int i = sbLimit; i != 0; i--)
				{
					if (bitA[bitAOffset++] != 0)
						scfsi[scfsiOffset++] = (c_uint)(c_char)lib.getBits.GetBits_Fast(fr, 2);
				}
			}

			needBits = 0;
			bitAOffset = 0;
			scfsiOffset = 0;

			for (c_int i = sbLimit2; i != 0; --i)
			{
				if (bitA[bitAOffset++] != 0)
					needBits += scale_Bits[scfsi[scfsiOffset++]];
			}

			if (fr.Bits_Avail < needBits)
				return -1;

			bitAOffset = 0;
			scfsiOffset = 0;
			int scaleOffset = 0;

			for (c_int i = sbLimit2; i != 0; --i)
			{
				if (bitA[bitAOffset++] != 0)
				{
					switch (scfsi[scfsiOffset++])
					{
						case 0:
						{
							scale[scaleOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							break;
						}

						case 1:
						{
							scale[scaleOffset++] = sc = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = sc;
							scale[scaleOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							break;
						}

						case 2:
						{
							scale[scaleOffset++] = sc = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = sc;
							scale[scaleOffset++] = sc;
							break;
						}

						default:
						{
							scale[scaleOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = sc = (c_int)lib.getBits.GetBits_Fast(fr, 6);
							scale[scaleOffset++] = sc;
							break;
						}
					}
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void II_Step_Two(c_uint[] bit_Alloc, Memory<Real> fraction, c_int[] scale, Mpg123_Handle fr, c_int x1)
		{
			c_int stereo = fr.Stereo;
			c_int sbLimit = fr.II_SbLimit;
			c_int jsBound = fr.JsBound;
			Al_Table[] alloc = fr.alloc;
			int alloc1Offset = 0;
			int alloc2Offset;
			c_uint[] bitA = bit_Alloc;
			int bitAOffset = 0;
			c_int step;
			int scaleOffset = 0;

			Memory<Real>[,] buffers = new Memory<Real>[2, 3]
			{
				{
					GetLayerBuffer(fraction, 0, 0), GetLayerBuffer(fraction, 0, 1), GetLayerBuffer(fraction, 0, 2)
				},
				{
					GetLayerBuffer(fraction, 1, 0), GetLayerBuffer(fraction, 1, 1), GetLayerBuffer(fraction, 1, 2)
				}
			};

			for (c_int i = 0; i < jsBound; i++, alloc1Offset += (1 << step))
			{
				step = alloc[alloc1Offset].Bits;

				for (c_int j = 0; j < stereo; j++)
				{
					c_int ba = (c_int)bitA[bitAOffset++];
					if (ba != 0)
					{
						alloc2Offset = alloc1Offset + ba;
						c_int k = alloc[alloc2Offset].Bits;

						c_int d1 = alloc[alloc2Offset].D;
						if (d1 < 0)
						{
							Real cm = fr.Muls[k][scale[scaleOffset + x1]];
							buffers[j, 0].Span[i] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1), cm);
							buffers[j, 1].Span[i] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1), cm);
							buffers[j, 2].Span[i] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1), cm);
						}
						else
						{
							c_uchar[][] table = { null, null, null, L12Tabs.Grp_3Tab, null, L12Tabs.Grp_5Tab, null, null, null, L12Tabs.Grp_9Tab };
							c_uint m = (c_uint)scale[scaleOffset + x1];
							c_uint idx = lib.getBits.GetBits_(fr, k);
							Span<c_uchar> tab = table[d1].AsSpan((int)(idx + idx + idx));
							buffers[j, 0].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[0]][m]);
							buffers[j, 1].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[1]][m]);
							buffers[j, 2].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[2]][m]);
						}

						scaleOffset += 3;
					}
					else
						buffers[j, 0].Span[i] = buffers[j, 1].Span[i] = buffers[j, 2].Span[i] = Helpers.Double_To_Real(0.0);

					if (fr.Bits_Avail < 0)
						return;		// Caller checks that again
				}
			}

			for (c_int i = jsBound; i < sbLimit; i++, alloc1Offset += (1 << step))
			{
				step = alloc[alloc1Offset].Bits;
				bitAOffset++;	// Channel 1 and channel 2 bitalloc are the same

				c_int ba = (c_int)bitA[bitAOffset++];
				if (ba != 0)
				{
					alloc2Offset = alloc1Offset + ba;
					c_int k = alloc[alloc2Offset].Bits;

					c_int d1 = alloc[alloc2Offset].D;
					if (d1 < 0)
					{
						Real cm = fr.Muls[k][scale[scaleOffset + x1 + 3]];
						buffers[0, 0].Span[i] = Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1);
						buffers[0, 1].Span[i] = Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1);
						buffers[0, 2].Span[i] = Helpers.Double_To_Real_15((c_int)lib.getBits.GetBits_(fr, k) + d1);
						buffers[1, 0].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 0].Span[i], cm);
						buffers[1, 1].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 1].Span[i], cm);
						buffers[1, 2].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 2].Span[i], cm);
						cm = fr.Muls[k][scale[scaleOffset + x1]];
						buffers[0, 0].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 0].Span[i], cm);
						buffers[0, 1].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 1].Span[i], cm);
						buffers[0, 2].Span[i] = Helpers.Real_Mul_Scale_Layer12(buffers[0, 2].Span[i], cm);
					}
					else
					{
						c_uchar[][] table = { null, null, null, L12Tabs.Grp_3Tab, null, L12Tabs.Grp_5Tab, null, null, null, L12Tabs.Grp_9Tab };
						c_uint m1 = (c_uint)scale[scaleOffset + x1];
						c_uint m2 = (c_uint)scale[scaleOffset + x1 + 3];
						c_uint idx = lib.getBits.GetBits_(fr, k);
						Span<c_uchar> tab = table[d1].AsSpan((int)(idx + idx + idx));
						buffers[0, 0].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[0]][m1]);
						buffers[1, 0].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[0]][m2]);
						buffers[0, 1].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[1]][m1]);
						buffers[1, 1].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[1]][m2]);
						buffers[0, 2].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[2]][m1]);
						buffers[1, 2].Span[i] = Helpers.Real_Scale_Layer12(fr.Muls[tab[2]][m2]);
					}

					scaleOffset += 6;

					if (fr.Bits_Avail < 0)
						return;		// Caller checks that again
				}
				else
				{
					buffers[0, 0].Span[i] = buffers[0, 1].Span[i] = buffers[0, 2].Span[i] =
					buffers[1, 0].Span[i] = buffers[1, 1].Span[i] = buffers[1, 2].Span[i] =
					Helpers.Double_To_Real(0.0);
				}
			}

			if (sbLimit > fr.Down_Sample_SbLimit)
				sbLimit = fr.Down_Sample_SbLimit;

			for (c_int i = sbLimit; i < Constant.SBLimit; i++)
			{
				for (c_int j = 0; j < stereo; j++)
					buffers[j, 0].Span[i] = buffers[j, 1].Span[i] = buffers[j, 2].Span[i] = Helpers.Double_To_Real(0.0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void II_Select_Table(Mpg123_Handle fr)
		{
			c_int table;
			Al_Table[][] tables = { L2Tables.Alloc_0, L2Tables.Alloc_1, L2Tables.Alloc_2, L2Tables.Alloc_3, L2Tables.Alloc_4 };
			c_int[] sbLims = { 27, 30, 8, 12, 30 };

			if (fr.Sampling_Frequency >= 3)		// Or equivalent: (fr.lsf == 1)
				table = 4;
			else
				table = translate[fr.Sampling_Frequency, 2 - fr.Stereo, fr.Bitrate_Index];

			c_int sbLim = sbLims[table];
			fr.alloc = tables[table];
			fr.II_SbLimit = sbLim;
		}
		#endregion
	}
}
