/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares.Display;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SpinningSquaresWorker : IChannelChangeVisualAgent, IAgentGuiDisplay
	{
		private SpinningSquaresControl userControl;

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new SpinningSquaresControl();
			return userControl;
		}
		#endregion

		#region IVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels)
		{
			userControl.InitVisual(channels);
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
		#endregion

		#region IChannelChangeVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged channelChanged)
		{
			userControl.ChannelChange(channelChanged);
		}
		#endregion
	}
}
