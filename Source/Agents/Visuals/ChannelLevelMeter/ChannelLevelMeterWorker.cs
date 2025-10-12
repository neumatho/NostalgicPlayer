/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter.Display;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.ChannelLevelMeter
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class ChannelLevelMeterWorker : IChannelChangeVisualAgent, IAgentGuiDisplay
	{
		private ChannelLevelMeterControl userControl;

		#region IAgentDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling how to set up the display window
		/// </summary>
		/********************************************************************/
		public DisplayFlag Flags => DisplayFlag.None;
		#endregion

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new ChannelLevelMeterControl();
			return userControl;
		}



		/********************************************************************/
		/// <summary>
		/// Return the anchor name on the help page or null if none exists
		/// </summary>
		/********************************************************************/
		public string HelpAnchor => "channellevelmeter";
		#endregion

		#region IVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels, int virtualChannels, SpeakerFlag speakersToShow)
		{
			if (userControl != null)
			{
				userControl.InitVisual(virtualChannels);
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
				userControl.CleanupVisual();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Set the pause state
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
		}
		#endregion

		#region IChannelChangeVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Is called when initializing the visual agent. The array contains
		/// all the frequencies for each note per sample
		/// </summary>
		/********************************************************************/
		public void SetNoteFrequencies(uint[][] noteFrequencies)
		{
			// Not used for channel level meter
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
