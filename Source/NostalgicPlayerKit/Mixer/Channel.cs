/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Kit.Mixer
{
	/// <summary>
	/// This class is used to trigger samples when playing modules
	/// </summary>
	public class Channel
	{
		/// <summary>
		/// The different kind of loops supported
		/// </summary>
		public enum LoopType
		{
			/// <summary>
			/// Just a normal loop
			/// </summary>
			Normal,

			/// <summary>
			/// Ping-pong loop
			/// </summary>
			PingPong,

			/// <summary>
			/// Set this to trigger the sample
			/// </summary>
			Trigger
		}

		/// <summary>
		/// Channel flags
		/// </summary>
		[Flags]
		public enum Flags : uint
		{
			/// <summary>
			/// No flags
			/// </summary>
			None = 0,

			/// <summary>
			/// Mute the channel
			/// </summary>
			MuteIt = 0x00000001,

			/// <summary>
			/// Trig the sample (start over)
			/// </summary>
			TrigIt = 0x00000002,

			/// <summary>
			/// Set this if the sample is 16 bit
			/// </summary>
			_16Bit = 0x00000004,

			/// <summary>
			/// The sample loops
			/// </summary>
			Loop = 0x00000008,

			/// <summary>
			/// Set this together with the Loop flag for ping-pong loop
			/// </summary>
			PingPong = 0x00000010,

			/// <summary>
			/// Set this to trigger the sample when setting looping information
			/// </summary>
			TrigLoop = 0x00000020,

			/// <summary>
			/// Speaker volume changed. Overrules Volume and Panning
			/// </summary>
			SpeakerVolume = 0x00000100,

			/// <summary>
			/// Volume changed
			/// </summary>
			Volume = 0x00000200,

			/// <summary>
			/// Panning changed
			/// </summary>
			Panning = 0x00000400,

			/// <summary>
			/// New frequency
			/// </summary>
			Frequency = 0x0001000,

			/// <summary>
			/// Release the sample
			/// </summary>
			Release = 0x00002000,

			/// <summary>
			/// This is a read-only bit. When a sample is playing in the channel, it's set
			/// </summary>
			Active = 0x80000000
		}

		/// <summary>
		/// Indicate what has been set/changed
		/// </summary>
		protected Flags flags;

		/// <summary>
		/// Start address of the sample
		/// </summary>
		protected sbyte[] sampAddress;

		/// <summary>
		/// Start address of the loop/release sample
		/// </summary>
		protected sbyte[] loopAddress;

		/// <summary>
		/// Start offset in the sample in samples, not bytes
		/// </summary>
		protected uint sampStart;

		/// <summary>
		/// Length of the sample in samples, not bytes
		/// </summary>
		protected uint sampLength;

		/// <summary>
		/// Loop offset in the sample in samples, not bytes
		/// </summary>
		protected uint loopStart;

		/// <summary>
		/// Loop length in samples, not bytes
		/// </summary>
		protected uint loopLength;

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

		/********************************************************************/
		/// <summary>
		/// Will start to play the sample in the channel
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples of the sample</param>
		/// <param name="bit">is the number of bits each sample are, e.g. 8 or 16</param>
		/********************************************************************/
		public void PlaySample(sbyte[] adr, uint startOffset, uint length, byte bit = 8)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset > adr.Length)
				throw new ArgumentException("startOffset is bigger than length", nameof(startOffset));

			if ((bit != 8) && (bit != 16))
				throw new ArgumentException("Number of bits may only be 8 or 16", nameof(bit));

			sampAddress = adr;
			sampStart = startOffset;
			sampLength = length;
			flags |= Flags.TrigIt;

			if (bit == 16)
				flags |= Flags._16Bit;

			flags &= ~Flags.MuteIt;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point in the sample
		/// </summary>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to loop</param>
		/// <param name="type">is the type of the loop</param>
		/********************************************************************/
		public void SetLoop(uint startOffset, uint length, LoopType type = LoopType.Normal)
		{
			if (startOffset > (sampStart + sampLength))
				throw new ArgumentException("Start offset is bigger than previous set length of sample", nameof(startOffset));

			if ((startOffset + length) > (sampStart + sampLength))
				throw new ArgumentException("Loop length is bigger than previous set length of sample", nameof(length));

			SetLoop(sampAddress, startOffset, length, type);
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
		public void SetLoop(sbyte[] adr, uint startOffset, uint length, LoopType type = LoopType.Normal)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			loopAddress = adr;
			loopStart = startOffset;
			loopLength = length;
			flags |= Flags.Loop;

			if (type == LoopType.PingPong)
				flags |= Flags.PingPong;
			else if (type == LoopType.Trigger)
				flags |= Flags.TrigLoop;

			flags &= ~Flags.MuteIt;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the volume
		/// </summary>
		/// <param name="vol">is the new volume</param>
		/********************************************************************/
		public void SetVolume(ushort vol)
		{
			if (vol > 256)
				throw new ArgumentException("Volume should be between 0 and 256", nameof(vol));

			volume = vol;
			flags |= Flags.Volume;
			flags &= ~Flags.SpeakerVolume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the panning
		/// </summary>
		/// <param name="pan">is the new panning</param>
		/********************************************************************/
		public void SetPanning(ushort pan)
		{
			if ((pan > 256) && (pan != (ushort)Panning.Surround))
				throw new ArgumentException("Panning should be between 0 and 256 or 512 (Surround)", nameof(pan));

			panning = pan;
			flags |= Flags.Panning;
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
			flags |= Flags.Frequency;
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
			flags |= Flags.Frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true or false depending on the channel is in use
		/// </summary>
		/********************************************************************/
		public bool IsActive => (flags & Flags.Active) != 0;



		/********************************************************************/
		/// <summary>
		/// Mute the channel
		/// </summary>
		/********************************************************************/
		public void Mute()
		{
			flags |= Flags.MuteIt;
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
	}
}
