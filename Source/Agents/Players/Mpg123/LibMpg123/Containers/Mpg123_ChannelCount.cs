/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// They can be combined into one number (3) to indicate mono and stereo
	/// </summary>
	[Flags]
	internal enum Mpg123_ChannelCount
	{
		Mono = 1,
		Stereo = 2
	}
}
