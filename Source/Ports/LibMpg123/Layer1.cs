/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;
using Single = Polycode.NostalgicPlayer.Ports.LibMpg123.Containers.Single;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// The layer 1 decoder
	/// </summary>
	internal class Layer1
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Layer1(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Int123_Do_Layer1(Mpg123_Handle fr)
		{
			c_int clip = 0;
			c_int stereo = fr.Stereo;
			c_uint[] bAlloc = new c_uint[2 * Constant.SBLimit];
			c_uint[] scale_Index = new c_uint[2 * Constant.SBLimit];
			Memory<Real> fraction = fr.Layer1;
			Single single = fr.Single;

			fr.JsBound = (fr.Mode == Mode.Joint_Stereo) ? (fr.Mode_Ext << 2) + 4 : 32;

			if ((stereo == 1) || (single == Single.Mix))	// I don't see mixing handled here
				single = Single.Left;

			if (I_Step_One(bAlloc, scale_Index, fr) != 0)
				return clip;

			for (c_int i = 0; i < Constant.Scale_Block; i++)
			{
				if (I_Step_Two(fraction, bAlloc, scale_Index, fr) != 0)
					return clip;

				if (single != Single.Stereo)
					clip += fr.Synth_Mono(GetLayerBuffer(fraction, (int)single), fr);
				else
					clip += fr.Synth_Stereo(GetLayerBuffer(fraction, 0), GetLayerBuffer(fraction, 1), fr);
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
		private Memory<Real> GetLayerBuffer(Memory<Real> buffer, int index)
		{
			return buffer.Slice(index * Constant.SBLimit);
		}



		/********************************************************************/
		/// <summary>
		/// Allocation value is now allowed to be 15. Initially, libmad
		/// showed me the error that mpg123 used to ignore. Then, I found a
		/// quote on that in Shlien S. (1994): Guide to MPEG-1 Audio Standard.
		/// IEEE Transactions on Broadcasting 40, 4
		///
		/// "To avoid conflicts with the synchronization code, code '1111'
		/// is defined to be illegal"
		/// </summary>
		/********************************************************************/
		private c_int Check_BAlloc(c_uint[] bAlloc, c_int end)
		{
			for (c_int ba = 0; ba != end; ++ba)
			{
				if (bAlloc[ba] == 15)
					return -1;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int I_Step_One(c_uint[] bAlloc, c_uint[] scale_Index, Mpg123_Handle fr)
		{
			c_int ba = 0;
			c_int sca = 0;

			if (fr.Stereo == 2)
			{
				c_int jsBound = fr.JsBound;
				c_uint needBits = (c_uint)(jsBound * 2 * 4 + (Constant.SBLimit - jsBound) * 4);

				if (fr.Bits_Avail < needBits)
					return -1;

				needBits = 0;

				for (c_int i = 0; i < jsBound; i++)
				{
					bAlloc[ba] = lib.getBits.GetBits_Fast(fr, 4);
					bAlloc[ba + 1] = lib.getBits.GetBits_Fast(fr, 4);

					needBits += (c_uint)(((bAlloc[ba] != 0 ? 1 : 0) + (bAlloc[ba + 1] != 0 ? 1 : 0)) * 6);
					ba += 2;
				}

				for (c_int i = jsBound; i < Constant.SBLimit; i++)
				{
					bAlloc[ba] = lib.getBits.GetBits_Fast(fr, 4);
					needBits += (c_uint)((bAlloc[ba] != 0 ? 1 : 0) * 12);
					ba++;
				}

				if (Check_BAlloc(bAlloc, ba) != 0)
					return -1;

				ba = 0;

				if (fr.Bits_Avail < needBits)
					return -1;

				for (c_int i = 0; i < jsBound; i++)
				{
					if (bAlloc[ba++] != 0)
						scale_Index[sca++] = lib.getBits.GetBits_Fast(fr, 6);

					if (bAlloc[ba++] != 0)
						scale_Index[sca++] = lib.getBits.GetBits_Fast(fr, 6);
				}

				for (c_int i = jsBound; i < Constant.SBLimit; i++)
				{
					if (bAlloc[ba++] != 0)
					{
						scale_Index[sca++] = lib.getBits.GetBits_Fast(fr, 6);
						scale_Index[sca++] = lib.getBits.GetBits_Fast(fr, 6);
					}
				}
			}
			else
			{
				c_uint needBits = Constant.SBLimit * 4;

				if (fr.Bits_Avail < needBits)
					return -1;

				needBits = 0;

				for (c_int i = 0; i < Constant.SBLimit; i++)
				{
					bAlloc[ba] = lib.getBits.GetBits_Fast(fr, 4);
					needBits += (c_uint)((bAlloc[ba] != 0 ? 1 : 0) * 6);
					++ba;
				}

				if (Check_BAlloc(bAlloc, ba) != 0)
					return -1;

				ba = 0;

				if (fr.Bits_Avail < needBits)
					return -1;

				for (c_int i = 0; i < Constant.SBLimit; i++)
				{
					if (bAlloc[ba++] != 0)
						scale_Index[sca++] = lib.getBits.GetBits_Fast(fr, 6);
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_int Minus_Shift(c_uint n)
		{
			return (c_int)(c_uint.MaxValue << (c_int)n);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int I_Step_Two(Memory<Real> fraction, c_uint[] bAlloc, c_uint[] scale_Index, Mpg123_Handle fr)
		{
			c_int i;
			c_uint n;
			int[] smpb = new int[2 * Constant.SBLimit];		// Values: 0-65535
			int sample;
			int sca = 0;

			if (fr.Stereo == 2)
			{
				c_uint needBits = 0;
				c_int jsBound = fr.JsBound;
				Span<Real> f0 = GetLayerBuffer(fraction, 0).Span;
				Span<Real> f1 = GetLayerBuffer(fraction, 1).Span;
				int f0Index = 0;
				int f1Index = 0;

				c_uint ba = 0;

				for (i = 0; i < jsBound; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						needBits += n + 1;

					n = bAlloc[ba++];
					if (n != 0)
						needBits += n + 1;
				}

				for (i = jsBound; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						needBits += n + 1;
				}

				if (fr.Bits_Avail < needBits)
					return -1;

				ba = 0;

				for (sample = 0, i = 0; i < jsBound; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						smpb[sample++] = (c_int)lib.getBits.GetBits_(fr, (c_int)(n + 1));

					n = bAlloc[ba++];
					if (n != 0)
						smpb[sample++] = (c_int)lib.getBits.GetBits_(fr, (c_int)(n + 1));
				}

				for (i = jsBound; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						smpb[sample++] = (c_int)lib.getBits.GetBits_(fr, (c_int)(n + 1));
				}

				ba = 0;

				for (sample = 0, i = 0; i < jsBound; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						f0[f0Index++] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15(Minus_Shift(n) + (smpb[sample++]) + 1), fr.Muls[n + 1][scale_Index[sca++]]);
					else
						f0[f0Index++] = Helpers.Double_To_Real(0.0);

					n = bAlloc[ba++];
					if (n != 0)
						f1[f1Index++] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15(Minus_Shift(n) + (smpb[sample++]) + 1), fr.Muls[n + 1][scale_Index[sca++]]);
					else
						f1[f1Index++] = Helpers.Double_To_Real(0.0);
				}

				for (i = jsBound; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
					{
						Real samp = Helpers.Double_To_Real_15(Minus_Shift(n) + (smpb[sample++]) + 1);
						f0[f0Index++] = Helpers.Real_Mul_Scale_Layer12(samp, fr.Muls[n + 1][scale_Index[sca++]]);
						f1[f1Index++] = Helpers.Real_Mul_Scale_Layer12(samp, fr.Muls[n + 1][scale_Index[sca++]]);
					}
					else
						f0[f0Index++] = f1[f1Index++] = Helpers.Double_To_Real(0.0);
				}

				for (i = fr.Down_Sample_SbLimit; i < 32; i++)
					f0[i] = f1[i] = 0.0f;
			}
			else
			{
				c_uint needBits = 0;
				Span<Real> f0 = GetLayerBuffer(fraction, 0).Span;
				int f0Index = 0;

				c_uint ba = 0;

				for (i = 0; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						needBits += n + 1;
				}

				if (fr.Bits_Avail < needBits)
					return -1;

				ba = 0;

				for (sample = 0, i = 0; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						smpb[sample++] = (c_int)lib.getBits.GetBits_(fr, (c_int)(n + 1));
				}

				ba = 0;

				for (sample = 0, i = 0; i < Constant.SBLimit; i++)
				{
					n = bAlloc[ba++];
					if (n != 0)
						f0[f0Index++] = Helpers.Real_Mul_Scale_Layer12(Helpers.Double_To_Real_15(Minus_Shift(n) + (smpb[sample++]) + 1), fr.Muls[n + 1][scale_Index[sca++]]);
					else
						f0[f0Index++] = Helpers.Double_To_Real(0.0);
				}

				for (i = fr.Down_Sample_SbLimit; i < 32; i++)
					f0[i] = Helpers.Double_To_Real(0.0f);
			}

			return 0;
		}
		#endregion
	}
}
