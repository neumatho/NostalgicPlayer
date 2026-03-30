/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Tab page with a Visible property that controls tab header
	/// visibility without conflicting with the TabControl's internal
	/// use of Visible for content management
	/// </summary>
	public class NostalgicTabPage : TabPage
	{
		private bool tabVisible = true;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicTabPage()
		{
			BackColor = Color.Transparent;
		}



		/********************************************************************/
		/// <summary>
		/// Show or hide the tab header. This shadows the base Visible
		/// property so that TabControl's internal page content management
		/// (which uses the base property) does not interfere
		/// </summary>
		/********************************************************************/
		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public new bool Visible
		{
			get => tabVisible;

			set
			{
				if (tabVisible != value)
				{
					tabVisible = value;
					(Parent as NostalgicTab)?.NotifyTabPageVisibilityChanged();
				}
			}
		}
	}
}
