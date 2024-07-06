/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Hippel.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public byte[] EnvelopeTable;
		public int EnvelopePosition;
		public int OriginalEnvelopeNumber;
		public int CurrentEnvelopeNumber;

		public byte[] FrequencyTable;
		public int FrequencyPosition;
		public int OriginalFrequencyNumber;
		public int CurrentFrequencyNumber;

		public uint NextPosition;

		public byte[] Track;
		public uint TrackPosition;
		public sbyte TrackTranspose;
		public sbyte EnvelopeTranspose;
		public byte CurrentTrackNumber;

		public byte Transpose;
		public byte CurrentNote;
		public byte CurrentInfo;
		public byte PreviousInfo;

		public byte Sample;

		public byte Tick;

		public sbyte CosoCounter;
		public sbyte CosoSpeed;

		public byte Volume;
		public byte EnvelopeCounter;
		public byte EnvelopeSpeed;
		public byte EnvelopeSustain;

		public byte VibratoFlag;
		public byte VibratoSpeed;
		public byte VibratoDelay;
		public byte VibratoDepth;
		public byte VibratoDelta;

		public uint PortaDelta;

		public bool Slide;
		public byte SlideSample;
		public int SlideEndPosition;
		public ushort SlideLoopPosition;
		public ushort SlideLength;
		public short SlideDelta;
		public sbyte SlideCounter;
		public byte SlideSpeed;
		public bool SlideActive;
		public bool SlideDone;

		public byte VolumeFade;
		public byte VolumeVariationDepth;
		public byte VolumeVariation;

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
