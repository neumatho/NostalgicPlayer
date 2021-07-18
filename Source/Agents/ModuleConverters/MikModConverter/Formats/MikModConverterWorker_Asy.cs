/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Containers;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.MikModConverter.Formats
{
	/// <summary>
	/// MikMod loader for AMF (Asylum) format
	/// </summary>
	internal class MikModConverterWorker_Asy : MikModConverterWorkerBase
	{
		#region MSampInfo class
		private class MSampInfo
		{
			public readonly byte[] SampleName = new byte[23];
			public byte FineTune;
			public byte Volume;
			public uint Length;
			public uint RepPos;
			public uint RepLen;
		}
		#endregion

		#region ModuleHeader class
		private class ModuleHeader
		{
			public readonly byte[] SongName = new byte[22];
			public byte NumPatterns;									// Number of patterns used
			public byte NumOrders;
			public byte[] Positions = new byte[256];					// Which pattern to play at pos
			public readonly MSampInfo[] Samples = Helpers.InitializeArray<MSampInfo>(64);	// All sample info
		}
		#endregion

		#region ModType class
		private class ModType
		{
			public readonly byte[] Id = new byte[5];
//			public byte Channels;
//			public string Name;
		}
		#endregion

		#region ModNote class
		public class ModNote
		{
			public byte A;
			public byte B;
			public byte C;
			public byte D;
		}
		#endregion

		/* This table is taken from AMF2MOD.C
		 * written in 1995 by Mr. P / Powersource
		 * mrp@fish.share.net, ac054@sfn.saskatoon.sk.ca */
		private static readonly ushort[] periodTable =
		{
			6848, 6464, 6096, 5760, 5424, 5120, 4832, 4560, 4304,
			4064, 3840, 3628, 3424, 3232, 3048, 2880, 2712, 2560,
			2416, 2280, 2152, 2032, 1920, 1814, 1712, 1616, 1524,
			1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  907,
			856,   808,  762,  720,  678,  640,  604,  570,  538,
			508,   480,  453,  428,  404,  381,  360,  339,  320,
			302,   285,  269,  254,  240,  226,  214,  202,  190,
			180,   170,  160,  151,  143,  135,  127,  120,  113,
			107,   101,   95,   90,   85,   80,   75,   71,   67,
			63,     60,   56,   53,   50,   47,   45,   42,   40,
			37,     35,   33,   31,   30,   28
		};

		private ModuleHeader mh;
		private ModNote[] patBuf;

		private int modType;

		#region MikModConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			long fileSize = moduleStream.Length;
			if (fileSize < 38)								// Size of header
				return AgentResult.Unknown;

			// Read the magic string
			moduleStream.Seek(0, SeekOrigin.Begin);

			byte[] buf = new byte[24];
			moduleStream.Read(buf, 0, 24);

			if (Encoding.ASCII.GetString(buf, 0, 24) == "ASYLUM Music Format V1.0")
			{
				modType = 1;
				return AgentResult.Ok;
			}

			return AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		protected override bool LoadModule(ModuleStream moduleStream, MUniTrk uniTrk, out string errorMessage)
		{
			errorMessage = string.Empty;

			mh = new ModuleHeader();

			try
			{
				Encoding encoder = EncoderCollection.Dos;

				// No title in Asylum amf files :(
				mh.SongName[0] = 0x00;

				moduleStream.Seek(0x23, SeekOrigin.Begin);
				mh.NumPatterns = moduleStream.Read_UINT8();
				mh.NumOrders = moduleStream.Read_UINT8();

				// Skip unknown byte
				moduleStream.Seek(1, SeekOrigin.Current);

				moduleStream.Read(mh.Positions, 0, 256);

				// Read sample headers
				for (int t = 0; t < 64; t++)
				{
					MSampInfo s = mh.Samples[t];

					moduleStream.Seek(0x126 + (t * 37), SeekOrigin.Begin);

					moduleStream.ReadString(s.SampleName, 22);

					s.FineTune = moduleStream.Read_UINT8();
					s.Volume = moduleStream.Read_UINT8();

					// Skip unknown byte
					moduleStream.Seek(1, SeekOrigin.Current);

					s.Length = moduleStream.Read_L_UINT32();
					s.RepPos = moduleStream.Read_L_UINT32();
					s.RepLen = moduleStream.Read_L_UINT32();
				}

				if (moduleStream.EndOfStream)
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				// Set module variables
				of.InitSpeed = 6;
				of.InitTempo = 125;
				of.NumChn = 8;

				modType = 0;

				of.SongName = encoder.GetString(mh.SongName);
				of.NumPos = mh.NumOrders;
				of.RepPos = 0;
				of.NumPat = mh.NumPatterns;
				of.NumTrk = (ushort)(of.NumPat * of.NumChn);

				// Copy positions (orders)
				if (!MLoader.AllocPositions(of, of.NumPos))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_HEADER;
					return false;
				}

				for (int t = 0; t < of.NumPos; t++)
				{
					of.Positions[t] = mh.Positions[t];

					// Sanity check
					if (of.Positions[t] > of.NumPat)
					{
						errorMessage = Resources.IDS_MIKCONV_ERR_BAD_HEADER;
						return false;
					}
				}

				// Finally, init the sample info structures
				of.NumIns = 31;
				of.NumSmp = 31;

				if (!MLoader.AllocSamples(of))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_SAMPLEINFO;
					return false;
				}

				uint seekPos = (uint)(2662 + (2048 * of.NumPat));
				for (int t = 0; t < of.NumIns; t++)
				{
					MSampInfo s = mh.Samples[t];
					Sample q = of.Samples[t];

					// Convert sample name
					q.SampleName = encoder.GetString(s.SampleName);

					// Init the sample info variables
					q.Speed = SharedLookupTables.FineTune[s.FineTune & 0xf];
					q.Volume = (byte)(s.Volume & 0x7f);

					q.LoopStart = s.RepPos;
					q.LoopEnd = q.LoopStart + s.RepLen;
					q.Length = s.Length;

					q.Flags = SampleFlag.Signed;

					q.SeekPos = seekPos;
					seekPos += q.Length;

					if (s.RepLen > 2)
						q.Flags |= SampleFlag.Loop;

					// Fix replen if repend > length
					if (q.LoopEnd > q.Length)
						q.LoopEnd = q.Length;
				}

				if (!LoadPatterns(moduleStream, uniTrk))
				{
					errorMessage = Resources.IDS_MIKCONV_ERR_LOADING_PATTERNS;
					return false;
				}
			}
			finally
			{
				mh = null;
				patBuf = null;
			}

			return true;
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Loads all the patterns of a module and converts them into 3 byte
		/// format
		/// </summary>
		/********************************************************************/
		private bool LoadPatterns(ModuleStream moduleStream, MUniTrk uniTrk)
		{
			if (!MLoader.AllocPatterns(of))
				return false;

			if (!MLoader.AllocTracks(of))
				return false;

			// Allocate temporary buffer for loading and converting the patterns
			patBuf = Helpers.InitializeArray<ModNote>(64 * of.NumChn);

			// Patterns start here
			moduleStream.Seek(0xa66, SeekOrigin.Begin);

			int tracks = 0;
			for (int t = 0; t < of.NumPat; t++)
			{
				// Load the pattern into the temp buffer and convert it
				for (int s = 0; s < (64 * of.NumChn); s++)
				{
					patBuf[s].A = moduleStream.Read_UINT8();
					patBuf[s].B = moduleStream.Read_UINT8();
					patBuf[s].C = moduleStream.Read_UINT8();
					patBuf[s].D = moduleStream.Read_UINT8();
				}

				for (int s = 0; s < of.NumChn; s++)
				{
					if ((of.Tracks[tracks++] = ConvertTrack(patBuf, s, uniTrk)) == null)
						return false;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Convert a whole track
		/// </summary>
		/********************************************************************/
		private byte[] ConvertTrack(ModNote[] n, int offset, MUniTrk uniTrk)
		{
			uniTrk.UniReset();

			for (int t = 0; t < 64; t++)
			{
				if (!ConvertNote(n[offset], uniTrk))
					return null;

				uniTrk.UniNewLine();

				offset += of.NumChn;
			}

			return uniTrk.UniDup();
		}



		/********************************************************************/
		/// <summary>
		/// Convert a single note
		/// </summary>
		/********************************************************************/
		private bool ConvertNote(ModNote n, MUniTrk uniTrk)
		{
			byte instrument = (byte)(n.B & 0x1f);
			byte effect = n.C;
			byte effDat = n.D;

			// Convert amf note to mod period
			ushort period;
			if (n.A != 0)
				period = periodTable[n.A];
			else
				period = 0;

			// Convert the period to a note number
			byte note = 0;
			if (period != 0)
			{
				for (note = 0; note < 7 * SharedConstant.Octave; note++)
				{
					if (period >= LookupTables.NPerTab[note])
						break;
				}

				if (note == 7 * SharedConstant.Octave)
					note = 0;
				else
					note++;
			}

			if (instrument != 0)
			{
				// If instrument does not exists, note cut
				if ((instrument > 31) || (mh.Samples[instrument - 1].Length == 0))
				{
					uniTrk.UniPtEffect(0xc, 0, of.Flags);
					if (effect == 0xc)
						effect = effDat = 0;
				}
				else
				{
					// Protracker handling
					if (modType == 0)
					{
						// If we had a note, then change instrument...
						if (note != 0)
							uniTrk.UniInstrument((ushort)(instrument - 1));
						else	// Otherwise, only adjust volume...
						{
							// ...unless an effect was specified,
							// which forces a new note to be
							// played
							if ((effect != 0) || (effDat != 0))
								uniTrk.UniInstrument((ushort)(instrument - 1));
							else
								uniTrk.UniPtEffect(0xc, (byte)(mh.Samples[instrument - 1].Volume & 0x7f), of.Flags);
						}
					}
					else
					{
						// Fasttracker handling
						uniTrk.UniInstrument((ushort)(instrument - 1));
					}
				}
			}

			if (note != 0)
				uniTrk.UniNote((ushort)(note + 2 * SharedConstant.Octave - 1));

			// Convert pattern jump from Dec to Hex
			if (effect == 0xd)
				effDat = (byte)((((effDat & 0xf0) >> 4) * 10) + (effDat & 0xf));

			// Volume slide, up har priority
			if ((effect == 0xa) && ((effDat & 0xf) != 0) && ((effDat & 0xf0) != 0))
				effDat &= 0xf0;

			if (effect == 0x1b)
				return true;		// UniEffect(UNI_S3MEFFECTQ,dat) ?

			if (effect > 0xf)
				return true;		// Return false to fail?

			uniTrk.UniPtEffect(effect, effDat, of.Flags);

			return true;
		}
		#endregion
	}
}
