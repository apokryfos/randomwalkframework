using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;
using RandomWalks.Querier;


namespace RandomWalks {

		/// <summary>
		/// A simple random walk on an undirected graph		
		/// </summary>
		/// <typeparam name="TVertex"></typeparam>
		/// <typeparam name="TEdge"></typeparam>
	public class SimpleRandomWalk<TVertex, TEdge> : GeneralRandomWalk<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		public SimpleRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> dummy)
			: this(entryPoint, targetGraph) {
		}

		public SimpleRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph)
			: base(entryPoint, targetGraph, new KeyValuePair<string, string>("SRW", "Simple Random Walk")) {
		}

		protected override TEdge ChooseNext(TVertex current) {
			if (GetAdjacentTransitionCount(current) == 0) {
				return default(TEdge);
			}
			int adtrans;
			adtrans = r.Next(GetAdjacentTransitionCount(current));
			

			return GetAdjacentTransition(current, adtrans);

		}


		public override decimal GetStateWeight(TVertex state) {
			return (decimal)GetAdjacentTransitionCount(state);
		}

		public override decimal GetTransitionWeight(TEdge transition) {
			return (decimal)1;
		}
	}
}
