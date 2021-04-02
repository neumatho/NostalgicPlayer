/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Iff16Sv.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("2E0527E3-2BF2-481B-9BE8-9C0862DBCAAD")]

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Iff16Sv
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class Iff16Sv : AgentBase
	{
		private static readonly Dictionary<Format, Guid> supportedFormats = new Dictionary<Format, Guid>
		{
			{ Format.Pcm, Guid.Parse("D74FECF4-A461-4858-9DF2-1FD41C6069B8") }
		};

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name
		{
			get
			{
				return Resources.IDS_NAME;
			}
		}



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
					new AgentSupportInfo(Resources.IDS_IFF16SV_NAME_PCM, string.Format(Resources.IDS_IFF16SV_DESCRIPTION, Resources.IDS_IFF16SV_DESCRIPTION_PCM), supportedFormats[Format.Pcm])
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
			Format format = supportedFormats.Where(pair => pair.Value == typeId).Select(pair => pair.Key).FirstOrDefault();

			switch (format)
			{
				case Format.Pcm:
					return new Iff16SvWorker_Pcm();
			}

			return null;
		}
		#endregion
	}
}
