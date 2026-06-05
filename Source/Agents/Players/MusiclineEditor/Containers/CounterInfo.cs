/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Counter structure
	/// </summary>
	internal class CounterInfo : IDeepCloneable<CounterInfo>
	{
		public ushort Counter { get; set; }
		public short Speed { get; set; }
		public ushort Repeat { get; set; }
		public ushort RepeatEnd { get; set; }
		public short Turns { get; set; }
		public ushort Delay { get; set; }
		public bool Step { get; set; }
		public ushort SaveCounter { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public CounterInfo MakeDeepClone()
		{
			return (CounterInfo)MemberwiseClone();
		}
	}
}
