/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("920050C8-7A1F-46B1-968A-DE61E3332611")]

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Flac : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("29FCC952-9D05-42B3-B167-428F4F46F3EC");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_FLAC_NAME;



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
					new AgentSupportInfo(Resources.IDS_FLAC_NAME_AGENT1, Resources.IDS_FLAC_DESCRIPTION_AGENT1, agent1Id)
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
			return new FlacWorker();
		}
		#endregion
	}
}
