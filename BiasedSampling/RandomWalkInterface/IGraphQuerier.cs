using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace RandomWalks.RandomWalkInterface
{
    public interface IGraphQuerier<TVertex, TEdge> : IDisposable        
        where TEdge : IEdge<TVertex>
    {
        int TotalQueries { get; }
        IEnumerable<TEdge> AdjecentEdges(TVertex vertex);
        int AdjecentDegree(TVertex vertex);
        TEdge AdjecentEdge(TVertex vertex, int index);

		KeyValuePair<string, string> PolicyName { get; }
    }

    public interface IWeightedGraphQuerier<TVertex, TEdge> : IGraphQuerier<TVertex, TEdge>		
        where TEdge : IEdge<TVertex>
                       
    {

        TEdge WeightedAdjacentEdge(TVertex vertex, decimal weightedIndex);
        decimal EdgeWeight(TEdge edge);
        decimal VertexWeight(TVertex vertex);
    }


   

   
}
