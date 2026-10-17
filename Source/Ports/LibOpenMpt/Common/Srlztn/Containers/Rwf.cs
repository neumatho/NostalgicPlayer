/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum Rwf
	{
		/// <summary>
		/// Write. True to include data start pos entry to map
		/// </summary>
		WMapStartPosEntry,

		/// <summary>
		/// Write. True to include data size entry to map
		/// </summary>
		WMapSizeEntry,

		/// <summary>
		/// Write. True to include description entry to map
		/// </summary>
		WMapDescEntry,

		/// <summary>
		/// Write. True to include version numeric
		/// </summary>
		WVersionNum,

		/// <summary>
		/// Read. True if map has been cached
		/// </summary>
		RMapCached,

		/// <summary>
		/// Read. True if map has IDs
		/// </summary>
		RMapHasId,

		/// <summary>
		/// Read. True if map data start pos
		/// </summary>
		RMapHasStartPos,

		/// <summary>
		/// Read. True if map has entry size
		/// </summary>
		RMapHasSize,

		/// <summary>
		/// Read. True if map has entry description
		/// </summary>
		RMapHasDesc,

		/// <summary>
		/// Read. True if map description characters are two bytes
		/// </summary>
		RTwoBytesDescChar,

		/// <summary>
		/// Read. True when header is read
		/// </summary>
		RHeaderIsRead,

		/// <summary>
		/// Read/write. True if map exists
		/// </summary>
		RwHasMap,


		/// <summary>
		/// 
		/// </summary>
		NumFlags
	}
}
