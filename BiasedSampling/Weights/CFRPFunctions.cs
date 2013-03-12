using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace RandomWalks.Weights {
	public class CFRPFunctions<TVertex, TEdge>				
		where TEdge : IEdge<TVertex> {

		private IUndirectedGraph<TVertex, TEdge> targetGraph;
		public CFRPFunctions(IUndirectedGraph<TVertex, TEdge> targetGraph) {
			this.targetGraph = targetGraph;
		}

		private IDictionary<TVertex, int> TriangleCountCache = null;

		public double TriangleCount(TVertex v) {
			if (TriangleCountCache == null) {
				TriangleCountCache = new Dictionary<TVertex, int>();
			}
			double cnt = 0.0;
			if (!TriangleCountCache.ContainsKey(v)) {
				List<TVertex> n = new List<TVertex>(targetGraph.AdjacentEdges(v).Select(e => e.GetOtherVertex(v)).Where(t => !t.Equals(v)));
				n.Sort();
				for (int i = 1; i < n.Count; i++) {
					for (int j = 0; j < i; j++) {
						if (targetGraph.ContainsEdge(n[i], n[j]) && !n[i].Equals(n[j])) {
							cnt++;
						}
					}
				}
				TriangleCountCache.Add(v, (int)cnt);
			} else {
				cnt = TriangleCountCache[v];
			}
			return cnt;
		}

		public double NumVertices(TVertex v) {
			return 1.0;
		}

		private static Dictionary<IUndirectedGraph<TVertex, TEdge>, CFRPFunctions<TVertex, TEdge>> cache = new Dictionary<IUndirectedGraph<TVertex,TEdge>,CFRPFunctions<TVertex,TEdge>>();

		public static Func<TVertex, double>[] GetFunctions(IUndirectedGraph<TVertex, TEdge> targetGraph) {
			

			CFRPFunctions<TVertex, TEdge> t;
			if (!cache.TryGetValue(targetGraph, out t)) {
				t = new CFRPFunctions<TVertex, TEdge>(targetGraph);
				cache.Add(targetGraph, t);
			}
			return new Func<TVertex, double>[] { t.TriangleCount, t.NumVertices };
		}

	}
}
