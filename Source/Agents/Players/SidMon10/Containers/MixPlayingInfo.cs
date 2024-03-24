/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SidMon10.Containers
{
	internal class MixPlayingInfo : IDeepCloneable<MixPlayingInfo>
	{
		public uint Counter;
		public uint Position;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public MixPlayingInfo MakeDeepClone()
		{
			return (MixPlayingInfo)MemberwiseClone();
		}
	}
}
