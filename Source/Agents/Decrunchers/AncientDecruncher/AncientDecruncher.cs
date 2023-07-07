/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.Ancient;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

// This is needed to uniquely identify this agent
[assembly: Guid("AFDBEE3F-E5A6-4255-8A68-89D876BCE943")]

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher
{
    /// <summary>
    /// NostalgicPlayer agent interface implementation
    /// </summary>
    public class AncientDecruncher : AgentBase, IAgentMultipleFormatIdentify
	{
		private static readonly Guid agent1Id = Guid.Parse("C7165383-9774-4297-8168-9FACE978EFA3");
		private static readonly Guid agent2Id = Guid.Parse("15166B32-0EFF-49F3-93CC-DCB8F6D9C23C");
		private static readonly Guid agent3Id = Guid.Parse("911FDA93-928F-4A93-9370-347C7C442717");
		private static readonly Guid agent4Id = Guid.Parse("3E4DCDC1-2A25-4D0C-A8C7-17F489F6DAD3");
		private static readonly Guid agent5Id = Guid.Parse("890D80F6-CFD5-4253-9557-751847AD8E3A");
		private static readonly Guid agent6Id = Guid.Parse("5D917A8E-2FF5-4695-973B-6BA9C7CCA711");
		private static readonly Guid agent7Id = Guid.Parse("BFDBDDFA-A76A-4C2C-821A-3E2E1DD46E0A");
		private static readonly Guid agent8Id = Guid.Parse("03263F70-B384-4452-9D34-971F16DE2C6D");
		private static readonly Guid agent9Id = Guid.Parse("61410C72-454A-428F-9EF1-9A3CDB7FA3D0");
		private static readonly Guid agent10Id = Guid.Parse("8126B435-95E4-4205-A589-8A9B4CFF1DD1");
		private static readonly Guid agent11Id = Guid.Parse("85226D94-249F-4C52-983B-99858862680D");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_ANC_NAME;



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
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT1, Resources.IDS_ANC_DESCRIPTION_AGENT1, agent1Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT2, Resources.IDS_ANC_DESCRIPTION_AGENT2, agent2Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT3, Resources.IDS_ANC_DESCRIPTION_AGENT3, agent3Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT4, Resources.IDS_ANC_DESCRIPTION_AGENT4, agent4Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT5, Resources.IDS_ANC_DESCRIPTION_AGENT5, agent5Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT6, Resources.IDS_ANC_DESCRIPTION_AGENT6, agent6Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT7, Resources.IDS_ANC_DESCRIPTION_AGENT7, agent7Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT8, Resources.IDS_ANC_DESCRIPTION_AGENT8, agent8Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT9, Resources.IDS_ANC_DESCRIPTION_AGENT9, agent9Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT10, Resources.IDS_ANC_DESCRIPTION_AGENT10, agent10Id),
					new AgentSupportInfo(Resources.IDS_ANC_NAME_AGENT11, Resources.IDS_ANC_DESCRIPTION_AGENT11, agent11Id)
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
			if (typeId == agent1Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT1);

			if (typeId == agent2Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT2);

			if (typeId == agent3Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT3);

			if (typeId == agent4Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT4);

			if (typeId == agent5Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT5);

			if (typeId == agent6Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT6);

			if (typeId == agent7Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT7);

			if (typeId == agent8Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT8);

			if (typeId == agent9Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT9);

			if (typeId == agent10Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT10);

			if (typeId == agent11Id)
				return new AncientWorker(Resources.IDS_ANC_NAME_AGENT11, null);

			return null;
		}
		#endregion

		#region IAgentMultipleFormatIdentify implementation
		/********************************************************************/
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the format Guid if found
		/// </summary>
		/********************************************************************/
		public IdentifyFormatInfo IdentifyFormat(Stream dataStream)
		{
			try
			{
				Decompressor decompressor = new Decompressor(dataStream);
				string agentName;
				Guid typeId;

				switch (decompressor.GetDecompressorType())
				{
					case DecompressorType.PowerPacker:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT1;
						typeId = agent1Id;
						break;
					}

					case DecompressorType.Xpk_Sqsh:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT2;
						typeId = agent2Id;
						break;
					}

					case DecompressorType.Mmcmp:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT3;
						typeId = agent3Id;
						break;
					}

					case DecompressorType.Xpk_Bzp2:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT4;
						typeId = agent4Id;
						break;
					}

					case DecompressorType.Xpk_Blzw:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT5;
						typeId = agent5Id;
						break;
					}

					case DecompressorType.Xpk_Rake:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT6;
						typeId = agent6Id;
						break;
					}

					case DecompressorType.Xpk_Smpl:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT7;
						typeId = agent7Id;
						break;
					}

					case DecompressorType.Xpk_Shri:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT8;
						typeId = agent8Id;
						break;
					}

					case DecompressorType.Xpk_Lhlb:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT9;
						typeId = agent9Id;
						break;
					}

					case DecompressorType.Xpk_Mash:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT10;
						typeId = agent10Id;
						break;
					}

					case DecompressorType.CrunchMania:
					{
						agentName = Resources.IDS_ANC_NAME_AGENT11;
						typeId = agent11Id;
						break;
					}

					default:
						return null;
				}

				return new IdentifyFormatInfo(new AncientWorker(agentName, decompressor), typeId);
			}
			catch (InvalidFormatException)
			{
				return null;
			}
		}
		#endregion
	}
}
