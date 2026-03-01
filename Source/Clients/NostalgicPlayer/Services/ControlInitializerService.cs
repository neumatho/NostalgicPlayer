/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Initialize controls with dependency injection
	/// </summary>
	public class ControlInitializerService : IControlInitializerService
	{
		private readonly IApplicationContext _applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ControlInitializerService(IApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a collection of controls with dependency injections
		/// </summary>
		/********************************************************************/
		public void InitializeControls(Control.ControlCollection controls)
		{
			foreach (IDependencyInjectionControl diControl in FindDependencyInjectionControls(controls))
				CallControlInitializeMethod(diControl);
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a single control and its child controls with
		/// dependency injections
		/// </summary>
		/********************************************************************/
		public void InitializeSingleControl(Control control)
		{
			if (control is IDependencyInjectionControl)
				CallControlInitializeMethod(control as IDependencyInjectionControl);

			InitializeControls(control.Controls);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Try to find the initialize method and call it
		/// </summary>
		/********************************************************************/
		private void CallControlInitializeMethod(IDependencyInjectionControl control)
		{
			MethodInfo initializeMethod = control.GetType().GetMethod("InitializeControl", BindingFlags.Public | BindingFlags.Instance);

			if (initializeMethod != null)
			{
				ParameterInfo[] parameters = initializeMethod.GetParameters();
				object[] arguments = parameters.Select(p => _applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

				initializeMethod.Invoke(control, arguments);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return controls using dependency injections
		/// </summary>
		/********************************************************************/
		private IEnumerable<IDependencyInjectionControl> FindDependencyInjectionControls(Control.ControlCollection controls)
		{
			List<IDependencyInjectionControl> result = new List<IDependencyInjectionControl>();

			foreach (Control control in controls)
			{
				if (control is IDependencyInjectionControl diControl)
					result.Add(diControl);

				result.AddRange(FindDependencyInjectionControls(control.Controls));
			}

			return result;
		}
		#endregion
	}
}
