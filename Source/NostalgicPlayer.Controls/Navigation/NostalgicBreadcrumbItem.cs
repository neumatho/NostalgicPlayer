/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.ComponentModel;
using Polycode.NostalgicPlayer.Controls.Designer;

namespace Polycode.NostalgicPlayer.Controls.Navigation
{
	/// <summary>
	/// A single item shown in a NostalgicBreadcrumb control
	/// </summary>
	[TypeConverter(typeof(NostalgicBreadcrumbItemConverter))]
	public class NostalgicBreadcrumbItem
	{
		private string text;
		private bool enabled;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumbItem() : this(string.Empty)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumbItem(string text) : this(text, true)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicBreadcrumbItem(string text, bool enabled)
		{
			this.text = text ?? string.Empty;
			this.enabled = enabled;
		}



		/********************************************************************/
		/// <summary>
		/// The text shown for the item
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The text shown for the item.")]
		[DefaultValue("")]
		public string Text
		{
			get => text;

			set
			{
				text = value ?? string.Empty;
				Owner?.ItemsChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicates whether the item can be clicked
		/// </summary>
		/********************************************************************/
		[Category("Behavior")]
		[Description("Indicates whether the item can be clicked.")]
		[DefaultValue(true)]
		public bool Enabled
		{
			get => enabled;

			set
			{
				enabled = value;
				Owner?.ItemsChanged();
			}
		}



		/********************************************************************/
		/// <summary>
		/// User defined data associated with the item
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Tag
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the text, so the collection editor shows something useful
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return text;
		}

		#region Internal properties
		/********************************************************************/
		/// <summary>
		/// The control the item belongs to
		/// </summary>
		/********************************************************************/
		internal NostalgicBreadcrumb Owner
		{
			get; set;
		}
		#endregion
	}
}
