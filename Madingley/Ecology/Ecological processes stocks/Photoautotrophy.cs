using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// A class for performing photoautotrophy (convert nutrients, light etc. to autotrophic biomass)
    /// </summary>
    class Photoautotrophy
    {

        /// <summary>
        /// Set number of trophic levels to model (i.e. number of size groups)
        /// </summary>
        /// 
        private int _nTrophicLevels;
        public int nTrophicLevels { get { return _nTrophicLevels; } }

        /// <summary>
        /// Setup time step parameters
        /// </summary>
        // Length of the time step. This will be a fraction of the model time unit (i.e. if time unit is month,
        // then 30*1/720 will result in the unicellular NPZ model running at ca. 1 hour time steps)
        private double _dTime;
        public double dTime { get { return _dTime; } }
        // Total number of time steps. 
        private double _nMaxStep;
        public double nMaxStep;

        /// <summary>
        /// Create temporary working arrays
        /// </summary>
        // Array for holding biomass in nitrogen units
        double[] biomassN = new double[] { };
        // Array for holding the change in biomass in nitrogen units per time step
        double[] dBiomassNDt = new double[] { };


        





        

        /// <summary>
        /// 
        /// </summary>
    }
}
