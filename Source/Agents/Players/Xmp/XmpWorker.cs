﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Agent.Player.Xmp
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class XmpWorker : ModulePlayerWithPositionDurationAgentBase
	{
		private readonly Guid currentFormatId;

		private LibXmp libXmp;
		private Xmp_Module_Info moduleInfo;

		private bool hasInstruments;

		private int playingPosition;
		private int playingPattern;
		private int currentSpeed;
		private int currentTempo;

		private const int InfoPositionLine = 5;
		private const int InfoPatternOrTracksLine = 6;
		private const int InfoSpeedLine = 7;
		private const int InfoTempoLine = 8;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public XmpWorker(Guid formatId)
		{
			currentFormatId = formatId;
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
		public override string ExtraFormatInfo => moduleInfo.Mod.Type;



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

				// Used patterns
				case 1:
				{
					description = Resources.IDS_XMP_INFODESCLINE1;
					value = moduleInfo.Mod.Pat.ToString();
					break;
				}

				// Used tracks
				case 2:
				{
					description = Resources.IDS_XMP_INFODESCLINE2;
					value = moduleInfo.Mod.Trk.ToString();
					break;
				}

				// Used instruments
				case 3:
				{
					description = Resources.IDS_XMP_INFODESCLINE3;
					value = moduleInfo.Mod.Ins.ToString();
					break;
				}

				// Used samples
				case 4:
				{
					description = Resources.IDS_XMP_INFODESCLINE4;
					value = moduleInfo.Mod.Smp.ToString();
					break;
				}

				// Playing position
				case 5:
				{
					description = Resources.IDS_XMP_INFODESCLINE5;
					value = playingPosition.ToString();
					break;
				}

				// Playing pattern or Playing tracks
				case 6:
				{
					description = Resources.IDS_XMP_INFODESCLINE6a;
					value = FormatPatternOrTracks();
					break;
				}

				// Current speed
				case 7:
				{
					description = Resources.IDS_XMP_INFODESCLINE7;
					value = currentSpeed.ToString();
					break;
				}

				// Current tempo (BPM)
				case 8:
				{
					description = Resources.IDS_XMP_INFODESCLINE8;
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
		public override ModulePlayerSupportFlag SupportFlags => base.SupportFlags | ModulePlayerSupportFlag.SetPosition;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			libXmp = LibXmp.Xmp_Create_Context();

			libXmp.Xmp_Set_Load_Format(currentFormatId);
			int retVal = libXmp.Xmp_Load_Module_From_File(fileInfo.ModuleStream);

			if (retVal != 0)
			{
				libXmp.Xmp_Free_Context();

				errorMessage = Resources.IDS_XMP_ERR_LOADING;
				return AgentResult.Error;
			}

			libXmp.Xmp_Get_Module_Info(out moduleInfo);

			hasInstruments = (moduleInfo.Mod.Ins != moduleInfo.Mod.Smp) || moduleInfo.Mod.Xxi.Any(x => x.Nsm > 1) || moduleInfo.Mod.Xxs.Any(x => !string.IsNullOrEmpty(x.Name));

			return AgentResult.Ok;
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

			InitializeSound(0);
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
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			bool endReached = false;

			libXmp.Xmp_Get_Frame_Info(out Xmp_Frame_Info beforeInfo);

			libXmp.Xmp_Play_Frame();

			libXmp.Xmp_Get_Frame_Info(out Xmp_Frame_Info afterInfo);

			AmigaFilter = afterInfo.Filter;

			if (beforeInfo.Speed != afterInfo.Speed)
			{
				currentSpeed = afterInfo.Speed;

				ShowSpeed();
			}

			if (beforeInfo.Bpm != afterInfo.Bpm)
			{
				currentTempo = afterInfo.Bpm;

				SetPlayingFrequency(currentTempo);
				ShowTempo();
			}

			if (beforeInfo.Pos != afterInfo.Pos)
			{
				playingPosition = afterInfo.Pos;
				playingPattern = afterInfo.Pattern;

				if (HasPositionBeenVisited(playingPosition))
					endReached = true;

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
		/// Return the number of channels the module want to reserve
		/// </summary>
		/********************************************************************/
		public override int VirtualChannelCount => moduleInfo.VirtualChannels;



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
						for (int j = 0; j < InstrumentInfo.Octaves; j++)
						{
							for (int k = 0; k < InstrumentInfo.NotesPerOctave; k++)
								instInfo.Notes[j, k] = inst.Sub[inst.Map[j * InstrumentInfo.NotesPerOctave + k].Ins].Sid;
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
				if (hasInstruments)
				{

				}
				else
				{
					for (int i = 0; i < moduleInfo.Mod.Smp; i++)
					{
						Xmp_Instrument inst = moduleInfo.Mod.Xxi[i];

						SampleInfo sampleInfo = new SampleInfo
						{
							Name = inst.Name,
							Flags = SampleInfo.SampleFlag.None,
							Type = SampleInfo.SampleType.Sample,
							BitSize = SampleInfo.SampleSize._8Bit,
							Volume = (ushort)(inst.Vol * 4),
							Panning = -1
						};

						if (inst.Nsm > 0)
						{
							Xmp_SubInstrument sub = inst.Sub[0];
							Xmp_Sample sample = moduleInfo.Mod.Xxs[i];

							sampleInfo.Volume = (ushort)(inst.Vol * 4);
							sampleInfo.Panning = (short)(sub.Pan + 0x80);
							sampleInfo.Sample = sample.Data;
							sampleInfo.SampleOffset = (uint)sample.DataOffset;
							sampleInfo.Length = (uint)sample.Len;
							sampleInfo.LoopStart = (uint)sample.Lps;
							sampleInfo.LoopLength = (uint)(sample.Lpe - sample.Lps);

							if ((sample.Flg & Xmp_Sample_Flag.Synth) != 0)
								sampleInfo.Type = SampleInfo.SampleType.Synthesis;

							if ((sample.Flg & Xmp_Sample_Flag._16Bit) != 0)
								sampleInfo.BitSize = SampleInfo.SampleSize._16Bit;

							// Add extra loop flags if any
							if ((sample.Flg & Xmp_Sample_Flag.Loop) != 0)
							{
								// Set loop flag
								sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

								// Is the loop ping-pong?
								if ((sample.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0)
									sampleInfo.Flags |= SampleInfo.SampleFlag.PingPong;
							}

							// Build frequency table
							uint[] frequencies = new uint[10 * 12];

							for (int j = 0; j < 10 * 12; j++)
								frequencies[j] = (uint)(3546895 / Note_To_Period(j + sub.Xpo, sub.Fin));

							sampleInfo.NoteFrequencies = frequencies;
						}

						yield return sampleInfo;
					}
				}
			}
		}
		#endregion

		#region ModulePlayerWithPositionDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override int InitDuration(int startPosition)
		{
			InitializeSound(startPosition);
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

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int startPosition)
		{
			libXmp.Xmp_Start_Player(44100, 0);	// Arguments are not used
			libXmp.Xmp_Set_Player(Xmp_Player.Mix, 100);		// Make 100% pan separation, since our own mixer handle the panning
			SetPlayingFrequency(moduleInfo.Mod.Bpm);

			libXmp.Xmp_Set_NostalgicPlayer_Channels(VirtualChannels);

			libXmp.Xmp_Set_Position(startPosition);
		}



		/********************************************************************/
		/// <summary>
		/// Set the playing speed
		/// </summary>
		/********************************************************************/
		private void SetPlayingFrequency(int bpm)
		{
			PlayingFrequency = (float)(bpm / (moduleInfo.Time_Factor * moduleInfo.RRate / 1000));
		}



		/********************************************************************/
		/// <summary>
		/// Get period from note
		/// </summary>
		/********************************************************************/
		private double Note_To_Period(int note, int fineTune)
		{
			double d = note + (double)fineTune / 128;
			double per;

			switch (moduleInfo.PeriodType)
			{
				// Linear:
				case 2:
				{
					per = (240.0 - d) * 16;		// Linear
					break;
				}

				// CSpd
				case 3:
				{
					per = 8363.0 * Math.Pow(2, note / 12.0) / 32 + fineTune;	// Hz
					break;
				}

				default:
				{
					per = 13696.0 / Math.Pow(2, d / 12);	// Amiga
					break;
				}
			}

			return per;
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
			return playingPattern.ToString();
		}
		#endregion
	}
}
