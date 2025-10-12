/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Track : IClearable
	{
		/// <summary>
		/// Number of rows
		/// </summary>
		public c_int Rows { get; internal set; }

		/// <summary>
		/// Event data
		/// </summary>
		public Xmp_Event[] Event { get; internal set; }

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Rows = 0;

			foreach (Xmp_Event e in Event)
				e.Clear();
		}
	}
}
