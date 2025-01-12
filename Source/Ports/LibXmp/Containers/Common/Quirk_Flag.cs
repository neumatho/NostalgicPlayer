/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// Quirks
	/// </summary>
	[Flags]
	internal enum Quirk_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// S3M loop mode
		/// </summary>
		S3MLoop = (1 << 0),

		/// <summary>
		/// Fade at end of envelope
		/// </summary>
		EnvFade = (1 << 1),

		/// <summary>
		/// Use ProTracker-specific quirks
		/// </summary>
		ProTrack = (1 << 2),

		/// <summary>
		/// Retrigger one time only
		/// </summary>
		RtOnce = (1 << 3),

		/// <summary>
		/// Scream Tracker 3 bug compatibility
		/// </summary>
		St3Bugs = (1 << 4),

		/// <summary>
		/// Enable 0xf/0xe for fine effects
		/// </summary>
		FineFx = (1 << 5),

		/// <summary>
		/// Volume slides in all frames
		/// </summary>
		VsAll = (1 << 6),

		/// <summary>
		/// Pitch bending in all frames
		/// </summary>
		PbAll = (1 << 7),

		/// <summary>
		/// Cancel persistent fx at pat start
		/// </summary>
		PerPat = (1 << 8),

		/// <summary>
		/// Set priority to volume slide down
		/// </summary>
		VolPdn = (1 << 9),

		/// <summary>
		/// Unified pitch slide/portamento
		/// </summary>
		UniSld = (1 << 10),

		/// <summary>
		/// Disable fine bends in IT vol fx
		/// </summary>
		ItVPor = (1 << 11),

		/// <summary>
		/// Flag for multichannel mods
		/// </summary>
		FtMod = (1 << 12),

		/// <summary>
		/// Enable invert loop
		/// </summary>
		InvLoop = (1 << 13),

		/// <summary>
		/// Use instrument volume
		/// </summary>
		InsVol = (1 << 14),

		/// <summary>
		/// Enable virtual channels
		/// </summary>
		Virtual = (1 << 15),

		/// <summary>
		/// Enable filter
		/// </summary>
		Filter = (1 << 16),

		/// <summary>
		/// Ignore stray tone portamento
		/// </summary>
		IgStPor = (1 << 17),

		/// <summary>
		/// Keyoff doesn't reset fadeout
		/// </summary>
		KeyOff = (1 << 18),

		/// <summary>
		/// Vibrato is half as deep
		/// </summary>
		VibHalf = (1 << 19),

		/// <summary>
		/// Vibrato in all frames
		/// </summary>
		VibAll = (1 << 20),

		/// <summary>
		/// Vibrato has inverse waveform
		/// </summary>
		VibInv = (1 << 21),

		/// <summary>
		/// Portamento resets envelope and fade
		/// </summary>
		PrEnv = (1 << 22),

		/// <summary>
		/// IT old effects mode
		/// </summary>
		ItOldFx = (1 << 23),

		/// <summary>
		/// S3M-style retrig when count == 0
		/// </summary>
		S3MRtg = (1 << 24),

		/// <summary>
		/// Delay effect retrigs instrument
		/// </summary>
		RtDelay = (1 << 25),

		/// <summary>
		/// FT2 bug compatibility
		/// </summary>
		Ft2Bugs = (1 << 26),

		/// <summary>
		/// Patterns 0xfe and 0xff reserved
		/// </summary>
		Marker = (1 << 27),

		/// <summary>
		/// Adjust speed only, no BPM
		/// </summary>
		NoBpm = (1 << 28),

		/// <summary>
		/// Arpeggio has memory (S3M_ARPEGGIO)
		/// </summary>
		ArpMem = (1 << 29),

		/// <summary>
		/// Reset channel on sample end
		/// </summary>
		RstChn = (1 << 30),

		/// <summary>
		/// Use FT2-style envelope handling
		/// </summary>
		Ft2Env = (1 << 31),

		// Format quirks
		St3 = S3MLoop | VolPdn | FineFx | S3MRtg | Marker | RstChn,
		Ft2 = RtDelay | FineFx,
		It = S3MLoop | FineFx | VibAll | EnvFade | ItVPor | KeyOff | Virtual | Filter | RstChn | IgStPor | S3MRtg | Marker
	}
}
