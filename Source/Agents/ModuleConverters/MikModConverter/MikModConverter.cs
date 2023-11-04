/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("ED2854DF-91A7-4174-B100-FB169AAE5AF6")]

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class MikModConverter : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("DA884858-983E-45DB-B4FF-470DBB927239");
		private static readonly Guid agent2Id = Guid.Parse("216FCDE9-3E12-474A-A27D-28914C8C894D");
		private static readonly Guid agent3Id = Guid.Parse("D1499BF4-82D2-4B9C-9CDA-7D2ADE977E63");
		private static readonly Guid agent4Id = Guid.Parse("29FCE360-BD89-4A32-A068-BFF0D84F1C57");
		private static readonly Guid agent5Id = Guid.Parse("ED6F6B4C-3218-4B80-86C5-A2C62B9E3070");
		private static readonly Guid agent6Id = Guid.Parse("E2C8619A-6957-43C9-A518-71002E37408B");
		private static readonly Guid agent7Id = Guid.Parse("6118D229-7AEC-4FF6-8A0C-F4F5BCCE2564");
		private static readonly Guid agent8Id = Guid.Parse("F0906D97-B9B3-451E-870D-A97529D38480");
		private static readonly Guid agent9Id = Guid.Parse("6D40CCCF-45AE-4F5C-BF6B-2ABCD31B80AC");
		private static readonly Guid agent10Id = Guid.Parse("557C8681-93CF-4BB8-A3DD-632C55DD840A");
		private static readonly Guid agent11Id = Guid.Parse("EB0B4765-CA32-43A3-AC3A-93ED4907498B");
		private static readonly Guid agent12Id = Guid.Parse("BFFB2A7C-52D5-4492-9241-3AC2FD705D80");
		private static readonly Guid agent13Id = Guid.Parse("368009A5-E68F-4C33-8ACA-ED6B8448A2A6");
		private static readonly Guid agent14Id = Guid.Parse("E37DF813-DCBD-4A32-AA07-5EF1AF6DD037");
		private static readonly Guid agent15Id = Guid.Parse("E03F718C-8FA9-4843-9EBE-6D69EC3A421D");
		private static readonly Guid agent16Id = Guid.Parse("1574A876-5F9D-4BAE-81AF-7DB01370ADDD");

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_MIKCONV_NAME;



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public override string Description => Resources.IDS_MIKCONV_DESCRIPTION;



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
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT1, Resources.IDS_MIKCONV_DESCRIPTION_AGENT1, agent1Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT2, Resources.IDS_MIKCONV_DESCRIPTION_AGENT2, agent2Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT3, Resources.IDS_MIKCONV_DESCRIPTION_AGENT3, agent3Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT4, Resources.IDS_MIKCONV_DESCRIPTION_AGENT4, agent4Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT5, Resources.IDS_MIKCONV_DESCRIPTION_AGENT5, agent5Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT6, Resources.IDS_MIKCONV_DESCRIPTION_AGENT6, agent6Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT7, Resources.IDS_MIKCONV_DESCRIPTION_AGENT7, agent7Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT8, Resources.IDS_MIKCONV_DESCRIPTION_AGENT8, agent8Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT9, Resources.IDS_MIKCONV_DESCRIPTION_AGENT9, agent9Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT10, Resources.IDS_MIKCONV_DESCRIPTION_AGENT10, agent10Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT11, Resources.IDS_MIKCONV_DESCRIPTION_AGENT11, agent11Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT12, Resources.IDS_MIKCONV_DESCRIPTION_AGENT12, agent12Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT13, Resources.IDS_MIKCONV_DESCRIPTION_AGENT13, agent13Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT14, Resources.IDS_MIKCONV_DESCRIPTION_AGENT14, agent14Id),
					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT15, Resources.IDS_MIKCONV_DESCRIPTION_AGENT15, agent15Id),
//					new AgentSupportInfo(Resources.IDS_MIKCONV_NAME_AGENT16, Resources.IDS_MIKCONV_DESCRIPTION_AGENT16, agent16Id)
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
				return new _669Format(0x69, 0x66);

			if (typeId == agent2Id)
				return new _669Format(0x4a, 0x4e);

			if (typeId == agent3Id)
				return new AmfFormat();

			if (typeId == agent4Id)
				return new AsyFormat();

			if (typeId == agent5Id)
				return new DsmFormat();

			if (typeId == agent6Id)
				return new FarFormat();

			if (typeId == agent7Id)
				return new GdmFormat();

			if (typeId == agent8Id)
				return new ImfFormat();

			if (typeId == agent9Id)
				return new ItFormat();

			if (typeId == agent10Id)
				return new StmFormat();

			if (typeId == agent11Id)
				return new S3MFormat();

			if (typeId == agent12Id)
				return new StxFormat();

			if (typeId == agent13Id)
				return new UltFormat();

			if (typeId == agent14Id)
				return new UniFormat();

			if (typeId == agent15Id)
				return new UmxFormat();

			if (typeId == agent16Id)
				return new XmFormat();

			return null;
		}
		#endregion
	}
}
