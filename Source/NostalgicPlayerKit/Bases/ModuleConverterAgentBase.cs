/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for module converter agents
	/// </summary>
	public abstract class ModuleConverterAgentBase : IModuleConverterAgent
	{
		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public virtual string OriginalFormat => null;
		#endregion
	}
}
