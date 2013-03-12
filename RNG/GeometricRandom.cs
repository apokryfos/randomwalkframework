using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNG
{
   
        public class GeometricallyDistributedRandom : Random
        {   
            private double p;

            public GeometricallyDistributedRandom() : this(0.5) { }
            public GeometricallyDistributedRandom(double p)
            {
                if (p > 1 || p <= 0)
                    throw new ArgumentOutOfRangeException("Probability must be greater than 0.0 and less than (or equal) to 1.0)");
                this.p = p;
            }

			protected override double Sample() {
				int maxiter = (int)((double)100 / p);
				int i = 0;
				double n = base.Sample();
				while (n > p) {
					n = base.Sample();
					if (i >= maxiter) {
						n = p;
						break;
					}
				}
				return n;
			}


            public override int Next()
            {
                return (int)Math.Floor(Math.Log(Sample() / p) / Math.Log(1 - p));
            }


        }
    
}
