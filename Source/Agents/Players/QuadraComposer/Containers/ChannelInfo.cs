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
		public TrackLine TrackLine;
		public uint Loop;
		public uint LoopLength;
		public ushort Period;
		public ushort Volume;
		public uint Length;
		public uint Start;
		public short NoteNr;
		public ushort WantedPeriod;
		public bool PortDirection;
		public byte VibratoWave;
		public byte GlissandoControl;
		public byte VibratoCommand;
		public ushort VibratoPosition;
		public byte TremoloWave;
		public byte TremoloCommand;
		public ushort TremoloPosition;
		public byte SampleOffset;
		public byte Retrig;
		public ushort PortSpeed;
		public byte FineTune;
		public sbyte[] SampleData;

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
