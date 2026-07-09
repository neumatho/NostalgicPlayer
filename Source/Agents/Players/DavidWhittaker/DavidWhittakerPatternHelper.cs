/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker
{
	/// <summary>
	/// Helper class for converting David Whittaker patterns to SongPatternViewData
	/// </summary>
	internal static class DavidWhittakerPatternHelper
	{
		/********************************************************************/
		/// <summary>
		/// Create row change info for pattern viewer
		/// </summary>
		/********************************************************************/
		public static SongRowChangeInfo CreateRowChangeInfo(int currentPosition, int currentRow, ushort speed,
			PositionList[] positionLists, Dictionary<uint, int> trackNumbers, int channelCount, int[] channelPositions)
		{
			// Get track numbers for each channel at current position
			uint?[] channelTracks = null;
			if (positionLists != null && trackNumbers != null && currentPosition >= 0)
			{
				channelTracks = new uint?[channelCount];
				for (int channel = 0; channel < channelCount; channel++)
					if (channel < positionLists.Length && currentPosition < positionLists[channel].TrackOffsets.Length)
					{
						uint trackOffset = positionLists[channel].TrackOffsets[currentPosition];
						if (trackNumbers.ContainsKey(trackOffset))
							channelTracks[channel] = (uint)trackNumbers[trackOffset];
					}
			}

			// Convert channel positions to uint array
			uint[] channelPosArray = null;
			if (channelPositions != null && channelPositions.Length == channelCount)
			{
				channelPosArray = new uint[channelCount];
				for (int i = 0; i < channelCount; i++)
					channelPosArray[i] = (uint)(channelPositions[i] > 0 ? channelPositions[i] - 1 : 0);
			}

			return new SongRowChangeInfo
			{
				SongPosition = currentPosition,
				Row = currentRow,
				Speed = speed,
				Bpm = null, // No BPM support
				ChannelTracks = channelTracks,
				ChannelPositions = channelPosArray
			};
		}
	}
}
