/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Frame index data structure and functions
	/// </summary>
	internal class Index
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Fi_Init(Frame_Index fi)
		{
			fi.Data = null;
			fi.Step = 1;
			fi.Fill = 0;
			fi.Size = 0;
			fi.Grow_Size = 0;
			fi.Next = Fi_Next(fi);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Fi_Exit(Frame_Index fi)
		{
			Int123_Fi_Init(fi);		// Be prepared for further fun, still
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Int123_Fi_Resize(Frame_Index fi, size_t newSize)
		{
			if (newSize == fi.Size)
				return Mpg123_Errors.Ok;

			if ((newSize > 0) && (newSize < fi.Size))
			{
				// When we reduce buffer size a bit, shrink stuff
				while (fi.Fill > newSize)
					Fi_Shrink(fi);
			}

			int64_t[] newData = Memory.Int123_Safe_Realloc(fi.Data, newSize);
			if ((newSize == 0) || (newData != null))
			{
				fi.Data = newData;
				fi.Size = newSize;

				if (fi.Fill > fi.Size)
					fi.Fill = fi.Size;

				fi.Next = Fi_Next(fi);

				return Mpg123_Errors.Ok;
			}
			else
				return Mpg123_Errors.Err;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Fi_Add(Frame_Index fi, int64_t pos)
		{
			if (fi.Fill == fi.Size)
			{
				// Index is full, we need to shrink... or grow.
				// Store the current frame number to check later if we still want it
				int64_t frameNum = (int64_t)fi.Fill * fi.Step;

				// If we want not / cannot grow, we shrink
				if (!((fi.Grow_Size != 0) && Int123_Fi_Resize(fi, fi.Size + fi.Grow_Size) == 0))
					Fi_Shrink(fi);

				// Now check if we still want to add this frame (could be that not,
				// because of changed step)
				if (fi.Next != frameNum)
					return;
			}

			// When we are here, we want that frame
			if (fi.Fill < fi.Size)	// Safeguard for size=1, or just generally
			{
				fi.Data[fi.Fill] = pos;
				++fi.Fill;
				fi.Next = Fi_Next(fi);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Int123_Fi_Reset(Frame_Index fi)
		{
			fi.Fill = 0;
			fi.Step = 1;
			fi.Next = Fi_Next(fi);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// The next expected frame offset, one step ahead
		/// </summary>
		/********************************************************************/
		private int64_t Fi_Next(Frame_Index fi)
		{
			return (int64_t)fi.Fill * fi.Step;
		}



		/********************************************************************/
		/// <summary>
		/// Shrink down the used index to the half.
		/// Be careful with size = 1 ... there's no shrinking possible there
		/// </summary>
		/********************************************************************/
		private void Fi_Shrink(Frame_Index fi)
		{
			if (fi.Fill < 2)
				return;			// Don't shrink below 1
			else
			{
				// Double the step, half the fill. Should work as well for fill%2 = 1
				fi.Step *= 2;
				fi.Fill /= 2;

				// Move the data down
				for (size_t c = 0; c < fi.Fill; ++c)
					fi.Data[c] = fi.Data[2 * c];
			}

			fi.Next = Fi_Next(fi);
		}
		#endregion
	}
}
