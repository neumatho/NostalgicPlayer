//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.Options.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// ModernTrackerPainter - Tracker-specific options
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		private const string ColoredOptionId = "Colored";

		// Static reference to PatternViewerSettings for option persistence
		private static PatternViewerSettings viewerSettings;

		/********************************************************************/
		/// <summary>
		/// Set the viewer settings reference (called once during initialization)
		/// </summary>
		/********************************************************************/
		public static void SetViewerSettings(PatternViewerSettings settings)
		{
			viewerSettings = settings;
		}

		/********************************************************************/
		/// <summary>
		/// Get the current Colored setting
		/// </summary>
		/********************************************************************/
		public static bool GetColoredSetting()
		{
			return viewerSettings?.GetTrackerOption(FamilyId, ColoredOptionId, true) ?? true;
		}

		/********************************************************************/
		/// <summary>
		/// Initialize with a style and apply current options
		/// </summary>
		/********************************************************************/
		internal TrackerStyleColors InitializeWithStyle(string styleId)
		{
			CurrentColorMode = GetColoredSetting() ? ColorMode.Colored : ColorMode.Mono;
			return UpdateStyle(styleId);
		}
	}
}
