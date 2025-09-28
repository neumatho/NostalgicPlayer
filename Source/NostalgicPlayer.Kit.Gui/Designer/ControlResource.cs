/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Kit.Gui.Designer
{
	/// <summary>
	/// This component makes it possible to use resources directly in the designer
	/// </summary>
	[ProvideProperty("ResourceKey", typeof(Control))]
	public class ControlResource : Component, IExtenderProvider, ISupportInitialize
	{
		private readonly Dictionary<Control, string> controls;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ControlResource()
		{
			controls = new Dictionary<Control, string>();
		}

		#region Designer properties
		/********************************************************************/
		/// <summary>
		/// Full namespace to the resource class
		/// </summary>
		/********************************************************************/
		[Description("Full name of resource class, like YourAppNamespace.Resource1")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string ResourceClassName
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the resource key
		/// </summary>
		/********************************************************************/
		public string GetResourceKey(Control control)
		{
			if (controls.TryGetValue(control, out string value))
				return value;

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the resource key
		/// </summary>
		/********************************************************************/
		[DefaultValue(null)]
		public void SetResourceKey(Control control, string key)
		{
			if (string.IsNullOrEmpty(key))
				controls.Remove(control);
			else
				controls[control] = key;
		}



		/********************************************************************/
		/// <summary>
		/// Test if the property should be serialized
		/// </summary>
		/********************************************************************/
		public bool ShouldSerializeResourceKey(Control control)
		{
			return GetResourceKey(control) != null;
		}



		/********************************************************************/
		/// <summary>
		/// Possibility to reset the property
		/// </summary>
		/********************************************************************/
		public void ResetResourceKey(Control control)
		{
			SetResourceKey(control, null);
		}
		#endregion

		#region IExtenderProvider implementation
		/********************************************************************/
		/// <summary>
		/// Tells which object this class can extend
		/// </summary>
		/********************************************************************/
		public bool CanExtend(object extendee)
		{
			return extendee is Control;
		}
		#endregion

		#region ISupportInitialize implementation
		/********************************************************************/
		/// <summary>
		/// Is called when initialization begins
		/// </summary>
		/********************************************************************/
		public void BeginInit()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Is called when initialization is done
		/// </summary>
		/********************************************************************/
		public void EndInit()
		{
//			if (!DesignMode)
			{
				if (controls.Count > 0)
				{
					// Find assembly where the resource exists
					Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetType(ResourceClassName) != null);
					if (assembly != null)
					{
						ResourceManager manager = new ResourceManager(ResourceClassName, assembly);

						foreach (KeyValuePair<Control, string> pair in controls)
							pair.Key.Text = manager.GetString(pair.Value);
					}
				}
			}
		}
		#endregion
	}
}
