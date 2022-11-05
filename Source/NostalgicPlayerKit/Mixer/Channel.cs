/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Mixer
{
	/// <summary>
	/// This class is used to trigger samples when playing modules
	/// </summary>
	public class Channel : IChannel
	{
		/// <summary>
		/// Indicate what has been set/changed
		/// </summary>
		protected ChannelFlags flags;

		/// <summary>
		/// Start addresses of the sample
		/// </summary>
		protected readonly Array[] sampleAddresses = new Array[2];

		/// <summary>
		/// Start address of the loop/release sample
		/// </summary>
		protected readonly Array[] loopAddresses = new Array[2];

		/// <summary>
		/// The sample number being played or -1 if unknown
		/// </summary>
		protected short sampleNumber;

		/// <summary>
		/// Start offset in the sample in samples, not bytes
		/// </summary>
		protected uint sampleStart;

		/// <summary>
		/// Length of the sample in samples, not bytes
		/// </summary>
		protected uint sampleLength;

		/// <summary>
		/// Loop offset in the sample in samples, not bytes
		/// </summary>
		protected uint loopStart;

		/// <summary>
		/// Loop length in samples, not bytes
		/// </summary>
		protected uint loopLength;

		/// <summary>
		/// The new sample position to set
		/// </summary>
		protected int samplePosition;

		/// <summary>
		/// Length of the release part of the sample
		/// </summary>
		protected uint releaseLength;

		/// <summary>
		/// The frequency to play with
		/// </summary>
		protected uint frequency;

		/// <summary>
		/// The volume (0-256)
		/// </summary>
		protected ushort volume;

		/// <summary>
		/// The left volume (0-256)
		/// </summary>
		protected ushort leftVolume;

		/// <summary>
		/// The right volume (0-256)
		/// </summary>
		protected ushort rightVolume;

		/// <summary>
		/// The panning (0 = left; 128 = center; 255 = right; 512 = surround)
		/// </summary>
		protected uint panning;

		#region IChannel implementation
		/********************************************************************/
		/// <summary>
		/// Will start to play the buffer in the channel. Only use this if
		/// your player is running in buffer mode. Note that the length then
		/// have to be the same for each channel
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples of the sample</param>
		/// <param name="bit">is the number of bits each sample are, e.g. 8 or 16</param>
		/********************************************************************/
		public void PlayBuffer(Array adr, uint startOffset, uint length, byte bit)
		{
			PlaySample(-1, adr, startOffset, length, bit, false);
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the sample in the channel
		/// </summary>
		/// <param name="sampleNumber">is the sample number being played. If unknown, set it to -1</param>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples of the sample</param>
		/// <param name="bit">is the number of bits each sample are, e.g. 8 or 16</param>
		/// <param name="backwards">indicate if the sample should be played backwards</param>
		/********************************************************************/
		public void PlaySample(short sampleNumber, Array adr, uint startOffset, uint length, byte bit, bool backwards)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if ((adr.GetType() != typeof(sbyte[])) && (adr.GetType() != typeof(short[])))
				throw new ArgumentException("Type of array must be either sbyte[] or short[]", nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset > adr.Length)
				throw new ArgumentException("startOffset is bigger than length", nameof(startOffset));

			if ((bit != 8) && (bit != 16))
				throw new ArgumentException("Number of bits may only be 8 or 16", nameof(bit));

			this.sampleNumber = sampleNumber;

			sampleAddresses[0] = adr;
			sampleAddresses[1] = null;

			sampleStart = startOffset;
			sampleLength = length;
			flags |= ChannelFlags.TrigIt;

			if (bit == 16)
				flags |= ChannelFlags._16Bit;

			if (backwards)
				flags |= ChannelFlags.Backwards;

			flags &= ~ChannelFlags.MuteIt;
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play a stereo sample in the channel
		/// </summary>
		/// <param name="sampleNumber">is the sample number being played. If unknown, set it to -1</param>
		/// <param name="leftAdr">is a pointer to the sample in memory to be played in the left speaker</param>
		/// <param name="rightAdr">is a pointer to the sample in memory to be played in the right speaker</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples of the sample</param>
		/// <param name="bit">is the number of bits each sample are, e.g. 8 or 16</param>
		/// <param name="backwards">indicate if the sample should be played backwards</param>
		/********************************************************************/
		public void PlayStereoSample(short sampleNumber, Array leftAdr, Array rightAdr, uint startOffset, uint length, byte bit, bool backwards)
		{
			if (leftAdr == null)
				throw new ArgumentNullException(nameof(leftAdr));

			if (rightAdr == null)
				throw new ArgumentNullException(nameof(rightAdr));

			if (leftAdr.GetType() != rightAdr.GetType())
				throw new ArgumentException("Left and right speaker arrays must be of the same type", nameof(leftAdr));

			if (leftAdr.Length != rightAdr.Length)
				throw new ArgumentException("Left and right speaker arrays must be of equal size", nameof(leftAdr));

			if ((leftAdr.GetType() != typeof(sbyte[])) && (leftAdr.GetType() != typeof(short[])))
				throw new ArgumentException("Type of array must be either sbyte[] or short[]", nameof(leftAdr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset > leftAdr.Length)
				throw new ArgumentException("startOffset is bigger than length", nameof(startOffset));

			if ((bit != 8) && (bit != 16))
				throw new ArgumentException("Number of bits may only be 8 or 16", nameof(bit));

			this.sampleNumber = sampleNumber;

			sampleAddresses[0] = leftAdr;
			sampleAddresses[1] = rightAdr;

			sampleStart = startOffset;
			sampleLength = length;
			flags |= ChannelFlags.TrigIt;

			if (bit == 16)
				flags |= ChannelFlags._16Bit;

			if (backwards)
				flags |= ChannelFlags.Backwards;

			flags &= ~ChannelFlags.MuteIt;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point in the sample
		/// </summary>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		/********************************************************************/
		public void SetLoop(uint startOffset, uint length, ChannelLoopType type)
		{
			if (startOffset > (sampleStart + sampleLength))
				throw new ArgumentException("Start offset is bigger than previous set length of sample", nameof(startOffset));

			if ((startOffset + length) > (sampleStart + sampleLength))
				throw new ArgumentException("Loop length is bigger than previous set length of sample", nameof(length));

			SetLoopInfo(sampleAddresses[0], sampleAddresses[1], startOffset, length, type);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point and change the sample
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		/********************************************************************/
		public void SetLoop(Array adr, uint startOffset, uint length, ChannelLoopType type)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if ((adr.GetType() != typeof(sbyte[])) && (adr.GetType() != typeof(short[])))
				throw new ArgumentException("Type of array must be either sbyte[] or short[]", nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (sampleAddresses[1] != null)
				throw new InvalidOperationException("Can only be used on mono samples");

			SetLoopInfo(adr, null, startOffset, length, type);
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point and change the sample
		/// </summary>
		/// <param name="leftAdr">is a pointer to the sample in memory to be played in the left speaker</param>
		/// <param name="rightAdr">is a pointer to the sample in memory to be played in the right speaker</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		/********************************************************************/
		public void SetLoop(Array leftAdr, Array rightAdr, uint startOffset, uint length, ChannelLoopType type)
		{
			if (leftAdr == null)
				throw new ArgumentNullException(nameof(leftAdr));

			if (rightAdr == null)
				throw new ArgumentNullException(nameof(rightAdr));

			if (leftAdr.GetType() != rightAdr.GetType())
				throw new ArgumentException("Left and right speaker arrays must be of the same type", nameof(leftAdr));

			if (leftAdr.Length != rightAdr.Length)
				throw new ArgumentException("Left and right speaker arrays must be of equal size", nameof(leftAdr));

			if ((leftAdr.GetType() != typeof(sbyte[])) && (leftAdr.GetType() != typeof(short[])))
				throw new ArgumentException("Type of array must be either sbyte[] or short[]", nameof(leftAdr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			SetLoopInfo(leftAdr, rightAdr, startOffset, length, type);
		}



		/********************************************************************/
		/// <summary>
		/// Set the current playing position of the sample to the value given
		/// </summary>
		/// <param name="position">The new position in the sample</param>
		/// <param name="relative">Indicate if the given position is relative to the current position or an absolute position. If true, position can be negative as well as position</param>
		/********************************************************************/
		public void SetPosition(int position, bool relative)
		{
			if (!relative && (position < 0))
				throw new ArgumentException("Position may not be negative for absolute positions", nameof(position));

			samplePosition = position;
			flags |= ChannelFlags.ChangePosition;

			if (relative)
				flags |= ChannelFlags.Relative;
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the release part of the sample
		/// </summary>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to play</param>
		/********************************************************************/
		public void PlayReleasePart(uint startOffset, uint length)
		{
			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			loopAddresses[0] = sampleAddresses[0];
			loopAddresses[1] = sampleAddresses[1];

			loopStart = startOffset;
			releaseLength = length;
			flags |= ChannelFlags.Release;

			flags &= ~ChannelFlags.MuteIt;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the volume
		/// </summary>
		/// <param name="vol">is the new volume (0-256)</param>
		/********************************************************************/
		public void SetVolume(ushort vol)
		{
			if (vol > 256)
				throw new ArgumentException("Volume should be between 0 and 256", nameof(vol));

			volume = vol;
			flags |= ChannelFlags.Volume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the volume using Amiga range
		/// </summary>
		/// <param name="vol">is the new volume (0-64)</param>
		/********************************************************************/
		public void SetAmigaVolume(ushort vol)
		{
			if (vol > 64)
				throw new ArgumentException("Volume should be between 0 and 64", nameof(vol));

			volume = (ushort)(vol * 4);
			flags |= ChannelFlags.Volume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the panning
		/// </summary>
		/// <param name="pan">is the new panning</param>
		/********************************************************************/
		public void SetPanning(ushort pan)
		{
			if ((pan > 256) && (pan != (ushort)ChannelPanning.Surround))
				throw new ArgumentException("Panning should be between 0 and 256 or 512 (Surround)", nameof(pan));

			panning = pan;
			flags |= ChannelFlags.Panning;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the frequency
		/// </summary>
		/// <param name="freq">is the new frequency</param>
		/********************************************************************/
		public void SetFrequency(uint freq)
		{
			frequency = freq;
			flags |= ChannelFlags.Frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate the period given to a frequency and set it
		/// </summary>
		/// <param name="period">is the new frequency as an Amiga period</param>
		/********************************************************************/
		public void SetAmigaPeriod(uint period)
		{
			frequency = period != 0 ? 3546895 / period : 0;
			flags |= ChannelFlags.Frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true or false depending on the channel is in use
		/// </summary>
		/********************************************************************/
		public bool IsActive => (flags & ChannelFlags.Active) != 0;



		/********************************************************************/
		/// <summary>
		/// Mute the channel
		/// </summary>
		/********************************************************************/
		public void Mute()
		{
			flags |= ChannelFlags.MuteIt;
			flags &= ~ChannelFlags.Active;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current sample number used on the channel
		/// </summary>
		/********************************************************************/
		public short GetSampleNumber()
		{
			return sampleNumber;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current volume on the channel
		/// </summary>
		/********************************************************************/
		public ushort GetVolume()
		{
			return volume;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current frequency on the channel
		/// </summary>
		/********************************************************************/
		public uint GetFrequency()
		{
			return frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the length of the sample in samples
		/// </summary>
		/********************************************************************/
		public uint GetSampleLength()
		{
			return sampleLength;
		}



		/********************************************************************/
		/// <summary>
		/// Returns new sample position if set
		/// </summary>
		/********************************************************************/
		public int GetSamplePosition()
		{
			return samplePosition;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Set the loop information in internal variables
		/// </summary>
		/********************************************************************/
		private void SetLoopInfo(Array leftAdr, Array rightAdr, uint startOffset, uint length, ChannelLoopType type)
		{
			loopAddresses[0] = leftAdr;
			loopAddresses[1] = rightAdr;

			loopStart = startOffset;
			loopLength = length;
			flags |= ChannelFlags.Loop;

			if (type == ChannelLoopType.PingPong)
				flags |= ChannelFlags.PingPong;

			flags &= ~ChannelFlags.MuteIt;
		}
		#endregion
	}
}
