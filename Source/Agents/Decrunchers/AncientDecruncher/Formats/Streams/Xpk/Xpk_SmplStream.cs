/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Common;
using Polycode.NostalgicPlayer.Kit.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Formats.Streams.Xpk
{
	/// <summary>
	/// This stream read data crunched with XPK (SMPL)
	/// </summary>
	internal class Xpk_SmplStream : XpkStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Xpk_SmplStream(string agentName,  Stream wrapperStream) : base(agentName,  wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will decrunch a single chunk of data
		/// </summary>
		/********************************************************************/
		protected override void DecompressImpl(byte[] chunk,  byte[] rawData)
		{
			if (Read16(chunk,  0) != 1)
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);

			using (MemoryStream chunkStream = new MemoryStream(chunk, false))
			{
				ForwardInputStream inputStream = new ForwardInputStream(agentName, chunkStream, 2, (uint)chunk.Length);
				MsbBitReader bitReader = new MsbBitReader(inputStream);

				uint ReadBits(uint count) => bitReader.ReadBits8(count);
				uint ReadBit() => bitReader.ReadBits8(1);

				ForwardOutputStream outputStream = new ForwardOutputStream(agentName, rawData, 0, (uint)rawData.Length);

				HuffmanDecoder<uint> decoder = new HuffmanDecoder<uint>(agentName);

				for (uint i = 0; i < 256; i++)
				{
					uint codeLength = ReadBits(4);
					if (codeLength == 0)
						continue;

					if (codeLength == 15)
						codeLength = ReadBits(4) + 15;

					uint code = ReadBits(codeLength);
					decoder.Insert(new HuffmanCode<uint>(codeLength, code, i));
				}

				byte accum = 0;

				while (!outputStream.Eof)
				{
					uint code = decoder.Decode(ReadBit);
					accum += (byte)code;

					outputStream.WriteByte(accum);
				}
			}
		}
	}
}
