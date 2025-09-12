/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public byte SpeedCounter { get; set; }
		public byte CurrentSpeed { get; set; }
		public byte MeasureCounter { get; set; }
		public byte CurrentPosition { get; set; }
		public byte LoopPosition { get; set; }
		public byte EndPosition { get; set; }
		public byte CurrentRowPosition { get; set; }
		public byte NumberOfRows { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo)MemberwiseClone();
		}
	}
}
