/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// The implementation of the SidInfo interface
	/// </summary>
	internal class SidInfoImpl : SidInfo
	{
		public string speedString;

		public uint maxSids = Mixer.MAX_SIDS;

		public uint channels = 1;

		public uint_least16_t driverAddr = 0;
		public uint_least16_t driverLength = 0;

		public uint_least16_t powerOnDelay = 0;

		public SidConfig.sid_model_t sidModel;

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Number of SIDs supported by this library
		/// </summary>
		/********************************************************************/
		protected override uint GetMaxSids()
		{
			return maxSids;
		}



		/********************************************************************/
		/// <summary>
		/// Number of output channels (1-mono, 2-stereo)
		/// </summary>
		/********************************************************************/
		protected override uint GetChannels()
		{
			return channels;
		}



		/********************************************************************/
		/// <summary>
		/// Address of the driver
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetDriverAddr()
		{
			return driverAddr;
		}



		/********************************************************************/
		/// <summary>
		/// Size of the driver
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetDriverLength()
		{
			return driverLength;
		}



		/********************************************************************/
		/// <summary>
		/// Power on delay
		/// </summary>
		/********************************************************************/
		protected override uint_least16_t GetPowerOnDelay()
		{
			return powerOnDelay;
		}



		/********************************************************************/
		/// <summary>
		/// Describes the speed current song is running at
		/// </summary>
		/********************************************************************/
		protected override string GetSpeedString()
		{
			return speedString;
		}



		/********************************************************************/
		/// <summary>
		/// SID model used by the engine
		/// </summary>
		/********************************************************************/
		protected override SidConfig.sid_model_t GetSidModel()
		{
			return sidModel;
		}
		#endregion
	}
}
