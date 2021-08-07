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

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// This class is an empty implementation of the IChannel and is
	/// e.g. used when calculation duration of a module
	/// </summary>
	internal class DummyChannel : IChannel
	{
		#region IChannel implementation
		/********************************************************************/
		/// <summary>
		/// Will start to play the sample in the channel
		/// </summary>
		/********************************************************************/
		public void PlaySample(Array adr, uint startOffset, uint length, byte bit = 8)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point in the sample
		/// </summary>
		/********************************************************************/
		public void SetLoop(uint startOffset, uint length, ChannelLoopType type = ChannelLoopType.Normal)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point and change the sample
		/// </summary>
		/********************************************************************/
		public void SetLoop(Array adr, uint startOffset, uint length, ChannelLoopType type = ChannelLoopType.Normal)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the release part of the sample
		/// </summary>
		/********************************************************************/
		public void PlayReleasePart(uint startOffset, uint length)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will change the volume
		/// </summary>
		/********************************************************************/
		public void SetVolume(ushort vol)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will change the panning
		/// </summary>
		/********************************************************************/
		public void SetPanning(ushort pan)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will change the frequency
		/// </summary>
		/********************************************************************/
		public void SetFrequency(uint freq)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the period given to a frequency and set it
		/// </summary>
		/********************************************************************/
		public void SetAmigaPeriod(uint period)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns true or false depending on the channel is in use
		/// </summary>
		/********************************************************************/
		public bool IsActive => false;



		/********************************************************************/
		/// <summary>
		/// Mute the channel
		/// </summary>
		/********************************************************************/
		public void Mute()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current volume on the channel
		/// </summary>
		/********************************************************************/
		public ushort GetVolume()
		{
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current frequency on the channel
		/// </summary>
		/********************************************************************/
		public uint GetFrequency()
		{
			return 0;
		}
		#endregion
	}
}