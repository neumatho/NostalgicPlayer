/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Gui.Extensions;
using Polycode.NostalgicPlayer.Library.Containers;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModuleInfoWindow.Pages
{
	/// <summary>
	/// 
	/// </summary>
	public partial class PicturesPageControl : UserControl, IDependencyInjectionControl
	{
		private IThemeManager themeManager;

		private bool themeHasChanged;

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

		private static readonly float[][] fadeMatrix =
		[
			[ 1, 0, 0, 0, 0 ],
			[ 0, 1, 0, 0, 0 ],
			[ 0, 0, 1, 0, 0 ],
			[ 0, 0, 0, 1, 0 ],
			[ 0, 0, 0, 0, 1 ]
		];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PicturesPageControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IThemeManager themeManager)
		{
			this.themeManager = themeManager;
			themeManager.ThemeChanged += ThemeManagerOnThemeChanged;

			animationLock = new Lock();
			themeHasChanged = false;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the control
		/// </summary>
		/********************************************************************/
		public void CleanupControl()
		{
			themeManager.ThemeChanged -= ThemeManagerOnThemeChanged;

			CleanupPictures();
			CleanupPictureButtons();
		}



		/********************************************************************/
		/// <summary>
		/// Will refresh the control with new data
		/// </summary>
		/********************************************************************/
		public bool RefreshControl(bool isPlaying, ModuleInfoStatic staticInfo)
		{
			bool visible = false;

			CleanupPictures();

			// Check to see if there are any module loaded at the moment
			if (isPlaying)
			{
				// Add pictures
				if (staticInfo.Pictures?.Length > 0)
				{
					visible = true;

					// Remember the pictures
					pictures = staticInfo.Pictures;

					// Prepare needed bitmaps
					InitializePictures();
				}
			}

			return visible;
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a key is pressed in the window
		/// </summary>
		/********************************************************************/
		public bool ProcessKey(Keys key)
		{
			switch (key)
			{
				case Keys.Left:
				{
					ShowPreviousPicture();
					return true;
				}

				case Keys.Right:
				{
					ShowNextPicture();
					return true;
				}
			}

			return false;
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called when the theme changes
		/// </summary>
		/********************************************************************/
		private void ThemeManagerOnThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			lock (animationLock)
			{
				if (!animationRunning)
					UpdateThemedControls();
				else
					themeHasChanged = true;
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
			if (animationLock != null)
			{
				lock (animationLock)
				{
					// Replace the next/previous buttons
					int newYPos = ((pictureBox.Height - previousPictureButton.Height) / 2) + pictureBox.Location.Y;
					previousPictureButton.Location = new Point(8, newYPos);
					nextPictureButton.Location = new Point(picturePanel.Width - nextPictureButton.Width - 8, newYPos);

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
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Load an SVG resource and convert it to bitmap
		/// </summary>
		/********************************************************************/
		private Bitmap GetResourceBitmap(string resourceName)
		{
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Polycode.NostalgicPlayer.Client.GuiPlayer.Resources.{resourceName}.svg"))
			{
				return SvgHelper.ConvertToBitmap(stream, themeManager.CurrentTheme.LabelColors.TextColor, 24, 24);
			}
		}



		/********************************************************************/
		/// <summary>
		/// When the theme has changed, update themed custom controls
		/// </summary>
		/********************************************************************/
		private void UpdateThemedControls()
		{
			InitializePictureButtons();

			if (currentLabelBitmap != null)
				CreateLabelBitmap(pictures[pictureIndex].Description, ref currentLabelBitmap);
		}



		/********************************************************************/
		/// <summary>
		/// Will render the picture button images
		/// </summary>
		/********************************************************************/
		private void InitializePictureButtons()
		{
			CleanupPictureButtons();

			previousPictureButton.Image = GetResourceBitmap("PreviousPicture");
			nextPictureButton.Image = GetResourceBitmap("NextPicture");
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup the picture button images
		/// </summary>
		/********************************************************************/
		private void CleanupPictureButtons()
		{
			nextPictureButton.Image?.Dispose();
			nextPictureButton.Image = null;

			previousPictureButton.Image?.Dispose();
			previousPictureButton.Image = null;
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
				newXPos = picturePanel.Width;
			}
			else
			{
				currentXPos = 0;
				newXPos = -picturePanel.Width;
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

			Font font = themeManager.CurrentTheme.StandardFonts.RegularFont;
			string labelToDraw = description.EllipsisLine(picturePanel.Handle, pictureLabelPictureBox.Width, font, out int newWidth);

			using (Graphics g = Graphics.FromImage(labelBitmap))
			{
				g.TextRenderingHint = TextRenderingHint.AntiAlias;

				Color color = themeManager.CurrentTheme.LabelColors.TextColor;

				using (Brush brush = new SolidBrush(color))
				{
					int x = (labelBitmap.Width - newWidth) / 2;
					g.DrawString(labelToDraw, font, brush, x, 4);
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
					int posDiff = (int)(pos * picturePanel.Width);

					if (pictureIndex <= nextPictureIndex)
					{
						currentXPos = -posDiff;
						newXPos = picturePanel.Width - posDiff;
					}
					else
					{
						currentXPos = posDiff;
						newXPos = -(picturePanel.Width - posDiff);
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

					if (themeHasChanged)
					{
						UpdateThemedControls();
						themeHasChanged = false;
					}
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
	}
}
