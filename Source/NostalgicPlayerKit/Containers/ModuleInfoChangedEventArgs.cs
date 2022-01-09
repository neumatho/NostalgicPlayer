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
	/// </summary>
	public delegate void ModuleInfoChangedEventHandler(object sender, ModuleInfoChangedEventArgs e);

	/// <summary>
	/// Container class holding needed information when sending an update event
	/// </summary>
	public class ModuleInfoChangedEventArgs : EventArgs
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoChangedEventArgs(int line, string newValue)
		{
			Line = line;
			Value = newValue;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the line that need to be updated
		/// </summary>
		/********************************************************************/
		public int Line
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holding the new value
		/// </summary>
		/********************************************************************/
		public string Value
		{
			get;
		}
	}
}
