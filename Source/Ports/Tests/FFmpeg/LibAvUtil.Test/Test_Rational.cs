/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Rational : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test()
		{
			RunTest(null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			{
				AvRational a, b, r;

				for (a.Num = -2; a.Num <= 2; a.Num++)
				{
					for (a.Den = -2; a.Den <= 2; a.Den++)
					{
						for (b.Num = -2; b.Num <= 2; b.Num++)
						{
							for (b.Den = -2; b.Den <= 2; b.Den++)
							{
								c_int c = Rational.Av_Cmp_Q(a, b);
								c_double d = Rational.Av_Q2D(a) == Rational.Av_Q2D(b) ? 0 : (Rational.Av_Q2D(a) - Rational.Av_Q2D(b));

								if (d > 0)
									d = 1;
								else if (d < 0)
									d = -1;
								else if (CMath.isnan(d))
									d = c_int.MinValue;

								if (c != d)
									Log.Av_Log(null, Log.Av_Log_Error, "%d/%d %d/%d, %d %f\n", a.Num, a.Den, b.Num, b.Den, c, d);

								r = Rational.Av_Sub_Q(Rational.Av_Add_Q(b, a), b);

								if ((b.Den != 0) && (((r.Num * a.Den) != (a.Num * r.Den)) || ((r.Num == 0) != (a.Num == 0)) || ((r.Den == 0) != (a.Den == 0))))
									Log.Av_Log(null, Log.Av_Log_Error, "%d/%d ", r.Num, r.Den);
							}
						}
					}
				}
			}

			{
				int64_t[] numList = [ int64_t.MinValue, int64_t.MinValue + 1, int64_t.MaxValue, int32_t.MinValue, int32_t.MaxValue, 1, 0, -1, 123456789, int32_t.MaxValue - 1, int32_t.MaxValue + 1L, uint32_t.MaxValue - 1, uint32_t.MaxValue, uint32_t.MaxValue + 1L ];

				for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(numList); i++)
				{
					int64_t a = numList[i];

					for (c_int j = 0; j < (c_int)Macros.FF_Array_Elems(numList); j++)
					{
						int64_t b = numList[j];

						if (b <= 0)
							continue;

						for (c_int k = 0; k < (c_int)Macros.FF_Array_Elems(numList); k++)
						{
							int64_t c = numList[k];
							AvInteger ai;

							if (c <= 0)
								continue;

							int64_t res = Mathematics.Av_Rescale_Rnd(a, b, c, AvRounding.Zero);

							ai = Integer.Av_Mul_I(Integer.Av_Int2I(a), Integer.Av_Int2I(b));
							ai = Integer.Av_Div_I(ai, Integer.Av_Int2I(c));

							if ((Integer.Av_Cmp_I(ai, Integer.Av_Int2I(int64_t.MaxValue)) > 0) && (res == int64_t.MinValue))
								continue;

							if ((Integer.Av_Cmp_I(ai, Integer.Av_Int2I(int64_t.MinValue)) < 0) && (res == int64_t.MinValue))
								continue;

							if (Integer.Av_Cmp_I(ai, Integer.Av_Int2I(res)) == 0)
								continue;

							// Special exception for INT64_MIN, remove this in case INT64_MIN is handled without off by 1 error
							if ((Integer.Av_Cmp_I(ai, Integer.Av_Int2I(res - 1)) == 0) && (a == int64_t.MinValue))
								continue;

							Log.Av_Log(null, Log.Av_Log_Error, "%lld * %lld / %lld = %lld or %lld\n", a, b, c, res, Integer.Av_I2Int(ai));
						}
					}
				}
			}

			{
				AvRational a, b;

				for (a.Num = 1; a.Num <= 10; a.Num++)
				{
					for (a.Den = 1; a.Den <= 10; a.Den++)
					{
						if (Mathematics.Av_Gcd(a.Num, a.Den) > 1)
							continue;

						for (b.Num = 1; b.Num <= 10; b.Num++)
						{
							for (b.Den = 1; b.Den <= 10; b.Den++)
							{
								if (Mathematics.Av_Gcd(b.Num, b.Den) > 1)
									continue;

								if (Rational.Av_Cmp_Q(b, a) < 0)
									continue;

								for (c_int start = 0; start < 10; start++)
								{
									c_int acc = start;

									for (c_int i = 0; i < 100; i++)
									{
										c_int exact = (c_int)(start + Mathematics.Av_Rescale_Q(i + 1, b, a));
										acc = (c_int)Mathematics.Av_Add_Stable(a, acc, b, 1);

										if (Common.FFAbs(acc - exact) > 2)
										{
											Log.Av_Log(null, Log.Av_Log_Error, "%d/%d %d/%d, %d %d\n", a.Num, a.Den, b.Num, b.Den, acc, exact);

											return 1;
										}
									}
								}
							}
						}
					}
				}

				for (a.Den = 1; a.Den < 0x100000000 / 3; a.Den *= 3)
				{
					for (a.Num = -1; a.Num < (1 << 27); a.Num += 1 + (a.Num / 100))
					{
						c_float f = IntFloat.Av_Int2Float(Rational.Av_Q2IntFloat(a));
						c_float f2 = (c_float)Rational.Av_Q2D(a);

						if (CMath.fabs(f - f2) > (CMath.fabs(f) / 5000000))
						{
							Log.Av_Log(null, Log.Av_Log_Error, "%d/%d %f %f\n", a.Num, a.Den, f, f2);

							return 1;
						}
					}
				}
			}

			return 0;
		}
	}
}
