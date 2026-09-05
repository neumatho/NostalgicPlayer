/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers
{
	/// <summary>
	/// Possible return values for openmpt::probe_file_header()
	/// </summary>
	public enum Probe_File_Header_Result
	{
		/// <summary>
		/// 
		/// </summary>
		Success = 1,

		/// <summary>
		/// 
		/// </summary>
		Failure = 0,

		/// <summary>
		/// 
		/// </summary>
		WantMoreData = -1
	}
}
