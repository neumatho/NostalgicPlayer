/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers
{
	/// <summary>
	/// Optionals
	/// </summary>
	internal enum Optional : byte
	{
		ArpeggioOnce = 0x0,
		SetVolume = 0x1,
		SetSpeed = 0x2,
		Filter = 0x3,
		PortUp = 0x4,
		PortDown = 0x5,
		SetRepCount = 0x6,			// SoundMon 1.0 and 1.1/2.0
		DbraRepCount = 0x7,			//
		Vibrato = 0x6,				// SoundMon 2.1 and 2.2
		Jump = 0x7,					//
		SetAutoSlide = 0x8,
		SetArpeggio = 0x9,
		Transpose = 0xa,
		ChangeFx = 0xb,				// SoundMon 2.2
		ChangeInversion = 0xd,		// SoundMon 2.2
		ResetAdsr = 0xe,			// SoundMon 2.1 and 2.2
		ChangeNote = 0xf			// SoundMon 2.1 and 2.2
	}
}
