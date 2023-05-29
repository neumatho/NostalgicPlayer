/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Al_Table
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Al_Table(c_short bits, c_short d)
		{
			Bits = bits;
			D = d;
		}

		public c_short Bits;
		public c_short D;
	}
}
