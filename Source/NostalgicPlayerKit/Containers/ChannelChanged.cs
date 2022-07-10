﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about a channel change
	/// </summary>
	public class ChannelChanged
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelChanged(ChannelFlags flags, ushort volume, uint frequency, uint sampleLength, int samplePosition, VisualInfo visualInfo, bool enabled)
		{
			Flags = flags;
			Volume = volume;
			Frequency = frequency;
			SampleLength = sampleLength;
			SamplePosition = samplePosition;
			VisualInfo = visualInfo;
			Enabled = enabled;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the channel flags. Indicate which information has changed
		/// </summary>
		/********************************************************************/
		public ChannelFlags Flags
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current volume on the channel
		/// </summary>
		/********************************************************************/
		public ushort Volume
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current frequency on the channel
		/// </summary>
		/********************************************************************/
		public uint Frequency
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the length of the sample in samples
		/// </summary>
		/********************************************************************/
		public uint SampleLength
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds new sample position if set
		/// </summary>
		/********************************************************************/
		public int SamplePosition
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the visual information on the channel
		/// </summary>
		/********************************************************************/
		public VisualInfo VisualInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the channel are enabled (not muted)
		/// </summary>
		/********************************************************************/
		public bool Enabled
		{
			get;
		}
	}
}
