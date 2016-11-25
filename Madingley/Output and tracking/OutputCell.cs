using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using System.IO;

using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

using Timing;

namespace Madingley
{

    /// <summary>
    /// A class to perform all operations involved in outputting the results to console, screen or file
    /// </summary>
    public class OutputCell : Output
    {
        /// <summary>
        /// Designates the level of output detail
        /// </summary>
        private enum OutputDetailLevel { Low, Medium, High };

        /// <summary>
        /// An instance of the enumerator to designate output detail level
        /// </summary>
        OutputDetailLevel ModelOutputDetail;

        /// <summary>
        /// A dataset to store the live screen view
        /// </summary>
        private DataSet DataSetToViewLive;

        /// <summary>
        /// A version of the basic outputs dataset to hold data for output in memory while running the model
        /// </summary>
        private DataSet BasicOutputMemory;

        /// <summary>
        /// A memory version of the mass bins output to store data during the model run
        /// </summary>
        private DataSet MassBinsOutputMemory;

        /// <summary>
        /// A memory version of the tracked cohorts output to hold data during the model run
        /// </summary>
        private DataSet TrackedCohortsOutputMemory;

        /// <summary>
        /// The total living biomass in the model
        /// </summary>
        private double TotalLivingBiomass;

        /// <summary>
        /// The total living biomass in the model
        /// </summary>
        private double TotalLivingBiomassDensity;

        /// <summary>
        /// The total heterotroph biomass in the model
        /// </summary>
        private double TotalHeterotrophBiomassDensity;

        /// <summary>
        /// The total heterotroph abundance in the model
        /// </summary>
        private double TotalHeterotrophAbundanceDensity;


        /// <summary>
        /// Total NPP incoming from the marine model
        /// </summary>
        private double TotalIncomingNPP;

        /// <summary>
        /// Total densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalDensitiesOut = new SortedList<string, double>();

        /// <summary>
        /// Total densities of all cohorts within each combination of cohort traits (marine)
        /// </summary>
        private SortedList<string, double> TotalDensitiesMarineOut = new SortedList<string, double>();

        /// <summary>
        /// Total biomass densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalBiomassDensitiesOut = new SortedList<string, double>();

        /// <summary>
        /// Total biomass densities of all cohorts within each combination of cohort traits
        /// </summary>
        private SortedList<string, double> TotalBiomassDensitiesMarineOut = new SortedList<string, double>();


        /// <summary>
        /// List of vectors of abundances in mass bins corresponding with each unique trait value
        /// </summary>
        private SortedList<string, double[]> AbundancesInMassBins = new SortedList<string, double[]>();

        /// <summary>
        /// List of vectors of biomasses in mass bins corresponding with each unique trait value
        /// </summary>
        private SortedList<string, double[]> BiomassesInMassBins = new SortedList<string, double[]>();

        /// <summary>
        /// List of arrays of abundance in juvenile vs. adult mass bins correpsonding with each unique trait value
        /// </summary>
        private SortedList<string, double[,]> AbundancesInJuvenileAdultMassBins = new SortedList<string, double[,]>();

        /// <summary>
        /// List of arrays of biomass in juvenile vs. adult mass bins correpsonding with each unique trait value
        /// </summary>
        private SortedList<string, double[,]> BiomassesInJuvenileAdultMassBins = new SortedList<string, double[,]>();

        /// <summary>
        /// The number of mass bins to use in model outputs
        /// </summary>
        private int MassBinNumber;

        /// <summary>
        /// The mass bins to use in model outputs
        /// </summary>
        private float[] MassBins;

        /// <summary>
        /// The mass bin handler for the mass bins to use in the model output
        /// </summary>
        private MassBinsHandler _MassBinHandler;

        /// <summary>
        /// The upper limit for the y-axis of the live output
        /// </summary>
        private double MaximumYValue;

        /// <summary>
        /// The time steps in this model simulation
        /// </summary>
        private float[] TimeSteps;

        /// <summary>
        /// List to hold cohort IDs of tracked cohorts
        /// </summary>
        List<uint> TrackedCohorts;

        /// <summary>
        /// The path to the output folder
        /// </summary>
        private string _OutputPath;
        /// <summary>
        /// Get the path to the output folder
        /// </summary>
        public string OutputPath { get { return _OutputPath; } }

        /// <summary>
        /// The suffix to apply to all outputs from this grid cell
        /// </summary>
        private string _OutputSuffix;
        /// <summary>
        /// Get the suffix for ouputs for this grid cell
        /// </summary>
        public string OutputSuffix
        { get { return _OutputSuffix; } }


        /// <summary>
        /// The cohort traits to be considered in the outputs
        /// </summary>
        private string[] CohortTraits;

        /// <summary>
        /// All unique values of the traits to be considered in outputs (terrestrial only)
        /// </summary>
        private SortedDictionary<string, string[]> CohortTraitValues;

        /// <summary>
        /// All unique values of the traits to be considered in marine outputs
        /// </summary>
        private SortedDictionary<string, string[]> CohortTraitValuesMarine;

        /// <summary>
        /// The stock traits to be considered in the outputs
        /// </summary>
        private string[] StockTraits;

        /// <summary>
        /// The marine stock traits to be considered in the outputs
        /// </summary>
        private string[] StockTraitsMarine;

        /// <summary>
        /// All unique values of the traits to be considered in the outputs
        /// </summary>
        private SortedDictionary<string, string[]> StockTraitValues;

        /// <summary>
        /// All unique values of the traits to be considered in the marine outputs
        /// </summary>
        private SortedDictionary<string, string[]> StockTraitValuesMarine;

        /// <summary>
        /// Vector of individual body masses of the tracked cohorts
        /// </summary>
        private double[] TrackedCohortIndividualMasses;

        /// <summary>
        /// Vector of abundances of the tracked cohorts
        /// </summary>
        private double[] TrackedCohortAbundances;

        /// <summary>
        /// Instance of the class to convert data between arrays and SDS objects
        /// </summary>
        private ArraySDSConvert DataConverter;

        /// <summary>
        /// Intance of the class to create SDS objects
        /// </summary>
        private CreateSDSObject SDSCreator;

        /// <summary>
        /// An instance of the class to view grid results
        /// </summary>
        private ViewGrid GridViewer;

        /// <summary>
        /// Whether to display live outputs during the model run
        /// </summary>
        private Boolean LiveOutputs;

        /// <summary>
        ///  Track marine specific functional groups (i.e. plankton, baleen whales)
        /// </summary>
        private Boolean TrackMarineSpecifics;

        /// <summary>
        /// The size threshold for determining whether an organism is planktonic
        /// </summary>
        private double PlanktonSizeThreshold;


        /// <summary>
        /// Indicates whether to output metric information
        /// </summary>
        private Boolean OutputMetrics;

        /// <summary>
        /// Instance of the class to calculate ecosystem metrics
        /// </summary>
        private EcosytemMetrics Metrics;


        /// <summary>
        /// Constructor for the cell output class
        /// </summary>
        /// <param name="outputDetail">The level of detail to include in the ouputs: 'low', 'medium' or 'high'</param>
        /// <param name="modelInitialisation">Model initialisation object</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        public OutputCell(string outputDetail, MadingleyModelInitialisation modelInitialisation,
            int cellIndex, FunctionalGroupDefinitions cohortFGDefinitions, FunctionalGroupDefinitions stockFGDefinitions)
        {
            // Set the output path
            _OutputPath = modelInitialisation.OutputPath;

            // Set the initial maximum value for the y-axis of the live display
            MaximumYValue = 1000000;

            // Set the local copy of the mass bin handler from the model initialisation
            _MassBinHandler = modelInitialisation.ModelMassBins;

            // Get the number of mass bins to be used
            MassBinNumber = _MassBinHandler.NumMassBins;

            // Get the specified mass bins
            MassBins = modelInitialisation.ModelMassBins.GetSpecifiedMassBins();


            // Assign a number and name to each functional group (FG as defined by output, not by model)
            AssignFunctionalGroups(modelInitialisation, cohortFGDefinitions, stockFGDefinitions);

            // Set the output detail level
            if (outputDetail == "low")
                ModelOutputDetail = OutputDetailLevel.Low;
            else if (outputDetail == "medium")
                ModelOutputDetail = OutputDetailLevel.Medium;
            else if (outputDetail == "high")
                ModelOutputDetail = OutputDetailLevel.High;
            else
                Debug.Fail("Specified output detail level is not valid, must be 'low', 'medium' or 'high'");

            // Get whether to track metrics
            OutputMetrics = modelInitialisation.OutputMetrics;

            //Initialise the EcosystemMetrics class
            Metrics = new EcosytemMetrics();

            // Initialise the data converter
            DataConverter = new ArraySDSConvert();

            // Initialise the SDS object creator
            SDSCreator = new CreateSDSObject();

            // Initialise the grid viewer
            GridViewer = new ViewGrid();

            // Set the local variable designating whether to display live outputs
            if (modelInitialisation.LiveOutputs)
                LiveOutputs = true;

        }

        /// <summary>
        /// Spawn dataset viewer for the live outputs
        /// </summary>
        /// <param name="NumTimeSteps">The number of time steps in the model run</param>
        public void SpawnDatasetViewer(uint NumTimeSteps)
        {
            Console.WriteLine("Spawning Dataset Viewer\n");

            // Intialise the SDS object for the live view
            DataSetToViewLive = SDSCreator.CreateSDSInMemory(true);

            // Check the output detail level
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // For low detail level, just show total living biomass
                DataSetToViewLive.Metadata["VisualHints"] = "\"Total living biomass\"[Time step]; Style:Polyline; Visible: 0,1," +
                    NumTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y; Stroke:#D95F02; Thickness:3; Title:\"Total Biomass" + "\"";
            }
            else
            {
                // For medium and high detail levels, show biomass by trophic level
                //DataSetToViewLive.Metadata["VisualHints"] = "\"autotroph biomass\"[Time step]; Style:Polyline; Visible: 0,1,"
                //    + NumTimeSteps.ToString() + ","
                //    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF008040;Thickness:3;;\"carnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                //    + NumTimeSteps.ToString() + ","
                //    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FFFF0000;Thickness:3;;\"herbivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                //    + NumTimeSteps.ToString() + ","
                //    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF00FF00;Thickness:3;;\"omnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                //    + NumTimeSteps.ToString() + ","
                //    + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF0000FF;Thickness:3; Title:\"Biomass Densities";
                
            }


            // Start viewing
            GridViewer.AsynchronousView(ref DataSetToViewLive, "");

        }

        /// <summary>
        /// Set up all outputs (live, console and file) prior to the model run
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid that output data will be derived from</param>
        /// <param name="cohortFunctionalGroupDefinitions">The definitions for cohort functional groups</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions for stock functional groups</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="outputFilesSuffix">The suffix to be applied to all output files from the current model run</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current grid cell in the list of indices of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void SetUpOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, uint numTimeSteps, string outputFilesSuffix, List<uint[]> cellIndices,
            int cellIndex, Boolean marineCell)
        {
            Console.WriteLine("Setting up grid cell outputs...\n");

            // Set the suffix for all output files
            _OutputSuffix = outputFilesSuffix + "_Cell" + cellIndex;

            // Create vector to hold the values of the time dimension
            TimeSteps = new float[numTimeSteps + 1];

            // Set the first value to be -1 (this will hold initial outputs)
            TimeSteps[0] = 0;

            // Fill other values from 0 (this will hold outputs during the model run)
            for (int i = 1; i < numTimeSteps + 1; i++)
            {
                TimeSteps[i] = i;
            }

            // Initialise the trait based outputs
            InitialiseTraitBasedOutputs(cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, marineCell);

            // Setup low-level outputs
            SetUpLowLevelOutputs(numTimeSteps, ecosystemModelGrid);

            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {
                // Setup medium-level outputs
                SetupMediumLevelOutputs(ecosystemModelGrid, marineCell);

                if (ModelOutputDetail == OutputDetailLevel.High)
                {
                    // Setup high-level outputs
                    SetUpHighLevelOutputs(ecosystemModelGrid, cellIndices, cellIndex, cohortFunctionalGroupDefinitions, marineCell);
                }
            }


        }

        /// <summary>
        /// Set up the necessary architecture for generating outputs arranged by trait value
        /// </summary>
        /// <param name="cohortFunctionalGroupDefinitions">Functional group definitions for cohorts in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">Functional group definitions for stocks in the model</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void InitialiseTraitBasedOutputs(FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, FunctionalGroupDefinitions
            stockFunctionalGroupDefinitions, Boolean marineCell)
        {

            if (marineCell)
            {
                // Add unique cohort and stock FGs to the lists that will contain output data arranged by FG
                foreach (int FG in MarineFGsForTracking.Values)
                {
                    TotalBiomassDensitiesMarineOut.Add(MarineFGsForTracking.Keys.ToArray()[FG], 0.0);
                    // TotalDensitiesMarineOut.Add(MarineFGsForTracking.Keys.ToArray()[FG], 0.0);
                }

            }
            else
            {
                // Add unique cohort and stock FGs to the lists that will contain output data arranged by FG
                foreach (int FG in TerrestrialFGsForTracking.Values)
                {
                    TotalBiomassDensitiesMarineOut.Add(TerrestrialFGsForTracking.Keys.ToArray()[FG], 0.0);
                    // TotalDensitiesMarineOut.Add(TerrestrialFGsForTracking.Keys.ToArray()[FG], 0.0);
                }
            }
        }


        /// <summary>
        /// Sets up the outputs associated with all levels of output detail
        /// </summary>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="ecosystemModelGrid">The model grid</param>
        private void SetUpLowLevelOutputs(uint numTimeSteps, ModelGrid ecosystemModelGrid)
        {
            // Create an SDS object to hold total abundance and biomass data
            // BasicOutput = SDSCreator.CreateSDS("netCDF", "BasicOutputs" + _OutputSuffix, _OutputPath);
            BasicOutputMemory = SDSCreator.CreateSDSInMemory(true);
            string[] TimeDimension = { "Time step" };
            DataConverter.AddVariable(BasicOutputMemory, "Total Biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            DataConverter.AddVariable(BasicOutputMemory, "Heterotroph Abundance density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            DataConverter.AddVariable(BasicOutputMemory, "Heterotroph Biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
        }

        /// <summary>
        /// Sets up the outputs associated with medium and high levels of output detail
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void SetupMediumLevelOutputs(ModelGrid ecosystemModelGrid, Boolean marineCell)
        {

            string[] TimeDimension = { "Time step" };

            if (OutputMetrics)
            {
                DataConverter.AddVariable(BasicOutputMemory, "Mean Trophic Level", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Trophic Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Biomass Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Functional Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Rao Functional Evenness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Biomass Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Trophic Richness", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);

                DataConverter.AddVariable(BasicOutputMemory, "Max Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Min Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Max Trophic Index", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Min Trophic Index", "dimensionless", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Geometric Mean Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                DataConverter.AddVariable(BasicOutputMemory, "Arithmetic Mean Bodymass", "g", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
            }

            if (marineCell)
            {
                foreach (string FG in MarineFGsForTracking.Keys.ToArray())
                {
                    //  DataConverter.AddVariable(BasicOutputMemory, FG + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                    DataConverter.AddVariable(BasicOutputMemory, FG + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
            }
            else
            {
                foreach (string FG in TerrestrialFGsForTracking.Keys.ToArray())
                {
                    //  DataConverter.AddVariable(BasicOutputMemory, FG + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                    DataConverter.AddVariable(BasicOutputMemory, FG + " biomass density", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                }
            }
        }

        /// <summary>
        /// Sets up the outputs associated with the high level of output detail
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The indices of active cells in the model grid</param>
        /// <param name="cellNumber">The index of the current cell in the list of active cells</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void SetUpHighLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellNumber,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, Boolean marineCell)
        {
            ;
        }

        /// <summary>
        /// Calculates the variables to output
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get output data from</param>
        /// <param name="cohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">The sorted list of global diagnostic variables in the model</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber, SortedList<string, double>
            globalDiagnosticVariables, MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {
            // Calculate low-level outputs
            CalculateLowLevelOutputs(ecosystemModelGrid, cellIndices, cellNumber, globalDiagnosticVariables, cohortFunctionalGroupDefinitions,
                stockFunctionalGroupDefinitions, initialisation, month, marineCell);

            if (ModelOutputDetail == OutputDetailLevel.High)
            {
                // Calculate high-level outputs
                CalculateHighLevelOutputs(ecosystemModelGrid, cellIndices, cellNumber, marineCell);
            }


        }

        /// <summary>
        /// Calculate outputs associated with low-level outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">The list of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The position of the current cell in the list of active cells</param>
        /// <param name="globalDiagnosticVariables">The global diagnostic variables for this model run</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The functional group definitions of stocks in the model</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateLowLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex,
            SortedList<string, double> globalDiagnosticVariables, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, MadingleyModelInitialisation initialisation, uint month,
            Boolean marineCell)
        {
            // Reset the total living biomass
            TotalLivingBiomass = 0.0;

            // Note that this does both cohorts and stocks
            if (marineCell)
            {
                // Get biomass, abundance and densities for each of the trait combinations. Note that the GetStateVariableDensity function deals with the assessment of 
                // whether cohorts contain individuals of low enough mass to be considered zooplankton in the marine realm
                foreach (int FG in MarineFGsForTracking.Values)
                {
                    // Biomass density
                    TotalBiomassDensitiesOut[MarineFGsForTracking.Keys.ToArray()[FG]] = ecosystemModelGrid.GetStateVariableDensity(this, "Biomass", FG, cohortFunctionalGroupDefinitions,
                    stockFunctionalGroupDefinitions, cellIndices[cellIndex][0], cellIndices[cellIndex][1], initialisation, marineCell);

                    // Density
                    //  TotalDensitiesOut[FG] = ecosystemModelGrid.GetStateVariableDensity("Abundance", TraitValue, CohortTraitIndicesMarine[FG], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
                }
            }
            else
            {
                // Get biomass, abundance and densities for each of the trait combinations. Note that the GetStateVariableDensity function deals with the assessment of 
                // whether cohorts contain individuals of low enough mass to be considered zooplankton in the marine realm
                foreach (int FG in TerrestrialFGsForTracking.Values)
                {
                    // Biomass density
                    TotalBiomassDensitiesOut[TerrestrialFGsForTracking.Keys.ToArray()[FG]] = ecosystemModelGrid.GetStateVariableDensity(this, "Biomass", FG, cohortFunctionalGroupDefinitions,
                    stockFunctionalGroupDefinitions, cellIndices[cellIndex][0], cellIndices[cellIndex][1], initialisation, marineCell);
                    

            // Density
                    //  TotalDensitiesOut[FG] = ecosystemModelGrid.GetStateVariableDensity("Abundance", TraitValue, CohortTraitIndicesMarine[FG], cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
    }
}

            // Add the total biomass of all cohorts to the total living biomass variable
            TotalLivingBiomass += ecosystemModelGrid.GetStateVariable("Biomass", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex,
                cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
            TotalLivingBiomassDensity = ecosystemModelGrid.GetStateVariableDensity("Biomass", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation) / 1000.0;
            TotalHeterotrophAbundanceDensity = ecosystemModelGrid.GetStateVariableDensity("Abundance", "NA", cohortFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "cohort", initialisation);
            TotalHeterotrophBiomassDensity = TotalLivingBiomassDensity;

            // Add the total biomass of all stocks to the total living biomass variable
            TotalLivingBiomass += ecosystemModelGrid.GetStateVariable("Biomass", "NA", stockFunctionalGroupDefinitions.AllFunctionalGroupsIndex,
                cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation);
            TotalLivingBiomassDensity += ecosystemModelGrid.GetStateVariableDensity("Biomass", "NA", stockFunctionalGroupDefinitions.AllFunctionalGroupsIndex, cellIndices[cellIndex][0], cellIndices[cellIndex][1], "stock", initialisation) / 1000.0;
        }

        /// <summary>
        /// Calculate outputs associated with high-level outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current cell in the list of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateHighLevelOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex, Boolean marineCell)
        {
            ;
        }

        /// <summary>
        /// Write to the output file values of the output variables before the first time step
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get data from</param>C:\madingley-ecosystem-model\Madingley\Output and tracking\PredationTracker.cs
        /// <param name="cohortFunctionalGroupDefinitions">The definitions of cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions of stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void InitialOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber,
            SortedList<string, double> globalDiagnosticVariables, uint numTimeSteps, MadingleyModelInitialisation initialisation,
            uint month, Boolean marineCell)
        {

            // Calculate values of the output variables to be used
            CalculateOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, cellIndices, cellNumber, globalDiagnosticVariables, initialisation, month, marineCell);

            // Generate the intial live outputs
            if (LiveOutputs)
            {
                InitialLiveOutputs(ecosystemModelGrid, marineCell);
            }
            // Generate the intial file outputs
            InitialFileOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, marineCell, cellIndices, cellNumber);

        }

        /// <summary>
        /// Generates the intial output to the live dataset view
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void InitialLiveOutputs(ModelGrid ecosystemModelGrid, Boolean marineCell)
        {
            // Create a string holding the name of the x-axis variable
            string[] TimeDimension = { "Time step" };

            // Add the x-axis to the plots (time step)
            DataSetToViewLive.AddAxis("Time step", "Month", TimeSteps);

            // Add the relevant output variables depending on the specified level of detail
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // Add the variable for total living biomass
                DataConverter.AddVariable(DataSetToViewLive, "Total living biomass", "kg", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);

                // Add the initial value of total living biomass
                DataConverter.ValueToSDS1D(TotalLivingBiomass, "Total living biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
            }
            else
            {
                if (marineCell)
                {
                    foreach (string FG in MarineFGsForTracking.Keys.ToArray())
                    {
                        // Add in the carnivore and herbivore abundance variables
                       // DataConverter.AddVariable(DataSetToViewLive, FG + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        //DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                        // Add in the carnivore and herbivore biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, FG + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }
                }
                else
                {
                    foreach (string FG in TerrestrialFGsForTracking.Keys.ToArray())
                    {
                        // Add in the carnivore and herbivore abundance variables
                      //  DataConverter.AddVariable(DataSetToViewLive, FG + " density", "Individuals / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                       // DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                        // Add in the carnivore and herbivore biomass variables
                        DataConverter.AddVariable(DataSetToViewLive, FG + " biomass", "Kg / km^2", 1, TimeDimension, ecosystemModelGrid.GlobalMissingValue, TimeSteps);
                        // Add in the initial values of carnivore and herbivore abundance
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, 0);
                    }
                }



            }
        }



        /// <summary>
        /// Generates the initial file outputs
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        /// <param name="cellIndices">The list of all cells to run the model for</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        private void InitialFileOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            Boolean MarineCell, List<uint[]> cellIndices, int cellIndex)
        {
            Console.WriteLine("Writing initial grid cell outputs to memory...");

            //Write the low level outputs first
            DataConverter.ValueToSDS1D(TotalLivingBiomassDensity, "Total Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
            // DataConverter.ValueToSDS1D(TotalHeterotrophAbundanceDensity, "Heterotroph Abundance density", "Time step",
            //               ecosystemModelGrid.GlobalMissingValue,
            //             BasicOutputMemory, 0);
            DataConverter.ValueToSDS1D(TotalHeterotrophBiomassDensity, "Heterotroph Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);

            // File outputs for medium and high detail levels
            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {


                if (MarineCell)
                {
                    foreach (string FG in MarineFGsForTracking.Keys.ToArray())
                    {
                        // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
                        // DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step",
                        //   ecosystemModelGrid.GlobalMissingValue,
                        // BasicOutputMemory, 0);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                    }
                }
                else
                {
                    foreach (string FG in TerrestrialFGsForTracking.Keys.ToArray())
                    {
                        // Write densities, biomasses and abundances in different functional groups to the relevant one-dimensional output variables
                        DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, 0);
                    }
                }

                if (OutputMetrics)
                {
                    DataConverter.ValueToSDS1D(Metrics.CalculateMeanTrophicLevelCell(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Mean Trophic Level", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "trophic index"),
                                                "Trophic Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "biomass"),
                                                "Biomass Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    double[] FunctionalDiversity = Metrics.CalculateFunctionalDiversity(ecosystemModelGrid, cohortFunctionalGroupDefinitions,
                        cellIndices, cellIndex);

                    DataConverter.ValueToSDS1D(FunctionalDiversity[0],
                                                 "Functional Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                 BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[1],
                                                "Rao Functional Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[0],
                                                "Biomass Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[1],
                                                "Min Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[2],
                                                "Max Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[0],
                                                "Trophic Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[1],
                                                "Min Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[2],
                                                "Max Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                    DataConverter.ValueToSDS1D(Metrics.CalculateArithmeticCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Arithmetic Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);
                    DataConverter.ValueToSDS1D(Metrics.CalculateGeometricCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Geometric Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, 0);

                }

                // File outputs for high detail level
                if (ModelOutputDetail == OutputDetailLevel.High)
                {
                    ;
                }

            }

        }

        /// <summary>
        /// Write to the output file values of the output variables during the model time steps
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid to get data from</param>
        /// <param name="cohortFunctionalGroupDefinitions">The definitions of the cohort functional groups in the model</param>
        /// <param name="stockFunctionalGroupDefinitions">The definitions of the stock  functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="globalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="timeStepTimer">The timer for the current time step</param>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="currentTimestep">The current model time step</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void TimeStepOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber,
            SortedList<string, double> globalDiagnosticVariables, StopWatch timeStepTimer, uint numTimeSteps, uint currentTimestep,
            MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {

            // Calculate values of the output variables to be used
            CalculateOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, stockFunctionalGroupDefinitions, cellIndices, cellNumber, globalDiagnosticVariables, initialisation, month, marineCell);

            // Generate the live outputs for this time step
            if (LiveOutputs)
            {
                TimeStepLiveOutputs(numTimeSteps, currentTimestep, ecosystemModelGrid, marineCell);
            }

            // Generate the console outputs for the current time step
            TimeStepConsoleOutputs(currentTimestep, timeStepTimer);

            // Generate the file outputs for the current time step
            TimeStepFileOutputs(ecosystemModelGrid, cohortFunctionalGroupDefinitions, currentTimestep, marineCell, cellIndices, cellNumber);

        }

        /// <summary>
        /// Generate the live outputs for the current time step
        /// </summary>
        /// <param name="numTimeSteps">The number of time steps in the model run</param>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void TimeStepLiveOutputs(uint numTimeSteps, uint currentTimeStep, ModelGrid ecosystemModelGrid, Boolean marineCell)
        {

            // Output to the live graph view according to the specified level of detail
            if (ModelOutputDetail == OutputDetailLevel.Low)
            {
                // Rescale the y-axis if necessary
                if (TotalLivingBiomass > MaximumYValue)
                {
                    MaximumYValue = TotalLivingBiomass * 1.1;
                    DataSetToViewLive.Metadata["VisualHints"] = "\"Total living biomass\"[Time step]; Style:Polyline; Visible: 0,1," +
                    numTimeSteps.ToString() + "," + MaximumYValue.ToString() +
                    "; LogScale:Y; Stroke:#D95F02; Thickness:3; Title:\"Total Biomass" + "\"";
                }
                // Write out total living biomass
                DataConverter.ValueToSDS1D(TotalLivingBiomass, "Total living biomass", "Time step",
                    ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);

            }
            else
            {
                //Find the max value in the TotalBiomassDensities
                double MaxVal = 0.0;
                foreach (var KVPair in TotalBiomassDensitiesOut)
                {
                    if (KVPair.Value > MaxVal) MaxVal = KVPair.Value;
                }
                // Rescale the y-axis if necessary
                if (MaxVal > MaximumYValue)
                {
                    MaximumYValue = MaxVal * 1.1;
                    DataSetToViewLive.Metadata["VisualHints"] = "\"autotroph biomass\"[Time step]; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF008040;Thickness:3;;\"carnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FFFF0000;Thickness:3;;\"herbivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF00FF00;Thickness:3;;\"omnivore biomass\"[Time step] ; Style:Polyline; Visible: 0,1,"
                        + numTimeSteps.ToString() + ","
                        + MaximumYValue.ToString() + "; LogScale:Y;  Stroke:#FF0000FF;Thickness:3; Title:\"Biomass Densities";
                }

                if (marineCell)
                {
                    foreach (string FG in MarineFGsForTracking.Keys.ToArray())
                    {
                        // Output the total carnivore, herbivore and omnivore abundances
                        // DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }
                }
                else
                {
                    foreach (string FG in TerrestrialFGsForTracking.Keys.ToArray())
                    {
                        // Output the total carnivore, herbivore and omnivore abundances
                       // DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass", "Time step", ecosystemModelGrid.GlobalMissingValue, DataSetToViewLive, (int)currentTimeStep + 1);
                    }
                }
            }


        }

        /// <summary>
        /// Generates the console outputs for the current time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="timeStepTimer">The timer for the current time step</param>
        private void TimeStepConsoleOutputs(uint currentTimeStep, StopWatch timeStepTimer)
        {

        }

        /// <summary>
        /// Generate file outputs for the current time step
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions for cohorts in the model</param>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        /// <param name="cellIndices">The list of all cells to run the model for</param>
        /// <param name="cellIndex">The index of the current cell in the list of all cells to run the model for</param>
        private void TimeStepFileOutputs(ModelGrid ecosystemModelGrid, FunctionalGroupDefinitions cohortFunctionalGroupDefinitions,
            uint currentTimeStep, Boolean MarineCell, List<uint[]> cellIndices, int cellIndex)
        {
            Console.WriteLine("Writing grid cell ouputs to file...\n");
            //Write the low level outputs first
            DataConverter.ValueToSDS1D(TotalLivingBiomassDensity, "Total Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            DataConverter.ValueToSDS1D(TotalHeterotrophAbundanceDensity, "Heterotroph Abundance density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            DataConverter.ValueToSDS1D(TotalHeterotrophBiomassDensity, "Heterotroph Biomass density", "Time step",
                            ecosystemModelGrid.GlobalMissingValue,
                            BasicOutputMemory, (int)currentTimeStep + 1);
            // File outputs for medium and high detail levels
            if ((ModelOutputDetail == OutputDetailLevel.Medium) || (ModelOutputDetail == OutputDetailLevel.High))
            {
                if (MarineCell)
                {
                    // Loop over all cohort trait value combinations and output abundances, densities and biomasses
                    foreach (string FG in MarineFGsForTracking.Keys.ToArray())
                    {
                      //  DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }

                }
                else
                {
                    // Loop over all cohort trait value combinations and output abudnances, densities and biomasses
                    foreach (string FG in TerrestrialFGsForTracking.Keys.ToArray())
                    {
                       // DataConverter.ValueToSDS1D(TotalDensitiesOut[FG], FG + " density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                        DataConverter.ValueToSDS1D(TotalBiomassDensitiesOut[FG], FG + " biomass density", "Time step", ecosystemModelGrid.GlobalMissingValue, BasicOutputMemory, (int)currentTimeStep + 1);
                    }
                   
                }

                // If ouputting ecosystem metrics has been specified then add these metrics to the output
                if (OutputMetrics)
                {
                    DataConverter.ValueToSDS1D(Metrics.CalculateMeanTrophicLevelCell(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Mean Trophic Level", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "trophic index"),
                                                "Trophic Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalEvennessRao(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "biomass"),
                                                "Biomass Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    double[] FunctionalDiversity = Metrics.CalculateFunctionalDiversity(ecosystemModelGrid, cohortFunctionalGroupDefinitions,
                        cellIndices, cellIndex);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[0],
                                                 "Functional Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                 BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(FunctionalDiversity[1],
                                                "Rao Functional Evenness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[0],
                                                "Biomass Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[1],
                                                "Min Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Biomass")[2],
                                                "Max Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[0],
                                                "Trophic Richness", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[1],
                                                "Min Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateFunctionalRichness(ecosystemModelGrid, cohortFunctionalGroupDefinitions, cellIndices, cellIndex, "Trophic index")[2],
                                                "Max Trophic Index", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);

                    DataConverter.ValueToSDS1D(Metrics.CalculateArithmeticCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Arithmetic Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                    DataConverter.ValueToSDS1D(Metrics.CalculateGeometricCommunityMeanBodyMass(ecosystemModelGrid, cellIndices, cellIndex),
                                                "Geometric Mean Bodymass", "Time step", ecosystemModelGrid.GlobalMissingValue,
                                                BasicOutputMemory, (int)currentTimeStep + 1);
                }

                if (currentTimeStep % 600 == 0 && currentTimeStep > 0)
                {
                    BasicOutputMemory.Clone("msds:nc?file=" + _OutputPath + "BasicOutputs" + _OutputSuffix + ".nc&openMode=create");
                    Console.WriteLine("Cloning grid cell ouputs to file...\n");
                }



                // File outputs for high detail level
                if (ModelOutputDetail == OutputDetailLevel.High)
                {
                    ;
                }

            }




        }

        /// <summary>
        /// Write to the output file values of the output variables at the end of the model run
        /// </summary>
        /// <param name="EcosystemModelGrid">The model grid to get data from</param>
        /// <param name="CohortFunctionalGroupDefinitions">Definitions of the cohort functional groups in the model</param>
        /// <param name="StockFunctionalGroupDefinitions">Definitions of the stock functional groups in the model</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellNumber">The number of the current cell in the list of indices of active cells</param>
        /// <param name="GlobalDiagnosticVariables">List of global diagnostic variables</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="month">The current month in the model run</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        public void FinalOutputs(ModelGrid EcosystemModelGrid, FunctionalGroupDefinitions CohortFunctionalGroupDefinitions,
            FunctionalGroupDefinitions StockFunctionalGroupDefinitions, List<uint[]> cellIndices, int cellNumber,
            SortedList<string, double> GlobalDiagnosticVariables, MadingleyModelInitialisation initialisation, uint month, Boolean marineCell)
        {
            // Calculate output variables
            CalculateOutputs(EcosystemModelGrid, CohortFunctionalGroupDefinitions, StockFunctionalGroupDefinitions, cellIndices, cellNumber, GlobalDiagnosticVariables, initialisation, month, marineCell);

            // Dispose of the dataset objects  
            BasicOutputMemory.Clone("msds:nc?file=" + _OutputPath + "BasicOutputs" + _OutputSuffix + ".nc&openMode=create");
            BasicOutputMemory.Dispose();

            if (LiveOutputs)
            {
                DataSetToViewLive.Dispose();
            }

            if (ModelOutputDetail == OutputDetailLevel.High)
            {
                ;
            }

            Metrics.CloseRserve();

        }

        /// <summary>
        /// Calculates the abundances and biomasses within mass bins for all functional groups in the cohort indices array
        /// </summary>
        /// <param name="ecosystemModelGrid">The model grid</param>
        /// <param name="cellIndices">List of indices of active cells in the model grid</param>
        /// <param name="cellIndex">The number of the current cell in the list of indices of active cells</param>
        /// <param name="marineCell">Whether the current cell is a marine cell</param>
        private void CalculateMassBinOutputs(ModelGrid ecosystemModelGrid, List<uint[]> cellIndices, int cellIndex, Boolean marineCell)
        {
            ;
        }


    }
}
