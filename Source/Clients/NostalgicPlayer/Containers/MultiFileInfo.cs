/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
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
			Plain
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
		/// Holds the file name
		/// </summary>
		/********************************************************************/
		public string FileName
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
	}
}
