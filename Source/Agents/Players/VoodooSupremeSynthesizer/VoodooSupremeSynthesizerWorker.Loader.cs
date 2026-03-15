/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer
{
	internal partial class VoodooSupremeSynthesizerWorker
	{
		private int footerOffset;

		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		private AgentResult CheckModule(ModuleStream moduleStream)
		{
			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 64)
				return AgentResult.Unknown;

			// Try to find the mark (VSS0)
			byte[] buffer = new byte[64];

			long offset = (moduleStream.Length - 64 + 1) & ~1;
			moduleStream.Seek(offset, SeekOrigin.Begin);

			if (moduleStream.Read(buffer, 0, 64) != 64)
				return AgentResult.Unknown;

			for (int i = 0; i < 64 - 8; i += 2)
			{
				if ((buffer[i] == 'V') && (buffer[i + 1] == 'S') && (buffer[i + 2] == 'S') && (buffer[i + 3] == '0'))
				{
					footerOffset = (int)moduleStream.Length - 64 + i;

					uint songs = ((uint)buffer[i + 4] << 24) | ((uint)buffer[i + 5] << 16) | ((uint)buffer[i + 6] << 8) | buffer[i + 7];

					if (songs < 0x100)
						return AgentResult.Ok;

					return AgentResult.Unknown;
				}
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Load the module into memory
		/// </summary>
		/********************************************************************/
		private bool LoadModule(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			int[] songOffsets = LoadSongOffsets(moduleStream);
			if (songOffsets == null)
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_HEADER;
				return false;
			}

			subSongs = new SongInfo[songOffsets.Length];

			for (int i = 0; i < songOffsets.Length; i++)
			{
				subSongs[i] = LoadSubSong(moduleStream, songOffsets[i], out errorMessage);
				if (subSongs[i] == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sub-song offsets
		/// </summary>
		/********************************************************************/
		private int[] LoadSongOffsets(ModuleStream moduleStream)
		{
			moduleStream.Seek(footerOffset + 4, SeekOrigin.Begin);

			int numberOfSongs = moduleStream.Read_B_INT32();
			int[] songOffsets = new int[numberOfSongs];

			for (int i = 0; i < numberOfSongs; i++)
				songOffsets[i] = footerOffset + moduleStream.Read_B_INT32();

			if (moduleStream.EndOfStream)
				return null;

			return songOffsets;
		}



		/********************************************************************/
		/// <summary>
		/// Load all information needed for a sub-song
		/// </summary>
		/********************************************************************/
		private SongInfo LoadSubSong(ModuleStream moduleStream, int songOffset, out string errorMessage)
		{
			errorMessage = string.Empty;

			OffsetInfo[] offsetTable = LoadAllOffsets(moduleStream, songOffset);
			if (offsetTable == null)
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_HEADER;
				return null;
			}

			if (!LoadTracks(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_TRACK;
				return null;
			}

			if (!LoadVolumeEnvelopes(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_ENVELOPE;
				return null;
			}

			if (!LoadPeriodTables(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_PERIODTABLE;
				return null;
			}

			if (!LoadWaveformTables(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_WAVEFORMTABLE;
				return null;
			}

			if (!LoadWaveforms(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_WAVEFORM;
				return null;
			}

			if (!LoadSamples(moduleStream, offsetTable))
			{
				errorMessage = Resources.IDS_VSS_ERR_LOADING_SAMPLE;
				return null;
			}

			return new SongInfo
			{
				Data = offsetTable.Select(x => x.Data).ToArray(),
				Tracks = offsetTable.Take(4).Select(x => x.Data).Cast<TrackData>().ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all the offsets
		/// </summary>
		/********************************************************************/
		private OffsetInfo[] LoadAllOffsets(ModuleStream moduleStream, int songOffset)
		{
			moduleStream.Seek(songOffset, SeekOrigin.Begin);

			int minOffset = footerOffset;
			List<OffsetInfo> offsets = new List<OffsetInfo>();

			for (;;)
			{
				int newOffset = moduleStream.Read_B_INT32();

				if (moduleStream.EndOfStream)
					return null;

				if (newOffset >= 0)
					minOffset = Math.Min(minOffset, songOffset + newOffset);

				offsets.Add(new OffsetInfo
				{
					Offset = songOffset + newOffset,
					Type = OffsetType.Unknown,
					Data = null
				});

				if (moduleStream.Position == minOffset)
					break;
			}

			// We know the first 4 offsets are track data
			offsets[0].Type = OffsetType.Track;
			offsets[1].Type = OffsetType.Track;
			offsets[2].Type = OffsetType.Track;
			offsets[3].Type = OffsetType.Track;

			return offsets.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			// To start with, the first 4 offsets are already marked as tracks.
			// Other offsets may be marked as tracks while parsing a single track
			foreach (OffsetInfo offsetInfo in offsetTable)
			{
				if (offsetInfo.Type == OffsetType.Track)
				{
					offsetInfo.Data = LoadSingleTrack(moduleStream, offsetInfo.Offset, offsetTable);
					if (offsetInfo.Data == null)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private TrackData LoadSingleTrack(ModuleStream moduleStream, int trackOffset, OffsetInfo[] offsetTable)
		{
			moduleStream.Seek(trackOffset, SeekOrigin.Begin);

			List<byte> trackData = new List<byte>();

			bool sampleMode = false;
			int sampleNumber1 = -1;
			int sampleNumber2 = -1;
			bool done = false;

			do
			{
				byte cmd = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackData.Add(cmd);

				if (cmd < 0x80)
				{
					if (sampleNumber1 != -1)
					{
						offsetTable[sampleNumber1].Type = sampleMode ? OffsetType.Sample : OffsetType.Waveform;
						sampleNumber1 = -1;
					}

					if (sampleNumber2 != -1)
					{
						offsetTable[sampleNumber2].Type = sampleMode ? OffsetType.Sample : OffsetType.Waveform;
						sampleNumber2 = -1;
					}

					trackData.Add(moduleStream.Read_UINT8());
					continue;
				}

				switch ((Effect)cmd)
				{
					case Effect.Gosub:
					{
						byte arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						offsetTable[arg].Type = OffsetType.Track;
						break;
					}

					case Effect.Return:
					{
						done = true;
						break;
					}

					case Effect.DoLoop:
						break;

					case Effect.StartLoop:
					{
						trackData.Add(moduleStream.Read_UINT8());
						trackData.Add(moduleStream.Read_UINT8());
						break;
					}

					case Effect.SetSample:
					{
						byte arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						sampleNumber1 = arg;

						arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						sampleNumber2 = arg;
						break;
					}

					case Effect.SetVolumeEnvelope:
					{
						byte arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						offsetTable[arg].Type = OffsetType.VolumeEnvelope;
						break;
					}

					case Effect.SetPeriodTable:
					{
						byte arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						offsetTable[arg].Type = OffsetType.PeriodTable;
						break;
					}

					case Effect.SetWaveformTable:
					{
						byte arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						offsetTable[arg].Type = OffsetType.WaveformTable;

						arg = moduleStream.Read_UINT8();
						trackData.Add(arg);

						sampleMode = (arg & 0x20) != 0;
						break;
					}

					case Effect.Portamento:
					{
						trackData.Add(moduleStream.Read_UINT8());
						trackData.Add(moduleStream.Read_UINT8());
						trackData.Add(moduleStream.Read_UINT8());
						break;
					}

					case Effect.SetTranspose:
					case Effect.SetResetFlags:
					case Effect.SetWaveformMask:
					case Effect.NoteCut:
					{
						trackData.Add(moduleStream.Read_UINT8());
						break;
					}

					case Effect.Goto:
					{
						byte arg = moduleStream.Read_UINT8();
						int newTrackOffset = offsetTable[arg].Offset;

						// Convert the offset number to a position
						ushort pos = 0;

						if ((newTrackOffset >= trackOffset) && (newTrackOffset < (trackOffset + trackData.Count)))
							pos = (ushort)(newTrackOffset - trackOffset);

						trackData.Add((byte)(pos >> 8));
						trackData.Add((byte)(pos & 0xff));

						done = true;
						break;
					}

					default:
						return null;
				}
			}
			while (!done);

			return new TrackData
			{
				Track = trackData.ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all volume envelopes
		/// </summary>
		/********************************************************************/
		private bool LoadVolumeEnvelopes(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			foreach (OffsetInfo offsetInfo in offsetTable.Where(x => x.Type == OffsetType.VolumeEnvelope))
			{
				offsetInfo.Data = LoadSingleVolumeEnvelope(moduleStream, offsetInfo.Offset);
				if (offsetInfo.Data == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single volume envelope
		/// </summary>
		/********************************************************************/
		private Table LoadSingleVolumeEnvelope(ModuleStream moduleStream, int envelopeOffset)
		{
			moduleStream.Seek(envelopeOffset, SeekOrigin.Begin);

			List<byte> data = new List<byte>();
			bool done = false;

			do
			{
				byte byt = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				data.Add(byt);
				data.Add(moduleStream.Read_UINT8());

				if (byt == 0x88)
					done = true;
			}
			while (!done);

			return new Table
			{
				Data = data.ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all period command tables
		/// </summary>
		/********************************************************************/
		private bool LoadPeriodTables(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			foreach (OffsetInfo offsetInfo in offsetTable.Where(x => x.Type == OffsetType.PeriodTable))
			{
				offsetInfo.Data = LoadSinglePeriodTable(moduleStream, offsetInfo.Offset);
				if (offsetInfo.Data == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single period table
		/// </summary>
		/********************************************************************/
		private Table LoadSinglePeriodTable(ModuleStream moduleStream, int tableOffset)
		{
			moduleStream.Seek(tableOffset, SeekOrigin.Begin);

			List<byte> data = new List<byte>();
			bool done = false;

			do
			{
				byte byt = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				data.Add(byt);
				data.Add(moduleStream.Read_UINT8());

				if (byt == 0xff)
					done = true;
			}
			while (!done);

			return new Table
			{
				Data = data.ToArray()
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all waveform tables
		/// </summary>
		/********************************************************************/
		private bool LoadWaveformTables(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			foreach (OffsetInfo offsetInfo in offsetTable.Where(x => x.Type == OffsetType.WaveformTable))
			{
				offsetInfo.Data = LoadSingleWaveformTable(moduleStream, offsetInfo.Offset);
				if (offsetInfo.Data == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single waveform table
		/// </summary>
		/********************************************************************/
		private Table LoadSingleWaveformTable(ModuleStream moduleStream, int tableOffset)
		{
			moduleStream.Seek(tableOffset, SeekOrigin.Begin);

			Table result = new Table
			{
				Data = new byte[28]
			};

			if (moduleStream.Read(result.Data, 0, result.Data.Length) != result.Data.Length)
				return null;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Load all waveforms
		/// </summary>
		/********************************************************************/
		private bool LoadWaveforms(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			foreach (OffsetInfo offsetInfo in offsetTable.Where(x => x.Type == OffsetType.Waveform))
			{
				offsetInfo.Data = LoadSingleWaveform(moduleStream, offsetInfo.Offset);
				if (offsetInfo.Data == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single waveform
		/// </summary>
		/********************************************************************/
		private Waveform LoadSingleWaveform(ModuleStream moduleStream, int waveformOffset)
		{
			moduleStream.Seek(waveformOffset, SeekOrigin.Begin);

			Waveform result = new Waveform();

			result.Data = moduleStream.ReadSampleData(waveformOffset, 32, out _);
			result.Offset = waveformOffset;

			if (moduleStream.EndOfStream)
				return null;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Load all samples
		/// </summary>
		/********************************************************************/
		private bool LoadSamples(ModuleStream moduleStream, OffsetInfo[] offsetTable)
		{
			foreach (OffsetInfo offsetInfo in offsetTable.Where(x => x.Type == OffsetType.Sample))
			{
				offsetInfo.Data = LoadSingleSample(moduleStream, offsetInfo.Offset);
				if (offsetInfo.Data == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample
		/// </summary>
		/********************************************************************/
		private Sample LoadSingleSample(ModuleStream moduleStream, int sampleOffset)
		{
			moduleStream.Seek(sampleOffset - 2, SeekOrigin.Begin);

			Sample result = new Sample();

			result.Length = moduleStream.Read_B_UINT16();
			result.Data = moduleStream.ReadSampleData(sampleOffset, result.Length, out _);
			result.Offset = sampleOffset;

			if (moduleStream.EndOfStream)
				return null;

			return result;
		}
	}
}
