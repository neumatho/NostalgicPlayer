/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Client.GuiPlayer.Composition;
using Polycode.NostalgicPlayer.Library.Application;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Main entry point
	/// </summary>
	static class Program
	{
		private sealed class CastMemoryManager<TFrom, TTo> : MemoryManager<TTo>
				where TFrom : unmanaged
				where TTo : unmanaged
		{
			private readonly Memory<TFrom> _from;

			public CastMemoryManager(Memory<TFrom> from) => _from = from;

			public override Span<TTo> GetSpan()
				=> MemoryMarshal.Cast<TFrom, TTo>(_from.Span);

			protected override void Dispose(bool disposing)
			{
			}
			public override MemoryHandle Pin(int elementIndex = 0)
				=> throw new NotSupportedException();
			public override void Unpin()
				=> throw new NotSupportedException();
		}


	[StructLayout(LayoutKind.Sequential)]
	public struct AvComplexInt32
	{
		private int _re;
		private int _im;

		/// <summary>
		/// 
		/// </summary>
		public int Re
		{
			get => _re;
			set => _re = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public int Im
		{
			get => _im;
			set => _im = value;
		}
	}



		/********************************************************************/
		/// <summary>
		/// The main entry point for the application
		/// </summary>
		/********************************************************************/
		[STAThread]
		static void Main()
		{
			int[] mineBytes = [ 42, 1, 1, 2, 4, 7 ];
			Memory<int> mem = new Memory<int>(mineBytes);

			Memory<short> mem1 = new CastMemoryManager<int, short>(mem).Memory;
//			Memory<AvComplexInt32> mem2 = new CastMemoryManager<int, AvComplexInt32>(mem).Memory;

			if (MemoryMarshal.TryGetArray(mem, out ArraySegment<int> segment))
			{
			    int[] array = segment.Array!;
			    int offset = segment.Offset;
			    int length = segment.Count;
			}
			if (MemoryMarshal.TryGetArray(mem1, out ArraySegment<short> segment1))
			{
			    short[] array = segment1.Array!;
			    int offset = segment1.Offset;
			    int length = segment1.Count;
			}
			try
			{
				new ApplicationBuilder(Environment.GetCommandLineArgs())
					.ConfigureContainer(context =>
					{
						CompositionRoot.Register(context.Container);
					})
					.ConfigureInitialization(() =>
					{
						Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
						Application.EnableVisualStyles();
						Application.SetCompatibleTextRenderingDefault(false);
					})
					.Build()
					.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Resources.IDS_ERR_EXCEPTION, ex.Message), Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
		}
	}
}
