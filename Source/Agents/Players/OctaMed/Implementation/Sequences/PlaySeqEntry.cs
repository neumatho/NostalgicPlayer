/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences
{
	/// <summary>
	/// 
	/// </summary>
	internal class PlaySeqEntry
	{
		private BlockNum num;
		private PSeqCmd pSeqCmd;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlaySeqEntry(BlockNum init)
		{
			num = init;
			pSeqCmd = PSeqCmd.None;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IsCmd()
		{
			return pSeqCmd > 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PSeqCmd GetCmd()
		{
			return pSeqCmd;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetCmd(PSeqCmd cmdNum, BlockNum cmdLevel)
		{
			pSeqCmd = cmdNum;
			num = cmdLevel;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public BlockNum Value => num;
	}
}
