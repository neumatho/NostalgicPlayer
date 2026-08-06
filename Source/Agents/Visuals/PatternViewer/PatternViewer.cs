//---------------------------------------------------------------------------------------
// <copyright file="PatternViewer.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

// This is needed to uniquely identify this agent
[assembly: Guid("A7B8C9D0-E1F2-4A5B-8C6D-9E0F1A2B3C4D")]

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer
{
	/// <summary>
	/// NostalgicPlayer agent interface implementation
	/// </summary>
	public class PatternViewer : AgentBase
	{
		private static readonly Guid agent1Id = Guid.Parse("5E6F7A8B-9C0D-1E2F-3A4B-5C6D7E8F9A0B");

		private readonly IAgentWorkerFactory agentWorkerFactory;

		public PatternViewer(IAgentWorkerFactory agentWorkerFactory)
		{
			this.agentWorkerFactory = agentWorkerFactory;
		}

		#region IAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public override string Name => Resources.IDS_NAME;

		/********************************************************************/
		/// <summary>
		/// Returns all the formats/types this agent supports
		/// </summary>
		/********************************************************************/
		public override AgentSupportInfo[] AgentInformation =>
		[
			new(Resources.IDS_NAME_AGENT1, Resources.IDS_DESCRIPTION_AGENT1, agent1Id)
		];

		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public override IAgentWorker CreateInstance(Guid typeId)
		{
			return agentWorkerFactory.GetWorkerInstance<PatternViewerWorker>(typeId);
		}
		#endregion
	}
}
