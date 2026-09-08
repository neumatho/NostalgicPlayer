/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
		private readonly List<ConvertSamplePosition> samplePositions;
		private long markedSampleLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ConverterStream(Stream wrapperStream) : base(wrapperStream, true)
		{
			samplePositions = new List<ConvertSamplePosition>();
			markedSampleLength = 0;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// Return the length of the converted module including the sample
		/// data which has only been marked
		/// </summary>
		/********************************************************************/
		public override long Length => wrapperStream.Length + markedSampleLength;



		/********************************************************************/
		/// <summary>
		/// Return the current position in the converted module. The marked
		/// sample data is included, even though it has not been written to
		/// the wrapper stream
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => WrapperToConvertedPosition(wrapperStream.Position);

			set => Seek(value, SeekOrigin.Begin);
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position in the converted module
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			long newPosition;

			switch (origin)
			{
				case SeekOrigin.Current:
				{
					newPosition = Position + offset;
					break;
				}

				case SeekOrigin.End:
				{
					newPosition = Length + offset;
					break;
				}

				default:
				{
					newPosition = offset;
					break;
				}
			}

			if (newPosition < 0)
				newPosition = 0;

			wrapperStream.Seek(ConvertedToWrapperPosition(newPosition), SeekOrigin.Begin);

			return newPosition;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Tell the convert to mark a sample at the given position
		/// </summary>
		/********************************************************************/
		public void SetSampleDataMarker(ModuleStream moduleStream, int length)
		{
			samplePositions.Add(new ConvertSamplePosition
			{
				StartPosition = Position,
				SampleStreamStartPosition = moduleStream.Position,
				Length = length
			});

			markedSampleLength += length;

			moduleStream.Seek(length, SeekOrigin.Current);
		}



		/********************************************************************/
		/// <summary>
		/// Return the sample markings
		/// </summary>
		/********************************************************************/
		public ConvertSamplePosition[] GetSampleDataMarkings()
		{
			return samplePositions.ToArray();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Translate a position in the wrapper stream to the position in the
		/// converted module
		/// </summary>
		/********************************************************************/
		private long WrapperToConvertedPosition(long wrapperPosition)
		{
			long addedSampleLength = 0;

			foreach (ConvertSamplePosition convertSamplePos in samplePositions)
			{
				// The sample data itself is not stored in the wrapper stream, so
				// all the samples marked at or before this position have to be
				// added to get the position in the converted module
				if ((convertSamplePos.StartPosition - addedSampleLength) > wrapperPosition)
					break;

				addedSampleLength += convertSamplePos.Length;
			}

			return wrapperPosition + addedSampleLength;
		}



		/********************************************************************/
		/// <summary>
		/// Translate a position in the converted module to the position in
		/// the wrapper stream
		/// </summary>
		/********************************************************************/
		private long ConvertedToWrapperPosition(long convertedPosition)
		{
			long skippedSampleLength = 0;

			foreach (ConvertSamplePosition convertSamplePos in samplePositions)
			{
				if (convertedPosition < convertSamplePos.EndPosition)
					break;

				skippedSampleLength += convertSamplePos.Length;
			}

			return convertedPosition - skippedSampleLength;
		}
		#endregion
	}
}
