/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum AvIamfParamDefinitionType
	{
		/// <summary>
		/// Subblocks are of struct type AVIAMFMixGain
		/// </summary>
		Mix_Gain,

		/// <summary>
		/// Subblocks are of struct type AVIAMFDemixingInfo
		/// </summary>
		Demixing,

		/// <summary>
		/// Subblocks are of struct type AVIAMFReconGain
		/// </summary>
		Recon_Gain
	}
}
