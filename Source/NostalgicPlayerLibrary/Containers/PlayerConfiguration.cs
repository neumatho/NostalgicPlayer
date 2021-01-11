/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Players;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// Different configuration settings for the player
	/// </summary>
	public class PlayerConfiguration
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerConfiguration(IOutputAgent outputAgent, Loader loader, MixerConfiguration mixerConfiguration)
		{
			OutputAgent = outputAgent;
			Loader = loader;
			MixerConfiguration = mixerConfiguration;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the output agent to use
		/// </summary>
		/********************************************************************/
		public IOutputAgent OutputAgent
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the loader object that has loaded the module
		/// </summary>
		/********************************************************************/
		public Loader Loader
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the mixer configuration
		/// </summary>
		/********************************************************************/
		public MixerConfiguration MixerConfiguration
		{
			get;
		}
	}
}
