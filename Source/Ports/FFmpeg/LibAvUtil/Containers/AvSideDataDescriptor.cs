/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// This struct describes the properties of a side data type. Its instance
	/// corresponding to a given type can be obtained from av_frame_side_data_desc()
	/// </summary>
	public class AvSideDataDescriptor
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvSideDataDescriptor(string name, AvSideDataProps props)
		{
			Name = name.ToCharPointer();
			Props = props;
		}



		/********************************************************************/
		/// <summary>
		/// Human-readable side data description
		/// </summary>
		/********************************************************************/
		public CPointer<char> Name { get; }



		/********************************************************************/
		/// <summary>
		/// Side data property flags, a combination of AVSideDataProps values
		/// </summary>
		/********************************************************************/
		public AvSideDataProps Props { get; }
	}
}
