/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	/// <summary>
	/// The user control showing the oscilloscope
	/// </summary>
	internal partial class OscilloscopeControl : UserControl
	{
		private const int PanelMargin = 8;

		private readonly OscilloscopeSettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OscilloscopeControl()
		{
			InitializeComponent();

			// Set tooltip
			toolTip.SetToolTip(this, Resources.IDS_OSCIL_TOOLTIP);
			toolTip.SetToolTip(leftSpeakerOscilloscopeControl, Resources.IDS_OSCIL_TOOLTIP);
			toolTip.SetToolTip(rightSpeakerOscilloscopeControl, Resources.IDS_OSCIL_TOOLTIP);

			// Initialize settings
			settings = new OscilloscopeSettings();

			SpeakerOscilloscopeControl.ScopeType scopeType = settings.ScopeType;
			leftSpeakerOscilloscopeControl.SetScopeType(scopeType);
			rightSpeakerOscilloscopeControl.SetScopeType(scopeType);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			lock (this)
			{
				leftSpeakerOscilloscopeControl.DrawSample(null, 0, 0);
				rightSpeakerOscilloscopeControl.DrawSample(null, 0, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will tell each channel to draw itself
		/// </summary>
		/********************************************************************/
		public void SampleData(NewSampleData sampleData)
		{
			lock (this)
			{
				if (sampleData.Stereo)
				{
					if (sampleData.SwapSpeakers)
					{
						leftSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 1, 2);
						rightSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 0, 2);
					}
					else
					{
						leftSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 0, 2);
						rightSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 1, 2);
					}
				}
				else
				{
					leftSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 0, 1);
					rightSpeakerOscilloscopeControl.DrawSample(sampleData.SampleData, 0, 1);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the scope type on both views
		/// </summary>
		/********************************************************************/
		public void SwitchScopeType()
		{
			leftSpeakerOscilloscopeControl.RotateScopeType();
			SpeakerOscilloscopeControl.ScopeType type = rightSpeakerOscilloscopeControl.RotateScopeType();

			// Write the new type in the settings
			settings.ScopeType = type;
			settings.Settings.SaveSettings();
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the control is resize to recalculate
		/// position and sizes
		/// </summary>
		/********************************************************************/
		private void Control_Resize(object sender, EventArgs e)
		{
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the visible status changes for the control. Needed
		/// to layout the controls when the scope is shown
		/// </summary>
		/********************************************************************/
		private void Control_VisibleChanged(object sender, EventArgs e)
		{
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks the mouse button
		/// </summary>
		/********************************************************************/
		private void Control_Click(object sender, EventArgs e)
		{
			SwitchScopeType();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Calculate the position and sizes for the speaker controls
		/// </summary>
		/********************************************************************/
		private void LayoutPanels()
		{
			Size clientArea = ParentForm.ClientSize;

			int width = (clientArea.Width - 3 * PanelMargin) / 2;
			int height = clientArea.Height - 2 * PanelMargin;

			leftPanel.Location = new Point(PanelMargin, PanelMargin);
			leftPanel.Size = new Size(width, height);

			rightPanel.Location = new Point(leftPanel.Location.X + width + PanelMargin, PanelMargin);
			rightPanel.Size = new Size(width, height);
		}
		#endregion
	}
}
