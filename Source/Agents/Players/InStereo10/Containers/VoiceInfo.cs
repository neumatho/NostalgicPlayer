﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.InStereo10.Containers
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
		public byte Arpeggio;
		public Effect Effect;
		public byte EffectArg;

		public byte UseBuffer;
		public sbyte[] SynthSample1 = new sbyte[256];
		public sbyte[] SynthSample2 = new sbyte[256];

		public byte TransposedNote;
		public byte PreviousTransposedNote;

		public byte TransposedInstrument;

		public byte CurrentVolume;

		public sbyte SlideSpeed;
		public short SlideIncrement;

		public bool PortamentoEnabled;
		public sbyte PortamentoSpeed;
		public short PortamentoSpeedCounter;

		public byte VibratoDelay;
		public byte VibratoPosition;

		public bool AdsrEnabled;
		public ushort AdsrPosition;

		public EnvelopeGeneratorCounterMode EnvelopeGeneratorCounterMode;
		public ushort EnvelopeGeneratorCounterPosition;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.SynthSample1 = ArrayHelper.CloneArray(SynthSample1);
			clone.SynthSample2 = ArrayHelper.CloneArray(SynthSample2);

			return clone;
		}
	}
}
