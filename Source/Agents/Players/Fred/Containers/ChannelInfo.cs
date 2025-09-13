/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Fred.Containers
{
	/// <summary>
	/// Channel information
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public int ChanNum { get; set; }
		public sbyte[] PositionTable { get; set; }
		public byte[] TrackTable { get; set; }
		public ushort Position { get; set; }
		public ushort TrackPosition { get; set; }
		public ushort TrackDuration { get; set; }
		public byte TrackNote { get; set; }
		public ushort TrackPeriod { get; set; }
		public short TrackVolume { get; set; }
		public Instrument Instrument { get; set; }
		public VibFlags VibFlags { get; set; }
		public byte VibDelay { get; set; }
		public sbyte VibSpeed { get; set; }
		public sbyte VibAmpl { get; set; }
		public sbyte VibValue { get; set; }
		public bool PortRunning { get; set; }
		public ushort PortDelay { get; set; }
		public ushort PortLimit { get; set; }
		public byte PortTargetNote { get; set; }
		public ushort PortStartPeriod { get; set; }
		public short PeriodDiff { get; set; }
		public ushort PortCounter { get; set; }
		public ushort PortSpeed { get; set; }
		public EnvelopeState EnvState { get; set; }
		public byte SustainDelay { get; set; }
		public byte ArpPosition { get; set; }
		public byte ArpSpeed { get; set; }
		public bool PulseWay { get; set; }
		public byte PulsePosition { get; set; }
		public byte PulseDelay { get; set; }
		public byte PulseSpeed { get; set; }
		public byte PulseShot { get; set; }
		public bool BlendWay { get; set; }
		public ushort BlendPosition { get; set; }
		public byte BlendDelay { get; set; }
		public byte BlendShot { get; set; }
		public sbyte[] SynthSample { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ChannelInfo MakeDeepClone()
		{
			ChannelInfo clone = (ChannelInfo)MemberwiseClone();

			clone.SynthSample = ArrayHelper.CloneArray(SynthSample);

			return clone;
		}
	}
}
