﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class OscilloscopeWorker : ISampleDataVisualAgent, IAgentGuiDisplay
	{
		private OscilloscopeControl userControl;

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new OscilloscopeControl();
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

		#region ISampleDataVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Tell the visual about new sample data
		/// </summary>
		/********************************************************************/
		public void SampleData(NewSampleData sampleData)
		{
			userControl.SampleData(sampleData);
		}
		#endregion
	}
}