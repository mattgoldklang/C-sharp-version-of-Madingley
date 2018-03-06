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
            double dmicro = 0.0;
            double dnano = 0.0;
            double sinking = new double();
            double dpico = new double();
            double pchange = new double();
            double n_prod = new double();
            double NPPi = new double();
            double N0 = new double();
            double Ni = new double();
            double T0 = new double();
            double Ti = new double();
            double Pico0 = new double();
            double Picoi = new double();
            double Nano0 = new double();
            double Nanoi = new double();
            double Micro0 = new double();
            double Microi = new double();
            double totalNPP0 = new double();
            double totalNPPi = new double();
            double[] bloom_values_micro = new double[12];
            double[] bloom_values_nano = new double[12];
            double left_overs = new double();
            double sinking_rate = new double();
            double dn = new double();
            int[] bloom_start = new int[2];
            int[] bloom_end = new int[2];


            // Check that this is an ocean cell
            if (cellEnvironment["Realm"][0] == 2.0)
            {
                //Calculates the bloom phenology of Micro production using the Hopkins et al 2015 method
                /*(currentTimestep == 0)
                 {
                     for (int m = 0; m < 12; m++)
                     {
                         bloom_values_micro[m] = cellEnvironment["microNPP"][m];
                         bloom_values_nano[m] = cellEnvironment["nanoNPP"][m];
                         cellEnvironment["Original microNPP"][m] = cellEnvironment["microNPP"][m];
                         cellEnvironment["Original nanoNPP"][m] = cellEnvironment["nanoNPP"][m];
                         cellEnvironment["Original picoNPP"][m] = cellEnvironment["picoNPP"][m];
                         cellEnvironment["totalNPP"][m] = cellEnvironment["microNPP"][m] + cellEnvironment["nanoNPP"][m] + cellEnvironment["picoNPP"][m];
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
                 }*/

                if(currentTimestep == 0)
                { 
                    for (int m = 0; m < 12; m++)
                    {
                        cellEnvironment["NPP"][m] = cellEnvironment["picoNPP"][m] + cellEnvironment["nanoNPP"][m] + cellEnvironment["microNPP"][m];
                    }
                }
                if (currentTimestep >=  burninSteps && temperatureScenario.Item1 == "escalating")
                {
                    N0 = cellEnvironment["Original NO3"][currentMonth];
                    Ni = cellEnvironment["NO3"][currentMonth];
                    T0 = cellEnvironment["Original Temperature"][currentMonth];
                    Ti = cellEnvironment["Temperature"][currentMonth];
                    Pico0 = 1.145 - (0.021 * N0) - (6.936 * Math.Pow(10, -6) * T0);
                    Picoi = 1.145 - (0.021 * Ni) - (6.936 * Math.Pow(10, -6) * Ti);
                    Nano0 = 1.146 - (0.013 * N0) - (.064 * T0);
                    Nanoi = 1.146 - (0.013 * Ni) - (.064 * Ti);
                    Micro0 = .804 - (0.002 * N0) - (.077 * T0);
                    Microi = .804 - (0.002 * Ni) - (.077 * Ti);
                    totalNPP0 = Pico0 + Nano0 + Micro0;
                    totalNPPi = Picoi + Nanoi + Microi;
                    pchange = (totalNPPi - totalNPP0) / totalNPP0;
                    cellEnvironment["totalNPP"][currentMonth] = cellEnvironment["NPP"][currentMonth] + (pchange * cellEnvironment["totalNPP"][currentMonth]);

                }
                switch (gridCellStockHandler[actingStock].StockName)
                {
                    case "picophytoplankton":

                        left_overs = cellEnvironment["Remaining Biomass"][0];
                        // Get picophytoplankton NPP from cell environment
                        NPP = cellEnvironment["picoNPP"][currentMonth];
                        NPPi = cellEnvironment["Original picoNPP"][currentMonth];
                        // If picophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0])
                        {
                            NPP = 0.0;
                        }
                       /* if (currentTimestep >  burninSteps)
                        {
                            dpico = cellEnvironment["Original picoNPP"][currentMonth] * cellEnvironment["multiplier"][0];
                            dnano = (cellEnvironment["Original nanoNPP"][currentMonth] / (cellEnvironment["Original microNPP"][currentMonth] + cellEnvironment["Original nanoNPP"][currentMonth])) * dpico;
                            dmicro = (cellEnvironment["Original microNPP"][currentMonth] / (cellEnvironment["Original nanoNPP"][currentMonth] + cellEnvironment["Original microNPP"][currentMonth])) * dpico;
                            if (cellEnvironment["Original microNPP"][currentMonth] < cellEnvironment["DMicro"][currentMonth]) dmicro = 0;
                            if (cellEnvironment["Original nanoNPP"][currentMonth] < cellEnvironment["DNano"][currentMonth]) dnano = 0;
                            cellEnvironment["DMicro"][currentMonth] = dmicro;
                            cellEnvironment["DNano"][currentMonth] = dnano;
                            cellEnvironment["DPico"][currentMonth] = dpico;
                        }*/
                        if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                        {
                            /*exponent1 = (0.11 * cellEnvironment["Original Temperature"][currentMonth]) - 0.47;
                            exponent2 = (0.11 * cellEnvironment["Temperature"][currentMonth]) - 0.47;
                            n_ratio = NPP / cellEnvironment["totalNPP"][currentMonth];
                            starting_t = Math.Pow(10, exponent1);
                            new_n_t = Math.Pow(10, exponent2);
                            pchange = (new_n_t - starting_t) / starting_t;
                            n_prod = (n_ratio + (n_ratio * pchange)) * cellEnvironment["totalNPP"][currentMonth];
                           /* if (n_ratio != pchange)
                            {
                                new_n_t = NPPi + (((NPPi - (pchange * cellEnvironment["n_total NPP"][currentMonth])) / (pchange - 1)) * cellEnvironment["n_total NPP"][currentMonth]);
                            } */
                            //Trejos et al ratio calc 
                            pchange = (Picoi - Pico0) / Math.Abs(Pico0);
                            n_prod = NPP + (NPP*pchange);
                            dpico = n_prod * cellEnvironment["multiplier"][0];
                            NPP = n_prod + dpico;
                            
                         }
                        cellEnvironment["Original picoNPP"][currentMonth] = NPP;
                      
                        break;

                    case "nanophytoplankton":

                        // Get the left over  NPP value for this month;
                        left_overs = cellEnvironment["Remaining Biomass"][1];
                        // Get nanophytoplankton NPP from cell environment
                        NPP = cellEnvironment["nanoNPP"][currentMonth];
                        NPPi = cellEnvironment["Original nanoNPP"][currentMonth];
                        // If nanophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;
                        if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                        {
                            pchange = (Nanoi - Nano0) / Math.Abs(Nano0);
                            n_prod = NPP + (NPP * pchange);
                            NPP = n_prod;
                            /*n_ratio =  NPPi / cellEnvironment["n_total NPP"][currentMonth];
                            if (n_ratio != pchange)
                            {
                                n_prod = ((pchange * cellEnvironment["n_total NPP"][currentMonth])) / (pchange - 1);
                            }
                            else n_prod = 0;*/
                            // NPP = (cellEnvironment["totalNPP"][currentMonth] * pchange);  // cellEnvironment["DNano"][currentMonth];
                            if (NPP < 0) NPP = 0;
                            /*if (cellEnvironment["Bloom Start"][1] <= currentMonth && currentMonth <= cellEnvironment["Bloom End"][1])
                            {
                                pchange = 1 * pchange;
                            }*/
 
                        }
                        cellEnvironment["Original nanoNPP"][currentMonth] = NPP;

                        break;
          
                    case "microphytoplankton":

                        NPP = cellEnvironment["microNPP"][currentMonth];
                        left_overs = cellEnvironment["Remaining Biomass"][2];
                        NPPi = cellEnvironment["Original microNPP"][currentMonth];
                        // If nanophytoplankton NPP is a missing value then set to zero
                        if (NPP == cellEnvironment["Missing Value"][0]) NPP = 0.0;
                        if (temperatureScenario.Item1 == "escalating" && currentTimestep >= burninSteps)
                        {
                            pchange = (Microi - Micro0) / Math.Abs(Micro0);
                            n_prod = NPP + (NPP * pchange);
                            NPP = n_prod;
                            /*n_ratio = NPPi / cellEnvironment["n_total NPP"][currentMonth];
                            if (n_ratio != pchange)
                            {
                                n_prod = ((pchange * cellEnvironment["n_total NPP"][currentMonth])) / (pchange - 1);
                            }
                            else n_prod = 0;*/
                            //NPP = (cellEnvironment["totalNPP"][currentMonth] * pchange); // cellEnvironment["DNano"][currentMonth];
                            if (NPP < 0) NPP = 0;
                            /*if (cellEnvironment["Bloom Start"][1] <= currentMonth && currentMonth <= cellEnvironment["Bloom End"][1])
                            {
                                pchange = 1 * pchange;
                            }*/

                        }
                        cellEnvironment["Original microNPP"][currentMonth] = NPP;

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

                mortality = (left_overs * 0);

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

