/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.SidPlayFp
{
	/// <summary>
	/// This interface is used to get SID engine information
	/// </summary>
	internal abstract class SidInfo
	{
		/********************************************************************/
		/// <summary>
		/// Number of SIDs supported by this library
		/// </summary>
		/********************************************************************/
		public uint MaxSids()
		{
			return GetMaxSids();
		}



		/********************************************************************/
		/// <summary>
		/// Number of output channels (1-mono, 2-stereo)
		/// </summary>
		/********************************************************************/
		public uint Channels()
		{
			return GetChannels();
		}



		/********************************************************************/
		/// <summary>
		/// Address of the driver
		/// </summary>
		/********************************************************************/
		public uint_least16_t DriverAddr()
		{
			return GetDriverAddr();
		}



		/********************************************************************/
		/// <summary>
		/// Size of the driver
		/// </summary>
		/********************************************************************/
		public uint_least16_t DriverLength()
		{
			return GetDriverLength();
		}



		/********************************************************************/
		/// <summary>
		/// Power on delay
		/// </summary>
		/********************************************************************/
		public uint_least16_t PowerOnDelay()
		{
			return GetPowerOnDelay();
		}



		/********************************************************************/
		/// <summary>
		/// Describes the speed current song is running at
		/// </summary>
		/********************************************************************/
		public string SpeedString()
		{
			return GetSpeedString();
		}



		/********************************************************************/
		/// <summary>
		/// SID model used by the engine
		/// </summary>
		/********************************************************************/
		public SidConfig.sid_model_t SidModel()
		{
			return GetSidModel();
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Number of SIDs supported by this library
		/// </summary>
		/********************************************************************/
		protected abstract uint GetMaxSids();



		/********************************************************************/
		/// <summary>
		/// Number of output channels (1-mono, 2-stereo)
		/// </summary>
		/********************************************************************/
		protected abstract uint GetChannels();



		/********************************************************************/
		/// <summary>
		/// Address of the driver
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetDriverAddr();



		/********************************************************************/
		/// <summary>
		/// Size of the driver
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetDriverLength();



		/********************************************************************/
		/// <summary>
		/// Power on delay
		/// </summary>
		/********************************************************************/
		protected abstract uint_least16_t GetPowerOnDelay();



		/********************************************************************/
		/// <summary>
		/// Describes the speed current song is running at
		/// </summary>
		/********************************************************************/
		protected abstract string GetSpeedString();



		/********************************************************************/
		/// <summary>
		/// SID model used by the engine
		/// </summary>
		/********************************************************************/
		protected abstract SidConfig.sid_model_t GetSidModel();
		#endregion
	}
}
