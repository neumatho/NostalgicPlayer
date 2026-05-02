/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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

			// Rock'n'Roll (1989). No checksum based adjustments required, because
			// it uses the unique header tag that was specific to TFMX before v1

			// Gem'X. No checksum based adjustments required, but the soundtrack
			// strictly requires a special variant of $00 DMAoff as well as the
			// set of macro commands $22 to $29. Macro commands used:
			// 000000000000000011111111111111112222222222222222
			// 0123456789abcdef0123456789abcdef0123456789abcdef
			// XXXXX XXXX  XX X    X   XXX       XXXXXXXX    

			// Danger Freak (1989) seems to be a special TFMX v1 variant
			if ((crc1 == 0x48960d8c) || (crc1 == 0x5dcd624f) || (crc1 == 0x3f0b151f))
			{
				SetTfmxV1();

				variant.NoNoteDetune = true;
				variant.PortaUnscaled = false;
			}
			// Hard'n'Heavy (1989) is a TFMX v1 variant with an AddBegin macro
			// that is missing the effect updates. The game soundtrack sounds wrong
			// in many videos
			else if ((crc1 == 0x27f8998c) || (crc1 == 0x26447707) ||
			         // Somebody compressed these files, they are not the originals
			         (crc1 == 0xd404651b) || (crc1 == 0xb5348633))
			{
				SetTfmxV1();

				variant.PortaUnscaled = true;
			}
			// R-Type (1989)
			else if (crc1 == 0x8ac70fc8)
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
		}
	}
}
