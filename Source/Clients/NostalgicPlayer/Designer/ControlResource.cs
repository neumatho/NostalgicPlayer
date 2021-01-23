/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Designer
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



		/********************************************************************/
		/// <summary>
		/// Full namespace to the resource class
		/// </summary>
		/********************************************************************/
		[Description("Full name of resource class, like YourAppNamespace.Resource1")]
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
		public void SetResourceKey(Control control, string key)
		{
			if (string.IsNullOrEmpty(key))
				controls.Remove(control);
			else
				controls[control] = key;
		}

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
				ResourceManager manager = new ResourceManager(ResourceClassName, GetType().Assembly);

				foreach (KeyValuePair<Control, string> pair in controls)
					pair.Key.Text = manager.GetString(pair.Value);
			}
		}
		#endregion
	}
}
