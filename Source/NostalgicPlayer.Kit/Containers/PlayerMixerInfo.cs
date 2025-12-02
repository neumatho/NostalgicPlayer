/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds different mixer configurations your player need
	/// to take care of, if BufferDirect mode is used
	/// </summary>
	public class PlayerMixerInfo
	{
		/********************************************************************/
		/// <summary>
		/// The stereo separation in percent
		/// </summary>
		/********************************************************************/
		public int StereoSeparator
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if interpolation is enabled
		/// </summary>
		/********************************************************************/
		public InterpolationMode InterpolationMode
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate which surround mode to use
		/// </summary>
		/********************************************************************/
		public SurroundMode SurroundMode
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// Holds an array telling which channels to enable/disable. If null,
		/// all channels are enabled
		/// </summary>
		/********************************************************************/
		public bool[] ChannelsEnabled
		{
			get;
		} = new bool[IChannel.MaxNumberOfChannels];
	}
}
