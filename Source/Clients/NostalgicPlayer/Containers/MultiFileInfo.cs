/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// This class holds information about a item in the multi file
	/// </summary>
	public class MultiFileInfo
	{
		/// <summary>
		/// The different types that can be stored in the list
		/// </summary>
		public enum FileType
		{
			/// <summary>
			/// Just a plain file
			/// </summary>
			Plain,

			/// <summary>
			/// File is stored inside an archive
			/// </summary>
			Archive,

			/// <summary>
			/// A network URL
			/// </summary>
			Url,

			/// <summary>
			/// An Audius track
			/// </summary>
			Audius
		}

		/********************************************************************/
		/// <summary>
		/// Holds the type of the file
		/// </summary>
		/********************************************************************/
		public FileType Type
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the source, e.g. the full path to the file or the URL
		/// </summary>
		/********************************************************************/
		public string Source
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the display name for the source
		/// </summary>
		/********************************************************************/
		public string DisplayName
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the time to play the file if available
		/// </summary>
		/********************************************************************/
		public TimeSpan? PlayTime
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the default sub-song if set
		/// </summary>
		/********************************************************************/
		public int? DefaultSubSong
		{
			get; set;
		}
	}
}
