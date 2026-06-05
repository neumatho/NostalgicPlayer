/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility.Extensions;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	internal partial class MusiclineEditorWorker
	{
		private ushort[] zeroChannel;
		private Instrument zeroInstrument;

		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		private AgentResult CheckModule(ModuleStream moduleStream)
		{
			// Check the module size
			long fileSize = moduleStream.Length;
			if (fileSize < 12)
				return AgentResult.Unknown;

			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark(8) != "MLEDMODL")
				return AgentResult.Unknown;

			// Skip version
			if (!ReadVersion(moduleStream))
				return AgentResult.Unknown;

			// Find all TUNE chunks
			bool foundTune = false;
			bool hasMixerMode = false;

			for (;;)
			{
				string mark = moduleStream.ReadMark();
				uint length = moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
					return AgentResult.Unknown;

				if (mark != "TUNE")
					break;

				foundTune = true;

				moduleStream.Seek(38, SeekOrigin.Current);

				byte playMode = moduleStream.Read_UINT8();

				if (playMode == 1)
					hasMixerMode = true;

				moduleStream.Seek(length - 39, SeekOrigin.Current);
			}

			if (!foundTune)
				return AgentResult.Unknown;

			if (hasMixerMode && (currentModuleType == ModuleType._8Channels))
				return AgentResult.Ok;

			if (!hasMixerMode && (currentModuleType == ModuleType._4Channels))
				return AgentResult.Ok;

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Read the version number
		/// </summary>
		/********************************************************************/
		private bool ReadVersion(ModuleStream moduleStream)
		{
			uint length = moduleStream.Read_B_UINT32();

			// Some ancient modules do not have a MODL length,
			// but have the VERS chunk directly after MODL
			if (length != 0x56455253)
			{
				moduleStream.Seek(length, SeekOrigin.Current);

				if (moduleStream.ReadMark() != "VERS")
					return false;
			}

			length = moduleStream.Read_B_UINT32();

			moduleStream.Seek(length, SeekOrigin.Current);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the module into memory
		/// </summary>
		/********************************************************************/
		private bool LoadModule(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			moduleStream.Seek(8, SeekOrigin.Begin);
			ReadVersion(moduleStream);

			Encoding encoder = EncoderCollection.Amiga;

			InitializeStructures();

			// Load chunks
			for (;;)
			{
				string mark = moduleStream.ReadMark();
				uint length = moduleStream.Read_B_UINT32();

				if (moduleStream.EndOfStream)
					break;

				switch (mark)
				{
					case "TUNE":
					{
						if (!LoadTuneChunk(moduleStream, encoder))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_SONG;
							return false;
						}

						break;
					}

					case "PART":
					{
						if (!LoadPartChunk(moduleStream))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_TRACK;
							return false;
						}

						if ((moduleStream.Position & 1) != 0)
							moduleStream.Seek(1, SeekOrigin.Current);

						break;
					}

					case "ARPG":
					{
						if (!LoadArpgChunk(moduleStream, length))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_ARPEGGIO;
							return false;
						}

						break;
					}

					case "INST":
					{
						if (!LoadInstChunk(moduleStream, encoder))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_INSTRUMENT;
							return false;
						}

						break;
					}

					case "SMPL":
					{
						if (!LoadSmplChunk(moduleStream, length, encoder))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_SAMPLE;
							return false;
						}

						// Length does not include the sample header
						length += 6;
						break;
					}

					case "INFO":
					{
						if (!LoadInfoChunk(moduleStream, length, encoder))
						{
							errorMessage = Resources.IDS_MLE_ERR_LOADING_INFORMATION;
							return false;
						}

						break;
					}

					default:
					{
						// Unknown chunk
						errorMessage = Resources.IDS_MLE_ERR_MODULE_CORRUPT;
						return false;
					}
				}
			}

			if (tunes.Count == 0)
			{
				errorMessage = Resources.IDS_MLE_ERR_LOADING_NO_SONGS;
				return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all structures
		/// </summary>
		/********************************************************************/
		private void InitializeStructures()
		{
			zeroChannel = CreateSequence();
			zeroInstrument = new Instrument();

			tunes = new List<Tune>();

			parts = InitializeSingleStructure<Part>(1024);
			numberOfParts = 0;

			arpeggios = InitializeSingleStructure<Arpeggio>(256);

			instruments = InitializeSingleStructure<Instrument>(256, zeroInstrument);
			numberOfInstruments = 0;

			samples = InitializeSingleStructure<Sample>(256);
			numberOfSamples = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all structures
		/// </summary>
		/********************************************************************/
		private T[] InitializeSingleStructure<T>(int count, T zero = default) where T : new()
		{
			zero ??= new T();

			T[] array = new T[count];

			for (int i = 0; i < count; i++)
				array[i] = zero;

			return array;
		}



		/********************************************************************/
		/// <summary>
		/// Create channel sequence
		/// </summary>
		/********************************************************************/
		private ushort[] CreateSequence()
		{
			ushort[] sequence = new ushort[256];

			for (int i = 0; i < 256; i++)
				sequence[i] = 0x0060;	// TNE: Changed from original 0x0010. 0x0060 means stop channel

			return sequence;
		}



		/********************************************************************/
		/// <summary>
		/// Load the TUNE chunk
		/// </summary>
		/********************************************************************/
		private bool LoadTuneChunk(ModuleStream moduleStream, Encoding encoder)
		{
			Tune tune = new Tune();

			tune.Title = moduleStream.ReadString(encoder, 32);
			tune.Tempo = moduleStream.Read_B_UINT16();
			tune.Speed = moduleStream.Read_UINT8();
			tune.Groove = moduleStream.Read_UINT8();
			tune.Volume = moduleStream.Read_B_UINT16();
			tune.PlayMode = moduleStream.Read_UINT8() != 0;
			tune.Channels = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Fletch Dances have a lot of sub-songs with 0 channels.
			// If we reach such one, ignore it
			if (tune.Channels == 0)
				return true;

			uint[] channelSizes = new uint[tune.Channels];

			moduleStream.ReadArray_B_UINT32s(channelSizes, 0, tune.Channels);

			if (moduleStream.EndOfStream)
				return false;

			tune.Sequences = new ushort[tune.Channels][];

			for (int i = 0; i < tune.Channels; i++)
			{
				if (channelSizes[i] == 0)
				{
					tune.Sequences[i] = zeroChannel;
					continue;
				}

				ushort[] sequence = CreateSequence();

				moduleStream.ReadArray_B_UINT16s(sequence, 0, (int)(channelSizes[i] / 2));

				if (moduleStream.EndOfStream)
					return false;

				tune.Sequences[i] = sequence;
			}

			tunes.Add(tune);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the PART chunk
		/// </summary>
		/********************************************************************/
		private bool LoadPartChunk(ModuleStream moduleStream)
		{
			Part part = new Part();

			ushort partNumber = moduleStream.Read_B_UINT16();

			int line = 0;

			for (;;)
			{
				byte flag = moduleStream.Read_UINT8();
				if (flag == 0xff)
					break;

				if (moduleStream.EndOfStream)
					return false;

				if (line == part.PartData.Length)
					return false;

				PartLine partLine = part.PartData[line++];

				if ((flag & 0x01) != 0)
				{
					partLine.Note = moduleStream.Read_UINT8();
					partLine.Instrument = moduleStream.Read_UINT8();

					if (moduleStream.EndOfStream)
						return false;
				}

				for (int i = 0; i < 5; i++)
				{
					if ((flag & (1 << (i + 1))) != 0)
					{
						partLine.Effects[i].Effect = (PartEffect)moduleStream.Read_UINT8();
						partLine.Effects[i].Argument = moduleStream.Read_UINT8();

						if (moduleStream.EndOfStream)
							return false;
					}
				}
			}

			parts[partNumber] = part;
			numberOfParts++;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the ARPG chunk
		/// </summary>
		/********************************************************************/
		private bool LoadArpgChunk(ModuleStream moduleStream, uint length)
		{
			Arpeggio arp = new Arpeggio();

			ushort arpNumber = moduleStream.Read_B_UINT16();

			uint totalLines = (length - 2) / 6;

			for (uint line = 0; line < totalLines; line++)
			{
				ArpeggioLine arpLine = arp.ArpeggioData[line];

				arpLine.NoteTranspose = moduleStream.Read_INT8();
				arpLine.SampleNumber = moduleStream.Read_UINT8();

				for (int i = 0; i < 2; i++)
				{
					arpLine.Effects[i].Effect = (ArpeggioEffect)moduleStream.Read_UINT8();
					arpLine.Effects[i].Argument = moduleStream.Read_UINT8();
				}

				if (moduleStream.EndOfStream)
					return false;
			}

			arpeggios[arpNumber] = arp;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the INST chunk
		/// </summary>
		/********************************************************************/
		private bool LoadInstChunk(ModuleStream moduleStream, Encoding encoder)
		{
			Instrument inst = new Instrument();

			inst.Title = moduleStream.ReadString(encoder, 32);
			inst.SampleNumber = moduleStream.Read_UINT8();
			inst.SampleType = (SampleType)moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(4, SeekOrigin.Current);

			inst.SampleLength = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(4, SeekOrigin.Current);

			inst.SampleRepeatLength = moduleStream.Read_B_UINT16();
			inst.FineTune = moduleStream.Read_B_INT16();
			inst.SemiTone = moduleStream.Read_B_INT16();
			inst.SampleStart = moduleStream.Read_B_UINT16();
			inst.SampleEnd = moduleStream.Read_B_UINT16();
			inst.SampleRepeatStart = moduleStream.Read_B_UINT16();
			inst.SampleRepeatLen = moduleStream.Read_B_UINT16();
			inst.Volume = moduleStream.Read_B_UINT16();
			inst.Transposable = moduleStream.Read_UINT8() != 0;
			inst.SlideSpeed = moduleStream.Read_UINT8();
			inst.Effect1 = (InstrumentEffect1)moduleStream.Read_UINT8();
			inst.Effect2 = (InstrumentEffect2)moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Read envelope data

			inst.EnvelopeAttackLength = moduleStream.Read_B_UINT16();
			inst.EnvelopeDecayLength = moduleStream.Read_B_UINT16();
			inst.EnvelopeSustainLength = moduleStream.Read_B_UINT16();
			inst.EnvelopeReleaseLength = moduleStream.Read_B_UINT16();
			inst.EnvelopeAttackSpeed = moduleStream.Read_B_UINT16();
			inst.EnvelopeDecaySpeed = moduleStream.Read_B_UINT16();
			inst.EnvelopeSustainSpeed = moduleStream.Read_B_UINT16();
			inst.EnvelopeReleaseSpeed = moduleStream.Read_B_UINT16();
			inst.EnvelopeAttackVolume = moduleStream.Read_B_UINT16();
			inst.EnvelopeDecayVolume = moduleStream.Read_B_UINT16();
			inst.EnvelopeSustainVolume = moduleStream.Read_B_UINT16();
			inst.EnvelopeReleaseVolume = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read vibrato data

			inst.VibratoDirection = moduleStream.Read_UINT8() != 0 ? Direction.Upward : Direction.Downward;
			inst.VibratoWaveType = (WaveType)moduleStream.Read_UINT8();
			inst.VibratoSpeed = moduleStream.Read_B_UINT16();
			inst.VibratoDelay = moduleStream.Read_B_UINT16();
			inst.VibratoAttackSpeed = moduleStream.Read_B_UINT16();
			inst.VibratoAttack = moduleStream.Read_B_UINT16();
			inst.VibratoDepth = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read tremolo data

			inst.TremoloDirection = moduleStream.Read_UINT8() != 0 ? Direction.Upward : Direction.Downward;
			inst.TremoloWaveType = (WaveType)moduleStream.Read_UINT8();
			inst.TremoloSpeed = moduleStream.Read_B_UINT16();
			inst.TremoloDelay = moduleStream.Read_B_UINT16();
			inst.TremoloAttackSpeed = moduleStream.Read_B_UINT16();
			inst.TremoloAttack = moduleStream.Read_B_UINT16();
			inst.TremoloDepth = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read arpeggio data

			inst.ArpeggioTable = moduleStream.Read_B_UINT16();
			inst.ArpeggioSpeed = moduleStream.Read_UINT8();
			inst.ArpeggioGroove = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Read transform data

			inst.EnvTraPhaFilBits = (InstrumentFlag)moduleStream.Read_UINT8();
			moduleStream.ReadInto(inst.TransformWaveNumbers, 0, 5);
			inst.TransformStart = moduleStream.Read_B_UINT16();
			inst.TransformRepeat = moduleStream.Read_B_UINT16();
			inst.TransformRepeatEnd = moduleStream.Read_B_UINT16();
			inst.TransformSpeed = moduleStream.Read_B_UINT16();
			inst.TransformTurns = moduleStream.Read_B_UINT16();
			inst.TransformDelay = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read phase data

			inst.PhaseStart = moduleStream.Read_B_UINT16();
			inst.PhaseRepeat = moduleStream.Read_B_UINT16();
			inst.PhaseRepeatEnd = moduleStream.Read_B_UINT16();
			inst.PhaseSpeed = moduleStream.Read_B_UINT16();
			inst.PhaseTurns = moduleStream.Read_B_UINT16();
			inst.PhaseDelay = moduleStream.Read_B_UINT16();
			inst.PhaseType = (PhaseType)moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read mix data

			inst.MixResLooBits = (MixFlag)moduleStream.Read_UINT8();
			inst.MixWaveNumber = moduleStream.Read_UINT8();
			inst.MixStart = moduleStream.Read_B_UINT16();
			inst.MixRepeat = moduleStream.Read_B_UINT16();
			inst.MixRepeatEnd = moduleStream.Read_B_UINT16();
			inst.MixSpeed = moduleStream.Read_B_UINT16();
			inst.MixTurns = moduleStream.Read_B_UINT16();
			inst.MixDelay = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			// Read resonance data

			inst.ResonanceStart = moduleStream.Read_B_UINT16();
			inst.ResonanceRepeat = moduleStream.Read_B_UINT16();
			inst.ResonanceRepeatEnd = moduleStream.Read_B_UINT16();
			inst.ResonanceSpeed = moduleStream.Read_B_UINT16();
			inst.ResonanceTurns = moduleStream.Read_B_UINT16();
			inst.ResonanceDelay = moduleStream.Read_B_UINT16();
			inst.MixResFilBoost = (BoostFlag)moduleStream.Read_UINT8();
			inst.ResponanceAmp = moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Read filter data

			inst.FilterStart = moduleStream.Read_B_UINT16();
			inst.FilterRepeat = moduleStream.Read_B_UINT16();
			inst.FilterRepeatEnd = moduleStream.Read_B_UINT16();
			inst.FilterSpeed = moduleStream.Read_B_UINT16();
			inst.FilterTurns = moduleStream.Read_B_UINT16();
			inst.FilterDelay = moduleStream.Read_B_UINT16();
			moduleStream.Seek(1, SeekOrigin.Current);
			inst.FilterType = (FilterType)moduleStream.Read_UINT8();

			if (moduleStream.EndOfStream)
				return false;

			// Read play loop data

			inst.LoopStart = moduleStream.Read_B_UINT16();
			inst.LoopRepeat = moduleStream.Read_B_UINT16();
			inst.LoopRepeatEnd = moduleStream.Read_B_UINT16();
			inst.LoopLength = moduleStream.Read_B_UINT16();
			inst.LoopLoopStep = moduleStream.Read_B_UINT16();
			inst.LoopWait = moduleStream.Read_B_UINT16();
			inst.LoopDelay = moduleStream.Read_B_UINT16();
			inst.LoopTurns = moduleStream.Read_B_UINT16();

			if (moduleStream.EndOfStream)
				return false;

			instruments[++numberOfInstruments] = inst;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Load the SMPL chunk
		/// </summary>
		/********************************************************************/
		private bool LoadSmplChunk(ModuleStream moduleStream, uint length, Encoding encoder)
		{
			uint originalLength = moduleStream.Read_B_UINT32();
			byte deltaCommand = moduleStream.Read_UINT8();
			moduleStream.Seek(1, SeekOrigin.Current);

			Sample sample = new Sample();

			sample.Title = moduleStream.ReadString(encoder, 32);

			if (moduleStream.EndOfStream)
				return false;

			moduleStream.Seek(1, SeekOrigin.Current);

			sample.SampleType = (SampleType)moduleStream.Read_UINT8();
			sample.SamplePointer = new SamplePointer(null, moduleStream.Read_B_UINT32());
			sample.SampleLength = moduleStream.Read_B_UINT16();
			sample.SampleRepeatPointer = new SamplePointer(null, moduleStream.Read_B_UINT32());
			sample.SampleRepeatLength = moduleStream.Read_B_UINT16();
			sample.FineTune = moduleStream.Read_B_INT16();
			sample.SemiTone = moduleStream.Read_B_INT16();

			if (moduleStream.EndOfStream)
				return false;

			int extraToAllocate = originalLength == 256 ? 240 : 0;

			uint sampleLength = length - 50;	// Size of header
			if (sampleLength != originalLength)
			{
				// Crunched sample, decrunch it
				sample.SampleData = DecrunchSample(moduleStream, originalLength, extraToAllocate, sampleLength, deltaCommand);
				if (sample.SampleData == null)
					return false;
			}
			else
			{
				// Just load the sample as it
				sample.SampleData = new sbyte[sampleLength + extraToAllocate];

				int readBytes = moduleStream.ReadSampleData(numberOfSamples, sample.SampleData, (int)sampleLength);
				if (readBytes != sampleLength)
					return false;
			}

			samples[++numberOfSamples] = sample;

			if (originalLength == 256)
				CreateWaves(sample);

			FixSamplePointers(sample);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Decrunch sample data
		/// </summary>
		/********************************************************************/
		private sbyte[] DecrunchSample(ModuleStream moduleStream, uint originalLength, int extraToAllocate, uint crunchedLength, byte deltaCommand)
		{
			byte[] deltaPackBuffer = new byte[crunchedLength];
			int deltaPackBufferLen = (int)crunchedLength;

			if (moduleStream.Read(deltaPackBuffer, 0, deltaPackBufferLen) != deltaPackBufferLen)
				return null;

			sbyte[] sampleData = new sbyte[originalLength + extraToAllocate];

			if (!DeltaDecruncher(deltaPackBuffer, deltaPackBufferLen, deltaCommand, sampleData))
				return null;

			return sampleData;
		}



		/********************************************************************/
		/// <summary>
		/// Delta decruncher
		/// </summary>
		/********************************************************************/
		private bool DeltaDecruncher(byte[] deltaPackBuffer, int deltaPackBufferLen, byte deltaCommand, sbyte[] sampleData)
		{
			int readOffset = 0;
			int writeOffset = 0;

			bool ReadByte(out byte data)
			{
				if (readOffset == deltaPackBufferLen)
				{
					data = 0;
					return false;
				}

				data = deltaPackBuffer[readOffset++];

				return true;
			}

			void WriteByte(sbyte byt)
			{
				sampleData[writeOffset++] = byt;
			}

			sbyte Extend(byte byt)
			{
				if ((byt & 0x08) != 0)
					byt |= 0xf0;

				return (sbyte)byt;
			}

			while (readOffset < deltaPackBufferLen)
			{
				if (!ReadByte(out byte data))
					return false;

				if (data == deltaCommand)
				{
					if (!ReadByte(out byte dataBegin))
						return false;

					if (!ReadByte(out byte dataLenHi))
						return false;

					if (!ReadByte(out byte dataLenLo))
						return false;

					int dataLength = (dataLenHi << 8) | dataLenLo;
					sbyte sampleByte = (sbyte)dataBegin;

					WriteByte(sampleByte);

					while (dataLength != 0)
					{
						if (!ReadByte(out data))
							return false;

						sampleByte += Extend((byte)(data >> 4));
						WriteByte(sampleByte);
						dataLength--;

						if (dataLength != 0)
						{
							sampleByte += Extend((byte)(data & 0x0f));
							WriteByte(sampleByte);
							dataLength--;
						}
					}
				}
				else
					WriteByte((sbyte)data);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Create 5 octave waves
		/// </summary>
		/********************************************************************/
		private void CreateWaves(Sample sample)
		{
			for (int i = 0; i < 256; i++)
			{
				if (sample.SampleData[i] == -128)
					sample.SampleData[i] = -127;
			}

			for (int i = 0, j = 0; i < 240; i++, j += 2)
				sample.SampleData[256 + i] = sample.SampleData[j];
		}



		/********************************************************************/
		/// <summary>
		/// Fix the sample pointers in all instruments
		/// </summary>
		/********************************************************************/
		private void FixSamplePointers(Sample sample)
		{
			sbyte[] sampleData = sample.SampleData;

			for (int i = 1; i < instruments.Length; i++)
			{
				Instrument inst = instruments[i];

				if (inst == zeroInstrument)
					break;

				if (inst.SampleNumber == numberOfSamples)
				{
					inst.SamplePointer = new SamplePointer(sampleData, inst.SampleStart * 2U);
					inst.SampleRepeatPointer = new SamplePointer(sampleData, inst.SampleRepeatStart * 2U);
				}
			}

			int repeatOffset = (int)(sample.SampleRepeatPointer!.Value.StartOffset - sample.SamplePointer!.Value.StartOffset);
			if (repeatOffset < 0)
				repeatOffset = 0;

			// Fix length
			if ((sample.SampleLength * 2) > sample.SampleData.Length)
				sample.SampleLength = (ushort)(sample.SampleData.Length / 2);

			if ((repeatOffset + (sample.SampleRepeatLength * 2)) > sample.SampleData.Length)
				sample.SampleRepeatLength = (ushort)((sample.SampleData.Length - repeatOffset) / 2);

			sample.SamplePointer = new SamplePointer(sampleData);
			sample.SampleRepeatPointer = new SamplePointer(sampleData, (uint)repeatOffset);
		}



		/********************************************************************/
		/// <summary>
		/// Load the INFO chunk
		/// </summary>
		/********************************************************************/
		private bool LoadInfoChunk(ModuleStream moduleStream, uint length, Encoding encoder)
		{
			byte[] buffer = new byte[length];

			moduleName = GetInfoString(moduleStream, buffer, encoder);
			if (moduleStream.EndOfStream)
				return false;

			author = GetInfoString(moduleStream, buffer, encoder);
			if (moduleStream.EndOfStream)
				return false;

			// Skip date and duration
			GetInfoString(moduleStream, buffer, encoder);
			if (moduleStream.EndOfStream)
				return false;

			GetInfoString(moduleStream, buffer, encoder);
			if (moduleStream.EndOfStream)
				return false;

			List<string> lines = new List<string>();

			for (int i = 0; i < 5; i++)
			{
				lines.Add(GetInfoString(moduleStream, buffer, encoder));

				if (moduleStream.EndOfStream)
					return false;
			}

			comments = lines.RemoveTrailingEmptyLines().ToArray();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Read a single string from the information chunk
		/// </summary>
		/********************************************************************/
		private string GetInfoString(ModuleStream moduleStream, byte[] buffer, Encoding encoder)
		{
			for (int i = 0; ; i++)
			{
				buffer[i] = moduleStream.Read_UINT8();

				if (buffer[i] == 0x00)
					break;
			}

			return encoder.GetString(buffer);
		}
	}
}
