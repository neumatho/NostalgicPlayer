/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFx.Containers
{
	/// <summary>
	/// Information about a single playing channel
	/// </summary>
	internal class Channel : IDeepCloneable<Channel>
	{
		public uint PatternData { get; set; }
		public short SampleNumber { get; set; }
		public sbyte[] Sample { get; set; }
		public uint SampleLen { get; set; }
		public uint LoopStart { get; set; }
		public uint LoopLength { get; set; }
		public ushort CurrentNote { get; set; }
		public ushort Volume { get; set; }
		public short StepValue { get; set; }
		public ushort StepNote { get; set; }
		public ushort StepEndNote { get; set; }
		public ushort SlideControl { get; set; }
		public bool SlideDirection { get; set; }
		public ushort SlideParam { get; set; }
		public ushort SlidePeriod { get; set; }
		public ushort SlideSpeed { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Channel MakeDeepClone()
		{
			return (Channel)MemberwiseClone();
		}
	}
}
