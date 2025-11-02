/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace NostalgicPlayer.Kit.C.Test.String
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TestStringBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected void Fill(CPointer<char> buf, c_int length, char value)
		{
			for (c_int i = 0; i < length; i++)
				buf[i] = value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected bool Compare(CPointer<char> str1, CPointer<char> str2, c_int maxLength = -1)
		{
			if (maxLength < 0)
				maxLength = str2.Length;

			for (int i = 0; i < maxLength; i++)
			{
				if (str1[i] != str2[i])
					return false;
			}

			return true;
		}
	}
}
