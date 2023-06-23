/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// They can be combined into one number (3) to indicate mono and stereo
	/// </summary>
	[Flags]
	public enum Mpg123_ChannelCount
	{
		/// <summary></summary>
		Mono = 1,
		/// <summary></summary>
		Stereo = 2
	}
}
