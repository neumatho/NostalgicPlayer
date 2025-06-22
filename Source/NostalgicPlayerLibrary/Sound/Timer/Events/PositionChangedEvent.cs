/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events
{
	/// <summary>
	/// Event for position changes
	/// </summary>
	internal class PositionChangedEvent : ITimedEvent
	{
		private readonly SoundBase soundBase;
		private readonly TimedEventHandler noLatencyTimedEventHandler;

		private int mixerFrequency;

		private int currentSongPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PositionChangedEvent(SoundBase soundBase, TimedEventHandler noLatencyTimedEventHandler)
		{
			this.soundBase = soundBase;
			this.noLatencyTimedEventHandler = noLatencyTimedEventHandler;

			currentSongPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Do whatever this event want to do
		/// </summary>
		/********************************************************************/
		public void Execute(int differenceTime)
		{
			currentSongPosition++;
			soundBase.OnPositionChanged();

			noLatencyTimedEventHandler.AddEvent(this, CalculateSamplesPerPositionChange() - differenceTime);
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
				noLatencyTimedEventHandler.AddEvent(this, CalculateSamplesPerPositionChange());
		}



		/********************************************************************/
		/// <summary>
		/// Get current song position
		/// </summary>
		/********************************************************************/
		public int SongPosition
		{
			get => currentSongPosition;

			set
			{
				currentSongPosition = value;

				noLatencyTimedEventHandler.RemoveEvents(this);
				noLatencyTimedEventHandler.AddEvent(this, CalculateSamplesPerPositionChange());
			}
		}



		/********************************************************************/
		/// <summary>
		/// Restart the position
		/// </summary>
		/********************************************************************/
		public void RestartPosition(double restartTime)
		{
			int samplesPerPosition = CalculateSamplesPerPositionChange();

			currentSongPosition = (int)(restartTime / (IDuration.NumberOfSecondsBetweenEachSnapshot * 1000.0f));
			int samplesLeftToPositionChange = (int)(samplesPerPosition - (((restartTime - (currentSongPosition * IDuration.NumberOfSecondsBetweenEachSnapshot * 1000.0f)) / 1000.0f) * mixerFrequency));

			noLatencyTimedEventHandler.RemoveEvents(this);
			noLatencyTimedEventHandler.AddEvent(this, samplesLeftToPositionChange);

			soundBase.OnPositionChanged();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the number of samples between each position change
		/// </summary>
		/********************************************************************/
		private int CalculateSamplesPerPositionChange()
		{
			return (int)(IDuration.NumberOfSecondsBetweenEachSnapshot * mixerFrequency);
		}
		#endregion
	}
}
