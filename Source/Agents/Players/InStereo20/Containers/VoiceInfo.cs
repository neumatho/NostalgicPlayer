/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo20.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		// Position information
		public ushort StartTrackRow { get; set; }
		public sbyte SoundTranspose { get; set; }
		public sbyte NoteTranspose { get; set; }

		// Track row information
		public byte Note { get; set; }
		public byte Instrument { get; set; }
		public bool DisableSoundTranspose { get; set; }
		public bool DisableNoteTranspose { get; set; }
		public byte Arpeggio { get; set; }
		public Effect Effect { get; set; }
		public byte EffectArg { get; set; }

		public byte TransposedNote { get; set; }
		public byte PreviousTransposedNote { get; set; }

		public byte TransposedInstrument { get; set; }
		public VoicePlayingMode PlayingMode { get; set; }

		public byte CurrentVolume { get; set; }

		public ushort ArpeggioPosition { get; set; }
		public bool ArpeggioEffectNibble { get; set; }

		public sbyte SlideSpeed { get; set; }
		public short SlideValue { get; set; }

		public ushort PortamentoSpeedCounter { get; set; }
		public ushort PortamentoSpeed { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoLevel { get; set; }
		public ushort VibratoPosition { get; set; }

		public ushort AdsrPosition { get; set; }
		public ushort SustainCounter { get; set; }

		public sbyte EnvelopeGeneratorDuration { get; set; }
		public ushort EnvelopeGeneratorPosition { get; set; }

		public ushort LfoPosition { get; set; }

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
