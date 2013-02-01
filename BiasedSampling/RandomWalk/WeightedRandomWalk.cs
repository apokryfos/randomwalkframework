using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;
using RandomWalks.RandomWalkInterface;
using RandomWalks.Querier;

namespace RandomWalks
{

    public delegate decimal WeightFunction<TGraph, TVertex, TEdge>(TGraph graph, TEdge edge)
        where TGraph : IGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>;


    public class WeightedRandomWalk<TVertex, TEdge> : GeneralRandomWalk<TVertex, TEdge>, IWeightedRandomWalk<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {
        private IWeightedGraphQuerier<TVertex, TEdge> Querier { get; set; }

        public WeightedRandomWalk(TVertex entryPoint, IWeightedGraphQuerier<TVertex, TEdge> graph)
            : base(entryPoint, graph, graph.PolicyName)
        {
            Querier = graph;
        }
             

      
        public override decimal GetStateWeight(TVertex state)
        {
            return Querier.VertexWeight(state);            
        }

        public override decimal GetTransitionWeight(TEdge transition)
        {
            return Querier.EdgeWeight(transition);
        }

        protected override TEdge ChooseNext(TVertex current)
        {
			decimal rand;
			lock (RNG.RNGProvider.RandomSyncRoot) {
				rand = (decimal)r.NextDouble();
			}
            return Querier.WeightedAdjacentEdge(current, rand);
            
           
        }

    }
}
