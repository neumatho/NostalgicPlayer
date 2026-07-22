/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Contains information about a module that is about to be loaded
	/// </summary>
	public class SongModule
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SongModule(string playerName, string fileName, string songFormat, SongPatterns songPatterns = null)
		{
			PlayerName = playerName;
			FileName = fileName;
			SongFormat = songFormat;
			Patterns = songPatterns;
		}



		/********************************************************************/
		/// <summary>
		/// Name of the player that will be used to play this module
		/// </summary>
		/********************************************************************/
		public string PlayerName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Full path to the module file
		/// </summary>
		/********************************************************************/
		public string FileName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Format identifier of the module (e.g. "MOD", "XM", "IT")
		/// </summary>
		/********************************************************************/
		public string SongFormat
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Pattern data for the song, or null if the player doesn't support patterns
		/// </summary>
		/********************************************************************/
		public SongPatterns Patterns
		{
			get;
			private set;
		}



		/********************************************************************/
		/// <summary>
		/// Import pattern data into this module
		/// </summary>
		/********************************************************************/
		public void ImportPatterns(SongPatterns patterns)
		{
			Patterns = patterns;
		}
	}
}
