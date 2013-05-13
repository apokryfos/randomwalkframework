using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;
using System.Threading;

namespace RandomWalks.Querier
{

	/// <summary>
	/// Generalization of a graph querier with specific applicability to weighted random walks.
	/// Based on <see cref="UnweightedGraphQuerier"/> with added functionality for weighted queries.	
	/// Keeps the ordered weighted edge lists in memory for states with degrees higher than a specified threshold
	/// </summary>
	/// <typeparam name="TVertex">Vertex(state) type</typeparam>
	/// <typeparam name="TEdge">Edge(transition) type</typeparam>
    public class GeneralWeightedQuerier<TVertex, TEdge> : UnweightedGraphQuerier<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {

      
		protected WeightFunction<IGraph<TVertex,TEdge>, TVertex, TEdge> wf;
		protected ReaderWriterLockSlim SyncRoot { get; private set; }

		protected int DegreeThreshold { get; set; }

		/// <summary>
		/// Constructs a general weighted graph querier.
		/// </summary>
		/// <param name="targetGraph">The QuickGraph.IGraph object to work on</param>
		/// <param name="wf">A function which given the graph and an edge returns the weight associated with that edge</param>
		/// <param name="policyName">The name of the query policy</param>
		/// <param name="degreeThreshold">If the number of transitions for a specific state is greater than this number then the edge list will be kept in memory for faster future querying</param>
		public GeneralWeightedQuerier(IGraph<TVertex, TEdge> targetGraph, WeightFunction<IGraph<TVertex, TEdge>, TVertex, TEdge> wf, KeyValuePair<string, string> policyName, int degreeThreshold)
			: base(targetGraph, policyName) {
			this.wf = wf;			
			this.SyncRoot = new ReaderWriterLockSlim();
			DegreeThreshold = degreeThreshold;
			
			
		}


		protected Dictionary<TVertex, WeightedEdgeMapping> edgeWeightMap = new Dictionary<TVertex, WeightedEdgeMapping>();
		KeyValuePair<TVertex, WeightedEdgeMapping> lastVertex;

		protected class WeightedEdgeMapping {
			TVertex vertex;
			public decimal VertexWeight { get; private set; }
			List<decimal> map = new List<decimal>();
			Dictionary<TEdge, decimal> weightsMap = new Dictionary<TEdge, decimal>();
			GeneralWeightedQuerier<TVertex, TEdge> parent;

			public WeightedEdgeMapping(GeneralWeightedQuerier<TVertex, TEdge> parent, TVertex vertex, IList<TEdge> adjecentEdges, WeightFunction<IGraph<TVertex, TEdge>, TVertex, TEdge> weightFunc) {
				this.vertex = vertex;
				this.parent = parent;

				
				decimal cw = 0.0M;
				decimal nw;
				for (int i = 0; i < adjecentEdges.Count; i++) {
					map.Add(cw);							
					parent.TryGetEdgeWeight(adjecentEdges[i], out nw);
					weightsMap.Add(adjecentEdges[i], nw);
					cw += nw;						
				}
				VertexWeight = cw;

			}

			public decimal GetWeight(TEdge edge) {
				return weightsMap[edge];
			}

			public int GetMapping(decimal weightedIndex) {
				if (weightsMap.Count == 0)
					return -1;

				decimal wi = (decimal)weightedIndex * (decimal)VertexWeight;
				if (weightsMap.Count == 1) {
					return 0;
				} else if (weightsMap.Count >= 2) {
					var i = map.BinarySearch(wi);
					if (i < 0) {
						return (~i) - 1;
					} else {
						return i;
					}
				} else {
					return -1;
				}

			}
		}

	
		#region IWeightedGraphQuerier<IUndirectedGraph<TVertex,TEdge>,TVertex,TEdge,decimal> Members

		public override TEdge WeightedAdjacentEdge(TVertex vertex, decimal weightedIndex) {

			var index = VertexMapping(vertex).GetMapping(weightedIndex);
			if (index >= 0)
				return AdjecentEdge(vertex, index);
			else
				return default(TEdge);
		}

		public bool TryGetEdgeWeight(TEdge edge, out decimal weight) {
			weight = wf(targetGraph, edge);
			return false;
		}

		public override decimal EdgeWeight(TEdge edge) {
			WeightedEdgeMapping wem;
			wem = VertexMapping(edge.Source);
			return wem.GetWeight(edge);
		}
		#endregion

		protected virtual WeightedEdgeMapping VertexMapping(TVertex vertex) {
			WeightedEdgeMapping wem;
			SyncRoot.EnterReadLock();
			try {
				if (edgeWeightMap.TryGetValue(vertex, out wem)) {
					return wem;
				} else if (lastVertex.Key.Equals(vertex) && lastVertex.Value != null) {
					return lastVertex.Value;
				}
			} finally {
				SyncRoot.ExitReadLock();
			}

			SyncRoot.EnterWriteLock();
			try {
				var edgelist = this.AdjecentEdges(vertex).ToList();
				wem = new WeightedEdgeMapping(this, vertex, edgelist, wf);				
				if (!edgeWeightMap.ContainsKey(vertex) && edgelist.Count < DegreeThreshold) {
					edgeWeightMap.Add(vertex, wem);
				} else {
					lastVertex = new KeyValuePair<TVertex, WeightedEdgeMapping>(vertex, wem);
				}
				return wem;
			} finally {
				SyncRoot.ExitWriteLock();
			}
		}

		

		public override decimal VertexWeight(TVertex vertex) {
			return VertexMapping(vertex).VertexWeight;
		}

		#region IDisposable Members

		public override void Dispose() {
			this.edgeWeightMap.Clear();
		}

		#endregion
    }
}
