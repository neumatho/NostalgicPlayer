/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Hold information about a single format
	/// </summary>
	public class Xmp_Format_Info
	{
		/// <summary>
		/// A unique identifier for the format
		/// </summary>
		public Guid Id;

		/// <summary>
		/// The name of the format
		/// </summary>
		public string Name;

		/// <summary>
		/// A description about the format
		/// </summary>
		public string Description;
	}
}
