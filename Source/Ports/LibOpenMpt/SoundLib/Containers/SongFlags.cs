/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum SongFlags
	{
		/// <summary>
		/// Portamentos are executed on every tick
		/// </summary>
		FastPortas = 0x01,

		/// <summary>
		/// Old Scream Tracker 3.0 volume slides (executed on every tick)
		/// </summary>
		FastVolSlides = 0x02,

		/// <summary>
		/// Old Impulse Tracker effect implementations
		/// </summary>
		ItOldEffects = 0x04,

		/// <summary>
		/// IT "Compatible Gxx" (IT's flag to behave more like other trackers w/r/t portamento effects)
		/// </summary>
		ItCompatGxx = 0x08,

		/// <summary>
		/// Linear slides vs. Amiga slides
		/// </summary>
		LinearSlides = 0x10,

		/// <summary>
		/// Cutoff Filter has double frequency range (up to ~10Khz)
		/// </summary>
		ExFilterRange = 0x20,

		/// <summary>
		/// Enforce Amiga frequency limits
		/// </summary>
		AmigaLimits = 0x40,

		/// <summary>
		/// ScreamTracker 2 vibrato in S3M files
		/// </summary>
		S3MOldVibrato = 0x80,

		/// <summary>
		/// ProTracker 1/2 playback mode
		/// </summary>
		Pt_Mode = 0x100,

		/// <summary>
		/// Is an Amiga module and thus qualifies to be played using the Paula BLEP resampler
		/// </summary>
		IsAmiga = 0x200,

		/// <summary>
		/// Song type does not represent actual module format / was imported from a different format (OpenMPT)
		/// </summary>
		Imported = 0x400,

		/// <summary>
		/// Play all subsongs consecutively (libopenmpt)
		/// </summary>
		PlayAllSongs = 0x800,

		/// <summary>
		/// Tone portamento command is continued automatically
		/// </summary>
		Auto_TonePorta = 0x1000,

		/// <summary>
		/// Auto tone portamento is not interrupted by a tone portamento with parameter 0
		/// </summary>
		Auto_TonePorta_Cont = 0x2000,

		/// <summary>
		/// Global volume slide command is continued automatically
		/// </summary>
		Auto_GlobalVol = 0x4000,

		/// <summary>
		/// Vibrato command is continued automatically
		/// </summary>
		Auto_Vibrato = 0x8000,

		/// <summary>
		/// Tremolo command is continued automatically
		/// </summary>
		Auto_Tremolo = 0x1_8000,

		/// <summary>
		/// Automatic volume slide command is interpreted like in STK files (rather than like in STP files)
		/// </summary>
		Auto_VolSlide_Stk = 0x2_0000,

		/// <summary>
		/// The original (imported) format has no volume column, so it can be hidden in the pattern editor
		/// </summary>
		Format_No_VolCol = 0x4_0000
	}
}
