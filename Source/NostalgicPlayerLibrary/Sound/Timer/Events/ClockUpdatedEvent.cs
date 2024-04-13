/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events
{
	/// <summary>
	/// Event for the module clock / timer
	/// </summary>
	internal class ClockUpdatedEvent : ITimedEvent
	{
		private readonly SoundBase soundBase;
		private readonly TimedEventHandler noLatencyTimedEventHandler;

		private int mixerFrequency;

		private int playedSeconds;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ClockUpdatedEvent(SoundBase soundBase, TimedEventHandler noLatencyTimedEventHandler)
		{
			this.soundBase = soundBase;
			this.noLatencyTimedEventHandler = noLatencyTimedEventHandler;

			playedSeconds = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Do whatever this event want to do
		/// </summary>
		/********************************************************************/
		public void Execute(int differenceTime)
		{
			playedSeconds++;
			soundBase.OnClockUpdated(new ClockUpdatedEventArgs(TimeSpan.FromSeconds(playedSeconds)));

			noLatencyTimedEventHandler.AddEvent(this, mixerFrequency - differenceTime);
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			int oldMixerFrequency = mixerFrequency;
			mixerFrequency = outputInformation.Frequency;

			if (oldMixerFrequency == 0)
				noLatencyTimedEventHandler.AddEvent(this, mixerFrequency);
		}



		/********************************************************************/
		/// <summary>
		/// Restart the position
		/// </summary>
		/********************************************************************/
		public void SetClockToGivenTime(double timeInMilliseconds)
		{
			playedSeconds = (int)(timeInMilliseconds / 1000);

			int samplesLeftToClockUpdate = (int)(((timeInMilliseconds - (playedSeconds * 1000)) / 1000.0f) * mixerFrequency);

			noLatencyTimedEventHandler.RemoveEvents(this);
			noLatencyTimedEventHandler.AddEvent(this, mixerFrequency - samplesLeftToClockUpdate);

			soundBase.OnClockUpdated(new ClockUpdatedEventArgs(TimeSpan.FromSeconds(playedSeconds)));
		}
	}
}
