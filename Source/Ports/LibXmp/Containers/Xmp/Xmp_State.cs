/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public enum Xmp_State
	{
		/// <summary>
		/// Context created
		/// </summary>
		Unloaded = 0,

		/// <summary>
		/// Module loaded
		/// </summary>
		Loaded = 1,

		/// <summary>
		/// Module playing
		/// </summary>
		Playing = 2
	}
}
