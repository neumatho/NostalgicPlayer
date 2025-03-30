/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Output.DiskSaver
{
	/// <summary>
	/// Used as middle-man between the pass-through output agent and the mixer
	/// </summary>
	internal class SaverStream : SoundStream
	{
		private readonly DiskSaverWorker worker;
		private readonly SoundStream wrapperStream;

		/********************************************************************/
		/// <summary>
		/// Initialize the stream
		/// </summary>
		/********************************************************************/
		public SaverStream(DiskSaverWorker worker, SoundStream wrapper)
		{
			this.worker = worker;
			wrapperStream = wrapper;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the pass-through output agent output info
		/// </summary>
		/********************************************************************/
		public OutputInfo OutputInfo
		{
			get; private set;
		}

		#region SoundStream implementation
		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInfo)
		{
			OutputInfo = outputInfo;

			wrapperStream.SetOutputFormat(outputInfo);
		}



		/********************************************************************/
		/// <summary>
		/// Return which speakers that are used to play the sound. Is first
		/// available after calling SetOutputFormat()
		/// </summary>
		/********************************************************************/
		public override SpeakerFlag VisualizerSpeakers => wrapperStream.VisualizerSpeakers;



		/********************************************************************/
		/// <summary>
		/// Start the playing
		/// </summary>
		/********************************************************************/
		public override void Start()
		{
			wrapperStream.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Stop the playing
		/// </summary>
		/********************************************************************/
		public override void Stop()
		{
			wrapperStream.Stop();
		}



		/********************************************************************/
		/// <summary>
		/// Pause the playing
		/// </summary>
		/********************************************************************/
		public override void Pause()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Resume the playing
		/// </summary>
		/********************************************************************/
		public override void Resume()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Set the stream to the given song position
		/// </summary>
		/********************************************************************/
		public override int SongPosition
		{
			get => wrapperStream.SongPosition;
			set => wrapperStream.SongPosition = value;
		}



		/********************************************************************/
		/// <summary>
		/// Read mixed data
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offsetInBytes, int frameCount)
		{
			int framesRead = wrapperStream.Read(buffer, offsetInBytes, frameCount);
			if (framesRead > 0)
			{
				byte[] newBuffer = FixBuffer(buffer, offsetInBytes, framesRead);
				worker.SaveSampleBuffer(newBuffer, framesRead, OutputInfo.BytesPerSample * 8);
			}

			return framesRead;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will make sure that the returned buffer only contain max 2
		/// channels
		/// </summary>
		/********************************************************************/
		private byte[] FixBuffer(byte[] buffer, int offsetInBytes, int framesCount)
		{
			int countInSamples = framesCount * OutputInfo.Channels;

			if (OutputInfo.Channels > 2)
			{
				byte[] newBuffer = new byte[framesCount * 2 * OutputInfo.BytesPerSample];

				Span<int> source = MemoryMarshal.Cast<byte, int>(buffer.AsSpan(offsetInBytes, countInSamples * OutputInfo.BytesPerSample));
				Span<int> dest = MemoryMarshal.Cast<byte, int>(newBuffer);

				int channelsToSkip = OutputInfo.Channels - 2;
				int sourceOffset = 0;
				int destOffset = 0;

				while (sourceOffset < countInSamples)
				{
					dest[destOffset++] = source[sourceOffset++];
					dest[destOffset++] = source[sourceOffset++];

					sourceOffset += channelsToSkip;
				}

				return newBuffer;
			}

			if (offsetInBytes > 0)
				return buffer.AsSpan(offsetInBytes, countInSamples * OutputInfo.BytesPerSample).ToArray();

			return buffer;
		}
		#endregion
	}
}
