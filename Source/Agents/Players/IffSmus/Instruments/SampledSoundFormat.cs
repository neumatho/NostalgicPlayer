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
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Handle sampled sound instruments
	/// </summary>
	internal class SampledSoundFormat : IInstrumentFormat
	{
		private SampledSoundData formatData;

		/********************************************************************/
		/// <summary>
		/// Try to identify the format
		/// </summary>
		/********************************************************************/
		public static bool Identify(byte[] firstBytes)
		{
			const string Id = "SampledSound";

			string checkId = Encoding.ASCII.GetString(firstBytes, 0, Id.Length);
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

			instrumentStream.Seek(0x44, SeekOrigin.Begin);
			string sampleName = instrumentStream.ReadString(EncoderCollection.Amiga, 32);

			foreach (Instrument instr in instruments)
			{
				if ((instr.Format is SampledSoundFormat sampledSoundFormat) && (sampledSoundFormat.formatData.SampleName.Equals(sampleName, StringComparison.OrdinalIgnoreCase)))
				{
					// Reuse format data
					formatData = sampledSoundFormat.formatData;
					return true;
				}
			}

			formatData = new SampledSoundData
			{
				SampleName = sampleName
			};

			// Skip pointer to data
			instrumentStream.Seek(4, SeekOrigin.Current);

			// Read instrument information
			formatData.Volume = instrumentStream.Read_B_UINT16();
			instrumentStream.ReadArray_B_UINT16s(formatData.EnvelopeLevels, 0, 4);
			instrumentStream.ReadArray_B_UINT16s(formatData.EnvelopeRates, 0, 4);
			formatData.VibratoDepth = instrumentStream.Read_B_INT16();
			formatData.VibratoSpeed = instrumentStream.Read_B_UINT16();
			formatData.VibratoDelay = instrumentStream.Read_B_UINT16();

			if (instrumentStream.EndOfStream)
			{
				errorMessage = Resources.IDS_SMUS_ERR_LOADING_INSTRUMENTS;
				return false;
			}

			string sampleFileName = $"{sampleName}.ss";
			string samplePath = Path.Combine(instrumentPath, sampleFileName);

			using (ModuleStream sampleStream = fileInfo.Loader?.OpenExtraFileByFileName(samplePath, true))
			{
				// Did we get any file at all
				if (sampleStream == null)
				{
					errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_OPEN_EXTERNAL_FILE, sampleFileName);
					return false;
				}

				// Read the header
				formatData.LengthOfOctaveOne = sampleStream.Read_B_UINT16();
				formatData.LoopLengthOfOctaveOne = sampleStream.Read_B_UINT16();
				formatData.StartOctave = sampleStream.Read_UINT8();
				formatData.EndOctave = sampleStream.Read_UINT8();

				if (sampleStream.EndOfStream)
				{
					errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_READ_EXTERNAL_FILE, sampleFileName);
					return false;
				}

				sampleStream.Seek(0x3e, SeekOrigin.Begin);

				// Read sample data
				int sampleLength = (int)(sampleStream.Length - 0x3e);
				formatData.SampleData = sampleStream.ReadSampleData(instruments.Count, sampleLength, out int readBytes);
				if (readBytes != sampleLength)
				{
					errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_READ_EXTERNAL_FILE, sampleFileName);
					return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the instrument
		/// </summary>
		/********************************************************************/
		public void Setup(GlobalInfo globalInfo, GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber)
		{
			SampledSoundPlayInfo playInfo = playingInfo.SamplePlayInfo[channelNumber];

			switch (voice.InstrumentSetupSequence)
			{
				case InstrumentSetup.Initialize:
				{
					int octave = -((voice.Note / 12) - 10);
					int note = voice.Note % 12;

					if ((octave > formatData.EndOctave) || (octave < formatData.StartOctave))
					{
						voice.InstrumentSetupSequence = InstrumentSetup.Nothing;

						if (voice.Status == VoiceStatus.Silence)
							return;

						break;
					}

					if (voice.Status == VoiceStatus.Silence)
						playInfo.EnvelopeVolume = 0;

					if (voice.Status != VoiceStatus.Playing)
						playInfo.EnvelopeIndex = 0;

					playInfo.MappedNote = (ushort)((Tables.Note[note] * 54728) >> 15);
					playInfo.Octave = (byte)(voice.Note / 12);
					playInfo.Note = (byte)note;

					int temp1 = 1 << octave;
					int temp2 = 1 << formatData.StartOctave;

					voice.SampleData = formatData.SampleData;
					voice.SampleStartOffset = (uint)((temp1 - temp2) * formatData.LengthOfOctaveOne);
					voice.SampleLengthInWords = (ushort)((formatData.LengthOfOctaveOne * temp1) / 2);

					if (formatData.LengthOfOctaveOne != formatData.LoopLengthOfOctaveOne)
					{
						playInfo.LoopSampleData = voice.SampleData;
						playInfo.LoopStart = (uint)(voice.SampleStartOffset + formatData.LoopLengthOfOctaveOne * temp1);
						playInfo.LoopLengthInWords = (ushort)(((formatData.LengthOfOctaveOne - formatData.LoopLengthOfOctaveOne) * temp1) / 2);
					}
					else
					{
						playInfo.LoopSampleData = null;
						playInfo.LoopStart = 0;
						playInfo.LoopLengthInWords = 0;
					}

					playInfo.VibratoIndex = 0;
					playInfo.VibratoDelayCounter = (ushort)(((formatData.VibratoDelay << 15) / playingInfo.CalculatedTempo) / 2);

					voice.SetSampleSequence = SetSample.StartSample;

					channel.Mute();
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

			if (playInfo.VibratoDelayCounter == 0)
				playInfo.VibratoIndex += (ushort)(((formatData.VibratoSpeed * playingInfo.CalculatedTempo) >> 9) + 64);
			else
				playInfo.VibratoDelayCounter--;

			ushort vibVal = (ushort)((playInfo.VibratoIndex >> 7) + 128);
			if ((vibVal & 0x100) != 0)
				vibVal ^= 0xff;

			vibVal ^= 0x80;
			playInfo.VibratoValue = (short)-((sbyte)vibVal);

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

			voice.Period = (ushort)(((((formatData.VibratoDepth * playInfo.VibratoValue) >> 7) - (globalInfo.Tune - 0x80) + 0x1000) * playInfo.MappedNote) >> 0x13);
			voice.FinalVolume = (ushort)((((((((playingInfo.CurrentVolume + 1) * voice.Volume) / 256) + 1) * formatData.Volume) / 256) * (playInfo.EnvelopeVolume >> 16)) >> 10);
		}



		/********************************************************************/
		/// <summary>
		/// Play the initialized instrument
		/// </summary>
		/********************************************************************/
		public void Play(GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber)
		{
			switch (voice.SetSampleSequence)
			{
				case SetSample.StartSample:
				{
					SampledSoundPlayInfo playInfo = playingInfo.SamplePlayInfo[channelNumber];

					channel.PlaySample(voice.InstrumentNumber, voice.SampleData, voice.SampleStartOffset, voice.SampleLengthInWords * 2U);
					channel.SetNote(playInfo.Octave, playInfo.Note);

					voice.SetSampleSequence = SetSample.SetLoop;
					goto case SetSample.SetLoop;
				}

				case SetSample.SetLoop:
				{
					SampledSoundPlayInfo playInfo = playingInfo.SamplePlayInfo[channelNumber];

					if (playInfo.LoopSampleData != null)
						channel.SetLoop(playInfo.LoopSampleData, playInfo.LoopStart, playInfo.LoopLengthInWords * 2U);

					voice.SetSampleSequence = SetSample.Nothing;
					break;
				}
			}

			channel.SetAmigaPeriod(voice.Period);
			channel.SetAmigaVolume(voice.FinalVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Return sample information
		/// </summary>
		/********************************************************************/
		public SampleInfo GetSampleInfo(GlobalInfo globalInfo)
		{
			SampleInfo sampleInfo = new SampleInfo
			{
				Type = SampleInfo.SampleType.Sample,
				Flags = SampleInfo.SampleFlag.None,
				Volume = 256,
				Panning = -1,
			};

			if (formatData.StartOctave == formatData.EndOctave)
			{
				sampleInfo.Sample = formatData.SampleData;
				sampleInfo.Length = (uint)formatData.SampleData.Length;

				if (formatData.LengthOfOctaveOne != formatData.LoopLengthOfOctaveOne)
				{
					sampleInfo.LoopStart = 0;
					sampleInfo.LoopLength = sampleInfo.Length;
					sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
				}
			}
			else
			{
				sampleInfo.Length = (uint)formatData.SampleData.Length;

				if (formatData.LengthOfOctaveOne != formatData.LoopLengthOfOctaveOne)
				{
					sampleInfo.LoopStart = 0;
					sampleInfo.LoopLength = formatData.LoopLengthOfOctaveOne;
					sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
				}

				SampleInfo.MultiOctaveInfo[] multiOctaveInfo = new SampleInfo.MultiOctaveInfo[8];
				List<sbyte[]> allSamples = new List<sbyte[]>();

				for (int i = formatData.StartOctave; (i <= formatData.EndOctave) && (i < 8); i++)
				{
					int temp1 = 1 << i;
					int temp2 = 1 << formatData.StartOctave;

					multiOctaveInfo[i].Sample = formatData.SampleData;
					multiOctaveInfo[i].SampleOffset = (uint)((temp1 - temp2) * formatData.LengthOfOctaveOne);
					multiOctaveInfo[i].Length = (uint)(formatData.LengthOfOctaveOne * temp1);
					multiOctaveInfo[i].NoteAdd = (i - formatData.StartOctave) * 12;

					if (formatData.LengthOfOctaveOne != formatData.LoopLengthOfOctaveOne)
					{
						multiOctaveInfo[i].LoopStart = (uint)(formatData.LoopLengthOfOctaveOne * temp1);
						multiOctaveInfo[i].LoopLength = (uint)((formatData.LengthOfOctaveOne - formatData.LoopLengthOfOctaveOne) * temp1);
					}

					allSamples.Add(formatData.SampleData.AsSpan((int)multiOctaveInfo[i].SampleOffset, (int)multiOctaveInfo[i].Length).ToArray());
				}

				for (int i = 0; i < formatData.StartOctave; i++)
					multiOctaveInfo[i] = multiOctaveInfo[formatData.StartOctave];

				for (int i = formatData.EndOctave + 1; i < 8; i++)
					multiOctaveInfo[i] = multiOctaveInfo[formatData.EndOctave];

				sampleInfo.MultiOctaveSamples = multiOctaveInfo;
				sampleInfo.MultiOctaveAllSamples = allSamples.ToArray();
				sampleInfo.Flags |= SampleInfo.SampleFlag.MultiOctave;
			}

			return sampleInfo;
		}
	}
}
