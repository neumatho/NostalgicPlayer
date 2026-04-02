/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Initialize controls with dependency injection
	/// </summary>
	public class ControlInitializerService : IControlInitializerService
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ControlInitializerService(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all the controls in the given form
		/// </summary>
		/********************************************************************/
		public void InitializeControls(Form form)
		{
			CallComponentInitializeMethod(form);

			InitializeControls(form.Controls);
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
			if (control is IDependencyInjectionControl diControl)
				CallControlInitializeMethod(diControl);

			InitializeControls(control.Controls);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Check to see if the form has a component collection. If so,
		/// call initialize on them
		/// </summary>
		/********************************************************************/
		private void CallComponentInitializeMethod(Control control)
		{
			FieldInfo field = control.GetType().GetField("components", BindingFlags.NonPublic | BindingFlags.Instance);

			if (field != null)
			{
				if (field.GetValue(control) is IContainer container)
				{
					foreach (Component component in container.Components)
					{
						if (component is IDependencyInjectionControl diComponent)
						{
							MethodInfo initializeMethod = diComponent.GetType().GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance);

							if (initializeMethod != null)
							{
								ParameterInfo[] parameters = initializeMethod.GetParameters();
								object[] arguments = parameters.Select(p => applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

								initializeMethod.Invoke(diComponent, arguments);
							}
						}
					}
				}
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
				object[] arguments = parameters.Select(p => applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

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

				CallComponentInitializeMethod(control);

				result.AddRange(FindDependencyInjectionControls(control.Controls));
			}

			return result;
		}
		#endregion
	}
}
