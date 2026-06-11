/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Gui.Factories
{
	/// <summary>
	/// Can create instances needed in agents which
	/// use dependency injections
	/// </summary>
	internal class ControlFactory : IControlFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ControlFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new control instance. Will call the InitializeControl
		/// method if exists with dependency injections
		/// </summary>
		/********************************************************************/
		public T GetInstance<T>(params object[] extraArguments) where T : Control, IControl, new()
		{
			T control = new T();

			CallInitializeMethod(control, "InitializeControl", extraArguments);

			return control;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Try to find the given method and call it
		/// </summary>
		/********************************************************************/
		private void CallInitializeMethod(Control control, string methodName, params object[] extraArguments)
		{
			MethodInfo initializeMethod = control.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

			if (initializeMethod != null)
			{
				ParameterInfo[] parameters = initializeMethod.GetParameters();
				object[] arguments = parameters.Skip(extraArguments.Length).Select(p => applicationContext.Container.GetInstance(p.ParameterType)).ToArray();

				initializeMethod.Invoke(control, extraArguments.Union(arguments).ToArray());
			}
		}
		#endregion
	}
}
