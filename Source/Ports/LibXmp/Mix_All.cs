/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
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

		// Sample pre-amplification is required to fix filter rounding errors
		// at high sample rates. The non-filtered mixers do not need this
		private const c_int PreAmp_Bits = 15;

		// IT's WAV output driver uses a clamp that seems to roughly match this:
		// compare the WAV output of OpenMPT env-flt-max.it and filter-reset.it
		private const int64 Filter_Min = -65536 * (1 << PreAmp_Bits);
		private const int64 Filter_Max = 65535 * (1 << PreAmp_Bits);

		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit mono samples, spline interpolated mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Mono_8Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int8)

			#region VAR_MONO(int8)
			const c_int chn = 1;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO_AC
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit mono samples, spline interpolated mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Mono_16Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int16)

			#region VAR_MONO(int16)
			const c_int chn = 1;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO_AC
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit stereo samples, spline interpolated mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Stereo_8Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int8)

			#region VAR_STEREO(int8)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG_AC
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO_AC
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = old_VL >> 8;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}

								old_VL += delta_L;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = vl;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit stereo samples, spline interpolated mono
		/// output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Stereo_16Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int16)

			#region VAR_STEREO(int16)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG_AC
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO_AC
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = old_VL >> 8;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}

								old_VL += delta_L;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = vl;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit mono samples, spline interpolated stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Mono_8Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int8)

			#region VAR_MONO(int8)
			const c_int chn = 1;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit mono samples, spline interpolated stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Mono_16Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int16)

			#region VAR_MONO(int16)
			const c_int chn = 1;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit stereo samples, spline interpolated stereo
		/// output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Stereo_8Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int8)

			#region VAR_STEREO(int8)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit stereo samples, spline interpolated stereo
		/// output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Stereo_16Bit_Spline(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int16)

			#region VAR_STEREO(int16)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit mono samples, filtered spline interpolated mono
		/// output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Mono_8Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int8)

			#region VAR_MONO(int8)
			const c_int chn = 1;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO_AC
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_MONO
			{
				// Note: copying the left to the right here is for just in case these
				// values don't get cleared between playing stereo/mono samples
				vi.Filter.L1 = fl1;
				vi.Filter.L2 = fl2;
				vi.Filter.R1 = fl1;
				vi.Filter.R2 = fl2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit mono samples, filtered spline interpolated
		/// mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Mono_16Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int16)

			#region VAR_MONO(int16)
			const c_int chn = 1;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO_AC
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In = smpL;

					#region MIX_MONO
					{
						{
							c_int out_Sample = smp_In;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_MONO
			{
				// Note: copying the left to the right here is for just in case these
				// values don't get cleared between playing stereo/mono samples
				vi.Filter.L1 = fl1;
				vi.Filter.L2 = fl2;
				vi.Filter.R1 = fl1;
				vi.Filter.R2 = fl2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit stereo samples, filtered spline interpolated
		/// mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Stereo_8Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int8)

			#region VAR_STEREO(int8)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_STEREO

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			c_int fr1 = vi.Filter.R1, fr2 = vi.Filter.R2;
			int64 sr64;
			c_int sr;
			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG_AC
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO_AC
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = old_VL >> 8;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}

								old_VL += delta_L;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = vl;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_STEREO
			{
				#region SAVE_FILTER_MONO
				{
					// Note: copying the left to the right here is for just in case these
					// values don't get cleared between playing stereo/mono samples
					vi.Filter.L1 = fl1;
					vi.Filter.L2 = fl2;
					vi.Filter.R1 = fl1;
					vi.Filter.R2 = fl2;
				}
				#endregion

				vi.Filter.R1 = fr1;
				vi.Filter.R2 = fr2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit stereo samples, filtered spline interpolated
		/// mono output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_MonoOut_Stereo_16Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int16)

			#region VAR_STEREO(int16)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_STEREO

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			c_int fr1 = vi.Filter.R1, fr2 = vi.Filter.R2;
			int64 sr64;
			c_int sr;
			#endregion

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG_AC
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO_AC
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = old_VL >> 8;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}

								old_VL += delta_L;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_MONO_AVG
					{
						{
							c_int smp_In = (smp_In_L + smp_In_R) >> 1;

							#region MIX_MONO
							{
								{
									c_int out_Sample = smp_In;
									c_int out_Level = vl;

									#region MIX_OUT
									{
										buffer[0] += out_Sample * out_Level;
										buffer++;
									}
									#endregion
								}
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_STEREO
			{
				#region SAVE_FILTER_MONO
				{
					// Note: copying the left to the right here is for just in case these
					// values don't get cleared between playing stereo/mono samples
					vi.Filter.L1 = fl1;
					vi.Filter.L2 = fl2;
					vi.Filter.R1 = fl1;
					vi.Filter.R2 = fl2;
				}
				#endregion

				vi.Filter.R1 = fr1;
				vi.Filter.R2 = fr2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit mono samples, filtered spline interpolated
		/// stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Mono_8Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int8)

			#region VAR_MONO(int8)
			const c_int chn = 1;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_MONO
			{
				// Note: copying the left to the right here is for just in case these
				// values don't get cleared between playing stereo/mono samples
				vi.Filter.L1 = fl1;
				vi.Filter.L2 = fl2;
				vi.Filter.R1 = fl1;
				vi.Filter.R2 = fl2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit mono samples, filtered spline interpolated
		/// stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Mono_16Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_MONO(int16)

			#region VAR_MONO(int16)
			const c_int chn = 1;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;

					#region FILTER_MONO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpL;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_MONO
			{
				// Note: copying the left to the right here is for just in case these
				// values don't get cleared between playing stereo/mono samples
				vi.Filter.L1 = fl1;
				vi.Filter.L2 = fl2;
				vi.Filter.R1 = fl1;
				vi.Filter.R2 = fl2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 8-bit stereo samples, filtered spline interpolated
		/// stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Stereo_8Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int8)

			#region VAR_STEREO(int8)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int8)
			c_int smpL;
			Span<int8> sPtr = MemoryMarshal.Cast<byte, int8>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_STEREO

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			c_int fr1 = vi.Filter.R1, fr2 = vi.Filter.R2;
			int64 sr64;
			c_int sr;
			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_8BIT
					{
						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> (Spline_Shift - 8);
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_STEREO
			{
				#region SAVE_FILTER_MONO
				{
					// Note: copying the left to the right here is for just in case these
					// values don't get cleared between playing stereo/mono samples
					vi.Filter.L1 = fl1;
					vi.Filter.L2 = fl2;
					vi.Filter.R1 = fl1;
					vi.Filter.R2 = fl2;
				}
				#endregion

				vi.Filter.R1 = fr1;
				vi.Filter.R2 = fr2;
			}
			#endregion
		}



		/********************************************************************/
		/// <summary>
		/// Handler for 16-bit stereo samples, filtered spline interpolated
		/// stereo output
		/// </summary>
		/********************************************************************/
		public static void LibXmp_Mix_StereoOut_Stereo_16Bit_Spline_Filter(Mixer_Voice vi, CPointer<c_int> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R)
		{
			#region VAR_SPLINE_STEREO(int16)

			#region VAR_STEREO(int16)
			const c_int chn = 2;
			c_int smpR;

			#region VAR_NORM(int16)
			c_int smpL;
			Span<int16> sPtr = MemoryMarshal.Cast<byte, int16>(vi.SPtr.Buffer);
			int sPtrOffset = vi.SPtr.Offset / 2;
			c_int pos = ((c_int)vi.Pos) * chn;
			c_int frac = (c_int)((1 << Constants.SMix_Shift) * (vi.Pos - (c_int)vi.Pos));
			#endregion

			#endregion

			#endregion

			#region VAR_FILTER_STEREO

			#region VAR_FILTER_MONO
			c_int fl1 = vi.Filter.L1, fl2 = vi.Filter.L2;
			int64 a0 = vi.Filter.A0, b0 = vi.Filter.B0, b1 = vi.Filter.B1;
			int64 sl64;
			c_int sl;
			#endregion

			c_int fr1 = vi.Filter.R1, fr2 = vi.Filter.R2;
			int64 sr64;
			c_int sr;
			#endregion

			#region VAR_STEREOOUT

			#region VAR_MONOOUT
			c_int old_VL = vi.Old_VL;
			#endregion

			c_int old_VR = vi.Old_VR;
			#endregion

			for (; count > ramp; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}
				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO_AC
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = old_VL >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = old_VR >> 8;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						old_VL += delta_L;
						old_VR += delta_R;
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			for (; count != 0; count--)
			{
				{
					c_int off = 0 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpL = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int off = 1 + sPtrOffset;

					#region SPLINE_16BIT
					{

						c_int f = frac >> 6;
						smpR = (PreComp_Lut.Cubic_Spline_Lut0[f] * sPtr[pos + off - chn] +
							  PreComp_Lut.Cubic_Spline_Lut1[f] * sPtr[pos + off] +
							  PreComp_Lut.Cubic_Spline_Lut3[f] * sPtr[pos + off + (chn << 1)] +
							  PreComp_Lut.Cubic_Spline_Lut2[f] * sPtr[pos + off + chn]) >> Spline_Shift;
					}
					#endregion
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region FILTER_STEREO
					{
						#region FILTER_LEFT
						{
							sl64 = (a0 * (smp_In_L << PreAmp_Bits) + b0 * fl1 + b1 * fl2) >> Constants.Filter_Shift;
							sl = Mix_Filter_Clamp(sl64);
							fl2 = fl1; fl1 = sl;
							smp_In_L = (sl >> PreAmp_Bits);
						}
						#endregion

						#region FILTER_RIGHT
						{
							sr64 = (a0 * (smp_In_R << PreAmp_Bits) + b0 * fr1 + b1 * fr2) >> Constants.Filter_Shift;
							sr = Mix_Filter_Clamp(sr64);
							fr2 = fr1; fr1 = sr;
							smp_In_R = (sr >> PreAmp_Bits);
						}
						#endregion
					}
					#endregion

					smpL = smp_In_L;
					smpR = smp_In_R;
				}

				{
					c_int smp_In_L = smpL;
					c_int smp_In_R = smpR;

					#region MIX_STEREO
					{
						{
							c_int out_Sample = smp_In_L;
							c_int out_Level = vl;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}

						{
							c_int out_Sample = smp_In_R;
							c_int out_Level = vr;

							#region MIX_OUT
							{
								buffer[0] += out_Sample * out_Level;
								buffer++;
							}
							#endregion
						}
					}
					#endregion
				}

				#region UPDATE_POS
				{
					frac += step;
					pos += (frac >> Constants.SMix_Shift) * chn;
					frac &= Constants.SMix_Mask;
				}
				#endregion
			}

			#region SAVE_FILTER_STEREO
			{
				#region SAVE_FILTER_MONO
				{
					// Note: copying the left to the right here is for just in case these
					// values don't get cleared between playing stereo/mono samples
					vi.Filter.L1 = fl1;
					vi.Filter.L2 = fl2;
					vi.Filter.R1 = fl1;
					vi.Filter.R2 = fl2;
				}
				#endregion

				vi.Filter.R1 = fr1;
				vi.Filter.R2 = fr2;
			}
			#endregion
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_int Mix_Filter_Clamp(int64 a)
		{
			return (c_int)(a < Filter_Min ? Filter_Min : a > Filter_Max ? Filter_Max : a);
		}
		#endregion
	}
}
