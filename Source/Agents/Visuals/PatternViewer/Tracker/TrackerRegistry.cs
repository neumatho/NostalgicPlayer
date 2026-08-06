//---------------------------------------------------------------------------------------
// <copyright file="TrackerRegistry.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.ControlBar;

// PatternViewerSettings lives in the outer namespace.

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker
{
	/// <summary>
	/// ControlBar handler delegates for a TrackerStyle
	/// </summary>
	internal class ControlBarHandlers
	{
		public Func<ControlBarRects> GetRects
		{
			get;
			init;
		}

		public Func<Point, ControlBarState, ControlBarAction> HandleClick
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Central registry for all tracker styles
	/// </summary>
	internal static class TrackerRegistry
	{
		private static readonly Dictionary<string, TrackerStyleRegistration> registrations = new();
		private static readonly Dictionary<string, int> categorySortOrder = new();
		private static readonly Dictionary<string, ControlBarHandlers> controlBarHandlers = new();

		/// <summary>
		/// Style ID used as fallback when the persisted style cannot be resolved. A tracker can
		/// claim this slot during its [ModuleInitializer] Register() (first claim wins).
		/// </summary>
		public static string DefaultStyleId
		{
			get;
			set;
		}

		/// <summary>
		/// Raised once when the host has loaded the PatternViewerSettings. Trackers that need to
		/// persist family/style options subscribe here in their [ModuleInitializer] Register().
		/// </summary>
		public static event Action<PatternViewerSettings> SettingsReady;

		/********************************************************************/
		/// <summary>
		/// Called by the host once after PatternViewerSettings is available.
		/// </summary>
		/********************************************************************/
		public static void NotifySettingsReady(PatternViewerSettings settings)
		{
			SettingsReady?.Invoke(settings);
		}

		/********************************************************************/
		/// <summary>
		/// Register a tracker style
		/// </summary>
		/********************************************************************/
		public static void Register(TrackerStyleRegistration registration)
		{
			registrations[registration.Id] = registration;

			// Track category sort order based on root category (first part of path)
			string rootCategory = GetRootCategory(registration.Category);
			if (!categorySortOrder.ContainsKey(rootCategory))
			{
				categorySortOrder[rootCategory] = TrackerCategories.GetSortOrder(rootCategory);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get the root category (first part) from a category path
		/// </summary>
		/********************************************************************/
		public static string GetRootCategory(string categoryPath)
		{
			if (string.IsNullOrEmpty(categoryPath))
			{
				return string.Empty;
			}

			int separatorIndex = categoryPath.IndexOf('\\');
			return separatorIndex >= 0 ? categoryPath.Substring(0, separatorIndex) : categoryPath;
		}

		/********************************************************************/
		/// <summary>
		/// Get a tracker style registration by ID
		/// </summary>
		/********************************************************************/
		public static TrackerStyleRegistration Get(string id)
		{
			return registrations.TryGetValue(id, out TrackerStyleRegistration reg) ? reg : null;
		}

		/********************************************************************/
		/// <summary>
		/// Get all registered tracker styles
		/// </summary>
		/********************************************************************/
		public static IEnumerable<TrackerStyleRegistration> GetAll()
		{
			return registrations.Values;
		}

		/********************************************************************/
		/// <summary>
		/// Get all tracker styles for a specific category path, sorted alphabetically by DisplayName
		/// </summary>
		/********************************************************************/
		public static IEnumerable<TrackerStyleRegistration> GetByCategory(string category)
		{
			return registrations.Values
				.Where(r => r.Category == category)
				.OrderBy(r => r.DisplayName);
		}

		/********************************************************************/
		/// <summary>
		/// Get all root categories (first part of category paths), sorted by CategorySortOrder
		/// </summary>
		/********************************************************************/
		public static IEnumerable<string> GetRootCategories()
		{
			return categorySortOrder
				.OrderBy(kvp => kvp.Value)
				.Select(kvp => kvp.Key);
		}

		/********************************************************************/
		/// <summary>
		/// Get all unique category paths that start with the given prefix, returning only the next level.
		/// For example, if prefix is "Amiga" and categories are "Amiga", "Amiga\ProTracker", "Amiga\OctaMED",
		/// this returns ["Amiga", "Amiga\ProTracker", "Amiga\OctaMED"] (direct children only).
		/// </summary>
		/********************************************************************/
		public static IEnumerable<string> GetSubCategories(string prefix)
		{
			HashSet<string> result = new();
			string prefixWithSeparator = string.IsNullOrEmpty(prefix) ? "" : prefix + "\\";

			foreach (TrackerStyleRegistration reg in registrations.Values)
			{
				string category = reg.Category;

				// If category equals prefix exactly, add it
				if (category == prefix)
				{
					result.Add(category);
					continue;
				}

				// If category starts with prefix\, extract the next level
				if (!string.IsNullOrEmpty(prefix) && category.StartsWith(prefixWithSeparator))
				{
					string remainder = category.Substring(prefixWithSeparator.Length);
					int nextSeparator = remainder.IndexOf('\\');
					if (nextSeparator >= 0)
					{
						// There are more levels, add up to next level
						result.Add(prefix + "\\" + remainder.Substring(0, nextSeparator));
					}
					else
					{
						// This is the last level
						result.Add(category);
					}
				}
			}

			return result.OrderBy(c => c);
		}

		/********************************************************************/
		/// <summary>
		/// Get all tracker styles that have the exact category path
		/// </summary>
		/********************************************************************/
		public static IEnumerable<TrackerStyleRegistration> GetStylesAtPath(string categoryPath)
		{
			return registrations.Values
				.Where(r => r.Category == categoryPath)
				.OrderBy(r => r.DisplayName);
		}

		/********************************************************************/
		/// <summary>
		/// Check if there are any styles with categories that start with the given prefix (excluding exact matches)
		/// </summary>
		/********************************************************************/
		public static bool HasSubCategories(string categoryPath)
		{
			string prefixWithSeparator = categoryPath + "\\";
			return registrations.Values.Any(r => r.Category.StartsWith(prefixWithSeparator));
		}

		/********************************************************************/
		/// <summary>
		/// Get all styles in menu order (sorted by category path, then by display name)
		/// </summary>
		/********************************************************************/
		public static IEnumerable<TrackerStyleRegistration> GetAllInMenuOrder()
		{
			return registrations.Values
				.OrderBy(r => categorySortOrder.TryGetValue(GetRootCategory(r.Category), out int order) ? order : 999)
				.ThenBy(r => r.Category)
				.ThenBy(r => r.DisplayName);
		}

		/********************************************************************/
		/// <summary>
		/// Clear all registrations (for testing)
		/// </summary>
		/********************************************************************/
		public static void Clear()
		{
			registrations.Clear();
			categorySortOrder.Clear();
			controlBarHandlers.Clear();
		}

		/********************************************************************/
		/// <summary>
		/// Register ControlBar handlers for a style ID
		/// </summary>
		/********************************************************************/
		public static void RegisterControlBarHandlers(string styleId,
			Func<ControlBarRects> getRects,
			Func<Point, ControlBarState, ControlBarAction> handleClick)
		{
			controlBarHandlers[styleId] = new ControlBarHandlers {GetRects = getRects, HandleClick = handleClick};
		}

		/********************************************************************/
		/// <summary>
		/// Get ControlBar handlers for a style ID
		/// </summary>
		/********************************************************************/
		public static ControlBarHandlers GetControlBarHandlers(string styleId)
		{
			return controlBarHandlers.TryGetValue(styleId, out ControlBarHandlers handlers) ? handlers : null;
		}
	}
}
