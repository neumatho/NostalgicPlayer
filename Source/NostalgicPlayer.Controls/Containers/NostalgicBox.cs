/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;
using Polycode.NostalgicPlayer.Platform.Native;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Themed panel with a border
	/// </summary>
	public class NostalgicBox : Panel, IThemeControl
	{
		private const int BorderWidth = 1;

		private IBoxColors colors;

		private bool useBackgroundColor;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBox()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Set the FontConfiguration component to use for this control
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("Indicate if the background should be transparent or have a color.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(false)]
		public bool UseBackgroundColor
		{
			get => useBackgroundColor;

			set
			{
				useBackgroundColor = value;

				SetBackgroundColor();
				Invalidate();
			}
		}
		#endregion

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Initialize the control to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignerHelper.IsInDesignMode(this))
				SetTheme(new StandardTheme());

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
			colors = theme.BoxColors;

			SetBackgroundColor();
			DrawCustomNonClient();
		}



		/********************************************************************/
		/// <summary>
		/// Update the background color
		/// </summary>
		/********************************************************************/
		private void SetBackgroundColor()
		{
			if (colors != null)
				BackColor = useBackgroundColor ? colors.BackgroundColor : Color.Transparent;
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the client area minus the border, so anchoring and
		/// docking calculations use the correct size
		/// </summary>
		/********************************************************************/
		public override Rectangle DisplayRectangle
		{
			get
			{
				Rectangle rect = base.DisplayRectangle;

				if (!IsHandleCreated)
					return new Rectangle(rect.X, rect.Y, rect.Width - (2 * BorderWidth), rect.Height - (2 * BorderWidth));

				return rect;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process Windows messages to handle non-client area
		/// </summary>
		/********************************************************************/
		protected override void WndProc(ref Message m)
		{
			switch ((WM)m.Msg)
			{
				case WM.NCCALCSIZE:
				{
					RecalculateClientArea(m);
					
					m.Result = IntPtr.Zero;
					return;
				}

				case WM.NCPAINT:
				{
					DrawCustomNonClient();
					
					m.Result = IntPtr.Zero;
					return;
				}
			}

			base.WndProc(ref m);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Will recalculate the client area of the form
		/// </summary>
		/********************************************************************/
		private void RecalculateClientArea(Message m)
		{
			if (m.WParam != IntPtr.Zero)
			{
				// wParam is TRUE: using NCCALCSIZE_PARAMS
				NCCALCSIZE_PARAMS ncParams = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);
				
				// Shrink client area by border width on all sides
				ncParams.rgrc0.Left += BorderWidth;
				ncParams.rgrc0.Top += BorderWidth;
				ncParams.rgrc0.Right -= BorderWidth;
				ncParams.rgrc0.Bottom -= BorderWidth;
				
				Marshal.StructureToPtr(ncParams, m.LParam, false);
			}
			else
			{
				// wParam is FALSE: using simple RECT
				RECT rect = Marshal.PtrToStructure<RECT>(m.LParam);
				
				// Shrink client area by border width on all sides
				rect.Left += BorderWidth;
				rect.Top += BorderWidth;
				rect.Right -= BorderWidth;
				rect.Bottom -= BorderWidth;
				
				Marshal.StructureToPtr(rect, m.LParam, false);
			}
		}
		#endregion

		#region Drawing
		/********************************************************************/
		/// <summary>
		/// Will draw the title bar and border of the form
		/// </summary>
		/********************************************************************/
		private void DrawCustomNonClient()
		{
			if (!IsHandleCreated || (colors == null))
				return;

			// Draw border in non-client area
			IntPtr hdc = IntPtr.Zero;
			
			try
			{
				hdc = User32.GetWindowDC(Handle);
				if (hdc != IntPtr.Zero)
				{
					using (Graphics g = Graphics.FromHdc(hdc))
					{
						DrawBorder(g);
					}
				}
			}
			finally
			{
				if (hdc != IntPtr.Zero)
					User32.ReleaseDC(Handle, hdc);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Draw the themed border
		/// </summary>
		/********************************************************************/
		private void DrawBorder(Graphics g)
		{
			using (Pen borderPen = new Pen(colors.BorderColor, BorderWidth))
			{
				g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicBox()
		{
			TypeDescriptor.AddProvider(new NostalgicTabTypeDescriptionProvider(), typeof(NostalgicBox));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicTabTypeDescriptionProvider : TypeDescriptionProvider
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
			public NostalgicTabTypeDescriptionProvider() : base(parent)
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
