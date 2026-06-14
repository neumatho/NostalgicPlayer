/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Polycode.NostalgicPlayer.Controls.Containers;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// Designer for NostalgicTab. The framework's TabControlDesigner is
	/// internal and cannot be reused, and it would add "Add Tab" verbs that
	/// create plain TabPage objects. This designer is based on the public
	/// ParentControlDesigner instead: it has no such verbs (pages are added
	/// through the Pages property), but it keeps the design-time behavior
	/// needed to work with tabs - clicking a tab header to switch page and
	/// dropping controls onto the selected page
	/// </summary>
	internal class NostalgicTabDesigner : ParentControlDesigner
	{
		private bool tabControlSelected;

		private ISelectionService selectionService;

		#region ParentControlDesigner overrides
		/********************************************************************/
		/// <summary>
		/// Initialize the designer
		/// </summary>
		/********************************************************************/
		public override void Initialize(IComponent component)
		{
			base.Initialize(component);

			AutoResizeHandles = true;

			selectionService = (ISelectionService)GetService(typeof(ISelectionService));

			if (selectionService != null)
				selectionService.SelectionChanged += OnSelectionChanged;
		}



		/********************************************************************/
		/// <summary>
		/// Add two pages when the control is first dropped on a form, so it
		/// is not empty. The pages are created as NostalgicTabPage and added
		/// to the Controls collection only
		/// </summary>
		/********************************************************************/
		public override void InitializeNewComponent(IDictionary defaultValues)
		{
			base.InitializeNewComponent(defaultValues);

			TabControl tc = (TabControl)Component;

			MemberDescriptor member = TypeDescriptor.GetProperties(tc)["Controls"];

			RaiseComponentChanging(member);

			AddPage();
			AddPage();

			RaiseComponentChanged(member, null, null);

			tc.SelectedIndex = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Only the tab strip and border are click-through (so tabs can be
		/// switched), the page area belongs to the page designer
		/// </summary>
		/********************************************************************/
		protected override bool GetHitTest(Point point)
		{
			TabControl tc = (TabControl)Control;

			if (tabControlSelected)
			{
				Point hitTest = Control.PointToClient(point);
				return !tc.DisplayRectangle.Contains(hitTest);
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Forward toolbox drops to the designer of the selected page, so
		/// the dropped control is placed on the page instead of the tab
		/// control itself
		/// </summary>
		/********************************************************************/
		protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
		{
			TabControl tc = (TabControl)Control;

			if (tc.SelectedTab == null)
				throw new ArgumentException(string.Format("{0} cannot be added because there is no selected tab page.", tool.DisplayName));

			IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));

			if (host != null)
			{
				if (host.GetDesigner(tc.SelectedTab) is ParentControlDesigner selectedTabPageDesigner)
					InvokeCreateTool(selectedTabPageDesigner, tool);
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Only tab pages can be parented by the tab control
		/// </summary>
		/********************************************************************/
		public override bool CanParent(Control control)
		{
			return control is TabPage;
		}



		/********************************************************************/
		/// <summary>
		/// Don't draw the design grid on top of the control
		/// </summary>
		/********************************************************************/
		protected override bool DrawGrid => false;



		/********************************************************************/
		/// <summary>
		/// Don't allow lasso selection of child controls
		/// </summary>
		/********************************************************************/
		protected override bool AllowControlLasso => false;



		/********************************************************************/
		/// <summary>
		/// The tab control should not participate with snap lines
		/// </summary>
		/********************************************************************/
		public override bool ParticipatesWithSnapLines => false;



		/********************************************************************/
		/// <summary>
		/// Hide SelectedIndex from serialization, so switching tabs in the
		/// designer does not mark the form as changed
		/// </summary>
		/********************************************************************/
		protected override void PreFilterProperties(IDictionary properties)
		{
			base.PreFilterProperties(properties);

			PropertyDescriptor prop = (PropertyDescriptor)properties["SelectedIndex"];

			if (prop != null)
				properties["SelectedIndex"] = TypeDescriptor.CreateProperty(typeof(NostalgicTabDesigner), prop, BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden);
		}



		/********************************************************************/
		/// <summary>
		/// Unsubscribe from the selection service
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			if (disposing && (selectionService != null))
			{
				selectionService.SelectionChanged -= OnSelectionChanged;
				selectionService = null;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Track whether the tab control itself is the selected component
		/// </summary>
		/********************************************************************/
		private void OnSelectionChanged(object sender, EventArgs e)
		{
			tabControlSelected = (selectionService != null) && selectionService.GetComponentSelected(Component);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new NostalgicTabPage and add it to the Controls
		/// collection
		/// </summary>
		/********************************************************************/
		private void AddPage()
		{
			TabControl tc = (TabControl)Component;

			IDesignerHost host = (IDesignerHost)GetService(typeof(IDesignerHost));

			if (host == null)
				return;

			NostalgicTabPage page = (NostalgicTabPage)host.CreateComponent(typeof(NostalgicTabPage));

			// Copy the (generated) name into the Text, so the tab header
			// shows something meaningful
			PropertyDescriptor nameProp = TypeDescriptor.GetProperties(page)["Name"];

			if ((nameProp != null) && (nameProp.PropertyType == typeof(string)))
			{
				string pageText = nameProp.GetValue(page) as string;

				if (pageText != null)
					TypeDescriptor.GetProperties(page)["Text"]?.SetValue(page, pageText);
			}

			tc.Controls.Add(page);
			tc.SelectedIndex = tc.TabCount - 1;
		}
		#endregion
	}
}
