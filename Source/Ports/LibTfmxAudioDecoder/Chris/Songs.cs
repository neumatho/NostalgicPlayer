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
	/// 
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
		/// Some false positives look like valid song definitions, but define
		/// a false speed. It may be necessary to add some sort of blacklist
		/// based on checksum. Possibly MD5 then and not CRC
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

				// Skip invalid defs
				if ((s1 > s2) || (s1 >= 0x1ff) || (s2 >= 0x1ff))
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
				}

				// Avoid duplicates
				(c_int, c_int, c_int) a = new (s1, s2, s3);
				bool skipSong = false;

				foreach (var it in setSongArgs)
				{
					if ((it.Item1 == s1) && (s2 == it.Item1))
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
