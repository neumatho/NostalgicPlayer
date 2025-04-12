/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Agent.Player.Xmp
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class XmpWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private readonly Xmp_Format_Info currentFormat;

		private LibXmp libXmp;
		private Xmp_Module_Info moduleInfo;

		private bool hasInstruments;
		private bool useSurround;

		private int playingPosition;
		private int playingPattern;
		private int[] playingTracks;
		private int currentSpeed;
		private int currentTempo;

		private short[] leftBuffer;
		private short[] rightBuffer;
		private short[] leftRearBuffer;
		private short[] rightRearBuffer;

		private PlayerMixerInfo lastMixerInfo;

		private const int InfoPositionLine = 4;
		private const int InfoPatternOrTracksLine = 5;
		private const int InfoSpeedLine = 6;
		private const int InfoTempoLine = 7;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public XmpWorker(Guid formatId)
		{
			currentFormat = LibXmp.Xmp_Get_Format_Info_List().SkipLast(1).First(x => x.Id == formatId);
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => Xmp.fileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return some extra information about the format. If it returns
		/// null or an empty string, nothing extra is shown
		/// </summary>
		/********************************************************************/
		public override string ExtraFormatInfo => moduleInfo.Mod.Type.Equals(currentFormat.Name, StringComparison.InvariantCultureIgnoreCase) ? null : moduleInfo.Mod.Type;



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => moduleInfo.Mod.Name;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => moduleInfo.Mod.Author;



		/********************************************************************/
		/// <summary>
		/// Return the comment separated in lines
		/// </summary>
		/********************************************************************/
		public override string[] Comment => string.IsNullOrWhiteSpace(moduleInfo.Comment) ? Array.Empty<string>() : moduleInfo.Comment.Split('\n');



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			// Find out which line to take
			switch (line)
			{
				// Number of positions
				case 0:
				{
					description = Resources.IDS_XMP_INFODESCLINE0;
					value = moduleInfo.Mod.Len.ToString();
					break;
				}

				// Used patterns or Used Tracks
				case 1:
				{
					if ((moduleInfo.Flags & Xmp_Module_Flags.Uses_Tracks) != 0)
					{
						description = Resources.IDS_XMP_INFODESCLINE1b;
						value = moduleInfo.Mod.Trk.ToString();
					}
					else
					{
						description = Resources.IDS_XMP_INFODESCLINE1a;
						value = moduleInfo.Mod.Pat.ToString();
					}
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_XMP_INFODESCLINE2;
					value = moduleInfo.Mod.Ins.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_XMP_INFODESCLINE3;
					value = moduleInfo.Mod.Smp.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_XMP_INFODESCLINE4;
					value = playingPosition.ToString();
					break;
				}

				// Playing pattern or Playing tracks
				case 5:
				{
					if ((moduleInfo.Flags & Xmp_Module_Flags.Uses_Tracks) != 0)
						description = Resources.IDS_XMP_INFODESCLINE5b;
					else
						description = Resources.IDS_XMP_INFODESCLINE5a;

					value = FormatPatternOrTracks();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_XMP_INFODESCLINE6;
					value = currentSpeed.ToString();
					break;
				}

				// Current tempo (BPM)
				case 7:
				{
					description = Resources.IDS_XMP_INFODESCLINE7;
					value = currentTempo.ToString();
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.BufferMode | ModulePlayerSupportFlag.BufferDirect | ModulePlayerSupportFlag.Visualize | ModulePlayerSupportFlag.EnableChannels;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			libXmp = LibXmp.Xmp_Create_Context();

			libXmp.Xmp_Set_Load_Format(currentFormat.Id);
			int retVal = libXmp.Xmp_Load_Module_From_File(fileInfo.ModuleStream);

			if (retVal != 0)
			{
				libXmp.Xmp_Free_Context();

				errorMessage = Resources.IDS_XMP_ERR_LOADING;
				return AgentResult.Error;
			}

			libXmp.Xmp_Get_Module_Info(out moduleInfo);

			hasInstruments = (moduleInfo.Mod.Ins != moduleInfo.Mod.Smp) || moduleInfo.Mod.Xxi.Any(x => x.Nsm > 1) || moduleInfo.Mod.Xxs.Any(x => !string.IsNullOrEmpty(x.Name));
			useSurround = DoesModuleUseSurround();

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing a warning string. If there is no
		/// warning, an empty string is returned
		/// </summary>
		/********************************************************************/
		public override string GetWarning()
		{
			string[] dspEffectNames = libXmp.Xmp_Get_Used_Dsp_Effects();
			if (dspEffectNames != null)
				return string.Format(Resources.IDS_XMP_ERR_HAVE_DSP, string.Join("\n", dspEffectNames));

			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			libXmp.Xmp_Release_Module();

			libXmp.Xmp_Free_Context();
			libXmp = null;

			moduleInfo = null;

			leftBuffer = null;
			rightBuffer = null;
			leftRearBuffer = null;
			rightRearBuffer = null;

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public override bool InitSound(int songNumber, out string errorMessage)
		{
			if (!base.InitSound(songNumber, out errorMessage))
				return false;

			InitializeSound(0, Xmp_Interp.Spline);
			SetModuleInformation();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public override void CleanupSound()
		{
			libXmp.Xmp_End_Player();

			base.CleanupSound();
		}



		/********************************************************************/
		/// <summary>
		/// Set the output frequency and number of channels
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(uint mixerFrequency, int channels)
		{
			base.SetOutputFormat(mixerFrequency, channels);

			libXmp.Xmp_Set_Player(Xmp_Player.MixerFrequency, (int)mixerFrequency);
			libXmp.Xmp_Set_Player(Xmp_Player.MixerChannels, channels);
		}



		/********************************************************************/
		/// <summary>
		/// Is only called if BufferDirect is set in the SupportFlags. It
		/// tells your player about the different mixer settings you need to
		/// take care of
		/// </summary>
		/********************************************************************/
		public override void ChangeMixerConfiguration(PlayerMixerInfo mixerInfo)
		{
			base.ChangeMixerConfiguration(mixerInfo);

			libXmp.Xmp_Set_Player(Xmp_Player.Mix, mixerInfo.StereoSeparator);
			libXmp.Xmp_Set_Player(Xmp_Player.Surround, (int)mixerInfo.SurroundMode);

			for (int i = 0; i < ModuleChannelCount; i++)
			{
				bool channelEnabled = (mixerInfo.ChannelsEnabled != null) && (i < mixerInfo.ChannelsEnabled.Length) ? mixerInfo.ChannelsEnabled[i] : true;
				libXmp.Xmp_Channel_Mute(i, channelEnabled ? 0 : 1);
			}

			lastMixerInfo = mixerInfo;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			bool endReached = false;

			libXmp.Xmp_Get_Frame_Info(out Xmp_Frame_Info beforeInfo);

			libXmp.Xmp_Play_Frame();

			libXmp.Xmp_Get_Frame_Info(out Xmp_Frame_Info afterInfo);

			PlayBuffer(afterInfo);

			AmigaFilter = afterInfo.Filter;

			if (beforeInfo.Speed != afterInfo.Speed)
			{
				currentSpeed = afterInfo.Speed;

				ShowSpeed();
			}

			if (beforeInfo.Bpm != afterInfo.Bpm)
			{
				currentTempo = afterInfo.Bpm;

				ShowTempo();
			}

			if (beforeInfo.Pos != afterInfo.Pos)
			{
				playingPosition = afterInfo.Pos;
				playingPattern = afterInfo.Pattern;
				playingTracks = afterInfo.Playing_Tracks;

				MarkPositionAsVisited(playingPosition);

				ShowSongPosition();
				ShowPatternOrTracks();
			}

			if (beforeInfo.Loop_Count != afterInfo.Loop_Count)
				endReached = true;

			if (endReached)
				OnEndReached(afterInfo.Pos);
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag SpeakerFlags
		{
			get
			{
				SpeakerFlag flags = SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;

				if (useSurround && ((Surround)libXmp.Xmp_Get_Player(Xmp_Player.Surround) == Surround.RealChannels))
					flags |= SpeakerFlag.BackLeft | SpeakerFlag.BackRight;

				return flags;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => moduleInfo.Mod.Chn;



		/********************************************************************/
		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<InstrumentInfo> Instruments
		{
			get
			{
				if (hasInstruments)
				{
					for (int i = 0; i < moduleInfo.Mod.Ins; i++)
					{
						Xmp_Instrument inst = moduleInfo.Mod.Xxi[i];

						InstrumentInfo instInfo = new InstrumentInfo
						{
							Name = inst.Name,
							Flags = InstrumentInfo.InstrumentFlags.None
						};

						// Fill out the note samples
						if (inst.Nsm > 0)
						{
							for (int j = 0; j < InstrumentInfo.Octaves; j++)
							{
								for (int k = 0; k < InstrumentInfo.NotesPerOctave; k++)
								{
									byte ins = inst.Map[j * InstrumentInfo.NotesPerOctave + k].Ins;
									instInfo.Notes[j, k] = (ins != 0xff) && (ins < inst.Nsm) ? inst.Sub[ins].Sid : -1;
								}
							}
						}

						yield return instInfo;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public override IEnumerable<SampleInfo> Samples
		{
			get
			{
				for (int i = 0; i < moduleInfo.Mod.Smp; i++)
				{
					Xmp_Sample sample = moduleInfo.Mod.Xxs[i];

					Xmp_Instrument inst = null;
					Xmp_SubInstrument sub = null;
					string name = string.Empty;

					if (hasInstruments)
					{
						for (int j = 0; j < moduleInfo.Mod.Ins; j++)
						{
							if (moduleInfo.Mod.Xxi[j].Nsm > 0)
							{
								sub = moduleInfo.Mod.Xxi[j].Sub.FirstOrDefault(x => x.Sid == i);
								if (sub != null)
								{
									inst = moduleInfo.Mod.Xxi[j];
									name = sample.Name;
									break;
								}
							}
						}
					}
					else
					{
						inst = moduleInfo.Mod.Xxi[i];
						name = inst.Name;

						if (inst.Nsm > 0)
							sub = inst.Sub[0];
					}

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = name,
						Flags = SampleInfo.SampleFlag.None,
						Type = SampleInfo.SampleType.Sample,
						Volume = 256,
						Panning = -1,
						Sample = sample.Data.Buffer,
						SampleOffset = (uint)sample.Data.Offset,
						Length = (uint)sample.Len,
						LoopStart = (uint)sample.Lps,
						LoopLength = (uint)(sample.Lpe - sample.Lps)
					};

					if ((sample.Flg & Xmp_Sample_Flag.Synth) != 0)
						sampleInfo.Type = SampleInfo.SampleType.Synthesis;
					else if ((sample.Flg & Xmp_Sample_Flag.Adlib) != 0)
						sampleInfo.Type = SampleInfo.SampleType.Adlib;

					if ((sample.Flg & Xmp_Sample_Flag._16Bit) != 0)
						sampleInfo.Flags |= SampleInfo.SampleFlag._16Bit;

					if ((sample.Flg & Xmp_Sample_Flag.Stereo) != 0)
						sampleInfo.Flags |= SampleInfo.SampleFlag.Stereo;

					// Add extra loop flags if any
					if ((sample.Flg & Xmp_Sample_Flag.Loop) != 0)
					{
						// Set loop flag
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

						// Is the loop ping-pong?
						if ((sample.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlag.PingPong;
					}
					else
					{
						sampleInfo.LoopStart = 0;
						sampleInfo.LoopLength = 0;
					}

					if (inst != null)
					{
						sampleInfo.Volume = (ushort)(inst.Vol * (256.0f / moduleInfo.Vol_Base));

						if (sub != null)
							sampleInfo.Panning = (short)sub.Pan;
					}

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];
					int c5Spd = (int)moduleInfo.C5Speeds[i];
					int xpo = sub != null ? sub.Xpo : 0;
					int fin = sub != null ? sub.Fin : 0;

					for (int j = 0; j < 10 * 12; j++)
						frequencies[j] = (uint)(428 * c5Spd / Note_To_Period(j + xpo, fin));

					sampleInfo.NoteFrequencies = frequencies;

					yield return sampleInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Holds the channels used by visuals. Only needed for players using
		/// buffer mode if possible
		/// </summary>
		/********************************************************************/
		public override ChannelChanged[] VisualChannels => libXmp.Xmp_Get_Visualizer_Channels();
		#endregion

		#region ModulePlayerWithPositionDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDuration(int songNumber, int startPosition)
		{
			libXmp.Xmp_Get_Module_Info(out Xmp_Module_Info info);

			if (songNumber >= info.Num_Sequences)
				return -1;

			if (info.Seq_Data[songNumber].Duration == 0)
				return -1;

			startPosition = info.Seq_Data[songNumber].Entry_Point;

			InitializeSound(startPosition, Xmp_Interp.None);
			MarkPositionAsVisited(startPosition);

			return startPosition;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup needed stuff after a sub-song calculation
		/// </summary>
		/********************************************************************/
		protected override void CleanupDuration()
		{
			libXmp.Xmp_End_Player();
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return moduleInfo.Mod.Len;
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return libXmp.Xmp_Create_Snapshot();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			libXmp.Xmp_Set_Snapshot(snapshot);

			SetModuleInformation();
			UpdateModuleInformation();

			if (lastMixerInfo != null)
				ChangeMixerConfiguration(lastMixerInfo);

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition, Xmp_Interp interp)
		{
			libXmp.Xmp_Start_Player(44100, 0);	// Real values will be set in SetOutputFormat()

			libXmp.Xmp_Set_Player(Xmp_Player.Interp, (int)interp);
			libXmp.Xmp_Set_Position(startPosition);
		}



		/********************************************************************/
		/// <summary>
		/// Convert the playing buffer into two and play them
		/// </summary>
		/********************************************************************/
		private void PlayBuffer(Xmp_Frame_Info frameInfo)
		{
			int bufferSize = frameInfo.Buffer_Size / 2;
			Span<short> buffer = MemoryMarshal.Cast<sbyte, short>(frameInfo.Buffer.AsSpan());

			if (VirtualChannels.Length == 1)
			{
				if ((leftBuffer == null) || (leftBuffer.Length < bufferSize))
					leftBuffer = new short[bufferSize];

				for (int i = 0; i < bufferSize; i++)
					leftBuffer[i] = buffer[i];

				VirtualChannels[0].PlayBuffer(leftBuffer, 0, (uint)bufferSize, PlayBufferFlag._16Bit);
			}
			else
			{
				bufferSize /= 2;

				if ((leftBuffer == null) || (leftBuffer.Length < bufferSize))
				{
					leftBuffer = new short[bufferSize];
					rightBuffer = new short[bufferSize];
				}

				for (int i = 0, j = 0; i < bufferSize; i++)
				{
					leftBuffer[i] = buffer[j++];
					rightBuffer[i] = buffer[j++];
				}

				VirtualChannels[0].PlayBuffer(leftBuffer, 0, (uint)bufferSize, PlayBufferFlag._16Bit);
				VirtualChannels[1].PlayBuffer(rightBuffer, 0, (uint)bufferSize, PlayBufferFlag._16Bit);

				if (VirtualChannels.Length == 4)
				{
					buffer = MemoryMarshal.Cast<sbyte, short>(frameInfo.BufferRear.AsSpan());

					if ((leftRearBuffer == null) || (leftRearBuffer.Length < bufferSize))
					{
						leftRearBuffer = new short[bufferSize];
						rightRearBuffer = new short[bufferSize];
					}

					for (int i = 0, j = 0; i < bufferSize; i++)
					{
						leftRearBuffer[i] = buffer[j++];
						rightRearBuffer[i] = buffer[j++];
					}

					VirtualChannels[2].PlayBuffer(leftRearBuffer, 0, (uint)bufferSize, PlayBufferFlag._16Bit);
					VirtualChannels[3].PlayBuffer(rightRearBuffer, 0, (uint)bufferSize, PlayBufferFlag._16Bit);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get period from note
		/// </summary>
		/********************************************************************/
		private double Note_To_Period(int note, int fineTune)
		{
			double d = note + (double)fineTune / 128;

			return 13696.0 / Math.Pow(2, d / 12);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the module uses surround
		/// </summary>
		/********************************************************************/
		private bool DoesModuleUseSurround()
		{
			Xmp_Module mod = moduleInfo.Mod;

			// First check all channel properties
			foreach (Xmp_Channel channel in mod.Xxc)
			{
				if (channel.Flg.HasFlag(Xmp_Channel_Flag.Surround))
					return true;
			}

			// Now check all the patterns for surround effect
			foreach (Xmp_Track track in mod.Xxt)
			{
				for (int i = 0; i < track.Rows; i++)
				{
					Xmp_Event ev = track.Event[i];

					if (((ev.FxT == 0x8d) && (ev.FxP != 0)) || ((ev.F2T == 0x8d) && (ev.F2P != 0)))   // Surround effect
						return true;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Will set local variables to hold different information shown to
		/// the user
		/// </summary>
		/********************************************************************/
		private void SetModuleInformation()
		{
			libXmp.Xmp_Get_Frame_Info(out Xmp_Frame_Info frameInfo);

			playingPosition = frameInfo.Pos;
			playingPattern = frameInfo.Pattern;
			playingTracks = frameInfo.Playing_Tracks;
			currentSpeed = frameInfo.Speed;
			currentTempo = frameInfo.Bpm;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current song position
		/// </summary>
		/********************************************************************/
		private void ShowSongPosition()
		{
			OnModuleInfoChanged(InfoPositionLine, playingPosition.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with pattern number or track
		/// numbers
		/// </summary>
		/********************************************************************/
		private void ShowPatternOrTracks()
		{
			OnModuleInfoChanged(InfoPatternOrTracksLine, FormatPatternOrTracks());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with current speed
		/// </summary>
		/********************************************************************/
		private void ShowSpeed()
		{
			OnModuleInfoChanged(InfoSpeedLine, currentSpeed.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with tempo
		/// </summary>
		/********************************************************************/
		private void ShowTempo()
		{
			OnModuleInfoChanged(InfoTempoLine, currentTempo.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Will update the module information with all dynamic values
		/// </summary>
		/********************************************************************/
		private void UpdateModuleInformation()
		{
			ShowSongPosition();
			ShowPatternOrTracks();
			ShowSpeed();
			ShowTempo();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing pattern or tracks
		/// </summary>
		/********************************************************************/
		private string FormatPatternOrTracks()
		{
			if ((moduleInfo.Flags & Xmp_Module_Flags.Uses_Tracks) != 0)
				return FormatTracks();

			return playingPattern.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return a string containing the playing tracks
		/// </summary>
		/********************************************************************/
		private string FormatTracks()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < ModuleChannelCount; i++)
			{
				sb.Append(playingTracks[i]);
				sb.Append(", ");
			}

			sb.Remove(sb.Length - 2, 2);

			return sb.ToString();
		}
		#endregion
	}
}
