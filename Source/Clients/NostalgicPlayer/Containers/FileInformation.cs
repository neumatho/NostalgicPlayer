/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.NostalgicPlayer.Containers
{
	/// <summary>
	/// This class holds information about a single file
	/// </summary>
	public class FileInformation
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileInformation(string fullPath)
		{
			FullPath = fullPath;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the full path to the file
		/// </summary>
		/********************************************************************/
		public string FullPath
		{
			get;
		}
	}
}
