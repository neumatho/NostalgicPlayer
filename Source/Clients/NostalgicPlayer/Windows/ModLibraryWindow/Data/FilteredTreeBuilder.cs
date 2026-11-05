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
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Events;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Data
{
	/// <summary>
	/// Builds filtered tree in background with cancellation support
	/// </summary>
	public abstract class FilteredTreeBuilder
	{
		private readonly string filter;
		/// <summary></summary>
		protected readonly SearchMode searchMode;

		private readonly SynchronizationContext syncContext;

		/// <summary></summary>
		protected bool cancelled;

		/// <summary>
		/// Cache for fast node lookup during tree building
		/// </summary>
		protected readonly Dictionary<string, TreeNode> nodeCache = new Dictionary<string, TreeNode>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FilteredTreeBuilder(string searchText, SearchMode searchMode)
		{
			filter = searchText;
			this.searchMode = searchMode;
			syncContext = SynchronizationContext.Current;
		}



		/********************************************************************/
		/// <summary>
		/// Event fired when tree building is completed
		/// </summary>
		/********************************************************************/
		public event EventHandler<TreeBuildCompletedEventArgs> Completed;



		/********************************************************************/
		/// <summary>
		/// Start building the tree in background
		/// </summary>
		/********************************************************************/
		public void Start()
		{
			Task.Run(BuildTree);
		}



		/********************************************************************/
		/// <summary>
		/// Cancel the build task
		/// </summary>
		/********************************************************************/
		public void Cancel()
		{
			cancelled = true;
			Completed = null; // Remove all event handlers
		}



		/********************************************************************/
		/// <summary>
		/// Create tree from loaded data
		/// </summary>
		/********************************************************************/
		protected abstract void CreateTree(TreeNode root, bool showAll, Regex filterRegex);

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Build filtered tree
		/// </summary>
		/********************************************************************/
		private void BuildTree()
		{
			try
			{
				// Clear cache from previous builds
				nodeCache.Clear();

				TreeNode root = new TreeNode { Name = "Root", FullPath = string.Empty, IsDirectory = true };

				bool showAll = string.IsNullOrEmpty(filter);
				Regex filterRegex = showAll ? null : ConvertWildcardToRegex(filter);

				CreateTree(root, showAll, filterRegex);

				// Fire event with tree (no DisplayCache!)
				NotifyCompletion(root);
			}
			catch (Exception)
			{
				// Silently ignore errors if cancelled
				if (!cancelled)
					throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert wildcard pattern to regex pattern
		/// </summary>
		/********************************************************************/
		private Regex ConvertWildcardToRegex(string pattern)
		{
			// Check if pattern contains wildcards
			bool hasWildcards = pattern.Contains('*') || pattern.Contains('?');

			// If no wildcards, auto-add * around the pattern
			if (!hasWildcards)
				pattern = "*" + pattern + "*";

			// Escape regex special characters except * and ?
			string regexPattern = Regex.Escape(pattern);

			// Convert wildcards to regex
			regexPattern = regexPattern.Replace("\\*", ".*"); // * -> .* (zero or more chars)
			regexPattern = regexPattern.Replace("\\?", "."); // ? -> . (exactly one char)

			// Anchor pattern
			regexPattern = "^" + regexPattern + "$";

			return new Regex(regexPattern, RegexOptions.IgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Notify completion and fire event
		/// </summary>
		/********************************************************************/
		private void NotifyCompletion(TreeNode root)
		{
			if (!cancelled && (Completed != null))
			{
				TreeBuildCompletedEventArgs args = new TreeBuildCompletedEventArgs(root);

				if (syncContext != null)
					syncContext.Post(_ => Completed?.Invoke(this, args), null);
				else
					Completed?.Invoke(this, args);
			}
		}
		#endregion
	}
}
