using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomWalks.RandomWalkInterface;
using QuickGraph;
using RandomWalks.Querier;

namespace RandomWalks.RandomWalk {
	public abstract class LazySimpleRandomWalk<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IRandomWalk<TVertex, TEdge>
	  where TEdge : IEdge<TVertex> {
		public LazySimpleRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> name)
			: base(entryPoint, targetGraph, name) {
			Name = new KeyValuePair<string, string>("LSRW", "Lazy Simple Random Walk");
		}

		protected override TEdge ChooseNext(TVertex current) {
			if (r.NextDouble() <= 0.5)
				return base.ChooseNext(current);
			else
				return default(TEdge);
		}

		public override TEdge GetAdjacentTransition(TVertex state, int index) {
			if (index < 0 || index >= base.GetAdjacentTransitionCount(state))
				return default(TEdge);
			return base.GetAdjacentTransition(state, index);
		}

		public override int GetAdjacentTransitionCount(TVertex state) {
			return base.GetAdjacentTransitionCount(state) + 1;
		}

		public override decimal GetTransitionWeight(TEdge transition) {
			if (transition.Equals(default(TEdge)))
				return 0.5M;
			return base.GetTransitionWeight(transition) * 0.5M;
		}
	}
	public abstract class LazyWeightedRandomWalk<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IRandomWalk<TVertex, TEdge>
	  where TEdge : IEdge<TVertex> {
		public LazyWeightedRandomWalk(TVertex entryPoint, WeightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> name)
			: base(entryPoint, targetGraph, new KeyValuePair<string, string>("L" + targetGraph.PolicyName.Key, "Lazy " + targetGraph.PolicyName.Value)) {
			

		}
		protected override TEdge ChooseNext(TVertex current) {
			if (r.NextDouble() <= 0.5)
				return base.ChooseNext(current);
			else
				return default(TEdge);
		}

		public override TEdge GetAdjacentTransition(TVertex state, int index) {
			if (index < 0 || index >= base.GetAdjacentTransitionCount(state))
				return default(TEdge);
			return base.GetAdjacentTransition(state, index);
		}

		public override int GetAdjacentTransitionCount(TVertex state) {
			return base.GetAdjacentTransitionCount(state) + 1;
		}

		public override decimal GetTransitionWeight(TEdge transition) {
			if (transition.Equals(default(TEdge)))
				return 0.5M;
			return base.GetTransitionWeight(transition) * 0.5M;
		}

	}

}
