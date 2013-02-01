using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomWalks.RandomWalkInterface;
using QuickGraph;

namespace RandomWalkAnalysis.Samplers
{
    public interface ITerminationConditions<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {
        bool CheckCondition();
    }



    public class CoverageTerminationCondition<TVertex, TEdge> : ITerminationConditions<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        protected IWeightedRandomWalk<TVertex, TEdge> Walk { get; set; }
        private decimal CoverTarget { get; set; }
        RandomWalkCoverageObserver<TVertex, TEdge> CoverageObserver { get; set; }

        public CoverageTerminationCondition(IWeightedRandomWalk<TVertex, TEdge> rw, int totalVertices ,decimal coverPC)
        {
            Walk = rw;
            CoverageObserver = new RandomWalkCoverageObserver<TVertex, TEdge>(rw, totalVertices);
            CoverTarget = coverPC;
        }

        public virtual bool CheckCondition()
        {
            return CoverageObserver.Coverage >= CoverTarget;
        }

        public static CoverageTerminationCondition<TVertex, TEdge> CoveragePCCondition(IWeightedRandomWalk<TVertex, TEdge> rw, int vertexCount, int loopCount, decimal coverage, int i)
        {
            var r = new CoverageTerminationCondition<TVertex, TEdge>(rw, vertexCount, coverage);
            return r;
        }

    }

  

    public class StepsTerminationCondition<TVertex, TEdge> : ITerminationConditions<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {
        protected IWeightedRandomWalk<TVertex, TEdge> Walk { get; set; }
        protected decimal StepTarget { get; set; }

        public StepsTerminationCondition(IWeightedRandomWalk<TVertex, TEdge> rw, decimal steps)
        {
            Walk = rw;
            StepTarget = steps;
        }

        public virtual bool CheckCondition()
        {
            return Walk.DiscreetSteps >= StepTarget;
        }

        public static StepsTerminationCondition<TVertex, TEdge> IncrementalStepsConditions(IWeightedRandomWalk<TVertex, TEdge> rw, int loopCount, decimal maxTarget, int i)
        {
            var r = new StepsTerminationCondition<TVertex, TEdge>(rw, (decimal)(((decimal)(i + 1) / (decimal)loopCount) * (decimal)maxTarget));
            return r;
        }

        public static StepsTerminationCondition<TVertex, TEdge> SublinearStepsCondition(IWeightedRandomWalk<TVertex, TEdge> rw, int vertexCount, int loopCount, decimal minExponent, decimal maxTargetExponent, int i)
        {
            decimal step = (maxTargetExponent - minExponent) / (decimal)loopCount;


            var r = new StepsTerminationCondition<TVertex, TEdge>(rw, (decimal)Math.Pow(vertexCount, (double)(minExponent + (i+1)*step)));
            return r;
        }

    }

    public class TimeTerminationCondition<TVertex, TEdge> : StepsTerminationCondition<TVertex, TEdge>
          where TEdge : IEdge<TVertex>
    {
        public TimeTerminationCondition(IWeightedRandomWalk<TVertex, TEdge> rw, decimal time)
            : base(rw, time)
        {   
        }

        public override bool CheckCondition()
        {
            return Walk.TotalSteps >= StepTarget;
        }

    }


    public class RehitsTerminationCondition<TVertex, TEdge> : StepsTerminationCondition<TVertex, TEdge>
       where TEdge : IEdge<TVertex>
        
    {   
        private int Rehits { get; set; }
        private RandomWalkRevisitObserver<TVertex, TEdge> Observer;
        private int maxRehits = 0;

        public RehitsTerminationCondition(IWeightedRandomWalk<TVertex, TEdge> rw, int rehits)
            : base(rw, 0)
        {
            Walk = rw;
            Rehits = rehits;
            Observer = new RandomWalkRevisitObserver<TVertex, TEdge>(rw);
            Observer.ObservationEvent += new ObserverEvent<TVertex, TEdge>(Observer_ObservationEvent);
        }

        void Observer_ObservationEvent(RandomWalkObserver<TVertex, TEdge> sampler, TVertex current, TEdge transition, object ObservationParameters)
        {
            RandomWalkRevisitObserver<TVertex, TEdge> obs = (RandomWalkRevisitObserver<TVertex, TEdge>)sampler;
             if (obs.VisitSteps[current].Count > maxRehits)
                 maxRehits = obs.VisitSteps[current].Count;
        }

        public override bool CheckCondition()
        {
            return maxRehits >= Rehits;
        }

        public static RehitsTerminationCondition<TVertex, TEdge> IncrementalRehitsConditions(IWeightedRandomWalk<TVertex, TEdge> rw, int initialTarget, int maxTarget, int loopCount, int i)
        {
            var r = new RehitsTerminationCondition<TVertex, TEdge>(rw, initialTarget + (int)(((decimal)(i + 1) / (decimal)loopCount) * (decimal)(maxTarget - initialTarget)));                
            return r;
        }
    }
}
