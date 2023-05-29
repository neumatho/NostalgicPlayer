/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
{
	/// <summary>
	/// Puny code to pretend for a serious ICY data structure
	/// </summary>
	internal class Icy
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Init_Icy(Icy_Meta icy)
		{
			icy.Data = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear_Icy(Icy_Meta icy)
		{
			Init_Icy(icy);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset_Icy(Icy_Meta icy)
		{
			Clear_Icy(icy);
			Init_Icy(icy);
		}
	}
}
