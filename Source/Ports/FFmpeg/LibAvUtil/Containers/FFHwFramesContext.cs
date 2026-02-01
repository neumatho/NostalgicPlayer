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
	internal class FFHwFramesContext : AvHwFramesContext, IDataContext
	{
		/// <summary>
		/// The public AVHWFramesContext. See hwcontext.h for it
		/// </summary>
		public AvHwFramesContext P => this;

		/// <summary>
		/// 
		/// </summary>
		public HwContextType Hw_Type;

		/// <summary>
		/// 
		/// </summary>
		public AvBufferPool Pool_Internal;

		/// <summary>
		/// For a derived context, a reference to the original frames
		/// context it was derived from
		/// </summary>
		public AvBufferRef Source_Frames;

		/// <summary>
		/// Flags to apply to the mapping from the source to the derived
		/// frame context when trying to allocate in the derived context
		/// </summary>
		public c_int Source_Allocation_Map_Flags;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IDataContext MakeDeepClone()
		{
			return (FFHwFramesContext)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			base.CopyTo((AvHwFramesContext)destination);

			FFHwFramesContext dst = (FFHwFramesContext)destination;

			dst.Hw_Type = Hw_Type;
			dst.Pool_Internal = Pool_Internal;
			dst.Source_Frames = Source_Frames;
			dst.Source_Allocation_Map_Flags = Source_Allocation_Map_Flags;
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
			return new FFHwFramesContext();
		}
		#endregion
	}
}
