/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Implementation
{
	/// <summary>
	/// Handle loading of the modules
	/// </summary>
	internal static class Loader
	{
		private class DataChunk
		{
			public int Size;
			public int Position;
		}

		// Pattern decoder state machine states
		private enum PatternDecoderState
		{
			TrackNum,
			BitField,
			Note,
			Instrument,
			Command1,
			Parameter1,
			Command2,
			Parameter2
		}

		// Packed pattern bitmask fields
		[Flags]
		private enum PatternBitField : uint8_t
		{
			HaveNote = 0x01,
			HaveInstrument = 0x02,
			HaveCommand1 = 0x04,
			HaveParameter1 = 0x08,
			HaveCommand2 = 0x10,
			HaveParameter2 = 0x20
		}

		private enum EnvelopeType
		{
			Volume,
			Panning
		}

		[Flags]
		private enum EnvelopeFlag
		{
			Enabled = 0x01,
			Sustain_A = 0x02,
			Loop = 0x04,
			Sustain_B = 0x08
		}

		/********************************************************************/
		/// <summary>
		/// Loads and parses a DigiBooster (DBM0 type) module from a
		/// stream
		/// </summary>
		/********************************************************************/
		public static DB3Module LoadFromStream(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			DB3Module m = new DB3Module();

			Error error = Read_Header(m, moduleStream);
			if (error == Error.None)
			{
				error = Read_Contents(m, moduleStream);
				if (error == Error.None)
				{
					if (!Verify_Contents(m))
						error = Error.Data_Corrupted;
				}
			}

			if (error != Error.None)
			{
				Unload(m);
				m = null;

				switch (error)
				{
					case Error.Data_Corrupted:
					{
						errorMessage = Resources.IDS_DBM_ERR_DATA_CORRUPTED;
						break;
					}

					case Error.Version_Unsupported:
					{
						errorMessage = Resources.IDS_DBM_ERR_VERSION_UNSUPPORTED;
						break;
					}

					case Error.Reading_Data:
					{
						errorMessage = Resources.IDS_DBM_ERR_READING_DATA;
						break;
					}

					case Error.Wrong_Chunk_Order:
					{
						errorMessage = Resources.IDS_DBM_ERR_WRONG_CHUNK_ORDER;
						break;
					}

					case Error.Sample_Size_Not_Supported:
					{
						errorMessage = Resources.IDS_DBM_ERR_SAMPLE_SIZE_NOT_SUPPORTED;
						break;
					}
				}
			}

			return m;
		}



		/********************************************************************/
		/// <summary>
		/// Unloads DigiBooster module from memory
		/// </summary>
		/********************************************************************/
		public static void Unload(DB3Module m)
		{
			if (m != null)
			{
				m.VolumeEnvelopes = null;
				m.PanningEnvelopes = null;

				m.Instruments = null;
				m.Samples = null;
				m.Songs = null;
				m.Patterns = null;
				m.DspDefaults = null;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int Bcd2Bin(uint8_t x)
		{
			return (x >> 4) * 10 + (x & 0x0f);
		}

		#region Verify methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Assign_Envelopes(DB3Module m)
		{
			for (int env = 0; env < m.NumberOfVolumeEnvelopes; env++)
			{
				int instr = m.VolumeEnvelopes[env].InstrumentNumber;

				if (instr != Constants.Envelope_Disabled)
				{
					if ((instr > 0) && (instr <= m.NumberOfInstruments))
						m.Instruments[instr - 1].VolumeEnvelope = (uint16_t)env;
					else
						return false;
				}
			}

			for (int env = 0; env < m.NumberOfPanningEnvelopes; env++)
			{
				int instr = m.PanningEnvelopes[env].InstrumentNumber;

				if (instr != Constants.Envelope_Disabled)
				{
					if ((instr > 0) && (instr <= m.NumberOfInstruments))
						m.Instruments[instr - 1].PanningEnvelope = (uint16_t)env;
					else
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_PlayLists(DB3Module m)
		{
			for (int song = 0; song < m.NumberOfSongs; song++)
			{
				DB3ModuleSong mso = m.Songs[song];

				for (int order = 0; order < mso.NumberOfOrders; order++)
				{
					if (mso.PlayList[order] >= m.NumberOfPatterns)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_Songs(DB3Module m)
		{
			for (int i = 0; i < m.NumberOfSongs; i++)
			{
				if (m.Songs[i] == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_Patterns(DB3Module m)
		{
			for (int i = 0; i < m.NumberOfPatterns; i++)
			{
				if (m.Patterns[i] == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_Sampled_Instruments(DB3Module m)
		{
			for (int i = 0; i < m.NumberOfInstruments; i++)
			{
				if (m.Instruments[i] == null)
					return false;
			}

			for (int i = 0; i < m.NumberOfInstruments; i++)
			{
				DB3ModuleInstrument mi = m.Instruments[i];

				if (mi.Type == InstrumentType.Sample)
				{
					DB3ModuleSampleInstrument mis = (DB3ModuleSampleInstrument)mi;

					if (mis.SampleNumber >= m.NumberOfSamples)
						mis.SampleNumber = 0;

					DB3ModuleSample msmp = m.Samples[mis.SampleNumber];
					if ((msmp != null) && (msmp.Frames > 0))
					{
						if ((mis.Flags & InstrumentFlag.Loop_Mask) != 0)
						{
							// Loop verification. Negative loop start and loop length are rejected.
							// Loop start outside the sample disable loop. For improved error tolerance
							// if calculated loop end is beyond the sample end, the loop length is clipped
							// and then accepted. It is ensured then that after clipping the loop will have
							// at least one frame
							if ((mis.LoopStart < 0) || (mis.LoopLength < 0))
								return false;

							if (mis.LoopStart >= msmp.Frames)
							{
								mis.LoopStart = 0;
								mis.LoopLength = 0;

								mis.Flags &= ~InstrumentFlag.Loop_Mask;
							}

							if ((mis.LoopStart + mis.LoopLength) > msmp.Frames)
								mis.LoopLength = msmp.Frames - mis.LoopStart;
						}
					}
					else
					{
						// Clear the loop for instrument having no frames
						mis.LoopStart = 0;
						mis.LoopLength = 0;
						mis.Flags &= ~InstrumentFlag.Loop_Mask;
					}

					if ((mis.C3Frequency < 1000) || (mis.C3Frequency > 192000))
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Verify_Contents(DB3Module m)
		{
			if (Verify_Sampled_Instruments(m))
			{
				if (Verify_Patterns(m))
				{
					if (Verify_Songs(m))
					{
						if (Verify_PlayLists(m))
						{
							if (Assign_Envelopes(m))
								return true;
						}
					}
				}
			}

			return false;
		}
		#endregion

		#region Read methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Data(DataChunk dc, ModuleStream moduleStream, byte[] buf, int length)
		{
			Error error = Error.None;

			if (length <= (dc.Size - dc.Position))
			{
				int k = moduleStream.Read(buf, 0, length);
				if (k == length)
					dc.Position += length;
				else
					error = Error.Reading_Data;
			}
			else
				error = Error.Data_Corrupted;

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Dspe(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			uint8_t[] b = new uint8_t[8];

			Error error = Read_Data(dc, moduleStream, b, 2);
			if (error == Error.None)
			{
				int trackMaskLength = (b[0] << 8) | b[1];

				if (trackMaskLength == m.NumberOfTracks)
				{
					uint8_t[] trackMask = new uint8_t[trackMaskLength];

					error = Read_Data(dc, moduleStream, trackMask, trackMaskLength);
					if (error == Error.None)
					{
						error = Read_Data(dc, moduleStream, b, 8);
						if (error == Error.None)
						{
							for (int i = 0; i < trackMaskLength; i++)
							{
								if (trackMask[i] == 0)
									m.DspDefaults.EffectMask[i] |= Constants.Dsp_Mask_Echo;
							}

							m.DspDefaults.EchoDelay = b[1];
							m.DspDefaults.EchoFeedback = b[3];
							m.DspDefaults.EchoMix = b[5];
							m.DspDefaults.EchoCross = b[7];
						}
					}
				}
				else
					error = Error.Data_Corrupted;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Envelope(DataChunk dc, DB3ModuleEnvelope mEnv, ModuleStream moduleStream, EnvelopeType type, int creator)
		{
			uint8_t[] b = new uint8_t[136];

			Error error = Read_Data(dc, moduleStream, b, 136);
			if (error == Error.None)
			{
				EnvelopeFlag flags = (EnvelopeFlag)b[2];

				ushort sections = b[3];
				if (sections > 31)
					sections = 31;

				mEnv.NumberOfSections = sections;
				mEnv.SustainA = Constants.Envelope_Sustain_Disabled;
				mEnv.SustainB = Constants.Envelope_Sustain_Disabled;
				mEnv.LoopFirst = Constants.Envelope_Loop_Disabled;
				mEnv.LoopLast = Constants.Envelope_Loop_Disabled;

				ushort instrument = (ushort)((b[0] << 8) | b[1]);
				if ((instrument <= 0) || (instrument > 255))
					mEnv.InstrumentNumber = Constants.Envelope_Disabled;
				else
					mEnv.InstrumentNumber = instrument;

				if ((flags & EnvelopeFlag.Enabled) != 0)
				{
					if ((flags & EnvelopeFlag.Sustain_B) != 0)
					{
						if (b[7] <= mEnv.NumberOfSections)
							mEnv.SustainB = b[7];
						else
							error = Error.Data_Corrupted;
					}

					if ((flags & EnvelopeFlag.Sustain_A) != 0)
					{
						if (b[4] <= mEnv.NumberOfSections)
							mEnv.SustainA = b[4];
						else
							error = Error.Data_Corrupted;
					}

					if ((flags & EnvelopeFlag.Loop) != 0)
					{
						if ((b[6] <= mEnv.NumberOfSections) && (b[5] <= b[6]))
						{
							mEnv.LoopFirst = b[5];
							mEnv.LoopLast = b[6];
						}
						else
							error = Error.Data_Corrupted;
					}

					if ((mEnv.SustainA != Constants.Envelope_Sustain_Disabled) && (mEnv.SustainB != Constants.Envelope_Sustain_Disabled))
					{
						if (mEnv.SustainA > mEnv.SustainB)
							(mEnv.SustainA, mEnv.SustainB) = (mEnv.SustainB, mEnv.SustainA);
					}
				}

				int pp = 8;

				for (int point = 0; point < sections + 1; point++)
				{
					int16_t pos = (int16_t)((b[pp] << 8) | b[pp + 1]);
					int16_t val = (int16_t)((b[pp + 2] << 8) | b[pp + 3]);
					pp += 4;

					if (type == EnvelopeType.Volume)
					{
						if ((flags & EnvelopeFlag.Enabled) != 0)
						{
							if ((pos < 0) || (pos > 2048))
								error = Error.Data_Corrupted;

							if ((val < 0) || (val > 64))
								error = Error.Data_Corrupted;
						}
					}
					else
					{
						if ((flags & EnvelopeFlag.Enabled) != 0)
						{
							if (creator == Constants.Creator_DigiBooster_2)
								val = (int16_t)((val << 2) - 128);

							if ((pos < 0) || (pos > 2048))
								error = Error.Data_Corrupted;

							if ((val < -128) || (val > 128))
								error = Error.Data_Corrupted;
						}
						else
						{
							pos = 0;
							val = 0;
						}
					}

					if (error == Error.None)
					{
						mEnv.Points[point] = new DB3ModuleEnvelopePoint
						{
							Position = (uint16_t)pos,
							Value = val
						};
					}
					else
						break;
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Venv(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			uint8_t[] b = new uint8_t[2];

			Error error = Read_Data(dc, moduleStream, b, 2);
			if (error == Error.None)
			{
				m.NumberOfVolumeEnvelopes = (ushort)((b[0] << 8) | b[1]);

				if (m.NumberOfVolumeEnvelopes > 255)
					error = Error.Data_Corrupted;
				else if (m.NumberOfVolumeEnvelopes > 0)
				{
					m.VolumeEnvelopes = new DB3ModuleEnvelope[m.NumberOfVolumeEnvelopes];

					for (int i = 0; i < m.NumberOfVolumeEnvelopes; i++)
					{
						m.VolumeEnvelopes[i] = new DB3ModuleEnvelope();

						error = Read_Envelope(dc, m.VolumeEnvelopes[i], moduleStream, EnvelopeType.Volume, m.CreatorVersion);
						if (error != Error.None)
							break;
					}
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Penv(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			uint8_t[] b = new uint8_t[2];

			Error error = Read_Data(dc, moduleStream, b, 2);
			if (error == Error.None)
			{
				m.NumberOfPanningEnvelopes = (ushort)((b[0] << 8) | b[1]);

				if (m.NumberOfPanningEnvelopes > 255)
					error = Error.Data_Corrupted;
				else if (m.NumberOfPanningEnvelopes > 0)
				{
					m.PanningEnvelopes = new DB3ModuleEnvelope[m.NumberOfPanningEnvelopes];

					for (int i = 0; i < m.NumberOfPanningEnvelopes; i++)
					{
						m.PanningEnvelopes[i] = new DB3ModuleEnvelope();

						error = Read_Envelope(dc, m.PanningEnvelopes[i], moduleStream, EnvelopeType.Panning, m.CreatorVersion);
						if (error != Error.None)
							break;
					}
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Pattern(DataChunk dc, DB3ModulePattern mp, ModuleStream moduleStream, int tracks)
		{
			uint8_t[] b = new uint8_t[6];

			Error error = Read_Data(dc, moduleStream, b, 6);
			if (error == Error.None)
			{
				int rows = (b[0] << 8) | b[1];
				int packSize = (b[2] << 24) | (b[3] << 16) | (b[4] << 8) | b[5];

				if (rows == 0)
					return Error.Data_Corrupted;

				if (packSize <= 0)
					return Error.Data_Corrupted;

				mp.NumberOfRows = (ushort)rows;
				mp.Pattern = Helpers.InitializeArray<DB3ModuleEntry>(rows * tracks);

				uint8_t[] packedData = new uint8_t[packSize];
				error = Read_Data(dc, moduleStream, packedData, packSize);
				if (error == Error.None)
				{
					PatternBitField bitField = 0;
					int packedOffset = 0;
					PatternDecoderState state = PatternDecoderState.TrackNum;
					int row = 0;
					int packCounter = packSize;
					DB3ModuleEntry me = null;

					while ((error == Error.None) && (packCounter-- != 0) && (row < mp.NumberOfRows))	// Main decoder loop
					{
						uint8_t @byte = packedData[packedOffset++];

						switch (state)
						{
							case PatternDecoderState.TrackNum:
							{
								if (@byte == 0)
									row++;
								else
								{
									if (@byte <= tracks)
										me = mp.Pattern[row * tracks + @byte - 1];
									else
									{
										error = Error.Data_Corrupted;
										me = null;
									}

									state = PatternDecoderState.BitField;
								}
								break;
							}

							case PatternDecoderState.BitField:
							{
								bitField = (PatternBitField)@byte;

								if ((bitField & PatternBitField.HaveNote) != 0)
									state = PatternDecoderState.Note;
								else if ((bitField & PatternBitField.HaveInstrument) != 0)
									state = PatternDecoderState.Instrument;
								else if ((bitField & PatternBitField.HaveCommand1) != 0)
									state = PatternDecoderState.Command1;
								else if ((bitField & PatternBitField.HaveParameter1) != 0)
									state = PatternDecoderState.Parameter1;
								else if ((bitField & PatternBitField.HaveCommand2) != 0)
									state = PatternDecoderState.Command2;
								else if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Note:
							{
								if (me != null)
								{
									me.Octave = (uint8_t)(@byte >> 4);
									me.Note = (uint8_t)(@byte & 0x0f);
								}

								if ((bitField & PatternBitField.HaveInstrument) != 0)
									state = PatternDecoderState.Instrument;
								else if ((bitField & PatternBitField.HaveCommand1) != 0)
									state = PatternDecoderState.Command1;
								else if ((bitField & PatternBitField.HaveParameter1) != 0)
									state = PatternDecoderState.Parameter1;
								else if ((bitField & PatternBitField.HaveCommand2) != 0)
									state = PatternDecoderState.Command2;
								else if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Instrument:
							{
								if (me != null)
									me.Instrument = @byte;

								if ((bitField & PatternBitField.HaveCommand1) != 0)
									state = PatternDecoderState.Command1;
								else if ((bitField & PatternBitField.HaveParameter1) != 0)
									state = PatternDecoderState.Parameter1;
								else if ((bitField & PatternBitField.HaveCommand2) != 0)
									state = PatternDecoderState.Command2;
								else if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Command1:
							{
								if (me != null)
									me.Command1 = (Effect)@byte;

								if ((bitField & PatternBitField.HaveParameter1) != 0)
									state = PatternDecoderState.Parameter1;
								else if ((bitField & PatternBitField.HaveCommand2) != 0)
									state = PatternDecoderState.Command2;
								else if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Parameter1:
							{
								if (me != null)
									me.Parameter1 = @byte;

								if ((bitField & PatternBitField.HaveCommand2) != 0)
									state = PatternDecoderState.Command2;
								else if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Command2:
							{
								if (me != null)
									me.Command2 = (Effect)@byte;

								if ((bitField & PatternBitField.HaveParameter2) != 0)
									state = PatternDecoderState.Parameter2;
								else
									state = PatternDecoderState.TrackNum;

								break;
							}

							case PatternDecoderState.Parameter2:
							{
								if (me != null)
									me.Parameter2 = @byte;

								state = PatternDecoderState.TrackNum;
								break;
							}
						}
					}
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Patt(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			Error error = Error.None;

			for (int i = 0; i < m.NumberOfPatterns; i++)
			{
				DB3ModulePattern mp = new DB3ModulePattern();

				error = Read_Pattern(dc, mp, moduleStream, m.NumberOfTracks);
				if (error == Error.None)
					m.Patterns[i] = mp;
				else
					break;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Sample_Data_8Bit(int sampleNumber, DataChunk dc, DB3ModuleSample ms, ModuleStream moduleStream)
		{
			ms.Data8 = moduleStream.ReadSampleData(sampleNumber, ms.Frames, out int readBytes);
			if (readBytes < (ms.Frames - 512))
				return Error.Reading_Data;

			dc.Position += ms.Frames;

			return Error.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Sample_Data_16Bit(int sampleNumber, DataChunk dc, DB3ModuleSample ms, ModuleStream moduleStream)
		{
			ms.Data16 = moduleStream.Read_B_16BitSampleData(sampleNumber, ms.Frames, out int readSamples);
			if (readSamples < (ms.Frames - 256))
				return Error.Reading_Data;

			dc.Position += ms.Frames << 1;

			return Error.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Sample_Data_32Bit(int sampleNumber, DataChunk dc, DB3ModuleSample ms, ModuleStream moduleStream)
		{
			return Error.Sample_Size_Not_Supported;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Sample(int sampleNumber, DataChunk dc, DB3ModuleSample ms, ModuleStream moduleStream)
		{
			uint8_t[] b = new uint8_t[8];

			Error error = Read_Data(dc, moduleStream, b, 8);
			if (error == Error.None)
			{
				ms.Frames = (b[4] << 24) | (b[5] << 16) | (b[6] << 8) | b[7];

				// Negative length means corrupted module, limit sample to 2 GB
				if ((ms.Frames >= 0) && (ms.Frames < 0x40000000))
				{
					if (ms.Frames > 0)			// There may be samples of 0 length
					{
						switch (b[3] & 0x07)	// Bytes per sample
						{
							case 1:
							{
								error = Read_Sample_Data_8Bit(sampleNumber, dc, ms, moduleStream);
								break;
							}

							case 2:
							{
								error = Read_Sample_Data_16Bit(sampleNumber, dc, ms, moduleStream);
								break;
							}

							case 4:
							{
								error = Read_Sample_Data_32Bit(sampleNumber, dc, ms, moduleStream);
								break;
							}
						}
					}
				}
				else
					error = Error.Data_Corrupted;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Smpl(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			Error error = Error.None;

			for (int i = 0; i < m.NumberOfSamples; i++)
			{
				DB3ModuleSample ms = new DB3ModuleSample();

				error = Read_Sample(i, dc, ms, moduleStream);
				if (error == Error.None)
					m.Samples[i] = ms;
				else
					break;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Instrument(DataChunk dc, DB3ModuleSampleInstrument mi, ModuleStream moduleStream, int creator)
		{
			Encoding encoder = creator == Constants.Creator_DigiBooster_2 ? EncoderCollection.Amiga : EncoderCollection.Iso8859_1;
			uint8_t[] b = new uint8_t[50];

			Error error = Read_Data(dc, moduleStream, b, 50);
			if (error == Error.None)
			{
				int nameLen = b[29] != 0x00 ? 30 : encoder.GetCharCount(b);

				mi.Name = encoder.GetString(b, 0, nameLen);
				mi.SampleNumber = (uint16_t)(((b[30] << 8) | b[31]) - 1);		// Samples from 1 in the mod, from 0 in the app
				mi.Volume = (uint16_t)((b[32] << 8) | b[33]);
				mi.Type = InstrumentType.Sample;
				mi.VolumeEnvelope = Constants.Envelope_Disabled;
				mi.PanningEnvelope = Constants.Envelope_Disabled;
				mi.C3Frequency = (uint32_t)((b[34] << 24) | (b[35] << 16) | (b[36] << 8) | b[37]);
				mi.LoopStart = (b[38] << 24) | (b[39] << 16) | (b[40] << 8) | b[41];
				mi.LoopLength = (b[42] << 24) | (b[43] << 16) | (b[44] << 8) | b[45];
				mi.Panning = (int16_t)((b[46] << 8) | b[47]);
				mi.Flags = (InstrumentFlag)((b[48] << 8) | b[49]);

				if (mi.LoopLength == 0)
					mi.Flags &= ~InstrumentFlag.Loop_Mask;			// Set loop type to none for 0 length

				if ((mi.Flags & InstrumentFlag.Loop_Mask) == InstrumentFlag.No_Loop)
				{
					mi.LoopLength = 0;
					mi.LoopStart = 0;
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Inst(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			Error error = Error.None;

			for (int i = 0; i < m.NumberOfInstruments; i++)
			{
				DB3ModuleSampleInstrument mi = new DB3ModuleSampleInstrument();

				error = Read_Instrument(dc, mi, moduleStream, m.CreatorVersion);
				if (error == Error.None)
					m.Instruments[i] = mi;
				else
					break;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Song(DataChunk dc, DB3ModuleSong ms, ModuleStream moduleStream, int creator)
		{
			Encoding encoder = creator == Constants.Creator_DigiBooster_2 ? EncoderCollection.Amiga : EncoderCollection.Iso8859_1;
			uint8_t[] b = new uint8_t[46];

			Error error = Read_Data(dc, moduleStream, b, 46);
			if (error == Error.None)
			{
				int nameLen = b[43] != 0x00 ? 44 : encoder.GetCharCount(b);

				ms.Name = encoder.GetString(b, 0, nameLen);
				ms.NumberOfOrders = (uint16_t)((b[44] << 8) | b[45]);

				ms.PlayList = new uint16_t[ms.NumberOfOrders];
				byte[] tempList = new byte[ms.NumberOfOrders * sizeof(uint16_t)];

				error = Read_Data(dc, moduleStream, tempList, tempList.Length);
				if (error == Error.None)
				{
					for (int i = 0; i < ms.NumberOfOrders; i++)
						ms.PlayList[i] = (uint16_t)((tempList[i * 2] << 8) | tempList[i * 2 + 1]);
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Song(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			Error error = Error.None;

			for (int songNum = 0; (error == Error.None) && (songNum < m.NumberOfSongs); songNum++)
			{
				DB3ModuleSong ms = new DB3ModuleSong();

				error = Read_Song(dc, ms, moduleStream, m.CreatorVersion);
				m.Songs[songNum] = ms;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Info(DB3Module m, DataChunk dc, ModuleStream moduleStream)
		{
			uint8_t[] b = new uint8_t[10];

			Error error = Read_Data(dc, moduleStream, b, 10);
			if (error == Error.None)
			{
				m.NumberOfInstruments = (uint16_t)((b[0] << 8) | b[1]);
				m.NumberOfSamples = (uint16_t)((b[2] << 8) | b[3]);
				m.NumberOfSongs = (uint16_t)((b[4] << 8) | b[5]);
				m.NumberOfPatterns = (uint16_t)((b[6] << 8) | b[7]);
				m.NumberOfTracks = (uint16_t)((b[8] << 8) | b[9]);

				if ((m.NumberOfInstruments == 0) || (m.NumberOfInstruments > 255))
					error = Error.Data_Corrupted;

				if ((m.NumberOfSamples == 0) || (m.NumberOfSamples > 255))
					error = Error.Data_Corrupted;

				if ((m.NumberOfTracks == 0) || (m.NumberOfTracks > 254) || ((m.NumberOfTracks & 1) != 0))
					error = Error.Data_Corrupted;

				if ((m.NumberOfSongs == 0) || (m.NumberOfSongs > 255))
					error = Error.Data_Corrupted;

				if (m.NumberOfPatterns == 0)
					error = Error.Data_Corrupted;

				if (error == Error.None)
				{
					m.Instruments = new DB3ModuleInstrument[m.NumberOfInstruments];
					m.Samples = new DB3ModuleSample[m.NumberOfSamples + 1];
					m.Songs = new DB3ModuleSong[m.NumberOfSongs];
					m.Patterns = new DB3ModulePattern[m.NumberOfPatterns];

					m.DspDefaults = new DB3GlobalDsp
					{
						EffectMask = new uint32_t[m.NumberOfTracks],
						EchoDelay = 0x40,
						EchoFeedback = 0x80,
						EchoMix = 0x80,
						EchoCross = 0xff
					};
				}
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Chunk_Name(DB3Module m, DataChunk dc, ModuleStream moduleStream, int creator)
		{
			Encoding encoder = creator == Constants.Creator_DigiBooster_2 ? EncoderCollection.Amiga : EncoderCollection.Iso8859_1;
			byte[] name = new byte[48];

			Error error = Read_Data(dc, moduleStream, name, 44);
			if (error == Error.None)
				m.Name = encoder.GetString(name);

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Skip_To_Chunk_End(ModuleStream moduleStream, DataChunk dc)
		{
			byte[] buf = new byte[512];
			Error error = Error.None;

			while ((error == Error.None) && (dc.Position < dc.Size))
			{
				int block = dc.Size - dc.Position;
				if (block > 512)
					block = 512;

				if (moduleStream.Read(buf, 0, block) != block)
					error = Error.Reading_Data;

				dc.Position += block;
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Contents(DB3Module m, ModuleStream moduleStream)
		{
			bool haveInfo = false;
			int neededChunkCount = 0;
			Error error = Error.None;
			DataChunk dc = new DataChunk();

			while ((error == Error.None) && !moduleStream.EndOfStream && (neededChunkCount < 5))
			{
				uint name = moduleStream.Read_B_UINT32();
				dc.Size = moduleStream.Read_B_INT32();
				dc.Position = 0;

				switch (name)
				{
					// NAME
					case 0x4e414d45:
					{
						error = Read_Chunk_Name(m, dc, moduleStream, m.CreatorVersion);
						break;
					}

					// INFO
					case 0x494e464f:
					{
						error = Read_Chunk_Info(m, dc, moduleStream);
						if (error == Error.None)
							haveInfo = true;

						neededChunkCount++;
						break;
					}

					// SONG
					case 0x534f4e47:
					{
						if (haveInfo)
							error = Read_Chunk_Song(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						neededChunkCount++;
						break;
					}

					// INST
					case 0x494e5354:
					{
						if (haveInfo)
							error = Read_Chunk_Inst(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						neededChunkCount++;
						break;
					}

					// PATT
					case 0x50415454:
					{
						if (haveInfo)
							error = Read_Chunk_Patt(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						neededChunkCount++;
						break;
					}

					// SMPL
					case 0x534d504c:
					{
						if (haveInfo)
							error = Read_Chunk_Smpl(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						neededChunkCount++;
						break;
					}

					// VENV
					case 0x56454e56:
					{
						if (haveInfo)
							error = Read_Chunk_Venv(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						break;
					}

					// PENV
					case 0x50454e56:
					{
						if (haveInfo)
							error = Read_Chunk_Penv(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						break;
					}

					// DSPE
					case 0x44535045:
					{
						if (haveInfo)
							error = Read_Chunk_Dspe(m, dc, moduleStream);
						else
							error = Error.Wrong_Chunk_Order;

						break;
					}
				}

				if (error == Error.None)
					error = Skip_To_Chunk_End(moduleStream, dc);
			}

			return error;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Error Read_Header(DB3Module m, ModuleStream moduleStream)
		{
			Error error = Error.None;

			if (moduleStream.Read_B_UINT32() == 0x44424d30)		// DBM0
			{
				byte version = moduleStream.Read_UINT8();

				if (version == 2)
					m.CreatorVersion = Constants.Creator_DigiBooster_2;
				else if (version == 3)
					m.CreatorVersion = Constants.Creator_DigiBooster_3;
				else
					error = Error.Version_Unsupported;

				m.CreatorRevision = (uint16_t)Bcd2Bin(moduleStream.Read_UINT8());

				if (moduleStream.EndOfStream)
					error = Error.Reading_Data;

				moduleStream.Seek(2, SeekOrigin.Current);
			}
			else
				error = Error.Data_Corrupted;

			return error;
		}
		#endregion

		#endregion
	}
}
