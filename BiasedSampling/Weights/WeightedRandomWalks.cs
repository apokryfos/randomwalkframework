using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;
using RandomWalks.Querier;



namespace RandomWalks.Weights {
	/// <summary>
	/// Examples on how to use the <see cref="WeightFunction"/>
	/// These methods generally provide an implementation for the weight function delegate
	/// <para>
	///	These weights were used in various experiments
	/// </para>
	/// </summary>
	public static class HighDegreeBias {
		public static decimal EdgeWeight<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> targetGraph, TEdge e, double beta)
		  where TEdge : IEdge<TVertex> { 
			return (decimal)Math.Pow((targetGraph.AdjacentDegree(e.Source) * targetGraph.AdjacentDegree(e.Target)), beta);
		}
	}
	
	public class HiddenPartitionRandomWalkWeight<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		private int vcount;
		private decimal c;
		public HiddenPartitionRandomWalkWeight(decimal c, int vcount) {
			this.c = c;
			this.vcount = vcount;
		}
		public decimal EdgeWeight(IGraph<TVertex, TEdge> targetGraph, TEdge e) {
			return (1.0M + (Convert.ToInt32(e.Source) < vcount ? c : 0) + (Convert.ToInt32(e.Target) < vcount ? c : 0));
		}
	}

	/// <summary>
	/// General weight to use for weighted random walks.
	/// Tag must be convertible to decimal. 
	/// Tags are commonly (signed) real or natural numbers
	/// If you deal with different weights consider creating another delegate implementation
	/// </summary>
	/// <typeparam name="TVertex">Vertex Type</typeparam>
	/// <typeparam name="TEdge">Edge Type</typeparam>
	/// <typeparam name="TTag">Edge tag type</typeparam>
	public static class GeneralWeight<TVertex, TEdge, TTag>
		where TEdge : IEdge<TVertex> 
		where TEdge : ITagged<TTag> {
		private int vcount;
		private decimal c;

		public decimal EdgeWeight(IGraph<TVertex, TEdge> targetGraph, TEdge e) {
			return Convert.ToDecimal(e.Tag);
		}
	}

	public static class VertexRandomWalkWeight {
		public static decimal EdgeWeight<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> targetGraph, TEdge e)
			where TEdge : IEdge<TVertex> {
			return ((1.0M / (decimal)targetGraph.AdjacentDegree(e.Source)) + (1.0M / (decimal)targetGraph.AdjacentDegree(e.Target)));
		}
	}

	public static class TriangleRandomWalkWeight {
		public static decimal EdgeWeight<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> targetGraph, TEdge e)
			where TEdge : IEdge<TVertex> {
			if (e.Source.Equals(e.Target))
				return 1.0M;

			int t = 0;
			HashSet<TVertex> hs = new HashSet<TVertex>();
			foreach (var ed in targetGraph.AdjacentEdges(e.Source)) {
				if (e.Equals(ed) || ed.Source.Equals(ed.Target))
					continue;
				hs.Add(ed.GetOtherVertex(e.Source));
			}

			foreach (var ed in targetGraph.AdjacentEdges(e.Target)) {
				if (e.Equals(ed) || ed.Source.Equals(ed.Target))
					continue;

				if (hs.Contains(ed.GetOtherVertex(e.Target)))
					t++;
			}
			return 1.0M + (decimal)t;
		}

	}
}
