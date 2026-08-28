//---------------------------------------------------------------------------------------
// <copyright file="PatternContextMenu.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Creates and manages the pattern viewer context menu
	/// </summary>
	internal class PatternContextMenu : IDisposable
	{
		private readonly Control panel;
		private readonly PatternRenderer renderer;
		private readonly PatternViewerSettings settings;
		private readonly List<ToolStripMenuItem> trackerOptionMenuItems = new();

		// Tracker style menu items (keyed by registration ID)
		private readonly Dictionary<string, ToolStripMenuItem> trackerStyleMenuItems = new();
		private readonly Action updateStyleCallback;

		// VU meter menu items (keyed by VuMeterRegistration.Id)
		private readonly Dictionary<string, ToolStripMenuItem> vuMeterStyleMenuItems = new();


		// Display mode menu items
		private ToolStripMenuItem compactModeItem;

		// Compress patterns menu items
		private ToolStripMenuItem compressPatternsAutoItem;
		private ToolStripMenuItem compressPatternsOffItem;
		private ToolStripMenuItem compressPatternsOnItem;

		// Font Scale menu items
		private ToolStripMenuItem fontScale100Item;
		private ToolStripMenuItem fontScale125Item;
		private ToolStripMenuItem fontScale150Item;
		private ToolStripMenuItem fontScale175Item;
		private ToolStripMenuItem fontScale200Item;
		private ToolStripMenuItem fontScale300Item;
		private ToolStripMenuItem fontScale400Item;
		private ToolStripMenuItem fullModeItem;

		// Grid lines menu items
		private ToolStripMenuItem gridLinesAutoItem;
		private ToolStripMenuItem gridLinesOffItem;
		private ToolStripMenuItem gridLinesOnItem;

		// Toggle menu items
		private ToolStripMenuItem hideEmptyItem;

		// Number format menu items
		private ToolStripMenuItem instrumentAutoItem;
		private ToolStripMenuItem instrumentDecimalItem;
		private ToolStripMenuItem instrumentHexItem;
		private ToolStripMenuItem notesOnlyModeItem;
		private ToolStripMenuItem rollingPatternsItem;
		private ToolStripMenuItem rowNumbersAutoItem;
		private ToolStripMenuItem rowNumbersDecimalItem;
		private ToolStripMenuItem rowNumbersHexItem;
		private ToolStripMenuItem showControlBarItem;
#if DEBUG
		private ToolStripMenuItem showDebugInfoItem;
#endif
		private ToolStripMenuItem showTransposedNotesItem;
		private ToolStripMenuItem singleEffectModeItem;

		// Tracker-specific options
		private ToolStripMenuItem trackerStyleMenuItem;
		private ToolStripMenuItem trackPatternAutoItem;
		private ToolStripMenuItem trackPatternDecimalItem;
		private ToolStripMenuItem trackPatternHexItem;
		private ToolStripMenuItem volumeAutoItem;
		private ToolStripMenuItem volumeDecimalItem;
		private ToolStripMenuItem volumeHexItem;

		// VU meter mode menu items
		private ToolStripMenuItem volumeNoteKickItem;
		private ToolStripMenuItem volumeOffItem;
		private ToolStripMenuItem volumeRealItem;
		private ToolStripMenuItem vuMeterMatchTrackerItem;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PatternContextMenu(
			PatternRenderer renderer,
			PatternViewerSettings settings,
			Control panel,
			string initialStyleId,
			Action updateStyleCallback)
		{
			this.renderer = renderer;
			this.settings = settings;
			this.panel = panel;
			this.updateStyleCallback = updateStyleCallback;

			CurrentStyleId = initialStyleId;

			Menu = new ContextMenuStrip();
			Menu.Opening += ContextMenu_Opening;

			BuildMenu();
		}

		// The context menu strip
		public ContextMenuStrip Menu
		{
			get;
		}

		/********************************************************************/
		/// <summary>
		/// Get/set the current tracker style ID from registry
		/// </summary>
		/********************************************************************/
		public string CurrentStyleId
		{
			get;
			set;
		}

		#region IDisposable
		/********************************************************************/
		/// <summary>
		/// Dispose resources
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			Menu?.Dispose();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Build the complete menu structure
		/// </summary>
		/********************************************************************/
		private void BuildMenu()
		{
			// Tracker Style submenu
			ToolStripMenuItem trackerStyleItem = CreateTrackerStyleMenu();

			// VU Meters submenu
			ToolStripMenuItem volumeBarsItem = CreateVuMetersMenu();

			// Display Mode submenu
			ToolStripMenuItem displayModeItem = CreateDisplayModeMenu();

			// Rolling Patterns, Hide Empty, Control Bar, Transposed Notes
			rollingPatternsItem = CreateMenuItem("Rolling Patterns", () => ToggleRollingPatterns());
			hideEmptyItem = CreateMenuItem("Hide Empty", () => ToggleHideEmpty());
			showControlBarItem = CreateMenuItem("Player Control Bar", () => ToggleShowControlBar());
			showTransposedNotesItem = CreateMenuItem("Transposed Notes", () => ToggleShowTransposedNotes());

			// Row Numbers submenu
			ToolStripMenuItem rowNumbersItem = CreateRowNumbersMenu();

			// Instrument Format submenu
			ToolStripMenuItem instrumentFormatItem = CreateInstrumentFormatMenu();

			// Volume Format submenu
			ToolStripMenuItem volumeFormatItem = CreateVolumeFormatMenu();

			// Track/Pattern Format submenu
			ToolStripMenuItem trackPatternFormatItem = CreateTrackPatternFormatMenu();

			// Grid Lines submenu
			ToolStripMenuItem gridLinesItem = CreateGridLinesMenu();

			// Scaling submenu
			ToolStripMenuItem fontScaleItem = CreateScalingMenu();

			// Compress Patterns submenu
			ToolStripMenuItem compressPatternsItem = CreateCompressPatternsMenu();

			// Add items to menu
			Menu.Items.AddRange(trackerStyleItem, volumeBarsItem, new ToolStripSeparator(),
				displayModeItem, fontScaleItem, new ToolStripSeparator(),
				showControlBarItem, rollingPatternsItem, hideEmptyItem, showTransposedNotesItem, compressPatternsItem, new ToolStripSeparator(),
				rowNumbersItem, instrumentFormatItem, volumeFormatItem, trackPatternFormatItem, gridLinesItem);

#if DEBUG
			// Debug-only items go at the end
			showDebugInfoItem = CreateMenuItem("Show Debug Info", () => ToggleShowDebugInfo());
			Menu.Items.Add(new ToolStripSeparator());
			Menu.Items.Add(showDebugInfoItem);
#endif
		}

		/********************************************************************/
		/// <summary>
		/// Create the Tracker Style submenu dynamically from registry
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateTrackerStyleMenu()
		{
			trackerStyleMenuItem = new ToolStripMenuItem("Tracker Style");

			// Create category submenus dynamically from registry
			foreach (string rootCategory in TrackerRegistry.GetRootCategories())
			{
				ToolStripMenuItem categoryItem = new(rootCategory);

				// Build hierarchical menu for this root category
				BuildCategoryMenu(categoryItem, rootCategory);

				trackerStyleMenuItem.DropDownItems.Add(categoryItem);
			}

			return trackerStyleMenuItem;
		}

		/********************************************************************/
		/// <summary>
		/// Recursively build menu items for a category path
		/// </summary>
		/********************************************************************/
		private void BuildCategoryMenu(ToolStripMenuItem parentMenu, string categoryPath)
		{
			// Collect all menu entries (both submenus and direct styles) for unified sorting
			List<(string SortName, Action<ToolStripMenuItem> AddToMenu)> menuEntries = new();

			// Get the display name for a sub-category (last part of the path)
			string GetDisplayName(string path)
			{
				int lastSeparator = path.LastIndexOf('\\');
				return lastSeparator >= 0 ? path.Substring(lastSeparator + 1) : path;
			}

			// Collect sub-category entries
			List<string> subCategories = TrackerRegistry.GetSubCategories(categoryPath)
				.Where(c => c != categoryPath && c.StartsWith(categoryPath + "\\"))
				.ToList();

			foreach (string subCategoryPath in subCategories)
			{
				string displayName = GetDisplayName(subCategoryPath);

				// Check if this sub-category has further sub-categories or multiple styles
				List<TrackerStyleRegistration> stylesAtPath = TrackerRegistry.GetStylesAtPath(subCategoryPath).ToList();
				bool hasSubCategories = TrackerRegistry.HasSubCategories(subCategoryPath);

				if (hasSubCategories || stylesAtPath.Count > 1)
				{
					// Will be a submenu
					string capturedPath = subCategoryPath;
					menuEntries.Add((displayName, parent =>
					{
						ToolStripMenuItem subMenu = new(displayName);
						BuildCategoryMenu(subMenu, capturedPath);
						parent.DropDownItems.Add(subMenu);
					}));
				}
				else if (stylesAtPath.Count == 1)
				{
					// Single style - add directly without submenu
					TrackerStyleRegistration registration = stylesAtPath[0];
					menuEntries.Add((registration.DisplayName, parent =>
					{
						ToolStripMenuItem menuItem = CreateMenuItem(registration.DisplayName, () => SwitchToStyleById(registration.Id));
						trackerStyleMenuItems[registration.Id] = menuItem;
						parent.DropDownItems.Add(menuItem);
					}));
				}
			}

			// Collect styles that are directly at this category path
			foreach (TrackerStyleRegistration registration in TrackerRegistry.GetStylesAtPath(categoryPath))
			{
				TrackerStyleRegistration reg = registration;
				menuEntries.Add((reg.DisplayName, parent =>
				{
					ToolStripMenuItem menuItem = CreateMenuItem(reg.DisplayName, () => SwitchToStyleById(reg.Id));
					trackerStyleMenuItems[reg.Id] = menuItem;
					parent.DropDownItems.Add(menuItem);
				}));
			}

			// Sort all entries alphabetically and add to menu
			foreach ((string SortName, Action<ToolStripMenuItem> AddToMenu) entry in menuEntries.OrderBy(e => e.SortName))
			{
				entry.AddToMenu(parentMenu);
			}

			// Add tracker-specific options for certain categories
			AddTrackerFamilyOptions(parentMenu, categoryPath);
		}

		/********************************************************************/
		/// <summary>
		/// Add tracker family-specific options to a category menu
		/// </summary>
		/********************************************************************/
		private void AddTrackerFamilyOptions(ToolStripMenuItem parentMenu, string categoryPath)
		{
			TrackerFamilyRegistration family = TrackerFamilyOptions.GetFamily(categoryPath);
			if (family == null || family.Options.Count == 0)
			{
				return;
			}

			parentMenu.DropDownItems.Add(new ToolStripSeparator());

			foreach (TrackerFamilyOptionDefinition option in family.Options)
			{
				ToolStripMenuItem menuItem = CreateMenuItem(option.DisplayName, () => ToggleTrackerOption(family.FamilyId, option.Id));
				menuItem.Tag = (family.FamilyId, option.Id, option.DefaultValue);
				trackerOptionMenuItems.Add(menuItem);
				parentMenu.DropDownItems.Add(menuItem);
			}
		}

		/********************************************************************/
		/// <summary>
		/// Toggle a tracker option
		/// </summary>
		/********************************************************************/
		private void ToggleTrackerOption(string trackerFamily, string optionId)
		{
			bool currentValue = settings.GetTrackerOption(trackerFamily, optionId);
			settings.SetTrackerOption(trackerFamily, optionId, !currentValue);
			updateStyleCallback?.Invoke();
			panel?.Invalidate();
		}


		/********************************************************************/
		/// <summary>
		/// Create the Display Mode submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateDisplayModeMenu()
		{
			fullModeItem = CreateMenuItem("Multi Effect Mode", () => SetDisplayMode(DisplayMode.MultiEffect));
			singleEffectModeItem = CreateMenuItem("Single Effect Mode", () => SetDisplayMode(DisplayMode.SingleEffect));
			compactModeItem = CreateMenuItem("Compact Mode", () => SetDisplayMode(DisplayMode.Compact));
			notesOnlyModeItem = CreateMenuItem("Notes Only Mode", () => SetDisplayMode(DisplayMode.NotesOnly));

			ToolStripMenuItem displayModeItem = new("Display Mode");
			displayModeItem.DropDownItems.AddRange(fullModeItem, singleEffectModeItem, compactModeItem, notesOnlyModeItem);

			return displayModeItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the VU Meters submenu dynamically from registry
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateVuMetersMenu()
		{
			ToolStripMenuItem volumeBarsItem = new("VU Meters");

			// VU meter mode options
			volumeOffItem = CreateMenuItem("Off", () => SetVolumeBarMode(VolumeBarMode.Off));
			volumeNoteKickItem = CreateMenuItem("Note Kick", () => SetVolumeBarMode(VolumeBarMode.NoteKick));
			volumeRealItem = CreateMenuItem("Real Volume", () => SetVolumeBarMode(VolumeBarMode.RealVolume));

			volumeBarsItem.DropDownItems.Add(volumeOffItem);
			volumeBarsItem.DropDownItems.Add(volumeNoteKickItem);
			volumeBarsItem.DropDownItems.Add(volumeRealItem);
			volumeBarsItem.DropDownItems.Add(new ToolStripSeparator());

			// Match Tracker option (acts as a VU choice; selecting another VU turns Match off)
			vuMeterMatchTrackerItem = CreateMenuItem("Match Tracker", () => SetMatchTrackerVuMeter());
			volumeBarsItem.DropDownItems.Add(vuMeterMatchTrackerItem);

			// Root level VU meters (no category)
			foreach (VuMeterRegistration vu in VuMeterRegistry.GetRootLevel())
			{
				string id = vu.Id;
				ToolStripMenuItem menuItem = CreateMenuItem(vu.DisplayName, () => SetVuMeterStyle(id));
				vuMeterStyleMenuItems[id] = menuItem;
				volumeBarsItem.DropDownItems.Add(menuItem);
			}

			volumeBarsItem.DropDownItems.Add(new ToolStripSeparator());

			// Category submenus dynamically from registry
			foreach (string category in VuMeterRegistry.GetCategories())
			{
				ToolStripMenuItem categoryItem = new(category);

				foreach (VuMeterRegistration vu in VuMeterRegistry.GetByCategory(category))
				{
					string id = vu.Id;
					ToolStripMenuItem menuItem = CreateMenuItem(vu.DisplayName, () => SetVuMeterStyle(id));
					vuMeterStyleMenuItems[id] = menuItem;
					categoryItem.DropDownItems.Add(menuItem);
				}

				volumeBarsItem.DropDownItems.Add(categoryItem);
			}

			return volumeBarsItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Row Numbers submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateRowNumbersMenu()
		{
			rowNumbersAutoItem = CreateMenuItem("Auto", () => SetRowNumbersFormat(NumberDisplayMode.Auto));
			rowNumbersDecimalItem = CreateMenuItem("Decimal", () => SetRowNumbersFormat(NumberDisplayMode.Decimal));
			rowNumbersHexItem = CreateMenuItem("Hexadecimal", () => SetRowNumbersFormat(NumberDisplayMode.Hexadecimal));

			ToolStripMenuItem rowNumbersItem = new("Row Numbers");
			rowNumbersItem.DropDownItems.AddRange(rowNumbersAutoItem, rowNumbersDecimalItem, rowNumbersHexItem);

			return rowNumbersItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Instrument Format submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateInstrumentFormatMenu()
		{
			instrumentAutoItem = CreateMenuItem("Auto", () => SetInstrumentFormat(NumberDisplayMode.Auto));
			instrumentDecimalItem = CreateMenuItem("Decimal", () => SetInstrumentFormat(NumberDisplayMode.Decimal));
			instrumentHexItem = CreateMenuItem("Hexadecimal", () => SetInstrumentFormat(NumberDisplayMode.Hexadecimal));

			ToolStripMenuItem instrumentFormatItem = new("Instrument");
			instrumentFormatItem.DropDownItems.AddRange(instrumentAutoItem, instrumentDecimalItem, instrumentHexItem);

			return instrumentFormatItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Volume Format submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateVolumeFormatMenu()
		{
			volumeAutoItem = CreateMenuItem("Auto", () => SetVolumeFormat(NumberDisplayMode.Auto));
			volumeDecimalItem = CreateMenuItem("Decimal", () => SetVolumeFormat(NumberDisplayMode.Decimal));
			volumeHexItem = CreateMenuItem("Hexadecimal", () => SetVolumeFormat(NumberDisplayMode.Hexadecimal));

			ToolStripMenuItem volumeFormatItem = new("Volume");
			volumeFormatItem.DropDownItems.AddRange(volumeAutoItem, volumeDecimalItem, volumeHexItem);

			return volumeFormatItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Track/Pattern Number Format submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateTrackPatternFormatMenu()
		{
			trackPatternAutoItem = CreateMenuItem("Auto", () => SetTrackPatternFormat(NumberDisplayMode.Auto));
			trackPatternDecimalItem = CreateMenuItem("Decimal", () => SetTrackPatternFormat(NumberDisplayMode.Decimal));
			trackPatternHexItem = CreateMenuItem("Hexadecimal", () => SetTrackPatternFormat(NumberDisplayMode.Hexadecimal));

			ToolStripMenuItem trackPatternItem = new("Track/Pattern");
			trackPatternItem.DropDownItems.AddRange(trackPatternAutoItem, trackPatternDecimalItem, trackPatternHexItem);

			return trackPatternItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Grid Lines submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateGridLinesMenu()
		{
			gridLinesAutoItem = CreateMenuItem("Auto", () => SetGridLinesMode(GridLinesMode.Auto));
			gridLinesOnItem = CreateMenuItem("On", () => SetGridLinesMode(GridLinesMode.On));
			gridLinesOffItem = CreateMenuItem("Off", () => SetGridLinesMode(GridLinesMode.Off));

			ToolStripMenuItem gridLinesItem = new("Grid Lines");
			gridLinesItem.DropDownItems.AddRange(gridLinesAutoItem, gridLinesOnItem, gridLinesOffItem);

			return gridLinesItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Scaling submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateScalingMenu()
		{
			fontScale100Item = CreateMenuItem("100%", () => SetFontScale(FontScale.Scale100));
			fontScale125Item = CreateMenuItem("125%", () => SetFontScale(FontScale.Scale125));
			fontScale150Item = CreateMenuItem("150%", () => SetFontScale(FontScale.Scale150));
			fontScale175Item = CreateMenuItem("175%", () => SetFontScale(FontScale.Scale175));
			fontScale200Item = CreateMenuItem("200%", () => SetFontScale(FontScale.Scale200));
			fontScale300Item = CreateMenuItem("300%", () => SetFontScale(FontScale.Scale300));
			fontScale400Item = CreateMenuItem("400%", () => SetFontScale(FontScale.Scale400));

			ToolStripMenuItem fontScaleItem = new("Scaling");
			fontScaleItem.DropDownItems.AddRange(fontScale100Item, fontScale125Item, fontScale150Item, fontScale175Item, fontScale200Item, fontScale300Item, fontScale400Item);

			return fontScaleItem;
		}

		/********************************************************************/
		/// <summary>
		/// Create the Compress Patterns submenu
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateCompressPatternsMenu()
		{
			compressPatternsAutoItem = CreateMenuItem("Auto", () => SetCompressPatternsMode(CompressPatternsMode.Auto));
			compressPatternsOnItem = CreateMenuItem("On", () => SetCompressPatternsMode(CompressPatternsMode.On));
			compressPatternsOffItem = CreateMenuItem("Off", () => SetCompressPatternsMode(CompressPatternsMode.Off));

			ToolStripMenuItem compressPatternsItem = new("Compress Patterns");
			compressPatternsItem.DropDownItems.AddRange(compressPatternsAutoItem, compressPatternsOnItem, compressPatternsOffItem);

			return compressPatternsItem;
		}

		/********************************************************************/
		/// <summary>
		/// Helper to create a menu item with click handler
		/// </summary>
		/********************************************************************/
		private ToolStripMenuItem CreateMenuItem(string text, Action onClick)
		{
			ToolStripMenuItem item = new(text);
			if (onClick != null)
			{
				item.Click += (s, e) => onClick();
			}

			return item;
		}

		/********************************************************************/
		/// <summary>
		/// Update context menu checkmarks when opening
		/// </summary>
		/********************************************************************/
		private void ContextMenu_Opening(object sender, CancelEventArgs e)
		{
			// Update display mode
			fullModeItem.Checked = renderer.CurrentDisplayMode == DisplayMode.MultiEffect;
			singleEffectModeItem.Checked = renderer.CurrentDisplayMode == DisplayMode.SingleEffect;
			compactModeItem.Checked = renderer.CurrentDisplayMode == DisplayMode.Compact;
			notesOnlyModeItem.Checked = renderer.CurrentDisplayMode == DisplayMode.NotesOnly;

			// Update row number format
			rowNumbersAutoItem.Checked = renderer.RowNumberDisplayMode == NumberDisplayMode.Auto;
			rowNumbersDecimalItem.Checked = renderer.RowNumberDisplayMode == NumberDisplayMode.Decimal;
			rowNumbersHexItem.Checked = renderer.RowNumberDisplayMode == NumberDisplayMode.Hexadecimal;

			// Update instrument format
			instrumentAutoItem.Checked = renderer.InstrumentDisplayMode == NumberDisplayMode.Auto;
			instrumentDecimalItem.Checked = renderer.InstrumentDisplayMode == NumberDisplayMode.Decimal;
			instrumentHexItem.Checked = renderer.InstrumentDisplayMode == NumberDisplayMode.Hexadecimal;

			// Update volume format
			volumeAutoItem.Checked = renderer.VolumeDisplayMode == NumberDisplayMode.Auto;
			volumeDecimalItem.Checked = renderer.VolumeDisplayMode == NumberDisplayMode.Decimal;
			volumeHexItem.Checked = renderer.VolumeDisplayMode == NumberDisplayMode.Hexadecimal;

			// Update track/pattern format
			trackPatternAutoItem.Checked = renderer.TrackPatternDisplayMode == NumberDisplayMode.Auto;
			trackPatternDecimalItem.Checked = renderer.TrackPatternDisplayMode == NumberDisplayMode.Decimal;
			trackPatternHexItem.Checked = renderer.TrackPatternDisplayMode == NumberDisplayMode.Hexadecimal;

			// Update grid lines
			gridLinesAutoItem.Checked = renderer.GridLinesDisplayMode == GridLinesMode.Auto;
			gridLinesOnItem.Checked = renderer.GridLinesDisplayMode == GridLinesMode.On;
			gridLinesOffItem.Checked = renderer.GridLinesDisplayMode == GridLinesMode.Off;

			// Update volume bar mode
			volumeOffItem.Checked = renderer.VolumeBarState.DisplaySettings.Mode == VolumeBarMode.Off;
			volumeNoteKickItem.Checked = renderer.VolumeBarState.DisplaySettings.Mode == VolumeBarMode.NoteKick;
			volumeRealItem.Checked = renderer.VolumeBarState.DisplaySettings.Mode == VolumeBarMode.RealVolume;

			// Update VU meter style checkmarks using dictionary
			bool matchActive = settings.MatchTrackerVuMeter;
			string currentVuId = renderer.VolumeBarState.DisplaySettings.VuMeterId;
			foreach (KeyValuePair<string, ToolStripMenuItem> kvp in vuMeterStyleMenuItems)
			{
				kvp.Value.Checked = !matchActive && kvp.Key == currentVuId;
			}

			vuMeterMatchTrackerItem.Checked = matchActive;

			// Update rolling patterns, hide empty, control bar, transposed notes
			rollingPatternsItem.Checked = renderer.RollingPatterns;
			hideEmptyItem.Checked = renderer.HideEmpty;
			showControlBarItem.Checked = renderer.ShowControlBar;
			showTransposedNotesItem.Checked = settings.ShowTransposedNotes;

			// Update compress patterns mode
			compressPatternsAutoItem.Checked = settings.CompressPatterns == CompressPatternsMode.Auto;
			compressPatternsOnItem.Checked = settings.CompressPatterns == CompressPatternsMode.On;
			compressPatternsOffItem.Checked = settings.CompressPatterns == CompressPatternsMode.Off;

#if DEBUG
			showDebugInfoItem.Checked = renderer.ShowDebugInfo;
#endif

			// Update tracker style checkmarks using dictionary
			foreach (KeyValuePair<string, ToolStripMenuItem> kvp in trackerStyleMenuItems)
			{
				kvp.Value.Checked = kvp.Key == CurrentStyleId;
			}

			// Update font scale
			fontScale100Item.Checked = settings.FontScale == FontScale.Scale100;
			fontScale125Item.Checked = settings.FontScale == FontScale.Scale125;
			fontScale150Item.Checked = settings.FontScale == FontScale.Scale150;
			fontScale175Item.Checked = settings.FontScale == FontScale.Scale175;
			fontScale200Item.Checked = settings.FontScale == FontScale.Scale200;
			fontScale300Item.Checked = settings.FontScale == FontScale.Scale300;
			fontScale400Item.Checked = settings.FontScale == FontScale.Scale400;

			// Update tracker family options (checked state)
			UpdateTrackerFamilyOptionsCheckedState();
		}

		/********************************************************************/
		/// <summary>
		/// Update checked state of tracker family options
		/// </summary>
		/********************************************************************/
		private void UpdateTrackerFamilyOptionsCheckedState()
		{
			foreach (ToolStripMenuItem item in trackerOptionMenuItems)
			{
				if (item.Tag is (string familyId, string optionId, bool defaultValue))
				{
					item.Checked = settings.GetTrackerOption(familyId, optionId, defaultValue);
				}
			}
		}

		#region Setting Methods
		/********************************************************************/
		/// <summary>
		/// Switch tracker style by registry ID
		/// </summary>
		/********************************************************************/
		private void SwitchToStyleById(string styleId)
		{
			TrackerStyleRegistration registration = TrackerRegistry.Get(styleId);
			if (registration == null)
			{
				return;
			}

			CurrentStyleId = styleId;
			renderer.DrawGrid = registration.DrawGrid;
			SaveSettings();

			// Apply matching VU meter (null/empty = tracker has no VU, render nothing)
			if (settings.MatchTrackerVuMeter)
			{
				renderer.VolumeBarState.DisplaySettings.VuMeterId = registration.VuMeterId;
				SaveSettings();
			}

			updateStyleCallback?.Invoke();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Navigate to the next tracker style in menu order
		/// </summary>
		/********************************************************************/
		public void NavigateToNextStyle()
		{
			List<TrackerStyleRegistration> allStyles = TrackerRegistry.GetAllInMenuOrder().ToList();
			if (allStyles.Count == 0)
			{
				return;
			}

			int currentIndex = allStyles.FindIndex(s => s.Id == CurrentStyleId);
			int nextIndex = (currentIndex + 1) % allStyles.Count;

			SwitchToStyleById(allStyles[nextIndex].Id);
		}

		/********************************************************************/
		/// <summary>
		/// Navigate to the previous tracker style in menu order
		/// </summary>
		/********************************************************************/
		public void NavigateToPreviousStyle()
		{
			List<TrackerStyleRegistration> allStyles = TrackerRegistry.GetAllInMenuOrder().ToList();
			if (allStyles.Count == 0)
			{
				return;
			}

			int currentIndex = allStyles.FindIndex(s => s.Id == CurrentStyleId);
			int prevIndex = currentIndex <= 0 ? allStyles.Count - 1 : currentIndex - 1;

			SwitchToStyleById(allStyles[prevIndex].Id);
		}

		/********************************************************************/
		/// <summary>
		/// Set display mode
		/// </summary>
		/********************************************************************/
		private void SetDisplayMode(DisplayMode mode)
		{
			renderer.CurrentDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set grid lines mode
		/// </summary>
		/********************************************************************/
		private void SetGridLinesMode(GridLinesMode mode)
		{
			renderer.GridLinesDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set row numbers format
		/// </summary>
		/********************************************************************/
		private void SetRowNumbersFormat(NumberDisplayMode mode)
		{
			renderer.RowNumberDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set instrument format
		/// </summary>
		/********************************************************************/
		private void SetInstrumentFormat(NumberDisplayMode mode)
		{
			renderer.InstrumentDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set volume format
		/// </summary>
		/********************************************************************/
		private void SetVolumeFormat(NumberDisplayMode mode)
		{
			renderer.VolumeDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set track/pattern number format
		/// </summary>
		/********************************************************************/
		private void SetTrackPatternFormat(NumberDisplayMode mode)
		{
			renderer.TrackPatternDisplayMode = mode;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set volume bar mode
		/// </summary>
		/********************************************************************/
		private void SetVolumeBarMode(VolumeBarMode mode)
		{
			renderer.VolumeBarState.DisplaySettings.Mode = mode;
			SaveSettings();

			// VU meters are baked into the cached bitmap, so the cache must be
			// rebuilt for the change to show (a dummy pattern otherwise stays
			// frozen, as it gets no row-change repaints to refresh the cache)
			renderer.InvalidateCache();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Set VU meter style
		/// </summary>
		/********************************************************************/
		private void SetVuMeterStyle(string id)
		{
			settings.MatchTrackerVuMeter = false;
			renderer.VolumeBarState.DisplaySettings.VuMeterId = id;
			SaveSettings();
			renderer.InvalidateCache();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Activate match tracker VU meter (behaves like selecting a VU style)
		/// </summary>
		/********************************************************************/
		private void SetMatchTrackerVuMeter()
		{
			settings.MatchTrackerVuMeter = true;
			ApplyMatchingVuMeterStyle();
			renderer.InvalidateCache();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Apply the matching VU meter style based on current tracker style
		/// </summary>
		/********************************************************************/
		private void ApplyMatchingVuMeterStyle()
		{
			TrackerStyleRegistration registration = TrackerRegistry.Get(CurrentStyleId);
			if (registration != null)
			{
				renderer.VolumeBarState.DisplaySettings.VuMeterId = registration.VuMeterId;
				SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Toggle hide empty
		/// </summary>
		/********************************************************************/
		private void ToggleHideEmpty()
		{
			renderer.HideEmpty = !renderer.HideEmpty;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Toggle rolling patterns
		/// </summary>
		/********************************************************************/
		private void ToggleRollingPatterns()
		{
			renderer.RollingPatterns = !renderer.RollingPatterns;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Toggle show control bar
		/// </summary>
		/********************************************************************/
		private void ToggleShowControlBar()
		{
			renderer.ShowControlBar = !renderer.ShowControlBar;
			SaveSettings();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Toggle show transposed notes
		/// </summary>
		/********************************************************************/
		private void ToggleShowTransposedNotes()
		{
			settings.ShowTransposedNotes = !settings.ShowTransposedNotes;
			ShowTransposedNotesChanged?.Invoke();
			panel?.Invalidate();
		}

		/// <summary>
		/// Event fired when show transposed notes mode changes
		/// </summary>
		public event Action ShowTransposedNotesChanged;

		/********************************************************************/
		/// <summary>
		/// Set compress patterns mode
		/// </summary>
		/********************************************************************/
		private void SetCompressPatternsMode(CompressPatternsMode mode)
		{
			settings.CompressPatterns = mode;
			CompressPatternsChanged?.Invoke();
			panel?.Invalidate();
		}

		/// <summary>
		/// Event fired when compress patterns mode changes
		/// </summary>
		public event Action CompressPatternsChanged;

		/********************************************************************/
		/// <summary>
		/// Toggle show debug info
		/// </summary>
		/********************************************************************/
#if DEBUG
		private void ToggleShowDebugInfo()
		{
			renderer.ShowDebugInfo = !renderer.ShowDebugInfo;
			renderer.InvalidateCache();
			panel?.Invalidate();
		}
#endif

		/********************************************************************/
		/// <summary>
		/// Set font scale
		/// </summary>
		/********************************************************************/
		private void SetFontScale(FontScale scale)
		{
			settings.FontScale = scale;
			updateStyleCallback?.Invoke();
			panel?.Invalidate();
		}

		/********************************************************************/
		/// <summary>
		/// Save settings
		/// </summary>
		/********************************************************************/
		private void SaveSettings()
		{
			if (settings == null)
			{
				return;
			}

			settings.DisplayMode = renderer.CurrentDisplayMode;
			settings.RowNumberFormat = renderer.RowNumberDisplayMode;
			settings.InstrumentFormat = renderer.InstrumentDisplayMode;
			settings.VolumeFormat = renderer.VolumeDisplayMode;
			settings.TrackPatternNumberFormat = renderer.TrackPatternDisplayMode;
			settings.GridLinesMode = renderer.GridLinesDisplayMode;
			settings.VolumeBarMode = renderer.VolumeBarState.DisplaySettings.Mode;
			settings.VuMeterId = renderer.VolumeBarState.DisplaySettings.VuMeterId;
			settings.RollingPatterns = renderer.RollingPatterns;
			settings.StripedChannels = renderer.StripedChannels;
			settings.HideEmpty = renderer.HideEmpty;
			settings.TrackerStyleId = CurrentStyleId;
			settings.PlayerControlBar = renderer.ShowControlBar;
		}
		#endregion
	}
}
