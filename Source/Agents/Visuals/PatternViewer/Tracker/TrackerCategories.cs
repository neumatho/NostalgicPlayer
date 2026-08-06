//---------------------------------------------------------------------------------------
// <copyright file="TrackerCategories.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker
{
	/// <summary>
	/// Category constants for tracker styles
	/// </summary>
	public static class TrackerCategories
	{
		/// <summary>
		/// System/generic tracker styles
		/// </summary>
		public const string System = "System";

		/// <summary>
		/// Amiga tracker styles
		/// </summary>
		public const string Amiga = "Amiga";

		/// <summary>
		/// DOS tracker styles
		/// </summary>
		public const string DOS = "DOS";

		/// <summary>
		/// Next generation tracker styles
		/// </summary>
		public const string NextGen = "NextGen";

		/// <summary>
		/// Category sort orders (lower = higher in menu)
		/// </summary>
		private static readonly Dictionary<string, int> SortOrders = new() {{System, 0}, {Amiga, 10}, {DOS, 20}, {NextGen, 30}};

		/// <summary>
		/// Get the sort order for a category
		/// </summary>
		public static int GetSortOrder(string category)
		{
			return SortOrders.TryGetValue(category, out int order) ? order : 999;
		}
	}
}
