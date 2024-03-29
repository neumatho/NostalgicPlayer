﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
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
		public void Int123_Init_Icy(Icy_Meta icy)
		{
			icy.Data = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Clear_Icy(Icy_Meta icy)
		{
			Int123_Init_Icy(icy);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Reset_Icy(Icy_Meta icy)
		{
			Int123_Clear_Icy(icy);
			Int123_Init_Icy(icy);
		}
	}
}
