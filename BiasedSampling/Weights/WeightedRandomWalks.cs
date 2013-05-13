using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;
using RandomWalks.Querier;
using DataStructures;
using System.Diagnostics;



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

	public static class AgeBias {
		public static decimal EdgeWeight<TEdge>(IUndirectedGraph<int, TEdge> targetGraph, TEdge e, double c)
		  where TEdge : IEdge<int> {
			return (decimal)((e.Source + e.Target) * c);
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
		where TEdge : IEdge<TVertex>, ITagged<TTag> {				
		public static decimal EdgeWeight(IGraph<TVertex, TEdge> targetGraph, TEdge e) {
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
				return TriangleRandomWalkWeight.EdgeWeight<TVertex, TEdge>(targetGraph, e, 1.0);
		}
		
		public static decimal EdgeWeight<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> targetGraph, TEdge e, double c)
			where TEdge : IEdge<TVertex> {
			if (e.Source.Equals(e.Target))
				return 1.0M;
			
			var t = targetGraph.GetContainingTriangles(e);
			int cnt = t.Count();			
			return 1.0M + (decimal)c*(decimal)cnt;
		}

	}
}
