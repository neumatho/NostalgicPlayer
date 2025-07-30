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
	public class Xmp_Envelope
	{
		/// <summary>
		/// Flags
		/// </summary>
		public Xmp_Envelope_Flag Flg { get; internal set; }

		/// <summary>
		/// Number of envelope points
		/// </summary>
		public c_int Npt { get; internal set; }

		/// <summary>
		/// Envelope scaling
		/// </summary>
		public c_int Scl { get; internal set; }

		/// <summary>
		/// Sustain start point
		/// </summary>
		public c_int Sus { get; internal set; }

		/// <summary>
		/// Sustain end point
		/// </summary>
		public c_int Sue { get; internal set; }

		/// <summary>
		/// Loop start point
		/// </summary>
		public c_int Lps { get; internal set; }

		/// <summary>
		/// Loop end point
		/// </summary>
		public c_int Lpe { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public c_short[] Data { get; } = new c_short[Constants.Xmp_Max_Env_Points * 2];
	}
}
