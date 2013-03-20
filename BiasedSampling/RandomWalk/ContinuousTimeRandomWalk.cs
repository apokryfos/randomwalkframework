using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;
using RandomWalks.Querier;

namespace RandomWalks.RandomWalk {


	public class RandomWalkWithDistributionalIncrements<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IWeightedRandomWalk<TVertex, TEdge>
	where TEdge : IEdge<TVertex> {

		public RandomWalkWithDistributionalIncrements(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> dummy)
			: this(entryPoint, targetGraph) {
		}

		public RandomWalkWithDistributionalIncrements(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph)
			: base(entryPoint, targetGraph, new KeyValuePair<string, string>("CTSRW", "Continuous Time Simple Random Walk")) {
			Name = new KeyValuePair<string, string>("CTSRW", "Continuous Time Triangle Random Walk");
		}

		public override decimal TimeIncrement {
			get {
				return 1.0M / (decimal)base.GetAdjacentTransitionCount(CurrentState);
			}
		}
	}
	public class ContinuousTimeSimpleRandomWalk<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IWeightedRandomWalk<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		public ContinuousTimeSimpleRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> dummy)
			: this(entryPoint, targetGraph) {
		}

		public ContinuousTimeSimpleRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph)
			: base(entryPoint, targetGraph, new KeyValuePair<string, string>("CTSRW", "Continuous Time Simple Random Walk")) {
			Name = new KeyValuePair<string, string>("CTSRW", "Continuous Time Simple Random Walk");
		}

		public override decimal TimeIncrement {
			get {
				return 1.0M / (decimal)base.GetAdjacentTransitionCount(CurrentState);
			}
		}
	}

	public class ContinuousTimeRandomWalkWithNegativeExponentialTimeIncrements<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IWeightedRandomWalk<TVertex, TEdge>
	   where TEdge : IEdge<TVertex> {

		public ContinuousTimeRandomWalkWithNegativeExponentialTimeIncrements(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph, KeyValuePair<string, string> dummy)
			: this(entryPoint, targetGraph) {
		}

		public ContinuousTimeRandomWalkWithNegativeExponentialTimeIncrements(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph)
			: base(entryPoint, targetGraph, new KeyValuePair<string, string>("CTSRW", "Continuous Time Simple Random Walk")) {
			Name = new KeyValuePair<string, string>("CTRW", "Continuous Time Random Walk");
		}
		public override decimal TimeIncrement {
			get {
				double random = r.NextDouble();
				return (decimal)(-(Math.Log(random))) / (decimal)base.GetAdjacentTransitionCount(CurrentState);
			}
		}


	}
}
