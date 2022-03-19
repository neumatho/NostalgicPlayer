/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanTrack
	{
		protected InstNum lastINum;		// Last instrument number (for instr. 0)

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void DoTrack(MedBlock blk, TrackNum trkNum)
		{
			DoRange(blk, trkNum, 0, trkNum, blk.Lines() - 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void DoRange(MedBlock blk, TrackNum st, LineNum sl, TrackNum et, LineNum el)
		{
			for (TrackNum trk = st; (trk <= et) && (trk < blk.Tracks()); trk++)
			{
				lastINum = 0;

				for (LineNum ln = sl; (ln <= el) && (ln < blk.Lines()); ln++)
				{
					MedNote note = blk.Note(ln, trk);

					if (note.InstrNum != 0)
						lastINum = note.InstrNum;

					NoteOperation(note);

					for (PageNum pg = 0; pg < blk.Pages(); pg++)
					{
						CmdOperation(blk.Cmd(ln, trk, pg));
						NoteCmdOperation(note, blk.Cmd(ln, trk, pg));
					}
				}
			}
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void CmdOperation(MedCmd cmd)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void NoteOperation(MedNote note)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void NoteCmdOperation(MedNote note, MedCmd cmd)
		{
		}
		#endregion
	}
}
