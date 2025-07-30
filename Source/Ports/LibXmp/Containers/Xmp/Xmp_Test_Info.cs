/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Test_Info
	{
		/// <summary>
		/// Module title
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		/// Module format
		/// </summary>
		public string Type { get; internal set; }

		/// <summary>
		/// Unique ID for the format
		/// </summary>
		public Guid Id { get; internal set; }
	}
}
