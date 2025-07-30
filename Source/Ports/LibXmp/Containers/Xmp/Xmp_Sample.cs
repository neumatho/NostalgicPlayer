/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

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
		public ref string Name => ref _Name;
		private string _Name = string.Empty;

		/// <summary>
		/// Sample length
		/// </summary>
		public c_int Len { get; internal set; }

		/// <summary>
		/// Loop start
		/// </summary>
		public c_int Lps { get; internal set; }

		/// <summary>
		/// Loop end
		/// </summary>
		public c_int Lpe { get; internal set; }

		/// <summary>
		/// Flags
		/// </summary>
		public Xmp_Sample_Flag Flg { get; internal set; }

		/// <summary>
		/// Sample data
		/// </summary>
		public ref CPointer<byte> Data => ref _Data;
		private CPointer<byte> _Data;
	}
}
