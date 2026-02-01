/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class HwMapDescriptor : IDataContext
	{
		/// <summary>
		/// A reference to the original source of the mapping
		/// </summary>
		public AvFrame Source;

		/// <summary>
		/// A reference to the hardware frames context in which this
		/// mapping was made. May be the same as source->hw_frames_ctx,
		/// but need not be
		/// </summary>
		public AvBufferRef Hw_Frames_Ctx;

		/// <summary>
		/// Unmap function
		/// </summary>
		public UtilFunc.Unmap_Delegate Unmap;

		/// <summary>
		/// Hardware-specific private data associated with the mapping
		/// </summary>
		public object Priv;//XX

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IDataContext MakeDeepClone()
		{
			return (HwMapDescriptor)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			HwMapDescriptor dst = (HwMapDescriptor)destination;

			dst.Source = Source;
			dst.Hw_Frames_Ctx = Hw_Frames_Ctx;
			dst.Unmap = Unmap;
			dst.Priv = Priv;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public UtilFunc.Alloc_DataContext_Delegate Allocator => CreateInstance;

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate a new instance of this type
		/// </summary>
		/********************************************************************/
		private static IDataContext CreateInstance()
		{
			return new HwMapDescriptor();
		}
		#endregion
	}
}
