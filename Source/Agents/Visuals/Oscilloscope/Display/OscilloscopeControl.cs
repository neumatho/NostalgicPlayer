/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.Agent.Visual.Oscilloscope.Display
{
	/// <summary>
	/// The user control showing the oscilloscope
	/// </summary>
	internal partial class OscilloscopeControl : UserControl
	{
		private const int PanelMargin = 8;

		private readonly OscilloscopeSettings settings;

		private SpeakerFlag speakers;
		private int numberOfRows;

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

			// Initialize settings
			settings = new OscilloscopeSettings();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(SpeakerFlag speakersToShow)
		{
			lock (this)
			{
				speakers = speakersToShow;
				numberOfRows = CalculateNeededRows(speakersToShow);

				DestroyBoxes();
				CreateBoxes();

				hashPanel.Visible = false;
				oscilloscopesPanel.Visible = true;
			}
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
				oscilloscopesPanel.Visible = false;
				hashPanel.Visible = true;

				DestroyBoxes();
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
				int controlIndex = 0;

				if (speakers.HasFlag(SpeakerFlag.FrontLeft))
				{
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.FrontLeft], sampleData.ChannelCount);

					if (speakers.HasFlag(SpeakerFlag.FrontCenter))
						((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.FrontCenter], sampleData.ChannelCount);

					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.FrontRight], sampleData.ChannelCount);
				}
				else if (speakers.HasFlag(SpeakerFlag.FrontCenter))
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.FrontCenter], sampleData.ChannelCount);

				if (speakers.HasFlag(SpeakerFlag.SideLeft))
				{
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.SideLeft], sampleData.ChannelCount);
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.SideRight], sampleData.ChannelCount);
				}

				if (speakers.HasFlag(SpeakerFlag.BackLeft))
				{
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.BackLeft], sampleData.ChannelCount);
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex++].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.BackRight], sampleData.ChannelCount);
				}

				if (speakers.HasFlag(SpeakerFlag.LowFrequency))
					((SpeakerOscilloscopeControl)oscilloscopesPanel.Controls[controlIndex].Controls[0]).DrawSample(sampleData.SampleData, sampleData.ChannelMapping[SpeakerFlag.LowFrequency], sampleData.ChannelCount);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will change the scope type on both views
		/// </summary>
		/********************************************************************/
		public void SwitchScopeType()
		{
			ScopeType newScopeType = RotateScopeType(settings.ScopeType);

			lock (this)
			{
				foreach (Control control in oscilloscopesPanel.Controls)
				{
					SpeakerOscilloscopeControl oscilloscopeControl = (SpeakerOscilloscopeControl)control.Controls[0];
					oscilloscopeControl.SetScopeType(newScopeType);
				}
			}

			// Write the new type in the settings
			settings.ScopeType = newScopeType;
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
			lock (this)
			{
				LayoutPanels();
			}
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
		/// Create all the boxes
		/// </summary>
		/********************************************************************/
		private int CalculateNeededRows(SpeakerFlag speakerFlag)
		{
			int rows = 0;

			if (speakerFlag.HasFlag(SpeakerFlag.FrontLeft) || speakerFlag.HasFlag(SpeakerFlag.FrontCenter))
				rows++;

			if (speakerFlag.HasFlag(SpeakerFlag.SideLeft))
				rows++;

			if (speakerFlag.HasFlag(SpeakerFlag.BackLeft))
				rows++;

			if (speakerFlag.HasFlag(SpeakerFlag.LowFrequency))
				rows++;

			return rows;
		}



		/********************************************************************/
		/// <summary>
		/// Create all the boxes
		/// </summary>
		/********************************************************************/
		private void CreateBoxes()
		{
			ScopeType scopeType = settings.ScopeType;

			if (speakers.HasFlag(SpeakerFlag.FrontLeft))
			{
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("FL", scopeType));

				if (speakers.HasFlag(SpeakerFlag.FrontCenter))
					oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("FC", scopeType));

				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("FR", scopeType));
			}
			else if (speakers.HasFlag(SpeakerFlag.FrontCenter))
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("FC", scopeType));

			if (speakers.HasFlag(SpeakerFlag.SideLeft))
			{
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("SL", scopeType));
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("SR", scopeType));
			}

			if (speakers.HasFlag(SpeakerFlag.BackLeft))
			{
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("BL", scopeType));
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("BR", scopeType));
			}

			if (speakers.HasFlag(SpeakerFlag.LowFrequency))
				oscilloscopesPanel.Controls.Add(CreateOscilloscopeControl("LFE", scopeType));

			// Now layout the panels
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Destroy all the boxes
		/// </summary>
		/********************************************************************/
		private void DestroyBoxes()
		{
			while (oscilloscopesPanel.Controls.Count > 0)
			{
				// Since dispose also removes the control from the
				// collection, we just remove the first item in every
				// iteration
				Control ctrl = oscilloscopesPanel.Controls[0];
				toolTip.SetToolTip(ctrl, null);

				ctrl.Dispose();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create a single oscilloscope control
		/// </summary>
		/********************************************************************/
		private Control CreateOscilloscopeControl(string speakerName, ScopeType scopeType)
		{
			Panel panel = new Panel
			{
				BorderStyle = BorderStyle.Fixed3D
			};

			SpeakerOscilloscopeControl oscilloscopeControl = new SpeakerOscilloscopeControl(speakerName)
			{
				Dock = DockStyle.Fill,
				BackColor = Color.Black,
				ForeColor = Color.Green
			};

			panel.Controls.Add(oscilloscopeControl);

			toolTip.SetToolTip(oscilloscopeControl, Resources.IDS_OSCIL_TOOLTIP);
			oscilloscopeControl.SetScopeType(scopeType);

			return panel;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the position and sizes for the speaker controls
		/// </summary>
		/********************************************************************/
		private void LayoutPanels()
		{
			if (numberOfRows > 0)
			{
				Size clientArea = oscilloscopesPanel.ClientSize;

				float height = (clientArea.Height - (numberOfRows * PanelMargin - PanelMargin)) / (float)numberOfRows;
				if (height < 128.0)
					height = 128.0f;

				float width1 = clientArea.Width;
				float width2 = (clientArea.Width - PanelMargin) / 2.0f;
				float width3 = (clientArea.Width - 2 * PanelMargin) / 3.0f;

				float yPos = 0.0f;
				int controlIndex = 0;

				if (speakers.HasFlag(SpeakerFlag.FrontLeft))
				{
					if (speakers.HasFlag(SpeakerFlag.FrontCenter))
					{
						oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
						oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width3, (int)height);
						controlIndex++;

						oscilloscopesPanel.Controls[controlIndex].Location = new Point((int)width3 + PanelMargin, (int)yPos);
						oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width3, (int)height);
						controlIndex++;

						oscilloscopesPanel.Controls[controlIndex].Location = new Point((int)(clientArea.Width - width3), (int)yPos);
						oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width3, (int)height);
						controlIndex++;
					}
					else
					{
						oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
						oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
						controlIndex++;

						oscilloscopesPanel.Controls[controlIndex].Location = new Point((int)(clientArea.Width - width2), (int)yPos);
						oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
						controlIndex++;
					}

					yPos += height + PanelMargin;
				}
				else if (speakers.HasFlag(SpeakerFlag.FrontCenter))
				{
					oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width1, (int)height);
					controlIndex++;
				}

				if (speakers.HasFlag(SpeakerFlag.SideLeft))
				{
					oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
					controlIndex++;

					oscilloscopesPanel.Controls[controlIndex].Location = new Point((int)(clientArea.Width - width2), (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
					controlIndex++;

					yPos += height + PanelMargin;
				}

				if (speakers.HasFlag(SpeakerFlag.BackLeft))
				{
					oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
					controlIndex++;

					oscilloscopesPanel.Controls[controlIndex].Location = new Point((int)(clientArea.Width - width2), (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width2, (int)height);
					controlIndex++;

					yPos += height + PanelMargin;
				}

				if (speakers.HasFlag(SpeakerFlag.LowFrequency))
				{
					oscilloscopesPanel.Controls[controlIndex].Location = new Point(0, (int)yPos);
					oscilloscopesPanel.Controls[controlIndex].Size = new Size((int)width1, (int)height);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will rotate the scope type
		/// </summary>
		/********************************************************************/
		private ScopeType RotateScopeType(ScopeType scopeType)
		{
			switch (scopeType)
			{
				case ScopeType.Filled:
				{
					scopeType = ScopeType.Lines;
					break;
				}

				case ScopeType.Lines:
				{
					scopeType = ScopeType.Dots;
					break;
				}

				default:
				{
					scopeType = ScopeType.Filled;
					break;
				}
			}

			return scopeType;
		}
		#endregion
	}
}
