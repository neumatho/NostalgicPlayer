/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Tab page with a Visible property that controls tab header
	/// visibility without conflicting with the TabControl's internal
	/// use of Visible for content management
	/// </summary>
	public class NostalgicTabPage : TabPage
	{
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
			get;

			set
			{
				if (field != value)
				{
					field = value;
					(Parent as NostalgicTab)?.NotifyTabPageVisibilityChanged();
				}
			}
		} = true;

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicTabPage()
		{
			TypeDescriptor.AddProvider(new NostalgicTabTypeDescriptionProvider(), typeof(NostalgicTabPage));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer
		/// </summary>
		private sealed class NostalgicTabTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(TabPage));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(BorderStyle),
				nameof(RightToLeft),
				nameof(DrawMode),
				nameof(Appearance),
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
