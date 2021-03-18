/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// The different kind of information stored in the database
	/// </summary>
	public class ModuleDatabaseInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleDatabaseInfo(TimeSpan duration)
		{
			Duration = duration;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the duration of the module
		/// </summary>
		/********************************************************************/
		public TimeSpan Duration
		{
			get;
		}
	}
}
