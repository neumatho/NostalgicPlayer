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
	/// Helper to instance Windows Form objects
	/// </summary>
	public class FormCreatorService : IFormCreatorService
	{
		private readonly IApplicationContext _applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FormCreatorService(IApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Will create a new instance of the window type given and call
		/// InitializeForm() on it while resolving dependencies
		/// </summary>
		/********************************************************************/
		public T GetFormInstance<T>() where T : Form, new()
		{
			T form = new T();

			CallInitializeMethod(form, "InitializeBaseForm");
			CallInitializeMethod(form, "InitializeForm");

			foreach (IDependencyInjectionControl diControl in FindDependencyInjectionControls(form.Controls))
				CallControlInitializeMethod(diControl);

			return form;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a single control with dependency injections
		/// </summary>
		/********************************************************************/
		public void InitializeControl(Control control)
		{
			if (control is IDependencyInjectionControl)
				CallControlInitializeMethod(control as IDependencyInjectionControl);

			foreach (IDependencyInjectionControl diControl in FindDependencyInjectionControls(control.Controls))
				CallControlInitializeMethod(diControl);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Try to find the given method and call it
		/// </summary>
		/********************************************************************/
		private void CallInitializeMethod(Form form, string methodName)
		{
			MethodInfo initializeMethod = form.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

			if (initializeMethod != null)
			{
				ParameterInfo[] parameters = initializeMethod.GetParameters();
				object[] arguments = parameters.Select(p => _applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

				initializeMethod.Invoke(form, arguments);
			}
		}



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
