/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for even more detailed information out of the decoder,
	/// for MPEG layer III only.
	/// This was added to support the frame analyzer by the Lame project and
	/// just follows what was used there before. You know what the fields mean
	/// if you want use this structure
	/// </summary>
	internal class Mpg123_MoreInfo
	{
		public readonly c_double[,,] Xr = new c_double[2, 2, 576];	// < Internal data
		public readonly c_double[,,] Sfb = new c_double[2, 2, 22];	// < [2][2][SBMAX_1]
		public readonly c_double[,,] Sfb_S = new c_double[2, 2, 3 * 13];// < [2][2][3 * SBMAX_s]
		public readonly c_int[,] Qss = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] Big_Values = new c_int[2, 2];	// < Internal data
		public readonly c_int[,,] Sub_Gain = new c_int[2, 2, 3];	// < Internal data
		public readonly c_int[,] ScaleFac_Scale = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] PreFlag = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] BlockType = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] Mixed = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] MainBits = new c_int[2, 2];	// < Internal data
		public readonly c_int[,] SfBits = new c_int[2, 2];	// < Internal data
		public readonly c_int[] Scfsi = new c_int[2];	// < Internal data
		public c_int MainData;			// < Internal data
		public c_int Padding;			// < Internal data
	}
}
