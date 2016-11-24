using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Madingley
{
    public class FunctionalGroupTracker : Tracker
    {

        private StreamWriter FGFlowsWriter;

        private TextWriter SyncedFGFlowsWriter;
        
        private float[] Latitudes;
        private float[] Longitudes;

        private string OutputPath;
        private string FileName;
        private string FileSuffix;

        /// <summary>
        /// Array to hold flows of mass among functional groups. Order is:
        /// Lat, Lon, From (group), To (group)
        /// </summary>
        private double[,,,] FGMassFlows;

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
        public FunctionalGroupTracker(float[] latitudes, float[] longitudes, MadingleyModelInitialisation initialisation, FunctionalGroupDefinitions fGDefinitions, 
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
            FGMassFlows = new double[latitudes.Length, longitudes.Length, MaxNumberFunctionalGroups, MaxNumberFunctionalGroups];

        }

        /// <summary>
        /// Open the tracking file for writing to
        /// </summary>
        override public void OpenTrackerFile()
        {
            FGFlowsWriter = new StreamWriter(OutputPath + FileName + FileSuffix + ".txt");
            SyncedFGFlowsWriter = TextWriter.Synchronized(FGFlowsWriter);
            SyncedFGFlowsWriter.WriteLine("Latitude\tLongitude\ttime_step\tfromIndex\ttoIndex\tmass_eaten_g");

        }
             

        /// <summary>
        /// Record a flow of biomass between two functional groups (as specified by the tracker)
        /// </summary>
        /// <param name="latIndex"></param>
        /// <param name="lonIndex"></param>
        /// <param name="madingleyInitialisation"></param>
        /// <param name="predatorCohortOrStockName"></param>
        /// <param name="predatorBodyMass"></param>
        /// <param name="preyCohortOrStockName"></param>
        /// <param name="preyBodyMass"></param>
        /// <param name="massEaten"></param>
        /// <param name="marineCell"></param>
        public void RecordFGFlow(uint latIndex, uint lonIndex, MadingleyModelInitialisation madingleyInitialisation, 
            FunctionalGroupDefinitions stockFunctionalGroupDefinitions, string predatorCohortOrStockName, double predatorBodyMass, 
            string preyCohortOrStockName, double preyBodyMass, double massEaten, Boolean marineCell)
        {
            int fromIndex = 0;
            int toIndex = 0;

            // Get the functional group that the mass is flowing to
            toIndex = DetermineFunctionalGroup(madingleyInitialisation, stockFunctionalGroupDefinitions, predatorCohortOrStockName, predatorBodyMass, marineCell);

            // Get the functional group that the mass is flowing from
            fromIndex = DetermineFunctionalGroup(madingleyInitialisation, stockFunctionalGroupDefinitions, preyCohortOrStockName, preyBodyMass, marineCell);
            
            // Add the flow of matter to the matrix of functional group mass flows
            FGMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;
        }


        /// <summary>
        /// Write flows of matter among functional groups to the output file at the end of the time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="numLats">The latitudinal dimension of the model grid in number of cells</param>
        /// <param name="numLons">The longitudinal dimension of the model grid in number of cells</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        override public void WriteToTrackerFile(uint currentTimeStep, uint numLats, uint numLons, MadingleyModelInitialisation initialisation,
            Boolean MarineCell)
        {
            int NumFGs = FGMassFlows.GetLength(2);

            for (int lat = 0; lat < numLats; lat++)
            {
                for (int lon = 0; lon < numLons; lon++)
                {
                    for (int i = 0; i < NumFGs; i++)
                    {
                        for (int j = 0; j < NumFGs; j++)
                        {
                            if (FGMassFlows[lat, lon, i, j] > 0)
                            {
                                if (MarineCell)
                                SyncedFGFlowsWriter.WriteLine(Convert.ToString(lat) + '\t' + Convert.ToString(lon) + '\t' + Convert.ToString(currentTimeStep) +
                                    '\t' + MarineFGsForTracking.Keys.ToArray()[i] + '\t' + MarineFGsForTracking.Keys.ToArray()[j] + '\t' + Convert.ToString(FGMassFlows[lat, lon, i, j]));
                                else
                                    SyncedFGFlowsWriter.WriteLine(Convert.ToString(lat) + '\t' + Convert.ToString(lon) + '\t' + Convert.ToString(currentTimeStep) +
                                     '\t' + TerrestrialFGsForTracking.Keys.ToArray()[i] + '\t' + TerrestrialFGsForTracking.Keys.ToArray()[j] + '\t' + Convert.ToString(FGMassFlows[lat, lon, i, j]));

                            }
                        }
                    }
                }
            }

            // Reset array to hold mass flows among trophic levels
            FGMassFlows = new double[numLats, numLons, MaxNumberFunctionalGroups, MaxNumberFunctionalGroups];
        }


        /// <summary>
        /// Close the file that has been written to
        /// </summary>
        override public void CloseTrackerFile()
        {
            FGFlowsWriter.Dispose();
        }


    }
}
