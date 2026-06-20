/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Controls.Events;
using Polycode.NostalgicPlayer.Controls.Extensions;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.External.Audius.Models.Users;
using Polycode.NostalgicPlayer.External.Download;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.AudiusWindow.Pages
{
	/// <summary>
	/// Shows the given profile information
	/// </summary>
	public partial class ProfileControl : UserControl, IDependencyInjectionControl
	{
		private const int Page_Tracks = 0;
		private const int Page_Playlists = 1;

		// How close to mid-gray (luminance 128) the background has to be before
		// we snap the cross to pure black/white instead of a plain inversion,
		// since an inversion of a mid-gray gives almost no contrast
		private const double MidGraySnapRange = 40.0;

		private IThemeManager themeManager;
		private INostalgicImageBank imageBank;

		private IPictureDownloader pictureDownloader;

		private UserModel userModel;
		private IAudiusPage currentPage;

		private Bitmap closeButtonImage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ProfileControl()
		{
			InitializeComponent();

			Disposed += ProfileControl_Disposed;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IThemeManager themeManager, INostalgicImageBank imageBank)
		{
			this.themeManager = themeManager;
			this.imageBank = imageBank;

			themeManager.ThemeChanged += ThemeChanged;

			// Set the tab titles
			tabControl.Pages[Page_Tracks].Text = Resources.IDS_AUDIUS_TAB_TRACKS;
			tabControl.Pages[Page_Playlists].Text = Resources.IDS_AUDIUS_TAB_PLAYLISTS;
		}



		/********************************************************************/
		/// <summary>
		/// Will initialize the control
		/// </summary>
		/********************************************************************/
		public void Initialize(UserModel user, IAudiusWindowApi audiusWindow, IPictureDownloader downloader)
		{
			pictureDownloader = downloader;

			userModel = user;

			SetupControls();
			UpdateCloseButton(false);
			UpdateLabelColors();

			// Initialize all pages
			profileTracksPageControl.Initialize(audiusWindow, pictureDownloader, user.Id);
			profilePlaylistsPageControl.Initialize(audiusWindow, pictureDownloader, user.Id);

			RefreshCurrentPage();
		}

		#region Events
		/********************************************************************/
		/// <summary>
		/// Event called when to close the profile
		/// </summary>
		/********************************************************************/
		public event EventHandler Close;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProfileControl_Disposed(object sender, EventArgs e)
		{
			themeManager.ThemeChanged -= ThemeChanged;

			currentPage?.CleanupPage();
			currentPage = null;

			closeButtonImage?.Dispose();
			closeButtonImage = null;
		}



		/********************************************************************/
		/// <summary>
		/// A theme change resets the label colors to the theme's text color, so
		/// re-apply our contrast colors. The cover photo does not change, so
		/// the cross is left untouched (it does not use a theme color)
		/// </summary>
		/********************************************************************/
		private void ThemeChanged(object sender, ThemeChangedEventArgs e)
		{
			UpdateLabelColors();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_MouseEnter(object sender, EventArgs e)
		{
			UpdateCloseButton(true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_MouseLeave(object sender, EventArgs e)
		{
			UpdateCloseButton(false);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Close_Click(object sender, EventArgs e)
		{
			if (Close != null)
				Close(this, EventArgs.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Is called when a tab is selected
		/// </summary>
		/********************************************************************/
		private void Tab_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshCurrentPage();
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Setup all the controls with the values from the item
		/// </summary>
		/********************************************************************/
		private void SetupControls()
		{
			nameLabel.Text = userModel.Name;
			handleLabel.Text = "@" + userModel.Handle;

			DownloadPictures();

			Invalidate(true);
		}



		/********************************************************************/
		/// <summary>
		/// Rebuilds the close button image so the cross is always visible on
		/// top of the cover photo, no matter how the photo looks. When the
		/// mouse hovers, a dotted focus frame is added around the button
		/// </summary>
		/********************************************************************/
		private void UpdateCloseButton(bool hover)
		{
			int width = closeButton.ClientSize.Width;
			int height = closeButton.ClientSize.Height;

			if ((width <= 0) || (height <= 0))
				return;

			// The cross shape. Only its shape (alpha) is used here, not its color
			Bitmap cross = imageBank.Audius.Close;

			// Build a mask telling which pixels make up the button graphics
			Bitmap result;

			using (Bitmap mask = new Bitmap(width, height))
			{
				using (Graphics g = Graphics.FromImage(mask))
				{
					// Center the cross in the button
					g.DrawImage(cross, (width - cross.Width) / 2, (height - cross.Height) / 2, cross.Width, cross.Height);
				}

				if (hover)
				{
					// Add a dotted focus frame around the edge of the button
					for (int x = 0; x < width; x += 2)
					{
						mask.SetPixel(x, 0, Color.White);
						mask.SetPixel(x, height - 1, Color.White);
					}

					for (int y = 0; y < height; y += 2)
					{
						mask.SetPixel(0, y, Color.White);
						mask.SetPixel(width - 1, y, Color.White);
					}
				}

				// Invert the cover photo behind each mask pixel (focus rectangle style)
				result = CreateInvertedOverlay(mask, closeButton.Location);

				// No cover photo behind the button - fall back to a solid color
				// that contrasts with the panel background color
				if (result == null)
					result = CreateSolidOverlay(mask, infoPanel.BackColor);
			}

			SetCloseButtonImage(result);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the text color of the labels that sit on top of the cover
		/// photo, so they stay readable no matter how the photo looks. The
		/// whole text gets a single color that contrasts with the average
		/// of the photo right behind it
		/// </summary>
		/********************************************************************/
		private void UpdateLabelColors()
		{
			nameLabel.ForeColor = GetLabelContrastColor(nameLabel.Bounds);
			handleLabel.ForeColor = GetLabelContrastColor(handleLabel.Bounds);
		}



		/********************************************************************/
		/// <summary>
		/// Creates an overlay bitmap with the same size as the given mask.
		/// For every visible (non-transparent) pixel in the mask, the
		/// resulting pixel becomes a color that contrasts with the cover
		/// photo shown right behind it - the same trick Windows uses for its
		/// focus rectangle, so the graphics stays visible no matter how the
		/// photo looks. clientLocation is where the overlay will be placed
		/// in the panel (control) coordinates.
		///
		/// Returns null if no cover photo has been set
		/// </summary>
		/********************************************************************/
		private Bitmap CreateInvertedOverlay(Bitmap mask, Point clientLocation)
		{
			Bitmap background = infoPanel.BackgroundBitmap;
			if (background == null)
				return null;

			Size panelSize = infoPanel.ClientSize;
			if ((panelSize.Width <= 0) || (panelSize.Height <= 0))
				return null;

			Rectangle sourceRect = infoPanel.GetSourceRectangle();

			float scaleX = (float)sourceRect.Width / panelSize.Width;
			float scaleY = (float)sourceRect.Height / panelSize.Height;

			Bitmap overlay = new Bitmap(mask.Width, mask.Height);

			for (int y = 0; y < mask.Height; y++)
			{
				for (int x = 0; x < mask.Width; x++)
				{
					int alpha = mask.GetPixel(x, y).A;
					if (alpha == 0)
						continue;

					// Map the overlay pixel to a pixel in the source background image
					int bx = sourceRect.X + (int)((clientLocation.X + x) * scaleX);
					int by = sourceRect.Y + (int)((clientLocation.Y + y) * scaleY);

					bx = Math.Max(0, Math.Min(bx, background.Width - 1));
					by = Math.Max(0, Math.Min(by, background.Height - 1));

					// Keep the mask's alpha so anti-aliased edges still blend nicely
					overlay.SetPixel(x, y, Color.FromArgb(alpha, GetContrastColor(background.GetPixel(bx, by))));
				}
			}

			return overlay;
		}



		/********************************************************************/
		/// <summary>
		/// Creates an overlay where every visible mask pixel is drawn in a
		/// single color that contrasts with the given background color.
		/// Used when there is no cover photo to invert against
		/// </summary>
		/********************************************************************/
		private Bitmap CreateSolidOverlay(Bitmap mask, Color background)
		{
			Color color = GetContrastColor(background);

			Bitmap overlay = new Bitmap(mask.Width, mask.Height);

			for (int y = 0; y < mask.Height; y++)
			{
				for (int x = 0; x < mask.Width; x++)
				{
					int alpha = mask.GetPixel(x, y).A;
					if (alpha != 0)
						overlay.SetPixel(x, y, Color.FromArgb(alpha, color));
				}
			}

			return overlay;
		}



		/********************************************************************/
		/// <summary>
		/// Finds a color that contrasts with the given background color. The
		/// background is inverted (focus rectangle style), but when it is
		/// too close to mid-gray - where an inversion would give almost no
		/// contrast - we snap to pure black or white instead
		/// </summary>
		/********************************************************************/
		private static Color GetContrastColor(Color background)
		{
			// Perceived brightness (ITU-R BT.601 luma)
			double luminance = (0.299 * background.R) + (0.587 * background.G) + (0.114 * background.B);

			if (Math.Abs(luminance - 128.0) < MidGraySnapRange)
				return luminance < 128.0 ? Color.White : Color.Black;

			return Color.FromArgb(255 - background.R, 255 - background.G, 255 - background.B);
		}



		/********************************************************************/
		/// <summary>
		/// Finds a text color that contrasts with the cover photo behind the
		/// given label bounds. Falls back to the panel background color when
		/// there is no cover photo
		/// </summary>
		/********************************************************************/
		private Color GetLabelContrastColor(Rectangle bounds)
		{
			Color background = GetAverageBackgroundColor(bounds);
			if (background.IsEmpty)
				background = infoPanel.BackColor;

			return GetContrastColor(background);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the average color of the cover photo shown behind the
		/// given control rectangle (in panel coordinates). Returns an empty
		/// color if no cover photo has been set
		/// </summary>
		/********************************************************************/
		private Color GetAverageBackgroundColor(Rectangle clientRect)
		{
			Bitmap background = infoPanel.BackgroundBitmap;
			if (background == null)
				return Color.Empty;

			Size panelSize = infoPanel.ClientSize;
			if ((panelSize.Width <= 0) || (panelSize.Height <= 0))
				return Color.Empty;

			Rectangle sourceRect = infoPanel.GetSourceRectangle();

			float scaleX = (float)sourceRect.Width / panelSize.Width;
			float scaleY = (float)sourceRect.Height / panelSize.Height;

			// Map the wanted control rectangle into source image coordinates
			int left = sourceRect.X + (int)(clientRect.Left * scaleX);
			int top = sourceRect.Y + (int)(clientRect.Top * scaleY);
			int right = sourceRect.X + (int)(clientRect.Right * scaleX);
			int bottom = sourceRect.Y + (int)(clientRect.Bottom * scaleY);

			// Clamp to the bitmap bounds
			left = Math.Max(0, Math.Min(left, background.Width - 1));
			top = Math.Max(0, Math.Min(top, background.Height - 1));
			right = Math.Max(left + 1, Math.Min(right, background.Width));
			bottom = Math.Max(top + 1, Math.Min(bottom, background.Height));

			// Sample the pixels in the area (use a small step to keep it fast)
			int stepX = Math.Max(1, (right - left) / 16);
			int stepY = Math.Max(1, (bottom - top) / 16);

			long sumR = 0, sumG = 0, sumB = 0;
			int count = 0;

			for (int y = top; y < bottom; y += stepY)
			{
				for (int x = left; x < right; x += stepX)
				{
					Color pixel = background.GetPixel(x, y);

					sumR += pixel.R;
					sumG += pixel.G;
					sumB += pixel.B;
					count++;
				}
			}

			if (count == 0)
				return Color.Empty;

			return Color.FromArgb((int)(sumR / count), (int)(sumG / count), (int)(sumB / count));
		}



		/********************************************************************/
		/// <summary>
		/// Assigns a new image to the close button and disposes the previous
		/// one (we generate these images ourselves)
		/// </summary>
		/********************************************************************/
		private void SetCloseButtonImage(Bitmap image)
		{
			Bitmap old = closeButtonImage;

			closeButtonImage = image;
			closeButton.Image = image;

			old?.Dispose();
		}



		/********************************************************************/
		/// <summary>
		/// Download pictures and initialize them
		/// </summary>
		/********************************************************************/
		private void DownloadPictures()
		{
			Bitmap coverPhoto = null;
			Bitmap profilePhoto = null;

			Task.Run(async () =>
			{
				Task<Bitmap> coverPhotoTask = pictureDownloader.GetPictureAsync(userModel.CoverPhoto?._640x, CancellationToken.None);
				Task<Bitmap> profilePhotoTask = pictureDownloader.GetPictureAsync(userModel.ProfilePicture?._150x150, CancellationToken.None);

				try
				{
					coverPhoto = await coverPhotoTask;
				}
				catch (Exception)
				{
					// Ignore any errors here
				}

				try
				{
					profilePhoto = await profilePhotoTask;
				}
				catch (Exception)
				{
					// Ignore any errors here
				}
			}).Wait();

			if (coverPhoto != null)
				infoPanel.SetBackgroundImage(coverPhoto);
			else
				infoPanel.BackColor = Color.Gainsboro;

			if (profilePhoto != null)
			{
				// Scale the bitmap to 128 x 128 pixels
				using (Bitmap scaledBitmap = new Bitmap(profilePhoto, 128, 128))
				{
					profilePictureBox.Image = scaledBitmap.CreateCircularBitmap(2);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Refresh current page
		/// </summary>
		/********************************************************************/
		private void RefreshCurrentPage()
		{
			currentPage?.CleanupPage();
			currentPage = null;

			switch (tabControl.SelectedIndex)
			{
				case Page_Tracks:
				{
					currentPage = profileTracksPageControl;
					break;
				}

				case Page_Playlists:
				{
					currentPage = profilePlaylistsPageControl;
					break;
				}
			}

			currentPage?.RefreshPage();
		}
		#endregion
	}
}
