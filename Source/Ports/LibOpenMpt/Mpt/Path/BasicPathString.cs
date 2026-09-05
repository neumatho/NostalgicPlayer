/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Path
{
	/// <summary>
	/// 
	/// </summary>
	internal class BasicPathString<Traits> : ISharedFileNameType
	{
		private readonly bool _allow_Transcode_Local;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public BasicPathString(bool allow_Transcode_Local)
		{
			_allow_Transcode_Local = allow_Transcode_Local;
		}
	}
}
