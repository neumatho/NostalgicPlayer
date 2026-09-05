/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Wrapper around a single array to simulate multiple levels of std::arrays
	/// </summary>
	internal class ModPatternData
	{
		private const int NumberOfRows = 64;
		private const int NumberOfChannels = 4;
		private const int BytesPerChannel = 4;
		private const int BytesPerRow = NumberOfChannels * BytesPerChannel;

		// Pattern data of a 4-channel MOD file
		private readonly uint8[] patternData = new uint8[BytesPerRow * NumberOfRows];

		#region ModPatternRowData class
		/// <summary>
		/// Holds the 4 * 4 bytes of a single row of the pattern
		/// </summary>
		public class ModPatternRowData
		{
			private readonly Memory<uint8> rowData;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ModPatternRowData(Memory<uint8> row)
			{
				rowData = row;
			}



			/********************************************************************/
			/// <summary>
			/// Return an enumerator over the channels of the row
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator GetEnumerator()
			{
				return new Enumerator(rowData);
			}

			#region Enumerator class
			/// <summary>
			/// The enumerator returned by <see cref="GetEnumerator"/>. Each
			/// element is a pointer to the 4 bytes of a single channel
			/// </summary>
			public struct Enumerator
			{
				private readonly Memory<uint8> buffer;

				// Offset of the channel currently referred to, which is
				// before the start of the row until the first call to
				// MoveNext()
				private int offset;

				/********************************************************************/
				/// <summary>
				/// Constructs an enumerator over the channels in the given row
				/// </summary>
				/********************************************************************/
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public Enumerator(Memory<uint8> row)
				{
					buffer = row;
					offset = -BytesPerChannel;
				}



				/********************************************************************/
				/// <summary>
				/// Return a pointer to the channel the enumerator currently refers
				/// to
				/// </summary>
				/********************************************************************/
				public CPointer<uint8> Current
				{
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					get => new CPointer<uint8>(buffer.Slice(offset, BytesPerChannel));
				}



				/********************************************************************/
				/// <summary>
				/// Advances the enumerator to the next channel. Returns true if
				/// there is such a channel, or false if the end has been reached
				/// </summary>
				/********************************************************************/
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public bool MoveNext()
				{
					int next = offset + BytesPerChannel;

					if (next < buffer.Length)
					{
						offset = next;
						return true;
					}

					return false;
				}
			}
			#endregion
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Read a single pattern
		/// </summary>
		/********************************************************************/
		public bool Read(FileReader file)
		{
			return file.ReadArray(patternData);
		}



		/********************************************************************/
		/// <summary>
		/// Return an enumerator over the rows of the pattern
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(patternData);
		}

		#region Enumerator class
		/// <summary>
		/// The enumerator returned by GetEnumerator. Each element holds a single row of the pattern
		/// </summary>
		public struct Enumerator
		{
			private readonly uint8[] buffer;

			// Offset of the row currently referred to, which is before the
			// start of the buffer until the first call to MoveNext()
			private int offset;

			/********************************************************************/
			/// <summary>
			/// Constructs an enumerator over the rows in the given buffer,
			/// positioned before the first row
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Enumerator(uint8[] arr)
			{
				buffer = arr;
				offset = -BytesPerRow;
			}



			/********************************************************************/
			/// <summary>
			/// Return the row the enumerator currently refers to
			/// </summary>
			/********************************************************************/
			public ModPatternRowData Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => new ModPatternRowData(buffer.AsMemory(offset, BytesPerRow));
			}



			/********************************************************************/
			/// <summary>
			/// Advances the enumerator to the next row. Returns true if there
			/// is such a row, or false if the end has been reached
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				int next = offset + BytesPerRow;

				if (next < buffer.Length)
				{
					offset = next;
					return true;
				}

				return false;
			}
		}
		#endregion
	}
}
