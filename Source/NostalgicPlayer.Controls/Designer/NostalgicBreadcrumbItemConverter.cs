/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using Polycode.NostalgicPlayer.Controls.Navigation;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// TypeConverter that makes the designer serialize a
	/// NostalgicBreadcrumbItem as a constructor call instead of putting it
	/// into the resource file
	/// </summary>
	internal class NostalgicBreadcrumbItemConverter : TypeConverter
	{
		#region TypeConverter overrides
		/********************************************************************/
		/// <summary>
		/// We can convert to an InstanceDescriptor
		/// </summary>
		/********************************************************************/
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(InstanceDescriptor))
				return true;

			return base.CanConvertTo(context, destinationType);
		}



		/********************************************************************/
		/// <summary>
		/// Return the constructor to use to create the item
		/// </summary>
		/********************************************************************/
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((destinationType == typeof(InstanceDescriptor)) && (value is NostalgicBreadcrumbItem item))
			{
				if (item.Enabled)
					return new InstanceDescriptor(typeof(NostalgicBreadcrumbItem).GetConstructor([ typeof(string) ]), new object[] { item.Text }, true);

				return new InstanceDescriptor(typeof(NostalgicBreadcrumbItem).GetConstructor([ typeof(string), typeof(bool) ]), new object[] { item.Text, item.Enabled }, true);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
		#endregion
	}
}
