/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Stereo 3D type: this structure describes how two videos are packed
	/// within a single video surface, with additional information as needed.
	///
	/// Note: The struct must be allocated with av_stereo3d_alloc() and
	///       its size is not a part of the public ABI.
	/// </summary>
	public class AvStereo3D : IDataContext
	{
		/// <summary>
		/// How views are packed within the video
		/// </summary>
		public AvStereo3DType Type;

		/// <summary>
		/// Additional information about the frame packing
		/// </summary>
		public AvStereo3DFlag Flags;

		/// <summary>
		/// Determines which views are packed
		/// </summary>
		public AvStereo3DView View;

		/// <summary>
		/// Which eye is the primary eye when rendering in 2D
		/// </summary>
		public AvStereo3DPrimaryEye Primary_Eye;

		/// <summary>
		/// The distance between the centres of the lenses of the camera system,
		/// in micrometers. Zero if unset
		/// </summary>
		public uint32_t Baseline;

		/// <summary>
		/// Relative shift of the left and right images, which changes the zero parallax plane.
		/// Range is -1.0 to 1.0. Zero if unset
		/// </summary>
		public AvRational Horizontal_Disparity_Adjustment;

		/// <summary>
		/// Horizontal field of view, in degrees. Zero if unset
		/// </summary>
		public AvRational Horizontal_Field_Of_View;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public UtilFunc.Alloc_DataContext_Delegate Allocator => CreateInstance;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IDataContext MakeDeepClone()
		{
			return (AvStereo3D)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			AvStereo3D dest = (AvStereo3D)destination;

			dest.Type = Type;
			dest.Flags = Flags;
			dest.View = View;
			dest.Primary_Eye = Primary_Eye;
			dest.Baseline = Baseline;
			dest.Horizontal_Disparity_Adjustment = Horizontal_Disparity_Adjustment;
			dest.Horizontal_Field_Of_View = Horizontal_Field_Of_View;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate a new instance of this type
		/// </summary>
		/********************************************************************/
		private static IDataContext CreateInstance()
		{
			return new AvStereo3D();
		}
		#endregion
	}
}
