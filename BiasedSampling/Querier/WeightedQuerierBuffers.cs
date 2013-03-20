using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomWalks.RandomWalkInterface;
using QuickGraph;
using System.Threading;

namespace RandomWalks.Querier {

	public delegate decimal WeightFunction<TGraph, TVertex, TEdge>(TGraph graph, TEdge edge)
		where TGraph : IGraph<TVertex, TEdge>
		where TEdge : IEdge<TVertex>;


	public class UnbufferedWeightedQuerier<TVertex, TEdge> : GeneralWeightedQuerier<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		public UnbufferedWeightedQuerier(IGraph<TVertex, TEdge> targetGraph, WeightFunction<IGraph<TVertex, TEdge>, TVertex, TEdge> wf, KeyValuePair<string, string> policyName)
			: base(targetGraph, wf, policyName, 0) {
		}
	}
	public class WeightedGraphQuerier<TVertex, TEdge> : GeneralWeightedQuerier<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		public WeightedGraphQuerier(IGraph<TVertex, TEdge> targetGraph, WeightFunction<IGraph<TVertex, TEdge>, TVertex, TEdge> wf, KeyValuePair<string, string> policyName)

			: base(targetGraph, wf, policyName,int.MaxValue) {
		}
	}
}
