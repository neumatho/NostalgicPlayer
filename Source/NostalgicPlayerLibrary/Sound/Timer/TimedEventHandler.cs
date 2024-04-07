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
		private int outputLatencyInSamples;
		private int currentLatency;
		private int latencyInSamples;

		private long currentTimeInSamples;

		private readonly List<EventItem> events;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TimedEventHandler()
		{
			currentTimeInSamples = 0;
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
			outputLatencyInSamples = outputInformation.BufferSizeInSamples / outputInformation.Channels;

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
		/// Increase the current time by the number of samples given
		/// </summary>
		/********************************************************************/
		public void IncreaseCurrentTime(int numberOfSamples)
		{
			currentTimeInSamples += numberOfSamples;
		}



		/********************************************************************/
		/// <summary>
		/// Add an event. The time given is relative to the current time
		/// </summary>
		/********************************************************************/
		public void AddEvent(ITimedEvent timedEvent, int executionTime)
		{
			events.Add(new EventItem(currentTimeInSamples + executionTime + latencyInSamples, timedEvent));
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
				if (item.ExecutionTime > currentTimeInSamples)
					break;

				item.TimedEvent.Execute((int)(currentTimeInSamples - item.ExecutionTime));

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
			latencyInSamples = (int)(((float)mixerFrequency / 1000) * currentLatency) + outputLatencyInSamples;
		}
		#endregion
	}
}
