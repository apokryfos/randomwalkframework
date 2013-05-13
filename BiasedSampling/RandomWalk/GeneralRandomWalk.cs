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

	public class RandomWalk<TVertex, TEdge> : IRandomWalk<TVertex, TEdge>
		where TEdge : IEdge<TVertex> {

		/// <summary>
		/// Gets the value which represents the "time" it takes to make a single step. 
		/// Defaults to 1 if the walk is discreet.
		/// </summary>
		public virtual decimal TimeIncrement { get { return 1.0M; } }
		
		protected Random r = RNG.RNGProvider.r;
		

		private IGraphQuerier<TVertex, TEdge> Querier { get; set; }

		/// <summary>
		/// Constructs a general random walk
		/// </summary>
		/// <param name="entryPoint">Initial state</param>
		/// <param name="gq">Querier on the graph</param>
		/// <param name="name">The name of the walk. Key is the short name and value the long name</param>
		public RandomWalk(TVertex entryPoint, IGraphQuerier<TVertex, TEdge> gq, KeyValuePair<string, string> name) {
			Querier = gq;
			this.CurrentState = entryPoint;
			this.InitialState = entryPoint;
			this.PreviousState = default(TVertex);
			this.Name = name;

		}

		/// <summary>
		/// Constructs a general random walk
		/// </summary>
		/// <param name="entryPoint">Initial state</param>
		/// <param name="gq">Querier on the graph</param>
		/// <param name="name">The name of the walk. Key is the short name and value the long name</param>
		public RandomWalk(TVertex entryPoint, IGraphQuerier<TVertex, TEdge> gq) :
			this(entryPoint,gq,gq.PolicyName) {
			
		}

		#region IRandomWalk<TVertex,TEdge, decimal> Members

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
			this.CurrentState = this.InitialState;
			TotalSteps = 0;
			DiscreetSteps = 0;
		}


		/// <summary>
		/// Chose the next state based on the current state. 
		/// </summary>
		/// <param name="current">The current state of the walk. Provided to allow flexibility.</param>
		/// <returns>Should return the next transition or default(TEdge) to wait</returns>
		protected virtual TEdge ChooseNext(TVertex current) {			
			if (Querier.AdjecentDegree(current) > 0) {
				return Querier.WeightedAdjacentEdge(current, (decimal)r.NextDouble());
			} else {
				return default(TEdge);
			}
		}

		/// <summary>
		/// Simulates a transition of the walk
		/// </summary>
		/// <param name="From">Transition From</param>
		/// <param name="To">Transition To</param>
		/// <param name="TransitionRef">Transition along edge</param>
		protected virtual void Transition(TVertex From, TVertex To, TEdge TransitionRef) {
			TotalSteps += TimeIncrement;
			DiscreetSteps++;
			PreviousState = From;
			CurrentState = To;			
			OnStep(PreviousState, CurrentState, TransitionRef, GetTransitionWeight(TransitionRef));
		}

		/// <summary>
		/// Simulates one step of the walk
		/// </summary>
		/// <returns>Returns the vertex that was reached after one step was simulated</returns>
		public virtual TVertex NextSample() {
			var next = ChooseNext(CurrentState);
			Transition(CurrentState, (!object.Equals(next, default(TEdge))?next.GetOtherVertex(CurrentState):CurrentState), next);
			return CurrentState;
		}
		/// <summary>
		/// Method to fire the step event
		/// </summary>
		/// <param name="CurrentState">The state that the walk was coming from</param>
		/// <param name="NextState">The state the walk is moving to</param>
		/// <param name="Transition">The edge the walk has taken</param>
		/// <param name="TransitionWeight">The weight of the transition taken</param>
		protected void OnStep(TVertex CurrentState, TVertex NextState, TEdge Transition, decimal TransitionWeight) {
			if (Step != null) {
				Step(this, CurrentState, NextState, Transition, TransitionWeight);
			}
		}
		/// <summary>
		/// Holds the steps that the walk took
		/// </summary>
		public virtual ulong DiscreetSteps {
			protected set;
			get;
		}

		/// <summary>
		/// Holds the time that the walk has been runing for a continous time walk.
		/// This is the same as @property(DiscreetSteps) for a discreet time walk.
		/// </summary>
		public virtual decimal TotalSteps {
			get;
			protected set;
		}

		public TVertex CurrentState { get; protected set; }
		public TVertex PreviousState { get; protected set; }
		public TVertex InitialState { get; private set; }


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

		public virtual decimal GetStateWeight(TVertex state) {
			return Querier.VertexWeight(state);
		}

		public virtual decimal GetTransitionWeight(TEdge transition) {
			return Querier.EdgeWeight(transition);
		}

		#endregion
	}
}
