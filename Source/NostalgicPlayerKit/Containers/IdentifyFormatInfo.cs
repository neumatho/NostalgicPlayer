/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Hold needed information when identifying multiple formats
	/// </summary>
	public class IdentifyFormatInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo(IAgentWorker worker, Guid typeId)
		{
			Worker = worker;
			TypeId = typeId;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the worker agent for the identified format
		/// </summary>
		/********************************************************************/
		public IAgentWorker Worker
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the agent type ID for the identified format
		/// </summary>
		/********************************************************************/
		public Guid TypeId
		{
			get;
		}
	}
}
