/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// String handling
	/// </summary>
	internal class StringBuf
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public StringBuf(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize an existing mpg123_string structure to {NULL, 0, 0}.
		/// If you hand in a NULL pointer here, your program should crash.
		/// The other string functions are more forgiving, but this one here
		/// is too basic
		/// </summary>
		/********************************************************************/
		public void Mpg123_Init_String(Mpg123_String sb)
		{
			// Handing in NULL here is a fatal mistake and rightfully so
			sb.P = null;
			sb.Size = 0;
			sb.Fill = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Free-up memory of the contents of an mpg123_string (not the
		/// struct itself). This also calls mpg123_init_string() and hence
		/// is safe to be called repeatedly
		/// </summary>
		/********************************************************************/
		public void Mpg123_Free_String(Mpg123_String sb)
		{
			if (sb == null)
				return;

			lib.Mpg123_Init_String(sb);
		}



		/********************************************************************/
		/// <summary>
		/// Increase size of a mpg123_string if necessary (it may stay
		/// larger). Note that the functions for adding and setting in
		/// current libmpg123 use this instead of mpg123_resize_string().
		/// That way, you can preallocate memory and safely work afterwards
		/// with pieces
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Grow_String(Mpg123_String sb, size_t news)
		{
			if (sb == null)
				return 0;

			if (sb.Size < news)
				return lib.Mpg123_Resize_String(sb, news);
			else
				return 1;
		}



		/********************************************************************/
		/// <summary>
		/// Change the size of a mpg123_string
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Resize_String(Mpg123_String sb, size_t new_)
		{
			if (sb == null)
				return 0;

			if (new_ == 0)
			{
				lib.Mpg123_Init_String(sb);
				return 1;
			}

			if (sb.Size != new_)
			{
				c_uchar[] t = Memory.Int123_Safe_Realloc(sb.P, new_);
				if (t != null)
				{
					sb.P = t;
					sb.Size = new_;

					if (sb.Size < sb.Fill)
					{
						// Cut short the existing data, properly
						sb.Fill = sb.Size;
						sb.P[sb.Fill - 1] = 0;
					}

					return 1;
				}
				else
					return 0;
			}
			else
				return 1;	// Success
		}



		/********************************************************************/
		/// <summary>
		/// Move the contents of one mpg123_string string to another.
		/// This frees any memory associated with the target and moves over
		/// the pointers from the source, leaving the source without content
		/// after that. The only possible error is that you hand in NULL
		/// pointers. If you handed in a valid source, its contents will be
		/// gone, even if there was no target to move to. If you hand in a
		/// valid target, its original contents will also always be gone, to
		/// be replaced with the source's contents if there was some
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Move_String(Mpg123_String from, ref Mpg123_String to)
		{
			if (to != null)
				lib.Mpg123_Free_String(to);
			else
				lib.Mpg123_Free_String(from);

			if ((from != null) && (to != null))
				to = from;

			return (from != null) && (to != null) ? 1 : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Determine if two strings contain the same data.
		/// This only returns 1 if both given handles are non-NULL and if
		/// they are filled with the same bytes
		/// </summary>
		/********************************************************************/
		public c_int Mpg123_Same_String(Mpg123_String a, Mpg123_String b)
		{
			if ((a == null) || (b == null))
				return 0;

			if (a.Fill != b.Fill)
				return 0;

			if (!a.P.AsSpan(0, (int)a.Fill).SequenceEqual(b.P.AsSpan(0, (int)b.Fill)))
				return 0;

			return 1;
		}
	}
}
