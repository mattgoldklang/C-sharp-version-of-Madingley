﻿using System;
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
                        cellEnvironment.Add("Original NO3", new double[12]);
                        for (int m = 0; m < 12; m++)
                        {
                            cellEnvironment["Original Temperature"][m] = cellEnvironment["Temperature"][m];
                            cellEnvironment["Original NO3"][m] = cellEnvironment["NO3"][m];
                        }
                    }
                    // If the spin-up period has been completed, then increment cell temperature
                    // according to the number of time-steps that have elapsed since the spin-up ended
                    if (currentTimestep >= burninSteps && currentTimestep <= (burninSteps + impactSteps))
                    {
                        cellEnvironment["Temperature"][currentMonth] = cellEnvironment["Original Temperature"][currentMonth] + ((((currentTimestep - burninSteps) / 12) + 1) *
                            cellEnvironment["dSST"][currentMonth]);
                        cellEnvironment["NO3"][currentMonth] = cellEnvironment["Original NO3"][currentMonth] + ((((currentTimestep - burninSteps) / 12) + 1) *
                            cellEnvironment["dNO3"][currentMonth]);

                        if (cellEnvironment["Realm"][0] == 2)
                        {
                        }
                        else
                        {
                           
                        }
                        // cellEnvironment["Temperature"][currentMonth] += gridCellStocks[actingStock].TotalBiomass *
                        //     (Math.Min(5.0, (((currentTimestep - burninSteps) / 12.0) * humanNPPScenario.Item2)));
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
