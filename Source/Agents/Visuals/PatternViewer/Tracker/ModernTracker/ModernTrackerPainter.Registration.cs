//---------------------------------------------------------------------------------------
// <copyright file="ModernTrackerPainter.Registration.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Drawing;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker.ModernTracker
{
	/// <summary>
	/// Registration of Standard tracker styles
	/// </summary>
	internal partial class ModernTrackerPainter
	{
		public const string FamilyId = "System";
		public const string StyleIdLight = "System.Light";
		public const string StyleIdDark = "System.Dark";

		public const string VuMeterIdLight = "System.Light";
		public const string VuMeterIdDark = "System.Dark";

		/********************************************************************/
		/// <summary>
		/// Register all Standard tracker styles with the TrackerRegistry
		/// </summary>
		/********************************************************************/
		[ModuleInitializer]
		internal static void Register()
		{
			// Pick up host-provided settings as soon as they are ready
			TrackerRegistry.SettingsReady += SetViewerSettings;

			// Claim the global default style fallback (first claimant wins)
			TrackerRegistry.DefaultStyleId ??= StyleIdLight;

			// Register tracker family and options
			TrackerFamilyOptions.RegisterFamily(TrackerCategories.System, FamilyId);
			TrackerFamilyOptions.AddOption(FamilyId, new TrackerFamilyOptionDefinition {Id = "Colored", DisplayName = "Colored", DefaultValue = true});

			// System.Light
			TrackerRegistry.Register(new TrackerStyleRegistration
			{
				Id = StyleIdLight,
				DisplayName = "Light",
				Category = TrackerCategories.System,
				DrawGrid = true,
				RendererType = typeof(ModernTrackerPainter),
				CreateRenderer = () => new ModernTrackerPainter(),
				Initialize = r =>
				{
					ModernTrackerPainter mr = (ModernTrackerPainter)r;
					mr.InitializeWithStyle(StyleIdLight);
				},
				VuMeterId = VuMeterIdLight
			});

			// System.Dark
			TrackerRegistry.Register(new TrackerStyleRegistration
			{
				Id = StyleIdDark,
				DisplayName = "Dark",
				Category = TrackerCategories.System,
				DrawGrid = true,
				RendererType = typeof(ModernTrackerPainter),
				CreateRenderer = () => new ModernTrackerPainter(),
				Initialize = r =>
				{
					ModernTrackerPainter mr = (ModernTrackerPainter)r;
					mr.InitializeWithStyle(StyleIdDark);
				},
				VuMeterId = VuMeterIdDark
			});

			// Register ControlBar handlers
			TrackerRegistry.RegisterControlBarHandlers(
				StyleIdLight,
				ModernTrackerControlBarRenderer.GetCachedRects,
				ModernTrackerControlBarRenderer.HandleClick);

			TrackerRegistry.RegisterControlBarHandlers(
				StyleIdDark,
				ModernTrackerControlBarRenderer.GetCachedRects,
				ModernTrackerControlBarRenderer.HandleClick);

			VuMeterRegistry.Register(new VuMeterRegistration
			{
				Id = VuMeterIdLight,
				DisplayName = "Light",
				Category = "System",
				CategorySortOrder = 0,
				SortOrder = 0,
				Renderer = new StandardVuMeterRenderer(Color.White)
			});

			VuMeterRegistry.Register(new VuMeterRegistration
			{
				Id = VuMeterIdDark,
				DisplayName = "Dark",
				Category = "System",
				CategorySortOrder = 0,
				SortOrder = 1,
				Renderer = new StandardVuMeterRenderer(Color.Black)
			});
		}
	}
}
