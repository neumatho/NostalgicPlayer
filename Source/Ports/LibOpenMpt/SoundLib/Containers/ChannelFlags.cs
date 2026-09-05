/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using SampleFlags = Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.ChannelFlags;

using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Module channel / sample flags
	/// </summary>
	[Flags]
	internal enum ChannelFlags : uint32
	{
		// Sample flags

		/// <summary>
		/// 16-bit sample
		/// </summary>
		Chn_16Bit = 0x01,

		/// <summary>
		/// Looped sample
		/// </summary>
		Chn_Loop = 0x02,

		/// <summary>
		/// Bidi-looped sample
		/// </summary>
		Chn_PingPongLoop = 0x04,

		/// <summary>
		/// Sample with sustain loop
		/// </summary>
		Chn_SustainLoop = 0x08,

		/// <summary>
		/// Sample with bidi sustain loop
		/// </summary>
		Chn_PingPongSustain = 0x10,

		/// <summary>
		/// Sample with forced panning
		/// </summary>
		Chn_Panning = 0x20,

		/// <summary>
		/// Stereo sample
		/// </summary>
		Chn_Stereo = 0x40,

		/// <summary>
		/// Start sample playback from sample / loop end (Velvet Studio feature)
		/// </summary>
		Chn_Reverse = 0x80,

		/// <summary>
		/// Use surround channel
		/// </summary>
		Chn_Surround = 0x100,

		/// <summary>
		/// Adlib / OPL instrument is active on this channel
		/// </summary>
		Chn_Adlib = 0x200,

		// Channel flags

		/// <summary>
		/// When flag is on, sample is processed backwards - this is intentionally the same flag as Reverse
		/// </summary>
		Chn_PingPongFlag = 0x80,

		/// <summary>
		/// Muted channel
		/// </summary>
		Chn_Mute = 0x400,

		/// <summary>
		/// Exit sustain
		/// </summary>
		Chn_KeyOff = 0x800,

		/// <summary>
		/// Fade note (instrument mode)
		/// </summary>
		Chn_NoteFade = 0x1000,

		/// <summary>
		/// Loop just wrapped around to loop start (required for correct interpolation around loop points)
		/// </summary>
		Chn_Wrapped_Loop = 0x2000,

		/// <summary>
		/// Apply Amiga low-pass filter
		/// </summary>
		Chn_AmigaFilter = 0x4000,

		/// <summary>
		/// Apply resonant filter on sample
		/// </summary>
		Chn_Filter = 0x8000,

		/// <summary>
		/// Apply volume ramping
		/// </summary>
		Chn_VolumeRamp = 0x10000,

		/// <summary>
		/// Apply vibrato
		/// </summary>
		Chn_Vibrato = 0x20000,

		/// <summary>
		/// Apply tremolo
		/// </summary>
		Chn_Tremolo = 0x40000,

		/// <summary>
		/// Apply portamento
		/// </summary>
		Chn_Portamento = 0x80000,

		/// <summary>
		/// Glissando (force portamento to semitones) mode
		/// </summary>
		Chn_Glissando = 0x100000,

		/// <summary>
		/// Force usage of global ramping settings instead of ramping over the complete render buffer length
		/// </summary>
		Chn_FastVolRamp = 0x200000,

		/// <summary>
		/// Force sample to play at 0dB
		/// </summary>
		Chn_ExtraLoud = 0x400000,

		/// <summary>
		/// Apply reverb on this channel
		/// </summary>
		Chn_Reverb = 0x800000,

		/// <summary>
		/// Disable reverb on this channel
		/// </summary>
		Chn_NoReverb = 0x1000000,

		/// <summary>
		/// Dry channel (no plugins)
		/// </summary>
		Chn_NoFx = 0x2000000,

		/// <summary>
		/// Keep sample sync on mute
		/// </summary>
		Chn_SyncMute = 0x4000000,

		// Sample flags (only present in ModSample::uFlags, may overlap with CHN_CHANNELFLAGS)

		/// <summary>
		/// Sample data has been edited in the tracker
		/// </summary>
		Smp_Modified = 0x2000,

		/// <summary>
		/// Sample is not saved to file, data is restored from original sample file
		/// </summary>
		Smp_KeepOnDisk = 0x4000,

		/// <summary>
		/// Ignore default volume setting
		/// </summary>
		Smp_NoDefaultVolume = 0x8000
	}
}
