/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows;
using Polycode.NostalgicPlayer.Controls;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Services
{
	/// <summary>
	/// Helper to instance Windows Form objects
	/// </summary>
	public class FormCreatorService : IFormCreatorService
	{
		private readonly IApplicationContext applicationContext;
		private readonly IControlInitializerService controlInitializerService;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FormCreatorService(IApplicationContext applicationContext, IControlInitializerService controlInitializerService)
		{
			this.applicationContext = applicationContext;
			this.controlInitializerService = controlInitializerService;
		}



		/********************************************************************/
		/// <summary>
		/// Will create a new instance of the window type given and call
		/// InitializeForm() on it while resolving dependencies
		/// </summary>
		/********************************************************************/
		public T GetFormInstance<T>(params object[] extraArguments) where T : Form, new()
		{
			T form = new T();

			controlInitializerService.InitializeControls(form);

			CallInitializeMethod(form, "InitializeBaseForm");
			CallInitializeMethod(form, "InitializeForm", extraArguments);

			return form;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Try to find the given method and call it
		/// </summary>
		/********************************************************************/
		private void CallInitializeMethod(Form form, string methodName, params object[] extraArguments)
		{
			MethodInfo initializeMethod = form.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

			if (initializeMethod != null)
			{
				ParameterInfo[] parameters = initializeMethod.GetParameters();
				object[] arguments = parameters.Skip(extraArguments.Length).Select(p => applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

				initializeMethod.Invoke(form, extraArguments.Union(arguments).ToArray());
			}
		}
		#endregion
	}
}
