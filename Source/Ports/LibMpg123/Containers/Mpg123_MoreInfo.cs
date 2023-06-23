/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for even more detailed information out of the decoder,
	/// for MPEG layer III only.
	/// This was added to support the frame analyzer by the Lame project and
	/// just follows what was used there before. You know what the fields mean
	/// if you want use this structure
	/// </summary>
	public class Mpg123_MoreInfo
	{
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_double[,,] Xr = new c_double[2, 2, 576];
		/// <summary>
		/// [2][2][SBMAX_1]
		/// </summary>
		public readonly c_double[,,] Sfb = new c_double[2, 2, 22];
		/// <summary>
		/// [2][2][3 * SBMAX_s]
		/// </summary>
		public readonly c_double[,,] Sfb_S = new c_double[2, 2, 3 * 13];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] Qss = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] Big_Values = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,,] Sub_Gain = new c_int[2, 2, 3];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] ScaleFac_Scale = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] PreFlag = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] BlockType = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] Mixed = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] MainBits = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[,] SfBits = new c_int[2, 2];
		/// <summary>
		/// Internal data
		/// </summary>
		public readonly c_int[] Scfsi = new c_int[2];
		/// <summary>
		/// Internal data
		/// </summary>
		public c_int MainData;
		/// <summary>
		/// Internal data
		/// </summary>
		public c_int Padding;
	}
}
