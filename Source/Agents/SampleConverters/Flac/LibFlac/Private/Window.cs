/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Window
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Bartlett(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;
			Flac__int32 n;

			if ((L & 1) != 0)
			{
				for (n = 0; n <= N / 2; n++)
					window[n] = 2.0f * n / N;

				for (; n <= N; n++)
					window[n] = 2.0f - 2.0f * n / N;
			}
			else
			{
				for (n = 0; n <= L / 2 - 1; n++)
					window[n] = 2.0f * n / N;

				for (; n <= N; n++)
					window[n] = 2.0f - 2.0f * n / N;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Bartlett_Hann(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.62 - 0.48 * Math.Abs((float)n / N - 0.5) - 0.38 * Math.Cos(2.0 * Math.PI * ((float)n / N)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Blackman(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.42 - 0.5 * Math.Cos(2.0 * Math.PI * n / N) + 0.08 * Math.Cos(4.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Blackman_Harris_4Term_92Db_Sidelobe(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n <= N; n++)
				window[n] = (Flac__real)(0.35875 - 0.48829 * Math.Cos(2.0 * Math.PI * n / N) + 0.14128 * Math.Cos(4.0 * Math.PI * n / N) - 0.01168 * Math.Cos(6.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Connes(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;
			double N2 = N / 2.0;

			for (Flac__int32 n = 0; n <= N; n++)
			{
				double k = (n - N2) / N2;
				k = 1.0 - k * k;
				window[n] = (Flac__real)(k * k);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Flattop(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.21557895 - 0.41663158 * Math.Cos(2.0 * Math.PI * n / N) + 0.277263158 * Math.Cos(4.0 * Math.PI * n / N) - 0.083578947 * Math.Cos(6.0 * Math.PI * n / N) + 0.006947368 * Math.Cos(8.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Gauss(Flac__real[] window, Flac__int32 L, Flac__real stdDev)
		{
			Flac__int32 N = L - 1;
			double N2 = N / 2.0;

			for (Flac__int32 n = 0; n <= N; n++)
			{
				double k = (n - N2) / (stdDev * N2);
				window[n] = (Flac__real)Math.Exp(-0.5 * k * k);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Hamming(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.54 - 0.46 * Math.Cos(2.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Hann(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.5 - 0.5 * Math.Cos(2.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Kaiser_Bessel(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.402 - 0.498 * Math.Cos(2.0 * Math.PI * n / N) + 0.098 * Math.Cos(4.0 * Math.PI * n / N) - 0.001 * Math.Cos(6.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Nuttall(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;

			for (Flac__int32 n = 0; n < L; n++)
				window[n] = (Flac__real)(0.3635819 - 0.4891775 * Math.Cos(2.0 * Math.PI * n / N) + 0.1365995 * Math.Cos(4.0 * Math.PI * n / N) - 0.0106411 * Math.Cos(6.0 * Math.PI * n / N));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Rectangle(Flac__real[] window, Flac__int32 L)
		{
			for (Flac__int32 n = 0; n < L; n++)
				window[n] = 1.0f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Triangle(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 n;

			if ((L & 1) != 0)
			{
				for (n = 1; n <= (L + 1) / 2; n++)
					window[n - 1] = 2.0f * n / (L + 1.0f);

				for (; n <= L; n++)
					window[n - 1] = (2.0f * (L - n + 1)) / (L + 1.0f);
			}
			else
			{
				for (n = 1; n <= L / 2; n++)
					window[n - 1] = 2.0f * n / (L + 1.0f);

				for (; n <= L; n++)
					window[n - 1] = (2.0f * (L - n + 1)) / (L + 1.0f);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Tukey(Flac__real[] window, Flac__int32 L, Flac__real p)
		{
			if (p <= 0.0f)
				Flac__Window_Rectangle(window, L);
			else if (p >= 1.0f)
				Flac__Window_Hann(window, L);
			else
			{
				Flac__int32 Np = (Flac__int32)(p / 2.0f * L) - 1;

				// Start with rectangle
				Flac__Window_Rectangle(window, L);

				// Replace ends with Hann
				if (Np > 0)
				{
					for (Flac__int32 n = 0; n <= Np; n++)
					{
						window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * n / Np));
						window[L - Np - 1 + n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * (n + Np) / Np));
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Partial_Tukey(Flac__real[] window, Flac__int32 L, Flac__real p, Flac__real start, Flac__real end)
		{
			if (p <= 0.0f)
				Flac__Window_Partial_Tukey(window, L, 0.05f, start, end);
			else if (p >= 1.0f)
				Flac__Window_Partial_Tukey(window, L, 0.95f, start, end);
			else
			{
				Flac__int32 start_N = (Flac__int32)(start * L);
				Flac__int32 end_N = (Flac__int32)(end * L);
				Flac__int32 N = end_N - start_N;
				Flac__int32 n, i;

				Flac__int32 Np = (Flac__int32)(p / 2.0f * N);

				for (n = 0; (n < start_N) && (n < L); n++)
					window[n] = 0.0f;

				for (i = 1; (n < (start_N + Np)) && (n < L); n++, i++)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Np));

				for (; (n < (end_N - Np)) && (n < L); n++)
					window[n] = 1.0f;

				for (i = Np; (n < end_N) && (n < L); n++, i--)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Np));

				for (; n < L; n++)
					window[n] = 0.0f;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Punchout_Tukey(Flac__real[] window, Flac__int32 L, Flac__real p, Flac__real start, Flac__real end)
		{
			if (p <= 0.0f)
				Flac__Window_Punchout_Tukey(window, L, 0.05f, start, end);
			else if (p >= 1.0f)
				Flac__Window_Punchout_Tukey(window, L, 0.95f, start, end);
			else
			{
				Flac__int32 start_N = (Flac__int32)(start * L);
				Flac__int32 end_N = (Flac__int32)(end * L);
				Flac__int32 n, i;

				Flac__int32 Ns = (Flac__int32)(p / 2.0f * start_N);
				Flac__int32 Ne = (Flac__int32)(p / 2.0f * (L - end_N));

				for (n = 0, i = 1; (n < Ns) && (n < L); n++, i++)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Ns));

				for (; (n < (start_N - Ns)) && (n < L); n++)
					window[n] = 1.0f;

				for (i = Ns; (n < start_N) && (n < L); n++, i--)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Ns));

				for (; (n < end_N) && (n < L); n++)
					window[n] = 0.0f;

				for (i = 1; (n < (end_N + Ne)) && (n < L); n++, i++)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Ne));

				for (; (n < (L - Ne)) && (n < L); n++)
					window[n] = 1.0f;

				for (i = Ne; n < L; n++, i--)
					window[n] = (Flac__real)(0.5f - 0.5f * Math.Cos(Math.PI * i / Ne));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Window_Welch(Flac__real[] window, Flac__int32 L)
		{
			Flac__int32 N = L - 1;
			double N2 = N / 2.0;

			for (Flac__int32 n = 0; n <= N; n++)
			{
				double k = (n - N2) / N2;
				window[n] = (Flac__real)(1.0 - k * k);
			}
		}
	}
}
