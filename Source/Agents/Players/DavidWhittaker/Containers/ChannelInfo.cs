/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public int ChannelNumber;

		public uint[] PositionList;
		public ushort CurrentPosition;
		public ushort RestartPosition;

		public byte[] TrackData;
		public int TrackDataPosition;

		public Sample CurrentSampleInfo;
		public byte Note;
		public sbyte Transpose;

		public bool EnableHalfVolume;

		public ushort Speed;
		public ushort SpeedCounter;

		public byte[] ArpeggioList;
		public int ArpeggioListPosition;

		public byte[] EnvelopeList;
		public int EnvelopeListPosition;
		public byte EnvelopeSpeed;
		public sbyte EnvelopeCounter;

		public bool SlideEnabled;
		public sbyte SlideSpeed;
		public byte SlideCounter;
		public short SlideValue;

		public sbyte VibratoDirection;
		public byte VibratoSpeed;
		public byte VibratoValue;
		public byte VibratoMaxValue;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ChannelInfo MakeDeepClone()
		{
			return (ChannelInfo)MemberwiseClone();
		}
	}
}
