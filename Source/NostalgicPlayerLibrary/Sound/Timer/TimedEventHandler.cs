/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer
{
	/// <summary>
	/// This handler will fire events at a specific time (or nearby).
	/// The time is measured in number of samples played from the beginning
	/// of a module
	/// </summary>
	internal class TimedEventHandler
	{
		#region EventItem
		private readonly struct EventItem : IComparable<EventItem>
		{
			public readonly long ExecutionTime;
			public readonly ITimedEvent TimedEvent;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public EventItem(long executionTime, ITimedEvent timedEvent)
			{
				ExecutionTime = executionTime;
				TimedEvent = timedEvent;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public int CompareTo(EventItem other)
			{
				return ExecutionTime.CompareTo(other.ExecutionTime);
			}
		}
		#endregion

		private int mixerFrequency;
		private int outputLatencyInFrames;
		private int currentLatency;
		private int latencyInFrames;

		private long currentTimeInFrames;

		private readonly List<EventItem> events;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TimedEventHandler()
		{
			currentTimeInFrames = 0;
			events = new List<EventItem>();
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public void SetOutputFormat(OutputInfo outputInformation)
		{
			mixerFrequency = outputInformation.Frequency;
			outputLatencyInFrames = outputInformation.BufferSizeInFrames;

			CalculateLatency();
		}



		/********************************************************************/
		/// <summary>
		/// Set the latency
		/// </summary>
		/********************************************************************/
		public void SetLatency(int latency)
		{
			currentLatency = latency;

			CalculateLatency();
		}



		/********************************************************************/
		/// <summary>
		/// Increase the current time by the number of frames given
		/// </summary>
		/********************************************************************/
		public void IncreaseCurrentTime(int numberOfFrames)
		{
			currentTimeInFrames += numberOfFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Add an event. The time given is relative to the current time
		/// </summary>
		/********************************************************************/
		public void AddEvent(ITimedEvent timedEvent, int executionTime)
		{
			events.Add(new EventItem(currentTimeInFrames + executionTime + latencyInFrames, timedEvent));
		}



		/********************************************************************/
		/// <summary>
		/// Remove events of a given type
		/// </summary>
		/********************************************************************/
		public void RemoveEvents(ITimedEvent timedEvent)
		{
			Type searchType = timedEvent.GetType();

			for (int i = events.Count - 1; i >= 0; i--)
			{
				if (events[i].TimedEvent.GetType() == searchType)
					events.RemoveAt(i);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will execute all events before the current time
		/// </summary>
		/********************************************************************/
		public void DoEvents()
		{
			events.Sort();

			while (events.Count > 0)
			{
				EventItem item = events[0];
				if (item.ExecutionTime > currentTimeInFrames)
					break;

				item.TimedEvent.Execute((int)(currentTimeInFrames - item.ExecutionTime));

				events.RemoveAt(0);
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the current latency
		/// </summary>
		/********************************************************************/
		private void CalculateLatency()
		{
			latencyInFrames = (int)(((float)mixerFrequency / 1000) * currentLatency) + outputLatencyInFrames;
		}
		#endregion
	}
}
