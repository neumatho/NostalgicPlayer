/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Ord_Data
	{
		public c_int Speed { get; set; }
		public c_int Bpm { get; set; }
		public c_int Gvl { get; set; }
		public c_int Time { get; set; }	// TODO: double
		public c_int Start_Row { get; set; }
		public c_int St26_Speed { get; set; }
	}
}
