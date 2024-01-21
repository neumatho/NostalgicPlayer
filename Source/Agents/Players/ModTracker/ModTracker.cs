/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.Player.ModTracker.Containers;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("2630FAE0-312B-4FC5-9C71-7D1016EC9562")]

namespace Polycode.NostalgicPlayer.Agent.Player.ModTracker
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class ModTracker : AgentBase, IPlayerAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("F59956F2-6A97-46BB-BC07-487CD10A20A7");
		private static readonly Guid agent2Id = Guid.Parse("2CCD4798-AE8E-4110-A22A-312F47792BEA");
		private static readonly Guid agent3Id = Guid.Parse("0352C1B7-2C80-4830-A7E2-0AA03885367D");
		private static readonly Guid agent4Id = Guid.Parse("19763654-250B-4EFD-B6D8-59A4120F12C8");
		private static readonly Guid agent5Id = Guid.Parse("62C2E7CA-EC5D-4BE5-A05D-86D0BC3927EE");
		private static readonly Guid agent6Id = Guid.Parse("27B74F65-2033-4883-B935-75B04C2C3AF8");
		private static readonly Guid agent7Id = Guid.Parse("5B37208E-AE9A-4DCA-8AB5-9D644199A252");
		private static readonly Guid agent8Id = Guid.Parse("EC50140C-0E21-40D3-B435-39A014A13C21");
		private static readonly Guid agent9Id = Guid.Parse("A5FDC6F5-DB8B-4066-9566-466E1DA8642C");
		private static readonly Guid agent10Id = Guid.Parse("AF2E4860-5731-4007-828E-34B84DF689D2");
		private static readonly Guid agent11Id = Guid.Parse("0A47F13A-78A4-4CD7-9A31-DBBC5107FC7C");
		private static readonly Guid agent12Id = Guid.Parse("7027B4BE-2D15-4377-8F95-D628184328AF");
		private static readonly Guid agent13Id = Guid.Parse("70E8FE84-09B2-4FE6-9515-4307F14DCBCC");
		private static readonly Guid agent14Id = Guid.Parse("07D57E48-3E89-44DF-BB6A-04AEFBEC8A29");

		private static readonly Dictionary<ModuleType, Guid> moduleTypeLookup = new Dictionary<ModuleType, Guid>
		{
			{ ModuleType.UltimateSoundTracker10, agent1Id },
			{ ModuleType.UltimateSoundTracker18, agent2Id },
			{ ModuleType.SoundTrackerII, agent3Id },
			{ ModuleType.SoundTrackerVI, agent4Id },
			{ ModuleType.SoundTrackerIX, agent5Id },
			{ ModuleType.MasterSoundTracker10, agent6Id },
			{ ModuleType.SoundTracker2x, agent7Id },
			{ ModuleType.NoiseTracker, agent8Id },
			{ ModuleType.StarTrekker, agent9Id },
			{ ModuleType.StarTrekker8, agent10Id },
			{ ModuleType.ProTracker, agent11Id },
			{ ModuleType.HisMastersNoise, agent12Id },
			{ ModuleType.AudioSculpture, agent13Id },
			{ ModuleType.SoundTracker26, agent14Id }
		};

		internal static readonly string[] fileExtensions = { "mod", "adsc", "st26" };

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_DESCRIPTION;



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
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT1, Resources.IDS_MOD_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT2, Resources.IDS_MOD_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT3, Resources.IDS_MOD_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT4, Resources.IDS_MOD_DESCRIPTION_AGENT4, agent4Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT5, Resources.IDS_MOD_DESCRIPTION_AGENT5, agent5Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT6, Resources.IDS_MOD_DESCRIPTION_AGENT6, agent6Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT7, Resources.IDS_MOD_DESCRIPTION_AGENT7, agent7Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT8, Resources.IDS_MOD_DESCRIPTION_AGENT8, agent8Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT9, Resources.IDS_MOD_DESCRIPTION_AGENT9, agent9Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT10, Resources.IDS_MOD_DESCRIPTION_AGENT10, agent10Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT11, Resources.IDS_MOD_DESCRIPTION_AGENT11, agent11Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT12, Resources.IDS_MOD_DESCRIPTION_AGENT12, agent12Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT13, Resources.IDS_MOD_DESCRIPTION_AGENT13, agent13Id),
					new AgentSupportInfo(Resources.IDS_MOD_NAME_AGENT14, Resources.IDS_MOD_DESCRIPTION_AGENT14, agent14Id)
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
			return new ModTrackerWorker();
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
			ModuleType moduleType = ModTrackerWorker.TestModule(fileInfo);
			if (moduleType == ModuleType.Unknown)
				return null;

			return new IdentifyFormatInfo(new ModTrackerWorker(moduleType), moduleTypeLookup[moduleType]);
		}
		#endregion
	}
}
