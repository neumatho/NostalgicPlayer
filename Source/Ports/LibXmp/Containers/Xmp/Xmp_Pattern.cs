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
	public class Xmp_Pattern
	{
		/// <summary>
		/// Number of rows
		/// </summary>
		public c_int Rows { get; internal set; }

		/// <summary>
		/// Track index
		/// </summary>
		public c_int[] Index { get; internal set; }
	}
}
