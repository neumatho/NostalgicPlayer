/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the favorite song system window settings
	/// </summary>
	public class FavoriteSongSystemWindowSettings
	{
		/// <summary>
		/// How many and which items to show in the list
		/// </summary>
		public enum WhatToShow
		{
			/// <summary></summary>
			Top10,
			/// <summary></summary>
			Top50,
			/// <summary></summary>
			Top100,
			/// <summary></summary>
			TopX,
			/// <summary></summary>
			Bottom10,
			/// <summary></summary>
			Bottom50,
			/// <summary></summary>
			Bottom100,
			/// <summary></summary>
			BottomX
		}

		private readonly ISettings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FavoriteSongSystemWindowSettings(ISettings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Column 1 width
		/// </summary>
		/********************************************************************/
		public int Column1Width
		{
			get => settings.GetIntEntry("List", "Col1Width", 28);

			set => settings.SetIntEntry("List", "Col1Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 1 position
		/// </summary>
		/********************************************************************/
		public int Column1Pos
		{
			get => settings.GetIntEntry("List", "Col1Pos", 0);

			set => settings.SetIntEntry("List", "Col1Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 2 width
		/// </summary>
		/********************************************************************/
		public int Column2Width
		{
			get => settings.GetIntEntry("List", "Col2Width", 244);

			set => settings.SetIntEntry("List", "Col2Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 2 position
		/// </summary>
		/********************************************************************/
		public int Column2Pos
		{
			get => settings.GetIntEntry("List", "Col2Pos", 1);

			set => settings.SetIntEntry("List", "Col2Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 3 width
		/// </summary>
		/********************************************************************/
		public int Column3Width
		{
			get => settings.GetIntEntry("List", "Col3Width", 104);

			set => settings.SetIntEntry("List", "Col3Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Column 3 position
		/// </summary>
		/********************************************************************/
		public int Column3Pos
		{
			get => settings.GetIntEntry("List", "Col3Pos", 2);

			set => settings.SetIntEntry("List", "Col3Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// What to show in the list
		/// </summary>
		/********************************************************************/
		public WhatToShow Show
		{
			get => settings.GetEnumEntry("Options", "Show", WhatToShow.Top10);

			set => settings.SetEnumEntry("Options", "Show", value);
		}



		/********************************************************************/
		/// <summary>
		/// How many to show
		/// </summary>
		/********************************************************************/
		public int ShowOther
		{
			get => settings.GetIntEntry("Options", "ShowOther", 25);

			set => settings.SetIntEntry("Options", "ShowOther", value);
		}
	}
}
