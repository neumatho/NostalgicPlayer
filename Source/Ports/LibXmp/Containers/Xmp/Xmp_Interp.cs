/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Interpolation types
	/// </summary>
	public enum Xmp_Interp
	{
		/// <summary>
		/// Nearest neighbor
		/// </summary>
		Nearest,

		/// <summary>
		/// Linear (default)
		/// </summary>
		Linear,

		/// <summary>
		/// Cubic spline
		/// </summary>
		Spline
	}
}
