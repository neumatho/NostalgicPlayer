/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces
{
	/// <summary>
	/// All objects that can be stored as data, should implement this interface
	/// </summary>
	public interface IDataContext : IDeepCloneable<IDataContext>, ICopyTo<IDataContext>
	{
		/// <summary>
		/// 
		/// </summary>
		UtilFunc.Alloc_DataContext_Delegate Allocator { get; }
	}
}
