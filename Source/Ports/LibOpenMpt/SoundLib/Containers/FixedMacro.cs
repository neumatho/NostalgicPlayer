/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Fixed macro presets
	/// </summary>
	internal enum FixedMacro
	{
		/// <summary>
		/// 
		/// </summary>
		ZxxUnused = 0,

		/// <summary>
		/// Z80 - ZFF controls resonant filter resonance
		/// </summary>
		ZxxReso4Bit,

		/// <summary>
		/// Z80 - ZFF controls resonant filter resonance
		/// </summary>
		ZxxReso7Bit,

		/// <summary>
		/// Z80 - ZFF controls resonant filter cutoff
		/// </summary>
		ZxxCutOff,

		/// <summary>
		/// Z80 - ZFF controls resonant filter mode (lowpass / highpass)
		/// </summary>
		ZxxFltMode,

		/// <summary>
		/// Z80 - Z9F controls resonance + filter mode
		/// </summary>
		ZxxResoFltMode,

		/// <summary>
		/// Z80 - ZFF controls Channel Aftertouch
		/// </summary>
		ZxxChannelAt,

		/// <summary>
		/// Z80 - ZFF controls Poly Aftertouch
		/// </summary>
		ZxxPolyAt,

		/// <summary>
		/// Z80 - ZFF controls Pitch Bend
		/// </summary>
		ZxxPitch,

		/// <summary>
		/// Z80 - ZFF controls MIDI Program Change
		/// </summary>
		ZxxProgChange,

		/// <summary>
		/// 
		/// </summary>
		ZxxCustom,

		/// <summary>
		/// 
		/// </summary>
		ZxxMax
	}
}
