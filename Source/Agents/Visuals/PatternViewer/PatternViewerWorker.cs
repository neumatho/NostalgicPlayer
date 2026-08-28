//---------------------------------------------------------------------------------------
// <copyright file="PatternViewerWorker.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class PatternViewerWorker : IPatternVisualAgent, IChannelChangeVisualAgent, IMixerChannelChangeVisualAgent,
		IAgentGuiDisplay
	{
		private readonly ISettingsFactory settingsFactory;

		private PatternControl userControl;

		public PatternViewerWorker(ISettingsFactory settingsFactory)
		{
			this.settingsFactory = settingsFactory;
		}

		#region IAgentDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling how to set up the display window
		/// </summary>
		/********************************************************************/
		public DisplayFlag Flags => DisplayFlag.None;
		#endregion

		#region IMixerChannelChangeVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Tell the visual about mixer channel enabled state changes
		/// </summary>
		/********************************************************************/
		public void MixerChannelsEnabledChanged(bool[] channelsEnabled)
		{
			if (userControl != null)
			{
				userControl.UpdateMixerChannelStatus(channelsEnabled);
			}
		}
		#endregion

		#region IPatternVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Called when playback moves to a new row
		/// </summary>
		/********************************************************************/
		public void PatternRowChanged(SongRowChangeInfo rowInfo)
		{
			if (userControl != null)
			{
				userControl.PatternRowChanged(rowInfo);
			}
		}
		#endregion

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new PatternControl(settingsFactory);

			return userControl;
		}

		/********************************************************************/
		/// <summary>
		/// Return the anchor name on the help page or null if none exists
		/// </summary>
		/********************************************************************/
		public string HelpAnchor => "patternviewer";
		#endregion

		#region IVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Called when a new module is about to be loaded
		/// </summary>
		/********************************************************************/
		public void SongModuleLoaded(SongModule data)
		{
			if (userControl != null)
			{
				userControl.SongModuleLoaded(data);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels, int virtualChannels, SpeakerFlag speakersToShow)
		{
			if (userControl != null)
			{
				userControl.InitVisualization(channels);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			if (userControl != null)
			{
				userControl.CleanupVisualization();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Set the pause state
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
			if (userControl != null)
			{
				userControl.SetPauseState(paused);
			}
		}
		#endregion

		#region IChannelChangeVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Is called when initializing the visual agent. The array
		/// contains
		/// all the frequencies for each note per sample
		/// </summary>
		/********************************************************************/
		public void SetNoteFrequencies(uint[][] noteFrequencies)
		{
			// Not used for pattern viewer
		}

		/********************************************************************/
		/// <summary>
		/// Tell the visual about changes of the channels
		/// </summary>
		/********************************************************************/
		public void ChannelsChanged(ChannelChanged[] channelChanged)
		{
			if (userControl != null)
			{
				userControl.ChannelChange(channelChanged);
			}
		}
		#endregion
	}
}
