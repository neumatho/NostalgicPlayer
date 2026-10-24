/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Drawing;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Images
{
	/// <summary>
	/// Holds all the general usable images
	/// </summary>
	internal class GeneralImages : ThemedImageBase, IGeneralImages
	{
		private const string Category = "General";

		private Bitmap logo;

		private Bitmap error;
		private Bitmap warning;
		private Bitmap information;
		private Bitmap question;

		private Bitmap search;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public GeneralImages(IThemeManager themeManager) : base(themeManager)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Dispose all the images
		/// </summary>
		/********************************************************************/
		public override void Dispose()
		{
			base.Dispose();

			FlushImages();
		}



		/********************************************************************/
		/// <summary>
		/// Flush images
		/// </summary>
		/********************************************************************/
		protected override void FlushImages()
		{
			logo?.Dispose();
			logo = null;

			error?.Dispose();
			error = null;
			warning?.Dispose();
			warning = null;
			information?.Dispose();
			information = null;
			question?.Dispose();
			question = null;

			search?.Dispose();
			search = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the logo image
		/// </summary>
		/********************************************************************/
		public Bitmap Logo
		{
			get
			{
				if (logo == null)
					logo = GetBitmap(Category, "Logo.png");

				return logo;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the message box error icon
		/// </summary>
		/********************************************************************/
		public Bitmap Error
		{
			get
			{
				if (error == null)
					error = GetSvgBitmap(Category, nameof(IGeneralImages.Error), CurrentColors.MessageBoxErrorColor, 32, 32);

				return error;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the message box warning icon
		/// </summary>
		/********************************************************************/
		public Bitmap Warning
		{
			get
			{
				if (warning == null)
					warning = GetSvgBitmap(Category, nameof(IGeneralImages.Warning), CurrentColors.MessageBoxWarningColor, 32, 32);

				return warning;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the message box information icon
		/// </summary>
		/********************************************************************/
		public Bitmap Information
		{
			get
			{
				if (information == null)
					information = GetSvgBitmap(Category, nameof(IGeneralImages.Information), CurrentColors.MessageBoxInformationColor, 32, 32);

				return information;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the message box question icon
		/// </summary>
		/********************************************************************/
		public Bitmap Question
		{
			get
			{
				if (question == null)
					question = GetSvgBitmap(Category, nameof(IGeneralImages.Question), CurrentColors.MessageBoxQuestionColor, 32, 32);

				return question;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Gets the search button image
		/// </summary>
		/********************************************************************/
		public Bitmap Search
		{
			get
			{
				if (search == null)
					search = GetSvgBitmap(Category, nameof(IGeneralImages.Search), CurrentColors.SearchColor, 20, 20);

				return search;
			}
		}
	}
}
