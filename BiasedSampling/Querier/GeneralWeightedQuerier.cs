using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using RandomWalks.RandomWalkInterface;
using System.Threading;

namespace RandomWalks.Querier
{
    public class GeneralWeightedQuerier<TVertex, TEdge> : UnweightedGraphQuerier<TVertex, TEdge>, IWeightedGraphQuerier<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {

      
		protected WeightFunction<IGraph<TVertex,TEdge>, TVertex, TEdge> wf;
		protected ReaderWriterLockSlim SyncRoot { get; private set; }

		protected int DegreeThreshold { get; set; }

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

			public WeightedEdgeMapping(GeneralWeightedQuerier<TVertex, TEdge> parent, TVertex vertex, IList<TEdge> adjecentEdges, IGraph<TVertex, TEdge> targetGraph, WeightFunction<IGraph<TVertex, TEdge>, TVertex, TEdge> weightFunc) {
				this.vertex = vertex;
				this.parent = parent;

				if (adjecentEdges.Count > 0) {
					decimal cw = 0.0M;
					parent.TryGetEdgeWeight(adjecentEdges[0], out cw);

					weightsMap.Add(adjecentEdges[0], cw);
					for (int i = 1; i < adjecentEdges.Count; i++) {
						map.Add(cw);
						decimal nw;
						parent.TryGetEdgeWeight(adjecentEdges[i], out nw);
						weightsMap.Add(adjecentEdges[i], nw);
						cw += nw;
					}
					VertexWeight = cw;

				}
			}

			public decimal GetWeight(TEdge edge) {
				return weightsMap[edge];
			}

			public int GetMapping(decimal weightedIndex) {
				if (weightsMap.Count == 0)
					return -1;

				decimal wi = (decimal)weightedIndex * (decimal)VertexWeight;
				if (weightsMap.Count == 1 || wi < map[0])
					return 0;
				else if (weightsMap.Count > 2) {
					var i = map.BinarySearch(wi);
					if (i < 0)
						return ~i;
					return i;
				} else
					return 1;

			}
		}

	
		#region IWeightedGraphQuerier<IUndirectedGraph<TVertex,TEdge>,TVertex,TEdge,decimal> Members

		public virtual TEdge WeightedAdjacentEdge(TVertex vertex, decimal weightedIndex) {

			var index = VertexMapping(vertex).GetMapping(weightedIndex);
			if (index >= 0)
				return AdjecentEdge(vertex, index);
			else
				return default(TEdge);


		}

		public virtual bool TryGetEdgeWeight(TEdge edge, out decimal weight) {
			weight = wf(targetGraph, edge);
			return false;
		}

		public virtual decimal EdgeWeight(TEdge edge) {
			WeightedEdgeMapping wem;
			wem = VertexMapping(edge.Source);
			return wem.GetWeight(edge);
		}

		#endregion

		protected virtual WeightedEdgeMapping VertexMapping(TVertex vertex) {
			WeightedEdgeMapping wem;
			SyncRoot.EnterUpgradeableReadLock();
			try {

				if (!edgeWeightMap.TryGetValue(vertex, out wem)) {
					if (!lastVertex.Key.Equals(vertex)) {
						var edgelist = this.AdjecentEdges(vertex).ToList();
						wem = new WeightedEdgeMapping(this, vertex, edgelist, targetGraph, wf);
						SyncRoot.EnterWriteLock();
						if (!edgeWeightMap.ContainsKey(vertex) && edgelist.Count > DegreeThreshold && DegreeThreshold > 0) {
							edgeWeightMap.Add(vertex, wem);
						} else {
							lastVertex = new KeyValuePair<TVertex, WeightedEdgeMapping>(vertex, wem);
						}
						SyncRoot.ExitWriteLock();
					} else {
						wem = lastVertex.Value;
					}					
				}
				return wem;

			} finally {
				SyncRoot.ExitUpgradeableReadLock();
			}
		}

		

		public virtual decimal VertexWeight(TVertex vertex) {
			return VertexMapping(vertex).VertexWeight;
		}

		#region IDisposable Members

		public override void Dispose() {
			this.edgeWeightMap.Clear();
		}

		#endregion
    }
}
