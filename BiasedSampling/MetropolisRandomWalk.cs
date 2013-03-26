using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using QuickGraph;
using RandomWalks.RandomWalkInterface;
using RandomWalks.Querier;

namespace RandomWalks
{

	public class MetropolisRandomWalk<TVertex, TEdge> : RandomWalk<TVertex, TEdge>, IRandomWalk<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        public MetropolisRandomWalk(TVertex entryPoint, UnweightedGraphQuerier<TVertex, TEdge> targetGraph)
            : base(entryPoint, targetGraph, new KeyValuePair<string, string>("MHRW", "Metropolis Hastings Random Walk"))
        { }

        protected override TEdge ChooseNext(TVertex current)
        {
            var e = base.ChooseNext(current);
            var v = e.GetOtherVertex(current);
            var random = r.NextDouble();
            if ((decimal)random <= (decimal)GetAdjacentTransitionCount(current) / (decimal)GetAdjacentTransitionCount(v))
                return e;
            else
                return default(TEdge);
        }

        public override decimal GetStateWeight(TVertex state)
        {
            return 1.0M;
        }  


        public override decimal GetTransitionWeight(TEdge transition)
        {
            if (!Object.ReferenceEquals(transition, default(TEdge)))
                return Math.Min(1.0M / (decimal)GetAdjacentTransitionCount(transition.Source), 1.0M / (decimal)GetAdjacentTransitionCount(transition.Target));
            else return 1.0M - 1.0M / (decimal)GetAdjacentTransitionCount(CurrentState);
        }



       
    }
}
