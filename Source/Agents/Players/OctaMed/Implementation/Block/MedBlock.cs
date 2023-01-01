/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block
{
	/// <summary>
	/// MED block class
	/// </summary>
	internal class MedBlock
	{
		private string name;

		private readonly MedNote[,] grid;
		private MedCmd[][,] cmdPages;

		private readonly LineNum numLines;
		private readonly TrackNum numTracks;
		private PageNum numCmdPages;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MedBlock(LineNum lines, TrackNum tracks, PageNum pages = 1)
		{
			// Remember the arguments
			numLines = lines;
			numTracks = tracks;
			numCmdPages = pages;

			grid = null;
			cmdPages = null;

			grid = new MedNote[lines, tracks];
			cmdPages = new MedCmd[pages][,];

			for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
			{
				for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
					grid[lineCnt, trkCnt] = new MedNote();
			}

			for (PageNum cnt = 0; cnt < pages; cnt++)
			{
				cmdPages[cnt] = new MedCmd[lines, tracks];

				for (LineNum lineCnt = 0; lineCnt < lines; lineCnt++)
				{
					for (TrackNum trkCnt = 0; trkCnt < tracks; trkCnt++)
						cmdPages[cnt][lineCnt, trkCnt] = new MedCmd();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the block name
		/// </summary>
		/********************************************************************/
		public void SetName(byte[] newName)
		{
			name = EncoderCollection.Amiga.GetString(newName);
		}



		/********************************************************************/
		/// <summary>
		/// Set the number of pages in the block
		/// </summary>
		/********************************************************************/
		public void SetCmdPages(PageNum numOfPages)
		{
			if ((Pages() != numOfPages) && (numOfPages > 0))
			{
				MedCmd[][,] newCmdPages = new MedCmd[numOfPages][,];

				for (PageNum cnt = 0; cnt < Math.Min(numOfPages, Pages()); cnt++)
					newCmdPages[cnt] = cmdPages[cnt];

				if (numOfPages > Pages())
				{
					for (PageNum cnt = Pages(); cnt < numOfPages; cnt++)
					{
						newCmdPages[cnt] = new MedCmd[Lines(), Tracks()];

						for (LineNum lineCnt = 0; lineCnt < Lines(); lineCnt++)
						{
							for (TrackNum trkCnt = 0; trkCnt < Tracks(); trkCnt++)
								newCmdPages[cnt][lineCnt, trkCnt] = new MedCmd();
						}
					}
				}

				cmdPages = newCmdPages;
				numCmdPages = numOfPages;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the note at the position given
		/// </summary>
		/********************************************************************/
		public MedNote Note(LineNum line, TrackNum track)
		{
			return grid[line, track];
		}



		/********************************************************************/
		/// <summary>
		/// Return the effect command at the position given
		/// </summary>
		/********************************************************************/
		public MedCmd Cmd(LineNum line, TrackNum track, PageNum page)
		{
			return cmdPages[page][line, track];
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of lines in the block
		/// </summary>
		/********************************************************************/
		public LineNum Lines()
		{
			return numLines;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of tracks in the block
		/// </summary>
		/********************************************************************/
		public TrackNum Tracks()
		{
			return numTracks;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of pages in the block
		/// </summary>
		/********************************************************************/
		public PageNum Pages()
		{
			return numCmdPages;
		}
	}
}
