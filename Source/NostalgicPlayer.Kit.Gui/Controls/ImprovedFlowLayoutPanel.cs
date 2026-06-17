/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Kit.Gui.Controls
{
	/// <summary>
	/// Improved version of the FlowLayoutPanel that can be used to create a more responsive layout
	/// </summary>
	public class ImprovedFlowLayoutPanel : FlowLayoutPanel
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ImprovedFlowLayoutPanel()
		{
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;		// WS_EX_COMPOSITED to reduce flickering

				return cp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnScroll(ScrollEventArgs se)
		{
			Invalidate();

			base.OnScroll(se);
		}
		#endregion
	}
}
