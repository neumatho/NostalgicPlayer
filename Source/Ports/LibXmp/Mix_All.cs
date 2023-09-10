/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// To increase performance eight mixers are defined, one for each
	/// combination of the following parameters: interpolation, resolution
	/// and number of channels
	/// </summary>
	internal static class Mix_All
	{
		// The following lut settings are PRECOMPUTED. If you plan on changing these
		// settings, you MUST also regenerate the arrays
		private const c_int Spline_QuantBits = 14;
		private const c_int Spline_Shift = Spline_QuantBits;

		/********************************************************************/
		/// <summary>
		/// Handler for 8 bit samples, spline interpolated mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_Mono_8Bit_Spline(Mixer_Voice vi, c_int[] buffer, c_int offset, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int8)
			c_int old_VL = vi.Old_VL;

			#region VAR_NORM
			c_int smp_In;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr);
			c_int sPtrOffset = vi.SPtrOffset;
			c_uint pos = (c_uint)vi.Pos;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			for (; count > ramp; count--)
			{
				#region SPLINE_INTERP
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> (Spline_Shift - 8);
				#endregion

				#region MIX_MONO_AC
				buffer[offset++] += smp_In * (old_VL >> 8);
				old_VL += delta_L;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}

			for (; count != 0; count--)
			{
				#region SPLINE_INTERP
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> (Spline_Shift - 8);
				#endregion

				#region MIX_MONO
				buffer[offset++] += smp_In * vl;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16 bit samples, spline interpolated mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_Mono_16Bit_Spline(Mixer_Voice vi, c_int[] buffer, c_int offset, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int16)
			c_int old_VL = vi.Old_VL;

			#region VAR_NORM
			c_int smp_In;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr);
			c_int sPtrOffset = vi.SPtrOffset / 2;
			c_uint pos = (c_uint)vi.Pos;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			for (; count > ramp; count--)
			{
				#region SPLINE_INTERP_16BIT
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> Spline_Shift;
				#endregion

				#region MIX_MONO_AC
				buffer[offset++] += smp_In * (old_VL >> 8);
				old_VL += delta_L;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}

			for (; count != 0; count--)
			{
				#region SPLINE_INTERP_16BIT
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> Spline_Shift;
				#endregion

				#region MIX_MONO
				buffer[offset++] += smp_In * vl;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8 bit samples, spline interpolated stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_Stereo_8Bit_Spline(Mixer_Voice vi, c_int[] buffer, c_int offset, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int8)

			#region VAR_SPLINE_MONO
			c_int old_VL = vi.Old_VL;

			#region VAR_NORM
			c_int smp_In;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr);
			c_int sPtrOffset = vi.SPtrOffset;
			c_uint pos = (c_uint)vi.Pos;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				#region SPLINE_INTERP
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> (Spline_Shift - 8);
				#endregion

				#region MIX_STEREO_AC
				buffer[offset++] += smp_In * (old_VR >> 8);
				old_VR += delta_R;

				buffer[offset++] += smp_In * (old_VL >> 8);
				old_VL += delta_L;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}

			for (; count != 0; count--)
			{
				#region SPLINE_INTERP
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> (Spline_Shift - 8);
				#endregion

				#region MIX_STEREO
				buffer[offset++] += smp_In * vr;
				buffer[offset++] += smp_In * vl;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16 bit samples, spline interpolated stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_Stereo_16Bit_Spline(Mixer_Voice vi, c_int[] buffer, c_int offset, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int16)

			#region VAR_SPLINE_MONO
			c_int old_VL = vi.Old_VL;

			#region VAR_NORM
			c_int smp_In;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr);
			c_int sPtrOffset = vi.SPtrOffset / 2;
			c_uint pos = (c_uint)vi.Pos;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				#region SPLINE_INTERP_16BIT
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> Spline_Shift;
				#endregion

				#region MIX_STEREO_AC
				buffer[offset++] += smp_In * (old_VR >> 8);
				old_VR += delta_R;

				buffer[offset++] += smp_In * (old_VL >> 8);
				old_VL += delta_L;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}

			for (; count != 0; count--)
			{
				#region SPLINE_INTERP_16BIT
				c_int f = frac >> 6;
				smp_In = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[sPtrOffset + (c_int)pos - 1] +
						  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[sPtrOffset + (c_int)pos] +
						  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[sPtrOffset + (c_int)pos + 2] +
						  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[sPtrOffset + (c_int)pos + 1]) >> Spline_Shift;
				#endregion

				#region MIX_STEREO
				buffer[offset++] += smp_In * vr;
				buffer[offset++] += smp_In * vl;
				#endregion

				#region UPDATE_POS
				frac += step;
				pos += (c_uint)(frac >> Constants.SMix_Shift);
				frac &= Constants.SMix_Mask;
				#endregion
			}
		}
	}
}
