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
	/// Holds a string as data context
	/// </summary>
	public class StringBufferContext : IDataContext
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StringBufferContext(CPointer<char> str, size_t size)
		{
			String = str;
			Size = size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CPointer<char> String { get; private set; }



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
			return new StringBufferContext(String.MakeDeepClone(), Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyTo(IDataContext destination)
		{
			StringBufferContext dst = (StringBufferContext)destination;

			dst.String = String.MakeDeepClone();
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
			return new StringBufferContext(null, 0);
		}
		#endregion
	}
}
