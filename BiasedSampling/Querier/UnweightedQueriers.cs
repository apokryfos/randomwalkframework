using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomWalks.RandomWalkInterface;
using QuickGraph;
using System.Reflection;

namespace RandomWalks.Querier
{
	/// <summary>
	/// Generalization of a graph querier with specific applicability to unweighted random walks.
	/// Implements <see cref="IGraphQuerier"/>
	/// </summary>
	/// <typeparam name="TVertex">The type of the vertices (states)</typeparam>
	/// <typeparam name="TEdge">The type of the edges</typeparam>
    public class UnweightedGraphQuerier<TVertex, TEdge> : IGraphQuerier<TVertex, TEdge>        		
        where TEdge : IEdge<TVertex>
    {
        
        protected IGraph<TVertex,TEdge> targetGraph;
		protected IGraphQuerier<TVertex, TEdge> internalQuerier;

		private class UndirectedGraphQuerier : IGraphQuerier<TVertex, TEdge> {
			private IUndirectedGraph<TVertex, TEdge> targetGraph;

			public UndirectedGraphQuerier(IUndirectedGraph<TVertex, TEdge> targetGraph) { this.targetGraph = targetGraph; }

			#region IGraphQuerier<TVertex,TEdge> Members

			public int TotalQueries { get; set; }
			public IEnumerable<TEdge> AdjecentEdges(TVertex vertex) { TotalQueries++; return targetGraph.AdjacentEdges(vertex); }
			public int AdjecentDegree(TVertex vertex) { TotalQueries++; return targetGraph.AdjacentDegree(vertex); }
			public TEdge AdjecentEdge(TVertex vertex, int index) { TotalQueries++; return targetGraph.AdjacentEdge(vertex, index); }
			public KeyValuePair<string, string> PolicyName { get; set; }
			#endregion

			#region IDisposable Members
			public void Dispose() { }
			#endregion
		}
		private class DirectedGraphQuerier : IGraphQuerier<TVertex, TEdge> {			
			private IVertexAndEdgeListGraph<TVertex, TEdge> targetGraph;

			public DirectedGraphQuerier(IVertexAndEdgeListGraph<TVertex, TEdge> targetGraph) { this.targetGraph = targetGraph; }

			#region IGraphQuerier<TVertex,TEdge> Members
			public int TotalQueries { get; set; }
			public IEnumerable<TEdge> AdjecentEdges(TVertex vertex) { TotalQueries++; return targetGraph.OutEdges(vertex); }
			public int AdjecentDegree(TVertex vertex) { TotalQueries++; return targetGraph.OutDegree(vertex); }
			public TEdge AdjecentEdge(TVertex vertex, int index) { TotalQueries++; return targetGraph.OutEdge(vertex, index); }
			public KeyValuePair<string, string> PolicyName { get; set; }
			#endregion

			#region IDisposable Members
			public void Dispose() { }
			#endregion
		}
		private class BidirectionalGraphQuerier : IGraphQuerier<TVertex, TEdge> {			
			private IBidirectionalGraph<TVertex, TEdge> targetGraph;

			public BidirectionalGraphQuerier(IBidirectionalGraph<TVertex, TEdge> targetGraph) { this.targetGraph = targetGraph; }

			#region IGraphQuerier<TVertex,TEdge> Members
			public int TotalQueries { get; set; }
			public IEnumerable<TEdge> AdjecentEdges(TVertex vertex) { TotalQueries+=2; return targetGraph.OutEdges(vertex).Concat(targetGraph.InEdges(vertex)); }
			public int AdjecentDegree(TVertex vertex) { TotalQueries++; return targetGraph.Degree(vertex); }
			public TEdge AdjecentEdge(TVertex vertex, int index) 
			{
				TotalQueries += 2;
				if (index >= targetGraph.OutDegree(vertex)) {
					return targetGraph.InEdge(vertex, index - targetGraph.OutDegree(vertex));
				} else {
					return targetGraph.OutEdge(vertex, index);
				}				
			}
			public KeyValuePair<string, string> PolicyName { get; set; }
			#endregion

			#region IDisposable Members
			public void Dispose() { }
			#endregion
		}


		/// <summary>
		/// Initializes the querier on the specified graph and a default name.
		/// </summary>
		/// <param name="targetGraph">The QuickGraph.IGraph object to use.</param>
		public UnweightedGraphQuerier(IGraph<TVertex, TEdge> targetGraph) 
			: this(targetGraph, new KeyValuePair<string,string>("SRW","Simple Random Walk")) {
		}
		
		public UnweightedGraphQuerier(IGraph<TVertex, TEdge> targetGraph, KeyValuePair<string, string> policyName)
        {
            this.targetGraph = targetGraph;
			this.PolicyName = policyName;
			if (typeof(IUndirectedGraph<TVertex, TEdge>).IsAssignableFrom(targetGraph.GetType())) {
				internalQuerier = new UnweightedGraphQuerier<TVertex, TEdge>.UndirectedGraphQuerier((IUndirectedGraph<TVertex, TEdge>)targetGraph);
			} else if (typeof(IBidirectionalGraph<TVertex, TEdge>).IsAssignableFrom(targetGraph.GetType())) {
				internalQuerier = new UnweightedGraphQuerier<TVertex, TEdge>.BidirectionalGraphQuerier((IBidirectionalGraph<TVertex, TEdge>)targetGraph);
			} else if (typeof(IVertexAndEdgeListGraph<TVertex, TEdge>).IsAssignableFrom(targetGraph.GetType())) {
				internalQuerier = new UnweightedGraphQuerier<TVertex, TEdge>.DirectedGraphQuerier((IVertexAndEdgeListGraph<TVertex, TEdge>)targetGraph);
			} else {
				throw new NotSupportedException();
			}
				

		}

		#region IGraphQuerier Members

		public virtual IEnumerable<TEdge> AdjecentEdges(TVertex vertex) {
			return internalQuerier.AdjecentEdges(vertex);
		}


		public virtual int AdjecentDegree(TVertex vertex) {
			return internalQuerier.AdjecentDegree(vertex);
		}

		public virtual TEdge AdjecentEdge(TVertex vertex, int index) {
			return internalQuerier.AdjecentEdge(vertex, index);
		}

		public int TotalQueries { get { return internalQuerier.TotalQueries; } }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            
        }

		public KeyValuePair<string, string> PolicyName {
			get;
			protected set;
		}

		#endregion
	}


   
}
