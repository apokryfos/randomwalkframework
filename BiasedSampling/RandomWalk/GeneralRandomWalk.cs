using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using System.IO;
using System.Diagnostics;
using RandomWalks.RandomWalkInterface;

namespace RandomWalks {


	/// <summary>
	/// General random walk class. Used as a base for specific random walks
	/// </summary>
	/// <typeparam name="TVertex">Type of vertices (states) encountered</typeparam>
	/// <typeparam name="TEdge">Type of edges(transitions) encountered</typeparam>

	public abstract class GeneralRandomWalk<TVertex, TEdge> : IWeightedRandomWalk<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		public virtual decimal TimeIncrement { get { return 1.0M; } }
		protected Random r = RNG.RNGProvider.r;
		

		private IGraphQuerier<TVertex, TEdge> Querier { get; set; }

		/// <summary>
		/// Constructs a general random walk
		/// </summary>
		/// <param name="entryPoint">Initial state</param>
		/// <param name="gq">Querier on the graph</param>
		/// <param name="name">The name of the walk. Key is the short name and value the long name</param>
		public GeneralRandomWalk(TVertex entryPoint, IGraphQuerier<TVertex, TEdge> gq, KeyValuePair<string, string> name) {
			Querier = gq;
			this.CurrentState = entryPoint;
			this.Name = name;

		}

		#region IWeightedRandomWalk<TVertex,TEdge, decimal> Members

		public void Terminate() {

			if (Terminated != null)
				Terminated(this, EventArgs.Empty);

		}

		/// <summary>
		/// Event should be raised when the walk terminates
		/// </summary>
		public event EventHandler Terminated;

		public KeyValuePair<string, string> Name { get; protected set; }

		/// <summary>
		/// Event should be raised at each transition (step) the walk takes
		/// </summary>
		public event TransitionEvent<TVertex, TEdge> Step;


		/// <summary>
		/// Additional initialization instructions go here
		/// </summary>
		public virtual void Initialize() {			
			TotalSteps = 0;
			DiscreetSteps = 0;
		}


		/// <summary>
		/// Chose the next state based on the current state. 
		/// </summary>
		/// <param name="current">The current state of the walk. Provided to allow flexibility.</param>
		/// <returns>Should return the next transition or default(TEdge) to wait</returns>
		protected abstract TEdge ChooseNext(TVertex current);


		/// <summary>
		/// Simulates one step of the walk
		/// </summary>
		/// <returns>Returns the vertex that was reached after one step was simulated</returns>
		public virtual TVertex NextSample() {
			var next = ChooseNext(CurrentState);
			TotalSteps += TimeIncrement;
			DiscreetSteps++;



			if (!object.ReferenceEquals(next, default(TEdge)))
				CurrentState = next.GetOtherVertex(CurrentState);

			if (Step != null) {
				//If the transition is null then the walk waits
				if (!object.ReferenceEquals(next, default(TEdge)))
					Step(this, CurrentState, next.GetOtherVertex(CurrentState), next, GetTransitionWeight(next));
				else
					Step(this, CurrentState, CurrentState, next, GetTransitionWeight(next));
			}
			
			return CurrentState;
		}

		/// <summary>
		/// Holds the steps that the walk took
		/// </summary>
		public virtual ulong DiscreetSteps {
			private set;
			get;
		}

		/// <summary>
		/// Holds the time that the walk has been runing for a continous time walk.
		/// This is the same as @property(DiscreetSteps) for a discreet time walk.
		/// </summary>
		public virtual decimal TotalSteps {
			get;
			private set;
		}

		public TVertex CurrentState { get; private set; }

		/// <summary>
		/// Gets the number of transitions that might be performed from a specific state
		/// </summary>
		public virtual int GetAdjacentTransitionCount(TVertex state) {
			return Querier.AdjecentDegree(state);
		}

		/// <summary>
		/// Gets an adjacent transtion
		/// </summary>
		/// <param name="state">The state from which to get the adjacent transition</param>
		/// <param name="index">The index of the adjacent transition</param>
		/// <returns>The adjacent transition</returns>
		/// <exception cref="IndexOutOfRangeException">The index specified is out of range</exception>
		public virtual TEdge GetAdjacentTransition(TVertex state, int index) {
			return Querier.AdjecentEdge(state, index);
		}

		public abstract decimal GetStateWeight(TVertex state);

		public abstract decimal GetTransitionWeight(TEdge transition);

		#endregion
	}
}
