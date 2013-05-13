using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;

namespace RandomWalkAnalysis {

	[Flags]
	public enum ObserverType {
		NONE = 0x0,
		STEP = 0x1,
		REVISITS = 0x2,
		COVERAGE = 0x4,
		DEGREECOVER = 0x10
	}



	public class RandomWalkStepObserver<TVertex, TEdge> : RandomWalkObserver<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		public RandomWalkStepObserver(IRandomWalk<TVertex, TEdge> sampler)
			: base(sampler) {
		}


		protected override void Observed_Transition(IRandomWalk<TVertex, TEdge> sampler, TVertex previous, TVertex current, TEdge transition, decimal weight) {
			OnObservation(current, transition, weight);
		}

		public override void Dispose() {

		}
	}

	public class RandomWalkRevisitObserver<TVertex, TEdge> : RandomWalkObserver<TVertex, TEdge>
		 where TEdge : IEdge<TVertex> {
		public Dictionary<TVertex, List<decimal>> VisitSteps { private set; get; }
		public Dictionary<TVertex, decimal> StateWeights { private set; get; }


		public RandomWalkRevisitObserver(IRandomWalk<TVertex, TEdge> sampler)
			: base(sampler) {
			VisitSteps = new Dictionary<TVertex, List<decimal>>();
			StateWeights = new Dictionary<TVertex, decimal>();
		}



		protected override void Observed_Transition(IRandomWalk<TVertex, TEdge> sampler, TVertex previous, TVertex current, TEdge transition, decimal weight) {

			List<decimal> vsv;
			if (!VisitSteps.TryGetValue(current, out vsv)) {
				vsv = new List<decimal>();
				VisitSteps.Add(current, vsv);
				StateWeights.Add(current, sampler.GetStateWeight(current));
			}
			vsv.Add(sampler.TotalSteps);
			OnObservation(current, transition, StateWeights[current]);

		}

		public override void Dispose() {
			VisitSteps.Clear();
			StateWeights.Clear();
		}
	}


	public class RandomWalkCoverageObserver<TVertex, TEdge> : RandomWalkObserver<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		HashSet<TVertex> visited;

		public int VisitedStates { get { return visited.Count; } }
		public decimal Coverage { get { return ((decimal)VisitedStates / (decimal)TotalStates); } }
		public int TotalStates { get; private set; }

		public RandomWalkCoverageObserver(IRandomWalk<TVertex, TEdge> sampler, int totalStates)
			: base(sampler) {
			TotalStates = totalStates;
			visited = new HashSet<TVertex>();
		}


		protected override void Observed_Transition(IRandomWalk<TVertex, TEdge> sampler, TVertex previous, TVertex current, TEdge transition, decimal weight) {
			if (visited.Add(current))
				OnObservation(current, transition, weight);
		}


		public override void Dispose() {

		}
	}

	public class RandomWalkDegreeCoverageObserver<TVertex, TEdge> : RandomWalkObserver<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {
		private Dictionary<int, int> degreeCounts;
		private RandomWalkCoverageObserver<TVertex, TEdge> coverage;
		private IUndirectedGraph<TVertex, TEdge> targetGraph;

		public RandomWalkDegreeCoverageObserver(IRandomWalk<TVertex, TEdge> obs, IUndirectedGraph<TVertex, TEdge> targetGraph)
			: base(obs) {
			this.targetGraph = targetGraph;
			degreeCounts = new Dictionary<int, int>();
			foreach (var v in targetGraph.Vertices) {
				if (degreeCounts.ContainsKey(targetGraph.AdjacentDegree(v)))
					degreeCounts[targetGraph.AdjacentDegree(v)]++;
				else
					degreeCounts.Add(targetGraph.AdjacentDegree(v), 1);
			}
			coverage = new RandomWalkCoverageObserver<TVertex, TEdge>(obs, targetGraph.VertexCount);
			coverage.ObservationEvent += new ObserverEvent<TVertex, TEdge>(coverage_Hit);

		}

		void coverage_Hit(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object parameters) {
			degreeCounts[targetGraph.AdjacentDegree(current)]--;
			if (degreeCounts[targetGraph.AdjacentDegree(current)] == 0)
				OnObservation(current, transition, targetGraph.AdjacentDegree(current));
		}


		protected override void Observed_Transition(IRandomWalk<TVertex, TEdge> sampler, TVertex previous, TVertex current, TEdge transition, decimal weight) {

		}

		public override void Dispose() {
			degreeCounts.Clear();
		}
	}
}
