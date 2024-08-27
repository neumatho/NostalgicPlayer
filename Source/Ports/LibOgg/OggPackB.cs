/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;
using Polycode.NostalgicPlayer.Ports.LibOgg.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibOgg
{
	/// <summary>
	/// Interface to OggPackB methods
	/// </summary>
	public class OggPackB
	{
		private readonly OggPack_Buffer buffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OggPackB(OggPack_Buffer b)
		{
			buffer = b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void WriteInit(out OggPackB b)
		{
			Bitwise.OggPackB_WriteInit(out OggPack_Buffer bu);
			b = new OggPackB(bu);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteCopy(byte[] source, c_long bits)
		{
			Bitwise.OggPackB_WriteCopy(buffer, source, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Bitwise.OggPackB_Reset(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteClear()
		{
			Bitwise.OggPackB_WriteClear(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadInit(out OggPackB b, byte[] buf, c_int bytes)
		{
			Bitwise.OggPackB_ReadInit(out OggPack_Buffer bu, buf, bytes);
			b = new OggPackB(bu);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(c_ulong value, c_int bits)
		{
			Bitwise.OggPackB_Write(buffer, value, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Look(c_int bits)
		{
			return Bitwise.OggPackB_Look(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Look1()
		{
			return Bitwise.OggPackB_Look1(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Adv(c_int bits)
		{
			Bitwise.OggPackB_Adv(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Read(c_int bits)
		{
			return Bitwise.OggPackB_Read(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Read1()
		{
			return Bitwise.OggPackB_Read1(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Bytes()
		{
			return Bitwise.OggPackB_Bytes(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte[] GetBuffer()
		{
			return Bitwise.OggPackB_GetBuffer(buffer);
		}
	}
}
