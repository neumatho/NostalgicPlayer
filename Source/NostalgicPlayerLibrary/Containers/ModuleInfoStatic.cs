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
		private readonly ModulePlayerSupportFlag supportFlag;

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
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ModuleInfoStatic(string moduleName, string author, string moduleFormat, string playerName, int channels, long moduleSize)
		{
			ModuleName = moduleName;
			Author = author;
			ModuleFormat = moduleFormat;
			PlayerName = playerName;
			Channels = channels;
			ModuleSize = moduleSize;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		internal ModuleInfoStatic(string moduleName, string author, string moduleFormat, string playerName, int channels, long moduleSize, ModulePlayerSupportFlag supportFlag, int maxSongNumber) : this(moduleName, author, moduleFormat, playerName, channels, moduleSize)
		{
			this.supportFlag = supportFlag;
			MaxSongNumber = maxSongNumber;
		}

		#region Common properties
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
				return (supportFlag & ModulePlayerSupportFlag.SetPosition) != 0;
			}
		}
		#endregion
	}
}
