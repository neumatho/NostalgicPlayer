/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Oktalyzer.Containers
{
	/// <summary>
	/// ChannelInfo structure
	/// </summary>
	internal class ChannelInfo : IDeepCloneable<ChannelInfo>
	{
		public byte CurrNote { get; set; }
		public short CurrPeriod { get; set; }
		public uint ReleaseStart { get; set; }
		public uint ReleaseLength { get; set; }

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
