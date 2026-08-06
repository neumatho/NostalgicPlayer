/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Containers;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Pattern;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Tracker;
using Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer
{
	/// <summary>
	/// This class holds the PatternViewer specific settings
	/// </summary>
	internal class PatternViewerSettings
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PatternViewerSettings(ISettingsFactory settingsFactory)
		{
			Settings = settingsFactory.GetSettings();
			Settings.LoadSettings("PatternViewer");
		}

		/********************************************************************/
		/// <summary>
		/// Return the main settings object
		/// </summary>
		/********************************************************************/
		public ISettings Settings
		{
			get;
		}

		/********************************************************************/
		/// <summary>
		/// Display mode (Full/Compact)
		/// </summary>
		/********************************************************************/
		public DisplayMode DisplayMode
		{
			get => (DisplayMode)Settings.GetIntEntry("PatternViewer", "DisplayMode", (int)DisplayMode.SingleEffect);
			set
			{
				Settings.SetIntEntry("PatternViewer", "DisplayMode", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Tracker style ID (from TrackerRegistry)
		/// </summary>
		/********************************************************************/
		public string TrackerStyleId
		{
			get => Settings.GetStringEntry("PatternViewer", "TrackerStyleId", "System.Light");
			set
			{
				Settings.SetStringEntry("PatternViewer", "TrackerStyleId", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Row number display format (Auto/Decimal/Hexadecimal)
		/// </summary>
		/********************************************************************/
		public NumberDisplayMode RowNumberFormat
		{
			get => (NumberDisplayMode)Settings.GetIntEntry("PatternViewer", "RowNumberFormat");
			set
			{
				Settings.SetIntEntry("PatternViewer", "RowNumberFormat", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Instrument number display format (Auto/Decimal/Hexadecimal)
		/// </summary>
		/********************************************************************/
		public NumberDisplayMode InstrumentFormat
		{
			get => (NumberDisplayMode)Settings.GetIntEntry("PatternViewer", "InstrumentFormat");
			set
			{
				Settings.SetIntEntry("PatternViewer", "InstrumentFormat", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Volume display format (Auto/Decimal/Hexadecimal)
		/// </summary>
		/********************************************************************/
		public NumberDisplayMode VolumeFormat
		{
			get => (NumberDisplayMode)Settings.GetIntEntry("PatternViewer", "VolumeFormat");
			set
			{
				Settings.SetIntEntry("PatternViewer", "VolumeFormat", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Track/Pattern number display format (Auto/Decimal/Hexadecimal)
		/// Auto = follows RowNumberFormat
		/// </summary>
		/********************************************************************/
		public NumberDisplayMode TrackPatternNumberFormat
		{
			get => (NumberDisplayMode)Settings.GetIntEntry("PatternViewer", "TrackPatternNumberFormat");
			set
			{
				Settings.SetIntEntry("PatternViewer", "TrackPatternNumberFormat", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Grid lines display mode (Auto/On/Off)
		/// </summary>
		/********************************************************************/
		public GridLinesMode GridLinesMode
		{
			get => (GridLinesMode)Settings.GetIntEntry("PatternViewer", "GridLinesMode");
			set
			{
				Settings.SetIntEntry("PatternViewer", "GridLinesMode", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Volume bar mode (Off/NoteKick/RealVolume)
		/// </summary>
		/********************************************************************/
		public VolumeBarMode VolumeBarMode
		{
			get => (VolumeBarMode)Settings.GetIntEntry("PatternViewer", "VolumeBarMode", (int)VolumeBarMode.RealVolume);
			set
			{
				Settings.SetIntEntry("PatternViewer", "VolumeBarMode", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// VU meter visual style (reference to VuMeterRegistration.Id, e.g. "ProTracker.ProTracker36")
		/// </summary>
		/********************************************************************/
		public string VuMeterId
		{
			get => Settings.GetStringEntry("PatternViewer", "VuMeterId");
			set
			{
				Settings.SetStringEntry("PatternViewer", "VuMeterId", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Match VU meter style to tracker style.
		/// When enabled, the VU meter style automatically matches the current tracker style.
		/// </summary>
		/********************************************************************/
		public bool MatchTrackerVuMeter
		{
			get => Settings.GetBoolEntry("PatternViewer", "MatchTrackerVuMeter", true);
			set
			{
				Settings.SetBoolEntry("PatternViewer", "MatchTrackerVuMeter", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Rolling patterns mode
		/// </summary>
		/********************************************************************/
		public bool RollingPatterns
		{
			get => Settings.GetBoolEntry("PatternViewer", "RollingPatterns", true);
			set
			{
				Settings.SetBoolEntry("PatternViewer", "RollingPatterns", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Striped channels (alternating background colors)
		/// </summary>
		/********************************************************************/
		public bool StripedChannels
		{
			get => Settings.GetBoolEntry("PatternViewer", "StripedChannels");
			set
			{
				Settings.SetBoolEntry("PatternViewer", "StripedChannels", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Hide empty pattern entries
		/// </summary>
		/********************************************************************/
		public bool HideEmpty
		{
			get => Settings.GetBoolEntry("PatternViewer", "HideEmpty");
			set
			{
				Settings.SetBoolEntry("PatternViewer", "HideEmpty", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Color mode for modern tracker (Mono/Colored)
		/// </summary>
		/********************************************************************/
		public ColorMode ColorMode
		{
			get => (ColorMode)Settings.GetIntEntry("PatternViewer", "ColorMode");
			set
			{
				Settings.SetIntEntry("PatternViewer", "ColorMode", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Font scale for bitmap font rendering (100/125/150/200)
		/// </summary>
		/********************************************************************/
		public FontScale FontScale
		{
			get => (FontScale)Settings.GetIntEntry("PatternViewer", "FontScale", (int)FontScale.Scale100);
			set
			{
				Settings.SetIntEntry("PatternViewer", "FontScale", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Player control bar with playback buttons and time display
		/// </summary>
		/********************************************************************/
		public bool PlayerControlBar
		{
			get => Settings.GetBoolEntry("PatternViewer", "PlayerControlBar", true);
			set
			{
				Settings.SetBoolEntry("PatternViewer", "PlayerControlBar", value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Compress patterns mode (Auto/On/Off)
		/// </summary>
		/********************************************************************/
		public CompressPatternsMode CompressPatterns
		{
			get => (CompressPatternsMode)Settings.GetIntEntry("PatternViewer", "CompressPatternsMode");
			set
			{
				Settings.SetIntEntry("PatternViewer", "CompressPatternsMode", (int)value);
				Settings.SaveSettings();
			}
		}

		/********************************************************************/
		/// <summary>
		/// Show transposed notes (apply transpose to displayed note)
		/// </summary>
		/********************************************************************/
		public bool ShowTransposedNotes
		{
			get => Settings.GetBoolEntry("PatternViewer", "ShowTransposedNotes", true);
			set
			{
				Settings.SetBoolEntry("PatternViewer", "ShowTransposedNotes", value);
				Settings.SaveSettings();
			}
		}

		#region Tracker-specific options
		/********************************************************************/
		/// <summary>
		/// Get a tracker-specific boolean option
		/// </summary>
		/// <param name="trackerFamily">The tracker family (e.g., "FastTracker2")</param>
		/// <param name="optionId">The option ID (e.g., "WideFont")</param>
		/// <param name="defaultValue">Default value if not set</param>
		/********************************************************************/
		public bool GetTrackerOption(string trackerFamily, string optionId, bool defaultValue = false)
		{
			string key = $"TrackerOption.{trackerFamily}.{optionId}";
			return Settings.GetBoolEntry("PatternViewer", key, defaultValue);
		}

		/********************************************************************/
		/// <summary>
		/// Set a tracker-specific boolean option
		/// </summary>
		/// <param name="trackerFamily">The tracker family (e.g., "FastTracker2")</param>
		/// <param name="optionId">The option ID (e.g., "WideFont")</param>
		/// <param name="value">The value to set</param>
		/********************************************************************/
		public void SetTrackerOption(string trackerFamily, string optionId, bool value)
		{
			string key = $"TrackerOption.{trackerFamily}.{optionId}";
			Settings.SetBoolEntry("PatternViewer", key, value);
			Settings.SaveSettings();
		}
		#endregion
	}
}
