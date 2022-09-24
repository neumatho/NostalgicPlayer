/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Interface for players playing module files
	/// </summary>
	public interface IModulePlayer : IPlayer
	{
		/// <summary>
		/// Will select the song you want to play
		/// </summary>
		bool SelectSong(int songNumber, out string errorMessage);

		/// <summary>
		/// Will set a new song position
		/// </summary>
		void SetSongPosition(int position);

		/// <summary>
		/// Event called when the player change position
		/// </summary>
		event EventHandler PositionChanged;

		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		public event SubSongChangedEventHandler SubSongChanged;
	}
}
