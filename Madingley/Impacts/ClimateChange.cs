using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Adjusts cell climate parameters to simulate the impacts of climate change
    /// </summary>
    class ClimateChange
    {
        /// <summary>
        /// Constructor for the climate change class
        /// </summary>
        public ClimateChange()
        {
        }
        public double GetNPP(double x, double y)
        {
            double[] phyto = new double[3];
            phyto[0] = (1.145 - 0.021 * x - 6.936E-6 * y);
            phyto[1] = (1.146 + 0.013 * x - 0.064 * y);
            phyto[2] = (0.804 - 0.002 * x - 0.077 * y);
            double total = phyto.Sum();

            return total;
        }
        public void ApplyTemperatureScenario(SortedList<string, double[]> cellEnvironment,
            Tuple<string, double, double> temperatureScenario, uint currentTimestep, uint currentMonth, uint burninSteps,
            uint impactSteps, Boolean impactCell)
        {
            if (impactCell)
            {
                if (temperatureScenario.Item1 == "no")
                {
                }
                else if (temperatureScenario.Item1 == "constant")
                {
                    // Check to see whether this time step is at the transition between burn-in and impact
                    if ((currentTimestep >= (burninSteps + 1)) && (currentTimestep <= (burninSteps + 12)))
                    {
                        // Increment temperature
                        cellEnvironment["Temperature"][currentMonth] += temperatureScenario.Item2;
                    }
                    else
                    {
                    }
                }
                else if (temperatureScenario.Item1 == "temporary")
                {
                    // Check to see whether this time step is at the transition between burn-in and impact
                    // or at the transition between impact and post-impat
                    if ((currentTimestep >= (burninSteps + 1)) && (currentTimestep <= (burninSteps + 12)))
                    {
                        // Increment temperature
                        cellEnvironment["Temperature"][currentMonth] += temperatureScenario.Item2;
                    }
                    else if ((currentTimestep >= (burninSteps + impactSteps + 1)) && (currentTimestep <= (burninSteps + impactSteps + 12)))
                    {
                        // Revert temperature to original value
                        cellEnvironment["Temperature"][currentMonth] -= temperatureScenario.Item2;
                    }
                    else
                    {
                    }
                }
                else if (temperatureScenario.Item1 == "escalating")
                {
                    // If this is the first time step, add items to the cell environment to store the original cell temperature
                    if (currentTimestep == 0)
                    {
                        cellEnvironment.Add("Original Temperature", new double[12]);
                        cellEnvironment.Add("Original NPP", new double[12]);


                        for (int m = 0; m < 12; m++)
                        {
                            cellEnvironment["Original Temperature"][m] = cellEnvironment["Temperature"][m];
                            cellEnvironment["Original NPP"][m] = cellEnvironment["NPP"][m];
                        }
                    }
                    // If the spin-up period has been completed, then increment cell temperature
                    // according to the number of time-steps that have elapsed since the spin-up ended
                    if (currentTimestep >= burninSteps & currentTimestep <= burninSteps + impactSteps )
                    {
                        double tstep = 1 + (currentTimestep - burninSteps) / 12;
                        cellEnvironment["Temperature"][currentMonth] = cellEnvironment["Original Temperature"][currentMonth] + (temperatureScenario.Item2/(impactSteps / 12)) * Math.Floor(tstep);
                        double pnpp = (GetNPP(cellEnvironment["Temperature"][currentMonth], cellEnvironment["no3"][currentMonth]) - GetNPP(cellEnvironment["Original Temperature"][currentMonth], cellEnvironment["no3"][currentMonth])) / GetNPP(cellEnvironment["Temperature"][currentMonth], cellEnvironment["no3"][currentMonth]);
                        cellEnvironment["NPP"][currentMonth] = cellEnvironment["Original NPP"][currentMonth]*(1 + pnpp);
                        // cellEnvironment["Temperature"][currentMonth] += gridCellStocks[actingStock].TotalBiomass *
                        //     (Math.Min(5.0, (((currentTimestep - burninSteps) / 12.0) * humanNPPScenario.Item2)));
                    }
                }
                else if (temperatureScenario.Item1 == "rcp")
                {
                    if (currentTimestep >= burninSteps)
                    {
                        double no3i = cellEnvironment["no3"][currentMonth];
                        double ssti = cellEnvironment["Temperature"][currentMonth];
                        cellEnvironment["Temperature"][currentMonth] = cellEnvironment["rcpSST"][burninSteps - currentTimestep];
                        cellEnvironment["no3"][currentMonth] = cellEnvironment["rcpNO3"][burninSteps - currentTimestep];
                        double no3 = cellEnvironment["no3"][currentMonth];
                        double sst = cellEnvironment["Temperature"][currentMonth];
                        double pNPP = (GetNPP(no3, sst) - GetNPP(no3i, ssti)) / GetNPP(no3i, ssti);
                        cellEnvironment["NPP"][currentMonth] *= (1+pNPP);
                      }
                }
                else if (temperatureScenario.Item1 == "pb")
                {
                    // If the spin-up period (100 years) has been completed, then increment cell temperature
                    // according to the number of time-steps that have elapsed since the spin-up ended, for a period of 100 years until the 
                    // maximum thermal increase is reached
                    if ((currentTimestep > (burninSteps)) && (currentTimestep < burninSteps + impactSteps))
                    {
                          cellEnvironment["Temperature"][currentMonth] += (temperatureScenario.Item2 / 1200) * (currentTimestep - 1199);
                    }
                }
                else
                {
                    Debug.Fail("There is no method for the climate change (temperature) scenario specified");
     
                }
            }        
        }
    }
}