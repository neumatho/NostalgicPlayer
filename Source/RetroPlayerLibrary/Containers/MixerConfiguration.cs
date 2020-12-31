/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.RetroPlayer.RetroPlayerLibrary.Containers
{
	/// <summary>
	/// Different configuration settings for the mixer
	/// </summary>
	public class MixerConfiguration
	{
		/********************************************************************/
		/// <summary>
		/// Indicate if Amiga filter should be emulated
		/// </summary>
		/********************************************************************/
		public bool EnableAmigaFilter
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// The stereo separation in percent
		/// </summary>
		/********************************************************************/
		public int StereoSeparator
		{
			get; set;
		} = 100;
	}
}
