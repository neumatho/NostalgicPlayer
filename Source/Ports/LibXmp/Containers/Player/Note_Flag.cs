/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum Note_Flag : c_int
	{
		FadeOut = 1 << 0,
		Env_Release = 1 << 1,			// Envelope sustain loop release
		End = 1 << 2,
		Cut = 1 << 3,
		Env_End = 1 << 4,
		Sample_End = 1 << 5,
		Set = 1 << 6,					// For IT portamento after keyoff
		SusExit = 1 << 7,				// For delayed envelope release
		Key_Cut = 1 << 8,				// Note cut with XMP_KEY_CUT event
		Glissando = 1 << 9,
		Sample_Release = 1 << 10,		// Sample sustain loop release

		Release = Env_Release | Sample_Release	// Most of the time, these should be set/reset together
	}
}
