/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Containers;
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
		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = wrapperStream.Read(buffer, offset, count);
			if (read > 0)
			{
				byte[] newBuffer = FixBuffer(buffer, offset, read);
				worker.SaveSampleBuffer(newBuffer, newBuffer.Length, OutputInfo.BytesPerSample * 8);
			}

			return read;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will make sure that the returned buffer only contain max 2
		/// channels
		/// </summary>
		/********************************************************************/
		private byte[] FixBuffer(byte[] buffer, int offset, int count)
		{
			if (OutputInfo.Channels > 2)
			{
				byte[] newBuffer = new byte[(count / OutputInfo.Channels) * 2];

				Span<int> source = MemoryMarshal.Cast<byte, int>(buffer.AsSpan(offset, count));
				Span<int> dest = MemoryMarshal.Cast<byte, int>(newBuffer);

				int channelsToSkip = OutputInfo.Channels - 2;
				int sourceOffset = 0;
				int destOffset = 0;

				while (sourceOffset < count / 4)
				{
					dest[destOffset++] = source[sourceOffset++];
					dest[destOffset++] = source[sourceOffset++];

					sourceOffset += channelsToSkip;
				}

				return newBuffer;
			}

			if (offset > 0)
				return buffer.AsSpan(offset, count).ToArray();

			return buffer;
		}
		#endregion
	}
}
