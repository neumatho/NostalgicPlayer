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
	/// Handle IFF sample instruments
	/// </summary>
	internal class FormFormat : IInstrumentFormat
	{
		private IffSample formatData;
		private ushort numberOfHiOctavesToSkip;

		/********************************************************************/
		/// <summary>
		/// Try to identify the format
		/// </summary>
		/********************************************************************/
		public static bool Identify(byte[] firstBytes)
		{
			const string Id = "FORM";

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

			instrumentStream.Seek(0, SeekOrigin.Begin);

			LoadResult result = IffSampleLoader.Load(instrumentStream, instruments.Count, out formatData);

			if (result != LoadResult.Ok)
			{
				errorMessage = string.Format(Resources.IDS_SMUS_ERR_LOADING_READ_EXTERNAL_FILE, instrumentFileName);
				return false;
			}

			uint oneShot = formatData.OneShotHiSamples;
			uint repeat = formatData.RepeatHiSamples;
			uint samplesPerCycle = formatData.SamplesPerHiCycle;
			ushort count = 0;

			for (;;)
			{
				oneShot /= 2;
				repeat /= 2;
				samplesPerCycle /= 2;
				count++;

				if (((oneShot | repeat) & 1) != 0)
					break;

				if (samplesPerCycle == 1)
					break;

				if ((count + formatData.Octaves) >= 8)
					break;
			}

			numberOfHiOctavesToSkip = count;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the instrument
		/// </summary>
		/********************************************************************/
		public void Setup(GlobalInfo globalInfo, GlobalPlayingInfo playingInfo, VoiceInfo voice, IChannel channel, int channelNumber)
		{
			FormPlayInfo playInfo = playingInfo.FormPlayInfo[channelNumber];

			switch (voice.InstrumentSetupSequence)
			{
				case InstrumentSetup.Initialize:
				{
					int octave = voice.Note / 12;
					int note = voice.Note % 12;

					playInfo.MappedNote = (ushort)((Tables.Note[note] * 54728) >> 15);
					playInfo.Octave = (byte)octave;
					playInfo.Note = (byte)note;

					int octaveInFormat = (-(octave - 10)) - numberOfHiOctavesToSkip;
					if ((octaveInFormat < 0) || (octaveInFormat >= formatData.Octaves))
					{
						voice.InstrumentSetupSequence = InstrumentSetup.Nothing;

						if (voice.Status == VoiceStatus.Silence)
							return;

						break;
					}

					uint sampleLength = formatData.OneShotHiSamples + formatData.RepeatHiSamples;
					uint startOffset = (sampleLength << octaveInFormat) - sampleLength;

					uint oneShotLength = formatData.OneShotHiSamples << octaveInFormat;
					uint loopLength = formatData.RepeatHiSamples << octaveInFormat;

					voice.SampleData = formatData.SampleData;
					voice.SampleStartOffset = startOffset;
					voice.SampleLengthInWords = (ushort)((oneShotLength != 0 ? oneShotLength : loopLength) / 2);

					if (loopLength != 0)
					{
						playInfo.LoopSampleData = voice.SampleData;
						playInfo.LoopStart = voice.SampleStartOffset + oneShotLength;
						playInfo.LoopLengthInWords = (ushort)(loopLength / 2);
					}
					else
					{
						playInfo.LoopSampleData = null;
						playInfo.LoopStart = 0;
						playInfo.LoopLengthInWords = 0;
					}

					voice.SetSampleSequence = SetSample.StartSample;

					channel.Mute();

					playInfo.VolumeMultiply = 1;
					break;
				}

				case InstrumentSetup.ReleaseNote:
				{
					playInfo.VolumeMultiply = 0;
					break;
				}

				case InstrumentSetup.Mute:
				{
					channel.Mute();
					return;
				}
			}

			voice.Period = (ushort)(((0x1080 - globalInfo.Tune) * playInfo.MappedNote) >> 0x13);
			voice.FinalVolume = (ushort)(((((((playingInfo.CurrentVolume + 1) * voice.Volume) / 256) + 1) * (formatData.Volume >> 1)) >> 0x11) * playInfo.VolumeMultiply);
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
					FormPlayInfo playInfo = playingInfo.FormPlayInfo[channelNumber];

					channel.PlaySample(voice.InstrumentNumber, voice.SampleData, voice.SampleStartOffset, voice.SampleLengthInWords * 2U);
					channel.SetNote(playInfo.Octave, playInfo.Note);

					voice.SetSampleSequence = SetSample.SetLoop;
					goto case SetSample.SetLoop;
				}

				case SetSample.SetLoop:
				{
					FormPlayInfo playInfo = playingInfo.FormPlayInfo[channelNumber];

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

			if (formatData.Octaves == 1)
			{
				sampleInfo.Sample = formatData.SampleData;
				sampleInfo.Length = (uint)formatData.SampleData.Length;

				if (formatData.RepeatHiSamples != 0)
				{
					sampleInfo.LoopStart = formatData.OneShotHiSamples;
					sampleInfo.LoopLength = formatData.RepeatHiSamples;
					sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
				}
			}
			else
			{
				sampleInfo.Length = (uint)formatData.SampleData.Length;

				if (formatData.RepeatHiSamples != 0)
				{
					sampleInfo.LoopStart = formatData.OneShotHiSamples;
					sampleInfo.LoopLength = formatData.RepeatHiSamples;
					sampleInfo.Flags |= SampleInfo.SampleFlag.Loop;
				}

				SampleInfo.MultiOctaveInfo[] multiOctaveInfo = new SampleInfo.MultiOctaveInfo[8];
				List<sbyte[]> allSamples = new List<sbyte[]>();

				for (int i = 0, j = numberOfHiOctavesToSkip; (i < formatData.Octaves) && (j < 8); i++, j++)
				{
					uint sampleLength = formatData.OneShotHiSamples + formatData.RepeatHiSamples;
					uint startOffset = (sampleLength << i) - sampleLength;

					uint oneShotLength = formatData.OneShotHiSamples << i;
					uint loopLength = formatData.RepeatHiSamples << i;

					multiOctaveInfo[j].Sample = formatData.SampleData;
					multiOctaveInfo[j].SampleOffset = startOffset;
					multiOctaveInfo[j].Length = oneShotLength != 0 ? oneShotLength : loopLength;
					multiOctaveInfo[j].NoteAdd = i * 12;

					if (loopLength != 0)
					{
						multiOctaveInfo[j].LoopStart = startOffset + oneShotLength;
						multiOctaveInfo[j].LoopLength = loopLength;
					}

					allSamples.Add(formatData.SampleData.AsSpan((int)multiOctaveInfo[j].SampleOffset, (int)multiOctaveInfo[j].Length).ToArray());
				}

				for (int i = 0; i < numberOfHiOctavesToSkip; i++)
					multiOctaveInfo[i] = multiOctaveInfo[numberOfHiOctavesToSkip];

				for (int i = numberOfHiOctavesToSkip + formatData.Octaves; i < 8; i++)
					multiOctaveInfo[i] = multiOctaveInfo[numberOfHiOctavesToSkip + formatData.Octaves - 1];

				sampleInfo.MultiOctaveSamples = multiOctaveInfo;
				sampleInfo.MultiOctaveAllSamples = allSamples.ToArray();
				sampleInfo.Flags |= SampleInfo.SampleFlag.MultiOctave;
			}

			return sampleInfo;
		}
	}
}
