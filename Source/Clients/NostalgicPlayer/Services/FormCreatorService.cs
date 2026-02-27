/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
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

			return form;
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
		#endregion
	}
}
