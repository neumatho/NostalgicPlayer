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
using Polycode.NostalgicPlayer.Kit.Mixer;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for player agents
	/// </summary>
	public abstract class ModulePlayerAgentBase : PlayerAgentBase, IModulePlayerAgent
	{
		private static readonly SubSongInfo subSongInfo = new SubSongInfo(1, 0); 

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ModulePlayerAgentBase()
		{
			VirtualChannels = null;
			PlayingFrequency = 50.0f;
		}



		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public virtual ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.None;



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
		public virtual int VirtualChannelCount => ModuleChannelCount;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public virtual int ModuleChannelCount => 4;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public virtual SubSongInfo SubSongs => subSongInfo;



		/********************************************************************/
		/// <summary>
		/// Return the length of the current song
		/// </summary>
		/********************************************************************/
		public virtual int SongLength => 0;



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
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public virtual InstrumentInfo[] Instruments => null;



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public virtual SampleInfo[] Samples => null;



		/********************************************************************/
		/// <summary>
		/// Return the total size of all the extra files loaded
		/// </summary>
		/********************************************************************/
		public virtual long ExtraFilesSizes
		{
			get; protected set;
		} = 0;


		/********************************************************************/
		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		/********************************************************************/
		public virtual Channel[] VirtualChannels
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



		/********************************************************************/
		/// <summary>
		/// Return the current state of the Amiga filter
		/// </summary>
		/********************************************************************/
		public bool AmigaFilter
		{
			get; protected set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Event called when the player change position
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this every time your player change it's position
		/// </summary>
		/********************************************************************/
		protected void OnPositionChanged()
		{
			if (PositionChanged != null)
				PositionChanged(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency to the BPM you give and change the
		/// playing speed
		/// </summary>
		/********************************************************************/
		protected void SetBpmTempo(ushort bpm)
		{
			PlayingFrequency = bpm / 2.5f;
		}
		#endregion
	}
}
