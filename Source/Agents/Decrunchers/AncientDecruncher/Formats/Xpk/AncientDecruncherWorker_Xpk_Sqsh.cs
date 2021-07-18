/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Xpk
{
	/// <summary>
	/// Can decrunch XPK (SQSH) packed files
	/// </summary>
	internal class AncientDecruncherWorker_Xpk_Sqsh : AncientDecruncherWorker_Xpk
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AncientDecruncherWorker_Xpk_Sqsh(string agentName) : base(agentName)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return a stream holding the depacked data
		/// </summary>
		/********************************************************************/
		public override DepackerStream OpenStream(Stream packedDataStream)
		{
			return new Xpk_SqshStream(agentName, packedDataStream);
		}



		/********************************************************************/
		/// <summary>
		/// Return the packer ID
		/// </summary>
		/********************************************************************/
		protected override uint PackerId => 0x53515348;		// SQSH
	}
}
