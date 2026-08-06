//---------------------------------------------------------------------------------------
// <copyright file="VuMeterRegistration.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	internal class VuMeterRegistration
	{
		/// <summary>
		/// Unique ID for this VU-Meter (Family.Variant, e.g. "ProTracker.ProTracker36"). Used by trackers to point at it.
		/// </summary>
		public required string Id
		{
			get;
			init;
		}

		public required string DisplayName
		{
			get;
			init;
		}

		/// <summary>
		/// Category for menu grouping (null = root level)
		/// </summary>
		public string Category
		{
			get;
			init;
		}

		public int CategorySortOrder
		{
			get;
			init;
		}

		public int SortOrder
		{
			get;
			init;
		}

		public required IVuMeterStyleRenderer Renderer
		{
			get;
			init;
		}
	}
}
