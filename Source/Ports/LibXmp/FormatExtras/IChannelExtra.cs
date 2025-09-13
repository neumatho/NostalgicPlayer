/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IChannelExtra : IDeepCloneable<IChannelExtra>
	{
		IChannelExtraInfo Channel_Extras { get; }
		void SetChannel(Channel_Data xc);

		void Release_Channel_Extras();
	}
}
