/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Handles all the effects
	/// </summary>
	internal class EffectMaster
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EffectMaster(SubSong ss)
		{
			GlobalGroup = new EffectGroup(ss);
		}



		/********************************************************************/
		/// <summary>
		/// Holds the global effect group
		/// </summary>
		/********************************************************************/
		public EffectGroup GlobalGroup
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all effect groups
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			GlobalGroup.Initialize();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup all effect groups
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			GlobalGroup.Cleanup();
		}
	}
}
