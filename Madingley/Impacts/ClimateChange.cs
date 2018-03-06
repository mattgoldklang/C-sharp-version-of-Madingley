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
                            (1000 * cellEnvironment["dNO3"][currentMonth]));
                        if (cellEnvironment["NO3"][currentMonth] <= 0)
                        {
                            cellEnvironment["NO3"][currentMonth] = 0;
                        }



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
                else if (temperatureScenario.Item1 == "escalating2")
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
                            cellEnvironment["Temperature"][m] = 10;
                        }
                    }
                    // If the spin-up period has been completed, then increment cell temperature
                    // according to the number of time-steps that have elapsed since the spin-up ended
                    if (currentTimestep >= burninSteps && currentTimestep <= (burninSteps + impactSteps))
                    {
                        if (currentMonth == 0)
                            for (int m = 0; m < 12; m++)
                            {
                                cellEnvironment["Temperature"][m] = +temperatureScenario.Item2;
                            }

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
                else if (temperatureScenario.Item1 == "bifurcation")
                {
                    if (currentTimestep == 0)
                    {
                        cellEnvironment.Add("Original Temperature", new double[12]);
                        cellEnvironment.Add("Increment Tstep", new double[1]);
                        cellEnvironment.Add("Original NO3", new double[12]);

                        for (int m = 0; m < 12; m++)
                        {
                            cellEnvironment["Original Temperature"][m] = cellEnvironment["Temperature"][m];
                            cellEnvironment["Original NO3"][m] = cellEnvironment["NO3"][m];
                            cellEnvironment["Temperature"][m] = 10;
                        }
                        cellEnvironment["Increment Tstep"][0] = burninSteps;
                    }

                    if (currentTimestep > burninSteps)
                    {
                        if (currentTimestep < (burninSteps + impactSteps))
                        {
                            if (currentTimestep - cellEnvironment["Increment Tstep"][0] == 600)
                            {
                                for (int m = 0; m < 12; m++)
                                {
                                    cellEnvironment["Temperature"][currentMonth + m] += temperatureScenario.Item2;
                                }


                                cellEnvironment["Increment Tstep"][0] = currentTimestep;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            if (currentTimestep - cellEnvironment["Increment Tstep"][0] == 600)
                            {

                                for (int m = 0; m < 12; m++)
                                {
                                    cellEnvironment["Temperature"][currentMonth + m] -= temperatureScenario.Item2;
                                }
                                cellEnvironment["Increment Tstep"][0] = currentTimestep;
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else if (temperatureScenario.Item1 == "seasonality")
                {
                    if (currentTimestep == 0)
                    {
                        if (temperatureScenario.Item3 == 2)
                        {
                            for (int m=0; m < 12; m++)
                            {
                                if (m < 3 & m > 9)
                                {
                                    cellEnvironment["Temperature"][m] = 7;                             
                                }
                                else
                                {
                                    cellEnvironment["Temperature"][m] = 13;
                                }
                            }
                        }
                        else if(temperatureScenario.Item3 == 3)
                        {
                            for (int m=0; m < 12; m++)
                            {
                                if (m < 2 & m > 10)
                                {
                                    cellEnvironment["Temperature"][m] = 7;
                                }
                                else if(m >= 2 && m < 6)
                                {
                                    cellEnvironment["Temperature"][m] = 10;
                                }
                                else { cellEnvironment["Temperature"][m] = 13; }
                            }
                        }
                        else
                        {
                            for (int m = 0; m < 12; m++)
                            {
                                if (m <= 1 & m == 11)
                                {
                                    cellEnvironment["Temperature"][m] = 7;
                                }
                                else if (m >= 1 && m < 4)
                                {
                                    cellEnvironment["Temperature"][m] = 9;
                                }
                                else if(m>=4 && m<7)
                                {
                                    cellEnvironment["Temperature"][m] = 11;
                                }
                                else { cellEnvironment["Temperature"][m] = 13; }
                            }
                        }
                    }
                    else if (currentTimestep > burninSteps && currentTimestep <= burninSteps+impactSteps)
                    {
                        if(currentMonth == 0)
                        {
                            for(int m = 0; m < 12; m++)
                            {
                                cellEnvironment["Temperature"][m] += temperatureScenario.Item2;
                            }
                        }
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
