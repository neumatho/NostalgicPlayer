/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;

// This is needed to uniquely identify this agent
[assembly: Guid("96CEC140-44A8-4318-AAE3-06FFB9981374")]

namespace Polycode.NostalgicPlayer.Agent.Player.OpenMpt
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class OpenMpt : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MPT_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MPT_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return LibOpenMpt.GetFormats().Select(x => new AgentSupportInfo(x.Name, x.Description, x.Id)).ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new OpenMptWorker(typeId);
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => OpenMptIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			Probe_File_Header_Info testInfo = OpenMptIdentifier.TestModule(fileInfo);
			if (testInfo != null)
				return new IdentifyFormatInfo(new OpenMptWorker(testInfo.Id), testInfo.Id);

			return null;
		}
		#endregion
	}
}
