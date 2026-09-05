/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class FileDataSeekable : IFileData
	{
		private readonly size_t streamLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FileDataSeekable(size_t streamLength_)
		{
			streamLength = streamLength_;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IsValid()
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool HasPinnedView()
		{
			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override CPointer<byte> GetRawData()
		{
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override size_t GetLength()
		{
			return streamLength;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span Read(size_t pos, byte_span dst)
		{
			return InternalReadSeekable(pos, dst);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span2 Read(size_t pos, byte_span2 dst)
		{
			return InternalReadSeekable(pos, dst);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract byte_span InternalReadSeekable(size_t pos, byte_span dst);



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract byte_span2 InternalReadSeekable(size_t pos, byte_span2 dst);
		#endregion
	}
}
