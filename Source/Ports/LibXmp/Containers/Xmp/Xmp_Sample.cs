/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Sample
	{
		/// <summary>
		/// Sample name
		/// </summary>
		public string Name = string.Empty;

		/// <summary>
		/// Sample length
		/// </summary>
		public c_int Len;

		/// <summary>
		/// Loop start
		/// </summary>
		public c_int Lps;

		/// <summary>
		/// Loop end
		/// </summary>
		public c_int Lpe;

		/// <summary>
		/// Flags
		/// </summary>
		public Xmp_Sample_Flag Flg;

		/// <summary>
		/// Sample data
		/// </summary>
		public CPointer<byte> Data;
	}
}
