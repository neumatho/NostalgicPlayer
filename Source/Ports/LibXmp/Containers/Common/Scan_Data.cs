/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Scan_Data : IClearable
	{
		/// <summary>
		/// Reply time in ms
		/// </summary>
		public c_double Time;
		public c_int Row;
		public c_int Ord;
		public c_int Num;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Time = 0;
			Row = 0;
			Ord = 0;
			Num = 0;
		}
	}
}
