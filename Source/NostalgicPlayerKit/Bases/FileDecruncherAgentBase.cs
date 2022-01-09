/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for file decruncher agents
	/// </summary>
	public abstract class FileDecruncherAgentBase : IFileDecruncherAgent
	{
		#region IFileDecruncherAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(Stream crunchedDataStream);



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the decrunched data
		/// </summary>
		/********************************************************************/
		public abstract DecruncherStream OpenStream(Stream crunchedDataStream);
		#endregion
	}
}
