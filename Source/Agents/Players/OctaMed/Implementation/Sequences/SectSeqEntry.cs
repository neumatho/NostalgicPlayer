﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences
{
	/// <summary>
	/// 
	/// </summary>
	internal class SectSeqEntry
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SectSeqEntry(PSeqNum init)
		{
			Value = init;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the value
		/// </summary>
		/********************************************************************/
		public PSeqNum Value
		{
			get; set;
		}
	}
}
