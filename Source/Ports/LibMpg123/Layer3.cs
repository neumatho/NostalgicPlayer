/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Single = Polycode.NostalgicPlayer.Ports.LibMpg123.Containers.Single;
using Mask_SType = System.Int32;
using Mask_Type = System.UInt32;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
    /// <summary>
    /// The layer 3 decoder
    /// </summary>
    internal class Layer3
	{
		/// <summary>
		/// Decoder state data
		/// </summary>
		private class Gr_Info_S
		{
			public c_int Scfsi;
			public c_uint Part2_3_Length;
			public c_uint Big_Values;
			public c_uint ScaleFac_Compress;
			public c_uint Block_Type;
			public c_uint Mixed_Block_Flag;
			public readonly c_uint[] Table_Select = new c_uint[3];

			// Making those two signed int as workaround for open64/pathscale/sun
			// compilers, and also for consistency, since they're worked on together with
			// other signed variables
			public readonly c_int[] MaxBand = new c_int[3];
			public c_int MaxBandL;
			public c_uint MaxB;
			public c_uint Region1Start;
			public c_uint Region2Start;
			public c_uint PreFlag;
			public c_uint ScaleFac_Scale;
			public c_uint Count1Table_Select;
			public readonly Memory<Real>[] Full_Gain = new Memory<Real>[3];
			public Memory<Real> Pow2Gain;
		}

		private class III_SideInfo
		{
			public c_uint Main_Data_Begin;
			public c_uint Private_Bits;
			public Gr_Info_S[,] Ch_Gr = ArrayHelper.Initialize2DArray<Gr_Info_S>(2, 2);
		}

		private static readonly c_uchar[,] sLen =
		{
			{ 0, 0, 0, 0, 3, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4 },
			{ 0, 1, 2, 3, 0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 2, 3 }
		};

		private static readonly c_uchar[,][] sTab =
		{
			{
				new c_uchar[] { 6, 5, 5, 5 }, new c_uchar[] { 6, 5, 7, 3 }, new c_uchar[] { 11, 10, 0, 0},
				new c_uchar[] { 7, 7, 7, 0 }, new c_uchar[] { 6, 6, 6, 3 }, new c_uchar[] {  8, 8, 5, 0}
			},
			{
				new c_uchar[] { 9, 9, 9, 9 }, new c_uchar[] { 9, 9, 12, 6 }, new c_uchar[] { 18, 18, 0, 0},
				new c_uchar[] { 12, 12, 12, 0 }, new c_uchar[] { 12, 9, 9, 6 }, new c_uchar[] { 15, 12, 9, 0}
			},
			{
				new c_uchar[] { 6, 9, 9, 9 }, new c_uchar[] { 6, 9, 12, 6 }, new c_uchar[] { 15, 18, 0, 0},
				new c_uchar[] { 6, 15, 12, 0 }, new c_uchar[] { 6, 12, 9, 6 }, new c_uchar[] { 6, 18, 9, 0}
			}
		};

		private static readonly c_uchar[][] preTab_Choice =
		{
			new c_uchar[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			new c_uchar[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3, 3, 3, 2, 0 }
		};

		private const int BitShift = (sizeof(Mask_Type) - 1) * 8;

		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Layer3(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Real Int123_Init_Layer3_GainPow2(Mpg123_Handle fr, c_int i)
		{
			return Helpers.Double_To_Real_Scale_Layer3(Math.Pow(2.0, -0.25 * (i + 210)), i + 256);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Init_Layer3_Stuff(Mpg123_Handle fr, Func<Mpg123_Handle, c_int, Real> gainPow2_Func)
		{
			for (c_int i = -256; i < 118 + 4; i++)
				fr.GainPow2[i + 256] = gainPow2_Func(fr, i);

			for (c_int j = 0; j < 9; j++)
			{
				for (c_int i = 0; i < 23; i++)
				{
					fr.LongLimit[j, i] = (L3BandGain.BandInfo[j].LongIdx[i] - 1 + 8) / 18 + 1;

					if (fr.LongLimit[j, i] > fr.Down_Sample_SbLimit)
						fr.LongLimit[j, i] = fr.Down_Sample_SbLimit;
				}

				for (c_int i = 0; i < 14; i++)
				{
					fr.ShortLimit[j, i] = (L3BandGain.BandInfo[j].ShortIdx[i] - 1) / 18 + 1;

					if (fr.ShortLimit[j, i] > fr.Down_Sample_SbLimit)
						fr.ShortLimit[j, i] = fr.Down_Sample_SbLimit;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Int123_Do_Layer3(Mpg123_Handle fr)
		{
			c_int clip = 0;
			c_int[][] scaleFacs = ArrayHelper.Initialize2Arrays<c_int>(2, 39);	// Max 39 for short[13][3] mode, mixed: 38, long: 22
			III_SideInfo sideInfo = new III_SideInfo();
			c_int stereo = fr.Stereo;
			Single single = fr.Single;
			c_int ms_Stereo, i_Stereo;
			c_int sFreq = fr.Hdr.Sampling_Frequency;
			c_int stereo1;

			if (stereo == 1)
			{	// Stream is mono
				stereo1 = 1;
				single = Single.Left;
			}
			else if (single != Single.Stereo)	// Stream is stereo, but force to mono
				stereo1 = 1;
			else
				stereo1 = 2;

			if (fr.Hdr.Mode == Mode.Joint_Stereo)
			{
				ms_Stereo = (fr.Hdr.Mode_Ext & 0x2) >> 1;
				i_Stereo = fr.Hdr.Mode_Ext & 0x1;
			}
			else
				ms_Stereo = i_Stereo = 0;

			c_int granules = fr.Hdr.Lsf != 0 ? 1 : 2;

			// Quick hack to keep the music playing
			// after having seen this nasty test file...
			if (III_Get_Side_Info(fr, sideInfo, stereo, ms_Stereo, sFreq, single) != 0)
				return clip;

			lib.parse.Int123_Set_Pointer(fr, true, (c_long)sideInfo.Main_Data_Begin);

			if (fr.PInfo != null)
			{
				fr.PInfo.MainData = (c_int)sideInfo.Main_Data_Begin;
				fr.PInfo.Padding = fr.Hdr.Padding;
			}

			for (c_int gr = 0; gr < granules; gr++)
			{
				Memory<Real> hybridIn = fr.Layer3.Hybrid_In;
				Memory<Real> hybridOut = fr.Layer3.Hybrid_Out;

				Span<Real> hybridIn0 = GetHybridInBuffer(hybridIn, 0).Span;
				Span<Real> hybridIn1 = GetHybridInBuffer(hybridIn, 1).Span;

				{
					Gr_Info_S gr_Info = sideInfo.Ch_Gr[0, gr];

					if (gr_Info.Part2_3_Length > fr.Bits_Avail)
						return clip;

					c_long part2Bits;

					if (fr.Hdr.Lsf != 0)
						part2Bits = III_Get_Scale_Factors_2(fr, scaleFacs[0], gr_Info, 0);
					else
						part2Bits = III_Get_Scale_Factors_1(fr, scaleFacs[0], gr_Info, 0, gr);

					if (part2Bits < 0)
						return clip;

					if (fr.PInfo != null)
					{
						fr.PInfo.SfBits[gr, 0] = part2Bits;

						for (c_int i = 0; i < 39; ++i)
							fr.PInfo.Sfb_S[gr, 0, i] = scaleFacs[0][i];
					}

					if (III_Dequantize_Sample(fr, hybridIn0, scaleFacs[0], gr_Info, sFreq, part2Bits) != 0)
						return clip;

					if (fr.Bits_Avail < 0)
						return clip;
				}

				if (stereo == 2)
				{
					Gr_Info_S gr_Info = sideInfo.Ch_Gr[1, gr];

					c_long part2Bits;

					if (fr.Hdr.Lsf != 0)
						part2Bits = III_Get_Scale_Factors_2(fr, scaleFacs[1], gr_Info, i_Stereo);
					else
						part2Bits = III_Get_Scale_Factors_1(fr, scaleFacs[1], gr_Info, 1, gr);

					if (part2Bits < 0)
						return clip;

					if (fr.PInfo != null)
					{
						fr.PInfo.SfBits[gr, 1] = part2Bits;

						for (c_int i = 0; i < 39; ++i)
							fr.PInfo.Sfb_S[gr, 1, i] = scaleFacs[1][i];
					}

					if (III_Dequantize_Sample(fr, hybridIn1, scaleFacs[1], gr_Info, sFreq, part2Bits) != 0)
						return clip;

					if (fr.Bits_Avail < 0)
						return clip;

					if (ms_Stereo != 0)
					{
						c_uint maxB = sideInfo.Ch_Gr[0, gr].MaxB;

						if (sideInfo.Ch_Gr[1, gr].MaxB > maxB)
							maxB = sideInfo.Ch_Gr[1, gr].MaxB;

						for (c_int i = 0; i < Constant.SSLimit * maxB; i++)
						{
							Real tmp0 = hybridIn0[i];
							Real tmp1 = hybridIn1[i];

							hybridIn0[i] = tmp0 + tmp1;
							hybridIn1[i] = tmp0 - tmp1;
						}
					}

					if (i_Stereo != 0)
						III_I_Stereo(hybridIn, scaleFacs[1], gr_Info, sFreq, ms_Stereo, fr.Hdr.Lsf);

					if ((ms_Stereo != 0) || (i_Stereo != 0) || (single == Single.Mix))
					{
						if (gr_Info.MaxB > sideInfo.Ch_Gr[0, gr].MaxB)
							sideInfo.Ch_Gr[0, gr].MaxB = gr_Info.MaxB;
						else
							gr_Info.MaxB = sideInfo.Ch_Gr[0, gr].MaxB;
					}

					switch (single)
					{
						case Single.Mix:
						{
							Span<Real> in0 = hybridIn0;
							Span<Real> in1 = hybridIn1;
							int in0Offset = 0;
							int in1Offset = 0;

							for (c_int i = 0; i < Constant.SSLimit * gr_Info.MaxB; i++, in0Offset++)
								in0[in0Offset] = in0[in0Offset] + in1[in1Offset++];	// *0.5 done by pow-scale

							break;
						}

						case Single.Right:
						{
							Span<Real> in0 = hybridIn0;
							Span<Real> in1 = hybridIn1;
							int in0Offset = 0;
							int in1Offset = 0;

							for (c_int i = 0; i < Constant.SSLimit * gr_Info.MaxB; i++)
								in0[in0Offset++] = in1[in1Offset++];

							break;
						}
					}
				}

				if (fr.PInfo != null)
					Fill_PInfo_Side(fr, sideInfo, gr, stereo1);

				for (c_int ch = 0; ch < stereo1; ch++)
				{
					Gr_Info_S gr_Info = sideInfo.Ch_Gr[ch, gr];

					III_AntiAlias(GetHybridInBuffer(hybridIn, ch).Span, gr_Info);
					III_Hybrid(GetHybridInBuffer(hybridIn, ch), GetHybridOutBuffer(hybridOut, ch), ch, gr_Info, fr);
				}

				for (c_int ss = 0; ss < Constant.SSLimit; ss++)
				{
					if (single != Single.Stereo)
						clip += fr.Synth_Mono(GetHybridOutBuffer(hybridOut, 0, ss), fr);
					else
						clip += fr.Synth_Stereo(GetHybridOutBuffer(hybridOut, 0, ss), GetHybridOutBuffer(hybridOut, 1, ss), fr);
				}
			}

			return clip;
		}



		/********************************************************************/
		/// <summary>
		/// This is an optimized DCT from Jeff Tsay's maplay 1.2+ package.
		/// Saved one multiplication by doing the 'twiddle factor' stuff
		/// together with the window mul. (MH)
		///
		/// This uses Byeong Gi Lee's Fast Cosine Transform algorithm, but
		/// the 9 point IDCT needs to be reduced further. Unfortunately, I
		/// don't know how to do that, because 9 is not an even number.
		/// - Jeff.
		///
		/// Original Message:
		///
		/// 9 Point Inverse Discrete Cosine Transform
		///
		/// This piece of code is Copyright 1997 Mikko Tommila and is
		/// freely usable by anybody. The algorithm itself is of course in
		/// the public domain.
		///
		/// Again derived heuristically from the 9-point WFTA.
		///
		/// The algorithm is optimized (?) for speed, not for small rounding
		/// errors or good readability.
		///
		/// 36 additions, 11 multiplications
		///
		/// Again this is very likely sub-optimal.
		///
		/// The code is optimized to use a minimum number of temporary
		/// variables, so it should compile quite well even on 8-register
		/// Intel x86 processors. This makes the code quite obfuscated and
		/// very difficult to understand.
		///
		/// References:
		/// [1] S. Winograd: "On Computing the Discrete Fourier Transform",
		/// Mathematics of Computation, Volume 32, Number 141, January 1978,
		/// Pages 175-199
		/// </summary>
		/********************************************************************/
		public void Int123_Dct36(Memory<Real> inBuf, Memory<Real> o1, Memory<Real> o2, Real[] winTab, Memory<Real> tsBuf)
		{
			Real[] tmp = new Real[18];

			{
				Span<Real> in_ = inBuf.Span;

				in_[17] += in_[16]; in_[16] += in_[15]; in_[15] += in_[14];
				in_[14] += in_[13]; in_[13] += in_[12]; in_[12] += in_[11];
				in_[11] += in_[10]; in_[10] += in_[9];  in_[9]  += in_[8];
				in_[8]  += in_[7];  in_[7]  += in_[6];  in_[6]  += in_[5];
				in_[5]  += in_[4];  in_[4]  += in_[3];  in_[3]  += in_[2];
				in_[2]  += in_[1];  in_[1]  += in_[0];

				in_[17] += in_[15]; in_[15] += in_[13]; in_[13] += in_[11]; in_[11] += in_[9];
				in_[9]  += in_[7];  in_[7]  += in_[5];  in_[5]  += in_[3];  in_[3]  += in_[1];

				{
					Real t3;

					{
						Real t0 = Helpers.Real_Mul(Constant.Cos6_2, (in_[8] + in_[16] - in_[4]));
						Real t1 = Helpers.Real_Mul(Constant.Cos6_2, in_[12]);

						t3 = in_[0];
						Real t2 = t3 - t1 - t1;
						tmp[1] = tmp[7] = t2 - t0;
						tmp[4] = t2 + t0 + t0;
						t3 += t1;

						t2 = Helpers.Real_Mul(Constant.Cos6_1, (in_[10] + in_[14] - in_[2]));
						tmp[1] -= t2;
						tmp[7] += t2;
					}
					{
						Real t0 = Helpers.Real_Mul(L3Tabs.Cos9[0], (in_[4] + in_[8]));
						Real t1 = Helpers.Real_Mul(L3Tabs.Cos9[1], (in_[8] - in_[16]));
						Real t2 = Helpers.Real_Mul(L3Tabs.Cos9[2], (in_[4] + in_[16]));

						tmp[2] = tmp[6] = t3 - t0 - t2;
						tmp[0] = tmp[8] = t3 + t0 + t1;
						tmp[3] = tmp[5] = t3 - t1 + t2;
					}
				}
				{
					Real t1 = Helpers.Real_Mul(L3Tabs.Cos18[0], (in_[2] + in_[10]));
					Real t2 = Helpers.Real_Mul(L3Tabs.Cos18[1], (in_[10] - in_[14]));
					Real t3 = Helpers.Real_Mul(Constant.Cos6_1, in_[6]);

					{
						Real t0 = t1 + t2 + t3;
						tmp[0] += t0;
						tmp[8] -= t0;
					}

					t2 -= t3;
					t1 -= t3;

					t3 = Helpers.Real_Mul(L3Tabs.Cos18[2], (in_[2] + in_[14]));

					t1 += t3;
					tmp[3] += t1;
					tmp[5] -= t1;

					t2 -= t3;
					tmp[2] += t2;
					tmp[6] -= t2;
				}

				{
					Real t0, t1, t2, t3, t4, t5, t6, t7;

					t1 = Helpers.Real_Mul(Constant.Cos6_2, in_[13]);
					t2 = Helpers.Real_Mul(Constant.Cos6_2, (in_[9] + in_[17] - in_[5]));

					t3 = in_[1] + t1;
					t4 = in_[1] - t1 - t1;
					t5 = t4 - t2;

					t0 = Helpers.Real_Mul(L3Tabs.Cos9[0], (in_[5] + in_[9]));
					t1 = Helpers.Real_Mul(L3Tabs.Cos9[1], (in_[9] - in_[17]));

					tmp[13] = Helpers.Real_Mul((t4 + t2 + t2), L3Tabs.Int123_TfCos36[17 - 13]);
					t2 = Helpers.Real_Mul(L3Tabs.Cos9[2], (in_[5] + in_[17]));

					t6 = t3 - t0 - t2;
					t0 += t3 + t1;
					t3 += t2 - t1;

					t2 = Helpers.Real_Mul(L3Tabs.Cos18[0], (in_[3] + in_[11]));
					t4 = Helpers.Real_Mul(L3Tabs.Cos18[1], (in_[11] - in_[15]));
					t7 = Helpers.Real_Mul(Constant.Cos6_1, in_[7]);

					t1 = t2 + t4 + t7;
					tmp[17] = Helpers.Real_Mul((t0 + t1), L3Tabs.Int123_TfCos36[17 - 17]);
					tmp[9] = Helpers.Real_Mul((t0 - t1), L3Tabs.Int123_TfCos36[17 - 9]);
					t1 = Helpers.Real_Mul(L3Tabs.Cos18[2], (in_[3] + in_[15]));
					t2 += t1 - t7;

					tmp[14] = Helpers.Real_Mul((t3 + t2), L3Tabs.Int123_TfCos36[17 - 14]);
					t0 = Helpers.Real_Mul(Constant.Cos6_1, (in_[11] + in_[15] - in_[3]));
					tmp[12] = Helpers.Real_Mul((t3 - t2), L3Tabs.Int123_TfCos36[17 - 12]);

					t4 -= t1 + t7;

					tmp[16] = Helpers.Real_Mul((t5 - t0), L3Tabs.Int123_TfCos36[17 - 16]);
					tmp[10] = Helpers.Real_Mul((t5 + t0), L3Tabs.Int123_TfCos36[17 - 10]);
					tmp[15] = Helpers.Real_Mul((t6 + t4), L3Tabs.Int123_TfCos36[17 - 15]);
					tmp[11] = Helpers.Real_Mul((t6 - t4), L3Tabs.Int123_TfCos36[17 - 11]);
				}

				{
					Real[] w = winTab;

					void Macro(c_int v)
					{
						Span<Real> out2 = o2.Span;
						Span<Real> out1 = o1.Span;
						Span<Real> ts = tsBuf.Span;

						Real tmpVal = tmp[v] + tmp[17 - v];

						out2[9 + v] = Helpers.Real_Mul(tmpVal, w[27 + v]);
						out2[8 - v] = Helpers.Real_Mul(tmpVal, w[26 - v]);

						tmpVal = tmp[v] - tmp[17 - v];

						ts[Constant.SBLimit * (8 - v)] = out1[8 - v] + Helpers.Real_Mul(tmpVal, w[8 - v]);
						ts[Constant.SBLimit * (9 + v)] = out1[9 + v] + Helpers.Real_Mul(tmpVal, w[9 + v]);
					}

					Macro(0);
					Macro(1);
					Macro(2);
					Macro(3);
					Macro(4);
					Macro(5);
					Macro(6);
					Macro(7);
					Macro(8);
				}
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<Real> GetHybridInBuffer(Memory<Real> buffer, int index)
		{
			return buffer.Slice(index * Constant.SBLimit * Constant.SSLimit);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<Real> GetHybridInBuffer(Memory<Real> buffer, int index1, int index2)
		{
			return buffer.Slice(index1 * Constant.SBLimit * Constant.SSLimit + index2 * Constant.SSLimit);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<Real> GetHybridOutBuffer(Memory<Real> buffer, int index)
		{
			return buffer.Slice(index * Constant.SSLimit * Constant.SBLimit);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<Real> GetHybridOutBuffer(Memory<Real> buffer, int index1, int index2)
		{
			return buffer.Slice(index1 * Constant.SSLimit * Constant.SBLimit + index2 * Constant.SBLimit);
		}



		/********************************************************************/
		/// <summary>
		/// Read additional side information (for MPEG 1 and MPEG 2)
		/// </summary>
		/********************************************************************/
		private c_int III_Get_Side_Info(Mpg123_Handle fr, III_SideInfo si, c_int stereo, c_int ms_Stereo, c_long sFreq, Single single)
		{
			c_int powDiff = (single == Single.Mix) ? 4 : 0;

			c_int[][] tabs =
			[
				[ 2, 9, 5, 3, 4 ],
				[ 1, 8, 1, 2, 9 ]
			];
			c_int[] tab = tabs[fr.Hdr.Lsf];

			{	// First ensure we got enough bits available
				c_uint needBits = 0;
				needBits += (c_uint)tab[1];	// main_data begin
				needBits += (c_uint)(stereo == 1 ? tab[2] : tab[3]);	// Private

				if (fr.Hdr.Lsf == 0)
					needBits += (c_uint)stereo * 4;		// Scfsi

				// For each granule for each channel ...
				needBits += (c_uint)(tab[0] * stereo * (29 + tab[4] + 1 + 22 + (fr.Hdr.Lsf == 0 ? 1 : 0) + 2));

				if (fr.Bits_Avail < needBits)
					return 1;
			}

			si.Main_Data_Begin = lib.getBits.GetBits_(fr, tab[1]);

			if (si.Main_Data_Begin > fr.BitReservoir)
			{
				// Overwrite main_data_begin for the really available bit reservoir
				lib.getBits.BackBits(fr, tab[1]);

				if (fr.Hdr.Lsf == 0)
				{
					fr.WordPointer[fr.WordPointerIndex] = (c_uchar)(fr.BitReservoir >> 1);
					fr.WordPointer[fr.WordPointerIndex + 1] = (c_uchar)((fr.BitReservoir & 1) << 7);
				}
				else
					fr.WordPointer[fr.WordPointerIndex] = (c_uchar)fr.BitReservoir;

				// Zero "side-info" data for a silence-frame
				// without touching audio data used as bit reservoir for following frame
				Array.Clear(fr.WordPointer, fr.WordPointerIndex + 2, fr.Hdr.SSize - 2);

				// Reread the new bit reservoir offset
				si.Main_Data_Begin = lib.getBits.GetBits_(fr, tab[1]);
			}

			// Keep track of the available data bytes for the bit reservoir.
			// CRC is included in ssize already
			fr.BitReservoir = (c_uint)(fr.BitReservoir + fr.Hdr.FrameSize - fr.Hdr.SSize);

			// Limit the reservoir to the max for MPEG 1.0 or 2.x
			if (fr.BitReservoir > (fr.Hdr.Lsf == 0 ? 511U : 255U))
				fr.BitReservoir = (fr.Hdr.Lsf == 0 ? 511U : 255U);

			// Now back into less commented territory. It's code. It works
			if (stereo == 1)
				si.Private_Bits = lib.getBits.GetBits_(fr, tab[2]);
			else
				si.Private_Bits = lib.getBits.GetBits_(fr, tab[3]);

			if (fr.Hdr.Lsf == 0)
			{
				for (c_int ch = 0; ch < stereo; ch++)
				{
					si.Ch_Gr[ch, 0].Scfsi = -1;
					si.Ch_Gr[ch, 1].Scfsi = (c_int)lib.getBits.GetBits_(fr, 4);
				}
			}

			for (c_int gr = 0; gr < tab[0]; gr++)
			{
				for (c_int ch = 0; ch < stereo; ch++)
				{
					Gr_Info_S gr_Info = si.Ch_Gr[ch, gr];

					gr_Info.Part2_3_Length = lib.getBits.GetBits_(fr, 12);
					gr_Info.Big_Values = lib.getBits.GetBits_(fr, 9);

					if (gr_Info.Big_Values > 288)
						gr_Info.Big_Values = 288;

					c_uint qss = lib.getBits.GetBits_Fast(fr, 8);

					gr_Info.Pow2Gain = fr.GainPow2.AsMemory((int)(256 - qss + powDiff));

					if (ms_Stereo != 0)
						gr_Info.Pow2Gain = gr_Info.Pow2Gain.Slice(2);

					if (fr.PInfo != null)
						fr.PInfo.Qss[gr, ch] = (int)qss;

					gr_Info.ScaleFac_Compress = lib.getBits.GetBits_(fr, tab[4]);

					if (gr_Info.Part2_3_Length == 0)
						gr_Info.ScaleFac_Compress = 0;

					// 22 bits for if/else block
					if (lib.getBits.GetBits_(fr, 1) != 0)
					{	// Window switch flag
						gr_Info.Block_Type = lib.getBits.GetBits_Fast(fr, 2);
						gr_Info.Mixed_Block_Flag = lib.getBits.Get1Bit(fr);
						gr_Info.Table_Select[0] = lib.getBits.GetBits_Fast(fr, 5);
						gr_Info.Table_Select[1] = lib.getBits.GetBits_Fast(fr, 5);

						// table_select[2] not needed, because there is no region2,
						// but to satisfy some verification tools we set it either
						gr_Info.Table_Select[2] = 0;

						for (c_int i = 0; i < 3; i++)
						{
							c_uint sbg = (lib.getBits.GetBits_Fast(fr, 3) << 3);
							gr_Info.Full_Gain[i] = gr_Info.Pow2Gain.Slice((int)sbg);

							if (fr.PInfo != null)
								fr.PInfo.Sub_Gain[gr, ch, i] = (int)(sbg / 8);
						}

						if (gr_Info.Block_Type == 0)
							return 1;

						// Region count/start parameters are implicit in this case
						if (((fr.Hdr.Lsf == 0) || (gr_Info.Block_Type == 2)) && !fr.Hdr.Mpeg25)
						{
							gr_Info.Region1Start = 36 >> 1;
							gr_Info.Region2Start = 576 >> 1;
						}
						else
						{
							if (fr.Hdr.Mpeg25)
							{
								c_int r0c;

								if ((gr_Info.Block_Type == 2) && (gr_Info.Mixed_Block_Flag == 0))
									r0c = 5;
								else
									r0c = 7;

								// r0c+1+r1c+1 == 22, always
								c_int r1c = 20 - r0c;

								gr_Info.Region1Start = (c_uint)(L3BandGain.BandInfo[sFreq].LongIdx[r0c + 1] >> 1);
								gr_Info.Region2Start = (c_uint)(L3BandGain.BandInfo[sFreq].LongIdx[r0c + 1 + r1c + 1] >> 1);
							}
							else
							{
								gr_Info.Region1Start = 54 >> 1;
								gr_Info.Region2Start = 576 >> 1;
							}
						}
					}
					else
					{
						for (c_int i = 0; i < 3; i++)
							gr_Info.Table_Select[i] = lib.getBits.GetBits_Fast(fr, 5);

						c_int r0c = (c_int)lib.getBits.GetBits_Fast(fr, 4);	// 0 .. 15
						c_int r1c = (c_int)lib.getBits.GetBits_Fast(fr, 3);	// 0 .. 7

						gr_Info.Region1Start = (uint)(L3BandGain.BandInfo[sFreq].LongIdx[r0c + 1] >> 1);

						// max(r0c+r1c+2) = 15+7+2 = 24
						if ((r0c + 1 + r1c + 1) > 22)
							gr_Info.Region2Start = 576 >> 1;
						else
							gr_Info.Region2Start = (c_uint)(L3BandGain.BandInfo[sFreq].LongIdx[r0c + 1 + r1c + 1] >> 1);

						gr_Info.Block_Type = 0;
						gr_Info.Mixed_Block_Flag = 0;
					}

					if (fr.Hdr.Lsf == 0)
						gr_Info.PreFlag = lib.getBits.Get1Bit(fr);

					gr_Info.ScaleFac_Scale = lib.getBits.Get1Bit(fr);
					gr_Info.Count1Table_Select = lib.getBits.Get1Bit(fr);
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int III_Get_Scale_Factors_1(Mpg123_Handle fr, c_int[] scf, Gr_Info_S gr_Info, c_int ch, c_int gr)
		{
			int scfOffset = 0;

			c_int numBits;
			c_int num0 = sLen[0, gr_Info.ScaleFac_Compress];
			c_int num1 = sLen[1, gr_Info.ScaleFac_Compress];

			if (gr_Info.Block_Type == 2)
			{
				c_int i = 18;
				numBits = (num0 + num1) * 18 - (gr_Info.Mixed_Block_Flag != 0 ? num0 : 0);

				if (fr.Bits_Avail < numBits)
					return -1;

				if (gr_Info.Mixed_Block_Flag != 0)
				{
					for (i = 8; i != 0; i--)
						scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num0);

					i = 9;
				}

				for (; i != 0; i--)
					scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num0);

				for (i = 18; i != 0; i--)
					scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num1);

				scf[scfOffset++] = 0;
				scf[scfOffset++] = 0;
				scf[scfOffset++] = 0;
			}
			else
			{
				c_int i;
				c_int scfsi = gr_Info.Scfsi;

				if (scfsi < 0)
				{	// scfsi < 0 => granule == 0
					numBits = (num0 + num1) * 10 + num0;

					if (fr.Bits_Avail < numBits)
						return -1;

					for (i = 11; i != 0; i--)
						scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num0);

					for (i = 10; i != 0; i--)
						scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num1);

					scf[scfOffset++] = 0;
				}
				else
				{
					numBits = ((scfsi & 0x8) == 0 ? 1 : 0) * num0 * 6
					          + ((scfsi & 0x4) == 0 ? 1 : 0) * num0 * 5
					          + ((scfsi & 0x2) == 0 ? 1 : 0) * num1 * 5
					          + ((scfsi & 0x1) == 0 ? 1 : 0) * num1 * 5;

					if (fr.Bits_Avail < numBits)
						return -1;

					if ((scfsi & 0x8) == 0)
					{
						for (i = 0; i < 6; i++)
							scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num0);
					}
					else
						scfOffset += 6;

					if ((scfsi & 0x4) == 0)
					{
						for (i = 0; i < 5; i++)
							scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num0);
					}
					else
						scfOffset += 5;

					if ((scfsi & 0x2) == 0)
					{
						for (i = 0; i < 5; i++)
							scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num1);
					}
					else
						scfOffset += 5;

					if ((scfsi & 0x1) == 0)
					{
						for (i = 0; i < 5; i++)
							scf[scfOffset++] = (Mask_SType)lib.getBits.GetBits_Fast(fr, num1);
					}
					else
						scfOffset += 5;

					scf[scfOffset++] = 0;
				}
			}

			return numBits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int III_Get_Scale_Factors_2(Mpg123_Handle fr, c_int[] scf, Gr_Info_S gr_Info, c_int i_Stereo)
		{
			int scfOffset = 0;

			c_int numBits = 0;
			c_uint sLen;

			if (i_Stereo != 0)	// i_stereo AND second channel -> INT123_do_layer3() checks this
				sLen = L3Tabs.I_SLen2[gr_Info.ScaleFac_Compress >> 1];
			else
				sLen = L3Tabs.N_SLen2[gr_Info.ScaleFac_Compress];

			gr_Info.PreFlag = (sLen >> 15) & 0x1;

			c_int n = 0;

			if (gr_Info.Block_Type == 2)
			{
				n++;
				if (gr_Info.Mixed_Block_Flag != 0)
					n++;
			}

			c_uchar[] pnt = sTab[n, (sLen >> 12) & 0x7];

			c_uint sLen2 = sLen;

			for (c_int i = 0; i < 4; i++)
			{
				c_int num = (c_int)(sLen2 & 0x7);
				sLen2 >>= 3;

				if (num != 0)
					numBits += pnt[i] * num;
			}

			if (numBits > gr_Info.Part2_3_Length)
				return -1;

			for (c_int i = 0; i < 4; i++)
			{
				c_int num = (c_int)(sLen & 0x7);
				sLen >>= 3;

				if (num != 0)
				{
					for (c_int j = 0; j < pnt[i]; j++)
						scf[scfOffset++] = (c_int)lib.getBits.GetBits_Fast(fr, num);
				}
				else
				{
					for (c_int j = 0; j < pnt[i]; j++)
						scf[scfOffset++] = 0;
				}
			}

			n = (n << 1) + 1;

			for (c_int i = 0; i < n; i++)
				scf[scfOffset++] = 0;

			return numBits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Refresh_Mask(Mpg123_Handle fr, ref c_int num, ref Mask_Type mask, ref c_int part2Remain)
		{
			while (num < BitShift)
			{
				mask |= (lib.getBits.GetByte(fr)) << (BitShift - num);
				num += 8;
				part2Remain -= 8;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Dequantize samples - includes Huffman decoding
		/// </summary>
		/********************************************************************/
		private c_int III_Dequantize_Sample(Mpg123_Handle fr, Span<Real> xr, c_int[] scf, Gr_Info_S gr_Info, c_int sFreq, c_int part2Bits)
		{
			c_int shift = 1 + (c_int)gr_Info.ScaleFac_Scale;
			c_int xrPnt = 0;
			int xrPntLimit = Constant.SBLimit * Constant.SSLimit;
			c_int[] l = new c_int[3];
			c_int l3;
			c_int part2Remain = (c_int)(gr_Info.Part2_3_Length - part2Bits);
			c_int me;
			int scfOffset = 0;

			// Assumption: If there is some part2_3 length at all, there should be
			// enough of it to work with properly. In case of zero length we silently
			// zero things
			if (gr_Info.Part2_3_Length > 0)
			{
				// Mhipp tree has this split up a bit...
				c_int num = lib.getBits.GetBitOffset(fr);

				// We must split this, because for num==0 the shift is undefined if you do it in one step
				Mask_Type mask = (lib.getBits.GetBits_(fr, num)) << BitShift;
				mask <<= 8 - num;
				part2Remain -= num;

				// Bitindex is zero now, we are allowed to use getbyte()
				{
					c_int bv = (c_int)gr_Info.Big_Values;
					c_int region1 = (c_int)gr_Info.Region1Start;
					c_int region2 = (c_int)gr_Info.Region2Start;
					l3 = ((576 >> 1) - bv) >> 1;

					// We may lose the 'odd' bit here!! Check this later again
					if (bv <= region1)
					{
						l[0] = bv;
						l[1] = 0;
						l[2] = 0;
					}
					else
					{
						l[0] = region1;

						if (bv <= region2)
						{
							l[1] = bv - l[0];
							l[2] = 0;
						}
						else
						{
							l[1] = region2 - l[0];
							l[2] = bv - region2;
						}
					}
				}

				if (gr_Info.Block_Type == 2)
				{
					// Decoding with short or mixed mode BandIndex table
					c_int[] max = new c_int[4];
					c_int step = 0, lWin = 3, cb = 0;
					Real v = 0.0f;
					c_short[] m;
					c_int mOffset = 0;

					if (gr_Info.Mixed_Block_Flag != 0)
					{
						max[3] = -1;
						max[0] = max[1] = max[2] = 2;
						m = L3Tabs.Map[sFreq, 0];
						me = L3Tabs.MapEnd[sFreq, 0];
					}
					else
					{
						max[0] = max[1] = max[2] = max[3] = -1;		// max[3] not really needed in this case
						m = L3Tabs.Map[sFreq, 1];
						me = L3Tabs.MapEnd[sFreq, 1];
					}

					c_int mc = 0;

					for (c_int i = 0; i < 2; i++)
					{
						c_int lp = l[i];
						NewHuff h = NewHuffman.Ht[gr_Info.Table_Select[i]];

						for (; lp != 0; lp--, mc--)
						{
							Mask_SType x, y;

							if (mc == 0)
							{
								mc = m[mOffset++];
								xrPnt = m[mOffset++];
								lWin = m[mOffset++];
								cb = m[mOffset++];

								if (lWin == 3)
								{
									v = gr_Info.Pow2Gain.Span[scf[scfOffset++] << shift];
									step = 1;
								}
								else
								{
									v = gr_Info.Full_Gain[lWin].Span[scf[scfOffset++] << shift];
									step = 3;
								}
							}

							{
								c_short[] val = h.Table;
								c_int valOffset = 0;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								while ((y = val[valOffset + (mask >> (BitShift + 4))]) < 0)
								{
									valOffset -= y;
									num -= 4;
									mask <<= 4;
								}

								num -= (y >> 8);
								mask <<= (y >> 8);
								x = (y >> 4) & 0xf;
								y &= 0xf;
							}

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((x == 15) && (h.LinBits != 0))
							{
								max[lWin] = cb;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								x += (Mask_SType)(mask >> (c_int)(BitShift + 8 - h.LinBits));
								num -= (c_int)h.LinBits + 1;
								mask <<= (c_int)h.LinBits;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[x], v);
								else
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[x], v);

								mask <<= 1;
							}
							else if (x != 0)
							{
								max[lWin] = cb;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[x], v);
								else
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[x], v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt] = Helpers.Double_To_Real(0.0);

							xrPnt += step;

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((y == 15) && (h.LinBits != 0))
							{
								max[lWin] = cb;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								y += (Mask_SType)(mask >> (int)(BitShift + 8 - h.LinBits));
								num -= (c_int)h.LinBits + 1;
								mask <<= (c_int)h.LinBits;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[y], v);
								else
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[y], v);

								mask <<= 1;
							}
							else if (y != 0)
							{
								max[lWin] = cb;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[y], v);
								else
									xr[xrPnt] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[y], v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt] = Helpers.Double_To_Real(0.0);

							xrPnt += step;
						}
					}

					for (; (l3 != 0) && ((part2Remain + num) > 0); l3--)
					{
						c_short a;

						NewHuff h = NewHuffman.Htc[gr_Info.Count1Table_Select];
						c_short[] val = h.Table;
						int valOffset = 0;

						Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

						while ((a = val[valOffset++]) < 0)
						{
							if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
								valOffset -= a;

							num--;
							mask <<= 1;
						}

						if ((part2Remain + num) <= 0)
						{
							num -= part2Remain + num;
							break;
						}

						for (c_int i = 0; i < 4; i++)
						{
							if ((i & 1) == 0)
							{
								if (mc == 0)
								{
									mc = m[mOffset++];
									xrPnt = m[mOffset++];
									lWin = m[mOffset++];
									cb = m[mOffset++];

									if (lWin == 3)
									{
										v = gr_Info.Pow2Gain.Span[scf[scfOffset++] << shift];
										step = 1;
									}
									else
									{
										v = gr_Info.Full_Gain[lWin].Span[scf[scfOffset++] << shift];
										step = 3;
									}
								}

								mc--;
							}

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((a & (0x8 >> i)) != 0)
							{
								max[lWin] = cb;

								if ((part2Remain + num) <= 0)
									break;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt] = -Helpers.Real_Scale_Layer3(v);
								else
									xr[xrPnt] = Helpers.Real_Scale_Layer3(v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt] = Helpers.Double_To_Real(0.0);

							xrPnt += step;
						}
					}

					if (lWin < 3)
					{	// Short band?
						while (true)
						{
							for (; mc > 0; mc--)
							{
								if (xrPnt >= xrPntLimit)
									return 1;

								xr[xrPnt] = Helpers.Double_To_Real(0.0);
								xrPnt += 3;		// Short band -> step=3

								xr[xrPnt] = Helpers.Double_To_Real(0.0);
								xrPnt += 3;
							}

							if (mOffset >= me)
								break;

							mc = m[mOffset++];
							xrPnt = m[mOffset++];

							if (m[mOffset++] == 0)
								break;	// Optimize: field will be set to zero at the end of the function

							mOffset++;
						}
					}

					gr_Info.MaxBand[0] = max[0] + 1;
					gr_Info.MaxBand[1] = max[1] + 1;
					gr_Info.MaxBand[2] = max[2] + 1;
					gr_Info.MaxBandL = max[3] + 1;

					{
						c_int rMax = max[0] > max[1] ? max[0] : max[1];
						rMax = (rMax > max[2] ? rMax : max[2]) + 1;
						gr_Info.MaxB = (c_uint)(rMax != 0 ? fr.ShortLimit[sFreq, rMax] : fr.LongLimit[sFreq, max[3] + 1]);
					}
				}
				else
				{
					// Decoding with 'long' BandIndex table (block_type != 2)
					c_uchar[] preTab = preTab_Choice[gr_Info.PreFlag];
					int preTabOffset = 0;
					c_int max = -1;
					c_int cb = 0;
					c_short[] m = L3Tabs.Map[sFreq, 2];
					int mOffset = 0;
					Real v = 0.0f;
					c_int mc = 0;

					// Long hash table values
					for (c_int i = 0; i < 3; i++)
					{
						c_int lp = l[i];
						NewHuff h = NewHuffman.Ht[gr_Info.Table_Select[i]];

						for (; lp != 0; lp--, mc--)
						{
							Mask_SType x, y;

							if (mc == 0)
							{
								mc = m[mOffset++];
								cb = m[mOffset++];

								v = gr_Info.Pow2Gain.Span[(scf[scfOffset++] + preTab[preTabOffset++]) << shift];
							}

							{
								c_short[] val = h.Table;
								int valOffset = 0;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								while ((y = val[valOffset + (mask >> (BitShift + 4))]) < 0)
								{
									valOffset -= y;
									num -= 4;
									mask <<= 4;
								}

								num -= (y >> 8);
								mask <<= (y >> 8);
								x = (y >> 4) & 0xf;
								y &= 0xf;
							}

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((x == 15) && (h.LinBits != 0))
							{
								max = cb;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								x += (c_int)(mask >> (int)(BitShift + 8 - h.LinBits));
								num -= (c_int)h.LinBits + 1;
								mask <<= (c_int)h.LinBits;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[x], v);
								else
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[x], v);

								mask <<= 1;
							}
							else if (x != 0)
							{
								max = cb;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[x], v);
								else
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[x], v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt++] = Helpers.Double_To_Real(0.0);

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((y == 15) && (h.LinBits != 0))
							{
								max = cb;

								Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

								y += (c_int)(mask >> (int)(BitShift + 8 - h.LinBits));
								num -= (c_int)h.LinBits + 1;
								mask <<= (c_int)h.LinBits;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[y], v);
								else
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[y], v);

								mask <<= 1;
							}
							else if (y != 0)
							{
								max = cb;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(-L3Tabs.IsPow[y], v);
								else
									xr[xrPnt++] = Helpers.Real_Mul_Scale_Layer3(L3Tabs.IsPow[y], v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt++] = Helpers.Double_To_Real(0.0);
						}
					}

					// Short (count1table) values
					for (; (l3 != 0) && ((part2Remain + num) > 0); l3--)
					{
						NewHuff h = NewHuffman.Htc[gr_Info.Count1Table_Select];
						c_short[] val = h.Table;
						int valOffset = 0;
						c_short a;

						Refresh_Mask(fr, ref num, ref mask, ref part2Remain);

						while ((a = val[valOffset++]) < 0)
						{
							if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
								valOffset -= a;

							num--;
							mask <<= 1;
						}

						if ((part2Remain + num) <= 0)
						{
							num -= part2Remain + num;
							break;
						}

						for (c_int i = 0; i < 4; i++)
						{
							if ((i & 1) == 0)
							{
								if (mc == 0)
								{
									mc = m[mOffset++];
									cb = m[mOffset++];

									v = gr_Info.Pow2Gain.Span[(scf[scfOffset++] + preTab[preTabOffset++]) << shift];
								}

								mc--;
							}

							if (xrPnt >= xrPntLimit)
								return 1;

							if ((a & (0x8 >> i)) != 0)
							{
								max = cb;

								if ((part2Remain + num) <= 0)
									break;

								if ((mask & (1 << (sizeof(Mask_Type) * 8 - 1))) != 0)
									xr[xrPnt++] = -Helpers.Real_Scale_Layer3(v);
								else
									xr[xrPnt++] = Helpers.Real_Scale_Layer3(v);

								num--;
								mask <<= 1;
							}
							else
								xr[xrPnt++] = Helpers.Double_To_Real(0.0);
						}
					}

					gr_Info.MaxBandL = max + 1;
					gr_Info.MaxB = (c_uint)fr.LongLimit[sFreq, gr_Info.MaxBandL];
				}

				part2Remain += num;
				lib.getBits.BackBits(fr, num);
				num = 0;
			}
			else
			{
				part2Remain = 0;

				// Not entirely sure what good values are, must be > 0
				gr_Info.MaxBand[0] =
				gr_Info.MaxBand[1] =
				gr_Info.MaxBand[2] =
				gr_Info.MaxBandL = 1;
				gr_Info.MaxB = 1;
			}

			while (xrPnt < xrPntLimit)
				xr[xrPnt++] = Helpers.Double_To_Real(0.0);

			while (part2Remain > 16)
			{
				lib.getBits.SkipBits(fr, 16);	// Dismiss stuffing bits
				part2Remain -= 16;
			}

			if (part2Remain > 0)
				lib.getBits.SkipBits(fr, part2Remain);
			else if (part2Remain < 0)
				return 1;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate real channel values for Joint-I-Stereo-mode
		/// </summary>
		/********************************************************************/
		private void III_I_Stereo(Memory<Real> xr_Buf, c_int[] scaleFac, Gr_Info_S gr_Info, c_int sFreq, c_int ms_Stereo, c_int lsf)
		{
			Memory<Real> xr = xr_Buf;
			BandInfoStruct bi = L3BandGain.BandInfo[sFreq];

			Span<Real> xrBuf0 = GetHybridInBuffer(xr, 0).Span;
			Span<Real> xrBuf1 = GetHybridInBuffer(xr, 1).Span;

			Real[][][][] tabs =
			{
				new []
				{
					new [] { L3Tabs.Tan1_1, L3Tabs.Tan2_1 },
					new [] { L3Tabs.Tan1_2, L3Tabs.Tan2_2 }
				},
				new []
				{
					new [] { L3Tabs.Pow1_1[0], L3Tabs.Pow2_1[0] },
					new [] { L3Tabs.Pow1_2[0], L3Tabs.Pow2_2[0] }
				},
				new []
				{
					new [] { L3Tabs.Pow1_1[1], L3Tabs.Pow2_1[1] },
					new [] { L3Tabs.Pow1_2[1], L3Tabs.Pow2_2[1] }
				}
			};

			c_int tab = (c_int)(lsf + (gr_Info.ScaleFac_Compress & lsf));
			Real[] tab1 = tabs[tab][ms_Stereo][0];
			Real[] tab2 = tabs[tab][ms_Stereo][1];

			if (gr_Info.Block_Type == 2)
			{
				c_int do_L = 0;

				if (gr_Info.Mixed_Block_Flag != 0)
					do_L = 1;

				for (c_int lWin = 0; lWin < 3; lWin++)
				{	// Process each window
					c_int is_P, sb, idx;

					// Get first band with zero values
					c_int sfb = gr_Info.MaxBand[lWin];	// sfb is minimal 3 for mixed mode

					if (sfb > 3)
						do_L = 0;

					for (; sfb < 12; sfb++)
					{
						is_P = scaleFac[sfb * 3 + lWin - gr_Info.Mixed_Block_Flag];	// Scale: 0-15

						if (is_P != 7)
						{
							sb = bi.ShortDiff[sfb];
							idx = bi.ShortIdx[sfb] + lWin;
							Real t1 = tab1[is_P];
							Real t2 = tab2[is_P];

							for (; sb > 0; sb--, idx += 3)
							{
								Real v = xrBuf0[idx];
								xrBuf0[idx] = Helpers.Real_Mul_15(v, t1);
								xrBuf1[idx] = Helpers.Real_Mul_15(v, t2);
							}
						}
					}

					is_P = scaleFac[11 * 3 + lWin - gr_Info.Mixed_Block_Flag];	// Scale: 0-15
					sb = bi.ShortDiff[12];
					idx = bi.ShortIdx[12] + lWin;

					if (is_P != 7)
					{
						Real t1 = tab1[is_P];
						Real t2 = tab2[is_P];

						for (; sb > 0; sb--, idx += 3)
						{
							Real v = xrBuf0[idx];
							xrBuf0[idx] = Helpers.Real_Mul_15(v, t1);
							xrBuf1[idx] = Helpers.Real_Mul_15(v, t2);
						}
					}
				}

				// Also check 1-part, if ALL bands in the three windows are 'empty' and mode = mixed_mode
				if (do_L != 0)
				{
					c_int sfb = gr_Info.MaxBandL;
					if (sfb > 21)
						return;		// Similarity fix related to CVE-2006-1655

					c_int idx = bi.LongIdx[sfb];

					for (; sfb < 8; sfb++)
					{
						c_int sb = bi.LongDiff[sfb];
						c_int is_P = scaleFac[sfb];	// Scale: 0-15

						if (is_P != 7)
						{
							Real t1 = tab1[is_P];
							Real t2 = tab2[is_P];

							for (; sb > 0; sb--, idx++)
							{
								Real v = xrBuf0[idx];
								xrBuf0[idx] = Helpers.Real_Mul_15(v, t1);
								xrBuf1[idx] = Helpers.Real_Mul_15(v, t2);
							}
						}
						else
							idx += sb;
					}
				}
			}
			else
			{
				c_int sfb = gr_Info.MaxBandL;
				if (sfb > 21)
					return;		// Tightened fix for CVE-2006-1655

				c_int is_P;
				c_int idx = bi.LongIdx[sfb];

				for (; sfb < 21; sfb++)
				{
					c_int sb = bi.LongDiff[sfb];
					is_P = scaleFac[sfb];	// Scale: 0-15

					if (is_P != 7)
					{
						Real t1 = tab1[is_P];
						Real t2 = tab2[is_P];

						for (; sb > 0; sb--, idx++)
						{
							Real v = xrBuf0[idx];
							xrBuf0[idx] = Helpers.Real_Mul_15(v, t1);
							xrBuf1[idx] = Helpers.Real_Mul_15(v, t2);
						}
					}
					else
						idx += sb;
				}

				is_P = scaleFac[20];
				if (is_P != 7)
				{
					// Copy l-band 20 to l-band 21
					Real t1 = tab1[is_P];
					Real t2 = tab2[is_P];

					for (c_int sb = bi.LongDiff[21]; sb > 0; sb--, idx++)
					{
						Real v = xrBuf0[idx];
						xrBuf0[idx] = Helpers.Real_Mul_15(v, t1);
						xrBuf1[idx] = Helpers.Real_Mul_15(v, t2);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void III_AntiAlias(Span<Real> xr, Gr_Info_S gr_Info)
		{
			c_int sbLim;

			if (gr_Info.Block_Type == 2)
			{
				if (gr_Info.Mixed_Block_Flag == 0)
					return;

				sbLim = 1;
			}
			else
				sbLim = (c_int)gr_Info.MaxB - 1;

			// 31 alias-reduction operations between each pair of sub-bands
			// with 8 butterflies between each pair
			{
				c_int xr1 = 1 * Constant.SSLimit;

				for (c_int sb = sbLim; sb != 0; sb--, xr1 += 10)
				{
					Real[] cs = L3Tabs.Aa_Cs;
					int csOffset = 0;
					Real[] ca = L3Tabs.Aa_Ca;
					int caOffset = 0;
					c_int xr2 = xr1;

					for (c_int ss = 7; ss >= 0; ss--)
					{	// Upper and lower butterfly inputs
						Real bu = xr[--xr2];
						Real bd = xr[xr1];

						xr[xr2] = Helpers.Real_Mul(bu, cs[csOffset]) - Helpers.Real_Mul(bd, ca[caOffset]);
						xr[xr1++] = Helpers.Real_Mul(bd, cs[csOffset++]) + Helpers.Real_Mul(bu, ca[caOffset++]);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Dct12(Memory<Real> in_, Memory<Real> rawOut1, Memory<Real> rawOut2, Real[] wi, Memory<Real> ts)
		{
			Real in0, in1, in2, in3, in4, in5;
			Span<Real> tsSpan = ts.Span;

			void Dct12_Part1()
			{
				Span<Real> inSpan = in_.Span;

				in5 = inSpan[5 * 3];
				in5 += (in4 = inSpan[4 * 3]);
				in4 += (in3 = inSpan[3 * 3]);
				in3 += (in2 = inSpan[2 * 3]);
				in2 += (in1 = inSpan[1 * 3]);
				in1 += (in0 = inSpan[0 * 3]);

				in5 += in3;
				in3 += in1;

				in2 = Helpers.Real_Mul(in2, Constant.Cos6_1);
				in3 = Helpers.Real_Mul(in3, Constant.Cos6_1);
			}

			void Dct12_Part2()
			{
				in0 += Helpers.Real_Mul(in4, Constant.Cos6_2);

				in4 = in0 + in2;
				in0 -= in2;

				in1 += Helpers.Real_Mul(in5, Constant.Cos6_2);

				in5 = Helpers.Real_Mul((in1 + in3), L3Tabs.TfCos12[0]);
				in1 = Helpers.Real_Mul((in1 - in3), L3Tabs.TfCos12[2]);

				in3 = in4 + in5;
				in4 -= in5;

				in2 = in0 + in1;
				in0 -= in1;
			}

			{
				Span<Real> out1 = rawOut1.Span;

				tsSpan[Constant.SBLimit * 0] = out1[0];
				tsSpan[Constant.SBLimit * 1] = out1[1];
				tsSpan[Constant.SBLimit * 2] = out1[2];
				tsSpan[Constant.SBLimit * 3] = out1[3];
				tsSpan[Constant.SBLimit * 4] = out1[4];
				tsSpan[Constant.SBLimit * 5] = out1[5];

				Dct12_Part1();

				{
					Real tmp0;
					Real tmp1 = (in0 - in4);

					{
						Real tmp2 = Helpers.Real_Mul((in1 - in5), L3Tabs.TfCos12[1]);
						tmp0 = tmp1 + tmp2;
						tmp1 -= tmp2;
					}

					tsSpan[(17 - 1) * Constant.SBLimit] = out1[17 - 1] + Helpers.Real_Mul(tmp0, wi[11 - 1]);
					tsSpan[(12 + 1) * Constant.SBLimit] = out1[12 + 1] + Helpers.Real_Mul(tmp0, wi[6 + 1]);
					tsSpan[(6 + 1) * Constant.SBLimit] = out1[6 + 1] + Helpers.Real_Mul(tmp1, wi[1]);
					tsSpan[(11 - 1) * Constant.SBLimit] = out1[11 - 1] + Helpers.Real_Mul(tmp1, wi[5 - 1]);
				}

				Dct12_Part2();

				tsSpan[(17 - 0) * Constant.SBLimit] = out1[17 - 0] + Helpers.Real_Mul(in2, wi[11 - 0]);
				tsSpan[(12 + 0) * Constant.SBLimit] = out1[12 + 0] + Helpers.Real_Mul(in2, wi[6 + 0]);
				tsSpan[(12 + 2) * Constant.SBLimit] = out1[12 + 2] + Helpers.Real_Mul(in3, wi[6 + 2]);
				tsSpan[(17 - 2) * Constant.SBLimit] = out1[17 - 2] + Helpers.Real_Mul(in3, wi[11 - 2]);

				tsSpan[(6 + 0) * Constant.SBLimit] = out1[6 + 0] + Helpers.Real_Mul(in0, wi[0]);
				tsSpan[(11 - 0) * Constant.SBLimit] = out1[11 - 0] + Helpers.Real_Mul(in0, wi[5 - 0]);
				tsSpan[(6 + 2) * Constant.SBLimit] = out1[6 + 2] + Helpers.Real_Mul(in4, wi[2]);
				tsSpan[(11 - 2) * Constant.SBLimit] = out1[11 - 2] + Helpers.Real_Mul(in4, wi[5 - 2]);
			}

			in_ = in_.Slice(1);

			{
				Span<Real> out2 = rawOut2.Span;

				Dct12_Part1();

				{
					Real tmp0;
					Real tmp1 = (in0 - in4);

					{
						Real tmp2 = Helpers.Real_Mul((in1 - in5), L3Tabs.TfCos12[1]);
						tmp0 = tmp1 + tmp2;
						tmp1 -= tmp2;
					}

					out2[5 - 1] = Helpers.Real_Mul(tmp0, wi[11 - 1]);
					out2[0 + 1] = Helpers.Real_Mul(tmp0, wi[6 + 1]);
					tsSpan[(12 + 1) * Constant.SBLimit] += Helpers.Real_Mul(tmp1, wi[1]);
					tsSpan[(17 - 1) * Constant.SBLimit] += Helpers.Real_Mul(tmp1, wi[5 - 1]);
				}

				Dct12_Part2();

				out2[5 - 0] = Helpers.Real_Mul(in2, wi[11 - 0]);
				out2[0 + 0] = Helpers.Real_Mul(in2, wi[6 + 0]);
				out2[0 + 2] = Helpers.Real_Mul(in3, wi[6 + 2]);
				out2[5 - 2] = Helpers.Real_Mul(in3, wi[11 - 2]);

				tsSpan[(12 + 0) * Constant.SBLimit] += Helpers.Real_Mul(in0, wi[0]);
				tsSpan[(17 - 0) * Constant.SBLimit] += Helpers.Real_Mul(in0, wi[5 - 0]);
				tsSpan[(12 + 2) * Constant.SBLimit] += Helpers.Real_Mul(in4, wi[2]);
				tsSpan[(17 - 2) * Constant.SBLimit] += Helpers.Real_Mul(in4, wi[5 - 2]);
			}

			in_ = in_.Slice(1);

			{
				Span<Real> out2 = rawOut2.Span;

				out2[12] = out2[13] = out2[14] = out2[15] = out2[16] = out2[17] = 0.0f;

				Dct12_Part1();

				{
					Real tmp0;
					Real tmp1 = (in0 - in4);

					{
						Real tmp2 = Helpers.Real_Mul((in1 - in5), L3Tabs.TfCos12[1]);
						tmp0 = tmp1 + tmp2;
						tmp1 -= tmp2;
					}

					out2[11 - 1] = Helpers.Real_Mul(tmp0, wi[11 - 1]);
					out2[6 + 1] = Helpers.Real_Mul(tmp0, wi[6 + 1]);
					out2[0 + 1] += Helpers.Real_Mul(tmp1, wi[1]);
					out2[5 - 1] += Helpers.Real_Mul(tmp1, wi[5 - 1]);
				}

				Dct12_Part2();

				out2[11 - 0] = Helpers.Real_Mul(in2, wi[11 - 0]);
				out2[6 + 0] = Helpers.Real_Mul(in2, wi[6 + 0]);
				out2[6 + 2] = Helpers.Real_Mul(in3, wi[6 + 2]);
				out2[11 - 2] = Helpers.Real_Mul(in3, wi[11 - 2]);

				out2[0 + 0] += Helpers.Real_Mul(in0, wi[0]);
				out2[5 - 0] += Helpers.Real_Mul(in0, wi[5 - 0]);
				out2[0 + 2] += Helpers.Real_Mul(in4, wi[2]);
				out2[5 - 2] += Helpers.Real_Mul(in4, wi[5 - 2]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void III_Hybrid(Memory<Real> fsIn, Memory<Real> tsOut, c_int ch, Gr_Info_S gr_Info, Mpg123_Handle fr)
		{
			Real[][][] block = fr.Hybrid_Block;
			c_int[] blc = fr.Hybrid_Blc;

			c_int tsPnt = 0;
			Real[] rawOut1, rawOut2;
			int rawOut1Offset = 0;
			int rawOut2Offset = 0;

			c_int bt = 0;
			size_t sb = 0;

			{
				c_int b = blc[ch];
				rawOut1 = block[b][ch];

				b = -b + 1;
				rawOut2 = block[b][ch];

				blc[ch] = b;
			}

			if (gr_Info.Mixed_Block_Flag != 0)
			{
				sb = 2;

				lib.optimize.Opt_Dct36()(fsIn.Slice(0 * Constant.SSLimit), rawOut1.AsMemory(rawOut1Offset), rawOut2.AsMemory(rawOut2Offset), L3Tabs.Win[0], tsOut.Slice(tsPnt));
				lib.optimize.Opt_Dct36()(fsIn.Slice(1 * Constant.SSLimit), rawOut1.AsMemory(rawOut1Offset + 18), rawOut2.AsMemory(rawOut2Offset + 18), L3Tabs.Win1[0], tsOut.Slice(tsPnt + 1));

				rawOut1Offset += 36;
				rawOut2Offset += 36;
				tsPnt += 2;
			}

			bt = (c_int)gr_Info.Block_Type;

			if (bt == 2)
			{
				for (; sb < gr_Info.MaxB; sb += 2, tsPnt += 2, rawOut1Offset += 36, rawOut2Offset += 36)
				{
					Dct12(fsIn.Slice((int)(sb * Constant.SSLimit)), rawOut1.AsMemory(rawOut1Offset), rawOut2.AsMemory(rawOut2Offset), L3Tabs.Win[2], tsOut.Slice(tsPnt));
					Dct12(fsIn.Slice((int)((sb + 1) * Constant.SSLimit)), rawOut1.AsMemory(rawOut1Offset + 18), rawOut2.AsMemory(rawOut2Offset + 18), L3Tabs.Win1[2], tsOut.Slice(tsPnt + 1));
				}
			}
			else
			{
				for (; sb < gr_Info.MaxB; sb += 2, tsPnt += 2, rawOut1Offset += 36, rawOut2Offset += 36)
				{
					lib.optimize.Opt_Dct36()(fsIn.Slice((int)(sb * Constant.SSLimit)), rawOut1.AsMemory(rawOut1Offset), rawOut2.AsMemory(rawOut2Offset), L3Tabs.Win[bt], tsOut.Slice(tsPnt));
					lib.optimize.Opt_Dct36()(fsIn.Slice((int)((sb + 1) * Constant.SSLimit)), rawOut1.AsMemory(rawOut1Offset + 18), rawOut2.AsMemory(rawOut2Offset + 18), L3Tabs.Win1[bt], tsOut.Slice(tsPnt + 1));
				}
			}

			for (; sb < Constant.SBLimit; sb++, tsPnt++)
			{
				for (c_int i = 0; i < Constant.SSLimit; i++)
				{
					tsOut.Span[tsPnt + i * Constant.SBLimit] = rawOut1[rawOut1Offset++];
					rawOut2[rawOut2Offset++] = Helpers.Double_To_Real(0.0);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Fill_PInfo_Side(Mpg123_Handle fr, III_SideInfo si, c_int gr, c_int stereo1)
		{
			c_int sb;
			c_float ifqStep;	// Why not double?

			for (c_int ch = 0; ch < stereo1; ++ch)
			{
				Gr_Info_S gr_Info = si.Ch_Gr[ch, gr];

				fr.PInfo.Big_Values[gr, ch] = (c_int)gr_Info.Big_Values;
				fr.PInfo.ScaleFac_Scale[gr, ch] = (c_int)gr_Info.ScaleFac_Scale;
				fr.PInfo.Mixed[gr, ch] = (c_int)gr_Info.Mixed_Block_Flag;
				fr.PInfo.BlockType[gr, ch] = (c_int)gr_Info.Block_Type;
				fr.PInfo.MainBits[gr, ch] = (c_int)gr_Info.Part2_3_Length;
				fr.PInfo.PreFlag[gr, ch] = (c_int)gr_Info.PreFlag;

				if (gr == 1)
					fr.PInfo.Scfsi[ch] = gr_Info.Scfsi;
			}

			for (c_int ch = 0; ch < stereo1; ++ch)
			{
				Gr_Info_S gr_Info = si.Ch_Gr[ch, gr];

				ifqStep = (fr.PInfo.ScaleFac_Scale[gr, ch] == 0) ? .5f : 1.0f;

				if (2 == gr_Info.Block_Type)
				{
					for (c_int i = 0; i < 3; ++i)
					{
						for (sb = 0; sb < 12; ++sb)
						{
							c_int j = 3 * sb + i;

							fr.PInfo.Sfb_S[gr, ch, j] = -ifqStep * fr.PInfo.Sfb_S[gr, ch, j - gr_Info.Mixed_Block_Flag];
							fr.PInfo.Sfb_S[gr, ch, j] -= 2 * (fr.PInfo.Sub_Gain[gr, ch, i]);
						}

						fr.PInfo.Sfb_S[gr, ch, 3 * sb + i] = -2 * (fr.PInfo.Sub_Gain[gr, ch, i]);
					}
				}
				else
				{
					for (sb = 0; sb < 21; ++sb)
					{
						fr.PInfo.Sfb[gr, ch, sb] = fr.PInfo.Sfb_S[gr, ch, sb];

						if (gr_Info.PreFlag != 0)
							fr.PInfo.Sfb[gr, ch, sb] += preTab_Choice[1][sb];

						fr.PInfo.Sfb[gr, ch, sb] *= -ifqStep;
					}

					fr.PInfo.Sfb[gr, ch, 21] = 0;
				}
			}

			for (c_int ch = 0; ch < stereo1; ++ch)
			{
				c_int j = 0;

				for (sb = 0; sb < Constant.SBLimit; ++sb)
				{
					Span<Real> hybridIn = GetHybridInBuffer(fr.Layer3.Hybrid_In, ch, sb).Span;

					for (c_int ss = 0; ss < Constant.SSLimit; ++ss, ++j)
						fr.PInfo.Xr[gr, ch, j] = hybridIn[ss];
				}
			}
		}
		#endregion
	}
}
