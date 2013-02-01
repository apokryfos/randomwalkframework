using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace RandomWalks.RandomWalkInterface {
	/// <summary>
	/// General graph querier interface.
	/// </summary>
	/// <typeparam name="TVertex">Type of vertices (states)</typeparam>
	/// <typeparam name="TEdge">Type of edges (transitions)</typeparam>
	public interface IGraphQuerier<TVertex, TEdge> : IDisposable
		where TEdge : IEdge<TVertex> {
		/// <summary>
		/// Should return total queries performed on the graph
		/// </summary>
		int TotalQueries { get; }

		/// <param name="vertex">The state to be queried</param>
		/// <returns>An enumerable of all available transitions from that state</returns>
		IEnumerable<TEdge> AdjecentEdges(TVertex vertex);
		/// <param name="vertex">The state to be queried</param>
		/// <returns>The number of transitions that could be performed from that state </returns>
		int AdjecentDegree(TVertex vertex);

		/// <summary>
		/// Assuming the transitions are ordered this should return the i-th transition from a state
		/// </summary>
		/// <param name="vertex">The state to be queried</param>
		/// <param name="index">The transition index</param>
		/// <returns>The i-th transition</returns>
		/// <exception cref="IndexOutOfRangeException"/>
		TEdge AdjecentEdge(TVertex vertex, int index);

		/// <summary>
		/// Should have the policy name. The key should be the short abbreviation of the name and the value should be the long name
		/// </summary>
		KeyValuePair<string, string> PolicyName { get; }
	}

	/// <summary>
	/// Weighted querier interface extension.
	/// </summary>
	/// <typeparam name="TVertex">Type of vertices (states)</typeparam>
	/// <typeparam name="TEdge">Type of edges (transitions)</typeparam>
	public interface IWeightedGraphQuerier<TVertex, TEdge> : IGraphQuerier<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		/// <summary>
		/// Assuming the transitions are ordered (irrespective of their weight)
		/// General indexing is of the form: 
		/// The first transition is always indexed between 0.0 (inclusive) and w(e1)/w(u) (exclusive)
		/// where w(e1) is the transition weight of the first transition (e1) and w(u) is the vertex weight
		/// the subsequent transitions are indexed from w(ei-1)/w(u) (inclusive) to w(ei)/w(u) (exclusive)
		/// This is done in order to be used in conjuction with Random.NextDouble()
		/// </summary>
		/// <param name="vertex">The state to be queried</param>
		/// <param name="weightedIndex">Weighted transition index. Should range from 0-1</param>
		/// <returns>The transition which corresponds to the weighted index specified</returns>
		/// <example> 
		///		Assume a state with 3 transitions: 
		///		Transition 1 has a weight 10.0
		///		Transition 2 has a weight 5.0
		///		Transition 3 has a weight 1.0
		///		The weight of the state is 16.0
		///		The weighted index which corresponds to transition 1 ranges from 0.0(inclusive) to 10.0/16.0 (exclusive)
		///		Respectively the weighted index which corresponds to transition 2 ranges from 10.0/16.0 to (15.0/16.0)
		///		etc.		
		/// </example>
		TEdge WeightedAdjacentEdge(TVertex vertex, decimal weightedIndex);

		/// <summary>
		/// Gets the weight if the specified transition
		/// </summary>
		/// <param name="edge">The transition to be queried</param>
		/// <returns>The weight of the transition</returns>
		decimal EdgeWeight(TEdge edge);

		/// <summary>
		/// Gets the weight if the specified state.
		/// Typically the sum of all adjecent transition weights
		/// </summary>
		/// <param name="edge">The state to be queried</param>
		/// <returns>The weight of the state</returns>
		decimal VertexWeight(TVertex vertex);
	}
}
