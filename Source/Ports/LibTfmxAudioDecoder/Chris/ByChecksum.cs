/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// Since the TFMX header and data don't tell which specific version or
	/// variant of TFMX are required, this function comes as a last resort.
	/// It tries to recognize specific files via a checksum of a small portion
	/// of the pattern data and then adjusts player traits
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TraitsByChecksum()
		{
			SmartPtr<ubyte> sBuf = new SmartPtr<ubyte>(pBuf.TellBegin(), pBuf.TellLength());
			udword p0 = offsets.Header + MyEndian.ReadBEUdword(sBuf, offsets.Patterns);
			udword crc1 = CrcLight.Get(sBuf, p0, 0x100);

			// Gem'X. No checksum based adjustments required, but the soundtrack
			// strictly requires a special variant of $00 DMAoff as well as the
			// set of macro commands $22 to $29. Macro commands used:
			// 000000000000000011111111111111112222222222222222
			// 0123456789abcdef0123456789abcdef0123456789abcdef
			// XXXXX XXXX  XX X    X   XXX       XXXXXXXX    

			// Turrican
			udword[] t1_Checksums =
			[
				0x73868fda,		// Title, credits, highscore
				0x93730029,		// Title
				0x6e799869,		// World 1
				0x3d00fc52,		// World 2
				0xd76d33ed,		// World 3
				0xb989d570,		// World 4
				0x8fa80b4e,		// World 5
				0x88f7c34b,		// Loader
				0xb762f2dc		// Bonus
			];

			// Turrican II
			udword[] t2_Checksums =
			[
				0xe2d6b5e0,		// World 1
				0x0bf41b64,		// World 2
				0x19ac72c3,		// World 3
				0x03854473,		// World 4
				0x9430d9de,		// World 5
				0xc1ac5e1c,		// Loader
				0xcbb842b0,		// Loading
				0x78cd70f9		// Demo
			];

			// Turrican II (1991) ingame music requires a special player variant
			// with different execution order of macros and effects
			if (t2_Checksums.Contains(crc1))
				variant.ExecOrder = ExecOrder.Mod_Mac_Seq;
			// Turrican (1990) is a TFMXv1 variant and strictly requires old
			// features such as non-scaled vibrato/portamento
			else if (t1_Checksums.Contains(crc1))
			{
				SetTfmxV1();

				variant.VibratoTimeMask = true;

				macroCmdFuncs[0x0d] = MacroFunc_AddVolNote;
				macroCmdFuncs[0x1a] = MacroFunc_WaitOnDma;
				macroCmdFuncs[0x1c] = MacroFunc_SplitKey;
				macroCmdFuncs[0x1d] = MacroFunc_SplitVolume;
			}
			// Danger Freak (1989) seems to be a special TFMX v1 variant
			else if ((crc1 == 0x48960d8c) || (crc1 == 0x5dcd624f) || (crc1 == 0x3f0b151f))
			{
				SetTfmxV1();

				variant.NoNoteDetune = true;
			}
			// Hard'n'Heavy (1989) is a TFMX v1 variant. In particular, it strictly
			// requires the old AddBegin macro that is missing the effect updates.
			// Alternatively, the AddBegin count parameter must be ignored.
			// The game's title soundtrack sounds wrong in many videos
			else if ((crc1 == 0x27f8998c) || (crc1 == 0x26447707) ||
					 // Somebody compressed these files, they are not the originals
					 (crc1 == 0xd404651b) || (crc1 == 0xb5348633))
			{
				SetTfmxV1();
			}
			// R-Type (1989)
			else if (crc1 == 0x8ac70fc8)
			{
				SetTfmxV1();
				variant.MacroLoopExtraWait = true;
			}
			// The Adventures of Quik & Silva
			else if ((crc1 == 0x04f469a6) || (crc1 == 0xd37c9008))
			{
				variant.ExecOrder = ExecOrder.Mac_Mod_Seq;
			}
			// Rock'n'Roll (1989). No checksum based adjustments required, because
			// it uses the unique header tag that was specific to TFMX before v1.
			// Except for the intro
			else if (crc1 == 0xda279570)
			{
				SetTfmxV1();
			}
			// Software Manager - Title 2
			else if (crc1 == 0xa8566760)
			{
				variant.NoTrackMute = true;
			}
			// BiFi Adventure 2 - Ongame
			else if (crc1 == 0xab1a6c6e)
			{
				variant.NoTrackMute = true;
			}
			// Ooops Up by Peter Thierolf. First two sub-songs specify a BPM customization
			// that isn't compatible with the speed count value of default TFMX
			else if (crc1 == 0x76f8aa6e)
				variant.BpmSpeed5 = true;
			// TFMX music by ern0 in the music demo "Musication Vol. 1" are
			// replayed with a TFMX v1 player
			else if ((crc1 == 0x0eed9c91) ||	// Danubius Replay (aka Gitar)
					 (crc1 == 0x1a5d2b53) ||	// Flying World
					 (crc1 == 0x22a92c26) ||	// Armageddon
					 (crc1 == 0xe60babf2))		// Magnetic Fields IV (aka Oxygen)
			{
				SetTfmxV1();
			}
			else if (crc1 == 0x5fb2f54e)		// Puzzy
			{
				SetTfmxV1();

				// Fix second song's track end
				pBuf[offsets.Header + 0x143] = 0x6f;
			}
			else if (crc1 == 0xcb3c7113)		// Sledge Hammer One (aka Hammer One)
			{
				SetTfmxV1();

				// Fix song start/end. Song table is wrecked
				pBuf[offsets.Header + 0x101] = 0x04;
				pBuf[offsets.Header + 0x141] = 0x6b;
			}
			// File "mdat.blade of destiny - titel (7ch)" is bad/corrupted
			else if (crc1 == 0xc83b701b)
			{
				blacklisted = true;
			}
		}
	}
}
