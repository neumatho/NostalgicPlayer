/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Polycode.NostalgicPlayer.Controls.Containers;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// TypeConverter that provides a dropdown of the pages in a NostalgicTab,
	/// so the SelectedPage property can be used to switch between tabs in the
	/// designer. The pages are shown by their (site) name
	/// </summary>
	internal class SelectedTabPageConverter : TypeConverter
	{
		#region TypeConverter overrides
		/********************************************************************/
		/// <summary>
		/// We support standard values (dropdown)
		/// </summary>
		/********************************************************************/
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Only allow values from the dropdown
		/// </summary>
		/********************************************************************/
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return the pages of the owning tab control
		/// </summary>
		/********************************************************************/
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (context?.Instance is NostalgicTab tab)
			{
				List<NostalgicTabPage> pages = new List<NostalgicTabPage>();

				foreach (NostalgicTabPage page in tab.Pages)
					pages.Add(page);

				return new StandardValuesCollection(pages);
			}

			return new StandardValuesCollection(Array.Empty<NostalgicTabPage>());
		}



		/********************************************************************/
		/// <summary>
		/// We can convert a page to its display string
		/// </summary>
		/********************************************************************/
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return (destinationType == typeof(string)) || base.CanConvertTo(context, destinationType);
		}



		/********************************************************************/
		/// <summary>
		/// Convert a page to its display string (the page name)
		/// </summary>
		/********************************************************************/
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
				return value is NostalgicTabPage page ? GetPageName(page) : string.Empty;

			return base.ConvertTo(context, culture, value, destinationType);
		}



		/********************************************************************/
		/// <summary>
		/// We can convert a display string back to a page
		/// </summary>
		/********************************************************************/
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
		}



		/********************************************************************/
		/// <summary>
		/// Convert a display string back to the matching page
		/// </summary>
		/********************************************************************/
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if ((value is string name) && (context?.Instance is NostalgicTab tab))
			{
				foreach (NostalgicTabPage page in tab.Pages)
				{
					if (GetPageName(page) == name)
						return page;
				}

				return null;
			}

			return base.ConvertFrom(context, culture, value);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Return the name to show for a page in the dropdown
		/// </summary>
		/********************************************************************/
		private static string GetPageName(NostalgicTabPage page)
		{
			return page.Site?.Name ?? page.Name;
		}
		#endregion
	}
}
