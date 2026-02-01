/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum UrlProtocolFlag
	{
		/// <summary>
		/// The protocol name can be the first part of a nested protocol scheme
		/// </summary>
		Nested_Scheme = 1,

		/// <summary>
		/// The protocol uses network
		/// </summary>
		Network = 2
	}
}
