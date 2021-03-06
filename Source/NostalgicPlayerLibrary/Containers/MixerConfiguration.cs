﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// Different configuration settings for the mixer
	/// </summary>
	public class MixerConfiguration
	{
		/// <summary>
		/// The maximum number of channels supported
		/// </summary>
		public const int MaxNumberOfChannels = 64;

		/********************************************************************/
		/// <summary>
		/// The stereo separation in percent
		/// </summary>
		/********************************************************************/
		public int StereoSeparator
		{
			get; set;
		} = 100;



		/********************************************************************/
		/// <summary>
		/// Indicate if interpolation is enabled
		/// </summary>
		/********************************************************************/
		public bool EnableInterpolation
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Indicate if Amiga filter should be emulated
		/// </summary>
		/********************************************************************/
		public bool EnableAmigaFilter
		{
			get; set;
		} = true;



		/********************************************************************/
		/// <summary>
		/// Holds an array telling which channels to enable/disable. If null,
		/// all channels are enabled
		/// </summary>
		/********************************************************************/
		public bool[] ChannelsEnabled
		{
			get; set;
		} = null;
	}
}
