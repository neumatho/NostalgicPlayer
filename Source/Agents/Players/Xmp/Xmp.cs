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
		internal static readonly string[] fileExtensions = { "gdm", "xm", "oxm" };

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
				return Ports.LibXmp.LibXmp.Xmp_Get_Format_Info_List()
					.SkipLast(1)	// Skip last null value
					.Where(x => x.Id == Guid.Parse("6118D229-7AEC-4FF6-8A0C-F4F5BCCE2564") || x.Id == Guid.Parse("1574A876-5F9D-4BAE-81AF-7DB01370ADDD") || x.Id == Guid.Parse("F1878ED9-37B8-4D5F-9AFE-46B6A9C195DF"))//XX
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
		public string[] FileExtensions => fileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			LibXmp libXmp = LibXmp.Xmp_Create_Context();

			int retVal = libXmp.Xmp_Test_Module_From_File(fileInfo.ModuleStream, out Xmp_Test_Info testInfo);

			libXmp.Xmp_Free_Context();

			if (retVal == 0 && (testInfo.Id == Guid.Parse("6118D229-7AEC-4FF6-8A0C-F4F5BCCE2564") || testInfo.Id == Guid.Parse("1574A876-5F9D-4BAE-81AF-7DB01370ADDD") || testInfo.Id == Guid.Parse("F1878ED9-37B8-4D5F-9AFE-46B6A9C195DF")))//XX
				return new IdentifyFormatInfo(new XmpWorker(testInfo.Id), testInfo.Id);

			return null;
		}
		#endregion
	}
}
