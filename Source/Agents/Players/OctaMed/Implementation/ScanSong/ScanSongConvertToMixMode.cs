/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanSongConvertToMixMode : ScanSong
	{
		private static readonly sbyte[] panVals = { -16, 16, 16, -16, -16, 16, 16, -16 };

		private readonly bool[] transpInstr = new bool[Constants.MaxInstr + 1];
		private readonly int[] iTrans = new int[Constants.MaxInstr + 1];
		private readonly bool[] isMidi = new bool[Constants.MaxInstr + 1];
		private readonly bool[] isMultiOctave = new bool[Constants.MaxInstr + 1];
		private bool type0;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public virtual void Do(Song sg, bool isType0)
		{
			type0 = isType0;

			// Check which instruments will be transposed...
			transpInstr[0] = true;
			iTrans[0] = 0;

			for (uint cnt = 1; cnt <= Constants.MaxInstr; cnt++)
			{
				Instr i = sg.GetInstr(cnt - 1);

				if (!sg.SampleSlotUsed(cnt - 1))
					transpInstr[cnt] = true;
				else
				{
					Sample sample = sg.GetSample(cnt - 1);

					if (!sample.IsSynthSound() || (sample.GetLength() != 0))
						transpInstr[cnt] = true;
					else
						transpInstr[cnt] = false;

					isMultiOctave[cnt] = sample.IsMultiOctave();
				}

				iTrans[cnt] = i.GetTransp();
				isMidi[cnt] = i.IsMidi();
			}

			DoSong(sg);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void NoteOperation(MedNote note)
		{
			if ((note.NoteNum != 0) && (note.NoteNum <= (0x7f - 24)) && transpInstr[lastINum])
			{
				if (isMidi[lastINum])
				{
					if (type0)
						note.NoteNum += 24;		// For MMD0 mods, transpose 2 oct, otherwise nothing
				}
				else
				{
					// Kludge for broken 4-ch modules that use x-4/x-5/x-6... as x-3
					if (!isMultiOctave[lastINum])
					{
						while (note.NoteNum + iTrans[lastINum] > 3 * 12)
							note.NoteNum -= 12;
					}

					// The actual transposition up
					note.NoteNum += 24;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void SubSongOperation(SubSong ss)
		{
			// For each sub-song, set panning & stereo mode
			ss.SetStereo(true);

			for (int cnt = 0; cnt < 8; cnt++)
				ss.SetTrackPan(cnt, panVals[cnt]);
		}
		#endregion
	}
}
