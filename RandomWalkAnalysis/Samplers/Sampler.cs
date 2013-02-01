using System;
using System.Collections.Generic;
using QuickGraph;
using RandomWalkAnalysis.PropertyEstimators;
using RandomWalks.RandomWalkInterface;

namespace RandomWalkAnalysis.Samplers {
	public class Sampler<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		public const int vcount = 500;

		public virtual IWeightedRandomWalk<TVertex, TEdge> RandomWalk { get; protected set; }
		protected List<RandomWalkCumulativeLogger<TVertex, TEdge>> loggers = new List<RandomWalkCumulativeLogger<TVertex, TEdge>>();
		protected List<RandomWalkObserver<TVertex, TEdge>> observers = new List<RandomWalkObserver<TVertex, TEdge>>();
		protected GraphWeightEstimation<TVertex, TEdge> Estimator { get; set; }
		protected MostVerticesRehitsDistribution<TVertex, TEdge> FEstimator { get; set; }
		protected ITerminationConditions<TVertex, TEdge> terminationCondition;



		public virtual void AttachLoggers(LoggerType l, IUndirectedGraph<TVertex, TEdge> graph, string logPath) {
			string NameBase = logPath + "\\" + RandomWalk.Name.Key + "-";

			if ((l & (LoggerType.STEP | LoggerType.HITS | LoggerType.RANDOMVARIABLE | LoggerType.HIDDENPARTITION)) != 0) {
				var rwSO = new RandomWalkStepObserver<TVertex, TEdge>(RandomWalk);
				observers.Add(rwSO);

				if (l.HasFlag(LoggerType.STEP)) {
					var rwsl = new RandomWalkStepLogger<TVertex, TEdge>(rwSO, NameBase + "STEP.csv");
					loggers.Add(rwsl);
				}
				if (l.HasFlag(LoggerType.HITS))
					loggers.Add(new RandomWalkHitsLogger<TVertex, TEdge>(rwSO, NameBase + "HITS.csv"));

				if (l.HasFlag(LoggerType.RANDOMVARIABLE)) {
					Func<RandomWalkObserver<TVertex, TEdge>, TVertex, TEdge, double> function = ((rw, v, e) => 1.0 / (double)rw.Observed.GetStateWeight(v));
					loggers.Add(new RandomWalkRandomVariableIncrementalLogger<TVertex, TEdge>(rwSO, NameBase + "CTRW.csv", function));
				}
				if (l.HasFlag(LoggerType.HIDDENPARTITION) && typeof(TVertex) == typeof(int))
					loggers.Add(new RandomWalkHiddenPartitionLogger<TVertex, TEdge>(rwSO, NameBase + "HP.csv", (v => Convert.ToInt32(v) <= vcount)));

			}

			if (l.HasFlag(LoggerType.REVISITS)) {
				var rwRO = new RandomWalkRevisitObserver<TVertex, TEdge>(RandomWalk);
				observers.Add(rwRO);
				loggers.Add(new RandomWalkRevisitsLogger<TVertex, TEdge>(rwRO, NameBase + "REVISITS.csv"));
				//Estimator = new GraphWeightEstimation<TVertex, TEdge>(rwRO, NameBase);
				// FEstimator = new MostVerticesRehitsDistribution<TVertex, TEdge>(rwRO, NameBase);
			}
			if (l.HasFlag(LoggerType.DEGREECOVER)) {
				var rwDO = new RandomWalkDegreeCoverageObserver<TVertex, TEdge>(RandomWalk, graph);
				observers.Add(rwDO);
				loggers.Add(new RandomWalkUndirectedDegreeCoverageLogger<TVertex, TEdge>(rwDO, NameBase + "DCOVER.csv"));
			}

		}

		public Sampler(IWeightedRandomWalk<TVertex, TEdge> rw, ITerminationConditions<TVertex, TEdge> c) {
			this.RandomWalk = rw;
			terminationCondition = c;
			RandomWalk.Terminated += new EventHandler(RandomWalk_Terminated);
			Estimator = null;
			FEstimator = null;
		}

		protected virtual void RandomWalk_Terminated(object sender, EventArgs e) {
			if (Estimator != null) {
				Estimator.WriteEstimates();
			}
			if (FEstimator != null) {
				FEstimator.WriteEstimates();
			}

			RandomWalk = null;
			foreach (var l in loggers) {
				l.Dispose();
			}
		}

		protected bool CheckCondition() {
			return terminationCondition.CheckCondition();
		}

		protected virtual TVertex SampleOne() {
			if (RandomWalk != null) {
				return RandomWalk.NextSample();
			} else {
				return default(TVertex);
			}
		}


		public virtual void Sample(object nothing) {
			bool conditionReached = false;
			while (!conditionReached) {
				var s = SampleOne();
				conditionReached = CheckCondition() || (s.Equals(default(TVertex)) && RandomWalk == null);
			}
			if (RandomWalk != null) {
				RandomWalk.Terminate();
			}

			foreach (var l in this.loggers) {
				l.Dispose();
			}

			foreach (var o in this.observers) {
				o.Dispose();
			}
		}
	}
}
