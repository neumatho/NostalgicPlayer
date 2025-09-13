/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte[] EnvelopeTable { get; set; }
		public int EnvelopePosition { get; set; }
		public int OriginalEnvelopeNumber { get; set; }
		public int CurrentEnvelopeNumber { get; set; }

		public byte[] FrequencyTable { get; set; }
		public int FrequencyPosition { get; set; }
		public int OriginalFrequencyNumber { get; set; }
		public int CurrentFrequencyNumber { get; set; }

		public uint NextPosition { get; set; }

		public byte[] Track { get; set; }
		public uint TrackPosition { get; set; }
		public sbyte TrackTranspose { get; set; }
		public sbyte EnvelopeTranspose { get; set; }
		public byte CurrentTrackNumber { get; set; }

		public byte Transpose { get; set; }
		public byte CurrentNote { get; set; }
		public byte CurrentInfo { get; set; }
		public byte PreviousInfo { get; set; }

		public byte Sample { get; set; }

		public byte Tick { get; set; }

		public sbyte CosoCounter { get; set; }
		public sbyte CosoSpeed { get; set; }

		public byte Volume { get; set; }
		public byte EnvelopeCounter { get; set; }
		public byte EnvelopeSpeed { get; set; }
		public byte EnvelopeSustain { get; set; }

		public byte VibratoFlag { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoDelay { get; set; }
		public byte VibratoDepth { get; set; }
		public byte VibratoDelta { get; set; }

		public uint PortaDelta { get; set; }

		public bool Slide { get; set; }
		public byte SlideSample { get; set; }
		public int SlideEndPosition { get; set; }
		public ushort SlideLoopPosition { get; set; }
		public ushort SlideLength { get; set; }
		public short SlideDelta { get; set; }
		public sbyte SlideCounter { get; set; }
		public byte SlideSpeed { get; set; }
		public bool SlideActive { get; set; }
		public bool SlideDone { get; set; }

		public byte VolumeFade { get; set; }
		public byte VolumeVariationDepth { get; set; }
		public byte VolumeVariation { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.EnvelopeTable = ArrayHelper.CloneArray(EnvelopeTable);

			return clone;
		}
	}
}
