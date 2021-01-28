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
using Polycode.NostalgicPlayer.PlayerLibrary.Agent;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Containers.Settings
{
	/// <summary>
	/// This class holds all the agents tab window settings
	/// </summary>
	public class SettingsAgentsWindowSettings
	{
		private readonly Kit.Utility.Settings settings;
		private readonly string type;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsAgentsWindowSettings(Kit.Utility.Settings windowSettings, Manager.AgentType? agentType)
		{
			settings = windowSettings;
			type = agentType.HasValue ? agentType.ToString() : string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Active tab
		/// </summary>
		/********************************************************************/
		public int ActiveTab
		{
			get => settings.GetIntEntry("AgentsTab", "ActiveTab", 0);

			set => settings.SetIntEntry("AgentsTab", "ActiveTab", value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents column 1 position
		/// </summary>
		/********************************************************************/
		public int AgentsColumn1Pos
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsCol1Pos" + type, 0);

			set => settings.SetIntEntry("AgentsTab", "AgentsCol1Pos" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents column 2 width
		/// </summary>
		/********************************************************************/
		public int AgentsColumn2Width
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsCol2Width" + type, 192);

			set => settings.SetIntEntry("AgentsTab", "AgentsCol2Width" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents column 2 position
		/// </summary>
		/********************************************************************/
		public int AgentsColumn2Pos
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsCol2Pos" + type, 1);

			set => settings.SetIntEntry("AgentsTab", "AgentsCol2Pos" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents column 3 width
		/// </summary>
		/********************************************************************/
		public int AgentsColumn3Width
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsCol3Width" + type, 50);

			set => settings.SetIntEntry("AgentsTab", "AgentsCol3Width" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents column 3 position
		/// </summary>
		/********************************************************************/
		public int AgentsColumn3Pos
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsCol3Pos" + type, 2);

			set => settings.SetIntEntry("AgentsTab", "AgentsCol3Pos" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents sorted column
		/// </summary>
		/********************************************************************/
		public int AgentsSortKey
		{
			get => settings.GetIntEntry("AgentsTab", "AgentsSortKey" + type, 1);

			set => settings.SetIntEntry("AgentsTab", "AgentsSortKey" + type, value);
		}



		/********************************************************************/
		/// <summary>
		/// Agents sorting order
		/// </summary>
		/********************************************************************/
		public SortOrder AgentsSortOrder
		{
			get
			{
				if (Enum.TryParse(settings.GetStringEntry("AgentsTab", "AgentsSortOrder" + type, SortOrder.Ascending.ToString()), out SortOrder result))
					return result;

				return SortOrder.Ascending;
			}

			set => settings.SetStringEntry("AgentsTab", "AgentsSortOrder" + type, value.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// Description column 1 width
		/// </summary>
		/********************************************************************/
		public int DescriptionColumn1Width
		{
			get => settings.GetIntEntry("AgentsTab", "DescriptionCol1Width" + type, 265);

			set => settings.SetIntEntry("AgentsTab", "DescriptionCol1Width" + type, value);
		}
	}
}
