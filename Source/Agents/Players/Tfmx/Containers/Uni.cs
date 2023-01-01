/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// Holds different part of an uint
	/// </summary>
	internal class Uni
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Uni(uint x)
		{
			l = x;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the full uint
		/// </summary>
		/********************************************************************/
		public uint l
		{
			get => _l;

			set
			{
				_l = value;

				_w0 = (ushort)((value & 0xffff0000) >> 16);
				_w1 = (ushort)(value & 0xffff);
				_b0 = (byte)((value >> 24) & 0xff);
				_b1 = (byte)((value >> 16) & 0xff);
				_b2 = (byte)((value >> 8) & 0xff);
				_b3 = (byte)(value & 0xff);
			}
		}
		private uint _l;



		/********************************************************************/
		/// <summary>
		/// Holds the upper ushort of the uint
		/// </summary>
		/********************************************************************/
		public ushort w0
		{
			get => _w0;

			set
			{
				_w0 = value;

				_l = (_l & 0x0000ffff) | (uint)value << 16;
				_b0 = (byte)((value >> 8) & 0xff);
				_b1 = (byte)(value & 0xff);
			}
		}
		private ushort _w0;



		/********************************************************************/
		/// <summary>
		/// Holds the lower ushort of the uint
		/// </summary>
		/********************************************************************/
		public ushort w1
		{
			get => _w1;

			set
			{
				_w1 = value;

				_l = (_l & 0xffff0000) | value;
				_b2 = (byte)((value >> 8) & 0xff);
				_b3 = (byte)(value & 0xff);
			}
		}
		private ushort _w1;



		/********************************************************************/
		/// <summary>
		/// Holds the high byte of the uint
		/// </summary>
		/********************************************************************/
		public byte b0
		{
			get => _b0;

			set
			{
				_b0 = value;

				_l = (_l & 0x00ffffff) | (uint)value << 24;
				_w0 = (ushort)((_w0 & 0x00ff) | (value << 8));
			}
		}
		private byte _b0;



		/********************************************************************/
		/// <summary>
		/// Holds the second byte of the uint
		/// </summary>
		/********************************************************************/
		public byte b1
		{
			get => _b1;

			set
			{
				_b1 = value;

				_l = (_l & 0xff00ffff) | (uint)value << 16;
				_w0 = (ushort)((_w0 & 0xff00) | value);
			}
		}
		private byte _b1;



		/********************************************************************/
		/// <summary>
		/// Holds the third byte of the uint
		/// </summary>
		/********************************************************************/
		public byte b2
		{
			get => _b2;

			set
			{
				_b2 = value;

				_l = (_l & 0xffff00ff) | (uint)value << 8;
				_w1 = (ushort)((_w1 & 0x00ff) | (value << 8));
			}
		}
		private byte _b2;



		/********************************************************************/
		/// <summary>
		/// Holds the lower byte of the uint
		/// </summary>
		/********************************************************************/
		public byte b3
		{
			get => _b3;

			set
			{
				_b3 = value;

				_l = (_l & 0xffffff00) | value;
				_w1 = (ushort)((_w1 & 0xff00) | value);
			}
		}
		private byte _b3;
	}
}
