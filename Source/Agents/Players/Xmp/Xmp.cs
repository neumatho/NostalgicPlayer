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
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

// This is needed to uniquely identify this agent
[assembly: Guid("B217F053-F6A3-4969-967C-F43A3AC5389A")]

namespace Polycode.NostalgicPlayer.Agent.Player.Xmp
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Xmp : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_XMP_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_XMP_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation
		{
			get
			{
				return LibXmp.Xmp_Get_Format_Info_List()
					.SkipLast(1)	// Skip last null value
					.Select(x => new AgentSupportInfo(x.Name, x.Description, x.Id)).ToArray();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new XmpWorker(typeId);
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => XmpIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			Xmp_Test_Info testInfo = XmpIdentifier.TestModule(fileInfo);
			if (testInfo != null)
				return new IdentifyFormatInfo(new XmpWorker(testInfo.Id), testInfo.Id);

			return null;
		}
		#endregion
	}
}
