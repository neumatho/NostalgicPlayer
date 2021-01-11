/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Containers
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
