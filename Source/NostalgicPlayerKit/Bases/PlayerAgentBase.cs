﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for player agents
	/// </summary>
	public abstract class PlayerAgentBase : IPlayerAgent
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public abstract string[] FileExtensions
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public virtual string ModuleName => string.Empty;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public virtual string Author => string.Empty;



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public abstract void Play();



		/********************************************************************/
		/// <summary>
		/// This flag is set to true, when end is reached
		/// </summary>
		/********************************************************************/
		public bool HasEndReached
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		/********************************************************************/
		public event EventHandler EndReached;



		/********************************************************************/
		/// <summary>
		/// Event called when the player update some module information
		/// </summary>
		/********************************************************************/
		public event ModuleInfoChangedEventHandler ModuleInfoChanged;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of the module
		/// </summary>
		/********************************************************************/
		protected void OnEndReached()
		{
			// Set flag
			HasEndReached = true;

			// Call events
			if (EndReached != null)
				EndReached(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Call this every time your player change some information shown
		/// in the module information window
		/// </summary>
		/********************************************************************/
		protected void OnModuleInfoChanged(int line, string newValue)
		{
			if (ModuleInfoChanged != null)
				ModuleInfoChanged(this, new ModuleInfoChangedEventArgs(line, newValue));
		}
		#endregion
	}
}
