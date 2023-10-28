/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Player_Helpers
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Set(Channel_Data xc, Channel_Flag flag)
		{
			xc.Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Reset(Channel_Data xc, Channel_Flag flag)
		{
			xc.Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Test(Channel_Data xc, Channel_Flag flag)
		{
			return (xc.Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Set_Per(Channel_Data xc, Channel_Flag flag)
		{
			xc.Per_Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Reset_Per(Channel_Data xc, Channel_Flag flag)
		{
			xc.Per_Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Test_Per(Channel_Data xc, Channel_Flag flag)
		{
			return (xc.Per_Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Set_Note(Channel_Data xc, Note_Flag flag)
		{
			xc.Note_Flags |= flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Reset_Note(Channel_Data xc, Note_Flag flag)
		{
			xc.Note_Flags &= ~flag;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Test_Note(Channel_Data xc, Note_Flag flag)
		{
			return (xc.Note_Flags & flag) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Is_Valid_Instrument(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Ins) && (mod.Xxi[x].Nsm > 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Is_Valid_Instrument_Or_Sfx(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Ins) && (mod.Xxi[x].Nsm > 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Is_Valid_Sample(Xmp_Module mod, c_int x)
		{
			return ((uint32)x < mod.Smp) && (mod.Xxs[x].Data != null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Is_Valid_Note(c_int x)
		{
			return (uint32)x < Constants.Xmp_Max_Keys;
		}
	}
}
