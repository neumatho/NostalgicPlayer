/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mixer
	{
		private const c_int DownMix_Shift = 12;
		private const c_int Lim16_Hi = 32767;
		private const c_int Lim16_Lo = -32768;

		private const c_int AntiClick_FPShift = 24;

		private const c_int Loop_Prologue = 1;
		private const c_int Loop_Epilogue = 2;

		private delegate void Mix_Fp(Mixer_Voice vi, CPointer<int32> buffer, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R);

		#region Loop_Data class
		private class Loop_Data
		{
			public CPointer<byte> SPtr;
			public c_int Start;
			public c_int End;
			public bool First_Loop;
			public bool _16Bit;
			public bool Active;
			public c_int Prologue_Num;
			public c_int Epilogue_Num;
			public readonly uint8[] Prologue = new uint8[Loop_Prologue * 2 /* 16 bit */ * 2 /* stereo */];
			public readonly uint8[] Epilogue = new uint8[Loop_Epilogue * 2 /* 16 bit */ * 2 /* stereo */];
		}
		#endregion

		#region VisualizerChannel class
		private class VisualizerChannel
		{
			public bool Muted;
			public bool NoteKicked;
			public short SampleNumber;
			public uint SampleLength;
			public bool Looping;
			public int? SamplePosition;
			public ushort? Volume;
			public uint? Frequency;
		}
		#endregion

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		private readonly Mix_Fp[] nearest_Mixers;
		private readonly Mix_Fp[] linear_Mixers;
		private readonly Mix_Fp[] spline_Mixers;

		private VisualizerChannel[] visualizerChannels;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;

			nearest_Mixers = new Mix_Fp[]
			{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};

			linear_Mixers = new Mix_Fp[]
			{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};

			spline_Mixers = new Mix_Fp[]
			{
				Mix_All.LibXmp_Mix_MonoOut_Mono_8Bit_Spline,
				Mix_All.LibXmp_Mix_MonoOut_Mono_16Bit_Spline,
				Mix_All.LibXmp_Mix_MonoOut_Stereo_8Bit_Spline,
				Mix_All.LibXmp_Mix_MonoOut_Stereo_16Bit_Spline,
				Mix_All.LibXmp_Mix_StereoOut_Mono_8Bit_Spline,
				Mix_All.LibXmp_Mix_StereoOut_Mono_16Bit_Spline,
				Mix_All.LibXmp_Mix_StereoOut_Stereo_8Bit_Spline,
				Mix_All.LibXmp_Mix_StereoOut_Stereo_16Bit_Spline,
				Mix_All.LibXmp_Mix_MonoOut_Mono_8Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_MonoOut_Mono_16Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_MonoOut_Stereo_8Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_MonoOut_Stereo_16Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_StereoOut_Mono_8Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_StereoOut_Mono_16Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_StereoOut_Stereo_8Bit_Spline_Filter,
				Mix_All.LibXmp_Mix_StereoOut_Stereo_16Bit_Spline_Filter,
			};
		}



		/********************************************************************/
		/// <summary>
		/// Initialize new array with visualizer channels to be filled out
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Prepare_Frame()
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			visualizerChannels = ArrayHelper.InitializeArray<VisualizerChannel>(mod.Chn);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the required number of sample frames to render a tick
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Mixer_Get_TickSize(c_int freq, c_double time_Factor, c_double rRate, c_int bpm)
		{
			if ((freq < 0) || (bpm <= 0) || (time_Factor <= 0.0) || (rRate <= 0.0))
				return -1;

			c_double calc = freq * time_Factor * rRate / bpm / 1000;
			if ((calc > c_int.MaxValue) || c_double.IsNaN(calc))
				return -1;

			c_int tickSize = (c_int)calc;

			if (tickSize < (1 << Constants.AntiClick_Shift))
				tickSize = 1 << Constants.AntiClick_Shift;

			return tickSize;
		}



		/********************************************************************/
		/// <summary>
		/// Fill the output buffer calling one of the handlers. The buffer
		/// contains sound for one tick (a PAL frame or 1/50s for standard
		/// vblank-timed mods)
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SoftMixer()
		{
			Player_Data p = ctx.P;
			Mixer_Data s = ctx.S;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Extra_Sample_Data xtra;
			Xmp_Sample xxs;
			Mixer_Voice vi;
			Loop_Data loop_Data = new Loop_Data();
			c_double step, step_Dir;
			c_int samples, size;
			c_int vol, vol_L, vol_R, voc, uSmp;
			c_int prev_L, prev_R = 0;
			CPointer<c_int> buf_Pos;
			Mix_Fp mix_Fn;
			Mix_Fp[] mixerSet;

			switch (s.Interp)
			{
				case Xmp_Interp.Nearest:
				{
					mixerSet = nearest_Mixers;
					break;
				}

				case Xmp_Interp.Linear:
				{
					mixerSet = linear_Mixers;
					break;
				}

				case Xmp_Interp.Spline:
				{
					mixerSet = spline_Mixers;
					break;
				}

				default:
				{
					mixerSet = linear_Mixers;
					break;
				}
			}

			// OpenMPT Bidi-Loops.it: "In Impulse Tracker's software
			// mixer, ping-pong loops are shortened by one sample."
			s.BiDir_Adjust = Common.Is_Player_Mode_It(m) ? 1 : 0;

			LibXmp_Mixer_Prepare();

			if (s.Interp == Xmp_Interp.None)
				return;

			for (voc = 0; voc < p.Virt.MaxVoc; voc++)
			{
				c_int c5Spd, rampSize, delta_L, delta_R;

				vi = p.Virt.Voice_Array[voc];

				if ((vi.Flags & Mixer_Flag.AntiClick) != 0)
				{
					if (s.Interp > Xmp_Interp.Nearest)
						Do_AntiClick(voc, null, 0);

					vi.Flags &= ~Mixer_Flag.AntiClick;
				}

				if (vi.Chn < 0)
					continue;

				if (vi.Period < 1)
				{
					lib.virt.LibXmp_Virt_ResetVoice(voc, true);
					continue;
				}

				// Negative positions can be left over from some
				// loop edge cases. These can be safely clamped
				if (vi.Pos < 0.0)
					vi.Pos = 0.0;

				vi.Pos0 = (c_int)vi.Pos;

				buf_Pos = s.Buf32;
				vol = vi.Vol;

				// Mix volume (S3M and IT)
				if ((m.MVolBase > 0) && (m.MVol != m.MVolBase))
					vol = vol * m.MVol / m.MVolBase;

				if (vi.Pan == Constants.Pan_Surround)
				{
					vol_L = vol * 0x80;

					if (s.EnableSurround)
						vol_R = -vol * 0x80;
					else
						vol_R = vol * 0x80;
				}
				else
				{
					vol_L = vol * (0x80 - vi.Pan);
					vol_R = vol * (0x80 + vi.Pan);
				}

				// Sample is paused - skip channel unless a new sample is queued
				if ((vi.Flags & Mixer_Flag.Sample_Paused) != 0)
				{
					if (((~vi.Flags & Mixer_Flag.Sample_Queued) != 0) || (vi.Queued.Smp < 0))
					{
						vi.Flags &= ~Mixer_Flag.Sample_Queued;
						continue;
					}

					Hotswap_Sample(vi, voc, vi.Queued.Smp);
					Get_Current_Sample(vi, out xxs, out xtra, out c5Spd);

					vi.Pos = vi.Start;
				}
				else
					Get_Current_Sample(vi, out xxs, out xtra, out c5Spd);

				if (xxs == null)
					continue;

				step = Constants.C4_Period * c5Spd / s.Freq / vi.Period;

				// Don't allow <=0, otherwise m5v-nwlf.it crashes
				// Extremely high values that can cause undefined float/int
				// conversion are also possible for c5spd modules
				if ((step < 0.001) || (step > c_short.MaxValue))
					continue;

				Init_Sample_Wraparound(s, loop_Data, vi, xxs);

				rampSize = s.TickSize >> Constants.AntiClick_Shift;
				delta_L = (vol_L - vi.Old_VL) / rampSize;
				delta_R = (vol_R - vi.Old_VR) / rampSize;

				for (size = uSmp = s.TickSize; size > 0; )
				{
					bool split_NoLoop = false;

					if (p.Xc_Data[vi.Chn].Split != 0)
						split_NoLoop = true;

					// How many samples we can write before the loop break
					// or sample end...
					if ((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
					{
						if (vi.Pos >= vi.End)
						{
							samples = 0;

							if (--uSmp <= 0)
								break;
						}
						else
						{
							c_double c = Math.Ceiling((vi.End - vi.Pos) / step);

							// ...inside the tick boundaries
							if (c > size)
								c = size;

							samples = (c_int)c;
						}

						step_Dir = step;
					}
					else
					{
						// Reverse
						if (vi.Pos <= vi.Start)
						{
							samples = 0;

							if (--uSmp <= 0)
								break;
						}
						else
						{
							c_double c = Math.Ceiling((vi.Pos - vi.Start) / step);

							if (c > size)
								c = size;

							samples = (c_int)c;
						}

						step_Dir = -step;
					}

					if (vi.Vol != 0)
					{
						c_int mix_Size = samples;
						Mixer_Index_Flag mixer_Id = vi.FIdx & Mixer_Index_Flag.FlagMask;

						if ((~s.Format & Xmp_Format.Mono) != 0)
							mix_Size *= 2;

						// For Hipolito's anticlick routine
						if (samples > 0)
						{
							if ((~s.Format & Xmp_Format.Mono) != 0)
							{
								prev_L = buf_Pos[mix_Size - 2];
								prev_R = buf_Pos[mix_Size - 1];
							}
							else
								prev_L = buf_Pos[mix_Size - 1];
						}
						else
							prev_R = prev_L = 0;

						// See OpenMPT env-flt-max.it
						if ((vi.Filter.CutOff >= 0xfe) && (vi.Filter.Resonance == 0))
							mixer_Id &= ~Mixer_Index_Flag.Filter;

						mix_Fn = mixerSet[(c_int)mixer_Id];

						// Call the output handler
						if ((samples > 0) && vi.SPtr.IsNotNull)
						{
							c_int rSize = 0;

							if (rampSize > samples)
								rampSize -= samples;
							else
							{
								rSize = samples - rampSize;
								rampSize = 0;
							}

							if ((delta_L == 0) && (delta_R == 0))
							{
								// No need to ramp
								rSize = samples;
							}

							if (mix_Fn != null)
								mix_Fn(vi, buf_Pos, samples, vol_L >> 8, vol_R >> 8, (c_int)(step_Dir * (1 << Constants.SMix_Shift)), rSize, delta_L, delta_R);

							buf_Pos += mix_Size;
							vi.Old_VL += samples * delta_L;
							vi.Old_VR += samples * delta_R;

							// For Hipolito's anticlick routine
							if ((~s.Format & Xmp_Format.Mono) != 0)
							{
								vi.SLeft = buf_Pos[-2] - prev_L;
								vi.SRight = buf_Pos[-1] - prev_R;
							}
							else
								vi.SLeft = buf_Pos[-1] - prev_L;
						}
					}

					vi.Pos += step_Dir * samples;

					size -= samples;

					// One-shot samples do not loop
					if ((!Has_Active_Loop(vi, xxs) || split_NoLoop) && ((vi.Flags & Mixer_Flag.Sample_Queued) == 0))
					{
						if (size > 0)
						{
							Do_AntiClick(voc, buf_Pos, size);
							Set_Sample_End(voc, 1);

							// Next sample should ramp
							vol_L = vol_R = 0;
						}

						size = 0;
						continue;
					}

					// Loop before continuing to the next channel if the
					// tick is complete. This is particularly important
					// for reverse loops to avoid position clamping
					if ((size > 0) ||
						(((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos >= vi.End)) ||
						(((vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos <= vi.Start)))
					{
						if ((vi.Flags & Mixer_Flag.Sample_Queued) != 0)
						{
							// Protracker sample swap
							Do_AntiClick(voc, buf_Pos, size);

							if ((vi.Queued.Smp < 0) || (!Has_Active_Loop(vi, xxs) && ((mod.Xxs[vi.Queued.Smp].Flg & Xmp_Sample_Flag.Loop) == 0)))
							{
								// Invalid samples and one-shots that
								// are being replaced by one shots
								// (OpenMPT PTStoppedSwap.mod) stop
								// the current sample. If the current
								// sample is looped, it needs to be paused
								vi.Flags &= ~Mixer_Flag.Sample_Queued;
								vi.Flags |= Mixer_Flag.Sample_Paused;
								Set_Sample_End(voc, 1);

								// Next sample should ramp
								vol_L = vol_R = 0;
								size = 0;
								continue;
							}

							Reset_Sample_Wraparound(loop_Data);
							Hotswap_Sample(vi, voc, vi.Queued.Smp);
							Get_Current_Sample(vi, out xxs, out xtra, out c5Spd);
							Init_Sample_Wraparound(s, loop_Data, vi, xxs);

							vi.Pos = vi.Start;
							continue;
						}

						if (Loop_Reposition(vi, xxs, xtra))
						{
							Reset_Sample_Wraparound(loop_Data);
							Init_Sample_Wraparound(s, loop_Data, vi, xxs);
						}
					}
				}

				Reset_Sample_Wraparound(loop_Data);
				vi.Old_VL = vol_L;
				vi.Old_VR = vol_R;
			}

			// Render final frame
			size = s.TickSize;
			if ((~s.Format & Xmp_Format.Mono) != 0)
				size *= 2;

			if (size > Constants.Xmp_Max_FrameSize)
				size = Constants.Xmp_Max_FrameSize;

			if ((s.Format & Xmp_Format._8Bit) != 0)
				DownMix_Int_8Bit(s.Buffer, s.Buf32, size, s.Amplify, (s.Format & Xmp_Format.Unsigned) != 0 ? 0x80 : 0);
			else
				DownMix_Int_16Bit(MemoryMarshal.Cast<sbyte, int16>(s.Buffer.AsSpan()), s.Buf32, size, s.Amplify, (s.Format & Xmp_Format.Unsigned) != 0 ? 0x8000 : 0);

			s.DtRight = s.DtLeft = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_VoicePos(c_int voc, c_double pos, bool ac)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];
			Xmp_Sample xxs;
			Extra_Sample_Data xtra;

			// Position changes e.g. retrigger make the new sample take effect
			// if queued (OpenMPT InstrSwapRetrigger.mod)
			if ((vi.Flags & Mixer_Flag.Sample_Queued) != 0)
			{
				vi.Flags &= ~Mixer_Flag.Sample_Queued;

				if (vi.Queued.Smp < 0)
					vi.Flags |= Mixer_Flag.Sample_Paused;
				else if (vi.Smp != vi.Queued.Smp)
					Hotswap_Sample(vi, voc, vi.Queued.Smp);

				vi.Flags |= Mixer_Flag.Sample_Loop;
			}

			if (vi.Smp < m.Mod.Smp)
			{
				xxs = m.Mod.Xxs[vi.Smp];
				xtra = m.Xtra[vi.Smp];
			}
			else
				return;

			if (((xxs.Flg & Xmp_Sample_Flag.Synth) != 0) || ((xxs.Flg & Xmp_Sample_Flag.Adlib) != 0))
				return;

			vi.Pos = pos;

			Adjust_Voice_End(vi, xxs, xtra);

			if (vi.Pos >= vi.End)
			{
				vi.Pos = vi.End;

				// Restart forward sample loops
				if (((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && Has_Active_Loop(vi, xxs))
					Loop_Reposition(vi, xxs, xtra);
			}
			else if (((vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos <= 0.1))
			{
				// Hack: 0 maps to the end for reversed samples
				vi.Pos = vi.End;
			}

			if (ac)
			{
				AntiClick(vi);

				VisualizerChannel visualizerChannel = visualizerChannels[vi.Root];

				visualizerChannel.Muted = false;
				visualizerChannel.NoteKicked = true;
				visualizerChannel.SampleNumber = (short)vi.Smp;
				visualizerChannel.SamplePosition = (int)vi.Pos;
				visualizerChannel.SampleLength = (uint)vi.End;
				visualizerChannel.Looping = Has_Active_Loop(vi, xxs);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Mixer_GetVoicePos(c_int voc)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(vi.Smp);

			if (((xxs.Flg & Xmp_Sample_Flag.Synth) != 0) || ((xxs.Flg & Xmp_Sample_Flag.Adlib) != 0))
				return 0;

			return vi.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPatch(c_int voc, c_int smp, bool ac)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(smp);

			vi.Smp = smp;
			vi.Vol = 0;
			vi.Pan = 0;
			vi.Flags &= ~(Mixer_Flag.Sample_Loop | Mixer_Flag.Sample_Queued | Mixer_Flag.Sample_Paused | Mixer_Flag.Voice_Reverse | Mixer_Flag.Voice_BiDir);

			vi.FIdx = 0;

			if ((~s.Format & Xmp_Format.Mono) != 0)
				vi.FIdx |= Mixer_Index_Flag.StereoOut;

			Set_Sample_End(voc, 0);

			vi.SPtr = xxs.Data;
			vi.FIdx |= Mixer_Index_Flag.Active;

			if (Common.Has_Quirk(m, Quirk_Flag.Filter) && ((s.Dsp & Xmp_Dsp.LowPass) != 0))
				vi.FIdx |= Mixer_Index_Flag.Filter;

			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
				vi.FIdx |= Mixer_Index_Flag._16_Bits;

			if ((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0)
				vi.FIdx |= Mixer_Index_Flag.Stereo;

			LibXmp_Mixer_VoicePos(voc, 0, ac);
		}



		/********************************************************************/
		/// <summary>
		/// Replace the current playing sample when it reaches the end of its
		/// sample loop, a la Protracker 1/2. The new sample will begin
		/// playing at the start of its loop if it is looped, the start of
		/// the sample if it is a one-shot, and it will not play and instead
		/// pause the channel if both the original and the new sample are
		/// one-shots or if the new sample is empty/invalid/-1
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_QueuePatch(c_int voc, c_int smp)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if ((smp != vi.Smp) || ((vi.Flags & Mixer_Flag.Sample_Paused) != 0))
			{
				vi.Queued.Smp = smp;
				vi.Flags |= Mixer_Flag.Sample_Queued;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetNote(c_int voc, c_int note)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			// FIXME: Workaround for crash on notes that are too high
			//        see 6nations.it (+114 transposition on instrument 16)
			if (note > 149)
				note = 149;

			vi.Note = note;
			vi.Period = lib.period.LibXmp_Note_To_Period_Mix(note, 0);

			AntiClick(vi);

			visualizerChannels[vi.Root].Frequency = FindFrequency(vi.Smp, vi.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPeriod(c_int voc, c_double period)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			vi.Period = period;

			visualizerChannels[vi.Root].Frequency = FindFrequency(vi.Smp, vi.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetVol(c_int voc, c_int vol)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if (vol == 0)
				AntiClick(vi);

			vi.Vol = vol;

			visualizerChannels[vi.Root].Volume = (ushort)(vol / 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Release(c_int voc, bool rel)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if (rel)
			{
				// Cancel voice reverse when releasing an active sustain loop,
				// unless the main loop is bidirectional. This is done both for
				// bidirectional sustain loops and for forward sustain loops
				// that have been reversed with MPT S9F Play Backward
				if ((~vi.Flags & Mixer_Flag.Voice_Release) != 0)
				{
					Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(vi.Smp);

					if (Has_Active_Sustain_Loop(vi, xxs) && ((~xxs.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0))
						vi.Flags &= ~Mixer_Flag.Voice_Reverse;
				}

				vi.Flags |= Mixer_Flag.Voice_Release;
			}
			else
				vi.Flags &= ~Mixer_Flag.Voice_Release;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Reverse(c_int voc, bool rev)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			// Don't reverse samples that have already ended
			if ((~vi.FIdx & Mixer_Index_Flag.Active) != 0)
				return;

			if (rev)
				vi.Flags |= Mixer_Flag.Voice_Reverse;
			else
				vi.Flags &= ~Mixer_Flag.Voice_Reverse;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetEffect(c_int voc, Dsp_Effect type, c_int val)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			switch (type)
			{
				case Dsp_Effect.CutOff:
				{
					vi.Filter.CutOff = val;
					break;
				}

				case Dsp_Effect.Resonance:
				{
					vi.Filter.Resonance = val;
					break;
				}

				case Dsp_Effect.Filter_A0:
				{
					vi.Filter.A0 = val;
					break;
				}

				case Dsp_Effect.Filter_B0:
				{
					vi.Filter.B0 = val;
					break;
				}

				case Dsp_Effect.Filter_B1:
				{
					vi.Filter.B1 = val;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPan(c_int voc, c_int pan)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			vi.Pan = pan;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Mixer_NumVoices(c_int num)
		{
			Mixer_Data s = ctx.S;

			if ((num > s.NumVoc) || (num < 0))
				return s.NumVoc;
			else
				return num;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Mixer_On(c_int rate, Xmp_Format format, c_int c4Rate)
		{
			Mixer_Data s = ctx.S;

			s.Buffer = new int8[Constants.Xmp_Max_FrameSize * sizeof(int16)];
			if (s.Buffer.IsNull)
				goto Err;

			s.Buf32 = new int32[Constants.Xmp_Max_FrameSize];
			if (s.Buf32.IsNull)
				goto Err1;

			s.Freq = rate;
			s.Format = format;
			s.Amplify = Constants.Default_Amplify;
			s.Mix = Constants.Default_Mix;
			s.Interp = Xmp_Interp.Linear;	// Default interpolation type
			s.Dsp = Xmp_Dsp.LowPass;		// Enable filters by default
			s.DtRight = s.DtLeft = 0;
			s.BiDir_Adjust = 0;
			s.EnableSurround = true;

			return 0;

			Err1:
			s.Buffer.SetToNull();
			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Off()
		{
			Mixer_Data s = ctx.S;

			s.Buf32.SetToNull();
			s.Buffer.SetToNull();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_ResetChannel(c_int voc)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if (vi.Root != -1)
				visualizerChannels[vi.Root].Muted = true;
		}



		/********************************************************************/
		/// <summary>
		/// Will return the visualizer channels
		/// </summary>
		/********************************************************************/
		public ChannelChanged[] LibXmp_Mixer_GetVisualizerChannels()
		{
			Player_Data p = ctx.P;

			return visualizerChannels.Select((x, i) =>
				new ChannelChanged(!p.Channel_Mute[i], x.Muted, x.NoteKicked, x.SampleNumber, -1, -1, x.SampleLength, x.Looping, false, x.SamplePosition, x.Volume, x.Frequency)).ToArray();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Downmix 32bit samples to 8bit, signed or unsigned, mono or stereo
		/// output
		/// </summary>
		/********************************************************************/
		private void DownMix_Int_8Bit(CPointer<sbyte> dest, CPointer<int32> src, c_int num, c_int amp, c_int offs)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Downmix 32bit samples to 16bit, signed or unsigned, mono or
		/// stereo output
		/// </summary>
		/********************************************************************/
		private void DownMix_Int_16Bit(Span<int16> dest, CPointer<int32> src, c_int num, c_int amp, c_int offs)
		{
			c_int shift = DownMix_Shift - amp;

			for (c_int offset = 0; num-- != 0; offset++)
			{
				c_int smp = src[offset] >> shift;
				if (smp > Lim16_Hi)
					dest[offset] = Lim16_Hi;
				else if (smp < Lim16_Lo)
					dest[offset] = Lim16_Lo;
				else
					dest[offset] = (int16)smp;

				if (offs != 0)
					dest[offset] += (int16)offs;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AntiClick(Mixer_Voice vi)
		{
			vi.Flags |= Mixer_Flag.AntiClick;
			vi.Old_VL = 0;
			vi.Old_VR = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Ok, it's messy, but it works :-) Hipolito
		/// </summary>
		/********************************************************************/
		private void Do_AntiClick(c_int voc, CPointer<int32> buf, c_int count)
		{
			Player_Data p = ctx.P;
			Mixer_Data s = ctx.S;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];
			c_int discharge = s.TickSize >> Constants.AntiClick_Shift;

			c_int smp_L = vi.SLeft;
			c_int smp_R = vi.SRight;
			vi.SRight = vi.SLeft = 0;

			if ((smp_L == 0) && (smp_R == 0))
				return;

			if (buf.IsNull)
			{
				buf = s.Buf32;
				count = discharge;
			}
			else if (count > discharge)
			{
				count = discharge;
			}

			if (count <= 0)
				return;

			c_int stepVal = (1 << AntiClick_FPShift) / count;
			c_int stepMul = stepVal * count;

			if ((~s.Format & Xmp_Format.Mono) != 0)
			{
				while ((stepMul -= stepVal) > 0)
				{
					// Truncate to 16-bits of precision so the product is 32-bits
					uint32 stepMul_Sq = (uint32)(stepMul >> (AntiClick_FPShift - 16));
					stepMul_Sq *= stepMul_Sq;

					buf[0] += (int32)((stepMul_Sq * smp_L) >> 32);
					buf[1] += (int32)((stepMul_Sq * smp_R) >> 32);
					buf += 2;
				}
			}
			else
			{
				while ((stepMul -= stepVal) > 0)
				{
					// Truncate to 16-bits of precision so the product is 32-bits
					uint32 stepMul_Sq = (uint32)(stepMul >> (AntiClick_FPShift - 16));
					stepMul_Sq *= stepMul_Sq;

					buf[0] += (int32)((stepMul_Sq * smp_L) >> 32);
					buf++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Sample_End(c_int voc, c_int end)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if ((uint32)voc >= p.Virt.MaxVoc)
				return;

			Channel_Data xc = p.Xc_Data[vi.Chn];

			if (end != 0)
			{
				xc.Note_Flags |= Note_Flag.Sample_End;
				vi.FIdx &= ~Mixer_Index_Flag.Active;

				if (Common.Has_Quirk(m, Quirk_Flag.RstChn))
					lib.virt.LibXmp_Virt_ResetVoice(voc, false);
			}
			else
				xc.Note_Flags &= ~Note_Flag.Sample_End;
		}



		/********************************************************************/
		/// <summary>
		/// Backup sample data before and after loop and replace it for
		/// interpolation.
		/// TODO: If higher order interpolation than spline is added, the
		/// copy needs to properly wrap around the loop data (modulo) for
		/// correct small loops
		/// TODO: Use an overlap buffer like OpenMPT? This is easier, but a
		/// little dirty
		/// </summary>
		/********************************************************************/
		private void Init_Sample_Wraparound(Mixer_Data s, Loop_Data ld, Mixer_Voice vi, Xmp_Sample xxs)
		{
			c_int prologue_Num = Loop_Prologue;
			c_int epilogue_Num = Loop_Epilogue;

			if (vi.SPtr.IsNull || (s.Interp == Xmp_Interp.Nearest) || ((~xxs.Flg & Xmp_Sample_Flag.Loop) != 0))
			{
				ld.Active = false;
				return;
			}

			ld.SPtr = vi.SPtr;
			ld.Start = vi.Start;
			ld.End = vi.End;
			ld.First_Loop = !((vi.Flags & Mixer_Flag.Sample_Loop) != 0);
			ld._16Bit = (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0;
			ld.Active = true;

			// Stereo
			if ((xxs.Flg & Xmp_Sample_Flag.Stereo) != 0)
			{
				ld.Start <<= 1;
				ld.End <<= 1;

				prologue_Num <<= 1;
				epilogue_Num <<= 1;
			}

			ld.Prologue_Num = prologue_Num;
			ld.Epilogue_Num = epilogue_Num;

			bool biDir = (vi.Flags & Mixer_Flag.Voice_BiDir) != 0;

			if (ld._16Bit)
			{
				Span<uint16> buf = MemoryMarshal.Cast<byte, uint16>(ld.SPtr.Buffer);
				int sPtrOffset = ld.SPtr.Offset / 2;
				c_int start = ld.Start;
				c_int end = ld.End;

				CMemory.MemCpy(ld.Prologue, ld.SPtr + (start * 2 - prologue_Num * 2), prologue_Num * 2);
				CMemory.MemCpy(ld.Epilogue, ld.SPtr + end * 2, epilogue_Num * 2);

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < prologue_Num; i++)
					{
						c_int j = i - prologue_Num;
						buf[sPtrOffset + start + j] = biDir ? buf[sPtrOffset + start - 1 - j] : buf[sPtrOffset + end + j];
					}
				}

				for (c_int i = 0; i < epilogue_Num; i++)
					buf[sPtrOffset + end + i] = biDir ? buf[sPtrOffset + end - 1 - i] : buf[sPtrOffset + start + i];
			}
			else
			{
				CPointer<uint8> start = ld.SPtr + ld.Start;
				CPointer<uint8> end = ld.SPtr + ld.End;

				CMemory.MemCpy(ld.Prologue, start - prologue_Num, prologue_Num);
				CMemory.MemCpy(ld.Epilogue, end, epilogue_Num);

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < prologue_Num; i++)
					{
						c_int j = i - prologue_Num;
						start[j] = biDir ? start[-1 - j] : end[j];
					}
				}

				for (c_int i = 0; i < epilogue_Num; i++)
					end[i] = biDir ? end[-1 - i] : start[i];
			}
		}



		/********************************************************************/
		/// <summary>
		/// Restore old sample data from before and after loop
		/// </summary>
		/********************************************************************/
		private void Reset_Sample_Wraparound(Loop_Data ld)
		{
			c_int prologue_Num = ld.Prologue_Num;
			c_int epilogue_Num = ld.Epilogue_Num;

			if (!ld.Active)
				return;

			if (ld._16Bit)
			{
				c_int start = ld.Start;
				c_int end = ld.End;

				CMemory.MemCpy(ld.SPtr + (start - prologue_Num) * 2, ld.Prologue, prologue_Num * 2);
				CMemory.MemCpy(ld.SPtr + end * 2, ld.Epilogue, epilogue_Num * 2);
			}
			else
			{
				CPointer<uint8> start = ld.SPtr + ld.Start;
				CPointer<uint8> end = ld.SPtr + ld.End;

				CMemory.MemCpy(start - prologue_Num, ld.Prologue, prologue_Num);
				CMemory.MemCpy(end, ld.Epilogue, epilogue_Num);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Has_Active_Sustain_Loop(Mixer_Voice vi, Xmp_Sample xxs)
		{
			Module_Data m = ctx.M;

			return (vi.Smp < m.Mod.Smp) && ((xxs.Flg & Xmp_Sample_Flag.SLoop) != 0) && ((~vi.Flags & Mixer_Flag.Voice_Release) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Has_Active_Loop(Mixer_Voice vi, Xmp_Sample xxs)
		{
			return ((xxs.Flg & Xmp_Sample_Flag.Loop) != 0) || Has_Active_Sustain_Loop(vi, xxs);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Adjust_Voice_End(Mixer_Voice vi, Xmp_Sample xxs, Extra_Sample_Data xtra)
		{
			vi.Flags &= ~Mixer_Flag.Voice_BiDir;

			if ((xtra != null) && Has_Active_Sustain_Loop(vi, xxs))
			{
				vi.Start = xtra.Sus;
				vi.End = xtra.Sue;

				if ((xxs.Flg & Xmp_Sample_Flag.SLoop_BiDir) != 0)
					vi.Flags |= Mixer_Flag.Voice_BiDir;
			}
			else if ((xxs.Flg & Xmp_Sample_Flag.Loop) != 0)
			{
				vi.Start = xxs.Lps;

				if (((xxs.Flg & Xmp_Sample_Flag.Loop_Full) != 0) && (((~vi.Flags & Mixer_Flag.Sample_Loop) != 0)))
					vi.End = xxs.Len;
				else
				{
					vi.End = xxs.Lpe;

					if ((xxs.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0)
						vi.Flags |= Mixer_Flag.Voice_BiDir;
				}
			}
			else
			{
				vi.Start = 0;
				vi.End = xxs.Len;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Loop_Reposition(Mixer_Voice vi, Xmp_Sample xxs, Extra_Sample_Data xtra)
		{
			bool loop_Changed = !((vi.Flags & Mixer_Flag.Sample_Loop) != 0);

			vi.Flags |= Mixer_Flag.Sample_Loop;

			if (loop_Changed)
				Adjust_Voice_End(vi, xxs, xtra);

			if ((~vi.Flags & Mixer_Flag.Voice_BiDir) != 0)
			{
				// Reposition for next loop
				if ((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
					vi.Pos -= vi.End - vi.Start;
				else
					vi.Pos += vi.End - vi.Start;
			}
			else
			{
				// Bidirectional loop: switch directions
				vi.Flags ^= Mixer_Flag.Voice_Reverse;

				// Wrap voice position around endpoint
				if ((vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
				{
					// OpenMPT Bidi-Loops.it: "In Impulse Tracker's software
					// mixer, ping-pong loops are shortened by one sample."
					vi.Pos = vi.End * 2 - ctx.S.BiDir_Adjust - vi.Pos;
				}
				else
					vi.Pos = vi.Start * 2 - vi.Pos;
			}

			return loop_Changed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Hotswap_Sample(Mixer_Voice vi, c_int voc, c_int smp)
		{
			c_int vol = vi.Vol;
			c_int pan = vi.Pan;

			LibXmp_Mixer_SetPatch(voc, smp, false);

			vi.Flags |= Mixer_Flag.Sample_Loop;
			vi.Vol = vol;
			vi.Pan = pan;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Get_Current_Sample(Mixer_Voice vi, out Xmp_Sample xxs, out Extra_Sample_Data xtra, out c_int c5spd)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			if (vi.Smp < mod.Smp)
			{
				xxs = mod.Xxs[vi.Smp];
				xtra = m.Xtra[vi.Smp];
				c5spd = (c_int)m.Xtra[vi.Smp].C5Spd;
			}
			else
			{
				xxs = null;
				xtra = null;
				c5spd = 0;
				return;
			}

			Adjust_Voice_End(vi, xxs, xtra);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LibXmp_Mixer_Prepare()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;

			s.TickSize = LibXmp_Mixer_Get_TickSize(s.Freq, m.Time_Factor, m.RRate, p.Bpm);

			// Protect the mixer from broken values caused by xmp_set_tempo_factor
			if ((s.TickSize < 0) || (s.TickSize > (Constants.Xmp_Max_FrameSize / 2)))
				s.TickSize = Constants.Xmp_Max_FrameSize / 2;

			c_int byteLen = s.TickSize;
			if ((~s.Format & Xmp_Format.Mono) != 0)
				byteLen *= 2;

			CMemory.MemSet(s.Buf32, 0, byteLen);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint FindFrequency(c_int smp, c_double period)
		{
			Module_Data m = ctx.M;
			c_int c5Spd = (c_int)m.Xtra[smp].C5Spd;

			return (uint)(Constants.C4_Period * c5Spd / period);
		}
		#endregion
	}
}
