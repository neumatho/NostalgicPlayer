/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Interface used to trigger samples when playing modules
	/// </summary>
	public interface IChannel
	{
		/// <summary>
		/// Will start to play the sample in the channel. If your player is
		/// running in buffer mode, use this method to set the buffer
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples of the sample</param>
		/// <param name="bit">is the number of bits each sample are, e.g. 8 or 16</param>
		void PlaySample(Array adr, uint startOffset, uint length, byte bit = 8);

		/// <summary>
		/// Will set the loop point in the sample
		/// </summary>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		void SetLoop(uint startOffset, uint length, ChannelLoopType type = ChannelLoopType.Normal);

		/// <summary>
		/// Will set the loop point and change the sample
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		void SetLoop(Array adr, uint startOffset, uint length, ChannelLoopType type = ChannelLoopType.Normal);

		/// <summary>
		/// Will change the volume
		/// </summary>
		/// <param name="vol">is the new volume</param>
		void SetVolume(ushort vol);

		/// <summary>
		/// Will change the panning
		/// </summary>
		/// <param name="pan">is the new panning</param>
		void SetPanning(ushort pan);

		/// <summary>
		/// Will change the frequency
		/// </summary>
		/// <param name="freq">is the new frequency</param>
		void SetFrequency(uint freq);

		/// <summary>
		/// Will calculate the period given to a frequency and set it
		/// </summary>
		/// <param name="period">is the new frequency as an Amiga period</param>
		void SetAmigaPeriod(uint period);

		/// <summary>
		/// Returns true or false depending on the channel is in use
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// Mute the channel
		/// </summary>
		void Mute();

		/// <summary>
		/// Returns the current volume on the channel
		/// </summary>
		ushort GetVolume();

		/// <summary>
		/// Returns the current frequency on the channel
		/// </summary>
		uint GetFrequency();
	}
}
