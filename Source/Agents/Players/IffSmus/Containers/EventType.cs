/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// The different event types
	/// </summary>
	internal enum EventType : byte
	{
		FirstNote = 0,
		LastNote = 127,

		/// <summary>
		/// A rest (same data format as a note)
		/// </summary>
		Rest = 128,

		/// <summary>
		/// Set instrument number for this track
		/// </summary>
		Instrument,

		/// <summary>
		/// Set time signature for this track
		/// </summary>
		TimeSig,

		/// <summary>
		/// Set key signature for this track
		/// </summary>
		KeySig,

		/// <summary>
		/// Set volume for this track
		/// </summary>
		Volume,

		/// <summary>
		/// Set MIDI channel number (sequencers)
		/// </summary>
		MidiChnl,

		/// <summary>
		/// Set MIDI preset number (sequencers)
		/// </summary>
		MidiPreset,

		/// <summary>
		/// Inline clef change (0=Treble, 1=Bass, 2=Alto, 3=Tenor)
		/// </summary>
		Clef,

		/// <summary>
		/// Inline tempo in beats per minute
		/// </summary>
		Tempo,

		/// <summary>
		/// Reserved for an end-mark in RAM
		/// </summary>
		Mark = 255
	}
}
