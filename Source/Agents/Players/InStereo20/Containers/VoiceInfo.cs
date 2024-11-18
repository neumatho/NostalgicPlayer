/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		// Position information
		public ushort StartTrackRow;
		public sbyte SoundTranspose;
		public sbyte NoteTranspose;

		// Track row information
		public byte Note;
		public byte Instrument;
		public bool DisableSoundTranspose;
		public bool DisableNoteTranspose;
		public byte Arpeggio;
		public Effect Effect;
		public byte EffectArg;

		public byte TransposedNote;
		public byte PreviousTransposedNote;

		public byte TransposedInstrument;
		public VoicePlayingMode PlayingMode;

		public byte CurrentVolume;

		public ushort ArpeggioPosition;
		public bool ArpeggioEffectNibble;

		public sbyte SlideSpeed;
		public short SlideValue;

		public ushort PortamentoSpeedCounter;
		public ushort PortamentoSpeed;

		public byte VibratoDelay;
		public byte VibratoSpeed;
		public byte VibratoLevel;
		public ushort VibratoPosition;

		public ushort AdsrPosition;
		public ushort SustainCounter;

		public sbyte EnvelopeGeneratorDuration;
		public ushort EnvelopeGeneratorPosition;

		public ushort LfoPosition;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
