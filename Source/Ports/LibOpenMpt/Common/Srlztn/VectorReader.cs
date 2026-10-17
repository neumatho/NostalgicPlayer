/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C.Std;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn
{
	/// <summary>
	/// 
	/// </summary>
	internal readonly struct VectorReader<T> where T : unmanaged
	{
		private readonly size_t m_nCount;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public VectorReader(size_t nCount)
		{
			m_nCount = nCount;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Read(Stream iStrm, vector<T> vec)
		{
			vec.resize(m_nCount);

			for (size_t i = 0; i < m_nCount; ++i)
				Serialization_Utils.BinaryRead(iStrm, out vec[i]);
		}
	}
}
