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
                    if (currentTimestep >= 1 && currentTimestep <= (burninSteps + impactSteps))
                    {
                        // Implemenent growth rate change for picoplankton given Dutkiewicz et al 2015. 
                        cellEnvironment["multiplier"][0] = ((currentTimestep - burninSteps) / 12 + 1) * 0.00302;

                    }
                    else if ((currentTimestep - burninSteps) >= impactSteps)
                    {
                        cellEnvironment["multiplier"][0] = .302;
                    }
                }
                else if (OAScenario.Item1 == "microNPP" & OAScenario.Item1 == "nanoNPP" & OAScenario.Item1 == "picoNPP")
                {

                    if (currentTimestep == 0)
                    {
                        List<string> npps = new List<string>();
                        npps.Add("microNPP");
                        npps.Add("nanoNPP");
                        npps.Add("picoNPP");
                        npps.Remove(OAScenario.Item1);
                        for (int m = 0; m < 12; m++)
                        {
                            cellEnvironment[OAScenario.Item1][m] += -cellEnvironment[OAScenario.Item1][m] * OAScenario.Item2;
                            cellEnvironment[npps[0]][m] += 0.5 * OAScenario.Item2 * cellEnvironment[OAScenario.Item1][m];
                            cellEnvironment[npps[1]][m] += 0.5 *OAScenario.Item2 * cellEnvironment[OAScenario.Item1][m];
                        }

                    }

                }
                else if (OAScenario.Item1 == "totalNPP")
                {
                    if (currentTimestep == 0)
                    {
                        List<string> npps = new List<string>();
                        npps.Add("microNPP");
                        npps.Add("nanoNPP");
                        npps.Add("picoNPP");
                        for (int m = 0; m < 12; m++)
                        {
                            cellEnvironment[npps[2]][m] *= OAScenario.Item2;
                            cellEnvironment[npps[0]][m] *= OAScenario.Item2;
                            cellEnvironment[npps[1]][m] *= OAScenario.Item2;
                        }
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