/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.QuadraComposer.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public TrackLine TrackLine { get; set; }
		public uint Loop { get; set; }
		public uint LoopLength { get; set; }
		public ushort Period { get; set; }
		public ushort Volume { get; set; }
		public uint Length { get; set; }
		public uint Start { get; set; }
		public short NoteNr { get; set; }
		public ushort WantedPeriod { get; set; }
		public bool PortDirection { get; set; }
		public byte VibratoWave { get; set; }
		public byte GlissandoControl { get; set; }
		public byte VibratoCommand { get; set; }
		public ushort VibratoPosition { get; set; }
		public byte TremoloWave { get; set; }
		public byte TremoloCommand { get; set; }
		public ushort TremoloPosition { get; set; }
		public byte SampleOffset { get; set; }
		public byte Retrig { get; set; }
		public ushort PortSpeed { get; set; }
		public byte FineTune { get; set; }
		public sbyte[] SampleData { get; set; }

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
