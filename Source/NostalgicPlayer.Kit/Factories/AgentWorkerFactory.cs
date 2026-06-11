/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Factories
{
	/// <summary>
	/// Can create instances needed in agents which use dependency injections
	/// </summary>
	internal class AgentWorkerFactory : IAgentWorkerFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AgentWorkerFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new worker instance
		/// </summary>
		/********************************************************************/
		public T GetWorkerInstance<T>(Guid typeId, params object[] extraArguments) where T : IAgentWorker
		{
			ConstructorInfo ctor = typeof(T).GetConstructors().Single();
			ParameterInfo[] parameters = ctor.GetParameters();

			if (parameters.Length == 0)
				return Activator.CreateInstance<T>();

			List<object> arguments = new List<object>();

			if (parameters[0].ParameterType == typeof(Guid))
				arguments.Add(typeId);

			if (extraArguments.Length > 0)
				arguments.AddRange(extraArguments);

			arguments.AddRange(parameters
				.Skip(arguments.Count)
				.Select(x => applicationContext.Container.GetInstance(x.ParameterType)));

			return (T)ctor.Invoke(arguments.ToArray());
		}
	}
}
