/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.RetroPlayer.RetroPlayerKit.Streams;

namespace Polycode.RetroPlayer.RetroPlayerKit.Containers
{
	/// <summary>
	/// This class holds the information about the current file
	/// </summary>
	public class PlayerFileInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerFileInfo(string fileName, ModuleStream moduleStream)
		{
			FileName = fileName;
			ModuleStream = moduleStream;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the full path to the file
		/// </summary>
		/********************************************************************/
		public string FileName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the stream with the file data
		/// </summary>
		/********************************************************************/
		public ModuleStream ModuleStream
		{
			get;
		}
	}
}
