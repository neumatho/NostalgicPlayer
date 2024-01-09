/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Visual.Piano.Display;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.Piano
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class PianoWorker : IChannelChangeVisualAgent, IAgentGuiDisplay
	{
		private PianoControl userControl;

		#region IAgentDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling how to set up the display window
		/// </summary>
		/********************************************************************/
		public DisplayFlag Flags => DisplayFlag.StaticWindow;
		#endregion

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new PianoControl();
			return userControl;
		}



		/********************************************************************/
		/// <summary>
		/// Return the anchor name on the help page or null if none exists
		/// </summary>
		/********************************************************************/
		public string HelpAnchor => "piano";
		#endregion

		#region IVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels, int virtualChannels)
		{
			userControl.InitVisual(virtualChannels);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			userControl.CleanupVisual();
		}



		/********************************************************************/
		/// <summary>
		/// Set the pause state
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
			userControl.SetPauseState(paused);
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
			userControl.SetNoteFrequencies(noteFrequencies);
		}



		/********************************************************************/
		/// <summary>
		/// Tell the visual about changes of the channels
		/// </summary>
		/********************************************************************/
		public void ChannelsChanged(ChannelChanged[] channelChanged)
		{
			userControl.ChannelChange(channelChanged);
		}
		#endregion
	}
}
