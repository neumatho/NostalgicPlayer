/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers
{
	/// <summary>
	/// Holds information of the detected format
	/// </summary>
	public class Probe_File_Header_Info
	{
		/// <summary>
		/// Unique ID for the format
		/// </summary>
		public Guid Id { get; init; }
	}
}
