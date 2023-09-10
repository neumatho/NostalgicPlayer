/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Player parameters
	/// </summary>
	public enum Xmp_Player
	{
		/// <summary>
		/// Amplification factor
		/// </summary>
		Amp,

		/// <summary>
		/// Stereo mixing
		/// </summary>
		Mix,

		/// <summary>
		/// Interpolation type
		/// </summary>
		Interp,

		/// <summary>
		/// DSP effect flags
		/// </summary>
		Dsp,

		/// <summary>
		/// Player flags
		/// </summary>
		Flags,

		/// <summary>
		/// Player flags for current module
		/// </summary>
		CFlags,

		/// <summary>
		/// Sample control flags
		/// </summary>
		SmpCtl,

		/// <summary>
		/// Player module volume
		/// </summary>
		Volume,

		/// <summary>
		/// Internal player state (read only)
		/// </summary>
		State,

		/// <summary>
		/// Default pan setting
		/// </summary>
		DefPan,

		/// <summary>
		/// Player personality
		/// </summary>
		Mode,

		/// <summary>
		/// Current mixer (read only)
		/// </summary>
		Mixer_Type,

		/// <summary>
		/// Maximum number of mixer voices
		/// </summary>
		Voices,

		/// <summary>
		/// The mixer frequency to use
		/// </summary>
		MixerFrequency,

		/// <summary>
		/// Number of channels to mix.
		/// Currently only 1 and 2 are supported
		/// </summary>
		MixerChannels,
	}
}
