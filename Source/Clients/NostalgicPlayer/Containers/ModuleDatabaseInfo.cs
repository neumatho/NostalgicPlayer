/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// The different kind of information stored in the database
	/// </summary>
	public class ModuleDatabaseInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleDatabaseInfo(TimeSpan duration, int listenCount, DateTime lastLoaded)
		{
			Duration = duration;
			ListenCount = listenCount;
			LastLoaded = lastLoaded;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the duration of the module
		/// </summary>
		/********************************************************************/
		public TimeSpan Duration
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of times this module has been loaded
		/// </summary>
		/********************************************************************/
		public int ListenCount
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the last time this module was loaded
		/// </summary>
		/********************************************************************/
		public DateTime LastLoaded
		{
			get;
		}
	}
}
