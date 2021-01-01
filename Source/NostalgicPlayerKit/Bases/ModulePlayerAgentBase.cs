/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Mixer;

namespace Polycode.NostalgicPlayer.NostalgicPlayerKit.Bases
{
	/// <summary>
	/// Base class that can be used for player agents
	/// </summary>
	public abstract class ModulePlayerAgentBase : PlayerAgentBase, IModulePlayerAgent
	{
		private static readonly SubSongInfo subSongInfo = new SubSongInfo(1, 0); 

		private long totalSize;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ModulePlayerAgentBase()
		{
			Channels = null;
			PlayingFrequency = 50.0f;

			EndReached = false;

			totalSize = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public virtual ModulePlayerSupportFlag GetSupportFlags()
		{
			return ModulePlayerSupportFlag.None;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public virtual bool InitPlayer()
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public virtual void InitSound(int songNumber)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSound()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module want to reserve
		/// </summary>
		/********************************************************************/
		public virtual int GetVirtualChannelCount()
		{
			return GetModuleChannelCount();
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public virtual int GetModuleChannelCount()
		{
			return 4;
		}



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public virtual SubSongInfo GetSubSongs()
		{
			return subSongInfo;
		}



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public virtual int GetSongLength()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current position of the song
		/// </summary>
		/********************************************************************/
		public virtual int SongPosition
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the position time for each position
		/// </summary>
		/********************************************************************/
		public virtual TimeSpan GetPositionTimeTable(int songNumber, out TimeSpan[] positionTimes)
		{
			positionTimes = null;

			return new TimeSpan(0);
		}



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public virtual bool GetInformationString(int line, out string description, out string value)
		{
			description = null;
			value = null;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return the total size of all the extra files loaded
		/// </summary>
		/********************************************************************/
		public virtual long GetExtraFilesSizes()
		{
			return totalSize;
		}



		/********************************************************************/
		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		/********************************************************************/
		public virtual Channel[] Channels
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current playing frequency
		/// </summary>
		/********************************************************************/
		public virtual float PlayingFrequency
		{
			get; protected set;
		}

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this every time your player change it's position
		/// </summary>
		/********************************************************************/
		protected void ChangePosition()
		{
		}
		#endregion
	}
}
