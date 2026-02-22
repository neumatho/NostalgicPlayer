/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Logic.Databases
{
	/// <summary>
	/// 
	/// </summary>
	public interface IModuleDatabase
	{
		/// <summary>
		/// Is called when cleanup is done
		/// </summary>
		delegate void CleanupDoneHandler();

		/// <summary>
		/// Return a new object if you want to change it or null to leave
		/// the original in place
		/// </summary>
		delegate ModuleDatabaseInfo ActionHandler(string fullPath, ModuleDatabaseInfo moduleDatabaseInfo);

		/// <summary>
		/// Retrieve all module information
		/// </summary>
		IEnumerable<KeyValuePair<string, ModuleDatabaseInfo>> RetrieveAllInformation();

		/// <summary>
		/// Retrieve module information
		/// </summary>
		ModuleDatabaseInfo RetrieveInformation(string fullPath);

		/// <summary>
		/// Store module information
		/// </summary>
		void StoreInformation(string fullPath, ModuleDatabaseInfo info);

		/// <summary>
		/// Start the cleanup job
		/// </summary>
		void StartCleanup(CleanupDoneHandler handler);

		/// <summary>
		/// Delete the database from disk
		/// </summary>
		void DeleteDatabase();

		/// <summary>
		/// Save the database to disk
		/// </summary>
		void SaveDatabase();

		/// <summary>
		/// Do some action on all items in the database
		/// </summary>
		void RunAction(ActionHandler handler);
	}
}
