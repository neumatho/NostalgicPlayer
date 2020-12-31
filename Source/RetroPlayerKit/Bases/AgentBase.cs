/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Polycode.RetroPlayer.RetroPlayerKit.Interfaces;

namespace Polycode.RetroPlayer.RetroPlayerKit.Bases
{
	/// <summary>
	/// Base class that have some default implementations that can be used
	/// </summary>
	public abstract class AgentBase : IAgent
	{
		/********************************************************************/
		/// <summary>
		/// Returns an unique ID for this agent
		/// </summary>
		/********************************************************************/
		public virtual Guid Id
		{
			get
			{
				return new Guid(Assembly.GetAssembly(GetType()).GetCustomAttribute<GuidAttribute>().Value);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the version of this agent
		/// </summary>
		/********************************************************************/
		public virtual Version Version
		{
			get
			{
				return Assembly.GetAssembly(GetType()).GetName().Version;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of this agent
		/// </summary>
		/********************************************************************/
		public abstract string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns a description of this agent
		/// </summary>
		/********************************************************************/
		public abstract string Description
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new worker instance
		/// </summary>
		/********************************************************************/
		public abstract IAgentWorker CreateInstance();
	}
}
