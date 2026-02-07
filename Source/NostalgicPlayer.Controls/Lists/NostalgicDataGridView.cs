/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Components;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Lists
{
	/// <summary>
	/// Custom DataGridView with theme support and custom rendering
	/// </summary>
	public partial class NostalgicDataGridView : UserControl, ISupportInitialize, IThemeControl
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicDataGridView()
		{
			InitializeComponent();

			nostalgicDataGridViewInternal.SetControls(nostalgicVScrollBar, nostalgicHScrollBar, cornerPanel, this);
		}

		#region ISupportInitialize implementation
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		void ISupportInitialize.BeginInit()
		{
			((ISupportInitialize)nostalgicDataGridViewInternal).BeginInit();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		void ISupportInitialize.EndInit()
		{
			((ISupportInitialize)nostalgicDataGridViewInternal).EndInit();
		}
		#endregion

		#region Redirected properties
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[DefaultValue(false)]
		[Category("Behavior")]
		[Description("Indicates whether manual column repositioning is enabled")]
		public bool AllowUserToOrderColumns
		{
			get => nostalgicDataGridViewInternal.AllowUserToOrderColumns;

			set => nostalgicDataGridViewInternal.AllowUserToOrderColumns = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[DefaultValue(true)]
		[Category("Behavior")]
		[Description("Indicates whether users can resize columns")]
		public bool AllowUserToResizeColumns
		{
			get => nostalgicDataGridViewInternal.AllowUserToResizeColumns;

			set => nostalgicDataGridViewInternal.AllowUserToResizeColumns = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DataGridViewColumnCollection Columns => nostalgicDataGridViewInternal.Columns;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DataGridViewRowCollection Rows => nostalgicDataGridViewInternal.Rows;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public DataGridViewSelectedRowCollection SelectedRows => nostalgicDataGridViewInternal.SelectedRows;
		#endregion

		#region Redirected events
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public event EventHandler SelectionChanged;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected virtual void OnSelectionChanged(EventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public new event MouseEventHandler MouseDoubleClick;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (MouseDoubleClick != null)
				MouseDoubleClick(this, e);
		}
		#endregion

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
			get => nostalgicDataGridViewInternal.UseFont;

			set => nostalgicDataGridViewInternal.UseFont = value;
		}
		#endregion

		#region Public methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void AutoResizeRows()
		{
			nostalgicDataGridViewInternal.AutoResizeRows();
		}



		/********************************************************************/
		/// <summary>
		/// Returns information about the part of the DataGridView at the
		/// specified client coordinates
		/// </summary>
		/********************************************************************/
		public DataGridView.HitTestInfo HitTest(int x, int y)
		{
			return nostalgicDataGridViewInternal.HitTest(x, y);
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

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		internal bool IsInDesignMode => DesignMode;
		#endregion

		#region Handlers
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void SelectionChangedHandler(object sender, EventArgs e)
		{
			OnSelectionChanged(e);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MouseDoubleClickHandler(object sender, MouseEventArgs e)
		{
			OnMouseDoubleClick(e);
		}
		#endregion

		#region Designer filtering (hide properties from PropertyGrid)
		/********************************************************************/
		/// <summary>
		/// Register a provider that filters properties for the designer
		/// </summary>
		/********************************************************************/
		static NostalgicDataGridView()
		{
			TypeDescriptor.AddProvider(new NostalgicDataGridViewTypeDescriptionProvider(), typeof(NostalgicDataGridView));
		}

		/// <summary>
		/// Filter out properties we do not want to show in the designer.
		/// This is needed for properties that we cannot override, but we do
		/// it for all our hidden properties to be sure
		/// </summary>
		private sealed class NostalgicDataGridViewTypeDescriptionProvider : TypeDescriptionProvider
		{
			private static readonly TypeDescriptionProvider parent = TypeDescriptor.GetProvider(typeof(UserControl));

			private static readonly string[] propertiesToHide =
			[
				nameof(BackColor),
				nameof(BorderStyle),
				nameof(BackColor),
				nameof(BackgroundImage),
				nameof(BackgroundImageLayout),
				nameof(Font),
				nameof(ForeColor),
				nameof(RightToLeft),
			];

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public NostalgicDataGridViewTypeDescriptionProvider() : base(parent)
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
