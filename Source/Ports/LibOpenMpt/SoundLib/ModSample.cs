/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Module Sample header class and helpers
	/// </summary>
	internal class ModSample
	{
		#region PrecomputeLoop class
		private class PrecomputeLoop<T> where T : unmanaged
		{
			protected CPointer<T> target;
			protected CPointer<T> sampleData;
			protected SmpLength loopEnd;
			protected c_int numChannels;
			protected bool pingPong;
			protected bool ITPingPongMode;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PrecomputeLoop(CPointer<T> target, CPointer<T> sampleData, SmpLength loopEnd, c_int numChannels, bool pingPong, bool ITPingPongMode)
			{
				this.target = target;
				this.sampleData = sampleData;
				this.loopEnd = loopEnd;
				this.numChannels = numChannels;
				this.pingPong = pingPong;
				this.ITPingPongMode = ITPingPongMode;

				if (loopEnd > 0)
				{
					CopyLoop(true);
					CopyLoop(false);
				}
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void CopyLoop(bool direction)
			{
				// Direction: true = start reading and writing forward, false = start reading and writing backward (write direction never changes)
				c_int numSamples = (2 * Mixer.InterpolationLookaheadBufferSize) + (direction ? 1 : 0);			// Loop point is included in forward loop expansion
				CPointer<T> dest = target + (numChannels * ((2 * Mixer.InterpolationLookaheadBufferSize) - 1));	// Write buffer offset
				SmpLength readPosition = loopEnd - 1;
				c_int writeIncrement = direction ? 1 : -1;
				c_int readIncrement = writeIncrement;

				for (c_int i = 0; i < numSamples; i++)
				{
					// Copy sample over to lookahead buffer
					for (c_int c = 0; c < numChannels; c++)
						dest[c] = sampleData[(readPosition * numChannels) + c];

					dest += writeIncrement * numChannels;

					if ((readPosition == (loopEnd - 1)) && (readIncrement > 0))
					{
						// Reached end of loop while going forward
						if (pingPong)
						{
							readIncrement = -1;

							if (ITPingPongMode && (readPosition > 0))
								readPosition--;
						}
						else
							readPosition = 0;
					}
					else if ((readPosition == 0) && (readIncrement < 0))
					{
						// Reached start of loop while going backward
						if (pingPong)
							readIncrement = 1;
						else
							readPosition = loopEnd - 1;
					}
					else
						readPosition = (SmpLength)(readPosition + readIncrement);
				}
			}
		}
		#endregion

		/// <summary>
		/// In frames
		/// </summary>
		public SmpLength nLength;

		/// <summary>
		/// Ditto
		/// </summary>
		public SmpLength nLoopStart;

		/// <summary>
		/// 
		/// </summary>
		public SmpLength nLoopEnd;

		/// <summary>
		/// Ditto
		/// </summary>
		public SmpLength nSustainStart;

		/// <summary>
		/// 
		/// </summary>
		public SmpLength nSustainEnd;

		/// <summary>
		/// Pointer to sample data
		/// </summary>
		public IPointer pData;

		/// <summary>
		/// Frequency of middle-C, in Hz (for IT/S3M/MPTM)
		/// </summary>
		public uint32 nC5Speed;

		/// <summary>
		/// Default sample panning (if pan flag is set), 0...256
		/// </summary>
		public uint16 nPan;

		/// <summary>
		/// Default volume, 0...256 (ignored if uFlags[SMP_NODEFAULTVOLUME] is set)
		/// </summary>
		public uint16 nVolume;

		/// <summary>
		/// Global volume (sample volume is multiplied by this), 0...64
		/// </summary>
		public uint16 nGlobalVol;

		/// <summary>
		/// Sample flags (see ChannelFlags enum)
		/// </summary>
		public SampleFlags uFlags;

		/// <summary>
		/// Relative note to middle c (for MOD/XM)
		/// </summary>
		public int8 RelativeTone;

		/// <summary>
		/// Finetune period (for MOD/XM), -128...127, unit is 1/128th of a semitone
		/// </summary>
		public int8 nFineTune;

		/// <summary>
		/// Auto vibrato type
		/// </summary>
		public VibratoType nVibType;

		/// <summary>
		/// Auto vibrato sweep (i.e. how long it takes until the vibrato effect reaches its full depth)
		/// </summary>
		public uint8 nVibSweep;

		/// <summary>
		/// Auto vibrato depth
		/// </summary>
		public uint8 nVibDepth;

		/// <summary>
		/// Auto vibrato rate (speed)
		/// </summary>
		public uint8 nVibRate;

		/// <summary>
		/// For multisample import
		/// </summary>
		public uint8 RootNote;

		/// <summary>
		/// 
		/// </summary>
		public readonly CharBuf FileName = new CharBuf(Snd_Def.Max_SampleFileName);

		public readonly array<SmpLength> Cues = new array<SmpLength>(9);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModSample() : this(ModType.None)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModSample(ModType type)
		{
			pData = null;

			Initialize(type);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasSampleData()
		{
			return (pData != null) && (nLength != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IPointer SampleEv()
		{
			return pData;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<uint8> SampleB()
		{
			return pData.Cast<uint8>();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<int8> Sample8()
		{
			return pData.Cast<int8>();
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of one (elementary) sample in bytes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetElementarySampleSize()
		{
			return (uint8)((uFlags & ChannelFlags.Chn_16Bit) != 0 ? 2 : 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels in the sample
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetNumChannels()
		{
			return (uint8)((uFlags & ChannelFlags.Chn_Stereo) != 0 ? 2 : 1);
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of bytes per frame (Channels * Elementary
		/// Sample Size)
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8 GetBytesPerSample()
		{
			return (uint8)(GetElementarySampleSize() * GetNumChannels());
		}



		/********************************************************************/
		/// <summary>
		/// Return the size which pSample is at least
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SmpLength GetSampleSizeInBytes()
		{
			return nLength * GetBytesPerSample();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize sample slot with default values
		/// </summary>
		/********************************************************************/
		public void Initialize(ModType type)//XX 134
		{
			FreeSample();

			nLength = 0;
			nLoopStart = nLoopEnd = 0;
			nSustainStart = nSustainEnd = 0;
			nC5Speed = 8363;
			nPan = 128;
			nVolume = 256;
			nGlobalVol = 64;
			uFlags.Reset(ChannelFlags.Chn_Panning | ChannelFlags.Chn_SustainLoop | ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop | ChannelFlags.Chn_PingPongSustain | ChannelFlags.Chn_Reverse | ChannelFlags.Chn_Adlib | ChannelFlags.Smp_Modified | ChannelFlags.Smp_KeepOnDisk);

			if (type == ModType.Xm)
				uFlags.Set(ChannelFlags.Chn_Panning);

			RelativeTone = 0;
			nFineTune = 0;
			nVibType = VibratoType.Sine;
			nVibSweep = 0;
			nVibDepth = 0;
			nVibRate = 0;
			RootNote = 0;
			FileName.Assign(string.Empty);

			if ((type & (ModType.Dbm | ModType.Imf | ModType.Med)) != 0)
			{
				for (SmpLength i = 1; i < 10; i++)
					Cues[i - 1] = Util.MulDiv_Unsigned(i, 255 * 256, 9);
			}
			else
				RemoveAllCuePoints();
		}



		/********************************************************************/
		/// <summary>
		/// Allocate sample based on a ModSample's properties.
		/// Returns number of bytes allocated, 0 on failure
		/// </summary>
		/********************************************************************/
		public size_t AllocateSample()//XX 229
		{
			FreeSample();

			pData = AllocateSample(nLength, GetBytesPerSample());
			if (pData.IsNull)
				return 0;
			else
				return GetSampleSizeInBytes();
		}



		/********************************************************************/
		/// <summary>
		/// Allocate sample memory. On success, a pointer to the silenced
		/// sample buffer is returned. On failure, nullptr is returned.
		/// numFrames must contain the sample length, bytesPerSample the size
		/// of a sampling point multiplied with the number of channels
		/// </summary>
		/********************************************************************/
		public static IPointer AllocateSample(SmpLength numFrames, size_t bytesPerSample)//XX 245
		{
			size_t allocSize = GetRealSampleBufferSize(numFrames, bytesPerSample);

			if (allocSize != 0)
			{
				CPointer<byte> p = new CPointer<byte>(allocSize);

				if (p.IsNotNull)
				{
					CMemory.memset<byte>(p, 0, allocSize);

					return p + (Mixer.InterpolationLookaheadBufferSize * Mixer.MaxSamplingPointSize);
				}
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Compute sample buffer size in bytes, including any overhead
		/// introduced by pre-computed loops and such. Returns 0 if sample
		/// is too big
		/// </summary>
		/********************************************************************/
		public static size_t GetRealSampleBufferSize(SmpLength numSamples, size_t bytesPerSample)//XX 263
		{
			// Number of required lookahead samples:
			// * 1x InterpolationMaxLookahead samples before the actual sample start. This is set to MaxSamplingPointSize due to the way AllocateSample/FreeSample currently work.
			// * 1x InterpolationMaxLookahead samples of silence after the sample end (if normal loop end == sample end, this can be optimized out).
			// * 2x InterpolationMaxLookahead before the loop point (because we start at InterpolationMaxLookahead before the loop point and will look backwards from there as well)
			// * 2x InterpolationMaxLookahead after the loop point (for wrap-around)
			// * 4x InterpolationMaxLookahead for the sustain loop (same as the two points above)

			SmpLength maxSize = SmpLength.MaxValue;
			SmpLength lookaheadBufferSize = (Mixer.MaxSamplingPointSize + 1 + 4 + 4) * Mixer.InterpolationLookaheadBufferSize;

			if ((numSamples == 0) || (numSamples > Snd_Def.Max_Sample_Length) || (lookaheadBufferSize > (maxSize - numSamples)))
				return 0;

			numSamples += lookaheadBufferSize;

			if ((maxSize / bytesPerSample) < numSamples)
				return 0;

			return numSamples * bytesPerSample;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void FreeSample()//XX 290
		{
			FreeSample(pData);
			pData = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FreeSample(IPointer samplePtr)//XX 297
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void PrecomputeLoopsImpl<T>(ModSample smp, CSoundFile sndFile) where T : unmanaged//XX 421
		{
			c_int numChannels = smp.GetNumChannels();
			c_int copySamples = numChannels * Mixer.InterpolationLookaheadBufferSize;

			CPointer<T> sampleData = smp.SampleEv().Cast<T>();
			CPointer<T> afterSampleStart = sampleData + (smp.nLength * numChannels);
			CPointer<T> loopLookAheadStart = afterSampleStart + copySamples;
			CPointer<T> sustainLookAheadStart = loopLookAheadStart + (4 * copySamples);

			// Hold sample on the same level as the last sampling point at the end to prevent extra pops with interpolation.
			// Do the same at the sample start, too
			for (c_int i = 0; i < Mixer.InterpolationLookaheadBufferSize; i++)
			{
				for (c_int c = 0; c < numChannels; c++)
				{
					afterSampleStart[(i * numChannels) + c] = afterSampleStart[-numChannels + c];
					sampleData[(-(i + 1) * numChannels) + c] = sampleData[c];
				}
			}

			if (smp.uFlags.Test(ChannelFlags.Chn_Loop))
				_ = new PrecomputeLoop<T>(loopLookAheadStart, sampleData + (smp.nLoopStart * numChannels), smp.nLoopEnd - smp.nLoopStart, numChannels, smp.uFlags.Test(ChannelFlags.Chn_PingPongLoop), sndFile.m_PlayBehaviour[PlayBehaviour.ItPingPongMode]);

			if (smp.uFlags.Test(ChannelFlags.Chn_SustainLoop))
				_ = new PrecomputeLoop<T>(sustainLookAheadStart, sampleData + (smp.nSustainStart * numChannels), smp.nSustainEnd - smp.nSustainStart, numChannels, smp.uFlags.Test(ChannelFlags.Chn_PingPongSustain), sndFile.m_PlayBehaviour[PlayBehaviour.ItPingPongMode]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void PrecomputeLoops(CSoundFile sndFile, bool updateChannels)//XX 465
		{
			if (!HasSampleData())
				return;

			SanitizeLoops();

			// Update channels with possibly changed loop values
			if (updateChannels)
				UpdateLoopPointsInActiveChannels(sndFile);

			if (GetElementarySampleSize() == 2)
				PrecomputeLoopsImpl<int16>(this, sndFile);
			else if (GetElementarySampleSize() == 1)
				PrecomputeLoopsImpl<int8>(this, sndFile);
		}



		/********************************************************************/
		/// <summary>
		/// Propagate loop point changes to player
		/// </summary>
		/********************************************************************/
		public bool UpdateLoopPointsInActiveChannels(CSoundFile sndFile)//XX 486
		{
			if (!HasSampleData())
				return false;

			lock (sndFile.SndLock)
			{
				// Update channels with new loop values
				foreach (ModChannel chn in sndFile.m_PlayState.Chn)
				{
					if ((chn.pModSample != this) || (chn.nLength == 0))
						continue;

					bool looped = false, bidi = false;

					if ((nSustainStart < nSustainEnd) && (nSustainEnd <= nLength) && uFlags.Test(ChannelFlags.Chn_SustainLoop) && !chn.dwFlags.Test(ChannelFlags.Chn_KeyOff))
					{
						// Sustain loop is active
						chn.nLoopStart = nSustainStart;
						chn.nLoopEnd = nSustainEnd;
						chn.nLength = nSustainEnd;
						looped = true;
						bidi = uFlags.Test(ChannelFlags.Chn_PingPongSustain);
					}
					else if ((nLoopStart < nLoopEnd) && (nLoopEnd <= nLength) && uFlags.Test(ChannelFlags.Chn_Loop))
					{
						// Normal loop is active
						chn.nLoopStart = nLoopStart;
						chn.nLoopEnd = nLoopEnd;
						chn.nLength = nLoopEnd;
						looped = true;
						bidi = uFlags.Test(ChannelFlags.Chn_PingPongLoop);
					}

					chn.dwFlags.Set(ChannelFlags.Chn_Loop, looped);
					chn.dwFlags.Set(ChannelFlags.Chn_PingPongLoop, looped && bidi);

					if (chn.Position.GetUInt() > chn.nLength)
					{
						chn.Position.Set((int32)chn.nLoopStart);
						chn.dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);
					}

					if (!bidi)
						chn.dwFlags.Reset(ChannelFlags.Chn_PingPongFlag);

					if (!looped)
						chn.nLength = nLength;
				}

				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Remove loop points if they're invalid
		/// </summary>
		/********************************************************************/
		public void SanitizeLoops()//XX 540
		{
			OpenMpt.LimitMax(ref nSustainEnd, nLength);
			OpenMpt.LimitMax(ref nLoopEnd, nLength);

			if (nSustainStart >= nSustainEnd)
			{
				nSustainStart = nSustainEnd = 0;
				uFlags.Reset(ChannelFlags.Chn_SustainLoop | ChannelFlags.Chn_PingPongSustain);
			}

			if (nLoopStart >= nLoopEnd)
			{
				nLoopStart = nLoopEnd = 0;
				uFlags.Reset(ChannelFlags.Chn_Loop | ChannelFlags.Chn_PingPongLoop);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void RemoveAllCuePoints()//XX 646
		{
			if (!uFlags.Test(ChannelFlags.Chn_Adlib))
				Cues.fill(Snd_Def.Max_Sample_Length);
		}
	}
}
