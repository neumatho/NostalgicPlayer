/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers;
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Implementation;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class DigiBoosterProWorker : ModulePlayerWithSubSongDurationAgentBase
	{
		private static readonly Dictionary<Guid, ModuleType> moduleTypeLookup = new Dictionary<Guid, ModuleType>
		{
			{ DigiBoosterPro.Agent1Id, ModuleType.DigiBoosterPro2x },
			{ DigiBoosterPro.Agent2Id, ModuleType.DigiBooster3 }
		};

		private readonly ModuleType currentModuleType;

		private DB3Module module;
		private Implementation.Player player;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DigiBoosterProWorker(Guid typeId)
		{
			if (!moduleTypeLookup.TryGetValue(typeId, out currentModuleType))
				currentModuleType = ModuleType.Unknown;
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "dbm" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			// Check the module
			ModuleType checkType = TestModule(fileInfo);
			if (checkType == currentModuleType)
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => module.Name;



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
				// Number of positions:
				case 0:
				{
					description = Resources.IDS_DBM_INFODESCLINE0;
					value = player.GetSongLength().ToString();
					break;
				}

				// Used patterns
				case 1:
				{
					description = Resources.IDS_DBM_INFODESCLINE1;
					value = module.NumberOfPatterns.ToString();
					break;
				}

				// Used instruments
				case 2:
				{
					description = Resources.IDS_DBM_INFODESCLINE2;
					value = module.NumberOfInstruments.ToString();
					break;
				}

				// Used samples
				case 3:
				{
					description = Resources.IDS_DBM_INFODESCLINE3;
					value = module.NumberOfSamples.ToString();
					break;
				}

				// Playing position
				case 4:
				{
					description = Resources.IDS_DBM_INFODESCLINE4;
					value = player.GetPosition().ToString();
					break;
				}

				// Playing pattern
				case 5:
				{
					description = Resources.IDS_DBM_INFODESCLINE5;
					value = player.GetPattern().ToString();
					break;
				}

				// Current speed
				case 6:
				{
					description = Resources.IDS_DBM_INFODESCLINE6;
					value = player.GetSpeed().ToString();
					break;
				}

				// Current tempo (BPM)
				case 7:
				{
					description = Resources.IDS_DBM_INFODESCLINE7;
					value = player.GetTempo().ToString();
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
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			try
			{
				ModuleStream moduleStream = fileInfo.ModuleStream;

				module = Loader.LoadFromStream(moduleStream, out errorMessage);
				if (module == null)
					return AgentResult.Error;
			}
			catch (Exception)
			{
				Cleanup();
				throw;
			}

			// Everything is loaded alright
			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(out string errorMessage)
		{
			if (!base.InitPlayer(out errorMessage))
				return false;

			player = Implementation.Player.NewEngine(module, this);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			Cleanup();

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

			InitializeSound(songNumber);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
			player.Mix();
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public override int ModuleChannelCount => module.NumberOfTracks;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public override SubSongInfo SubSongs => new SubSongInfo(module.NumberOfSongs, 0);



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
				for (int i = 0; i < module.NumberOfInstruments; i++)
				{
					DB3ModuleSampleInstrument inst = (DB3ModuleSampleInstrument)module.Instruments[i];
					DB3ModuleSample sample = module.Samples[inst.SampleNumber];

					// Build frequency table
					uint[] frequencies = new uint[10 * 12];

					if (module.CreatorVersion == Constants.Creator_DigiBooster_2)
					{
						for (int j = 0; j < 8 * 12; j++)
						{
							int32_t pitch = Tables.Periods[j];
							int32_t frequency = 3579545 / pitch;
							pitch = (int32_t)(3579545 / (((inst.C3Frequency * 256) / 8363 * frequency) / 256));
							frequencies[j] = (uint)((3546895 / pitch) * 4);
						}
					}
					else
					{
						for (int j = 0; j < 7; j++)
						{
							long startFrequency = inst.C3Frequency;

							if (j < 3)
							{
								for (int k = j; k < 3; k++)
									startFrequency /= 2;
							}
							else if (j > 3)
							{
								for (int k = j; k > 3; k--)
									startFrequency *= 2;
							}

							for (int k = 0; k < 12; k++)
								frequencies[2 * 12 + j * 12 + k] = (uint)((startFrequency * Tables.MusicScale[k * 8]) / 65536);
						}
					}

					SampleInfo.SampleFlag flag = SampleInfo.SampleFlag.None;

					if (sample.Data16 != null)
						flag |= SampleInfo.SampleFlag._16Bit;

					SampleInfo sampleInfo = new SampleInfo
					{
						Name = inst.Name,
						Flags = flag,
						Type = SampleInfo.SampleType.Sample,
						Volume = (ushort)(inst.Volume * 4),
						Panning = (short)(inst.Panning + 128),
						Sample = (Array)sample.Data8 ?? sample.Data16,
						Length = (uint)sample.Frames,
						LoopStart = (uint)inst.LoopStart,
						LoopLength = (uint)inst.LoopLength,
						NoteFrequencies = frequencies
					};

					if ((inst.Flags & InstrumentFlag.Loop_Mask) != 0)
					{
						sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;

						if ((inst.Flags & InstrumentFlag.PingPong_Loop) != 0)
							sampleInfo.Flags |= SampleInfo.SampleFlag.PingPong;
					}

					yield return sampleInfo;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return an effect master instance if the player adds extra mixer
		/// effects to the output
		/// </summary>
		/********************************************************************/
		public override IEffectMaster EffectMaster => player.GetEffectMaster();
		#endregion

		#region ModulePlayerWithSubSongDurationAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation on a new sub-song
		/// </summary>
		/********************************************************************/
		protected override void InitDuration(int subSong)
		{
			InitializeSound(subSong);
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of positions. You only need to derive
		/// from this method, if your player have one position list for all
		/// channels and can restart on another position than 0
		/// </summary>
		/********************************************************************/
		protected override int GetTotalNumberOfPositions()
		{
			return player.GetSongLength();
		}



		/********************************************************************/
		/// <summary>
		/// Create a snapshot of all the internal structures and return it
		/// </summary>
		/********************************************************************/
		protected override ISnapshot CreateSnapshot()
		{
			return player.CreateSnapshot();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize internal structures based on the snapshot given
		/// </summary>
		/********************************************************************/
		protected override bool SetSnapshot(ISnapshot snapshot, out string errorMessage)
		{
			errorMessage = string.Empty;

			player.SetSnapshot(snapshot);

			player.UpdateModuleInformation();

			return true;
		}
		#endregion

		#region Helper methods from the player
		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer about new tempo
		/// </summary>
		/********************************************************************/
		internal void SetTempo(ushort bpm)
		{
			SetBpmTempo(bpm);
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer that the position has changed
		/// </summary>
		/********************************************************************/
		internal void PositionHasChanged(int position)
		{
			MarkPositionAsVisited(position);
		}



		/********************************************************************/
		/// <summary>
		/// Will restart the current sub-song from the beginning
		/// </summary>
		/********************************************************************/
		internal void Restart()
		{
			RestartSong();
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer that the module has ended
		/// </summary>
		/********************************************************************/
		internal void ModuleEnded()
		{
			OnEndReachedOnAllChannels(player.GetPosition());
		}



		/********************************************************************/
		/// <summary>
		/// Tell NostalgicPlayer that some module information has been
		/// updated
		/// </summary>
		/********************************************************************/
		internal void UpdateModuleInfo(int line, string newValue)
		{
			OnModuleInfoChanged(line, newValue);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		private ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			if (moduleStream.Length < 58)
				return ModuleType.Unknown;

			// Now check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if (mark != 0x44424d30)		// DBM0
				return ModuleType.Unknown;

			// Check the version
			byte version = moduleStream.Read_UINT8();

			if (version == 2)
				return ModuleType.DigiBoosterPro2x;

			if (version == 3)
				return ModuleType.DigiBooster3;

			return ModuleType.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sound structures
		/// </summary>
		/********************************************************************/
		private void InitializeSound(int songNumber)
		{
			player.Initialize();
			player.SetSong(songNumber);
			player.SetPosition(0, 0);
		}



		/********************************************************************/
		/// <summary>
		/// Frees all the memory the player have allocated
		/// </summary>
		/********************************************************************/
		private void Cleanup()
		{
			if (player != null)
			{
				player.DisposeEngine();
				player = null;
			}

			if (module != null)
			{
				Loader.Unload(module);
				module = null;
			}
		}
		#endregion
	}
}
