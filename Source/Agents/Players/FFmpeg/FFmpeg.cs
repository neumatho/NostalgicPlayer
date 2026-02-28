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
[assembly: Guid("AAEA706C-BBF4-41D2-BCE6-48B3A6982B18")]

namespace Polycode.NostalgicPlayer.Agent.Player.FFmpeg
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class FFmpeg : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		internal static readonly Guid Agent1Id = Guid.Parse("9C7018D0-C57C-4B45-A733-BD27C3920DD6");
		internal static readonly Guid Agent2Id = Guid.Parse("AF84D839-0F00-4B25-8127-7940272969F5");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_FFMPEG_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_FFMPEG_DESCRIPTION;



		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new AgentSupportInfo(Resources.IDS_FFMPEG_NAME_AGENT1, Resources.IDS_FFMPEG_DESCRIPTION_AGENT1, Agent1Id),
			new AgentSupportInfo(Resources.IDS_FFMPEG_NAME_AGENT2, Resources.IDS_FFMPEG_DESCRIPTION_AGENT2, Agent2Id)
		];



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return new FFmpegWorker();
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify all the formats that
		/// can be returned in IdentifyFormat()
		/// </summary>
		/********************************************************************/
		public string[] FileExtensions => FFmpegIdentifier.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(PlayerFileInfo fileInfo)
		{
			Guid? id = FFmpegIdentifier.TestModule(fileInfo);
			if (id.HasValue)
				return new IdentifyFormatInfo(new FFmpegWorker(), id.Value);

			return null;
		}
		#endregion
	}
}
