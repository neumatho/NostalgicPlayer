/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers
{
	/// <summary>
	/// Possible values for openmpt::probe_file_header() flags parameter
	/// </summary>
	[Flags]
	public enum Probe_File_Header_Flags : uint64_t
	{
		/// <summary>
		/// Probe for module formats
		/// </summary>
		Modules2 = 0x1,

		/// <summary>
		/// Probe for module-specific container formats
		/// </summary>
		Containers2 = 0x2,

		/// <summary>
		/// Probe for the default set of formats
		/// </summary>
		Default2 = Modules2 | Containers2,

		/// <summary>
		/// Probe for no formats
		/// </summary>
		None2 = 0x0
	}
}
