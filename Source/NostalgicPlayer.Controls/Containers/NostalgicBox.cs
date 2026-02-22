/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Native;
using Polycode.NostalgicPlayer.Controls.Theme;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Themed panel with a border
	/// </summary>
	public class NostalgicBox : Panel, IThemeControl
	{
		private const int BorderWidth = 1;

		private IBoxColors colors;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBox()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

		#region Initialize
		/********************************************************************/
		/// <summary>
		/// Initialize the control to use custom rendering
		/// </summary>
		/********************************************************************/
		protected override void OnHandleCreated(EventArgs e)
		{
			if (DesignMode)
				SetTheme(ThemeManagerFactory.GetThemeManager().CurrentTheme);

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

			DrawCustomNonClient();
		}
		#endregion

		#region Overrides
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



		/********************************************************************/
		/// <summary>
		/// Will draw the title bar and border of the form
		/// </summary>
		/********************************************************************/
		private void DrawCustomNonClient()
		{
			if (!IsHandleCreated)
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
			using (Pen borderPen = new Pen(colors.BorderColor, 1))
			{
				g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
			}
		}
		#endregion
	}
}
