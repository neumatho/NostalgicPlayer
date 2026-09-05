/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Extensions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// libopenmpt extensions - implementation
	/// </summary>
	internal class Module_Ext_Impl : Module_Impl
	{
		private IInteractive interactive;
		private INostalgicPlayer nostalgicPlayer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Module_Ext_Impl(Stream stream, Guid? formatId, map<string, string> ctls) : base(stream, formatId, ctls)
		{
			Ctor();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IExtension Get_Interface(string interface_Id)
		{
			if (string.IsNullOrEmpty(interface_Id))
				return null;

			if (interface_Id == "interactive")
				return interactive;
			else if (interface_Id == "nostalgicplayer")
				return nostalgicPlayer;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Ctor()
		{
			interactive = new Interactive(this);
			nostalgicPlayer = new NostalgicPlayer(this);

			// Add stuff here
		}

		#region Interactive
		private sealed class Interactive : IInteractive
		{
			private readonly Module_Ext_Impl impl;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Interactive(Module_Ext_Impl impl)
			{
				this.impl = impl;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void Set_Channel_Mute_Status(int32_t channel, bool mute)//XX 196
			{
				if ((channel < 0) || (channel >= impl.Get_Num_Channels()))
					throw new OpenMptException("Invalid channel");

				impl.m_SndFile.ChnSettings[channel].dwFlags.Set(SampleFlags.Chn_Mute | SampleFlags.Chn_SyncMute, mute);
				impl.m_SndFile.m_PlayState.Chn[channel].dwFlags.Set(SampleFlags.Chn_Mute | SampleFlags.Chn_SyncMute, mute);

				// Also update NNA channels
				for (ChannelIndex i = impl.m_SndFile.GetNumChannels(); i < Snd_Def.Max_Channels; i++)
				{
					if (impl.m_SndFile.m_PlayState.Chn[i].nMasterChn == (channel + 1))
						impl.m_SndFile.m_PlayState.Chn[i].dwFlags.Set(SampleFlags.Chn_Mute | SampleFlags.Chn_SyncMute, mute);
				}
			}
		}
		#endregion

		#region NostalgicPlayer
		private sealed class NostalgicPlayer : INostalgicPlayer
		{
			private readonly Module_Ext_Impl impl;

			// Interface to NostalgicPlayer visualizations
			private Dictionary<ModSample, short> sampleLookup;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicPlayer(Module_Ext_Impl impl)
			{
				this.impl = impl;
			}



			/********************************************************************/
			/// <summary>
			/// Return sample information for the given sample
			/// </summary>
			/********************************************************************/
			public SampleInformation GetSampleInformation(int32_t sampleNumber)
			{
				if ((sampleNumber < 0) || (sampleNumber >= impl.Get_Num_Samples()))
					return null;

				ModSample sample = impl.m_SndFile.GetSample((SampleIndex)(sampleNumber + 1));
				uint bytesPerFrame = sample.GetBytesPerSample();

				Array sampleData = null;
				uint sampleOffset = 0;

				if ((sample.pData != null) && MemoryMarshal.TryGetArray(sample.pData.Cast<byte>().AsMemory(), out ArraySegment<byte> segment))
				{
					sampleData = segment.Array;

					// NostalgicPlayer wants all the positions in frames, but the
					// array we get is in bytes, so it need to be converted
					sampleOffset = (uint)segment.Offset / bytesPerFrame;
				}

				SampleInformation.SampleFlags flags = SampleInformation.SampleFlags.None;

				if (sample.uFlags.Test(SampleFlags.Chn_16Bit))
					flags |= SampleInformation.SampleFlags._16Bit;

				if (sample.uFlags.Test(SampleFlags.Chn_Stereo))
					flags |= SampleInformation.SampleFlags.Stereo;

				if (sample.uFlags.Test(SampleFlags.Chn_Loop))
					flags |= SampleInformation.SampleFlags.Loop;

				if (sample.uFlags.Test(SampleFlags.Chn_PingPongLoop))
					flags |= SampleInformation.SampleFlags.PingPong;

				return new SampleInformation
				{
					Name = impl.m_SndFile.m_ModFormat.CharSet.GetString(impl.m_SndFile.m_szNames[sampleNumber + 1].Buf),
					Length = sample.nLength,
					LoopStart = sample.nLoopStart,
					LoopLength = sample.nLoopEnd - sample.nLoopStart,
					SampleData = sampleData,
					SampleOffset = sampleOffset,
					Volume = sample.nVolume,
					Panning = (short)(sample.uFlags.Test(SampleFlags.Chn_Panning) ? Math.Clamp(sample.nPan, (ushort)0, (ushort)255) : -1),
					Flags = flags,
					NoteFrequencies = BuildNoteFrequencyTable(sample)
				};
			}



			/********************************************************************/
			/// <summary>
			/// Return any extra information from the loader or null
			/// </summary>
			/********************************************************************/
			public string GetExtraInformation()
			{
				return impl.m_SndFile.m_ModFormat.ExtraInformation;
			}



			/********************************************************************/
			/// <summary>
			/// Return true if the module uses surround
			/// </summary>
			/********************************************************************/
			public bool DoesModuleUseSurround()
			{
				CSoundFile sndFile = impl.m_SndFile;

				// First check the initial channel settings
				for (ChannelIndex i = 0; i < sndFile.GetNumChannels(); i++)
				{
					if (sndFile.ChnSettings[i].dwFlags.Test(ChannelFlags.Chn_Surround))
						return true;
				}

				// Only some formats enable surround by panning to a specific value
				bool panToSurround = (sndFile.GetType_() & (ModType.S3M | ModType.Dsm | ModType.Amf0 | ModType.Amf | ModType.Mtm)) != 0;

				// The player will run the surround command no matter which format
				// the module is, so make sure the format really has the command.
				// Some modules do contain commands which their own format does
				// not support and those should just be ignored.
				//
				// MOD is an exception. It does not have the command itself, but
				// the loader converts 7-bit panning + surround (8A4) into it
				CModSpecifications specs = sndFile.GetModSpecifications();
				bool isMod = (sndFile.GetType_() & ModType.Mod) != 0;

				// Now check all the patterns for effects enabling surround
				foreach (CPattern pattern in sndFile.Patterns)
				{
					foreach (ModCommand m in pattern.m_ModCommands)
					{
						switch (m.Command)
						{
							// S91 / X91: Surround on
							case EffectCommand.S3MCmdEx:
							{
								if ((m.Param == 0x91) && (specs.HasCommand(m.Command) || isMod))
									return true;

								break;
							}

							case EffectCommand.XFinePortaUpDown:
							{
								if ((m.Param == 0x91) && specs.HasCommand(m.Command))
									return true;

								break;
							}

							// 7-bit panning + surround
							case EffectCommand.Panning8:
							{
								if (panToSurround && (m.Param == 0xa4))
									return true;

								break;
							}
						}
					}
				}

				return false;
			}



			/********************************************************************/
			/// <summary>
			/// Enable/disable surround
			/// </summary>
			/********************************************************************/
			public void EnableSurround(bool enabled)
			{
				impl.m_SndFile.m_PlayState.m_SurroundEnabled = enabled;
			}



			/********************************************************************/
			/// <summary>
			/// Get current number of samples per tick
			/// </summary>
			/********************************************************************/
			public uint GetSamplesPerTick()
			{
				return impl.m_SndFile.m_PlayState.m_nSamplesPerTick;
			}



			/********************************************************************/
			/// <summary>
			/// Will return the visualizer channels
			/// </summary>
			/********************************************************************/
			public ChannelChanged[] GetVisualChannels()
			{
				CSoundFile sndFile = impl.m_SndFile;
				ChannelIndex numberOfChannels = sndFile.GetNumChannels();

				ChannelChanged[] result = new ChannelChanged[numberOfChannels];

				// Only the pattern channels are used. When a new note is played,
				// OpenMPT moves the old note to a background channel and reuses
				// the pattern channel for the new note, so the pattern channel
				// always holds the note currently being triggered. Because of
				// that, the visualizers will stay on the same channel instead of
				// jumping around between the NNA channels
				for (ChannelIndex i = 0; i < numberOfChannels; i++)
				{
					ModChannel chn = sndFile.m_PlayState.Chn[i];

					bool noteKicked = sndFile.m_VisualNoteKicked[i];
					sndFile.m_VisualNoteKicked[i] = false;

					// Only tell about the sample position when the visualizers
					// cannot calculate it themselves, which is when the note is
					// retriggered or the position wraps around in a loop
					SmpLength samplePosition = chn.Position.GetUInt();
					int? newSamplePosition = (int)samplePosition;

					// nRealVolume is 14-bit, but NostalgicPlayer wants 0-256
					ushort volume = (ushort)(chn.nRealVolume >> 6);

					// The increment tells how many frames of the sample that is
					// played for each mixer frame, so multiplying it with the
					// mixer frequency gives the playing frequency. It is negated
					// when playing backwards in a ping-pong loop
					uint frequency = (uint)((Math.Abs(chn.Increment.GetRaw()) * sndFile.m_MixerSettings.gdwMixingFreq) >> 32);

					bool enabled = !sndFile.ChnSettings[i].dwFlags.Test(ChannelFlags.Chn_Mute);
					bool muted = chn.dwFlags.Test(ChannelFlags.Chn_Mute | ChannelFlags.Chn_SyncMute) || (chn.pCurrentSample == null);

					result[i] = new ChannelChanged(enabled, muted, noteKicked, FindSampleNumber(chn.pModSample), -1, -1, chn.nLength, chn.dwFlags.Test(ChannelFlags.Chn_Loop), false, newSamplePosition, volume, frequency);
				}

				return result;
			}



			/********************************************************************/
			/// <summary>
			/// Find the sample number NostalgicPlayer knows the given
			/// sample as or -1 if it could not be found
			/// </summary>
			/********************************************************************/
			private short FindSampleNumber(ModSample sample)
			{
				if (sample == null)
					return -1;

				if (sampleLookup == null)
				{
					CSoundFile sndFile = impl.m_SndFile;
					SampleIndex numberOfSamples = sndFile.GetNumSamples();

					sampleLookup = new Dictionary<ModSample, short>(numberOfSamples);

					// NostalgicPlayer uses zero based sample numbers
					for (SampleIndex i = 1; i <= numberOfSamples; i++)
						sampleLookup[sndFile.GetSample(i)] = (short)(i - 1);
				}

				return sampleLookup.TryGetValue(sample, out short sampleNumber) ? sampleNumber : (short)-1;
			}



			/********************************************************************/
			/// <summary>
			/// Build a table with the frequency for each note. Finetune,
			/// transpose and middle-C speed of the sample are taken into
			/// account. Notes which cannot be played by the format, are left as
			/// zero
			/// </summary>
			/********************************************************************/
			private uint[] BuildNoteFrequencyTable(ModSample sample)
			{
				CSoundFile sndFile = impl.m_SndFile;
				bool useTranspose = sndFile.UseFineTuneAndTranspose();
				c_int lowestNote, highestNote;

				if (useTranspose)
				{
					// RealNote = PatternNote + RelativeTone; (0..118, 0 = C-0, 118 = A#9)
					lowestNote = ModCommand.Note_Min + 11;
					highestNote = ModCommand.Note_Min + 130;

					// These formats cannot play the lowest octave at all
					if (((sndFile.GetType_() & (ModType.Xm | ModType.Mtm)) != 0) || sndFile.m_SongFlags.Test(SongFlags.LinearSlides))
						lowestNote = ModCommand.Note_Min + 12;
				}
				else
				{
					lowestNote = ModCommand.Note_Min;
					highestNote = ModCommand.Note_Max;
				}

				uint[] frequencies = new uint[10 * 12];

				for (c_int i = 0; i < frequencies.Length; i++)
				{
					c_int note = i + ModCommand.Note_Min;

					if (useTranspose)
						note += sample.RelativeTone;

					if ((note < lowestNote) || (note > highestNote))
						continue;

					uint32 period = sndFile.GetPeriodFromNote((uint32)note, sample.nFineTune, sample.nC5Speed);
					if (period == 0)
						continue;

					frequencies[i] = sndFile.GetFreqFromPeriod(period, sample.nC5Speed, 0) >> Snd_Def.Freq_FracBits;
				}

				return frequencies;
			}
		}
		#endregion
	}
}
