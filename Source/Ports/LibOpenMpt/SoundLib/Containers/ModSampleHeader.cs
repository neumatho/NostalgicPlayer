/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Sample header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 30)]
	internal struct ModSampleHeader
	{
		public String22 Name;
		public uint16be Length;
		public uint8be FineTune;
		public uint8be Volume;
		public uint16be LoopStart;
		public uint16be LoopLength;

		/// <summary>
		/// Suggested threshold for rejecting invalid files based on cumulated score returned by GetInvalidByteScore
		/// </summary>
		public const uint32 Invalid_Byte_Threshold = 40;

		/// <summary>
		/// This threshold is used for files where the file magic only gives a
		/// fragile result which alone would lead to too many false positives.
		/// In particular, the files from Inconexia demo by Iguana
		/// (https://www.pouet.net/prod.php?which=830) which have 3 \0 bytes in
		/// the file magic tend to cause misdetection of random files
		/// </summary>
		public const uint32 Invalid_Byte_Fragile_Threshold = 1;

		/********************************************************************/
		/// <summary>
		/// Convert an MOD sample header to OpenMPT's internal sample header
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModSample mptSmp, bool is4Chn)//XX 196
		{
			mptSmp.Initialize(ModType.Mod);

			mptSmp.nLength = Length * 2U;
			mptSmp.nFineTune = CSoundFile.Mod2XmFineTune(FineTune & 0x0f);
			mptSmp.nVolume = (uint16)(4U * Math.Min(Volume, (uint8)64));

			SmpLength lStart = LoopStart * 2U;
			SmpLength lLength = LoopLength * 2U;

			// See if loop start is incorrect as words, but correct as bytes (like in Soundtracker modules)
			if ((lLength > 2) && ((lStart + lLength) > mptSmp.nLength) && (((lStart / 2) + lLength) <= mptSmp.nLength))
				lStart /= 2;

			if (mptSmp.nLength == 2)
				mptSmp.nLength = 0;

			if (mptSmp.nLength != 0)
			{
				mptSmp.nLoopStart = lStart;
				mptSmp.nLoopEnd = lStart + lLength;

				if (mptSmp.nLoopStart >= mptSmp.nLength)
					mptSmp.nLoopStart = mptSmp.nLength - 1;

				if ((mptSmp.nLoopStart > mptSmp.nLoopEnd) || (mptSmp.nLoopEnd < 4) || ((mptSmp.nLoopEnd - mptSmp.nLoopStart) < 4))
				{
					mptSmp.nLoopStart = 0;
					mptSmp.nLoopEnd = 0;
				}

				// Fix for most likely broken sample loops. This fixes super_sufm_-_new_life.mod (M.K.) which has a long sample which is looped from 0 to 4.
				// This module also has notes outside of the Amiga frequency range, so we cannot say that it should be played using ProTracker one-shot loops.
				// On the other hand, "Crew Generation" by Necros (6CHN) has a sample with a similar loop, which is supposed to be played.
				// To be able to correctly play both modules, we will draw a somewhat arbitrary line here and trust the loop points in MODs with more than
				// 4 channels, even if they are tiny and at the very beginning of the sample
				if ((mptSmp.nLoopEnd <= 8) && (mptSmp.nLoopStart == 0) && (mptSmp.nLength > mptSmp.nLoopEnd) && is4Chn)
					mptSmp.nLoopEnd = 0;

				if (mptSmp.nLoopEnd > mptSmp.nLoopStart)
					mptSmp.uFlags.Set(ChannelFlags.Chn_Loop);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Compute a "rating" of this sample header by counting invalid
		/// header data to ultimately reject garbage files
		/// </summary>
		/********************************************************************/
		public uint32 GetInvalidByteScore()//XX 288
		{
			return (uint32)(((Volume > 64) ? 1 : 0) + ((FineTune > 15) ? 1 : 0) + ((LoopStart > (Length * 2)) ? 1 : 0));
		}
	}
}
