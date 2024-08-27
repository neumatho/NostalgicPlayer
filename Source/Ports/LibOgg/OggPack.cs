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
	/// Interface to OggPack methods
	/// </summary>
	public class OggPack
	{
		private readonly OggPack_Buffer buffer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OggPack(OggPack_Buffer b)
		{
			buffer = b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void WriteInit(out OggPack b)
		{
			Bitwise.OggPack_WriteInit(out OggPack_Buffer bu);
			b = new OggPack(bu);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteCopy(byte[] source, c_long bits)
		{
			Bitwise.OggPack_WriteCopy(buffer, source, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			Bitwise.OggPack_Reset(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteClear()
		{
			Bitwise.OggPack_WriteClear(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadInit(out OggPack b, byte[] buf, c_int bytes)
		{
			Bitwise.OggPack_ReadInit(out OggPack_Buffer bu, buf, bytes);
			b = new OggPack(bu);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Write(c_ulong value, c_int bits)
		{
			Bitwise.OggPack_Write(buffer, value, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Look(c_int bits)
		{
			return Bitwise.OggPack_Look(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Look1()
		{
			return Bitwise.OggPack_Look1(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Adv(c_int bits)
		{
			Bitwise.OggPack_Adv(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Read(c_int bits)
		{
			return Bitwise.OggPack_Read(buffer, bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Read1()
		{
			return Bitwise.OggPack_Read1(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_long Bytes()
		{
			return Bitwise.OggPack_Bytes(buffer);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public byte[] GetBuffer()
		{
			return Bitwise.OggPack_GetBuffer(buffer);
		}
	}
}
