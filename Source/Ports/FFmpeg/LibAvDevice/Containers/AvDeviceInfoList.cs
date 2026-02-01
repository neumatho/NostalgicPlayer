/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvDevice.Containers
{
	/// <summary>
	/// List of devices
	/// </summary>
	public class AvDeviceInfoList
	{
		/// <summary>
		/// List of autodetected devices
		/// </summary>
		public CPointer<AvDeviceInfo> Devices;

		/// <summary>
		/// Number of autodetected devices
		/// </summary>
		public c_int Nb_Devices;

		/// <summary>
		/// Index of default device or -1 if no default
		/// </summary>
		public c_int Default_Device;
	}
}
