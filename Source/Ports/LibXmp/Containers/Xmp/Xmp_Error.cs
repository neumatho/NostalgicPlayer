/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Error codes
	/// </summary>
	public enum Xmp_Error
	{
		/// <summary>
		/// 
		/// </summary>
		End = 1,

		/// <summary>
		/// Internal error
		/// </summary>
		Internal,

		/// <summary>
		/// Unsupported module format
		/// </summary>
		Format,

		/// <summary>
		/// Error loading file
		/// </summary>
		Load,

		/// <summary>
		/// Error depacking file
		/// </summary>
		Depack,

		/// <summary>
		/// System error
		/// </summary>
		System,

		/// <summary>
		/// Invalid parameter
		/// </summary>
		Invalid,

		/// <summary>
		/// Invalid player state
		/// </summary>
		State
	}
}
