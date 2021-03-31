﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers
{
	/// <summary>
	/// Container class holding information about a sample to play from the keyboard
	/// </summary>
	public class PlaySampleInfo
	{
		/********************************************************************/
		/// <summary>
		/// Holds the sample information for the sample to play
		/// </summary>
		/********************************************************************/
		public SampleInfo SampleInfo
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the frequency to play with
		/// </summary>
		/********************************************************************/
		public double Frequency
		{
			get; set;
		}
	}
}