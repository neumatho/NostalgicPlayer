/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.SoundMon.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundMon
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class SoundMonIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "bp", "bp1", "bp2", "bp3" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Check the module size
			if (moduleStream.Length < 512)
				return ModuleType.Unknown;

			// Read the module mark
			moduleStream.Seek(26, SeekOrigin.Begin);
			string mark4 = moduleStream.ReadMark(4);
			string mark3 = mark4.Substring(0, 3);

			// Check the mark
			if (mark4 == "BPSM")
				return ModuleType.SoundMon10;

			if (mark3 == "V.2")
				return IsSoundMon21Module(moduleStream) ? ModuleType.SoundMon21 : ModuleType.SoundMon11;

			if (mark3 == "V.3")
				return ModuleType.SoundMon22;

			return ModuleType.Unknown;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Both SoundMon 1.1/2.0 and 2.1 write the same mark in the modules,
		/// but some effects have changed meaning. This method looks at how
		/// the effects are used, to find out which version the module is
		/// made for. If nothing tells which version it is, version 2.1 is
		/// returned, since that is what the author's own later players and
		/// editor (2.2) do with these modules
		/// </summary>
		/********************************************************************/
		private static bool IsSoundMon21Module(ModuleStream moduleStream)
		{
			moduleStream.Seek(30, SeekOrigin.Begin);
			ushort numberOfSteps = moduleStream.Read_B_UINT16();

			// Read the track numbers from the step list and find all the used tracks
			moduleStream.Seek(512, SeekOrigin.Begin);

			ushort[] stepTracks = new ushort[numberOfSteps * 4];
			HashSet<ushort> usedTracks = new HashSet<ushort>();
			ushort maxTrack = 0;

			for (int i = 0; i < numberOfSteps * 4; i++)
			{
				ushort trackNumber = moduleStream.Read_B_UINT16();
				moduleStream.Seek(2, SeekOrigin.Current);

				stepTracks[i] = trackNumber;
				usedTracks.Add(trackNumber);

				if (trackNumber > maxTrack)
					maxTrack = trackNumber;
			}

			if (moduleStream.EndOfStream)
				return true;

			// Read all the tracks
			byte[] trackData = new byte[maxTrack * 16 * 3];
			int tracksRead = moduleStream.Read(trackData, 0, trackData.Length) / (16 * 3);

			// Effect E and F were introduced in version 2.1. Also count how
			// many times effect 6 is used as vibrato
			int vibratoVotes = 0;

			for (ushort i = 1; i <= tracksRead; i++)
			{
				if (!usedTracks.Contains(i))
					continue;

				int offset = (i - 1) * 16 * 3;

				for (int j = 0; j < 16; j++)
				{
					Optional optional = (Optional)(trackData[offset + (j * 3) + 1] & 0x0f);
					byte optionalData = trackData[offset + (j * 3) + 2];

					if (optional >= Optional.ResetAdsr)
						return true;

					if ((optional == Optional.SetRepCount) && (optionalData != 0) && !IsRepeatCount(optionalData))
						vibratoVotes++;
				}
			}

			// In version 1.1/2.0, a repeat count followed by a jump is used to
			// make a loop. The two effects can be in different tracks and
			// voices, so look at them in the order the rows are played
			int repeatVotes = 0;
			bool repeatCountSet = false;

			for (int i = 0; i < numberOfSteps; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					for (int k = 0; k < 4; k++)
					{
						ushort trackNumber = stepTracks[i * 4 + k];
						if ((trackNumber == 0) || (trackNumber > tracksRead))
							continue;

						int offset = ((trackNumber - 1) * 16 + j) * 3;

						Optional optional = (Optional)(trackData[offset + 1] & 0x0f);
						byte optionalData = trackData[offset + 2];

						if ((optional == Optional.SetRepCount) && IsRepeatCount(optionalData))
							repeatCountSet = true;
						else if ((optional == Optional.DbraRepCount) && repeatCountSet)
						{
							repeatVotes++;
							repeatCountSet = false;
						}
					}
				}
			}

			return (repeatVotes == 0) || (repeatVotes < vibratoVotes);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the given effect 6 value looks like a repeat count. In
		/// version 1.1/2.0, a small count or 0xff (in practice an endless
		/// jump) is used. As vibrato, these values are either way too much
		/// or nothing at all
		/// </summary>
		/********************************************************************/
		private static bool IsRepeatCount(byte optionalData)
		{
			return ((optionalData >= 1) && (optionalData <= 4)) || (optionalData == 0xff);
		}
		#endregion
	}
}
