//---------------------------------------------------------------------------------------
// <copyright file="TrackerStyleRegistration.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker
{
	/// <summary>
	/// Registration information for a tracker style
	/// </summary>
	internal class TrackerStyleRegistration
	{
		/// <summary>
		/// Unique identifier for this style (e.g., "ProTracker.ProTracker23")
		/// </summary>
		public required string Id
		{
			get;
			init;
		}

		/// <summary>
		/// Display name shown in the menu (e.g., "ProTracker 2.3")
		/// </summary>
		public required string DisplayName
		{
			get;
			init;
		}

		/// <summary>
		/// Menu path for hierarchical grouping. Use backslash to create nested menus.
		/// The first part should be a TrackerCategories constant, followed by optional sub-menus.
		/// Example: "Amiga" creates Amiga > DisplayName
		/// Example: "Amiga\ProTracker" creates Amiga > ProTracker > DisplayName
		/// Example: "DOS\FastTracker 1" creates DOS > FastTracker 1 > DisplayName
		/// </summary>
		public required string Category
		{
			get;
			init;
		}

		/// <summary>
		/// Whether to enable grid drawing when switching to this style
		/// </summary>
		public bool DrawGrid
		{
			get;
			init;
		}

		/// <summary>
		/// The renderer type for this style (used to check if renderer needs to be recreated)
		/// </summary>
		public required Type RendererType
		{
			get;
			init;
		}

		/// <summary>
		/// Factory function to create a new renderer instance
		/// </summary>
		public required Func<TrackerPainterBase> CreateRenderer
		{
			get;
			init;
		}

		/// <summary>
		/// Optional initialization action called after renderer is created or reused
		/// </summary>
		public Action<TrackerPainterBase> Initialize
		{
			get;
			init;
		}

		/// <summary>
		/// VU-Meter to use for this style (Id in the VuMeterRegistry).
		/// null/empty = no VU rendered.
		/// </summary>
		public string VuMeterId
		{
			get;
			init;
		}
	}
}
