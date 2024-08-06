/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Containers.Types;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer
{
	/// <summary>
	/// This class is used to trigger samples when playing modules
	/// </summary>
	internal class Channel : IChannel
	{
		protected class Sample
		{
			/// <summary>
			/// Sample data
			/// </summary>
			public Array SampleData;

			/// <summary>
			/// Start offset or loop offset in the sample in samples, not bytes
			/// </summary>
			public uint Start;

			/// <summary>
			/// Length or loop length of the sample in samples, not bytes
			/// </summary>
			public uint Length;
		}

		protected class SampleInfo
		{
			/// <summary>
			/// Indicate what has been set/changed
			/// </summary>
			public ChannelSampleFlag Flags;

			/// <summary>
			/// Start addresses of the sample
			/// </summary>
			public Sample Sample = new Sample();


			/// <summary>
			/// Start address of the loop/release sample
			/// </summary>
			public Sample Loop;
		}

		/// <summary>
		/// Indicate what has been set/changed
		/// </summary>
		protected ChannelFlag flags;

		/// <summary>
		/// The sample number being played or -1 if unknown
		/// </summary>
		protected short currentSampleNumber;

		/// <summary>
		/// The octave being played or -1 if not set
		/// </summary>
		protected sbyte currentOctave;

		/// <summary>
		/// The note being played or -1 if not set
		/// </summary>
		protected sbyte currentNote;

		/// <summary>
		/// Holds information about the sample to be played
		/// </summary>
		protected SampleInfo sampleInfo = new SampleInfo();

		/// <summary>
		/// Holds information about a sample to use when the current sample stops or loops
		/// </summary>
		protected SampleInfo newSampleInfo;

		/// <summary>
		/// The new sample position to set
		/// </summary>
		protected int samplePosition;

		/// <summary>
		/// The frequency to play with
		/// </summary>
		protected uint frequency;

		/// <summary>
		/// The volume (0-256)
		/// </summary>
		protected ushort volume;

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
		/// <param name="length">is the length in samples to be played</param>
		/// <param name="flag">indicate the format of the sample and how to play it</param>
		/********************************************************************/
		public void PlayBuffer(Array adr, uint startOffset, uint length, PlayBufferFlag flag)
		{
			PlaySampleFlag playFlag = PlaySampleFlag.None;

			if ((flag & PlayBufferFlag._16Bit) != 0)
				playFlag |= PlaySampleFlag._16Bit;

			PlaySample(-1, adr, startOffset, length, playFlag);
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the sample in the channel
		/// </summary>
		/// <param name="sampleNumber">is the sample number being played. If unknown, set it to -1</param>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to be played</param>
		/// <param name="flag">indicate the format of the sample and how to play it</param>
		/********************************************************************/
		public void PlaySample(short sampleNumber, Array adr, uint startOffset, uint length, PlaySampleFlag flag)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if ((adr.GetType() != typeof(sbyte[])) && (adr.GetType() != typeof(short[])) && (adr.GetType() != typeof(byte[])))
				throw new ArgumentException("Type of array must be either sbyte[], short[] or byte[]", nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset >= adr.Length)
				throw new ArgumentException("startOffset is bigger than sample array", nameof(startOffset));

			if ((startOffset + length) > adr.Length)
				throw new ArgumentException("startOffset+length is bigger than sample array", nameof(length));

			SetPlaySampleInfo(sampleNumber, adr, startOffset, length, flag);
		}



		/********************************************************************/
		/// <summary>
		/// Will play the sample in the channel, but first when the current
		/// sample stops or loops. No retrigger is made
		/// </summary>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to be played</param>
		/********************************************************************/
		public void SetSample(uint startOffset, uint length)
		{
			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset >= sampleInfo.Sample.SampleData.Length)
				throw new ArgumentException("startOffset is bigger than sample array", nameof(startOffset));

			if ((startOffset + length) > sampleInfo.Sample.SampleData.Length)
				throw new ArgumentException("startOffset+length is bigger than sample array", nameof(length));

			SetNewSampleInfo(sampleInfo.Sample.SampleData, startOffset, length, PlaySampleFlag.None);
			newSampleInfo.Flags = sampleInfo.Flags;
		}



		/********************************************************************/
		/// <summary>
		/// Will play the sample in the channel, but first when the current
		/// sample stops or loops. No retrigger is made
		/// </summary>
		/// <param name="adr">is a pointer to the sample in memory</param>
		/// <param name="startOffset">is the number of samples in the sample to start</param>
		/// <param name="length">is the length in samples to be played</param>
		/// <param name="flag">indicate the format of the sample and how to play it</param>
		/********************************************************************/
		public void SetSample(Array adr, uint startOffset, uint length, PlaySampleFlag flag)
		{
			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if ((adr.GetType() != typeof(sbyte[])) && (adr.GetType() != typeof(short[])) && (adr.GetType() != typeof(byte[])))
				throw new ArgumentException("Type of array must be either sbyte[], short[] or byte[]", nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset >= adr.Length)
				throw new ArgumentException("startOffset is bigger than sample array", nameof(startOffset));

			if ((startOffset + length) > adr.Length)
				throw new ArgumentException("startOffset+length is bigger than sample array", nameof(length));

			SetNewSampleInfo(adr, startOffset, length, flag);
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
			SampleInfo sampleInf = newSampleInfo ?? sampleInfo;

			if (startOffset >= sampleInf.Sample.SampleData.Length)
				throw new ArgumentException("Start offset is bigger than previous set length of sample", nameof(startOffset));

			if ((startOffset + length) > sampleInf.Sample.SampleData.Length)
				throw new ArgumentException("Start offset + loop length is bigger than length of sample", nameof(length));

			SetLoopInfo(sampleInf, sampleInf.Sample.SampleData, startOffset, length, type);
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
			SampleInfo sampleInf = newSampleInfo ?? sampleInfo;

			if (adr == null)
				throw new ArgumentNullException(nameof(adr));

			if ((adr.GetType() != typeof(sbyte[])) && (adr.GetType() != typeof(short[])) && (adr.GetType() != typeof(byte[])))
				throw new ArgumentException("Type of array must be either sbyte[], short[] or byte[]", nameof(adr));

			if (length == 0)
				throw new ArgumentException("Length may not be zero", nameof(length));

			if (startOffset >= adr.Length)
				throw new ArgumentException("Start offset is bigger than length of sample", nameof(startOffset));

			if ((startOffset + length) > adr.Length)
				throw new ArgumentException("Start offset + loop length is bigger than length of sample", nameof(length));

			SetLoopInfo(sampleInf, adr, startOffset, length, type);
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
			flags |= ChannelFlag.ChangePosition;

			if (relative)
				flags |= ChannelFlag.Relative;
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
			flags |= ChannelFlag.Volume;
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
			flags |= ChannelFlag.Volume;
		}



		/********************************************************************/
		/// <summary>
		/// Will change the panning
		/// </summary>
		/// <param name="pan">is the new panning</param>
		/********************************************************************/
		public void SetPanning(ushort pan)
		{
			if ((pan > 256) && (pan != (ushort)ChannelPanningType.Surround))
				throw new ArgumentException("Panning should be between 0 and 256 or 512 (Surround)", nameof(pan));

			panning = pan;
			flags |= ChannelFlag.Panning;
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
			flags |= ChannelFlag.Frequency;
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
			flags |= ChannelFlag.Frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Will update the current sample number being used
		/// </summary>
		/// <param name="sampleNumber">is the sample number being played. If unknown, set it to -1</param>
		/********************************************************************/
		public void SetSampleNumber(short sampleNumber)
		{
			currentSampleNumber = sampleNumber;
			flags |= ChannelFlag.VirtualTrig;
		}



		/********************************************************************/
		/// <summary>
		/// Will tell visuals the correct note and octave being played. This
		/// can be used when playing multi-octave samples. If this is not
		/// called, the old behaviour is used by using the frequency the
		/// sample is played with to find the note
		/// </summary>
		/// <param name="octave">is the octave between 0 and 9</param>
		/// <para> name="note" is the note in the octave between 0 and 11</para>
		/********************************************************************/
		public void SetNote(byte octave, byte note)
		{
			if (octave > 9)
				throw new ArgumentException("Octave should be between 0 and 9", nameof(octave));

			if (note > 11)
				throw new ArgumentException("Note should be between 0 and 11", nameof(note));

			currentOctave = (sbyte)octave;
			currentNote = (sbyte)note;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true or false depending on the channel is in use
		/// </summary>
		/********************************************************************/
		public bool IsActive => (flags & ChannelFlag.Active) != 0;



		/********************************************************************/
		/// <summary>
		/// Returns true or false depending on the channel is muted or not
		/// </summary>
		/********************************************************************/
		public bool IsMuted => (flags & ChannelFlag.MuteIt) != 0;



		/********************************************************************/
		/// <summary>
		/// Mute the channel
		/// </summary>
		/********************************************************************/
		public void Mute()
		{
			flags |= ChannelFlag.MuteIt;
			flags &= ~ChannelFlag.Active;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the current sample number used on the channel
		/// </summary>
		/********************************************************************/
		public short GetSampleNumber()
		{
			return currentSampleNumber;
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
			return sampleInfo.Sample.Length;
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
		/// Set the sample information in internal variables
		/// </summary>
		/********************************************************************/
		private void SetPlaySampleInfo(short sampleNumber, Array sampleData, uint startOffset, uint length, PlaySampleFlag playFlag)
		{
			currentSampleNumber = sampleNumber;
			currentOctave = -1;
			currentNote = -1;

			flags |= ChannelFlag.TrigIt;
			flags &= ~ChannelFlag.MuteIt;

			newSampleInfo = null;
			sampleInfo.Loop = null;

			SetSampleInfo(sampleInfo, sampleData, startOffset, length, playFlag);
		}



		/********************************************************************/
		/// <summary>
		/// Set new sample information in internal variables
		/// </summary>
		/********************************************************************/
		private void SetNewSampleInfo(Array sampleData, uint startOffset, uint length, PlaySampleFlag playFlag)
		{
			newSampleInfo = new SampleInfo();

			SetSampleInfo(newSampleInfo, sampleData, startOffset, length, playFlag);
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample information in internal variables
		/// </summary>
		/********************************************************************/
		private void SetSampleInfo(SampleInfo sampleInf, Array sampleData, uint startOffset, uint length, PlaySampleFlag playFlag)
		{
			sampleInf.Sample.SampleData = sampleData;
			sampleInf.Sample.Start = startOffset;
			sampleInf.Sample.Length = length;
			sampleInf.Flags = ChannelSampleFlag.None;

			if ((playFlag & PlaySampleFlag._16Bit) != 0)
				sampleInf.Flags |= ChannelSampleFlag._16Bit;

			if ((playFlag & PlaySampleFlag.Stereo) != 0)
				sampleInf.Flags |= ChannelSampleFlag.Stereo;

			if ((playFlag & PlaySampleFlag.Backwards) != 0)
				sampleInf.Flags |= ChannelSampleFlag.Backwards;
		}



		/********************************************************************/
		/// <summary>
		/// Set the loop information in internal variables
		/// </summary>
		/********************************************************************/
		private void SetLoopInfo(SampleInfo sampleInf, Array sampleData, uint startOffset, uint length, ChannelLoopType type)
		{
			sampleInf.Loop = new Sample
			{
				SampleData = sampleData,
				Start = startOffset,
				Length = length
			};

			if (type == ChannelLoopType.PingPong)
				sampleInf.Flags |= ChannelSampleFlag.PingPong;
		}
		#endregion
	}
}
