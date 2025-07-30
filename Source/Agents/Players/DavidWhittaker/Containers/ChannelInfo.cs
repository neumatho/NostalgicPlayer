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
		public int ChannelNumber { get; set; }

		public uint[] PositionList { get; set; }
		public ushort CurrentPosition { get; set; }
		public ushort RestartPosition { get; set; }

		public byte[] TrackData { get; set; }
		public int TrackDataPosition { get; set; }

		public Sample CurrentSampleInfo { get; set; }
		public byte Note { get; set; }
		public sbyte Transpose { get; set; }

		public bool EnableHalfVolume { get; set; }

		public ushort Speed { get; set; }
		public ushort SpeedCounter { get; set; }

		public byte[] ArpeggioList { get; set; }
		public int ArpeggioListPosition { get; set; }

		public byte[] EnvelopeList { get; set; }
		public int EnvelopeListPosition { get; set; }
		public byte EnvelopeSpeed { get; set; }
		public sbyte EnvelopeCounter { get; set; }

		public bool SlideEnabled { get; set; }
		public sbyte SlideSpeed { get; set; }
		public byte SlideCounter { get; set; }
		public short SlideValue { get; set; }

		public sbyte VibratoDirection { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoValue { get; set; }
		public byte VibratoMaxValue { get; set; }

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
