/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SubSong_Data
	{
		public c_double Duration;
		public int32_t Start_Row;
		public int32_t Start_Order;
		public int32_t Sequence;
		public int32_t Restart_Row;
		public int32_t Restart_Order;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSong_Data(c_double duration, int32_t start_Row, int32_t start_Order, int32_t sequence, int32_t restart_Row, int32_t restart_Order)
		{
			Duration = duration;
			Start_Row = start_Row;
			Start_Order = start_Order;
			Sequence = sequence;
			Restart_Row = restart_Row;
			Restart_Order = restart_Order;
		}
	}
}
