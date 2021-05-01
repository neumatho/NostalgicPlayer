/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// This loader can open just standard files in a file system
	/// </summary>
	public class NormalFileLoader : FileLoaderBase
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NormalFileLoader(string fileName) : base(fileName)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will try to open the file given
		/// </summary>
		/********************************************************************/
		protected override Stream OpenFile(string fileName)
		{
			return new FileStream(fileName, FileMode.Open, FileAccess.Read);
		}
	}
}
