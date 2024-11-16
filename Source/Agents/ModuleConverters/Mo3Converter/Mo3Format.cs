/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Chunks;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers.Plugins;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Streams;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter
{
	/// <summary>
	/// Can convert MO3 to its origin format
	/// </summary>
	internal class Mo3Format : ModuleConverterAgentBase
	{
		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 8)
				return AgentResult.Unknown;

			// Check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint mark = moduleStream.Read_B_UINT32();
			byte version = (byte)(mark & 0xff);
			mark &= 0xffffff00;

			if (mark != 0x4d4f3300)		// MO3
				return AgentResult.Unknown;

			if (version > 5)
				return AgentResult.Unknown;

			// Due to the LZ algorithm's unbounded back window, we could reach gigantic
			// sizes with just a few dozen bytes. 512 MB of music data (not samples) is
			// chosen as a safeguard that is probably (hopefully) *way* beyond anything
			// a real-world module will ever reach
			uint musicSize = moduleStream.Read_L_UINT32();

			if ((musicSize <= 0x1a6) || (musicSize >= 0x2000_0000))	// 0x1a6 -> size of header
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ModuleStream moduleStream = fileInfo.ModuleStream;

			Mo3Module module = new Mo3Module();

			// Read the version
			moduleStream.Seek(3, SeekOrigin.Begin);
			module.Version = moduleStream.Read_UINT8();
			module.MusicSize = moduleStream.Read_L_UINT32();

			uint compressedSize = uint.MaxValue;

			// Generous estimate based on biggest pre-v5 MO3s found in the wild (~350K music data)
			uint reserveSize = 1024 * 1024;

			if (module.Version >= 5)
			{
				// Size of compressed music chunk
				compressedSize = moduleStream.Read_L_UINT32();

				// Generous estimate based on highest real-world compression ratio I found in a module (~20:1)
				reserveSize = Math.Min(uint.MaxValue / 32U, compressedSize) * 32;
			}

			using (Mo3Stream mo3Stream = new Mo3Stream(moduleStream, module.MusicSize, reserveSize))
			{
				module.Header = LoadHeader(mo3Stream);
				if (module.Header == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_HEADER;
					return AgentResult.Error;
				}

				module.ModuleType = FindModuleType(module.Header);

				module.PatternInfo = LoadPatternInfo(mo3Stream, module.Header);
				if (module.PatternInfo == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_PATTERN_INFO;
					return AgentResult.Error;
				}

				module.Tracks = LoadTracks(mo3Stream, module.Header);
				if (module.Tracks == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_TRACKS;
					return AgentResult.Error;
				}

				module.Instruments = LoadInstruments(mo3Stream, module.Header, module.Version);
				if (module.Instruments == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_INSTRUMENTS;
					return AgentResult.Error;
				}

				module.Samples = LoadSampleInfo(mo3Stream, module.Header, module.Version);
				if (module.Samples == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_SAMPLEINFO;
					return AgentResult.Error;
				}

				module.Plugins = LoadPlugins(mo3Stream, module.Header);
				if (module.Plugins == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_PLUGINS;
					return AgentResult.Error;
				}

				module.Chunks = LoadChunks(mo3Stream, module);
				if (module.Chunks == null)
				{
					errorMessage = Resources.IDS_ERR_LOADING_CHUNKS;
					return AgentResult.Error;
				}

				if (module.Version < 5)
				{
					// As we don't know where the compressed data ends, we don't know where
					// the sample data starts, either
					if (!mo3Stream.UnpackedSuccessfully)
					{
						errorMessage = Resources.IDS_ERR_UNPACKING;
						return AgentResult.Error;
					}
				}
				else
					moduleStream.Seek(12 + compressedSize, SeekOrigin.Begin);
			}

			IFormatSaver saver;

			switch (module.ModuleType)
			{
				case ModuleType.Mod:
				{
					saver = new ModSaver();
					break;
				}

				case ModuleType.Mtm:
				{
					saver = new MtmSaver();
					break;
				}

				case ModuleType.S3M:
				{
					saver = new S3MSaver();
					break;
				}

				case ModuleType.Xm:
				{
					saver = new XmSaver();
					break;
				}

				case ModuleType.It:
				{
					saver = new ItSaver();
					break;
				}

				default:
					throw new NotImplementedException($"Module type {module.ModuleType} not supported");
			}

			if (!saver.SaveModule(module, moduleStream, converterStream, out errorMessage))
				return AgentResult.Error;

			return AgentResult.Ok;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load the header into memory
		/// </summary>
		/********************************************************************/
		private FileHeader LoadHeader(Mo3Stream mo3Stream)
		{
			FileHeader header = new FileHeader();

			header.SongName = ReadNullTerminatedString(mo3Stream);
			header.SongMessage = ReadNullTerminatedString(mo3Stream);
			header.NumChannels = mo3Stream.Read_UINT8();
			header.NumOrders = mo3Stream.Read_L_UINT16();
			header.RestartPos = mo3Stream.Read_L_UINT16();
			header.NumPatterns = mo3Stream.Read_L_UINT16();
			header.NumTracks = mo3Stream.Read_L_UINT16();
			header.NumInstruments = mo3Stream.Read_L_UINT16();
			header.NumSamples = mo3Stream.Read_L_UINT16();
			header.DefaultSpeed = mo3Stream.Read_UINT8();
			header.DefaultTempo = mo3Stream.Read_UINT8();
			header.Flags = (HeaderFlag)mo3Stream.Read_L_UINT32();
			header.GlobalVol = mo3Stream.Read_UINT8();
			header.PanSeparation = mo3Stream.Read_UINT8();
			header.SampleVolume = mo3Stream.Read_INT8();

			mo3Stream.ReadInto(header.ChnVolume, 0, 64);
			mo3Stream.ReadInto(header.ChnPan, 0, 64);
			mo3Stream.ReadInto(header.SfxMacros, 0, 16);

			mo3Stream.ReadInto(header.FixedMacros[0], 0, 128);
			mo3Stream.ReadInto(header.FixedMacros[1], 0, 128);

			return mo3Stream.EndOfStream || (header.NumChannels == 0) || (header.NumChannels > 64) || (header.RestartPos > header.NumOrders) ||
				(header.NumInstruments >= 240) || (header.NumSamples >= 240) ? null : header;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the pattern information
		/// </summary>
		/********************************************************************/
		private PatternInfo LoadPatternInfo(Mo3Stream mo3Stream, FileHeader header)
		{
			PatternInfo patternInfo = new PatternInfo();

			patternInfo.PositionList = new byte[header.NumOrders];

			int bytesRead = mo3Stream.Read(patternInfo.PositionList, 0, patternInfo.PositionList.Length);
			if (bytesRead < patternInfo.PositionList.Length)
				return null;

			patternInfo.Sequences = new ushort[header.NumPatterns, header.NumChannels];

			for (int i = 0; i < header.NumPatterns; i++)
			{
				for (int j = 0; j < header.NumChannels; j++)
					patternInfo.Sequences[i, j] = mo3Stream.Read_L_UINT16();
			}

			if (mo3Stream.EndOfStream)
				return null;

			patternInfo.RowLengths = new ushort[header.NumPatterns];

			mo3Stream.ReadArray_L_UINT16s(patternInfo.RowLengths, 0, patternInfo.RowLengths.Length);

			return mo3Stream.EndOfStream ? null : patternInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private Track[] LoadTracks(Mo3Stream mo3Stream, FileHeader header)
		{
			Track[] tracks = new Track[header.NumTracks];

			for (int i = 0; i < header.NumTracks; i++)
			{
				tracks[i] = LoadSingleTrack(mo3Stream);
				if (tracks[i] == null)
					return null;
			}

			return tracks;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private Track LoadSingleTrack(Mo3Stream mo3Stream)
		{
			uint len = mo3Stream.Read_L_UINT32();

			// A pattern can be at most 65535 rows long, one row can contain at most 15 events (with the status bytes, that 31 bytes per row).
			// Leaving some margin for error, that gives us an upper limit of 2MB per track
			if (len >= 0x20_0000)
				return null;

			List<TrackRow> rows = new List<TrackRow>();

			while (len != 0)
			{
				byte b = mo3Stream.Read_UINT8();
				if (b == 0)
					break;

				int numCommands = b & 0x0f;
				int rep = b >> 4;

				TrackRow trackRow = new TrackRow();

				for (int i = 0; i < numCommands; i++)
				{
					Effect eff = (Effect)mo3Stream.Read_UINT8();
					byte effVal = mo3Stream.Read_UINT8();

					switch (eff)
					{
						case Effect.Note:
						{
							trackRow.Note = effVal;

							if (effVal < 120)
								trackRow.Note++;

							break;
						}

						case Effect.Instrument:
						{
							trackRow.Instrument = (byte)(effVal + 1);
							break;
						}

						default:
						{
							if (trackRow.Effects == null)
								trackRow.Effects = new List<(Effect, byte)>();

							trackRow.Effects.Add((eff, effVal));
							break;
						}
					}
				}

				for (int i = 0; i < rep; i++)
					rows.Add(trackRow);

				if (mo3Stream.EndOfStream)
					return null;

				len -= (uint)(1 + numCommands * 2);
			}

			return new Track
			{
				Rows = rows
			};
		}



		/********************************************************************/
		/// <summary>
		/// Load all the instruments
		/// </summary>
		/********************************************************************/
		private Instrument[] LoadInstruments(Mo3Stream mo3Stream, FileHeader header, byte version)
		{
			void ReadEnvelope(Envelope env)
			{
				env.Flags = (EnvelopeFlag)mo3Stream.Read_UINT8();
				env.NumNodes = mo3Stream.Read_UINT8();
				env.SustainStart = mo3Stream.Read_UINT8();
				env.SustainEnd = mo3Stream.Read_UINT8();
				env.LoopStart = mo3Stream.Read_UINT8();
				env.LoopEnd = mo3Stream.Read_UINT8();

				mo3Stream.ReadArray_L_INT16s(env.Points, 0, env.Points.Length);
			}

			Instrument[] instruments = new Instrument[header.NumInstruments];

			for (int i = 0; i < header.NumInstruments; i++)
			{
				Instrument instr = new Instrument();

				instr.InstrumentName = ReadNullTerminatedString(mo3Stream);
				instr.FileName = version >= 5 ? ReadNullTerminatedString(mo3Stream) : [];

				instr.Flags = (InstrumentFlag)mo3Stream.Read_L_UINT32();

				mo3Stream.ReadArray_L_UINT16s(instr.SampleMap, 0, instr.SampleMap.Length);

				ReadEnvelope(instr.VolEnv);
				ReadEnvelope(instr.PanEnv);
				ReadEnvelope(instr.PitchEnv);

				instr.Vibrato.Type = mo3Stream.Read_UINT8();
				instr.Vibrato.Sweep = mo3Stream.Read_UINT8();
				instr.Vibrato.Depth = mo3Stream.Read_UINT8();
				instr.Vibrato.Rate = mo3Stream.Read_UINT8();

				instr.FadeOut = mo3Stream.Read_L_UINT16();
				instr.MidiChannel = mo3Stream.Read_UINT8();
				instr.MidiBank = mo3Stream.Read_UINT8();
				instr.MidiPatch = mo3Stream.Read_UINT8();
				instr.MidiBend = mo3Stream.Read_UINT8();
				instr.GlobalVol = mo3Stream.Read_UINT8();
				instr.Panning = mo3Stream.Read_L_UINT16();
				instr.Nna = mo3Stream.Read_UINT8();
				instr.Pps = mo3Stream.Read_UINT8();
				instr.Ppc = mo3Stream.Read_UINT8();
				instr.Dct = mo3Stream.Read_UINT8();
				instr.Dca = mo3Stream.Read_UINT8();
				instr.VolSwing = mo3Stream.Read_L_UINT16();
				instr.PanSwing = mo3Stream.Read_L_UINT16();
				instr.CutOff = mo3Stream.Read_UINT8();
				instr.Resonance = mo3Stream.Read_UINT8();

				if (mo3Stream.EndOfStream)
					return null;

				instruments[i] = instr;
			}

			return instruments;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private Sample[] LoadSampleInfo(Mo3Stream mo3Stream, FileHeader header, byte version)
		{
			Sample[] samples = new Sample[header.NumSamples];

			for (int i = 0; i < header.NumSamples; i++)
			{
				Sample sampleInfo = new Sample();

				sampleInfo.SampleName = ReadNullTerminatedString(mo3Stream);
				sampleInfo.FileName = version >= 5 ? ReadNullTerminatedString(mo3Stream) : [];

				sampleInfo.FreqFineTune = mo3Stream.Read_L_UINT32();
				sampleInfo.Transpose = mo3Stream.Read_INT8();
				sampleInfo.DefaultVolume = mo3Stream.Read_UINT8();
				sampleInfo.Panning = mo3Stream.Read_L_UINT16();
				sampleInfo.Length = mo3Stream.Read_L_UINT32();
				sampleInfo.LoopStart = mo3Stream.Read_L_UINT32();
				sampleInfo.LoopEnd = mo3Stream.Read_L_UINT32();
				sampleInfo.Flags = (SampleInfoFlag)mo3Stream.Read_L_UINT16();
				sampleInfo.VibType = mo3Stream.Read_UINT8();
				sampleInfo.VibSweep = mo3Stream.Read_UINT8();
				sampleInfo.VibDepth = mo3Stream.Read_UINT8();
				sampleInfo.VibRate = mo3Stream.Read_UINT8();
				sampleInfo.GlobalVol = mo3Stream.Read_UINT8();
				sampleInfo.SustainStart = mo3Stream.Read_L_UINT32();
				sampleInfo.SustainEnd = mo3Stream.Read_L_UINT32();
				sampleInfo.CompressedSize = mo3Stream.Read_L_INT32();
				sampleInfo.EncoderDelay = mo3Stream.Read_L_UINT16();
				sampleInfo.SharedOggHeader = (version >= 5) && ((sampleInfo.Flags & SampleInfoFlag.CompressionMask) == SampleInfoFlag.SharedOgg) ? mo3Stream.Read_L_INT16() : (short)0;

				if (mo3Stream.EndOfStream)
					return null;

				if ((sampleInfo.Flags & SampleInfoFlag.CompressionMask) == SampleInfoFlag.CompressionMpeg)
				{
					// Mpeg samples will always be 16-bit, so set the flag
					sampleInfo.Flags |= SampleInfoFlag._16Bit;
				}

				samples[i] = sampleInfo;
			}

			return samples;
		}



		/********************************************************************/
		/// <summary>
		/// Load the plugin information
		/// </summary>
		/********************************************************************/
		private IPlugin[] LoadPlugins(Mo3Stream mo3Stream, FileHeader header)
		{
			List<IPlugin> loadedPlugins = new List<IPlugin>();

			if (((header.Flags & HeaderFlag.HasPlugins) != 0) && (mo3Stream.Position < mo3Stream.Length))
			{
				byte pluginFlags = mo3Stream.Read_UINT8();

				if ((pluginFlags & 1) != 0)
				{
					// Channel plugins
					ChannelPlugin channelPlugin = new ChannelPlugin
					{
						Plugins = new uint[header.NumChannels]
					};

					mo3Stream.ReadArray_L_UINT32s(channelPlugin.Plugins, 0, header.NumChannels);

					loadedPlugins.Add(channelPlugin);
				}

				while (mo3Stream.Position < mo3Stream.Length)
				{
					byte plug = mo3Stream.Read_UINT8();
					if (plug == 0)
						break;

					uint len = mo3Stream.Read_L_UINT32();
					if ((len >= mo3Stream.Length) || ((mo3Stream.Length - len) < mo3Stream.Position))
						return null;

					FxPlugin plugin = new FxPlugin
					{
						Plugin = plug,
						Data = new byte[len]
					};

					mo3Stream.ReadInto(plugin.Data, 0, plugin.Data.Length);

					loadedPlugins.Add(plugin);
				}
			}

			return loadedPlugins.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the chunks
		/// </summary>
		/********************************************************************/
		private IChunk[] LoadChunks(Mo3Stream mo3Stream, Mo3Module module)
		{
			List<IChunk> loadedChunks = new List<IChunk>();

			while (mo3Stream.Position < (mo3Stream.Length - 8))
			{
				uint id = mo3Stream.Read_B_UINT32();
				uint len = mo3Stream.Read_L_UINT32();

				if ((len >= module.MusicSize) || ((module.MusicSize - len) < mo3Stream.Position))
					return null;

				switch (id)
				{
					// VERS
					case 0x56455253:
					{
						// Tracker magic bytes (depending on format)
						VersChunk versChunk = new VersChunk();

						switch (module.ModuleType)
						{
							case ModuleType.Mtm:
							{
								versChunk.Cwtv = (ushort)mo3Stream.Read_L_UINT32();
								break;
							}

							case ModuleType.S3M:
							{
								versChunk.Cwtv = (ushort)mo3Stream.Read_L_UINT32();
								break;
							}

							case ModuleType.Xm:
							{
								versChunk.CreatedWithTracker = new byte[len];
								mo3Stream.ReadInto(versChunk.CreatedWithTracker, 0, versChunk.CreatedWithTracker.Length);
								break;
							}

							case ModuleType.It:
							{
								versChunk.Cwtv = mo3Stream.Read_L_UINT16();
								versChunk.Cmwt = mo3Stream.Read_L_UINT16();
								break;
							}

							default:
								throw new NotImplementedException($"Module type {module.ModuleType} not supported for VERS chunk");
						}

						loadedChunks.Add(versChunk);
						break;
					}

					// OMPT
					case 0x4f4d5054:
					{
						// OpenMpt information chunk. Just load all data as it
						// and write it back in the saver
						OmptChunk omptChunk = new OmptChunk
						{
							Data = new byte[len]
						};

						mo3Stream.ReadInto(omptChunk.Data, 0, omptChunk.Data.Length);

						loadedChunks.Add(omptChunk);
						break;
					}

					// MIDI
					case 0x4d494449:
					{
						// Fill MIDI config
						MidiChunk midiChunk = new MidiChunk
						{
							Data = new byte[len]
						};

						mo3Stream.ReadInto(midiChunk.Data, 0, midiChunk.Data.Length);

						loadedChunks.Add(midiChunk);
						break;
					}

					default:
						throw new NotImplementedException($"Chunk with id {id:X8} not supported");
				}

				if (mo3Stream.EndOfStream)
					return null;
			}

			return loadedChunks.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Will read a null terminated string and return it "as it"
		/// </summary>
		/********************************************************************/
		private byte[] ReadNullTerminatedString(Mo3Stream mo3Stream)
		{
			List<byte> str = new List<byte>();

			for (;;)
			{
				byte b = mo3Stream.Read_UINT8();
				if (b == 0x00)
					break;

				str.Add(b);
			}

			return str.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Return the module type
		/// </summary>
		/********************************************************************/
		private ModuleType FindModuleType(FileHeader header)
		{
			if ((header.Flags & HeaderFlag.IsIT) != 0)
				return ModuleType.It;

			if ((header.Flags & HeaderFlag.IsS3M) != 0)
				return ModuleType.S3M;

			if ((header.Flags & HeaderFlag.IsMod) != 0)
				return ModuleType.Mod;

			if ((header.Flags & HeaderFlag.IsMTM) != 0)
				return ModuleType.Mtm;

			return ModuleType.Xm;
		}
		#endregion
	}
}
