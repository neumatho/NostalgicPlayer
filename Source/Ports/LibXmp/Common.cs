/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Common
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8 Msn(uint8 x)
		{
			return (uint8)((x & 0xf0) >> 4);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint8 Lsn(uint8 x)
		{
			return (uint8)(x & 0x0f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Msn(c_int x)
		{
			return (x & 0xf0) >> 4;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Lsn(c_int x)
		{
			return x & 0x0f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clamp<T>(ref T x, T a, T b) where T : INumber<T>
		{
			if (x < a)
				x = a;
			else if (x > b)
				x = b;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_int Track_Num(Module_Data m, c_int a, c_int c)
		{
			return m.Mod.Xxp[a].Index[c];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Xmp_Event Event(Module_Data m, c_int a, c_int c, c_int r)
		{
			return m.Mod.Xxt[Track_Num(m, a, c)].Event[r];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has_Quirk(Module_Data m, Quirk_Flag x)
		{
			return (m.Quirk & x) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has_Flow_Mode(Module_Data m, FlowMode_Flag x)
		{
			return (m.Flow_Mode & x) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Player_Mode_Mod(Module_Data m)
		{
			return m.Read_Event_Type == Read_Event.Mod;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Player_Mode_Ft2(Module_Data m)
		{
			return m.Read_Event_Type == Read_Event.Ft2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Player_Mode_It(Module_Data m)
		{
			return m.Read_Event_Type == Read_Event.It;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Player_Mode_Med(Module_Data m)
		{
			return m.Read_Event_Type == Read_Event.Med;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Period_ModRng(Module_Data m)
		{
			return m.Period_Type == Containers.Common.Period.ModRng;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Amiga_Mod(Module_Data m)
		{
			return Is_Player_Mode_Mod(m) && Is_Period_ModRng(m);
		}
	}
}
