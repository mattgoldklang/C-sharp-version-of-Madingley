using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>C:\Users\Matt\Source\Repos\madingley\Madingley\Impacts\OceanAcidification.cs
    /// Adjusts cell climate parameters to simulate the impacts of Ocean Acidification
    /// </summary>
    class OA
    {
        /// <summary>
        /// Constructor for the OA  class
        /// </summary>
        public OA()
        {
        }

        public void ApplyOAScenario(SortedList<string, double[]> cellEnvironment,
            Tuple<string, double, double> OAScenario, uint currentTimestep, uint currentMonth, uint burninSteps, uint NumTimeSteps,
            uint impactSteps, Boolean impactCell)
        {
            if (impactCell && cellEnvironment["Realm"][0] == 2)
            {
                if (OAScenario.Item1 == "no")
                {
                }
                else if (OAScenario.Item1 == "yes")
                {
                    // Check to see whether this time step is at the transition between burn-in and impact
                    if ((currentTimestep >= (burninSteps + 1)) && (currentTimestep <= (burninSteps + 12)))
                    {
                        // Implemenent growth rate change for picoplankton given Dutkiewicz et al 2015. 
                        cellEnvironment["multiplier"][0] = ((currentTimestep- burninSteps)/12 + 1) * 0.00302;
                    }
                    else
                    {
                    }
                }
                else
                {
                    Debug.Fail("There is no method for the OA scenario specified");
                }

            }
        }
    }
}