/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// This class holds all static information about the player
	/// </summary>
	public class ModuleInfoStatic
	{
		private readonly ModulePlayerSupportFlag moduleSupportFlag;
		private readonly SamplePlayerSupportFlag sampleSupportFlag;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic()
		{
			ModuleName = string.Empty;
			Author = string.Empty;
			ModuleFormat = string.Empty;
			PlayerName = string.Empty;
			Channels = 0;
			ModuleSize = 0;

			moduleSupportFlag = ModulePlayerSupportFlag.None;
			sampleSupportFlag = SamplePlayerSupportFlag.None;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ModuleInfoStatic(AgentInfo playerAgentInfo, string moduleName, string author, string[] comment, string moduleFormat, string playerName, int channels, long moduleSize)
		{
			PlayerAgentInfo = playerAgentInfo;
			ModuleName = moduleName;
			Author = author;
			Comment = comment;
			ModuleFormat = moduleFormat;
			PlayerName = playerName;
			Channels = channels;
			ModuleSize = moduleSize;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for module players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(AgentInfo playerAgentInfo, AgentInfo converterAgentInfo, string moduleName, string author, string[] comment, string moduleFormat, string playerName, int channels, long moduleSize, ModulePlayerSupportFlag supportFlag, int maxSongNumber, InstrumentInfo[] instruments, SampleInfo[] samples) : this(playerAgentInfo, moduleName, author, comment, moduleFormat, playerName, channels, moduleSize)
		{
			moduleSupportFlag = supportFlag;
			sampleSupportFlag = SamplePlayerSupportFlag.None;

			ConverterAgentInfo = converterAgentInfo;

			MaxSongNumber = maxSongNumber;
			Instruments = instruments;
			Samples = samples;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor (for sample players)
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(AgentInfo playerAgentInfo, string moduleName, string author, string[] comment, string moduleFormat, string playerName, int channels, long moduleSize, SamplePlayerSupportFlag supportFlag, int frequency) : this(playerAgentInfo, moduleName, author, comment, moduleFormat, playerName, channels, moduleSize)
		{
			sampleSupportFlag = supportFlag;
			moduleSupportFlag = ModulePlayerSupportFlag.None;

			Frequency = frequency;
		}

		#region Common properties
		/********************************************************************/
		/// <summary>
		/// Returns agent information about the player in use
		/// </summary>
		/********************************************************************/
		public AgentInfo PlayerAgentInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the module
		/// </summary>
		/********************************************************************/
		public string ModuleName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the name of the author
		/// </summary>
		/********************************************************************/
		public string Author
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Returns the comment separated into lines
		/// </summary>
		/********************************************************************/
		public string[] Comment
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the format of the module
		/// </summary>
		/********************************************************************/

		public string ModuleFormat
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the player
		/// </summary>
		/********************************************************************/
		public string PlayerName
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the module
		/// </summary>
		/********************************************************************/
		public long ModuleSize
		{
			get;
		}
		#endregion

		#region Module specific properties
		/********************************************************************/
		/// <summary>
		/// Returns agent information about the converted that has been used
		/// </summary>
		/********************************************************************/
		public AgentInfo ConverterAgentInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return the maximum number of songs in the current module
		/// </summary>
		/********************************************************************/
		public int MaxSongNumber
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether it is possible to change the position
		/// </summary>
		/********************************************************************/
		public bool CanChangePosition
		{
			get
			{
				return ((moduleSupportFlag & ModulePlayerSupportFlag.SetPosition) != 0) || ((sampleSupportFlag & SamplePlayerSupportFlag.SetPosition) != 0);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return all the instruments in the module
		/// </summary>
		/********************************************************************/
		public InstrumentInfo[] Instruments
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the samples in the module
		/// </summary>
		/********************************************************************/
		public SampleInfo[] Samples
		{
			get;
		}
		#endregion

		#region Sample specific properties
		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored as
		/// </summary>
		/********************************************************************/
		public int Frequency
		{
			get;
		}
		#endregion
	}
}
