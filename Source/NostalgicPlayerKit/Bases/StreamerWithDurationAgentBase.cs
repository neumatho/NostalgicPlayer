﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Derive from this class, if you want to calculate the duration in your streamer player
	/// </summary>
	public abstract class StreamerWithDurationAgentBase : StreamerAgentBase, IStreamerDuration
	{
		private TimeSpan duration;
		private bool hasCalculatedDuration = false;

		#region IStreamerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(StreamingStream streamingStream, IMetadata metadata, out string errorMessage)
		{
			if (!base.InitPlayer(streamingStream, metadata, out errorMessage))
				return false;

			duration = metadata?.Duration ?? TimeSpan.Zero;

			return true;
		}
		#endregion

		#region ISampleDuration implementation
		/********************************************************************/
		/// <summary>
		/// Calculate the duration for all sub-songs
		/// </summary>
		/********************************************************************/
		public DurationInfo[] CalculateDuration()
		{
			InitDuration();

			try
			{
				TimeSpan totalTime = GetTotalDuration();
				if (totalTime == TimeSpan.Zero)
					return null;

				TimeSpan increment = new TimeSpan(0, 0, (int)IDuration.NumberOfSecondsBetweenEachSnapshot);

				List<PositionInfo> positionInfoList = new List<PositionInfo>();

				TimeSpan currentTotalTime = TimeSpan.Zero;
				
				for (;;)
				{
					positionInfoList.Add(new PositionInfo(currentTotalTime));

					currentTotalTime += increment;
					if (currentTotalTime >= totalTime)
						break;
				}

				hasCalculatedDuration = true;

				return [ new DurationInfo(totalTime, positionInfoList.ToArray()) ];
			}
			finally
			{
				CleanupDuration();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will tell the player to change its current state to match the
		/// position given
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(PositionInfo positionInfo)
		{
			if (positionInfo != null)
				SetPosition(positionInfo.Time);
		}



		/********************************************************************/
		/// <summary>
		/// Return the time into the song when restarting
		/// </summary>
		/********************************************************************/
		public TimeSpan GetRestartTime()
		{
			return TimeSpan.Zero;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Initialize all internal structures when beginning duration
		/// calculation
		/// </summary>
		/********************************************************************/
		protected virtual void InitDuration()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup needed stuff after a sub-song calculation
		/// </summary>
		/********************************************************************/
		protected virtual void CleanupDuration()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the total time of the sample
		/// </summary>
		/********************************************************************/
		protected virtual TimeSpan GetTotalDuration()
		{
			return duration;
		}



		/********************************************************************/
		/// <summary>
		/// Set the position in the playing sample to the time given
		/// </summary>
		/********************************************************************/
		protected abstract void SetPosition(TimeSpan time);

		#region StreamerAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public override StreamerSupportFlag SupportFlags
		{
			get
			{
				StreamerSupportFlag flag = base.SupportFlags;

				if (hasCalculatedDuration && stream.CanSeek)
					flag |= StreamerSupportFlag.SetPosition;

				return flag;
			}
		}
		#endregion
	}
}
