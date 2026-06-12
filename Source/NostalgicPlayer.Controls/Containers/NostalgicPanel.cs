/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// Just an invisible panel
	/// </summary>
	public class NostalgicPanel : Panel
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicPanel()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
		}

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
