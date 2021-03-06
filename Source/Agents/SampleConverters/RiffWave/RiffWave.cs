﻿/******************************************************************************/
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
using Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave.Formats;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("AE32657A-9081-4C80-AD6D-BF8D71CA75BD")]

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.RiffWave
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class RiffWave : AgentBase
	{
		private static readonly Dictionary<WaveFormat, Guid> supportedFormats = new Dictionary<WaveFormat, Guid>
		{
			{ WaveFormat.WAVE_FORMAT_PCM, Guid.Parse("8E1352D0-863E-4E7F-8F43-DADC01F6558F") }
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
					new AgentSupportInfo(Resources.IDS_NAME_PCM, Resources.IDS_DESCRIPTION_PCM, supportedFormats[WaveFormat.WAVE_FORMAT_PCM])
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
			WaveFormat format = supportedFormats.Where(pair => pair.Value == typeId).Select(pair => pair.Key).FirstOrDefault();

			switch (format)
			{
				case WaveFormat.WAVE_FORMAT_PCM:
					return new RiffWaveWorker_Pcm();
			}

			return null;
		}
		#endregion
	}
}
