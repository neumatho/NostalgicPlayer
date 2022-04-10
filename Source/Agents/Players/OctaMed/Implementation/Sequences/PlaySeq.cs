/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences
{
	/// <summary>
	/// 
	/// </summary>
	internal class PlaySeq : List<PlaySeqEntry>
	{
		private string name;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetName(byte[] newName)
		{
			name = EncoderCollection.Amiga.GetString(newName);
		}
	}
}
