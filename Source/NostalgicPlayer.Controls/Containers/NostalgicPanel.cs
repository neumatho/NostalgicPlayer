/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Just an invisible panel
	/// </summary>
	public class NostalgicPanel : Panel
	{
		private Image backgroundImage;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicPanel()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the part of the (already scaled) background image
		/// that is shown inside the panel, using the same logic as OnPaint()
		/// </summary>
		/********************************************************************/
		public Rectangle GetSourceRectangle()
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

			return new Rectangle(x, y, width, height);
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
		/// Returns the (already scaled) background image, or null if none
		/// has been set
		/// </summary>
		/********************************************************************/
		public Bitmap BackgroundBitmap => backgroundImage as Bitmap;

		#region Overrides



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
				Rectangle drawRect = GetSourceRectangle();
		        e.Graphics.DrawImage(backgroundImage, ClientRectangle, drawRect, GraphicsUnit.Pixel);
		    }
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicPanel()
		{
			TypeDescriptor.AddProvider(new NostalgicPanelTypeDescriptionProvider(), typeof(NostalgicPanel));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicPanelTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Panel));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(BorderStyle),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicPanelTypeDescriptionProvider() : base(parent)
			{
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
			{
				return new HidingTypeDescriptor(base.GetTypeDescriptor(objectType, instance), propertiesToHide);
			}
		}
		#endregion
	}
}
