/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls.Designer;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Types;

namespace Polycode.NostalgicPlayer.Controls.Menus
{
	/// <summary>
	/// Menu item that picks its image from the image bank using
	/// ImageArea/ImageName
	/// </summary>
	public class NostalgicToolStripMenuItem : ToolStripMenuItem, IImageBank
	{
		private ImageBankArea imageArea = ImageBankArea.None;
		private string imageName = string.Empty;

		private INostalgicImageBank imageBank;

		private MethodInfo cachedImageProperty;
		private object cachedAreaObject;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicToolStripMenuItem()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NostalgicToolStripMenuItem(string text) : base(text)
		{
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Select the image name within the chosen area
		/// </summary>
		/********************************************************************/
		[Category("Appearance")]
		[Description("The image name within the chosen area.")]
		[TypeConverter(typeof(BankImageNameConverter))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string ImageName
		{
			get => imageName;

			set
			{
				if (value != imageName)
				{
					imageName = value ?? string.Empty;

					UpdateImageBinding();
					RefreshImage();
				}
			}
		}
		#endregion

		#region ImageBank
		/********************************************************************/
		/// <summary>
		/// Set the image bank to resolve the image from. Called by the
		/// owning NostalgicContextMenu when it receives the bank
		/// </summary>
		/********************************************************************/
		internal void SetImageBank(INostalgicImageBank imageBank)
		{
			this.imageBank = imageBank;

			UpdateImageBinding();
			RefreshImage();
		}



		/********************************************************************/
		/// <summary>
		/// Return the image rendered with the given color. Called by the
		/// renderer when it knows the current state-dependent text color
		/// </summary>
		/********************************************************************/
		internal Bitmap GetImage(Color color)
		{
			return cachedImageProperty?.Invoke(cachedAreaObject, [ color ]) as Bitmap;
		}



		/********************************************************************/
		/// <summary>
		/// Select the image bank area to pick an image from
		/// </summary>
		/********************************************************************/
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageBankArea ImageArea
		{
			get => imageArea;

			set
			{
				if (value != imageArea)
				{
					imageArea = value;

					UpdateImageBinding();
					RefreshImage();
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Update the cached reflection binding for the selected image
		/// </summary>
		/********************************************************************/
		private void UpdateImageBinding()
		{
			cachedImageProperty = null;
			cachedAreaObject = null;

			if ((imageBank == null) || (imageArea == ImageBankArea.None) || string.IsNullOrEmpty(imageName))
				return;

			cachedAreaObject = BankImageNameConverter.GetAreaObject(imageBank, imageArea);
			if (cachedAreaObject == null)
				return;

			Type interfaceType = BankImageNameConverter.GetAreaInterfaceType(imageArea);
			cachedImageProperty = interfaceType?.GetMethod(imageName, BindingFlags.Public | BindingFlags.Instance);
		}



		/********************************************************************/
		/// <summary>
		/// Refresh the Image property from the current binding. The actual
		/// color used here does not matter, as the renderer redraws the
		/// image with the current state-dependent color. The image only
		/// needs to be set so layout reserves space for it
		/// </summary>
		/********************************************************************/
		private void RefreshImage()
		{
			Image = GetImage(Color.Black);
		}
		#endregion
	}
}
