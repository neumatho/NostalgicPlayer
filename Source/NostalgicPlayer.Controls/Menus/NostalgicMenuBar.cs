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
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Theme.Standard;

namespace Polycode.NostalgicPlayer.Controls.Menus
{
	/// <summary>
	/// Themed menu bar with custom rendering
	/// </summary>
	public class NostalgicMenuBar : MenuStrip, IThemeControl, IFontConfiguration
	{
		private readonly NostalgicMenuRenderer menuRenderer = new NostalgicMenuRenderer();
		private readonly HashSet<ToolStripDropDown> hookedDropDowns = new HashSet<ToolStripDropDown>();

		private FontConfiguration fontConfiguration;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicMenuBar()
		{
			Renderer = menuRenderer;
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
				if (fontConfiguration != null)
					fontConfiguration.FontChanged -= FontConfiguration_FontChanged;

				fontConfiguration = value;

				if (fontConfiguration != null)
					fontConfiguration.FontChanged += FontConfiguration_FontChanged;

				if (IsHandleCreated)
				{
					menuRenderer.UpdateFont(fontConfiguration);
					Invalidate();
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
		}
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// React when the attached FontConfiguration recalculates its font
		/// (e.g. theme manager just initialized, or one of FontType /
		/// FontStyle / FontSize changed at runtime)
		/// </summary>
		/********************************************************************/
		private void FontConfiguration_FontChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated)
			{
				menuRenderer.UpdateFont(fontConfiguration);
				Invalidate();
			}
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
		/// Hook for items added to a drop-down panel after the menu strip
		/// has been built, so that nested sub-menus also use the custom
		/// renderer
		/// </summary>
		/********************************************************************/
		private void DropDown_ItemAdded(object sender, ToolStripItemEventArgs e)
		{
			ApplyRendererToItem(e.Item);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicMenuBar()
		{
			TypeDescriptor.AddProvider(new NostalgicMenuBarTypeDescriptionProvider(), typeof(NostalgicMenuBar));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicMenuBarTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(MenuStrip));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RenderMode),
				nameof(RightToLeft),
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicMenuBarTypeDescriptionProvider() : base(parent)
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
