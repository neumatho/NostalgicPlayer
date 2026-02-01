/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Threading;

namespace Polycode.NostalgicPlayer.Kit.C
{
	/// <summary>
	/// C like environment methods
	/// </summary>
	public static class CEnvironment
	{
		// Environment variables (thread-safe)
		private static readonly Dictionary<string, string> environmentVariables = new Dictionary<string, string>();
		private static readonly Lock envLock = new Lock();

		/********************************************************************/
		/// <summary>
		/// Set environment variable (C-style putenv). Format: "NAME=VALUE".
		/// Returns 0 on success, -1 on error
		/// </summary>
		/********************************************************************/
		public static c_int putenv(string str)
		{
			if (string.IsNullOrEmpty(str))
				return -1;

			c_int equalsIndex = str.IndexOf('=');

			if (equalsIndex <= 0)
				return -1;

			string name = str.Substring(0, equalsIndex);
			string value = str.Substring(equalsIndex + 1);

			lock (envLock)
			{
				environmentVariables[name] = value;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Set environment variable (C-style putenv). Format: "NAME=VALUE".
		/// Returns 0 on success, -1 on error
		/// </summary>
		/********************************************************************/
		public static c_int putenv(CPointer<char> str)
		{
			if (str.IsNull)
				return -1;

			return putenv(str.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Get environment variable value. Returns null if not found
		/// </summary>
		/********************************************************************/
		public static CPointer<char> getenv(string name)
		{
			lock (envLock)
			{
				return environmentVariables.TryGetValue(name, out string value) ? value.ToCharPointer() : null;
			}
		}
	}
}
