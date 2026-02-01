/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Holds a byte array as data context
	/// </summary>
	public class DataBufferContext : IDataContext
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DataBufferContext(CPointer<uint8_t> data, size_t size)
		{
			Data = data;
			Size = size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<uint8_t> Data { get; private set; }



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t Size { get; private set; }



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
			return new DataBufferContext(Data.MakeDeepClone(), Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			DataBufferContext dst = (DataBufferContext)destination;

			dst.Data = Data.MakeDeepClone();
			dst.Size = Size;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Allocate a new instance of this type
		/// </summary>
		/********************************************************************/
		private static IDataContext CreateInstance()
		{
			return new DataBufferContext(null, 0);
		}
		#endregion
	}
}
