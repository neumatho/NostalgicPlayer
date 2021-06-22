/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.AboutWindow
{
	/// <summary>
	/// This shows the about information
	/// </summary>
	public partial class AboutWindowForm : WindowFormBase
	{
		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = true)]
		private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

		private const int AreaWidth = 340;
		private const int AreaHeight = 170;

		private enum Mode
		{
			Text,
			Logo,
			Agents
		}

		private Bitmap bitmap;
		private readonly int outsideHeight;

		private Bitmap logoBitmap;

		private readonly string[] textToShow;
		private int textIndex;

		private Mode showMode;

		private int waitCount = 0;
		private int counter;
		private int linesToScroll;

		private int logoLine;
		private Color color;

		private string[] supportedFormats;
		private string[] outputAgents;
		private string[] sampleConverters;
		private string[] moduleConverters;
		private string[] visualAgents;
		private string[] decruncherAgents;

		private string[] agentsToShow;
		private int agentIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AboutWindowForm(Manager agentManager, MainWindowForm mainWindow, OptionSettings optionSettings)
		{
			InitializeComponent();

			if (!DesignMode)
			{
				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("AboutWindow");

				// Set the size of the window to a fixed size
				ClientSize = new Size(AreaWidth + pictureBox.Location.X * 2, AreaHeight + pictureBox.Location.Y * 2);

				// Set the title of the window
				Text = Resources.IDS_ABOUT_TITLE;

				// Prepare the text to show
				textToShow = Resources.IDS_ABOUT_TEXT.Split('\n');
				textIndex = 0;

				// Create the bitmap to draw in
				outsideHeight = Font.Height;

				bitmap = new Bitmap(AreaWidth, AreaHeight + outsideHeight);
				pictureBox.Image = bitmap;

				// Fill the bitmap with a white background
				using (Graphics g = Graphics.FromImage(bitmap))
				{
					using (SolidBrush brush = new SolidBrush(Color.White))
					{
						g.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
					}
				}

				// Make a copy of the logo
				logoBitmap = new Bitmap(Resources.IDB_ABOUT_LOGO);

				// Initialize variables
				showMode = Mode.Text;
				color = Color.Black;

				counter = outsideHeight - 1;
				linesToScroll = outsideHeight;

				// Force the logo and some text onto the window immediately
				for (int i = 0; i < bitmap.Height; i++)
					Pulse();

				// Set the wait count
				waitCount = 20;

				// Find all the different types of agents
				FindAgents(agentManager);

				// Start the timer
				pulseTimer.Start();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the form is closed
		/// </summary>
		/********************************************************************/
		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);

			if (bitmap != null)
			{
				// Do some cleanup
				bitmap.Dispose();
				bitmap = null;

				logoBitmap.Dispose();
				logoBitmap = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called by the timer
		/// </summary>
		/********************************************************************/
		private void Pulse_Tick(object sender, EventArgs e)
		{
			Pulse();

			// Force the control to redraw itself
			pictureBox.Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Find all the different agents installed and build the needed lists
		/// </summary>
		/********************************************************************/
		private void FindAgents(Manager agentManager)
		{
			supportedFormats = FindAgentTypes(agentManager, Manager.AgentType.Players, Manager.AgentType.ModuleConverters);
			outputAgents = FindAgentTypes(agentManager, Manager.AgentType.Output);
			sampleConverters = FindAgents(agentManager, Manager.AgentType.SampleConverters);
			moduleConverters = FindAgents(agentManager, Manager.AgentType.ModuleConverters);
			visualAgents = FindAgentTypes(agentManager, Manager.AgentType.Visuals);
			decruncherAgents = FindAgentTypes(agentManager, Manager.AgentType.FileDecrunchers);
		}



		/********************************************************************/
		/// <summary>
		/// Create a list with all the types available for the given agent
		/// type
		/// </summary>
		/********************************************************************/
		private string[] FindAgentTypes(Manager agentManager, params Manager.AgentType[] agentTypes)
		{
			List<string> names = new List<string>();

			foreach (Manager.AgentType type in agentTypes)
			{
				// Find all supported formats from the players
				foreach (AgentInfo agentInfo in agentManager.GetAllAgents(type).Where(agentInfo => !string.IsNullOrEmpty(agentInfo.TypeName)))
				{
					try
					{
						if (agentInfo.TypeName == agentInfo.AgentName)
							names.Add(agentInfo.TypeName);
						else
							names.Add($"{agentInfo.TypeName} ({agentInfo.AgentName})");
					}
					catch (Exception)
					{
						// Ignore exception
					}
				}
			}

			return names.OrderBy(n => n).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Create a list with all the agents available for the given agent
		/// type
		/// </summary>
		/********************************************************************/
		private string[] FindAgents(Manager agentManager, params Manager.AgentType[] agentTypes)
		{
			HashSet<string> names = new HashSet<string>();

			foreach (Manager.AgentType type in agentTypes)
			{
				// Find all supported formats from the players
				foreach (AgentInfo agentInfo in agentManager.GetAllAgents(type))
				{
					try
					{
						names.Add(agentInfo.AgentName);
					}
					catch (Exception)
					{
						// Ignore exception
					}
				}
			}

			return names.OrderBy(n => n).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Scroll the text into the view
		/// </summary>
		/********************************************************************/
		private void Pulse()
		{
			if (waitCount != 0)
			{
				waitCount--;
				return;
			}

			// Lock the bitmap so we can modify it
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

			try
			{
				// Get the address of the first line
				IntPtr ptr = bitData.Scan0;

				// Scroll the bitmap one line up
				CopyMemory(ptr, ptr + bitData.Stride, (uint)((bitData.Height - 1) * bitData.Stride));
			}
			finally
			{
				bitmap.UnlockBits(bitData);
			}

			counter++;
			if (counter == linesToScroll)
			{
				counter = 0;

				using (Graphics g = Graphics.FromImage(bitmap))
				{
					// If anything, but the logo, then clear the outside area
					if (showMode != Mode.Logo)
					{
						using (SolidBrush brush = new SolidBrush(Color.White))
						{
							g.FillRectangle(brush, 0, AreaHeight, AreaWidth, outsideHeight);
						}
					}

					// Calculate the x, y, width and height
					int x = 5;
					int y = AreaHeight;
					int w = AreaWidth - x - 5;
					int h = outsideHeight;

					// Remember the current state
					Mode currentMode;

					do
					{
						currentMode = showMode;

						switch (showMode)
						{
							// Show the logo
							case Mode.Logo:
							{
								rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
								bitData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);

								try
								{
									rect = new Rectangle(0, 0, logoBitmap.Width, logoBitmap.Height);
									BitmapData logoData = logoBitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

									try
									{
										// Get the address of the first line
										IntPtr ptr = bitData.Scan0;
										IntPtr logoPtr = logoData.Scan0;

										// Copy one line into the bitmap
										CopyMemory(ptr + ((AreaHeight - 1) * bitData.Stride), logoPtr + (logoLine * logoData.Stride), (uint)bitData.Stride);
									}
									finally
									{
										logoBitmap.UnlockBits(logoData);
									}
								}
								finally
								{
									bitmap.UnlockBits(bitData);
								}

								logoLine++;
								if (logoLine == logoBitmap.Height)
								{
									showMode = Mode.Text;
									linesToScroll = outsideHeight;
								}
								break;
							}

							// Show agents
							case Mode.Agents:
							{
								string str = string.Empty;

								if (agentsToShow.Length == 0)
								{
									// No agents installed
									str = Resources.IDS_NO_AGENTS;
								}
								else
								{
									if (agentIndex == agentsToShow.Length)
									{
										// No more agents to show
										showMode = Mode.Text;
									}
									else
									{
										// Take the next agent
										str = agentsToShow[agentIndex++];
									}
								}

								if (showMode == Mode.Agents)
								{
									// Center the name
									x += (w - (int)g.MeasureString(str, Font).Width) / 2;

									using (SolidBrush brush = new SolidBrush(color))
									{
										g.DrawString(str, Font, brush, x, y);
									}
								}
								break;
							}

							// Take the next line of text
							case Mode.Text:
							{
								string str = textToShow[textIndex++];

								if (textIndex == textToShow.Length)
									textIndex = 0;

								// Only parse and write the string if not empty
								if (!string.IsNullOrEmpty(str))
								{
									bool moreCommands = true;

									do
									{
										if (str[0] == '¤')
										{
											switch (str[1])
											{
												// Show logo
												case 'L':
												{
													str = str.Substring(2);

													linesToScroll = 1;
													logoLine = 0;
													showMode = Mode.Logo;
													moreCommands = false;
													break;
												}

												// Center the line
												case 'C':
												{
													str = str.Substring(2);

													x += (w - (int)g.MeasureString(str, Font).Width) / 2;
													break;
												}

												// Change color to black
												case 'B':
												{
													str = str.Substring(2);

													color = Color.Black;
													break;
												}

												// Change color to red
												case 'R':
												{
													str = str.Substring(2);

													color = Color.Red;
													break;
												}

												// Change color to blue
												case 'U':
												{
													str = str.Substring(2);

													color = Color.Blue;
													break;
												}

												// Add number of supported module formats
												case '1':
												{
													str = str.Substring(2).Replace("¤¤", supportedFormats.Length.ToString());
													break;
												}

												// Append NostalgicPlayer version number
												case 'V':
												{
													str = str.Substring(2) + " " + Env.CurrentVersion;
													break;
												}

												// Show module formats
												case 'F':
												{
													str = str.Substring(2);

													agentsToShow = supportedFormats;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												// Show output agents
												case 'O':
												{
													str = str.Substring(2);

													agentsToShow = outputAgents;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												// Show sample converters
												case 'S':
												{
													str = str.Substring(2);

													agentsToShow = sampleConverters;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												// Show module converters
												case 'M':
												{
													str = str.Substring(2);

													agentsToShow = moduleConverters;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												// Show visuals
												case 'I':
												{
													str = str.Substring(2);

													agentsToShow = visualAgents;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												// Show decrunchers
												case 'D':
												{
													str = str.Substring(2);

													agentsToShow = decruncherAgents;
													agentIndex = 0;

													showMode = Mode.Agents;
													moreCommands = false;
													break;
												}

												default:
												{
													moreCommands = false;
													break;
												}
											}
										}
										else
											moreCommands = false;
									}
									while (moreCommands && !string.IsNullOrEmpty(str));

									// Write the string
									if (!string.IsNullOrEmpty(str))
									{
										using (SolidBrush brush = new SolidBrush(color))
										{
											g.DrawString(str, Font, brush, x, y);
										}
									}
								}
								break;
							}
						}
					}
					while (currentMode != showMode);
				}
			}
		}
	}
}
