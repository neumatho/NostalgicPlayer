/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	/// 
	/// </summary>
	internal class CharBuf : IDeepCloneable<CharBuf>, IClearable
	{
		public uint8[] Buf;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private CharBuf()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CharBuf(size_t len)
		{
			Buf = new uint8[len];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StdString Str()
		{
			return MptString.ReadAutoBuf(Buf);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Assign(CharBuf str)
		{
			MptString.WriteAutoBuf(Buf).Assign(new StdString(str.Str()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Assign(StdString str)
		{
			MptString.WriteAutoBuf(Buf).Assign(str);
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public CharBuf MakeDeepClone()
		{
			CharBuf clone = new CharBuf();

			clone.Buf = ArrayHelper.CloneArray(Buf);

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Array.Clear(Buf);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()
		{
			return Encoding.Latin1.GetString(Buf);
		}
	}
}
