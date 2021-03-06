/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
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
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public override void SetMasterVolume(int volume)
		{
			wrapperStream.SetMasterVolume(volume);
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
		/// Read mixed data
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = wrapperStream.Read(buffer, offset, count);
			if (read > 0)
				worker.SaveSampleBuffer(buffer, read, OutputInfo.BytesPerSample * 8);

			return read;
		}
		#endregion
	}
}
