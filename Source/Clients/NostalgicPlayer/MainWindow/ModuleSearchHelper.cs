/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow
{
	/// <summary>
	/// Helper class for searching modules in the module list
	/// </summary>
	internal static class ModuleSearchHelper
	{
		private const int MaxResults = 200;

		/********************************************************************/
		/// <summary>
		/// Search modules based on search text
		/// </summary>
		/********************************************************************/
		public static async Task<ModuleListItem[]> SearchAsync(IEnumerable<ModuleListItem> items, string searchText,
			CancellationToken cancellationToken)
		{
			if (string.IsNullOrEmpty(searchText))
				return Array.Empty<ModuleListItem>();

			// Perform search in background thread
			return await Task.Run(() =>
			{
				// Check if user entered wildcards
				bool hasWildcards = searchText.Contains('*') || searchText.Contains('?');
				string pattern;

				if (hasWildcards)
					// Convert wildcard pattern to regex
					pattern = "^" + Regex.Escape(searchText).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				else
					// Auto-add wildcards: *text*
					pattern = Regex.Escape(searchText);

				// Filter module list
				List<ModuleListItem> filteredModules = new();
				Regex regex = new(pattern, RegexOptions.IgnoreCase);

				foreach (var item in items)
				{
					// Check for cancellation
					if (cancellationToken.IsCancellationRequested)
						break;

					if (hasWildcards)
					{
						if (regex.IsMatch(item.ListItem.DisplayName))
						{
							filteredModules.Add(item);

							// Limit results
							if (filteredModules.Count >= MaxResults)
								break;
						}
					}
					else
					{
						if (item.ListItem.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							filteredModules.Add(item);

							// Limit results
							if (filteredModules.Count >= MaxResults)
								break;
						}
					}
				}

				return filteredModules.ToArray();
			}, cancellationToken);
		}
	}
}