/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// This class holds the information about sub-songs
	/// </summary>
	public class SubSongInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSongInfo(int number, int defaultStartSong)
		{
			Number = number;
			DefaultStartSong = defaultStartSong;
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
	}
}
