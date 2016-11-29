using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Madingley
{
    public class CohortTracker : Tracker
    {
        private StreamWriter CohortFlowsWriter;

        private TextWriter SyncedCohortFlowsWriter;

        private float[] Latitudes;
        private float[] Longitudes;

        private string OutputPath;
        private string FileName;
        private string FileSuffix;

        // Lists to hold information to record. Everything is done at the individual level - i.e. PredationEaten is the amount eaten per individual
        private List<string[]> CohortIdentifierStrings;
        private List<uint[]> CohortIdentifiers;
        private List<double> PredationEaten;
        private List<double> HerbivoryEaten;
        private List<double[]> FixedCohortProperties;
        private List<double> MetabolicCosts;
        private List<double> GrowthRates;

        private int MaxNumberFunctionalGroups;
        /// <summary>
        /// Set up the tracker for outputing properties of the eating process between functional groups
        /// </summary>
        /// <param name="numLats">The number of latitudes in the model grid</param>
        /// <param name="numLons">The number of longitudes in the model grid</param>
        /// <param name="fGFlowsFilename">The filename to write data on function group flows to</param>
        /// <param name="outputFilesSuffix">The suffix to apply to output files from this simulation</param>
        /// <param name="outputPath">The file path to write all outputs to</param>
        /// <param name="cellIndex">The index of the current cell within the list of all grid cells in this simulation</param>
        /// <param name="initialisation">The instance of the MadingleyModelInitialisation class for this simulation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        /// <param name="fGDefinitions">Hold the functional group definitions</param>
        public CohortTracker(float[] latitudes, float[] longitudes, MadingleyModelInitialisation initialisation, FunctionalGroupDefinitions fGDefinitions,
            FunctionalGroupDefinitions stockFGDefinitions, string outputFileSuffix, string outputPath, string fileName)
        {
            Latitudes = latitudes;
            Longitudes = longitudes;

            FileName = fileName;
            OutputPath = outputPath;
            FileSuffix = outputFileSuffix;

            // Assign a number and name to each functional group (FG as defined by output, not by model)
            AssignFunctionalGroups(initialisation, fGDefinitions, stockFGDefinitions);

            // Initialise array to hold mass flows among functional groups
            MaxNumberFunctionalGroups = Math.Max(NumberMarineFGsForTracking, NumberTerrestrialFGsForTracking);

            CohortIdentifierStrings = new List<string[]>();
            CohortIdentifiers = new List<uint[]>();
            FixedCohortProperties = new List<double[]>();
            PredationEaten = new List<double>();
            HerbivoryEaten = new List<double>();
            MetabolicCosts = new List<double>();
            GrowthRates = new List<double>();

        }

        /// <summary>
        /// Open the tracking file for writing to
        /// </summary>
        override public void OpenTrackerFile()
        {
            CohortFlowsWriter = new StreamWriter(OutputPath + FileName + FileSuffix + ".txt");
            SyncedCohortFlowsWriter = TextWriter.Synchronized(CohortFlowsWriter);
            // Identifier properties: Lat index, lon index, cohort ID
            // Identified string properties: FG
            // Double flows: predation, herbivory
            // Fixed double properties: mass, abundance, metabolic cost, growth

            SyncedCohortFlowsWriter.WriteLine
                ("Latitude\tLongitude\ttime_step\tfunctional_group\tcohort_ID\tbody_mass_g\tabundance\tpredation_eaten_perind_g\therbivory_eaten_perind_g\tmetabolic_cost_perind_g\tgrowth_perind_g");
        }
        public void RecordGeneralCohortInformation(uint latIndex, uint lonIndex, Cohort cohort, MadingleyModelInitialisation madingleyInitialisation,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, string cohortOrStockName, double cohortOrStockBodyMass, Boolean Marine)
        {
            CohortIdentifiers.Add(new uint[3] { latIndex, lonIndex, cohort.CohortID[0] });
            CohortIdentifierStrings.Add(new string[1] { MarineFGsForTracking.Keys.ToArray()[DetermineFunctionalGroup(madingleyInitialisation, stockFunctionalGroupDefinitions, cohortOrStockName, cohortOrStockBodyMass, Marine)] });
            FixedCohortProperties.Add(new double[] { cohort.IndividualBodyMass, cohort.CohortAbundance });
        }

        public void RecordMetabolicCost(double metabolicCostPerIndividual)
        {
            MetabolicCosts.Add(metabolicCostPerIndividual);
        }

        public void RecordGrowth(double growthPerIndividual)
        {

            GrowthRates.Add(growthPerIndividual);

        }

        /// <summary>
        /// Record an eating (predation or herbivory) event
        /// </summary>
        /// <param name="latIndex"></param>
        /// <param name="lonIndex"></param>
        /// <param name="massEaten"></param>
        /// <param name="marineCell"></param>
        public void RecordPredationFlow(uint latIndex, uint lonIndex, Cohort cohort, double massEaten, Boolean herbivory, Boolean marineCell)
        {
            if (!herbivory)
                PredationEaten.Add(massEaten);
            else
                HerbivoryEaten.Add(massEaten);
        }


        /// <summary>
        /// Write flows of matter among functional groups to the output file at the end of the time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="numLats">The latitudinal dimension of the model grid in number of cells</param>
        /// <param name="numLons">The longitudinal dimension of the model grid in number of cells</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        override public void WriteToTrackerFile(uint currentTimeStep, ModelGrid madingleyModelGrid, uint numLats, uint numLons,
            MadingleyModelInitialisation initialisation, Boolean MarineCell)
        {
            int NumCohortsToWrite = CohortIdentifiers.Count;

           
                    for (int i = 0; i < NumCohortsToWrite; i++)
                    {
                Console.WriteLine("Num cohorts to write" + i);
                        SyncedCohortFlowsWriter.WriteLine(Convert.ToString(madingleyModelGrid.GetCellLatitude(CohortIdentifiers.ElementAt(i)[0])) + '\t' +
                            Convert.ToString(madingleyModelGrid.GetCellLongitude(CohortIdentifiers.ElementAt(i)[1])) + '\t' + Convert.ToString(currentTimeStep) +
                            '\t' + CohortIdentifierStrings.ElementAt(i)[0] + '\t' + CohortIdentifiers.ElementAt(i)[2] + '\t' +
                            FixedCohortProperties.ElementAt(i)[0] + '\t' + FixedCohortProperties.ElementAt(i)[1] + '\t' +
                            PredationEaten.ElementAt(i) + '\t' + HerbivoryEaten.ElementAt(i) + '\t' +
                            MetabolicCosts[i] + '\t' + GrowthRates[i]);
                    }


            // Reset lists
            CohortIdentifiers = new List<uint[]>();
            CohortIdentifierStrings = new List<string[]>();
            FixedCohortProperties = new List<double[]>();
            PredationEaten = new List<double>();
            HerbivoryEaten = new List<double>();
            MetabolicCosts = new List<double>();
            GrowthRates = new List<double>();
        }


        /// <summary>
        /// Close the file that has been written to
        /// </summary>
        override public void CloseTrackerFile()
        {
            CohortFlowsWriter.Dispose();
        }


    }
}
