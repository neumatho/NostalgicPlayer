/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds information about a single format/type supported by an agent
	/// </summary>
	public class AgentSupportInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentSupportInfo(string name, string description, Guid typeId)
		{
			Name = name;
			Description = description;
			TypeId = typeId;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the name of the format/type
		/// </summary>
		/********************************************************************/
		public string Name
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the description of the format/type
		/// </summary>
		/********************************************************************/
		public string Description
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the ID of the format/type
		/// </summary>
		/********************************************************************/
		public Guid TypeId
		{
			get;
		}
	}
}
