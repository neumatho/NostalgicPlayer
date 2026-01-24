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
	public class Xmp_Channel
	{
		/// <summary>
		/// Channel pan (0x80 is center)
		/// </summary>
		public c_int Pan;

		/// <summary>
		/// Channel volume
		/// </summary>
		public c_int Vol;

		/// <summary>
		/// Channel flags
		/// </summary>
		public Xmp_Channel_Flag Flg;
	}
}
