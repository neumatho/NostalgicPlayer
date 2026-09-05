/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Parametered macro presets
	/// </summary>
	internal enum ParameteredMacro
	{
		/// <summary>
		/// 
		/// </summary>
		SFxUnused = 0,

		/// <summary>
		/// Z00 - Z7F controls resonant filter cutoff
		/// </summary>
		SFxCutOff,

		/// <summary>
		/// Z00 - Z7F controls resonant filter resonance
		/// </summary>
		SFxReso,

		/// <summary>
		/// Z00 - Z7F controls resonant filter mode (lowpass / highpass)
		/// </summary>
		SFxFltMode,

		/// <summary>
		/// Z00 - Z7F controls plugin Dry / Wet ratio
		/// </summary>
		SFxDryWet,

		/// <summary>
		/// Z00 - Z7F controls a plugin parameter
		/// </summary>
		SFxPlugParam,

		/// <summary>
		/// Z00 - Z7F controls MIDI CC
		/// </summary>
		SFxCC,

		/// <summary>
		/// Z00 - Z7F controls Channel Aftertouch
		/// </summary>
		SFxChannelAt,

		/// <summary>
		/// Z00 - Z7F controls Poly Aftertouch
		/// </summary>
		SFxPolyAt,

		/// <summary>
		/// Z00 - Z7F controls Pitch Bend
		/// </summary>
		SFxPitch,

		/// <summary>
		/// Z00 - Z7F controls MIDI Program Change
		/// </summary>
		SFxProgChange,

		/// <summary>
		/// 
		/// </summary>
		SFxCustom,

		/// <summary>
		/// 
		/// </summary>
		SFxMax
	}
}
