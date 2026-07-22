//---------------------------------------------------------------------------------------
// <copyright file="VuMeterRegistry.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Central registry for all VU-Meter styles
	/// </summary>
	internal static class VuMeterRegistry
	{
		private static readonly List<VuMeterRegistration> registrations = new();
		private static readonly Dictionary<string, int> categorySortOrder = new();

		/********************************************************************/
		/// <summary>
		/// Register a VU-Meter style
		/// </summary>
		/********************************************************************/
		public static void Register(VuMeterRegistration registration)
		{
			registrations.Add(registration);

			// Track category sort order (first registration wins, skip null category)
			if (registration.Category != null && !categorySortOrder.ContainsKey(registration.Category))
			{
				categorySortOrder[registration.Category] = registration.CategorySortOrder;
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get all registered VU-Meter styles
		/// </summary>
		/********************************************************************/
		public static IEnumerable<VuMeterRegistration> GetAll()
		{
			return registrations;
		}

		/********************************************************************/
		/// <summary>
		/// Get VU-Meter styles at root level (no category), sorted by SortOrder
		/// </summary>
		/********************************************************************/
		public static IEnumerable<VuMeterRegistration> GetRootLevel()
		{
			return registrations.Where(r => r.Category == null).OrderBy(r => r.SortOrder);
		}

		/********************************************************************/
		/// <summary>
		/// Get all VU-Meter styles for a specific category, sorted by SortOrder then DisplayName
		/// </summary>
		/********************************************************************/
		public static IEnumerable<VuMeterRegistration> GetByCategory(string category)
		{
			return registrations
				.Where(r => r.Category == category)
				.OrderBy(r => r.SortOrder)
				.ThenBy(r => r.DisplayName);
		}

		/********************************************************************/
		/// <summary>
		/// Get all categories that have at least one registered style, sorted by CategorySortOrder
		/// </summary>
		/********************************************************************/
		public static IEnumerable<string> GetCategories()
		{
			return categorySortOrder
				.OrderBy(kvp => kvp.Value)
				.Select(kvp => kvp.Key);
		}

		/********************************************************************/
		/// <summary>
		/// Get the renderer for a given VU-Meter Id, or null if none registered.
		/// </summary>
		/********************************************************************/
		public static IVuMeterStyleRenderer GetRenderer(string id)
		{
			return string.IsNullOrEmpty(id) ? null : registrations.FirstOrDefault(r => r.Id == id)?.Renderer;
		}

		/********************************************************************/
		/// <summary>
		/// Get the full registration for a given VU-Meter Id.
		/// </summary>
		/********************************************************************/
		public static VuMeterRegistration GetById(string id)
		{
			return string.IsNullOrEmpty(id) ? null : registrations.FirstOrDefault(r => r.Id == id);
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
		}
	}
}
