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
		public Xmp_Envelope_Flag Flg;

		/// <summary>
		/// Number of envelope points
		/// </summary>
		public c_int Npt;

		/// <summary>
		/// Envelope scaling
		/// </summary>
		public c_int Scl;

		/// <summary>
		/// Sustain start point
		/// </summary>
		public c_int Sus;

		/// <summary>
		/// Sustain end point
		/// </summary>
		public c_int Sue;

		/// <summary>
		/// Loop start point
		/// </summary>
		public c_int Lps;

		/// <summary>
		/// Loop end point
		/// </summary>
		public c_int Lpe;

		/// <summary>
		/// 
		/// </summary>
		public c_short[] Data = new c_short[Constants.Xmp_Max_Env_Points * 2];
	}
}
