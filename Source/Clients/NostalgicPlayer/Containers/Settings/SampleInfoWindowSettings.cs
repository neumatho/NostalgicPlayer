/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the sample information window settings
	/// </summary>
	public class SampleInfoWindowSettings
	{
		private readonly Kit.Utility.Settings settings;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleInfoWindowSettings(Kit.Utility.Settings windowSettings)
		{
			settings = windowSettings;
		}



		/********************************************************************/
		/// <summary>
		/// Active tab
		/// </summary>
		/********************************************************************/
		public int ActiveTab
		{
			get => settings.GetIntEntry("Window", "ActiveTab", 0);

			set => settings.SetIntEntry("Window", "ActiveTab", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 1 width
		/// </summary>
		/********************************************************************/
		public int InstColumn1Width
		{
			get => settings.GetIntEntry("List", "InstCol1Width", 28);

			set => settings.SetIntEntry("List", "InstCol1Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 1 position
		/// </summary>
		/********************************************************************/
		public int InstColumn1Pos
		{
			get => settings.GetIntEntry("List", "InstCol1Pos", 0);

			set => settings.SetIntEntry("List", "InstCol1Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 2 width
		/// </summary>
		/********************************************************************/
		public int InstColumn2Width
		{
			get => settings.GetIntEntry("List", "InstCol2Width", 244);

			set => settings.SetIntEntry("List", "InstCol2Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 2 position
		/// </summary>
		/********************************************************************/
		public int InstColumn2Pos
		{
			get => settings.GetIntEntry("List", "InstCol2Pos", 1);

			set => settings.SetIntEntry("List", "InstCol2Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 3 width
		/// </summary>
		/********************************************************************/
		public int InstColumn3Width
		{
			get => settings.GetIntEntry("List", "InstCol3Width", 144);

			set => settings.SetIntEntry("List", "InstCol3Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument column 3 position
		/// </summary>
		/********************************************************************/
		public int InstColumn3Pos
		{
			get => settings.GetIntEntry("List", "InstCol3Pos", 2);

			set => settings.SetIntEntry("List", "InstCol3Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument sorted column
		/// </summary>
		/********************************************************************/
		public int InstSortKey
		{
			get => settings.GetIntEntry("List", "InstSortKey", 0);

			set => settings.SetIntEntry("List", "InstSortKey", value);
		}



		/********************************************************************/
		/// <summary>
		/// Instrument sorting order
		/// </summary>
		/********************************************************************/
		public SortOrder InstSortOrder
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("List", "InstSortOrder", SortOrder.Ascending.ToString()), out SortOrder result))
					return result;

				return SortOrder.Ascending;
			}

			set => settings.SetStringEntry("List", "InstSortOrder", value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 1 width
		/// </summary>
		/********************************************************************/
		public int SampColumn1Width
		{
			get => settings.GetIntEntry("List", "SampCol1Width", 28);

			set => settings.SetIntEntry("List", "SampCol1Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 1 position
		/// </summary>
		/********************************************************************/
		public int SampColumn1Pos
		{
			get => settings.GetIntEntry("List", "SampCol1Pos", 0);

			set => settings.SetIntEntry("List", "SampCol1Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 2 width
		/// </summary>
		/********************************************************************/
		public int SampColumn2Width
		{
			get => settings.GetIntEntry("List", "SampCol2Width", 244);

			set => settings.SetIntEntry("List", "SampCol2Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 2 position
		/// </summary>
		/********************************************************************/
		public int SampColumn2Pos
		{
			get => settings.GetIntEntry("List", "SampCol2Pos", 1);

			set => settings.SetIntEntry("List", "SampCol2Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 3 width
		/// </summary>
		/********************************************************************/
		public int SampColumn3Width
		{
			get => settings.GetIntEntry("List", "SampCol3Width", 44);

			set => settings.SetIntEntry("List", "SampCol3Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 3 position
		/// </summary>
		/********************************************************************/
		public int SampColumn3Pos
		{
			get => settings.GetIntEntry("List", "SampCol3Pos", 2);

			set => settings.SetIntEntry("List", "SampCol3Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 4 width
		/// </summary>
		/********************************************************************/
		public int SampColumn4Width
		{
			get => settings.GetIntEntry("List", "SampCol4Width", 44);

			set => settings.SetIntEntry("List", "SampCol4Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 4 position
		/// </summary>
		/********************************************************************/
		public int SampColumn4Pos
		{
			get => settings.GetIntEntry("List", "SampCol4Pos", 3);

			set => settings.SetIntEntry("List", "SampCol4Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 5 width
		/// </summary>
		/********************************************************************/
		public int SampColumn5Width
		{
			get => settings.GetIntEntry("List", "SampCol5Width", 52);

			set => settings.SetIntEntry("List", "SampCol5Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 5 position
		/// </summary>
		/********************************************************************/
		public int SampColumn5Pos
		{
			get => settings.GetIntEntry("List", "SampCol5Pos", 4);

			set => settings.SetIntEntry("List", "SampCol5Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 6 width
		/// </summary>
		/********************************************************************/
		public int SampColumn6Width
		{
			get => settings.GetIntEntry("List", "SampCol6Width", 36);

			set => settings.SetIntEntry("List", "SampCol6Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 6 position
		/// </summary>
		/********************************************************************/
		public int SampColumn6Pos
		{
			get => settings.GetIntEntry("List", "SampCol6Pos", 5);

			set => settings.SetIntEntry("List", "SampCol6Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 7 width
		/// </summary>
		/********************************************************************/
		public int SampColumn7Width
		{
			get => settings.GetIntEntry("List", "SampCol7Width", 36);

			set => settings.SetIntEntry("List", "SampCol7Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 7 position
		/// </summary>
		/********************************************************************/
		public int SampColumn7Pos
		{
			get => settings.GetIntEntry("List", "SampCol7Pos", 6);

			set => settings.SetIntEntry("List", "SampCol7Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 8 width
		/// </summary>
		/********************************************************************/
		public int SampColumn8Width
		{
			get => settings.GetIntEntry("List", "SampCol8Width", 36);

			set => settings.SetIntEntry("List", "SampCol8Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 8 position
		/// </summary>
		/********************************************************************/
		public int SampColumn8Pos
		{
			get => settings.GetIntEntry("List", "SampCol8Pos", 7);

			set => settings.SetIntEntry("List", "SampCol8Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 9 width
		/// </summary>
		/********************************************************************/
		public int SampColumn9Width
		{
			get => settings.GetIntEntry("List", "SampCol9Width", 56);

			set => settings.SetIntEntry("List", "SampCol9Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 9 position
		/// </summary>
		/********************************************************************/
		public int SampColumn9Pos
		{
			get => settings.GetIntEntry("List", "SampCol9Pos", 8);

			set => settings.SetIntEntry("List", "SampCol9Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 10 width
		/// </summary>
		/********************************************************************/
		public int SampColumn10Width
		{
			get => settings.GetIntEntry("List", "SampCol10Width", 44);

			set => settings.SetIntEntry("List", "SampCol10Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 10 position
		/// </summary>
		/********************************************************************/
		public int SampColumn10Pos
		{
			get => settings.GetIntEntry("List", "SampCol10Pos", 9);

			set => settings.SetIntEntry("List", "SampCol10Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 11 width
		/// </summary>
		/********************************************************************/
		public int SampColumn11Width
		{
			get => settings.GetIntEntry("List", "SampCol11Width", 56);

			set => settings.SetIntEntry("List", "SampCol11Width", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample column 11 position
		/// </summary>
		/********************************************************************/
		public int SampColumn11Pos
		{
			get => settings.GetIntEntry("List", "SampCol11Pos", 10);

			set => settings.SetIntEntry("List", "SampCol11Pos", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample sorted column
		/// </summary>
		/********************************************************************/
		public int SampSortKey
		{
			get => settings.GetIntEntry("List", "SampSortKey", 0);

			set => settings.SetIntEntry("List", "SampSortKey", value);
		}



		/********************************************************************/
		/// <summary>
		/// Sample sorting order
		/// </summary>
		/********************************************************************/
		public SortOrder SampSortOrder
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("List", "SampSortOrder", SortOrder.Ascending.ToString()), out SortOrder result))
					return result;

				return SortOrder.Ascending;
			}

			set => settings.SetStringEntry("List", "SampSortOrder", value.ToString());
		}
	}
}
