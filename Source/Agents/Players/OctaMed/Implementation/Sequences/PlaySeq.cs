/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
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
