using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph.Algorithms;
using QuickGraph;
using DataStructures;

namespace RandomWalks.Weights {

	public static class UndirectedGraphTrianglesExtensions	{


		private static IEnumerable<TriangleGeneric<TVertex>> GetTrianglesOptmized<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> graph)
			where TEdge : IEdge<TVertex> {
			List<TVertex> verticesSorted = new List<TVertex>(graph.Vertices);
			verticesSorted.Sort();
			for (int i = 0; i < verticesSorted.Count; i++) {
				foreach (var e in graph.AdjacentEdges(verticesSorted[i])) {
					if (((IComparable<TVertex>)e.GetOtherVertex(verticesSorted[i])).CompareTo(verticesSorted[i]) <= 0)
						continue;
					foreach (var t in GetContainingTriangles(graph, e))
						yield return t;
				}
			}
			yield break;
		}

		public static IEnumerable<TriangleGeneric<TVertex>> GetTriangles<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> graph) 
			where TEdge : IEdge<TVertex> {
			if (typeof(IComparable<TVertex>).IsAssignableFrom(typeof(TVertex))) {
				foreach (var t in GetTrianglesOptmized(graph)) {
					yield return t;
				}
				yield break;
			}

			HashSet<TVertex> checkedV = new HashSet<TVertex>();
			foreach (var v in graph.Vertices) {
				foreach (var t in GetContainingTriangles(graph, v)) {
					yield return t;
				}
			}
			yield break;
		}


		public static IEnumerable<TriangleGeneric<TVertex>> GetContainingTriangles<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> graph, TVertex v) 
			where TEdge : IEdge<TVertex> {		
			HashSet<TVertex> checkedV = new HashSet<TVertex>();
			foreach (var e in graph.AdjacentEdges(v)) {
				if (checkedV.Contains(e.GetOtherVertex(v)))
					continue;
				foreach (var t in GetContainingTriangles(graph, e))
					yield return t;
				checkedV.Add(v);
			}
			yield break;
		}

		public static IEnumerable<TriangleGeneric<TVertex>> GetContainingTriangles<TVertex, TEdge>(this IUndirectedGraph<TVertex, TEdge> graph, TEdge edge)
			where TEdge : IEdge<TVertex> {	
			HashSet<TVertex> s = new HashSet<TVertex>(graph.AdjacentEdges(edge.Source).Select(e => e.Source.Equals(edge.Source) ? e.Target : e.Source));
			s.IntersectWith(graph.AdjacentEdges(edge.Target).Select(e => e.Source.Equals(edge.Target) ? e.Target : e.Source));
			return s.Select<TVertex, TriangleGeneric<TVertex>>(v => new TriangleGeneric<TVertex>(edge.Source, edge.Target, v)).Where(t => t.IsValid);
		}

	}


}
