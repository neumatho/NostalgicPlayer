/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Buttons
{
	/// <summary>
	/// Themed button that displays an image from the image bank
	/// </summary>
	public class NostalgicImageButton : Button, IThemeControl, IDependencyInjectionControl
	{
		/// <summary>
		/// Colors for a single visual state
		/// </summary>
		protected struct StateColors
		{
			/// <summary></summary>
			public Color BorderColor { get; init; }
			/// <summary></summary>
			public Color BackgroundStartColor { get; init; }
			/// <summary></summary>
			public Color BackgroundStopColor { get; init; }
		}

		/// <summary>
		/// The themed button colors
		/// </summary>
		protected IButtonColors colors;

		private ImageBankArea imageArea = ImageBankArea.None;
		private string imageName = string.Empty;

		private INostalgicImageBank imageBank;
		private ThemeManager designTimeThemeManager;

		private PropertyInfo cachedImageProperty;
		private object cachedAreaObject;

		/// <summary>
		/// True while the mouse is over the control
		/// </summary>
		protected bool isHovered;
		/// <summary>
		/// True while mouse down or space-bar held
		/// </summary>
		protected bool isPressed;
		private bool isSpacePressed;	// Track if space currently holds the pressed state
		private bool isFocused;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicImageButton()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Select the image bank area to pick an image from
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The image bank area to pick an image from.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(ImageBankArea.None)]
		public ImageBankArea ImageArea
		{
			get => imageArea;

			set
			{
				if (value != imageArea)
				{
					imageArea = value;
					imageName = string.Empty;

					UpdateImageBinding();
					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Select the image name within the chosen area
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The image name within the chosen area.")]
		[TypeConverter(typeof(BankImageNameConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string ImageName
		{
			get => imageName;

			set
			{
				if (value != imageName)
				{
					imageName = value ?? string.Empty;

					UpdateImageBinding();
					Invalidate();
				}
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Initialize the control with dependency injection
		///
		/// Called from ControlInitializerService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(INostalgicImageBank imageBank)
		{
			this.imageBank = imageBank;

			UpdateImageBinding();
			Invalidate();
		}



		/********************************************************************/
		/// <summary>
		/// Clean up design-time resources
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing && (designTimeThemeManager != null))
			{
				(imageBank as IDisposable)?.Dispose();
				imageBank = null;

				designTimeThemeManager.Dispose();
				designTimeThemeManager = null;
			}

			base.Dispose(disposing);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the form to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignerHelper.IsInDesignMode(this))
			{
				SetTheme(new StandardTheme());

				designTimeThemeManager = new ThemeManager();
				imageBank = new NostalgicImageBank(designTimeThemeManager);

				UpdateImageBinding();
			}

			base.OnHandleCreated(e);
		}
		#endregion

		#region Theme
		/********************************************************************/
		/// <summary>
		/// Will setup the theme for the control
		/// </summary>
		/********************************************************************/
		public void SetTheme(ITheme theme)
		{
			colors = theme.ButtonColors;

			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseEnter(EventArgs e)
		{
			isHovered = true;
			Invalidate();

			base.OnMouseEnter(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseLeave(EventArgs e)
		{
			isHovered = false;
			Invalidate();

			base.OnMouseLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isPressed = true;
				Invalidate();
			}

			base.OnMouseDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (isPressed && !isSpacePressed)
			{
				isPressed = false;
				Invalidate();
			}

			base.OnMouseUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseMove(MouseEventArgs e)
		{
			// While captured (mouse button down) Windows keeps sending us mouse moves even outside.
			// We must manually track hover state when pressed
			bool inside = ClientRectangle.Contains(e.Location);

			if (inside)
			{
				if (!isHovered)
				{
					isHovered = true;
					Invalidate();
				}
				else if (!isPressed && (e.Button == MouseButtons.Left) && Focused)
				{
					isPressed = true;
					Invalidate();
				}
			}
			else
			{
				if (isPressed)
				{
					isPressed = false;
					Invalidate();
				}
			}

			base.OnMouseMove(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnMouseCaptureChanged(EventArgs e)
		{
			// If capture is lost unexpectedly while pressed, normalize state
			if (!Capture && isPressed && !isSpacePressed)
			{
				isPressed = false;
				Invalidate();
			}

			base.OnMouseCaptureChanged(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnEnter(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnEnter(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnLeave(EventArgs e)
		{
			isFocused = false;
			isSpacePressed = false;
			isPressed = false;
			Invalidate();

			base.OnLeave(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnGotFocus(EventArgs e)
		{
			Invalidate();

			base.OnGotFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnLostFocus(EventArgs e)
		{
			Invalidate();

			base.OnLostFocus(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Space) && !isSpacePressed)
			{
				isSpacePressed = true;
				isPressed = true;
				Invalidate();
			}

			base.OnKeyDown(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space && isSpacePressed)
			{
				isSpacePressed = false;
				isPressed = false;
				Invalidate();
			}

			base.OnKeyUp(e);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!Enabled)
			{
				isSpacePressed = false;
				isPressed = false;
			}

			Invalidate();

			base.OnEnabledChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// Don't do anything, we have all painting in OnPaint
		/// </summary>
		/********************************************************************/
		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Paint the whole control
		/// </summary>
		/********************************************************************/
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

			Rectangle rect = ClientRectangle;
			StateColors stateColors = GetColors();

			DrawBackground(g, rect, stateColors);
			DrawImage(g, rect);
			DrawFocus(g, rect);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update the cached reflection binding for the selected image
		/// </summary>
		/********************************************************************/
		private void UpdateImageBinding()
		{
			cachedImageProperty = null;
			cachedAreaObject = null;

			if ((imageBank == null) || (imageArea == ImageBankArea.None) || string.IsNullOrEmpty(imageName))
				return;

			cachedAreaObject = BankImageNameConverter.GetAreaObject(imageBank, imageArea);
			if (cachedAreaObject == null)
				return;

			Type interfaceType = BankImageNameConverter.GetAreaInterfaceType(imageArea);
			cachedImageProperty = interfaceType?.GetProperty(imageName, BindingFlags.Public | BindingFlags.Instance);
		}



		/********************************************************************/
		/// <summary>
		/// Get the current image from the image bank
		/// </summary>
		/********************************************************************/
		private Bitmap GetCurrentImage()
		{
			return cachedImageProperty?.GetValue(cachedAreaObject) as Bitmap;
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Return the colors to use for the current state
		/// </summary>
		/********************************************************************/
		protected virtual StateColors GetColors()
		{
			if (!Enabled)
			{
				return new StateColors
				{
					BorderColor = colors.DisabledBorderColor,
					BackgroundStartColor = colors.DisabledBackgroundStartColor,
					BackgroundStopColor = colors.DisabledBackgroundStopColor
				};
			}

			if (isPressed)
			{
				return new StateColors
				{
					BorderColor = colors.PressedBorderColor,
					BackgroundStartColor = colors.PressedBackgroundStartColor,
					BackgroundStopColor = colors.PressedBackgroundStopColor
				};
			}

			if (isHovered)
			{
				return new StateColors
				{
					BorderColor = colors.HoverBorderColor,
					BackgroundStartColor = colors.HoverBackgroundStartColor,
					BackgroundStopColor = colors.HoverBackgroundStopColor
				};
			}

			if (isFocused)
			{
				return new StateColors
				{
					BorderColor = colors.FocusedBorderColor,
					BackgroundStartColor = colors.FocusedBackgroundStartColor,
					BackgroundStopColor = colors.FocusedBackgroundStopColor
				};
			}

			return new StateColors
			{
				BorderColor = colors.NormalBorderColor,
				BackgroundStartColor = colors.NormalBackgroundStartColor,
				BackgroundStopColor = colors.NormalBackgroundStopColor
			};
		}



		/********************************************************************/
		/// <summary>
		/// Draw the background and border with square corners
		/// </summary>
		/********************************************************************/
		private void DrawBackground(Graphics g, Rectangle rect, StateColors stateColors)
		{
			Rectangle outerRect = new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

			using (LinearGradientBrush brush = new LinearGradientBrush(outerRect, stateColors.BackgroundStartColor, stateColors.BackgroundStopColor, LinearGradientMode.Vertical))
			{
				g.FillRectangle(brush, outerRect);
			}

			using (Pen p = new Pen(stateColors.BorderColor))
			{
				g.DrawRectangle(p, outerRect);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the image centered in the button
		/// </summary>
		/********************************************************************/
		private void DrawImage(Graphics g, Rectangle rect)
		{
			Bitmap image = GetCurrentImage();
			if (image == null)
				return;

			int x = rect.X + ((rect.Width - image.Width) / 2);
			int y = rect.Y + ((rect.Height - image.Height) / 2);

			if (Enabled)
			{
				g.DrawImage(image, x, y, image.Width, image.Height);
			}
			else
			{
				using (ImageAttributes attributes = new ImageAttributes())
				{
					ColorMatrix matrix = new ColorMatrix
					{
						Matrix33 = 0.4f
					};

					attributes.SetColorMatrix(matrix);
					g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the focus rectangle
		/// </summary>
		/********************************************************************/
		private void DrawFocus(Graphics g, Rectangle rect)
		{
			if (Focused && Enabled)
			{
				Rectangle focusRect = Rectangle.Inflate(rect, -3, -3);
				ControlPaint.DrawFocusRectangle(g, focusRect);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicImageButton()
		{
			TypeDescriptor.AddProvider(new NostalgicImageButtonTypeDescriptionProvider(), typeof(NostalgicImageButton));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicImageButtonTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(Button));

			private static readonly string[] propertiesToHide =
			[
				nameof(FlatStyle),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Image),
				nameof(ImageAlign),
				nameof(ImageIndex),
				nameof(ImageKey),
				nameof(ImageList),
				nameof(FlatAppearance),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
				nameof(Text),
				nameof(TextAlign),
				nameof(TextImageRelation),
				nameof(UseVisualStyleBackColor),
				nameof(UseCompatibleTextRendering),
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicImageButtonTypeDescriptionProvider() : base(parent)
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
