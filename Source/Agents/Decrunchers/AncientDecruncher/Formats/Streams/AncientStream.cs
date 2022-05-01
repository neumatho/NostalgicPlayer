/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams
{
	/// <summary>
	/// Base class for all decruncher streams
	/// </summary>
	internal abstract class AncientStream : DecruncherStream
	{
		protected readonly string agentName;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected AncientStream(string agentName, Stream wrapperStream) : base(wrapperStream, false)
		{
			this.agentName = agentName;
		}
	}
}
