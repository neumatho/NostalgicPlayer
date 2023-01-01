/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This stream is used by module converters
	/// </summary>
	public class ConverterStream : WriterStream
	{
		private readonly Dictionary<int, ConvertSampleInfo> sampleInfo;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ConverterStream(Stream wrapperStream, Dictionary<int, ConvertSampleInfo> sampleInfo) : base(wrapperStream)
		{
			this.sampleInfo = sampleInfo;
			HasSampleDataMarkers = false;
			ConvertedLength = 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Write data to the stream
		/// </summary>
		/********************************************************************/
		public override void Write(byte[] buffer, int offset, int count)
		{
			ConvertedLength += count;

			base.Write(buffer, offset, count);
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Write a marker about the sample, but not the data itself
		/// </summary>
		/********************************************************************/
		public void WriteSampleDataMarker(int sampleNumber, int length)
		{
			if (!sampleInfo.TryGetValue(sampleNumber, out ConvertSampleInfo convertInfo))
				throw new Exception($"Sample number {sampleNumber} has not been read before write");

			if (convertInfo.Length != length)
				throw new Exception($"Something is wrong when writing sample data. The given sample length {length} does not match the one stored in the converted data {convertInfo.Length} for sample number {sampleNumber}");

			Write_B_UINT32(convertInfo.Position);
			Write_B_UINT32((uint)convertInfo.Length);

			HasSampleDataMarkers = true;
			ConvertedLength += convertInfo.Length;
		}



		/********************************************************************/
		/// <summary>
		/// Tells whether any sample markings has been written
		/// </summary>
		/********************************************************************/
		public bool HasSampleDataMarkers
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the converted length including sample data
		/// </summary>
		/********************************************************************/
		public long ConvertedLength
		{
			get; private set;
		}
	}
}
