/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Containers.Events
{
	/// <summary>
	/// </summary>
	public delegate void PlayerFailedEventHandler(object sender, PlayerFailedEventArgs e);

	/// <summary>
	/// Event class holding needed information if a player fails while playing
	/// </summary>
	public class PlayerFailedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerFailedEventArgs(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the error message
		/// </summary>
		/********************************************************************/
		public string ErrorMessage
		{
			get;
		}
	}
}
