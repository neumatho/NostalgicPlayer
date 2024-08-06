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
	/// This class is an empty implementation of the IChannel and is
	/// e.g. used when calculation duration of a module
	/// </summary>
	internal class DummyChannel : IChannel
	{
		private uint bufferLength;

		#region IChannel implementation
		/********************************************************************/
		/// <summary>
		/// Will start to play the buffer in the channel. Only use this if
		/// your player is running in buffer mode. Note that the length then
		/// have to be the same for each channel
		/// </summary>
		/********************************************************************/
		public void PlayBuffer(Array adr, uint startOffset, uint length, PlayBufferFlag flag)
		{
			bufferLength = length;
		}



		/********************************************************************/
		/// <summary>
		/// Will start to play the sample in the channel
		/// </summary>
		/********************************************************************/
		public void PlaySample(short sampleNumber, Array adr, uint startOffset, uint length, PlaySampleFlag flag)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will play the sample in the channel, but first when the current
		/// sample stops or loops. No retrigger is made
		/// </summary>
		/********************************************************************/
		public void SetSample(uint startOffset, uint length)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will play the sample in the channel, but first when the current
		/// sample stops or loops. No retrigger is made
		/// </summary>
		/********************************************************************/
		public void SetSample(Array adr, uint startOffset, uint length, PlaySampleFlag flag)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point in the sample
		/// </summary>
		/********************************************************************/
		public void SetLoop(uint startOffset, uint length, ChannelLoopType type)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will set the loop point and change the sample
		/// </summary>
		/********************************************************************/
		public void SetLoop(Array adr, uint startOffset, uint length, ChannelLoopType type)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Set the current playing position of the sample to the value given
		/// </summary>
		/********************************************************************/
		public void SetPosition(int position, bool relative)
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
		/// Will change the volume using Amiga range
		/// </summary>
		/********************************************************************/
		public void SetAmigaVolume(ushort vol)
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
		/// Will update the current sample number being used
		/// </summary>
		/********************************************************************/
		public void SetSampleNumber(short sampleNumber)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will tell visuals the correct note and octave being played. This
		/// can be used when playing multi-octave samples. If this is not
		/// called, the old behaviour is used by using the frequency the
		/// sample is played with to find the note
		/// </summary>
		/********************************************************************/
		public void SetNote(byte octave, byte note)
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
		/// Returns true or false depending on the channel is muted or not
		/// </summary>
		/********************************************************************/
		public bool IsMuted => false;



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
		/// Returns the current sample number used on the channel
		/// </summary>
		/********************************************************************/
		public short GetSampleNumber()
		{
			return -1;
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



		/********************************************************************/
		/// <summary>
		/// Returns the length of the sample in samples
		/// </summary>
		/********************************************************************/
		public uint GetSampleLength()
		{
			return bufferLength;
		}



		/********************************************************************/
		/// <summary>
		/// Returns new sample position if set
		/// </summary>
		/********************************************************************/
		public int GetSamplePosition()
		{
			return 0;
		}
		#endregion
	}
}
