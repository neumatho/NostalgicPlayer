/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer;
using Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer.Events;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound
{
	/// <summary>
	/// Base class to all sound generators
	/// </summary>
	internal abstract class SoundBase
	{
		/// <summary></summary>
		protected TimedEventHandler timedEventHandler;
		/// <summary></summary>
		protected TimedEventHandler noLatencyTimedEventHandler;

		private int framesPlayedLastRound;

		private PositionChangedEvent positionChangedEvent;

		/********************************************************************/
		/// <summary>
		/// Event called when the position change
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;



		/********************************************************************/
		/// <summary>
		/// Event called when the player update some module information
		/// </summary>
		/********************************************************************/
		public event ModuleInfoChangedEventHandler ModuleInfoChanged;



		/********************************************************************/
		/// <summary>
		/// Send an event when the position change
		/// </summary>
		/********************************************************************/
		public void OnPositionChanged()
		{
			if (PositionChanged != null)
				PositionChanged(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Send an event when the module information change
		/// </summary>
		/********************************************************************/
		public void OnModuleInfoChanged(ModuleInfoChangedEventArgs e)
		{
			if (ModuleInfoChanged != null)
				ModuleInfoChanged(this, e);
		}



		/********************************************************************/
		/// <summary>
		/// Get current song position
		/// </summary>
		/********************************************************************/
		public int SongPosition
		{
			get => positionChangedEvent.SongPosition;

			set => positionChangedEvent.SongPosition = value;
		}



		/********************************************************************/
		/// <summary>
		/// Restart the position
		/// </summary>
		/********************************************************************/
		protected void RestartPosition(double restartPosition)
		{
			positionChangedEvent.RestartPosition(restartPosition);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Initialize the sound routines
		/// </summary>
		/********************************************************************/
		public virtual bool Initialize(Manager agentManager, PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup sound
		/// </summary>
		/********************************************************************/
		public virtual void Cleanup()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Starts the sound routines
		/// </summary>
		/********************************************************************/
		public virtual void Start()
		{
			// Initializer event timer
			timedEventHandler = new TimedEventHandler();
			noLatencyTimedEventHandler = new TimedEventHandler();
			framesPlayedLastRound = 0;

			positionChangedEvent = new PositionChangedEvent(this, noLatencyTimedEventHandler);
		}



		/********************************************************************/
		/// <summary>
		/// Stops the sound routines
		/// </summary>
		/********************************************************************/
		public virtual void Stop()
		{
			// Stop timer
			timedEventHandler = null;
			noLatencyTimedEventHandler = null;

			positionChangedEvent = null;
		}



		/********************************************************************/
		/// <summary>
		/// Pause the sound routines
		/// </summary>
		/********************************************************************/
		public virtual void Pause()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Resume the sound routines
		/// </summary>
		/********************************************************************/
		public virtual void Resume()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public virtual void SetOutputFormat(OutputInfo outputInformation)
		{
			timedEventHandler.SetOutputFormat(outputInformation);
			noLatencyTimedEventHandler.SetOutputFormat(outputInformation);

			positionChangedEvent.SetOutputFormat(outputInformation);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the configuration
		/// </summary>
		/********************************************************************/
		public virtual void ChangeConfiguration(MixerConfiguration mixerConfiguration)
		{
			timedEventHandler?.SetLatency(mixerConfiguration.VisualsLatency);
			noLatencyTimedEventHandler?.SetLatency(0);
		}
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Increase the current time by the number of samples given
		/// </summary>
		/********************************************************************/
		protected void IncreaseCurrentTime(int numberOfFrames)
		{
			timedEventHandler.IncreaseCurrentTime(framesPlayedLastRound);
			noLatencyTimedEventHandler?.IncreaseCurrentTime(framesPlayedLastRound);

			framesPlayedLastRound = numberOfFrames;
		}



		/********************************************************************/
		/// <summary>
		/// Will execute all events before the current time
		/// </summary>
		/********************************************************************/
		protected void DoTimedEvents()
		{
			timedEventHandler.DoEvents();
			noLatencyTimedEventHandler.DoEvents();
		}
		#endregion
	}
}
