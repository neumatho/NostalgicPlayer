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
	internal class NewHuff
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public NewHuff(c_uint linBits, c_short[] table)
		{
			LinBits = linBits;
			Table = table;
		}

		public c_uint LinBits;
		public c_short[] Table;
	}
}
