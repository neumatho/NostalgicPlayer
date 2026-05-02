/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// Some false positives look like valid song definitions, but define
	/// a false speed. It may be necessary to add some sort of blacklist
	/// based on checksum. Possibly MD5 then and not CRC.
	///
	/// There are song definitions that define start/end steps outside
	/// the range of the sequencer's track table. For example,
	/// Jim Power (End Level):
	/// Track table length is $e0, so the end step must be at most $e,
	/// but subsong is defined as $e to $f which would be behind the end
	/// of the track table. The inaccessible pattern data between end of
	/// track table and beginning of the actual first pattern are not
	/// helpful in that case
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetSongs()
		{
			return vSongs.Count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FindSongs()
		{
			HashSet<(c_int, c_int, c_int)> setSongArgs = new HashSet<(c_int, c_int, c_int)>();

			// Examine all (sub-)song definitions
			vSongs.Clear();

			for (c_int so = 0; so < 32; so++)
			{
				c_int s1 = MyEndian.ReadBEUword(pBuf, (udword)(offsets.Header + 0x100 + (so << 1)));
				c_int s2 = MyEndian.ReadBEUword(pBuf, (udword)(offsets.Header + 0x140 + (so << 1)));
				c_int s3 = MyEndian.ReadBEUword(pBuf, (udword)(offsets.Header + 0x180 + (so << 1)));
				c_int s1Next = s1;
				uword stepMax = (uword)((offsets.TrackTableEnd - offsets.TrackTable) / 0x10);

				// If the first song's track end is out of bounds, fix it
				if ((so == 0) && (s2 > stepMax))
					s2 = stepMax;

				// Skip invalid defs
				// Largest track number $1ff is not invalid per se,
				// but (1ff,1ff,0) was the dummy placeholder entry for song 32,
				// and some files have changed it to (0,1ff,5).
				// Yet we cannot reject (0,1ff,SPEED) entirely, since e.g.
				// the composer Erno used that in the first song definition
				if ((s1 > s2) || (s1 > 0x1ff) || (s2 > 0x1ff) || (s1 >= stepMax) || ((so > 0) && ((s1 == 0x1ff) || (s2 == 0x1ff))))
					continue;

				// First step == last step isn't invalid per se,
				// but in corner-cases the tracks don't advance either
				if (s1 == s2)
				{
					playerInfo.Sequencer.Step.Current = playerInfo.Sequencer.Step.First = s1;
					playerInfo.Sequencer.Step.Last = s2;

					ProcessTrackStep();

					c_int countInactive = 0;

					for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
					{
						Track tr = playerInfo.Track[t];

						if (tr.Pt >= 0x90)
							countInactive++;
					}

					if (countInactive == playerInfo.Sequencer.Tracks)
						continue;

					if (s1 != playerInfo.Sequencer.Step.Current)
						s1Next = playerInfo.Sequencer.Step.Current;
				}

				// Avoid two types of duplicates.
				//
				// 1: All like (0,0,5) and basically (X,X,SPEED) where
				//    a previously accepted song had the same start step X already.
				// 2: Exact dupes of (X,Y,SPEED) will be skipped, too
				(c_int, c_int, c_int) a = new (s1, s2, s3);
				bool skipSong = false;

				foreach (var it in setSongArgs)
				{
					if (((it.Item1 == s1) && (s2 == it.Item1)) ||
						// Also ignore songs which immediately advance to
						// the start step of a previously seen song def
						((s1Next != s1) && (it.Item1 == s1Next)))
					{
						skipSong = true;
						break;
					}
				}

				if (!skipSong && !setSongArgs.Contains(a))
				{
					vSongs.Add((ubyte)so);
					setSongArgs.Add(a);
				}
			}
		}
	}
}
