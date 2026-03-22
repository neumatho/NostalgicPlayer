/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class LibXmp_Path : IDeepCloneable<LibXmp_Path>
	{
		public CPointer<char> Path;
		public size_t Length;
		public size_t Alloc;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public LibXmp_Path MakeDeepClone()
		{
			return (LibXmp_Path)MemberwiseClone();
		}
	}
}
