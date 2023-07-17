/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Client.GuiPlayer.ModuleInfoWindow;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the module settings
	/// </summary>
	public class ModuleSettings
	{
		/// <summary>
		/// The different actions to take when a module error occur
		/// </summary>
		public enum ModuleErrorAction
		{
			/// <summary></summary>
			ShowError,
			/// <summary></summary>
			SkipFile,
			/// <summary></summary>
			SkipFileAndRemoveFromList,
			/// <summary></summary>
			StopPlaying
		}

		/// <summary>
		/// The different actions to take when reached the end of the list
		/// </summary>
		public enum ModuleListEndAction
		{
			/// <summary></summary>
			Eject,
			/// <summary></summary>
			JumpToStart,
			/// <summary></summary>
			Loop
		}

		/// <summary>
		/// The different tabs in the window. The order need to be the same as
		/// shown in the window
		/// </summary>
		public enum ModuleInfoTab
		{
			/// <summary></summary>
			Info = 0,
			/// <summary></summary>
			Comments,
			/// <summary></summary>
			Lyrics,
			/// <summary></summary>
			Pictures
		}

		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleSettings(ISettings userSettings)
		{
			settings = userSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Double buffering
		/// </summary>
		/********************************************************************/
		public bool DoubleBuffering
		{
			get => settings.GetBoolEntry("Modules", "DoubleBuffering", false);

			set => settings.SetBoolEntry("Modules", "DoubleBuffering", value);
		}



		/********************************************************************/
		/// <summary>
		/// Early load
		/// </summary>
		/********************************************************************/
		public int DoubleBufferingEarlyLoad
		{
			get => settings.GetIntEntry("Modules", "DoubleBufferingEarlyLoad", 2);

			set => settings.SetIntEntry("Modules", "DoubleBufferingEarlyLoad", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module error reached
		/// </summary>
		/********************************************************************/
		public ModuleErrorAction ModuleError
		{
			get => settings.GetEnumEntry("Modules", "ModuleError", ModuleErrorAction.ShowError);

			set => settings.SetEnumEntry("Modules", "ModuleError", value);
		}



		/********************************************************************/
		/// <summary>
		/// Never ending
		/// </summary>
		/********************************************************************/
		public bool NeverEnding
		{
			get => settings.GetBoolEntry("Modules", "NeverEnding", false);

			set => settings.SetBoolEntry("Modules", "NeverEnding", value);
		}



		/********************************************************************/
		/// <summary>
		/// Never ending timeout
		/// </summary>
		/********************************************************************/
		public int NeverEndingTimeout
		{
			get => settings.GetIntEntry("Modules", "NeverEndingTimeout", 180);

			set => settings.SetIntEntry("Modules", "NeverEndingTimeout", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module list end reached
		/// </summary>
		/********************************************************************/
		public ModuleListEndAction ModuleListEnd
		{
			get => settings.GetEnumEntry("Modules", "ModuleListEnd", ModuleListEndAction.JumpToStart);

			set => settings.SetEnumEntry("Modules", "ModuleListEnd", value);
		}



		/********************************************************************/
		/// <summary>
		/// Module information tab order
		/// </summary>
		/********************************************************************/
		public ModuleInfoTab[] ModuleInfoActivateTabOrder
		{
			get => settings.GetStringEntry("Modules", "ModuleInfoActivateTabOrder", string.Join(',', new[]
			{
				ModuleInfoTab.Pictures.ToString(),
				ModuleInfoTab.Lyrics.ToString(),
				ModuleInfoTab.Comments.ToString(),
				ModuleInfoTab.Info.ToString()
			})).Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Where(x => Enum.TryParse<ModuleInfoTab>(x, out _))
				.Select(Enum.Parse<ModuleInfoTab>).ToArray();

			set => settings.SetStringEntry("Modules", "ModuleInfoActivateTabOrder", string.Join(',', value));
		}
	}
}
