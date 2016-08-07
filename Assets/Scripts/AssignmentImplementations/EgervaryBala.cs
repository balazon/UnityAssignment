using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class EgervaryBala : IAssignmentSolver {

	// allocating ahead for 200 units
	public int MAXN = 200;

	//cimkek
	float[] CL;
	float[] CF;

	// besorolashoz
	int[] Fiu;
	int[] Lany;

	bool[,] PirosEl;

	// M[i]: i-edik fiuhoz mi a lanyindex
	int[] M;
	// RevM[i]: i-edik lanyhoz mi a fiuindex
	int[] RevM;

	public EgervaryBala()
	{
		CL = new float[MAXN];
		CF = new float[MAXN];
		Fiu = new int[MAXN];
		Lany = new int[MAXN];
		PirosEl = new bool[MAXN, MAXN];
		M = new int[MAXN];
		RevM = new int[MAXN];
	}

	public bool IsMaxSolver()
	{
		return true;
	}

	public void Solve(int[] weights, int n, out int[] result)
	{
		result = new int[n];

		if(n > MAXN)
		{
			return;
		}

		StringBuilder sb = new StringBuilder();
		foreach (var item in weights)
		{
			sb.AppendFormat(" {0}", item);
		}
		Debug.Log("weights" + sb.ToString());

		//kezdo cimkezes
		for (int i = 0; i < n; i++)
		{
			CL[i] = 0.0f;
			float maxW = weights[i * n + 0];
			for (int j = 0; j < n; j++)
			{
				if (maxW < weights[i * n + j]) maxW = weights[i * n + j];
			}
			CF[i] = maxW;


			M[i] = -1;
			RevM[i] = -1;
		}

		//piros elek + kezdo parositas
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n; j++)
			{
				//CF[i] + CL[j] == weights[i,j] -> piros
				if (CF[i] + CL[j] == weights[i * n + j])
				{
					PirosEl[j, i] = true;
					if (M[i] == -1 && RevM[j] == -1)
					{
						M[i] = j;
						RevM[j] = i;
					}
				}
				else
				{
					PirosEl[j, i] = false;
				}
			}
		}


		var Paths = new List<BipartitePath>();

		do
		{

			//javitoutas a piros reszgrafban
			bool VanJavitoUt = true;
			do
			{
				if (VanJavitoUt)
				{
					Paths.Clear();
					for (int j = 0; j < n; j++)
					{
						//parositatlan lanyok
						if (RevM[j] == -1)
						{
							BipartitePath OnePath = new BipartitePath(j);
							Paths.Add(OnePath);
						}
					}
				}
				if (Paths.Count == 0)
				{
					break;
				}

				VanJavitoUt = false;


				//Lanybol indul, Fiura vegzodik, int indexek
				BipartitePath javitoUt = Paths[Paths.Count - 1];
				Paths.RemoveAt(Paths.Count - 1);
				int LastIndex = javitoUt.Last;
				for (int i = 0; i < n; i++)
				{
					bool BreakFlag = false;
					if (PirosEl[LastIndex, i] && i != RevM[LastIndex] && !javitoUt.hasVertex(i, false))
					{
						if (M[i] != -1)
						{
							BipartitePath AnotherPath = new BipartitePath(javitoUt);
							AnotherPath.AddUnsafe(i);
							AnotherPath.AddUnsafe(M[i]);
							Paths.Add(AnotherPath);
						}
						else
						{
							VanJavitoUt = true;
							javitoUt.AddUnsafe(i);
							for (int j = 0; j < javitoUt.Count - 1; j++)
							{
								if (j % 2 == 0)
								{
									RevM[javitoUt[j]] = javitoUt[j + 1];
									M[javitoUt[j + 1]] = javitoUt[j];
								}
							}
							BreakFlag = true;
						}
					}
					if (BreakFlag) break;
				}

			} while (VanJavitoUt || Paths.Count > 0);

			bool TeljesParositas = true;
			for (int i = 0; i < n; i++)
			{
				if (M[i] == -1)
				{
					TeljesParositas = false;
					break;
				}
			}
			if (TeljesParositas)
			{
				for (int i = 0; i < n; i++)
				{
					result[i] = M[i];
				}
				return;
			}

			Paths.Clear();
			var fiuk = new HashSet<int>();
			//besorolas (F1,F2,F3 megfeleloje: Fiu[i] = 1,2,3, Lanyra ugyanigy)
			for (int i = 0; i < n; i++)
			{
				if (M[i] == -1)
				{
					Fiu[i] = 1;
					fiuk.Add(i);
				}
				else
				{
					Fiu[i] = 3;
				}
				if (RevM[i] == -1)
				{
					Lany[i] = 1;
				}
				else
				{
					Lany[i] = 3;
				}
			}

			//BFS - F2, L2 besorolasa
			var ujfiuk = new HashSet<int>(fiuk);
			var ujfiuk_unchecked = new HashSet<int>();
			do
			{
				ujfiuk_unchecked.Clear();
				foreach (int i in ujfiuk)
				{
					for (int j = 0; j < n; j++)
					{
						if (PirosEl[j, i] && j != M[i])
						{
							Lany[j] = 2;
							int fiuIndex = RevM[j];
							Fiu[fiuIndex] = 2;

							if (!fiuk.Contains(fiuIndex))
							{
								fiuk.Add(fiuIndex);
								ujfiuk_unchecked.Add(fiuIndex);
							}
						}
					}
				}

				ujfiuk = new HashSet<int>(ujfiuk_unchecked);
			} while (ujfiuk.Count > 0);


			//delta kiszamolasa
			float delta = -1.0f;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if ((Fiu[i] == 1 || Fiu[i] == 2) && (Lany[j] == 1 || Lany[j] == 3))
					{
						float tartalek = CF[i] + CL[j] - weights[i * n + j];
						if (delta > tartalek || delta == -1.0f)
						{
							delta = tartalek;
						}
					}
				}
			}

			//cimkeertekek frissitese
			for (int i = 0; i < n; i++)
			{
				if (Fiu[i] == 1 || Fiu[i] == 2)
				{
					CF[i] -= delta;
				}
				if (Lany[i] == 2)
				{
					CL[i] += delta;
				}
			}

			//mik az uj piros elek
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if (PirosEl[j, i] && !(Fiu[i] == 3 && Lany[j] == 2))
					{
						continue;
					}
					PirosEl[j, i] = (CF[i] + CL[j] == weights[i * n + j]);
				}
			}

		} while (true);

	}


	struct BipartitePath
	{
		List<int> vertices;
		HashSet<int> verticesSetEven;
		HashSet<int> verticesSetOdd;

		public BipartitePath(int vertex)
		{
			vertices = new List<int>();
			verticesSetEven = new HashSet<int>();
			verticesSetOdd = new HashSet<int>();

			vertices.Add(vertex);
			verticesSetEven.Add(vertex);
		}

		public BipartitePath(BipartitePath copy)
		{
			vertices = new List<int>(copy.vertices);
			verticesSetEven = new HashSet<int>(copy.verticesSetEven);
			verticesSetOdd = new HashSet<int>(copy.verticesSetOdd);
		}

		public bool hasVertex(int vertex, bool even)
		{
			return even ? verticesSetEven.Contains(vertex) : verticesSetOdd.Contains(vertex);
		}

		public void AddUnsafe(int vertex)
		{
			vertices.Add(vertex);
			if (vertices.Count % 2 == 1)
			{
				verticesSetEven.Add(vertex);
			}
			else
			{
				verticesSetOdd.Add(vertex);
			}
		}

		public int this[int key]
		{
			get
			{
				return vertices[key];
			}
		}

		public int Count { get { return vertices.Count; } }

		public int Last { get { return vertices[vertices.Count - 1]; } }

	}

}
