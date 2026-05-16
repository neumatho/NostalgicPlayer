/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds the information about sub-songs
	/// </summary>
	public class SubSongInfo
	{
		private readonly string[] titles;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSongInfo(int number, int defaultStartSong)
		{
			Number = number;
			DefaultStartSong = defaultStartSong;
			titles = null;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSongInfo(int number, int defaultStartSong, IEnumerable<string> songTitles)
		{
			Number = number;
			DefaultStartSong = defaultStartSong;
			titles = songTitles.ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of sub-songs
		/// </summary>
		/********************************************************************/
		public int Number
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the default song number to start playing where the first
		/// song is 0
		/// </summary>
		/********************************************************************/
		public int DefaultStartSong
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the song title for the given song if any
		/// </summary>
		/********************************************************************/
		public string GetSongTitle(int songNumber)
		{
			if ((titles == null) || (songNumber >= titles.Length))
				return string.Empty;

			return titles[songNumber] ?? string.Empty;
		}
	}
}
