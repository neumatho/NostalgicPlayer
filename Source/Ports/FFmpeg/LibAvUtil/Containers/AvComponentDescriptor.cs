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
	public class AvComponentDescriptor
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AvComponentDescriptor(c_int plane, c_int step, c_int offset, c_int shift, c_int depth)
		{
			Plane = plane;
			Step = step;
			Offset = offset;
			Shift = shift;
			Depth = depth;
		}



		/********************************************************************/
		/// <summary>
		/// Which of the 4 planes contains the component
		/// </summary>
		/********************************************************************/
		public c_int Plane { get; }



		/********************************************************************/
		/// <summary>
		/// Number of elements between 2 horizontally consecutive pixels.
		/// Elements are bits for bitstream formats, bytes otherwise
		/// </summary>
		/********************************************************************/
		public c_int Step { get; }



		/********************************************************************/
		/// <summary>
		/// Number of elements before the component of the first pixel.
		/// Elements are bits for bitstream formats, bytes otherwise
		/// </summary>
		/********************************************************************/
		public c_int Offset { get; }



		/********************************************************************/
		/// <summary>
		/// Number of least significant bits that must be shifted away
		/// to get the value
		/// </summary>
		/********************************************************************/
		public c_int Shift { get; }



		/********************************************************************/
		/// <summary>
		/// Number of bits in the component
		/// </summary>
		/********************************************************************/
		public c_int Depth { get; }
	}
}
