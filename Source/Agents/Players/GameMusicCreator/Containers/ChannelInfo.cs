/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.GameMusicCreator.Containers
{
	/// <summary>
	/// Contains work information for one channel
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public int Slide { get; set; }
		public ushort Period { get; set; }
		public ushort Volume { get; set; }

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
