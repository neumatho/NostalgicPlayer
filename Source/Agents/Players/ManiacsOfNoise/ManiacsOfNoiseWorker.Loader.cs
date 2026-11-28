/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.ManiacsOfNoise
{
	internal partial class ManiacsOfNoiseWorker
	{
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		private AgentResult CheckModule(ModuleStream moduleStream)
		{
			// Check the module size
			if (moduleStream.Length < 0x66e)
				return AgentResult.Unknown;

			// Read the first part of the file, so it is easier to search
			byte[] buffer = new byte[0x1000];

			moduleStream.Seek(0, SeekOrigin.Begin);
			moduleStream.ReadInto(buffer, 0, buffer.Length);

			return TestModule(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it's a Maniacs Of Noise player and
		/// extract needed information
		/// </summary>
		/********************************************************************/
		private AgentResult TestModule(byte[] buffer)
		{
			// Check the player assembler code to see if it's a Maniacs Of Noise player
			if ((buffer[0] != 0x4e) || (buffer[1] != 0xfa) || (buffer[4] != 0x4e) || (buffer[5] != 0xfa) || (buffer[8] != 0x4e) || (buffer[9] != 0xfa))
				return AgentResult.Unknown;

			// Find the init function
			int startOfInit = (((sbyte)buffer[2] << 8) | buffer[3]) + 2;
			if (startOfInit >= (buffer.Length - 12))
				return AgentResult.Unknown;

			if ((buffer[startOfInit] != 0x4b) || (buffer[startOfInit + 1] != 0xfa) || (buffer[startOfInit + 4] != 0x08) || (buffer[startOfInit + 5] != 0xad) || (buffer[startOfInit + 10] != 0x4a) || (buffer[startOfInit + 11] != 0x00))
				return AgentResult.Unknown;

			int globalsOffset = (((sbyte)buffer[startOfInit + 2] << 8) | buffer[startOfInit + 3]) + startOfInit + 2;
			if (globalsOffset >= buffer.Length)
				return AgentResult.Unknown;

			switch (globalsOffset - startOfInit)
			{
				case 0x662:
				{
					playerVersion = 1;
					break;
				}

				case 0x6a2:
				{
					playerVersion = 2;
					break;
				}

				case 0x68c:
				{
					playerVersion = 3;
					break;
				}

				case 0x6fa:
				{
					playerVersion = 4;
					break;
				}

				case 0x7da:
				{
					playerVersion = 5;
					break;
				}

				default:
					return AgentResult.Unknown;
			}

			// Find the set-sub-song function
			int startOfSetSubSong = (((sbyte)buffer[10] << 8) | buffer[11]) + 8 + 2;
			if (startOfSetSubSong >= (buffer.Length - 36))
				return AgentResult.Unknown;

			if ((buffer[startOfSetSubSong] != 0x4b) || (buffer[startOfSetSubSong + 1] != 0xfa) || (buffer[startOfSetSubSong + 4] != 0x02) || (buffer[startOfSetSubSong + 5] != 0x80) || (buffer[startOfSetSubSong + 10] != 0x67) || (buffer[startOfSetSubSong + 11] != 0x00))
				return AgentResult.Unknown;

			if ((buffer[startOfSetSubSong + 22] != 0xb0) || (buffer[startOfSetSubSong + 23] != 0x2d))
				return AgentResult.Unknown;

			int numberOfSubSongsOffset = (((sbyte)buffer[startOfSetSubSong + 24] << 8) | buffer[startOfSetSubSong + 25]) + globalsOffset;
			if (numberOfSubSongsOffset >= (buffer.Length - 1))
				return AgentResult.Unknown;

			numberOfSubSongs = buffer[numberOfSubSongsOffset];

			if ((buffer[startOfSetSubSong + 30] == 0xc0) && (buffer[startOfSetSubSong + 31] == 0xfc))
				startOfSetSubSong += 2;

			if ((buffer[startOfSetSubSong + 32] != 0x41) || (buffer[startOfSetSubSong + 33] != 0xfa))
				return AgentResult.Unknown;

			subSongsOffset = (((sbyte)buffer[startOfSetSubSong + 34] << 8) | buffer[startOfSetSubSong + 35]) + startOfSetSubSong + 32 + 2;
			if (subSongsOffset >= buffer.Length)
				return AgentResult.Unknown;

			// Find the offset to the sample sequence table
			if (playerVersion == 5)
			{
				int startOfPlay = (((sbyte)buffer[6] << 8) | buffer[7]) + 4 + 2;
				if (startOfPlay >= buffer.Length)
					return AgentResult.Unknown;

				sampleSequenceTableOffset = 0;

				for (int i = startOfPlay; i < (buffer.Length - 12); i += 2)
				{
					if ((buffer[i] == 0x52) && (buffer[i + 1] == 0x28) && (buffer[i + 2] == 0x00) && (buffer[i + 3] == 0x41) && (buffer[i + 8] == 0x45) && (buffer[i + 9] == 0xfa))
					{
						sampleSequenceTableOffset = ((buffer[i + 10] << 8) | buffer[i + 11]) + i + 8 + 2;
						break;
					}
				}

				if (sampleSequenceTableOffset == 0)
					return AgentResult.Unknown;
			}

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Load the module into memory
		/// </summary>
		/********************************************************************/
		private bool LoadModule(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!LoadSubSongInfo(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_SUBSONG;
				return false;
			}

			long sampleOffset = moduleStream.Position;

			if (!LoadPositionLists(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_POSITION_LISTS;
				return false;
			}

			if (!LoadTracks(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_TRACKS;
				return false;
			}

			moduleStream.Seek(sampleOffset, SeekOrigin.Begin);

			if (!LoadSampleInfo(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_SAMPLEINFO;
				return false;
			}

			if (!LoadEnvelopes(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_ENVELOPES;
				return false;
			}

			if (!LoadVibratos(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_VIBRATOS;
				return false;
			}

			if (!LoadArpeggios(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_ARPEGGIOS;
				return false;
			}

			if (!LoadSampleData(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_SAMPLES;
				return false;
			}

			if (!LoadWaveforms(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_SAMPLES;
				return false;
			}

			if (!LoadSampleSequences(moduleStream))
			{
				errorMessage = Resources.IDS_MON_ERR_LOADING_SAMPLESEQUENCES;
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the sub-song information for all sub-songs
		/// </summary>
		/********************************************************************/
		private bool LoadSubSongInfo(ModuleStream moduleStream)
		{
			subSongs = new SongInfo[numberOfSubSongs];

			// Seek to the sub-song list
			moduleStream.Seek(subSongsOffset, SeekOrigin.Begin);

			for (int i = 0; i < numberOfSubSongs; i++)
			{
				SongInfo song = new SongInfo();

				moduleStream.ReadArray_B_UINT16s(song.PositionListOffsets, 0, 4);

				song.MasterVolume = moduleStream.Read_UINT8();
				song.UseFilter = moduleStream.Read_UINT8() != 0;
				song.StartSpeed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (playerVersion <= 2)
					moduleStream.Seek(5, SeekOrigin.Current);
				else
					moduleStream.Seek(1, SeekOrigin.Current);

				subSongs[i] = song;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the position lists
		/// </summary>
		/********************************************************************/
		private bool LoadPositionLists(ModuleStream moduleStream)
		{
			positionLists = new Dictionary<ushort, ushort[]>();

			foreach (SongInfo song in subSongs)
			{
				for (int i = 0; i < 4; i++)
				{
					ushort positionListOffset = song.PositionListOffsets[i];

					// Zero means voice is muted
					if (positionListOffset == 0)
						continue;

					if (positionLists.ContainsKey(positionListOffset))
						continue;

					// Seek to position list
					moduleStream.Seek(subSongsOffset + positionListOffset, SeekOrigin.Begin);

					// Load the position list
					ushort[] positionList = LoadSinglePositionList(moduleStream);
					if (positionList == null)
						return false;

					positionLists[positionListOffset] = positionList;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single position list
		/// </summary>
		/********************************************************************/
		private ushort[] LoadSinglePositionList(ModuleStream moduleStream)
		{
			List<ushort> positionList = new List<ushort>();

			for (;;)
			{
				ushort trackOffset = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return null;

				positionList.Add(trackOffset);

				if (trackOffset >= 0xf000)
				{
					// Position list command
					int cmd = trackOffset & 0xff00;

					// Stop song or jump
					if ((cmd == 0xf600) || (cmd == 0xf500))
						break;
				}
			}

			return positionList.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the tracks
		/// </summary>
		/********************************************************************/
		private bool LoadTracks(ModuleStream moduleStream)
		{
			trackLists = new SortedList<ushort, byte[]>();

			foreach (ushort[] positionList in positionLists.Values)
			{
				foreach (ushort trackOffset in positionList)
				{
					// Skip commands
					if (trackOffset >= 0xf000)
						continue;

					if (trackLists.ContainsKey(trackOffset))
						continue;

					// Seek to track
					moduleStream.Seek(subSongsOffset + trackOffset, SeekOrigin.Begin);

					byte[] track = LoadSingleTrack(moduleStream);
					if (track == null)
						return false;

					trackLists[trackOffset] = track;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single track
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleTrack(ModuleStream moduleStream)
		{
			List<byte> trackBytes = new List<byte>();

			for (;;)
			{
				byte cmd = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				trackBytes.Add(cmd);

				if ((playerVersion == 1) && (cmd == 0xff))	// End of track?
					break;

				if ((playerVersion != 1) && (cmd >= 0x80) && ((cmd & 0x0f) == 0x00))	// End of track?
					break;

				int argCount = FindTrackCommandArgumentCount(cmd);
				if (argCount == -1)
					return null;

				for (; argCount > 0; argCount--)
					trackBytes.Add(moduleStream.Read_UINT8());
			}

			return trackBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Find number of arguments for the given track command
		/// </summary>
		/********************************************************************/
		private int FindTrackCommandArgumentCount(byte cmd)
		{
			if (cmd <= 0x7f)
				return 1;

			if (playerVersion == 1)
			{
				switch (cmd)
				{
					case 0xff:
						return 0;

					case 0xfe:
					case 0xfd:
						return 1;

					case 0xfc:
						return 2;

					case 0xfb:
						return 3;
				}
			}
			else
			{
				switch (cmd & 0x0f)
				{
					case 0x00:
						return 0;

					case 0x01:
					case 0x02:
					case 0x05:
						return 1;

					case 0x03:
						return 2;

					case 0x04:
						return 3;
				}

				if (playerVersion >= 3)
				{
					switch (cmd & 0x0f)
					{
						case 0x07:
						case 0x08:
						case 0x09:
							return 1;

						case 0x06:
							return 2;
					}
				}
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample information
		/// </summary>
		/********************************************************************/
		private bool LoadSampleInfo(ModuleStream moduleStream)
		{
			// Find max number of instrument used
			HashSet<byte> samplesUsed = new HashSet<byte>();

			foreach (byte[] trackData in trackLists.Values)
			{
				for (int i = 0; i < trackData.Length;)
				{
					byte cmd = trackData[i];

					if (((playerVersion == 1) && (cmd == 0xfe)) || ((playerVersion != 1) && (cmd >= 0x80) && ((cmd & 0x0f) == 0x01)))
						samplesUsed.Add(trackData[i + 1]);

					i += FindTrackCommandArgumentCount(cmd) + 1;
				}
			}

			samples = new Sample[samplesUsed.Max() + 1];

			for (int i = 0; i < samples.Length; i++)
			{
				Sample sample = new Sample();

				byte type = moduleStream.Read_UINT8();

				if (playerVersion <= 2)
				{
					switch (type)
					{
						case 0:
						{
							sample.Type = SampleType.Sample;
							break;
						}

						case 1:
						{
							sample.Type = SampleType.WaveformWithReset;
							break;
						}

						case 2:
						{
							sample.Type = SampleType.WaveformContinue;
							break;
						}

						default:
							return false;
					}
				}
				else if (playerVersion == 3)
				{
					switch (type)
					{
						case 0:
						{
							sample.Type = SampleType.None;
							break;
						}

						case 1:
						{
							sample.Type = SampleType.Sample;
							break;
						}

						default:
							return false;
					}
				}
				else if (playerVersion == 5)
				{
					switch (type)
					{
						case 0:
						{
							sample.Type = SampleType.SampleSequence;
							break;
						}

						case 1:
						{
							sample.Type = SampleType.Sample;
							break;
						}

						default:
							return false;
					}
				}
				else
					sample.Type = SampleType.Sample;

				sample.WaveformSpeed = moduleStream.Read_UINT8();
				sample.PeriodMultiplier = moduleStream.Read_B_UINT16();
				sample.DataOffset = moduleStream.Read_B_UINT32();
				sample.Length = moduleStream.Read_B_UINT16();
				sample.LoopStart = moduleStream.Read_B_UINT16();
				sample.EnvelopeOffset = moduleStream.Read_B_UINT16();
				sample.Volume = moduleStream.Read_UINT8();
				sample.EnvelopeSpeed = moduleStream.Read_UINT8();
				sample.VibratoOffset = moduleStream.Read_B_UINT16();
				sample.VibratoDelay = moduleStream.Read_UINT8();
				sample.VibratoSpeed = moduleStream.Read_UINT8();
				sample.ArpeggioOffset = moduleStream.Read_B_UINT16();
				sample.ArpeggioSpeed = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return false;

				if (sample.Type != SampleType.Sample)
					sample.DataOffset >>= 16;

				if (!samplesUsed.Contains((byte)i))
				{
					sample.Length = 0;
					sample.LoopStart = 0;
				}

				moduleStream.Seek(41, SeekOrigin.Current);

				samples[i] = sample;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the envelopes
		/// </summary>
		/********************************************************************/
		private bool LoadEnvelopes(ModuleStream moduleStream)
		{
			envelopes = new Dictionary<ushort, byte[]>();

			foreach (Sample sample in samples)
			{
				if (envelopes.ContainsKey(sample.EnvelopeOffset))
					continue;

				moduleStream.Seek(subSongsOffset + sample.EnvelopeOffset, SeekOrigin.Begin);

				byte[] envelope = LoadSingleEnvelope(moduleStream);
				if (envelope == null)
					return false;

				envelopes[sample.EnvelopeOffset] = envelope;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single envelope
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleEnvelope(ModuleStream moduleStream)
		{
			List<byte> envelopeBytes = new List<byte>();

			for (;;)
			{
				byte data = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				envelopeBytes.Add(data);

				if ((playerVersion <= 2) && (data >= 0x80))
					break;

				if (playerVersion >= 3)
				{
					if (data == 0x80)
					{
						envelopeBytes.Add(moduleStream.Read_UINT8());
						break;
					}

					if (data == 0x81)
						envelopeBytes.Add(moduleStream.Read_UINT8());
				}
			}

			return envelopeBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the vibratos
		/// </summary>
		/********************************************************************/
		private bool LoadVibratos(ModuleStream moduleStream)
		{
			vibratos = new Dictionary<ushort, sbyte[]>();

			foreach (Sample sample in samples)
			{
				// Zero means not used
				if (sample.VibratoOffset == 0)
					continue;

				if (vibratos.ContainsKey(sample.VibratoOffset))
					continue;

				moduleStream.Seek(subSongsOffset + sample.VibratoOffset, SeekOrigin.Begin);

				sbyte[] vibrato = LoadSingleVibrato(moduleStream);
				if (vibrato == null)
					return false;

				vibratos[sample.VibratoOffset] = vibrato;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single vibrato
		/// </summary>
		/********************************************************************/
		private sbyte[] LoadSingleVibrato(ModuleStream moduleStream)
		{
			List<sbyte> vibratoBytes = new List<sbyte>();

			for (;;)
			{
				sbyte data = moduleStream.Read_INT8();

				if (moduleStream.EndOfStream)
					return null;

				vibratoBytes.Add(data);

				if (data == -128)
				{
					data = moduleStream.Read_INT8();
					vibratoBytes.Add(data);

					if ((playerVersion >= 3) && (data < 0))
					{
						for (int i = -data - 2; i > 0; i--)
							vibratoBytes.Add(moduleStream.Read_INT8());

						continue;
					}

					break;
				}
			}

			return vibratoBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the arpeggios
		/// </summary>
		/********************************************************************/
		private bool LoadArpeggios(ModuleStream moduleStream)
		{
			arpeggios = new Dictionary<ushort, byte[]>();

			foreach (Sample sample in samples)
			{
				// Zero means not used
				if (sample.ArpeggioOffset == 0)
					continue;

				if (arpeggios.ContainsKey(sample.ArpeggioOffset))
					continue;

				moduleStream.Seek(subSongsOffset + sample.ArpeggioOffset, SeekOrigin.Begin);

				byte[] arp = LoadSingleArpeggio(moduleStream);
				if (arp == null)
					return false;

				arpeggios[sample.ArpeggioOffset] = arp;
			}

			if (playerVersion >= 4)
			{
				foreach (byte[] trackData in trackLists.Values)
				{
					for (int i = 0; i < trackData.Length;)
					{
						byte cmd = trackData[i];

						if ((cmd >= 0x80) && ((cmd & 0x0f) == 0x06))
						{
							ushort offset = (ushort)((trackData[i + 1] << 8) | trackData[i + 2]);

							if ((offset != 0) && !arpeggios.ContainsKey(offset))
							{
								moduleStream.Seek(subSongsOffset + offset, SeekOrigin.Begin);

								byte[] arp = LoadSingleArpeggio(moduleStream);
								if (arp == null)
									return false;

								arpeggios[offset] = arp;
							}
						}

						i += FindTrackCommandArgumentCount(cmd) + 1;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single arpeggio
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleArpeggio(ModuleStream moduleStream)
		{
			List<byte> arpeggioBytes = new List<byte>();

			for (;;)
			{
				byte data = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				arpeggioBytes.Add(data);

				if ((playerVersion < 4) && (data >= 0x80))
					break;

				if ((playerVersion >= 4) && (data == 0x80))
				{
					arpeggioBytes.Add(moduleStream.Read_UINT8());
					break;
				}
			}

			return arpeggioBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the sample sequences
		/// </summary>
		/********************************************************************/
		private bool LoadSampleSequences(ModuleStream moduleStream)
		{
			if (playerVersion == 5)
			{
				sampleSequences = new Dictionary<uint, byte[]>();

				foreach (Sample sample in samples)
				{
					if (sample.Type == SampleType.SampleSequence)
					{
						if (sampleSequences.ContainsKey(sample.DataOffset))
							continue;

						moduleStream.Seek(subSongsOffset + sample.DataOffset, SeekOrigin.Begin);

						byte[] seq = LoadSingleSampleSequence(moduleStream);
						if (seq == null)
							return false;

						sampleSequences[sample.DataOffset] = seq;
					}
				}

				if (sampleSequences.Count > 0)
				{
					int numberOfSequences = sampleSequences.Values.Select(ss =>
					{
						int max = 0;

						for (int i = 0; i < ss.Length; i++)
						{
							if (ss[i] == 0xff)
								break;

							if (ss[i] == 0xfe)
								i++;
							else
								max = Math.Max(ss[i], max);
						}

						return max;
					}).Max();
					numberOfSequences++;

					sampleSequenceTable = new ushort[numberOfSequences * 2];

					moduleStream.Seek(sampleSequenceTableOffset, SeekOrigin.Begin);
					moduleStream.ReadArray_B_UINT16s(sampleSequenceTable, 0, sampleSequenceTable.Length);

					if (moduleStream.EndOfStream)
						return false;

					for (int i = 0; i < sampleSequenceTable.Length; i += 2)
					{
						int length = sampleSequenceTable[i] * 2;
						ushort offset = sampleSequenceTable[i + 1];

						if (sampleData.TryGetValue(offset, out sbyte[] oldData) && (length <= oldData.Length))
							continue;

						moduleStream.Seek(subSongsOffset + offset, SeekOrigin.Begin);

						sbyte[] data = moduleStream.ReadSampleData(length, out int readBytes);

						if (readBytes != length)
							return false;

						sampleData[offset] = data;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single sample sequence
		/// </summary>
		/********************************************************************/
		private byte[] LoadSingleSampleSequence(ModuleStream moduleStream)
		{
			List<byte> sequenceBytes = new List<byte>();

			for (;;)
			{
				byte data = moduleStream.Read_UINT8();

				if (moduleStream.EndOfStream)
					return null;

				sequenceBytes.Add(data);

				if (data == 0xfe)
					sequenceBytes.Add(moduleStream.Read_UINT8());

				if (data == 0xff)
				{
					sequenceBytes.Add(moduleStream.Read_UINT8());
					break;
				}
			}

			return sequenceBytes.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load all the samples
		/// </summary>
		/********************************************************************/
		private bool LoadSampleData(ModuleStream moduleStream)
		{
			sampleData = new Dictionary<uint, sbyte[]>();

			foreach (Sample sample in samples)
			{
				if ((sample.Type == SampleType.Sample) && (sample.Length > 0))
				{
					if (sampleData.TryGetValue(sample.DataOffset, out sbyte[] oldData) && ((sample.Length * 2) <= oldData.Length))
						continue;

					moduleStream.Seek(subSongsOffset + sample.DataOffset, SeekOrigin.Begin);

					int length = sample.Length * 2;
					sbyte[] data = moduleStream.ReadSampleData(length, out int readBytes);

					if (readBytes != length)
						return false;

					sampleData[sample.DataOffset] = data;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load all the waveforms
		/// </summary>
		/********************************************************************/
		private bool LoadWaveforms(ModuleStream moduleStream)
		{
			waveforms = new Dictionary<uint, ushort[]>();

			if (playerVersion <= 2)
			{
				foreach (Sample sample in samples)
				{
					if (sample.Type != SampleType.Sample)
					{
						if (waveforms.ContainsKey(sample.DataOffset))
							continue;

						moduleStream.Seek(subSongsOffset + sample.DataOffset, SeekOrigin.Begin);

						ushort[] waveform = LoadSingleWaveform(moduleStream);
						if (waveform == null)
							return false;

						waveforms[sample.DataOffset] = waveform;
					}
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load a single waveform
		/// </summary>
		/********************************************************************/
		private ushort[] LoadSingleWaveform(ModuleStream moduleStream)
		{
			List<ushort> waveform = new List<ushort>();

			int waveformLength = -1;

			for (;;)
			{
				ushort offset = moduleStream.Read_B_UINT16();

				if (moduleStream.EndOfStream)
					return null;

				waveform.Add(offset);

				if (offset >= 0xfffd)
				{
					ushort arg = moduleStream.Read_B_UINT16();

					if (moduleStream.EndOfStream)
						return null;

					waveform.Add(arg);

					if (offset == 0xffff)
						break;

					if (offset == 0xfffd)
						waveformLength = arg;
				}
				else
				{
					if (waveformLength == -1)
						return null;

					if (sampleData.TryGetValue(offset, out sbyte[] oldData) && ((waveformLength * 2) <= oldData.Length))
						continue;

					sbyte[] waveformData = LoadSingleWaveformData(moduleStream, offset, waveformLength * 2);
					if (waveformData == null)
						return null;

					sampleData[offset] = waveformData;
				}
			}

			return waveform.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Load a single waveform data
		/// </summary>
		/********************************************************************/
		private sbyte[] LoadSingleWaveformData(ModuleStream moduleStream, ushort offset, int length)
		{
			long position = moduleStream.Position;

			moduleStream.Seek(subSongsOffset + offset, SeekOrigin.Begin);

			sbyte[] data = moduleStream.ReadSampleData(length, out int readBytes);
			if (readBytes != length)
				return null;

			moduleStream.Seek(position, SeekOrigin.Begin);

			return data;
		}
	}
}
