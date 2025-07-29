/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.GuiKit.Controls
{
	/// <summary>
	/// Normal WinForm panel with double buffering enabled to reduce flickering
	/// </summary>
	public class ImprovedPanel : Panel
	{
		private Image backgroundImage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ImprovedPanel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			UpdateStyles();
		}



		/********************************************************************/
		/// <summary>
		/// Sets the background image to be used and scale it to fit the
		/// panels height
		/// </summary>
		/********************************************************************/
		public void SetBackgroundImage(Image image)
		{
			BackColor = Color.Transparent;

			int panelHeight = ClientRectangle.Height;

			float imageRatio = (float)image.Width / image.Height;
			int scaledWidth = (int)(panelHeight * imageRatio);

			Bitmap scaledImage = new Bitmap(scaledWidth, panelHeight);

			using (Graphics g = Graphics.FromImage(scaledImage))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(image, 0, 0, scaledWidth, panelHeight);
			}

			backgroundImage = scaledImage;
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
		    base.OnPaint(e);

		    if (backgroundImage != null)
		    {
				int panelWidth = ClientRectangle.Width;
				int panelHeight = ClientRectangle.Height;

				int imageWidth = backgroundImage.Width;
				int imageHeight = backgroundImage.Height;

				int x = 0;
				int y = 0;
				int width = imageWidth;
				int height = imageHeight;

				if (panelWidth <= imageWidth)
				{
					// Center the image horizontally and clip it
					width = panelWidth;
					x = (imageWidth - width) / 2;
				}
				else
				{
					float scaleX = (float)panelWidth / imageWidth;
					float visibleHeight = panelHeight / scaleX;

					if (visibleHeight > imageHeight)
						visibleHeight = imageHeight;

					y = (int)((imageHeight - visibleHeight) / 2.0f);
					height = (int)visibleHeight;
				}

				Rectangle drawRect = new Rectangle(x, y, width, height);
		        e.Graphics.DrawImage(backgroundImage, ClientRectangle, drawRect, GraphicsUnit.Pixel);
		    }
		}
	}
}
