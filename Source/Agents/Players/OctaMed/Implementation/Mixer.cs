/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Mixer interface
	/// </summary>
	internal class Mixer
	{
		/// <summary>
		/// Flags for the Play() method
		/// </summary>
		[Flags]
		protected enum PlayFlag
		{
			None = 0,
			Backwards = 0x01,
			Loop = 0x02,
			PingPongLoop = 0x04
		}

		private static readonly ushort[] bpmCompVals = [ 195, 97, 65, 49, 39, 32, 28, 24, 22, 20 ];

		protected readonly OctaMedWorker worker;

		private uint channels;

		private readonly ushort[,] freqTable = new ushort[16, Constants.Octaves * 12];
		private readonly bool[] startSyn = new bool[Constants.MaxTracks];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer(OctaMedWorker worker)
		{
			// Remember the player pointer
			this.worker = worker;

			// Initialize member variables
			channels = 0;

			// Fill in the frequency table
			for (int cnt = 0; cnt < 16; cnt++)
			{
				// C-1 for this fine tune table (C-1 freq = 1046.502261 Hz)
				// Freq diff between C#1 and C-1 = 62.22826... = 8 * 7.778532836
				double calcVar = 1046.502261 + (cnt - 8) * 7.778532836;

				for (uint cnt2 = 0; cnt2 < Constants.Octaves * 12; cnt2++)
				{
					freqTable[cnt, cnt2] = (ushort)calcVar;
					calcVar *= 1.059463094;			// 12th root of 2
				}
			}

			// Initialize the startSyn table
			for (int cnt = 0; cnt < Constants.MaxTracks; cnt++)
				startSyn[cnt] = false;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency from the note given
		/// </summary>
		/********************************************************************/
		public uint GetNoteFrequency(NoteNum note, int fineTune)
		{
			if (note > 71)
				return freqTable[fineTune + 8, 71];

			return freqTable[fineTune + 8, note];
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency from the note given
		/// </summary>
		/********************************************************************/
		public uint GetInstrNoteFreq(NoteNum note, Instr i)
		{
			switch (note)
			{
				case Constants.NoteDef:
					return i.GetValidDefFreq();

				case Constants.Note11k:
					return 11025;

				case Constants.Note22k:
					return 22050;

				case Constants.Note44k:
					return 44100;

				default:
				{
					// The Amiga does not have the extra octave,
					// so we make a little hack here to get The Last Ninja II
					// to play correctly at position 26-27
					Sample samp = i.GetSample();
					if ((samp != null) && samp.IsSynthSound())
					{
						if (note <= 0x7f)
						{
							while (note > 60)
								note -= 12;

							return GetNoteFrequency((NoteNum)(note - 1), i.GetFineTune());
						}

						return 0;
					}

					if (note <= 0x7f)
					{
						if (samp != null)
							note = (NoteNum)(note + samp.GetNoteDifference((NoteNum)(note - 1)));

						return GetNoteFrequency((NoteNum)(note - 1), i.GetFineTune());
					}

					return 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency to use for the tempo given
		/// </summary>
		/********************************************************************/
		public void SetMixTempo(Tempo newTempo)
		{
			// Calculate the number of hertz the player should run in
			if (newTempo.bpm)
				worker.SetPlayingFrequency(((float)newTempo.tempo.Value * newTempo.linesPerBeat.Value) / 10);
			else
			{
				ushort tempo = newTempo.tempo.Value;
				if (tempo <= 10)
					tempo = bpmCompVals[tempo - 1];

				worker.SetPlayingFrequency(1.0f / (0.474326f / tempo * 1.3968255f));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Start(uint channels)
		{
			this.channels = channels;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Stop()
		{
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Stops the channel given
		/// </summary>
		/********************************************************************/
		protected virtual void MuteChannel(uint chNum)
		{
			DoMuteChannel(chNum);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Will begin to play the sample given with all the arguments
		/// </summary>
		/********************************************************************/
		protected void Play(uint chNum, NoteNum note, InstNum instNum, Sample smp, uint startOffs, uint loopStart, uint loopLen, PlayFlag flags)
		{
			if (chNum >= channels)
				return;

			// Fix out of range offsets
			if (startOffs >= smp.GetLength())
				startOffs = 0;

			sbyte[] sampAdr = smp.GetPlayBuffer(note, ref loopStart, ref loopLen);
			if (sampAdr != null)
			{
				PlaySampleFlag playSampleFlag = PlaySampleFlag.None;
				uint len = (uint)sampAdr.Length;

				if (smp.Is16Bit())
				{
					playSampleFlag |= PlaySampleFlag._16Bit;
					len /= 2;
				}

				if (smp.IsStereo())
				{
					playSampleFlag |= PlaySampleFlag.Stereo;
					len /= 2;
				}

				if ((flags & PlayFlag.Backwards) != 0)
					playSampleFlag |= PlaySampleFlag.Backwards;

				// Okay, tell NostalgicPlayer to play the sample
				worker.VirtualChannels[chNum].PlaySample((short)instNum, sampAdr, startOffs, len - startOffs, playSampleFlag);
			}

			// Set loop
			if (((flags & PlayFlag.Loop) != 0) && (loopLen > 2))
				worker.VirtualChannels[chNum].SetLoop(loopStart, loopLen, (flags & PlayFlag.PingPongLoop) != 0 ? ChannelLoopType.PingPong : ChannelLoopType.Normal);
		}



		/********************************************************************/
		/// <summary>
		/// Will add the "change" argument to the current position of the
		/// current playing sample
		/// </summary>
		/********************************************************************/
		protected void ChangeSamplePosition(uint chNum, int change)
		{
			if (chNum >= channels)
				return;

			worker.VirtualChannels[chNum].SetPosition(change, true);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the sample position to the value given
		/// </summary>
		/********************************************************************/
		protected void SetSamplePosition(uint chNum, int newPos)
		{
			if (chNum >= channels)
				return;

			worker.VirtualChannels[chNum].SetPosition(newPos, false);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the frequency of the channel given
		/// </summary>
		/********************************************************************/
		protected void SetChannelFreq(uint chNum, int freq)
		{
			if (chNum >= channels)
				return;

			if ((freq <= 0) || (freq > 65535))
			{
				DoMuteChannel(chNum);
				return;
			}

			// Tell NostalgicPlayer about the frequency change
			worker.VirtualChannels[chNum].SetFrequency((uint)freq);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the channel's volume and panning
		/// </summary>
		/********************************************************************/
		protected void SetChannelVolPan(uint chNum, ushort volume, short pan)
		{
			if (chNum >= channels)
				return;

			// Now set the volume and panning
			worker.VirtualChannels[chNum].SetVolume((ushort)(volume * 2));
			worker.VirtualChannels[chNum].SetPanning((ushort)(pan * 8 + 128));
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the channel to be ready to play a synth
		/// </summary>
		/********************************************************************/
		protected void PrepareSynthSound(uint chNum)
		{
			if (chNum < channels)
				startSyn[chNum] = true;
		}



		/********************************************************************/
		/// <summary>
		/// Will play the synth sound
		/// </summary>
		/********************************************************************/
		protected void SetSynthWaveform(uint chNum, InstNum instNum, sbyte[] data, uint length)
		{
			if ((chNum < channels) && (length > 0))
			{
				// Should we trig the sound?
				if (startSyn[chNum])
				{
					// Yes, do it
					worker.VirtualChannels[chNum].PlaySample((short)instNum, data, 0, length);
					startSyn[chNum] = false;
				}
				else
					worker.VirtualChannels[chNum].SetSample(data, 0, length);

				// Set the loop
				worker.VirtualChannels[chNum].SetLoop(0, length);
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Stops the channel given
		/// </summary>
		/********************************************************************/
		private void DoMuteChannel(uint chNum)
		{
			if (chNum < channels)
				worker.VirtualChannels[chNum].Mute();
		}
		#endregion
	}
}
