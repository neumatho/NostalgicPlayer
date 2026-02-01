/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Directory entry types
	/// </summary>
	public enum AvIoDirEntryType
	{
		/// <summary>
		/// 
		/// </summary>
		Unknown,

		/// <summary>
		/// 
		/// </summary>
		Block_Device,

		/// <summary>
		/// 
		/// </summary>
		Character_Device,

		/// <summary>
		/// 
		/// </summary>
		Directory,

		/// <summary>
		/// 
		/// </summary>
		Named_Pipe,

		/// <summary>
		/// 
		/// </summary>
		Symbolic_Link,

		/// <summary>
		/// 
		/// </summary>
		Socket,

		/// <summary>
		/// 
		/// </summary>
		File,

		/// <summary>
		/// 
		/// </summary>
		Server,

		/// <summary>
		/// 
		/// </summary>
		Share,

		/// <summary>
		/// 
		/// </summary>
		Workgroup
	}
}
