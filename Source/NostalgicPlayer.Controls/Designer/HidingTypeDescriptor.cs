/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class HidingTypeDescriptor : CustomTypeDescriptor
	{
		private readonly string[] hiddenProperties;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HidingTypeDescriptor(ICustomTypeDescriptor parent, string[] propertiesToHide) : base(parent)
		{
			hiddenProperties = propertiesToHide;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override PropertyDescriptorCollection GetProperties()
		{
			return GetProperties(null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection props = base.GetProperties(attributes);
			List<PropertyDescriptor> kept = new List<PropertyDescriptor>(props.Count);

			foreach (PropertyDescriptor pd in props)
			{
				if (hiddenProperties.Contains(pd.Name))
					continue;

				kept.Add(pd);
			}

			return new PropertyDescriptorCollection(kept.ToArray(), true);
		}
	}
}
