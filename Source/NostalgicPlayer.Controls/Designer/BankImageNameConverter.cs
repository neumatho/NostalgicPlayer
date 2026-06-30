/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Polycode.NostalgicPlayer.Controls.Images;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;
using Polycode.NostalgicPlayer.Controls.Types;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// TypeConverter that provides a dynamic dropdown of image names
	/// based on the selected ImageBankArea. Uses reflection to find
	/// all Bitmap properties on the area's interface
	/// </summary>
	internal class BankImageNameConverter : TypeConverter
	{
		/********************************************************************/
		/// <summary>
		/// Return the interface type for the given area
		/// </summary>
		/********************************************************************/
		internal static Type GetAreaInterfaceType(ImageBankArea area)
		{
			return area switch
			{
				ImageBankArea.General => typeof(IGeneralImages),
				ImageBankArea.Main => typeof(IMainImages),
				ImageBankArea.ModuleInformation => typeof(IModuleInformationImages),
				ImageBankArea.SampleInformation => typeof(ISampleInformationImages),
				ImageBankArea.Audius => typeof(IAudiusImages),
				_ => null
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the area object from the image bank for the given area
		/// </summary>
		/********************************************************************/
		internal static object GetAreaObject(INostalgicImageBank bank, ImageBankArea area)
		{
			return area switch
			{
				ImageBankArea.General => bank.General,
				ImageBankArea.Main => bank.Main,
				ImageBankArea.ModuleInformation => bank.ModuleInformation,
				ImageBankArea.SampleInformation => bank.SampleInformation,
				ImageBankArea.Audius => bank.Audius,
				_ => null
			};
		}

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
		/// Return the available image names for the selected area
		/// </summary>
		/********************************************************************/
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if (context?.Instance is IImageBank imageBank)
			{
				Type interfaceType = GetAreaInterfaceType(imageBank.ImageArea);
				if (interfaceType != null)
				{
					string[] names = interfaceType
						.GetProperties(BindingFlags.Public | BindingFlags.Instance)
						.Where(p => p.PropertyType == typeof(Bitmap))
						.Select(p => p.Name)
						.OrderBy(n => n)
						.ToArray();

					string[] result = new string[names.Length + 1];
					result[0] = string.Empty;
					Array.Copy(names, 0, result, 1, names.Length);

					return new StandardValuesCollection(result);
				}
			}

			return new StandardValuesCollection(new[] { string.Empty });
		}
		#endregion
	}
}
