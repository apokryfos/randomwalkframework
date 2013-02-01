using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QuickGraph;
using RandomWalks.RandomWalkInterface;

namespace RandomWalkAnalysis.Samplers
{
    public class SamplingThread<TVertex, TEdge> : Sampler<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
        
    {
        private ManualResetEvent mre;
        private object syncRoot = new object();

        public override IWeightedRandomWalk<TVertex, TEdge> RandomWalk
        {
            get
            {
                return base.RandomWalk;
            }
            protected set
            {
                lock (syncRoot)
                {
                    base.RandomWalk = value;
                }
            }
        }

        public SamplingThread(IWeightedRandomWalk<TVertex, TEdge> rw, ITerminationConditions<TVertex, TEdge> c)
            : base(rw, c)
        {
            this.mre = new ManualResetEvent(false);
            
        }

        protected override void  RandomWalk_Terminated(object sender, EventArgs e)
        {
            lock (syncRoot)
            {
                base.RandomWalk_Terminated(sender, e);
            }
            mre.Set();
        }

        public ManualResetEvent ResetFlag
        {
            get { return mre; }
        }

        protected override TVertex SampleOne()
        {
            lock (syncRoot)
            {
                return base.SampleOne();
            }
        }


       
    }
}
