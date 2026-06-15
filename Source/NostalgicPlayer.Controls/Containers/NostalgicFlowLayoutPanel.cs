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
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Containers
{
	/// <summary>
	/// A flow layout panel that gives the same functionality as the
	/// FlowLayoutPanel, but uses the themed NostalgicVScrollBar/NostalgicHScrollBar
	/// instead of the native scroll bars
	/// </summary>
	public partial class NostalgicFlowLayoutPanel : UserControl, IThemeControl, IDependencyInjectionControl
	{
		private IThemeManager themeManager;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicFlowLayoutPanel()
		{
			InitializeComponent();

			flowLayoutPanelInternal.SetControls(nostalgicVScrollBar, nostalgicHScrollBar, cornerPanel, this);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the control
		///
		/// Called from FormCreatorService
		/// </summary>
		/********************************************************************/
		public void InitializeControl(IThemeManager themeManager)
		{
			this.themeManager = themeManager;
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Gets or sets the direction in which the content is laid out
		/// </summary>
		/********************************************************************/
		[DefaultValue(FlowDirection.LeftToRight)]
		[Category("Layout")]
		[Description("Indicates the flow direction of the content")]
		public FlowDirection FlowDirection
		{
			get => flowLayoutPanelInternal.FlowDirection;

			set => flowLayoutPanelInternal.FlowDirection = value;
		}



		/********************************************************************/
		/// <summary>
		/// Gets or sets whether the content should wrap or be clipped
		/// </summary>
		/********************************************************************/
		[DefaultValue(true)]
		[Category("Layout")]
		[Description("Indicates whether the content should wrap when it reaches the edge")]
		public bool WrapContents
		{
			get => flowLayoutPanelInternal.WrapContents;

			set => flowLayoutPanelInternal.WrapContents = value;
		}
		#endregion

		#region Redirected properties
		/********************************************************************/
		/// <summary>
		/// Return the collection of controls hosted by the flow content.
		/// Use this to add and remove items, like the Controls collection on
		/// a normal FlowLayoutPanel.
		///
		/// Note: this is deliberately not called Controls, since that would
		/// collide with the UserControl Controls collection (the designer
		/// generates code that uses it) and create a circular reference
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ControlCollection FlowControls => flowLayoutPanelInternal.Controls;



		/********************************************************************/
		/// <summary>
		/// Return the size of the visible content area (excluding any
		/// visible scroll bars). Use this instead of ClientSize when you
		/// want to fit items to the visible width, like the native
		/// FlowLayoutPanel ClientSize shrinks when a scroll bar is shown
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Size ViewportSize => flowLayoutPanelInternal.ViewportSize;



		/********************************************************************/
		/// <summary>
		/// Return the current scroll position in pixels (0, 0 is the top
		/// left)
		/// </summary>
		/********************************************************************/
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point ScrollPosition => flowLayoutPanelInternal.ScrollPosition;
		#endregion

		#region Redirected events
		/********************************************************************/
		/// <summary>
		/// Is raised when the content is scrolled
		/// </summary>
		/********************************************************************/
		[Category("Action")]
		[Description("Occurs when the content is scrolled")]
		public event ScrollEventHandler ContentScrolled
		{
			add => flowLayoutPanelInternal.ContentScrolled += value;

			remove => flowLayoutPanelInternal.ContentScrolled -= value;
		}
		#endregion

		#region Redirected public methods
		/********************************************************************/
		/// <summary>
		/// Temporarily suspend the layout logic of the content while adding
		/// or removing many items. Call EndUpdate when done.
		///
		/// Note: this is not called SuspendLayout, since the designer
		/// generates code that uses SuspendLayout on the UserControl itself
		/// </summary>
		/********************************************************************/
		public void BeginUpdate()
		{
			flowLayoutPanelInternal.SuspendLayout();
		}



		/********************************************************************/
		/// <summary>
		/// Resume the layout logic of the content again
		/// </summary>
		/********************************************************************/
		public void EndUpdate()
		{
			flowLayoutPanelInternal.ResumeLayout();
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// Call this if you have added a lot of controls
		/// </summary>
		/********************************************************************/
		public void RefreshControls()
		{
			UpdateThemesOnControls(FlowControls);
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
			cornerPanel.BackColor = theme.ScrollBarColors.BackgroundColor;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Call this if you have added a lot of controls
		/// </summary>
		/********************************************************************/
		public void UpdateThemesOnControls(ControlCollection controls)
		{
			ITheme currentTheme = themeManager.CurrentTheme;

			foreach (Control ctrl in controls)
			{
				if (ctrl is IThemeControl themeControl)
					themeControl.SetTheme(currentTheme);

				UpdateThemesOnControls(ctrl.Controls);
			}
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicFlowLayoutPanel()
		{
			TypeDescriptor.AddProvider(new NostalgicFlowLayoutPanelTypeDescriptionProvider(), typeof(NostalgicFlowLayoutPanel));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicFlowLayoutPanelTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(UserControl));

			private static readonly string[] propertiesToHide =
			[
				nameof(AutoScroll),
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
			public NostalgicFlowLayoutPanelTypeDescriptionProvider() : base(parent)
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
