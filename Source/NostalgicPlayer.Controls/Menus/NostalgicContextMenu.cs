/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Forms;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;
using Polycode.NostalgicPlayer.Controls.Types;

namespace Polycode.NostalgicPlayer.Controls.Menus
{
	/// <summary>
	/// Themed context menu with custom rendering
	/// </summary>
	public class NostalgicContextMenu : ContextMenuStrip, IThemeControl, IFontConfiguration
	{
		private readonly NostalgicMenuRenderer menuRenderer = new NostalgicMenuRenderer();
		private readonly HashSet<ToolStripDropDown> hookedDropDowns = new HashSet<ToolStripDropDown>();

		private FontConfiguration fontConfiguration;
		private NostalgicForm registeredForm;
		private INostalgicImageBank imageBank;
		private ImageBankArea imageArea = ImageBankArea.None;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicContextMenu()
		{
			Renderer = menuRenderer;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicContextMenu(IContainer container) : this()
		{
			container.Add(this);
		}



		/********************************************************************/
		/// <summary>
		/// Show the context menu with its top-left corner at the current
		/// mouse position
		/// </summary>
		/********************************************************************/
		public void Show(Control control)
		{
			Show(control, control.PointToClient(MousePosition), ToolStripDropDownDirection.BelowRight);
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Set the FontConfiguration component to use for this control
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("Which font configuration to use if you want to change the default font.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue(null)]
		public FontConfiguration UseFont
		{
			get => fontConfiguration;

			set
			{
				fontConfiguration = value;

				if (IsHandleCreated)
				{
					menuRenderer.UpdateFont(fontConfiguration);
					Invalidate();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// The image bank area passed to NostalgicToolStripMenuItem children
		/// so they can resolve their images
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The image bank area passed to NostalgicToolStripMenuItem children.")]
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
					ApplyImageBankToItems(Items);
				}
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

			menuRenderer.UpdateFont(fontConfiguration);

			base.OnHandleCreated(e);
		}
		#endregion

		#region ImageBank
		/********************************************************************/
		/// <summary>
		/// Provide the image bank used by NostalgicToolStripMenuItem that
		/// resolve their image from it. Propagates to existing items and
		/// any items added later
		/// </summary>
		/********************************************************************/
		internal void InitializeControl(INostalgicImageBank imageBank)
		{
			this.imageBank = imageBank;

			ApplyImageBankToItems(Items);
		}



		/********************************************************************/
		/// <summary>
		/// Push the current image bank to an item (and recursively into any
		/// drop-down panel it owns)
		/// </summary>
		/********************************************************************/
		private void ApplyImageBankToItem(ToolStripItem item)
		{
			if (item is NostalgicToolStripMenuItem menuItem)
			{
				menuItem.ImageArea = imageArea;

				menuItem.SetImageBank(imageBank);
			}

			if (item is ToolStripDropDownItem dropDownItem)
				ApplyImageBankToItems(dropDownItem.DropDown.Items);
		}



		/********************************************************************/
		/// <summary>
		/// Push the current image bank to a collection of items
		/// </summary>
		/********************************************************************/
		private void ApplyImageBankToItems(ToolStripItemCollection items)
		{
			foreach (ToolStripItem item in items)
				ApplyImageBankToItem(item);
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
			menuRenderer.SetTheme(theme);

			ApplyRendererToItems(Items);

			menuRenderer.UpdateFont(fontConfiguration);
			Invalidate();
		}
		#endregion

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Make sure new items get the custom renderer for their drop-down
		/// panels
		/// </summary>
		/********************************************************************/
		protected override void OnItemAdded(ToolStripItemEventArgs e)
		{
			base.OnItemAdded(e);

			ApplyRendererToItem(e.Item);
			ApplyImageBankToItem(e.Item);
		}



		/********************************************************************/
		/// <summary>
		/// Lazily locate the owning NostalgicForm the first time the menu
		/// opens and register with it for theme updates. This is the central
		/// hook hit by every Show overload (Show(), Show(Point), Show(Control,
		/// Point), etc.) so it covers them all
		/// </summary>
		/********************************************************************/
		protected override void OnOpening(CancelEventArgs e)
		{
			if (registeredForm == null)
			{
				Form form = SourceControl?.FindForm();
				if (form is NostalgicForm nostalgicForm)
				{
					registeredForm = nostalgicForm;
					nostalgicForm.RegisterThemedComponent(this);
				}
			}

			base.OnOpening(e);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Walk a collection of items and apply the custom renderer to all
		/// drop-down panels they own (recursively)
		/// </summary>
		/********************************************************************/
		private void ApplyRendererToItems(ToolStripItemCollection items)
		{
			foreach (ToolStripItem item in items)
				ApplyRendererToItem(item);
		}



		/********************************************************************/
		/// <summary>
		/// Apply the custom renderer to a single item's drop-down panel
		/// (recursively for nested sub-menus)
		/// </summary>
		/********************************************************************/
		private void ApplyRendererToItem(ToolStripItem item)
		{
			if (item is ToolStripDropDownItem dropDownItem)
			{
				ToolStripDropDown dropDown = dropDownItem.DropDown;
				dropDown.Renderer = menuRenderer;

				if (hookedDropDowns.Add(dropDown))
					dropDown.ItemAdded += DropDown_ItemAdded;

				ApplyRendererToItems(dropDown.Items);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Hook for items added to a drop-down panel after the context menu
		/// has been built, so that nested sub-menus also use the custom
		/// renderer
		/// </summary>
		/********************************************************************/
		private void DropDown_ItemAdded(object sender, ToolStripItemEventArgs e)
		{
			ApplyRendererToItem(e.Item);
			ApplyImageBankToItem(e.Item);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicContextMenu()
		{
			TypeDescriptor.AddProvider(new NostalgicContextMenuTypeDescriptionProvider(), typeof(NostalgicContextMenu));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicContextMenuTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(ContextMenuStrip));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RenderMode),
				nameof(RightToLeft),
				nameof(Size)
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicContextMenuTypeDescriptionProvider() : base(parent)
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
