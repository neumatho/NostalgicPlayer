/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanSubSong : ScanBlock
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void DoSubSong(SubSong ss)
		{
			SubSongOperation(ss);

			for (BlockNum blk = 0; blk < ss.NumBlocks(); blk++)
				DoBlock(ss.Block(blk));
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void SubSongOperation(SubSong ss)
		{
		}
		#endregion
	}
}
