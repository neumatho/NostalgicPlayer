/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvDevice.Containers
{
	/// <summary>
	/// Structure describes basic parameters of the device
	/// </summary>
	public class AvDeviceInfo
	{
		/// <summary>
		/// Device name, format depends on device
		/// </summary>
		public CPointer<char> Device_Name;

		/// <summary>
		/// Human friendly name
		/// </summary>
		public CPointer<char> Device_Description;

		/// <summary>
		/// Array indicating what media types(s), if any, a device can provide. If null, cannot provide any
		/// </summary>
		public CPointer<AvMediaType> Media_Types;

		/// <summary>
		/// Length of media_types array, 0 if device cannot provide any media types
		/// </summary>
		public c_int Nb_Media_Types;
	}
}
