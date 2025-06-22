/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Krypton.Toolkit;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Bases;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings;
using Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Modules;
using Polycode.NostalgicPlayer.GuiKit.Components;
using Polycode.NostalgicPlayer.GuiKit.Extensions;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow
{
	/// <summary>
	/// This shows the module information
	/// </summary>
	public partial class ModuleInfoWindowForm : WindowFormBase
	{
		private ModuleHandler moduleHandler;
		private MainWindowForm mainWindow;

		private PictureInfo[] pictures;
		private int pictureIndex;
		private int nextPictureIndex;

		private Lock animationLock;
		private bool animationRunning;
		private bool pictureFading;
		private double easePosition;
		private float currentOpacity;
		private float newOpacity;

		private int currentXPos;
		private int newXPos;

		private const float FadeIncrement = 0.025f;
		private const double EaseIncrement = 0.025;

		private Bitmap currentPictureBitmap;
		private Bitmap newPictureBitmap;
		private Bitmap fadePictureBitmap;

		private Bitmap currentLabelBitmap;
		private Bitmap currentFadeLabelBitmap;
		private Bitmap newLabelBitmap;
		private Bitmap newFadeLabelBitmap;

		private readonly ModuleInfoWindowSettings settings;
		private readonly ModuleSettings moduleSettings;

		private int firstCustomLine;

		private static readonly float[][] fadeMatrix =
		{
			new float[] { 1, 0, 0, 0, 0 },
			new float[] { 0, 1, 0, 0, 0 },
			new float[] { 0, 0, 1, 0, 0 },
			new float[] { 0, 0, 0, 1, 0 },
			new float[] { 0, 0, 0, 0, 1 }
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoWindowForm(ModuleHandler moduleHandler, MainWindowForm mainWindow, OptionSettings optionSettings, ModuleSettings moduleSettings)
		{
			InitializeComponent();

			// Remember the arguments
			this.moduleHandler = moduleHandler;
			this.mainWindow = mainWindow;
			this.moduleSettings = moduleSettings;

			if (!DesignMode)
			{
				animationLock = new Lock();

				InitializeWindow(mainWindow, optionSettings);

				// Load window settings
				LoadWindowSettings("ModuleInfoWindow");
				settings = new ModuleInfoWindowSettings(allWindowSettings);

				// Set the title of the window
				Text = Resources.IDS_MODULE_INFO_TITLE;

				// Set the tab titles
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Info].Text = Resources.IDS_MODULE_INFO_TAB_INFO;
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Text = Resources.IDS_MODULE_INFO_TAB_COMMENT;
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Text = Resources.IDS_MODULE_INFO_TAB_LYRICS;
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Text = Resources.IDS_MODULE_INFO_TAB_PICTURES;

				// Add the columns to the controls
				moduleInfoInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
				{
					Name = Resources.IDS_MODULE_INFO_COLUMN_DESCRIPTION,
					Resizable = DataGridViewTriState.True,
					SortMode = DataGridViewColumnSortMode.NotSortable,
					Width = settings.Column1Width
				});

				moduleInfoInfoDataGridView.Columns.Add(new KryptonDataGridViewTextBoxColumn
				{
					Name = Resources.IDS_MODULE_INFO_COLUMN_VALUE,
					Resizable = DataGridViewTriState.True,
					SortMode = DataGridViewColumnSortMode.NotSortable,
					Width = settings.Column2Width
				});

				// Make sure that the content is up-to date
				RefreshWindow(false);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will clear the window and add all the items again
		/// </summary>
		/********************************************************************/
		public void RefreshWindow(bool onLoad)
		{
			if (moduleHandler != null)
			{
				// Remove all the items
				moduleInfoInfoDataGridView.Rows.Clear();
				moduleInfoCommentReadOnlyTextBox.Lines = null;
				moduleInfoLyricsReadOnlyTextBox.Lines = null;

				CleanupPictures();

				// Add the items
				AddItems(onLoad);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will be called every time a new value has changed
		/// </summary>
		/********************************************************************/
		public void UpdateWindow(int line, string newValue)
		{
			if (moduleHandler != null)
			{
				// Check to see if there are any module loaded at the moment
				if (moduleHandler.IsModuleLoaded)
				{
					if ((firstCustomLine + line) < moduleInfoInfoDataGridView.RowCount)
					{
						moduleInfoInfoDataGridView.Rows[firstCustomLine + line].Cells[1].Value = newValue;
						moduleInfoInfoDataGridView.InvalidateRow(firstCustomLine + line);
					}
				}
			}
		}

		#region WindowFormBase overrides
		/********************************************************************/
		/// <summary>
		/// Return the URL to the help page
		/// </summary>
		/********************************************************************/
		protected override string HelpUrl => "modinfo.html";
		#endregion

		#region Keyboard shortcuts
		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the main window
		/// </summary>
		/********************************************************************/
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Check for different keyboard shortcuts
			Keys key = keyData & Keys.KeyCode;

			switch (key)
			{
				case Keys.Left:
				{
					if (navigator.SelectedIndex == (int)ModuleSettings.ModuleInfoTab.Pictures)
						ShowPreviousPicture();

					return true;
				}

				case Keys.Right:
				{
					if (navigator.SelectedIndex == (int)ModuleSettings.ModuleInfoTab.Pictures)
						ShowNextPicture();

					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the window is closed
		/// </summary>
		/********************************************************************/
		private void ModuleInfoWindowForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (mainWindow != null)     // Main window is null, if the window has already been closed (because Owner has been set)
			{
				// Save the settings
				settings.Column1Width = moduleInfoInfoDataGridView.Columns[0].Width;
				settings.Column2Width = moduleInfoInfoDataGridView.Columns[1].Width;

				// Cleanup
				CleanupPictures();

				mainWindow = null;
				moduleHandler = null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks in a cell in the data grid
		/// </summary>
		/********************************************************************/
		private void ModuleInfoInfoDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (!Env.IsWindows10S)
			{
				// Check if the file name has been clicked
				if ((e.RowIndex == 7) && (e.ColumnIndex == 1))
				{
					string fileName = moduleInfoInfoDataGridView[e.ColumnIndex, e.RowIndex].Value.ToString();
					if (ArchivePath.IsArchivePath(fileName))
						fileName = ArchivePath.GetArchiveName(fileName);

					// Start File Explorer and select the file
					Process.Start("explorer.exe", $"/select,\"{fileName}\"");
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the previous picture button
		/// </summary>
		/********************************************************************/
		private void PreviousPictureButton_Click(object sender, EventArgs e)
		{
			ShowPreviousPicture();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the user clicks on the next picture button
		/// </summary>
		/********************************************************************/
		private void NextPictureButton_Click(object sender, EventArgs e)
		{
			ShowNextPicture();
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the picture group resizes
		/// </summary>
		/********************************************************************/
		private void PictureGroup_Resize(object sender, EventArgs e)
		{
			lock (animationLock)
			{
				// Replace the next/previous buttons
				int newYPos = ((pictureBox.Height - previousPictureButton.Height) / 2) + pictureBox.Location.Y;
				previousPictureButton.Location = new Point(8, newYPos);
				nextPictureButton.Location = new Point(pictureGroup.Width - nextPictureButton.Width - 8, newYPos);

				// Resize picture and label
				if (pictures != null)
				{
					if (currentPictureBitmap != null)
						CreateAllPictureBitmaps(pictureIndex, ref currentPictureBitmap, ref currentLabelBitmap);

					if (newPictureBitmap != null)
						CreateAllPictureBitmaps(nextPictureIndex, ref newPictureBitmap, ref newLabelBitmap);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Paint the picture
		/// </summary>
		/********************************************************************/
		private void Picture_Paint(object sender, PaintEventArgs e)
		{
			lock (animationLock)
			{
				if (currentPictureBitmap != null)
					e.Graphics.DrawImage(currentPictureBitmap, currentXPos, 0);

				Bitmap bitmapToDraw = fadePictureBitmap ?? newPictureBitmap;
				if (bitmapToDraw != null)
					e.Graphics.DrawImage(bitmapToDraw, newXPos, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Paint the picture label
		/// </summary>
		/********************************************************************/
		private void PictureLabel_Paint(object sender, PaintEventArgs e)
		{
			lock (animationLock)
			{
				Bitmap bitmapToDraw = currentFadeLabelBitmap ?? currentLabelBitmap;
				if (bitmapToDraw != null)
					e.Graphics.DrawImage(bitmapToDraw, 0, 0);

				if (newFadeLabelBitmap != null)
					e.Graphics.DrawImage(newFadeLabelBitmap, 0, 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called for every frame when the animation is running
		/// </summary>
		/********************************************************************/
		private void AnimationTimer_Tick(object sender, EventArgs e)
		{
			lock (animationLock)
			{
				animationRunning = true;

				currentOpacity -= FadeIncrement;
				newOpacity += FadeIncrement;

				if (newOpacity >= 1.0f)
				{
					currentOpacity = 0.0f;
					newOpacity = 1.0f;
				}

				if (!pictureFading)
				{
					// EaseOutCubic
					double pos = 1.0 - Math.Pow(1 - easePosition, 3.0);
					int posDiff = (int)(pos * pictureGroup.Width);

					if (pictureIndex <= nextPictureIndex)
					{
						currentXPos = -posDiff;
						newXPos = pictureGroup.Width - posDiff;
					}
					else
					{
						currentXPos = posDiff;
						newXPos = -(pictureGroup.Width - posDiff);
					}

					easePosition += EaseIncrement;

					if (easePosition > 1.0)
						easePosition = 1.0;
				}
				else
					easePosition = 1.0;

				if ((newOpacity == 1.0f) && (easePosition == 1.0))
				{
					animationTimer.Stop();
					animationRunning = false;
					pictureFading = false;

					currentFadeLabelBitmap?.Dispose();
					currentFadeLabelBitmap = null;

					newFadeLabelBitmap?.Dispose();
					newFadeLabelBitmap = null;

					currentLabelBitmap?.Dispose();
					currentLabelBitmap = newLabelBitmap;
					newLabelBitmap = null;

					fadePictureBitmap?.Dispose();
					fadePictureBitmap = null;

					currentPictureBitmap?.Dispose();
					currentPictureBitmap = newPictureBitmap;
					newPictureBitmap = null;

					currentXPos = 0;

					pictureIndex = nextPictureIndex;

					ShowHideArrows();
				}
				else
				{
					currentFadeLabelBitmap?.Dispose();
					newFadeLabelBitmap?.Dispose();

					if (currentLabelBitmap != null)
						currentFadeLabelBitmap = SetOpacity(currentLabelBitmap, currentOpacity);

					newFadeLabelBitmap = SetOpacity(newLabelBitmap, newOpacity);

					if (pictureFading)
					{
						newXPos = 0;

						fadePictureBitmap?.Dispose();
						fadePictureBitmap = SetOpacity(newPictureBitmap, newOpacity);
					}
				}

				pictureLabelPictureBox.Invalidate();
				pictureBox.Invalidate();
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will add all the items in the list
		/// </summary>
		/********************************************************************/
		private void AddItems(bool onLoad)
		{
			// Check to see if there are any module loaded at the moment
			MultiFileInfo fileInfo;

			if (moduleHandler.IsPlaying && ((fileInfo = mainWindow.GetFileInfo()) != null))
			{
				firstCustomLine = 8;

				// Module in memory, add items
				ModuleInfoStatic staticInfo = moduleHandler.StaticModuleInformation;
				ModuleInfoFloating floatingInfo = moduleHandler.PlayingModuleInformation;

				string val = staticInfo.Title;
				if (string.IsNullOrEmpty(val))
					val = fileInfo.DisplayName;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TITLE, val);

				val = staticInfo.Author;
				if (string.IsNullOrEmpty(val))
					val = Resources.IDS_MODULE_INFO_UNKNOWN;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, val);

				int row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, staticInfo.Format);
				moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewTextBoxCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, ToolTipText = string.Join("\r\n", staticInfo.FormatDescription.SplitIntoLines(moduleInfoInfoDataGridView.Handle, 400, moduleInfoInfoDataGridView.Font)) };

				row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, staticInfo.PlayerName);
				moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewTextBoxCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, ToolTipText = string.Join("\r\n", staticInfo.PlayerDescription.SplitIntoLines(moduleInfoInfoDataGridView.Handle, 400, moduleInfoInfoDataGridView.Font)) };

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, staticInfo.Channels);

				if (floatingInfo.DurationInfo == null)
					val = Resources.IDS_MODULE_INFO_UNKNOWN;
				else
				{
					if ((int)floatingInfo.DurationInfo.TotalTime.TotalHours > 0)
						val = floatingInfo.DurationInfo.TotalTime.ToString(Resources.IDS_TIMEFORMAT);
					else
						val = floatingInfo.DurationInfo.TotalTime.ToString(Resources.IDS_TIMEFORMAT_SMALL);
				}

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, val);

				if (fileInfo.Type == MultiFileInfo.FileType.Url)
				{
					moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_URL, fileInfo.Source);
				}
				else
				{
					val = string.Format(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE_VALUE, staticInfo.ModuleSize);
					moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, val);

					if (staticInfo.DecruncherAlgorithms != null)
					{
						val = staticInfo.CrunchedSize == -1 ? Resources.IDS_MODULE_INFO_UNKNOWN : string.Format(Resources.IDS_MODULE_INFO_ITEM_PACKEDSIZE_VALUE, staticInfo.CrunchedSize);
						val += string.Format(" / {0}", string.Join(" \u2b95 ", staticInfo.DecruncherAlgorithms));

						moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_PACKEDSIZE, val);
						firstCustomLine++;
					}

					if (Env.IsWindows10S)
						moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.Source);
					else
					{
						row = moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, fileInfo.Source);
						moduleInfoInfoDataGridView.Rows[row].Cells[1] = new KryptonDataGridViewLinkCell { Value = moduleInfoInfoDataGridView.Rows[row].Cells[1].Value, TrackVisitedState = false };
					}
					firstCustomLine++;
				}

				// Add player specific items
				if (floatingInfo.ModuleInformation != null)
				{
					// Add an empty line
					DataGridViewRow emptyRow = new DataGridViewRow();
					emptyRow.Cells.AddRange(new DataGridViewCell[]
					{
						new KryptonDataGridViewTextBoxCell { Value = string.Empty, ToolTipText = string.Empty },
						new KryptonDataGridViewTextBoxCell { Value = string.Empty, ToolTipText = string.Empty }
					});

					moduleInfoInfoDataGridView.Rows.Add(emptyRow);

					foreach (string info in floatingInfo.ModuleInformation)
					{
						string[] splittedInfo = info.Split('\t');
						moduleInfoInfoDataGridView.Rows.Add(splittedInfo[0], splittedInfo[1]);
					}
				}

				// Add comment
				if (staticInfo.Comment?.Length > 0)
				{
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Visible = true;

					// Switch font
					moduleInfoCommentReadOnlyTextBox.Font = staticInfo.CommentFont ?? FontPalette.GetMonospaceFont();

					// Set text
					moduleInfoCommentReadOnlyTextBox.Lines = staticInfo.Comment;
				}
				else
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Visible = false;

				// Add lyrics
				if (staticInfo.Lyrics?.Length > 0)
				{
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Visible = true;

					// Switch font
					moduleInfoLyricsReadOnlyTextBox.Font = staticInfo.LyricsFont ?? FontPalette.GetMonospaceFont();

					// Set text
					moduleInfoLyricsReadOnlyTextBox.Lines = staticInfo.Lyrics;
				}
				else
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Visible = false;

				// Add pictures
				if (staticInfo.Pictures?.Length > 0)
				{
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Visible = true;

					// Remember the pictures
					pictures = staticInfo.Pictures;

					// Prepare needed bitmaps
					InitializePictures();
				}
				else
					navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Visible = false;

				// Find out which tab to activate
				if (onLoad)
				{
					foreach (ModuleSettings.ModuleInfoTab tab in moduleSettings.ModuleInfoActivateTabOrder)
					{
						if (navigator.Pages[(int)tab].Visible)
						{
							navigator.SelectedIndex = (int)tab;
							break;
						}
					}
				}
			}
			else
			{
				// No module in memory
				string na = Resources.IDS_MODULE_INFO_ITEM_NA;

				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TITLE, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_AUTHOR, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULEFORMAT, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_ACTIVEPLAYER, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_CHANNELS, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_TIME, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_MODULESIZE, na);
				moduleInfoInfoDataGridView.Rows.Add(Resources.IDS_MODULE_INFO_ITEM_FILE, na);

				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Comments].Visible = false;
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Lyrics].Visible = false;
				navigator.Pages[(int)ModuleSettings.ModuleInfoTab.Pictures].Visible = false;

				CleanupPictures();
			}

			// Resize the rows, so the lines are compacted
			moduleInfoInfoDataGridView.AutoResizeRows();
		}



		/********************************************************************/
		/// <summary>
		/// Will prepare the pictures for the first appearance
		/// </summary>
		/********************************************************************/
		private void InitializePictures()
		{
			lock (animationLock)
			{
				pictureIndex = 0;
				nextPictureIndex = 0;

				previousPictureButton.Visible = false;
				nextPictureButton.Visible = false;

				CreateAllPictureBitmaps(0, ref newPictureBitmap, ref newLabelBitmap);

				pictureFading = true;
				StartAnimation();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will delete any used resources for the pictures
		/// </summary>
		/********************************************************************/
		private void CleanupPictures()
		{
			lock (animationLock)
			{
				animationTimer.Stop();
				animationRunning = false;

				currentLabelBitmap?.Dispose();
				currentLabelBitmap = null;

				currentFadeLabelBitmap?.Dispose();
				currentFadeLabelBitmap = null;

				newLabelBitmap?.Dispose();
				newLabelBitmap = null;

				newFadeLabelBitmap?.Dispose();
				newFadeLabelBitmap = null;

				currentPictureBitmap?.Dispose();
				currentPictureBitmap = null;

				newPictureBitmap?.Dispose();
				newPictureBitmap = null;

				fadePictureBitmap?.Dispose();
				fadePictureBitmap = null;

				previousPictureButton.Visible = false;
				nextPictureButton.Visible = false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show or hide the navigation arrows depending on the picture
		/// showing
		/// </summary>
		/********************************************************************/
		private void ShowHideArrows()
		{
			previousPictureButton.Visible = pictureIndex > 0;
			nextPictureButton.Visible = pictureIndex < pictures.Length - 1;
		}



		/********************************************************************/
		/// <summary>
		/// Will show the previous picture in line
		/// </summary>
		/********************************************************************/
		private void ShowPreviousPicture()
		{
			lock (animationLock)
			{
				if (!animationRunning && (pictureIndex > 0))
				{
					nextPictureIndex = pictureIndex - 1;

					CreateAllPictureBitmaps(nextPictureIndex, ref newPictureBitmap, ref newLabelBitmap);
					StartAnimation();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will show the next picture in line
		/// </summary>
		/********************************************************************/
		private void ShowNextPicture()
		{
			lock (animationLock)
			{
				if (!animationRunning && (pictureIndex < pictures.Length - 1))
				{
					nextPictureIndex = pictureIndex + 1;

					CreateAllPictureBitmaps(nextPictureIndex, ref newPictureBitmap, ref newLabelBitmap);
					StartAnimation();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start the picture animation
		/// </summary>
		/********************************************************************/
		private void StartAnimation()
		{
			easePosition = 0.0;
			currentOpacity = 1.0f;
			newOpacity = 0.0f;

			if (pictureIndex <= nextPictureIndex)
			{
				currentXPos = 0;
				newXPos = pictureGroup.Width;
			}
			else
			{
				currentXPos = 0;
				newXPos = -pictureGroup.Width;
			}

			animationRunning = true;
			animationTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Will draw a new bitmap with the opacity given
		/// </summary>
		/********************************************************************/
		private Bitmap SetOpacity(Bitmap bitmap, float opacity)
		{
			ColorMatrix matrix = new ColorMatrix(fadeMatrix);
			matrix.Matrix33 = opacity;

			Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

			using (Graphics g = Graphics.FromImage(newBitmap))
			{
				using (ImageAttributes attributes = new ImageAttributes())
				{
					attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
					g.Clear(Color.Transparent);
					g.DrawImage(bitmap, new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
				}
			}

			return newBitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Will create the bitmaps needed to show the picture
		/// </summary>
		/********************************************************************/
		private void CreateAllPictureBitmaps(int index, ref Bitmap pictureBitmap, ref Bitmap labelBitmap)
		{
			PictureInfo pictureInfo = pictures[index];

			CreateLabelBitmap(pictureInfo.Description, ref labelBitmap);
			CreatePictureBitmap(pictureInfo.PictureData, ref pictureBitmap);
		}



		/********************************************************************/
		/// <summary>
		/// Will create the bitmap needed to show the picture label
		/// </summary>
		/********************************************************************/
		private void CreateLabelBitmap(string description, ref Bitmap labelBitmap)
		{
			labelBitmap?.Dispose();
			labelBitmap = new Bitmap(pictureLabelPictureBox.Width, pictureLabelPictureBox.Height);

			using (Font font = FontPalette.GetRegularFont())
			{
				string labelToDraw = description.EllipsisLine(pictureGroup.Handle, pictureLabelPictureBox.Width, font, out int newWidth);

				using (Graphics g = Graphics.FromImage(labelBitmap))
				{
					g.TextRenderingHint = TextRenderingHint.AntiAlias;

					Color color = fontPalette.GetDefaultFontColor();

					using (Brush brush = new SolidBrush(color))
					{
						int x = (labelBitmap.Width - newWidth) / 2;
						g.DrawString(labelToDraw, font, brush, x, 4);
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will create the bitmap needed to show the picture itself
		/// </summary>
		/********************************************************************/
		private void CreatePictureBitmap(byte[] pictureData, ref Bitmap pictureBitmap)
		{
			pictureBitmap?.Dispose();
			pictureBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);

			using (MemoryStream ms = new MemoryStream(pictureData))
			{
				using (Bitmap sourceBitmap = (Bitmap)Image.FromStream(ms))
				{
					int width = sourceBitmap.Width;
					int height = sourceBitmap.Height;

					// Scale the picture if needed and keep the aspect ratio
					double ratioX = (double)pictureBitmap.Width / width;
					double ratioY = (double)pictureBitmap.Height / height;

					if ((ratioX < 1.0) || (ratioY < 1.0))
					{
						double ratio = Math.Min(ratioX, ratioY);
						width = (int)(width * ratio);
						height = (int)(height * ratio);
					}

					using (Graphics g = Graphics.FromImage(pictureBitmap))
					{
						g.DrawImage(sourceBitmap, (pictureBitmap.Width - width) / 2, (pictureBitmap.Height - height) / 2, width, height);
					}
				}
			}
		}
		#endregion
	}
}
