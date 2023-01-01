/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("1B432960-5996-4799-AE39-681BB18935C1")]

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Oscilloscope : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("4FA350E5-B774-4BEC-80A1-21CBF851D388");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_OSCIL_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return new AgentSupportInfo[]
				{
					new AgentSupportInfo(Resources.IDS_OSCIL_NAME_AGENT1, Resources.IDS_OSCIL_DESCRIPTION_AGENT1, agent1Id)
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new OscilloscopeWorker();
		}
		#endregion
	}
}
