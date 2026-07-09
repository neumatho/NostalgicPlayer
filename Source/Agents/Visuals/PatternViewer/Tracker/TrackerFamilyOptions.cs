//---------------------------------------------------------------------------------------
// <copyright file="TrackerFamilyOptions.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker
{
	/// <summary>
	/// Defines options for a tracker family (shown in the tracker's submenu)
	/// </summary>
	internal class TrackerFamilyOptionDefinition
	{
		/// <summary>
		/// Unique identifier for this option within the family
		/// </summary>
		public required string Id
		{
			get;
			init;
		}

		/// <summary>
		/// Display name shown in the menu
		/// </summary>
		public required string DisplayName
		{
			get;
			init;
		}

		/// <summary>
		/// Default value for this option
		/// </summary>
		public bool DefaultValue
		{
			get;
			init;
		}
	}

	/// <summary>
	/// Registered tracker family info
	/// </summary>
	internal class TrackerFamilyRegistration
	{
		public required string FamilyId
		{
			get;
			init;
		}

		public required string CategoryPath
		{
			get;
			init;
		}

		public List<TrackerFamilyOptionDefinition> Options
		{
			get;
		} = new();
	}

	/// <summary>
	/// Registry for tracker families and their options
	/// </summary>
	internal static class TrackerFamilyOptions
	{
		// Keyed by categoryPath for menu lookup
		private static readonly Dictionary<string, TrackerFamilyRegistration> familiesByCategory = new();

		// Keyed by familyId for option addition
		private static readonly Dictionary<string, TrackerFamilyRegistration> familiesById = new();

		/********************************************************************/
		/// <summary>
		/// Register a tracker family
		/// </summary>
		/// <param name="categoryPath">The category path for menu placement (e.g., "DOS\FastTracker II")</param>
		/// <param name="familyId">Unique identifier for settings storage</param>
		/********************************************************************/
		public static void RegisterFamily(string categoryPath, string familyId)
		{
			TrackerFamilyRegistration registration = new() {FamilyId = familyId, CategoryPath = categoryPath};

			familiesByCategory[categoryPath] = registration;
			familiesById[familyId] = registration;
		}

		/********************************************************************/
		/// <summary>
		/// Add an option to a registered tracker family
		/// </summary>
		/// <param name="familyId">The family identifier</param>
		/// <param name="option">The option definition</param>
		/********************************************************************/
		public static void AddOption(string familyId, TrackerFamilyOptionDefinition option)
		{
			if (familiesById.TryGetValue(familyId, out TrackerFamilyRegistration registration))
			{
				registration.Options.Add(option);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Get family registration for a category path
		/// </summary>
		/********************************************************************/
		public static TrackerFamilyRegistration GetFamily(string categoryPath)
		{
			familiesByCategory.TryGetValue(categoryPath, out TrackerFamilyRegistration registration);
			return registration;
		}


		/********************************************************************/
		/// <summary>
		/// Clear all registrations (for testing)
		/// </summary>
		/********************************************************************/
		public static void Clear()
		{
			familiesByCategory.Clear();
			familiesById.Clear();
		}
	}
}
