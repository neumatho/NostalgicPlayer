/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Handle synthesis instruments
	/// </summary>
	internal class SynthesisFormat : IInstrumentFormat
	{
		private SynthesisData formatData;

		/********************************************************************/
		/// <summary>
		/// Try to identify the format
		/// </summary>
		/********************************************************************/
		public static bool Identify(byte[] firstBytes)
		{
			if (firstBytes[0] == 0x00)
				return true;

			const string Id = "Synthesis";

			string checkId = Encoding.Latin1.GetString(firstBytes, 0, Id.Length);
			return checkId.Equals(Id, StringComparison.OrdinalIgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Load the instrument
		/// </summary>
		/********************************************************************/
		public bool Load(ModuleStream instrumentStream, PlayerFileInfo fileInfo, string instrumentPath, string instrumentFileName, List<Instrument> instruments, out string errorMessage)
		{
			errorMessage = string.Empty;

			formatData = new SynthesisData();

			instrumentStream.Seek(0x44, SeekOrigin.Begin);

			int bytesRead = instrumentStream.ReadSigned(formatData.Oscillator, 0, 128);
			if (bytesRead != 128)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_INSTRUMENTS;
				return false;
			}

			bytesRead = instrumentStream.ReadSigned(formatData.Lfo, 0, 256);
			if (bytesRead != 256)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_INSTRUMENTS;
				return false;
			}

			formatData.Waveform = (WaveformType)instrumentStream.Read_B_UINT16();

			instrumentStream.Seek(4, SeekOrigin.Current);

			formatData.WaveAmt = instrumentStream.Read_B_UINT16();
			formatData.AmplitudeVolume = instrumentStream.Read_B_UINT16();
			formatData.AmplitudeEnabled = instrumentStream.Read_B_UINT16();
			formatData.AmplitudeLfo = instrumentStream.Read_B_UINT16();
			formatData.FrequencyPort = instrumentStream.Read_B_UINT16();
			formatData.FrequencyLfo = instrumentStream.Read_B_UINT16();
			formatData.FilterFrequency = instrumentStream.Read_B_UINT16();
			formatData.FilterEg = instrumentStream.Read_B_UINT16();
			formatData.FilterLfo = instrumentStream.Read_B_UINT16();
			formatData.LfoSpeed = instrumentStream.Read_B_UINT16();
			formatData.LfoEnabled = (LfoStatus)instrumentStream.Read_B_UINT16();
			formatData.LfoDelay = instrumentStream.Read_B_UINT16();
			formatData.PhaseSpeed = instrumentStream.Read_B_UINT16();
			formatData.PhaseDepth = instrumentStream.Read_B_UINT16();
			instrumentStream.ReadArray_B_UINT16s(formatData.EnvelopeLevels, 0, 4);
			instrumentStream.ReadArray_B_UINT16s(formatData.EnvelopeRates, 0, 4);

			if (instrumentStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_INSTRUMENTS;
				return false;
			}

			foreach (Instrument instr in instruments)
			{
				if ((instr.Format is SynthesisFormat synthesisFormat) && (ArrayHelper.ArrayCompare(synthesisFormat.formatData.Oscillator, 0, formatData.Oscillator, 0, 128)))
				{
					// Reuse format data
					formatData.Samples = synthesisFormat.formatData.Samples;
					return true;
				}
			}

			CalculateSamples();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the instrument
		/// </summary>
		/********************************************************************/
		public void Setup(GlobalInfo globalInfo, GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber)
		{
			SynthesisPlayInfo playInfo = playingInfo.SynthesisPlayInfo[channelNumber];

			switch (voice.InstrumentSetupSequence)
			{
				case InstrumentSetup.Initialize:
				{
					if ((voice.Note < 36) || (voice.Note >= 108))
					{
						voice.InstrumentSetupSequence = InstrumentSetup.Nothing;

						if (voice.Status == VoiceStatus.Silence)
							return;

						break;
					}

					int note = voice.Note - 36;

					if (voice.Status == VoiceStatus.Silence)
						playInfo.EnvelopeVolume = 0;

					if (voice.Status != VoiceStatus.Playing)
						playInfo.EnvelopeIndex = 0;

					int octave = note / 12;
					note %= 12;

					ushort mappedNote = (ushort)((Tables.Note[note] * 54728) >> (octave + 17));
					playInfo.Octave = (byte)(octave + 3);
					playInfo.Note = (byte)note;

					if (playInfo.MappedNote == 0)
						playInfo.FrequencyCounter = 0;
					else
					{
						int difference = mappedNote - playInfo.MappedNote;
						playInfo.FrequencyCounter = (short)((((formatData.FrequencyPort << 15) / playingInfo.CalculatedTempo) >> 3) + 1);
						playInfo.FrequencySpeed = (short)(difference / playInfo.FrequencyCounter);

						mappedNote -= (ushort)(playInfo.FrequencySpeed * playInfo.FrequencyCounter);
					}

					playInfo.MappedNote = mappedNote;
					playInfo.PhaseDirection = 1;

					if (formatData.PhaseSpeed == 0)
						playInfo.PhaseIndex = 0;

					playInfo.LfoCounter = 0;

					if (formatData.LfoEnabled != LfoStatus.Off)
					{
						playInfo.LfoIndex = 0;
						playInfo.LfoCounter = (short)(((formatData.LfoDelay << 15) / playingInfo.CalculatedTempo) >> 2);
						playInfo.LfoValue = formatData.Lfo[0];
					}

					voice.SetSampleSequence = SetSample.StartSample;
					break;
				}

				case InstrumentSetup.ReleaseNote:
				{
					playInfo.EnvelopeIndex = 3;
					break;
				}

				case InstrumentSetup.Mute:
				{
					channel.Mute();
					return;
				}
			}

			if (playInfo.LfoCounter > 0)
				playInfo.LfoCounter--;
			else if (playInfo.LfoCounter == 0)
			{
				short index = (short)(playInfo.LfoIndex + ((formatData.LfoSpeed * playingInfo.CalculatedTempo) >> 10));

				if ((index < 0) && (formatData.LfoEnabled == LfoStatus.Once))
					playInfo.LfoCounter = -1;
				else
				{
					playInfo.LfoIndex = (ushort)index;
					playInfo.LfoValue = formatData.Lfo[playInfo.LfoIndex >> 8];
				}
			}

			int t1 = (formatData.EnvelopeLevels[playInfo.EnvelopeIndex] << 16);
			int t2 = playInfo.EnvelopeVolume;
			int t3 = formatData.EnvelopeRates[playInfo.EnvelopeIndex];
			int t0 = (t3 >> 5) ^ 7;
			t3 = ((((t3 & 0x1f) + 0x21) * playingInfo.CalculatedTempo) << 3) >> t0;

			t0 = t1 - t2;
			if (t0 < 0)
				t0 = -t0;

			if (t0 > t3)
			{
				if (t2 < t1)
					t2 += t3;
				else
					t2 -= t3;
			}
			else
			{
				t2 = t1;

				if (playInfo.EnvelopeIndex < 2)
					playInfo.EnvelopeIndex++;
			}

			playInfo.EnvelopeVolume = t2;

			ushort oct = 5;

			if (playInfo.FrequencyCounter != 0)
			{
				playInfo.FrequencyCounter--;
				playInfo.MappedNote = (ushort)(playInfo.MappedNote + playInfo.FrequencySpeed);
			}

			ushort tempNote = playInfo.MappedNote;

			while (tempNote > 428)
			{
				tempNote /= 2;
				oct--;
			}

			playInfo.PlayingOctave = oct;

			voice.SampleLengthInWords = (ushort)(64 >> oct);
			voice.Period = (ushort)(((((formatData.FrequencyLfo * playInfo.LfoValue) >> 7) - (globalInfo.Tune - 0x80) + 0x1000) * tempNote) >> 12);

			ushort vol = (ushort)(((formatData.AmplitudeLfo * -playInfo.LfoValue) >> 8) + formatData.AmplitudeVolume);

			if (formatData.AmplitudeEnabled != 0)
				vol = (ushort)(((playInfo.EnvelopeVolume >> 16) * vol) >> 8);
			else if (playInfo.EnvelopeIndex == 3)
				vol = 0;

			voice.FinalVolume = (ushort)(((((((((vol & 0xff) + 1) * playingInfo.CurrentVolume) / 256) + 1) * voice.Volume) / 256) + 1) / 4);

			int sampleIndex = (((formatData.FilterFrequency ^ 0xff) - (((playInfo.EnvelopeVolume >> 16) * formatData.FilterEg) >> 8)) + ((playInfo.LfoValue * formatData.FilterLfo) >> 8) & 0xff) >> 2;
			sbyte[] sourceSample = formatData.Samples[sampleIndex];

			playInfo.SampleStartIndex ^= 0x80;
			uint destIndex = playInfo.SampleStartIndex;

			if (voice.CalculatedSynthesisSample == null)
				voice.CalculatedSynthesisSample = new sbyte[256];

			voice.SampleData = voice.CalculatedSynthesisSample;
			voice.SampleStartOffset = destIndex;

			if (formatData.PhaseSpeed == 0)
			{
				int sourceIndex = 0;
				int sourceStep = 1 << playInfo.PlayingOctave;

				int length = 128 >> playInfo.PlayingOctave;

				for (int i = 0; i < length; i++)
				{
					voice.CalculatedSynthesisSample[destIndex++] = sourceSample[sourceIndex];
					sourceIndex += sourceStep;
				}
			}
			else if (formatData.PhaseDepth == 0)
			{
				int source1Index = 0;
				int sourceStep = 1 << playInfo.PlayingOctave;

				playInfo.PhaseIndex += (short)((formatData.PhaseSpeed * playingInfo.CalculatedTempo) >> 13);
				int source2Index = (ushort)playInfo.PhaseIndex >> 9;

				int length2 = source2Index >> playInfo.PlayingOctave;
				int length1 = (voice.SampleLengthInWords * 2) - length2;

				for (int i = 0; i < length1; i++)
				{
					voice.CalculatedSynthesisSample[destIndex++] = (sbyte)((sourceSample[source1Index] + sourceSample[source2Index]) / 2);
					source1Index += sourceStep;
					source2Index += sourceStep;
				}

				if (length2 > 0)
				{
					source2Index -= 128;

					for (int i = 0; i < length2; i++)
					{
						voice.CalculatedSynthesisSample[destIndex++] = (sbyte)((sourceSample[source1Index] + sourceSample[source2Index]) / 2);
						source1Index += sourceStep;
						source2Index += sourceStep;
					}
				}
			}
			else
			{
				short index = (short)((((formatData.PhaseSpeed * playingInfo.CalculatedTempo) >> 11) * playInfo.PhaseDirection) + playInfo.PhaseIndex);
				if (index < 0)
				{
					if (index == -32768)
						index += playInfo.PhaseDirection;

					playInfo.PhaseDirection = (short)-playInfo.PhaseDirection;
					index = (short)-index;
				}

				playInfo.PhaseIndex = index;

				int d =  (formatData.PhaseDepth * playInfo.PhaseIndex) >> (17 + playInfo.PlayingOctave);

				int length1 = voice.SampleLengthInWords + d;
				int length2 = voice.SampleLengthInWords - d;

				if (length1 != 0)
				{
					int increment = 64 / length1;
					int incrementRemainder = 64 % length1;
					int sourceIndex = 0;
					int t = 0;

					for (int i = 0; i < length1; i++)
					{
						voice.CalculatedSynthesisSample[destIndex++] = sourceSample[sourceIndex];

						t -= incrementRemainder;
						if (t < 0)
						{
							t += length1;
							sourceIndex++;
						}

						sourceIndex += increment;
					}
				}

				if (length2 != 0)
				{
					int increment = 64 / length2;
					int incrementRemainder = 64 % length2;
					int sourceIndex = 64;
					int t = 0;

					for (int i = 0; i < length2; i++)
					{
						voice.CalculatedSynthesisSample[destIndex++] = sourceSample[sourceIndex];

						t -= incrementRemainder;
						if (t < 0)
						{
							t += length1;
							sourceIndex++;
						}

						sourceIndex += increment;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Play the initialized instrument
		/// </summary>
		/********************************************************************/
		public void Play(GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber)
		{
			SynthesisPlayInfo playInfo = playingInfo.SynthesisPlayInfo[channelNumber];

			if (voice.SetSampleSequence == SetSample.StartSample)
			{
				channel.PlaySample(voice.InstrumentNumber, voice.SampleData, voice.SampleStartOffset, voice.SampleLengthInWords * 2U);
				channel.SetNote(playInfo.Octave, playInfo.Note);
			}
			else
				channel.SetSample(voice.SampleData, voice.SampleStartOffset, voice.SampleLengthInWords * 2U);

			channel.SetLoop(voice.SampleStartOffset, voice.SampleLengthInWords * 2U);

			channel.SetAmigaPeriod(voice.Period);
			channel.SetAmigaVolume(voice.FinalVolume);

			voice.SetSampleSequence = SetSample.Nothing;
		}



		/********************************************************************/
		/// <summary>
		/// Return sample information
		/// </summary>
		/********************************************************************/
		public SampleInfo GetSampleInfo(GlobalInfo globalInfo)
		{
			return new SampleInfo
			{
				Type = SampleInfo.SampleType.Synthesis,
				Flags = SampleInfo.SampleFlag.None,
				Volume = 256,
				Panning = -1,
				LoopStart = 0,
				LoopLength = 0
			};
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate all the synthesis samples
		/// </summary>
		/********************************************************************/
		private void CalculateSamples()
		{
			formatData.Samples = ArrayHelper.Initialize2Arrays<sbyte>(64, 128);

			short d4 = (short)(formatData.Oscillator[127] << 7);
			short d3 = 0;

			for (int i = 0; i < 64; i++)
			{
				sbyte[] dest = formatData.Samples[i];

				ushort d1 = Tables.X[i];
				ushort d2 = (ushort)(((0x8000 - d1) * 58982) >> 16);
				d1 /= 2;

				for (int j = 0; j < 128; j++)
				{
					d3 += (short)((((formatData.Oscillator[j] << 7) - d4) * d1) >> 14);
					d4 += d3;

					short sampleByte = (short)((d4 >> 7) | (d4 << (16 - 7)));
					dest[j] = (sbyte)sampleByte;

					d3 = (short)((d3 * d2) >> 15);
				}
			}
		}
		#endregion
	}
}
