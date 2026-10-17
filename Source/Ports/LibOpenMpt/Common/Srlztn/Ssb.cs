/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn
{
	/// <summary>
	/// 
	/// </summary>
	internal class Ssb
	{
		/// <summary>
		/// 
		/// </summary>
		protected Status m_Status;

		/// <summary>
		/// Read/write: If > 0, data entries have given fixed size
		/// </summary>
		protected uint32 m_nFixedEntrySize;

		/// <summary>
		/// Read/write: Stream position at the beginning of object
		/// </summary>
		protected long m_PosStart;

		/// <summary>
		/// Read/Write: Tells map ID entry size in bytes. If size is variable, value is IdSizeVariable
		/// </summary>
		protected uint16 m_nIdBytes;

		/// <summary>
		/// Read/write: Keeps count of entries written/read
		/// </summary>
		protected size_t m_nCounter;

		/// <summary>
		/// Read/write: Various flags
		/// </summary>
		protected readonly EnumBitSet<Rwf> m_Flags = new EnumBitSet<Rwf>(Rwf.NumFlags);

		/// <summary>
		/// 
		/// </summary>
		protected static readonly uint8[] s_EntryId = [ (uint8)'2', (uint8)'2', (uint8)'8' ];

		/// <summary>
		/// 
		/// </summary>
		protected const uint8 s_DefaultFlagByte = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Ssb()
		{
			m_Status = new Status();
			m_nFixedEntrySize = 0;
			m_PosStart = 0;
			m_nIdBytes = Serialization_Utils.IdSizeVariable;
			m_nCounter = 0;
			m_Flags.set(Rwf.WMapStartPosEntry);
			m_Flags.set(Rwf.WMapSizeEntry);
			m_Flags.set(Rwf.WVersionNum);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool HasFailed()
		{
			return m_Status.Level >= StatusLevel.Failure;
		}
	}
}
