/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibVorbisFile.Containers
{
	/// <summary>
	/// The function prototypes for the callbacks are basically the same as for
	/// the stdio functions fread, fseek, fclose, ftell.
	/// The one difference is that the FILE * arguments have been replaced with
	/// a void * - this is to be used as a pointer to whatever internal data these
	/// functions might need. In the stdio case, it's just a FILE * cast to a void *
	///
	/// If you use other functions, check the docs for these functions and return
	/// the right values. For seek_func(), you *MUST* return -1 if the stream is
	/// unseekable
	/// </summary>
	public class OvCallbacks
	{
		/// <summary>
		/// 
		/// </summary>
		public delegate size_t Read_Del(Pointer<byte> ptr, size_t size, size_t nmemb, object datasource);

		/// <summary>
		/// 
		/// </summary>
		public delegate c_int Seek_Del(object datasource, ogg_int64_t offset, SeekOrigin whence);

		/// <summary>
		/// 
		/// </summary>
		public delegate c_int Close_Del(object datasource);

		/// <summary>
		/// 
		/// </summary>
		public delegate c_long Tell_Del(object datasource);

		/// <summary>
		/// 
		/// </summary>
		public Read_Del Read_Func;

		/// <summary>
		/// 
		/// </summary>
		public Seek_Del Seek_Func;

		/// <summary>
		/// 
		/// </summary>
		public Close_Del Close_Func;

		/// <summary>
		/// 
		/// </summary>
		public Tell_Del Tell_Func;
	}
}
