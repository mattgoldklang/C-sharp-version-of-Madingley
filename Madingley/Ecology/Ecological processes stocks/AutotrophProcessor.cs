using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Class for converting primary productivity estimates to autotroph biomass
    /// </summary>
    public class AutotrophProcessor
    {
        /// <summary>
        /// Instance of the class to perform general functions
        /// </summary>
        private UtilityFunctions Utilities;

        // Conversion ratio for phytoplankton from grams carbon to grams wet weight
        /// <summary>
        /// Factor to convert phytoplankton biomass from grams carbon to grams wet weight
        /// </summary>
        /// <remarks>Currently derived from Ho et al. (2003) J. Phycol., Dalsgaard and Pauly (1997) and Strickland (1966)</remarks>
        private double _PhytoplanktonConversionRatio;
        /// <summary>
        /// Get the conversion ratio for phytoplankton from grams carbon to grams wet weight
        /// </summary>
        public double PhytoplanktonConversionRatio { get { return _PhytoplanktonConversionRatio; } }

        /// <summary>
        /// Factor to convert NPP from units per m^2 to units per km^2
        /// </summary>
        private const double _MsqToKmSqConversion = 1000000.0;
        /// <summary>
        /// Get the factor to convert NPP from units per m^2 to units per km^2
        /// </summary>
        public double MsqToKmSqConversion { get { return _MsqToKmSqConversion; } }

        /// <summary>
        /// Constructor for the autotroph processor: initialises necessary classes
        /// </summary>
        public AutotrophProcessor()
        {

            _PhytoplanktonConversionRatio = EcologicalParameters.Parameters["AutotrophProcessor.ConvertNPPtoAutotroph.PhytoplanktonConversionRatio"];

            // Initialise the utility functions
            Utilities = new UtilityFunctions();
        }

        /// <summary>
        /// Convert NPP estimate into biomass of an autotroph stock
        /// </summary>
        /// <param name="cellEnvironment">The environment of the current grid cell</param>
        /// <param name="gridCellStockHandler">The stock handler for the current stock</param>
        /// <param name="actingStock">The location of the stock to add biomass to</param>
        /// <param name="terrestrialNPPUnits">The units of the terrestrial NPP data</param>
        /// <param name="oceanicNPPUnits">The units of the oceanic NPP data</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="GlobalModelTimeStepUnit">The time step unit used in the model</param>
        /// <param name="trackProcesses">Whether to output data describing the ecological processes</param>
        /// <param name="globalTracker">Whether to output data describing the global-scale environment</param>
        /// <param name="outputDetail">The level of output detail to use for the outputs</param>
        /// <param name="specificLocations">Whether the model is being run for specific locations</param>
        /// <param name="currentMonth">The current month in the model run</param>
        public void ConvertNPPToAutotroph(FunctionalGroupDefinitions cohortDefinitions, FunctionalGroupDefinitions stockDefinitions,
            SortedList<string, double[]> cellEnvironment, GridCellStockHandler gridCellStockHandler, int[]
            actingStock, string terrestrialNPPUnits, string oceanicNPPUnits, Tuple<string, double, double> temperatureScenario, uint burninSteps, uint currentTimestep, string GlobalModelTimeStepUnit,
            ProcessTracker trackProcesses, FunctionalGroupTracker functionalTracker, GlobalProcessTracker globalTracker, string outputDetail, bool specificLocations, uint currentMonth)
        {
            double NPP = new double();
            double mortality = new double();
            double plarge = new double();
            double dplarge = new double();
            double new_n_t = new double();
            double sinking = new double();
            double dpico = new double();
            double total_npp = new double();
            double pchange = new double();
            double NPPi = new double();
            double micro_to_pico = new double();
            double SST = new double();
            double newT = new double();
            double dSST = new double();
            double[] bloom_values_micro = new double[12];
            double[] bloom_values_nano = new double[12];
            double left_overs = new double();
            double sinking_rate = new double();
            double dn = new double();
            int[] bloom_start = new int[2];
            int[] bloom_end = new int[2];
            double multiplier = new double();
            double exponent = new double();
            double new_npp = new double();


            // Check that this is an ocean cell
            if (cellEnvironment["Realm"][0] == 2.0)
            {
                micro_to_pico = cellEnvironment["microNPP"][currentMonth] / cellEnvironment["picoNPP"][currentMonth];
                micro_to_pico = cellEnvironment["micro_to_pico"][currentMonth];
                total_npp = cellEnvironment["microNPP"][currentMonth] + cellEnvironment["nanoNPP"][currentMonth] + cellEnvironment["picoNPP"][currentMonth];

                //Calculates the bloom phenology of Micro production using the Hopkins et al 2015 method
                if (currentTimestep == 0)
                {
                    for (int m = 0; m < 12; m++)
                    {
                        bloom_values_micro[m] = cellEnvironment["microNPP"][m];
                        bloom_values_nano[m] = cellEnvironment["nanoNPP"][m];
                        cellEnvironment["Original microNPP"][m] = cellEnvironment["microNPP"][m];
                        cellEnvironment["Original nanoNPP"][m] = cellEnvironment["nanoNPP"][m];
                        cellEnvironment["Original picoNPP"][m] = cellEnvironment["picoNPP"][m];
                    }
                    
                    double bloom_max = bloom_values_micro.Max();
                    int peak_month = bloom_values_micro.ToList().IndexOf(bloom_max);
                    List<double> prepeak = new List<double>();
                    List<double> postpeak = new List<double>();
                    for (int m = 0; m < 12; m++)
                    {
                        if (m < peak_month) prepeak.Add(bloom_values_micro[m]);
                        else if (m > peak_month) postpeak.Add(bloom_values_micro[m]);
                    }
                    double pre_min = prepeak.Min();
                    double post_min = postpeak.Min();
                    double pre_range = (0.05 * (bloom_max - pre_min)) + pre_min;
                    double post_range = (0.05 * (bloom_max - post_min)) + post_min;
                    bloom_start[0] = prepeak.FindIndex(item => item > pre_range);
                    if (bloom_start[0] < 0 || Convert.IsDBNull(bloom_start[0]))
                    {
                         bloom_start[0] = prepeak.Count();
                    }
                    bloom_end[0] = postpeak.FindIndex(item => item < post_range) + prepeak.Count() + 1;
                    if (bloom_end[0] < 0 || Convert.IsDBNull(bloom_end[0]))
                    {
                         bloom_end[0] = prepeak.Count() + 2;
                    }
                    cellEnvironment["Bloom Start"][0] = bloom_start[0];
                    cellEnvironment["Bloom End"][0] = bloom_end[0];

                    double bloom_max_n = bloom_values_nano.Max();
                    int peak_month_n = bloom_values_nano.ToList().IndexOf(bloom_max_n);
                    List<double> prepeak_n = new List<double>();
                    List<double> postpeak_n = new List<double>();
                    for (int m = 0; m < 12; m++)
                    {
                        if (m < peak_month_n) prepeak_n.Add(bloom_values_nano[m]);
                        else if (m > peak_month_n) postpeak_n.Add(bloom_values_nano[m]);
                    }
                    double pre_min_n = prepeak_n.Min();
                    double post_min_n = postpeak_n.Min();
                    double pre_range_n = (0.05 * (bloom_max_n - pre_min_n)) + pre_min_n;
                    double post_range_n = (0.05 * (bloom_max_n - post_min_n)) + post_min_n;
                    bloom_start[1] = prepeak.FindIndex(item => item > pre_range_n);
                    if (bloom_start[1] < 0 || Convert.IsDBNull(bloom_start[1]) )
                    {
                        bloom_start[1] = prepeak_n.Count();
                    }
                    bloom_end[1] = postpeak.FindIndex(item => item < post_range_n) + prepeak.Count() + 1;
                    if (bloom_end[1] < 0 || Convert.IsDBNull(bloom_end[1]))
                    {
                        bloom_end[1] = prepeak_n.Count() + 2;
                    }
                    cellEnvironment["Bloom Start"][1] = bloom_start[1];
                    cellEnvironment["Bloom End"][1] = bloom_end[1];
                    Console.WriteLine(bloom_start[1]);
                    Console.WriteLine(bloom_end[1]);
                }

                switch (gridCellStockHandler[actingStock].StockName)
                {
                    case "picophytoplankton":

                        left_overs = cellEnvironment["Remaining Biomass"][0];
                        // Get picophytoplankton NPP from cell environment
                        NPP = cellEnvironment["picoNPP"][currentMonth];
                        NPPi = cellEnvironment["picoNPP"][currentMonth];
                         // If picophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0])
                        {
                            NPP = 0.0;
                        }
                        NPP = cellEnvironment["picoNPP"][currentMonth];
                        multiplier = cellEnvironment["multiplier"][0];
                        dpico = (NPP * multiplier);
                        if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                        {
                            newT = cellEnvironment["Temperature"][currentMonth];
                            SST = cellEnvironment["Original Temperature"][currentMonth];
                            dSST = (newT - SST);
                            exponent = 0.11 * newT - 0.47;
                            new_n_t = .25328 * Math.Pow(10, exponent) * dSST;
                            new_npp = ((NPPi / total_npp) + new_n_t) * total_npp;
                            dpico += new_npp;
                         }
                        NPP = NPP + dpico + cellEnvironment["DMicro"][currentMonth] + cellEnvironment["DNano"][currentMonth];
                        cellEnvironment["DMicro"][currentMonth] += dpico*0.5;
                        cellEnvironment["DNano"][currentMonth] += dpico*0.5;
                      
                        break;

                    case "nanophytoplankton":

                        // Get the left over  NPP value for this month;
                        left_overs = cellEnvironment["Remaining Biomass"][1];
                        // Get nanophytoplankton NPP from cell environment
                        NPP = cellEnvironment["nanoNPP"][currentMonth];
                        // If nanophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;
                       // if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                       // {
                         //   newT = cellEnvironment["Temperature"][currentMonth];
                           // SST = cellEnvironment["Original Temperature"][currentMonth];
                            //dSST = newT - SST;
                           // if (newT < 12) pchange = -0.1;
                           // else
                          //  {
                            //    plarge = NPP / total_npp;
                              //  dplarge = plarge + -0.05 * dSST;
                              //  new_n_t = total_npp * dplarge;
                              //  pchange = (NPP - new_n_t) / NPP;
                               // if (cellEnvironment["Bloom Start"][1] <= currentMonth && currentMonth <= cellEnvironment["Bloom End"][1])
                               // {
                              //      pchange = 1.5 * pchange;
                              //  }
                              //  else pchange = 0.25 * pchange;
                           // }
                       // }
                        else pchange = 0;
                        cellEnvironment["DNano"][currentMonth] += NPP * pchange;
                        NPP = NPP - cellEnvironment["DNano"][currentMonth];

                        break;
          
                    case "microphytoplankton":

                        NPP = cellEnvironment["microNPP"][currentMonth];
                        // If nanophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;
                        /*  if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                          {
                              newT = cellEnvironment["Temperature"][currentMonth];
                              SST = cellEnvironment["Original Temperature"][currentMonth];
                              dSST = newT - SST;
                              if (newT < 12) pchange = -0.1;
                              else
                              {
                                  plarge = NPP / total_npp;
                                  dplarge = plarge + -0.05 * dSST;
                                  new_n_t = total_npp * dplarge;
                                  pchange = (NPP - new_n_t) / NPP;
                                  if (cellEnvironment["Bloom Start"][0] <= currentMonth && currentMonth <= cellEnvironment["Bloom End"][0])
                                  {
                                      pchange = 1.5 * pchange;
                                  }
                                  else pchange = 0.25 * pchange;
                              }
                         } */
                        else pchange = 0;
                        cellEnvironment["DMicro"][currentMonth] += NPP * pchange;
                        NPP = NPP - cellEnvironment["DMicro"][currentMonth];

                        break;
                }

                // Check that the units of oceanic NPP are gC per m2 per day
                Debug.Assert(oceanicNPPUnits == "gC/m2/day", "Oceanic NPP data are not in the correct units for this formulation of the model");

                if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;

                // Convert to g/cell/month
                NPP *= _MsqToKmSqConversion;

                // Calculate Carrying Capacity 
  
                // Multiply by cell area to get g/cell/day
                NPP *= cellEnvironment["Cell Area"][0];

                // Convert to g wet matter, assuming carbon content of phytoplankton is 10% of wet matter
                NPP *= _PhytoplanktonConversionRatio;

                // Convert to g/cell/month and add to the stock totalbiomass
                NPP *= Utilities.ConvertTimeUnits(GlobalModelTimeStepUnit, "day");

                //Calculate in grid biomass of current stock

                //Incorporate carrying capacity 
               // if (currentTimestep >= 500)
               // {
               //     dn = NPP *((K-left_overs)/K);
               // }
                dn = NPP;

                mortality = (left_overs + dn) * 0.1;

                sinking = (left_overs + dn) * sinking_rate;

                gridCellStockHandler[actingStock].TotalBiomass += (dn - mortality);

                if (trackProcesses.TrackProcesses && (outputDetail == "high") && specificLocations)
                {
                    trackProcesses.TrackPrimaryProductionTrophicFlow((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                        NPP);
                }

                if (trackProcesses.TrackProcesses)
                {
                    functionalTracker.RecordFGFlow((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0],
                        stockDefinitions.GetTraitNames("stock name", actingStock[0]), "autotroph net production", NPP,
                        cellEnvironment["Realm"][0] == 2.0);
                }

                if (globalTracker.TrackProcesses)
                {
                    globalTracker.RecordNPP((uint)cellEnvironment["LatIndex"][0], (uint)cellEnvironment["LonIndex"][0], (uint)actingStock[0],
                            NPP / cellEnvironment["Cell Area"][0]);
                }

                // If the biomass of the autotroph stock has been made less than zero (i.e. because of negative NPP) then reset to zero
                if (gridCellStockHandler[actingStock].TotalBiomass < 0.0)
                {
                    gridCellStockHandler[actingStock].TotalBiomass = 0.0;
                    Debug.Assert(gridCellStockHandler[actingStock].TotalBiomass >= 0.0, "stock negative");
                }

            }
            // Else if neither on land or in the ocean
            else
            {
                Debug.Fail("This is not a marine cell!");
                // Set the autotroph biomass to zero
                gridCellStockHandler[actingStock].TotalBiomass = 0.0;
            }
        }
    }
}

