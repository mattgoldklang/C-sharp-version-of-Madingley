using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        /// Array to hold flows of mass among functional groups.
        /// Order is: Lat, Lon, From (group), To (group)
        /// </summary>
        /// 
        private double[,,,] FGMassFlows;

        private int MaxNumberFunctionalGroups;

        /// <summary>
        /// Set up tracker for outputting properties of the eating process between functional groups
        /// </summary>
        /// <param name="latitudes">Array of latitudes</param>
        /// <param name="longitudes">Array of longitudes</param>
        /// <param name="fGDefinitions">Cohort functional group definitions</param>
        /// <param name="stockDefinitions">Stock function group definitions</param>
        /// <param name="outputFileSuffix">Output file suffix</param>
        /// <param name="outputPath">Output directory path</param>
        /// <param name="fileName">Output file name</param>
        public FunctionalGroupTracker(float[] latitudes, float[] longitudes, FunctionalGroupDefinitions fGDefinitions, FunctionalGroupDefinitions stockDefinitions, string outputFileSuffix,
            string outputPath, string fileName)
        {
            Latitudes = latitudes;
            Longitudes = longitudes;

            FileName = fileName;
            OutputPath = outputPath;
            FileSuffix = outputFileSuffix;

            // Assign a number and name to each functional group (FG as defined by output, not by model)
            AssignFunctionalGroups(fGDefinitions, stockDefinitions);

            // Initialise array to hold mass flows among functional groups
            MaxNumberFunctionalGroups = Math.Max(NumberMarineFGsForTracking, NumberTerrestrialFGsForTracking);
            FGMassFlows = new double[latitudes.Length, longitudes.Length, MaxNumberFunctionalGroups, MaxNumberFunctionalGroups];
        }

        /// <summary>
        /// Open the tracking file for writing
        /// </summary>
        /// 
        public override void OpenTrackerFile()
        {
            FGFlowsWriter = new StreamWriter(OutputPath + FileName + FileSuffix + ".txt");
            SyncedFGFlowsWriter = TextWriter.Synchronized(FGFlowsWriter);
            SyncedFGFlowsWriter.WriteLine("tLatitude\tLongitude\tTime_step\tFromIndex\tToIndex\tMass_eaten_g");
        }

        /// <summary>
        /// Record a flow of biomass between two functional groups (as specified by the the tracker)
        /// </summary>
        /// <param name="latIndex"></param>
        /// <param name="lonIndex"></param>
        /// <param name="predatorCohortOrStockName">Predator cohort or stock name</param>
        /// <param name="preyCohortOrStockName">Prey cohort or stock name</param>
        /// <param name="massEaten">Biomass eaten which flows from one functional group to another</param>
        /// <param name="marineCell">Whether this is a marine cell</param>
        public void RecordFGFlow(uint latIndex, uint lonIndex, string predatorCohortOrStockName, string preyCohortOrStockName, 
            double massEaten, Boolean marineCell)
        {
            int fromIndex = 0;
            int toIndex = 0;

            // Get the functional group that the mass is flowing to
            toIndex = DetermineFunctionalGroup(predatorCohortOrStockName, marineCell);

            // Get the functional group that the mass is flowing from
            fromIndex = DetermineFunctionalGroup(preyCohortOrStockName, marineCell);

            // Add the flow of matter to the matrix of functional group mass flows
            FGMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;
        }

        /// <summary>
        /// Write flows of matter among functional groups to the output file at the end of the time step
        /// </summary>
        /// 
        public override void WriteToTrackerFile(uint currentTimeStep, ModelGrid madingleyModelGrid, uint numLats, uint numLons, MadingleyModelInitialisation initialisation, bool MarineCell)
        {
            int NumFGs = FGMassFlows.GetLength(2);

            for(uint lat = 0; lat < numLats; lat++)
            {
                for(uint lon = 0; lon < numLons; lon++)
                {
                    for(int i = 0; i < NumFGs; i++)
                    {
                        for(int j = 0; j < NumFGs; j++)
                        {
                            if (FGMassFlows[lat, lon, i, j] > 0)
                            {
                                if (MarineCell)
                                {
                                    SyncedFGFlowsWriter.WriteLine(Convert.ToString(madingleyModelGrid.GetCellLatitude(lat)) + '\t' + 
                                        Convert.ToString(madingleyModelGrid.GetCellLongitude(lon)) + '\t' + Convert.ToString(currentTimeStep) + 
                                        '\t' + MarineFGsForTracking.Keys.ToArray()[i] + '\t' + MarineFGsForTracking.Keys.ToArray()[j] + '\t' + 
                                        Convert.ToString(FGMassFlows[lat, lon, i, j]));
                                }
                                else
                                {
                                    SyncedFGFlowsWriter.WriteLine(Convert.ToString(madingleyModelGrid.GetCellLatitude(lat)) + '\t' + 
                                        Convert.ToString(madingleyModelGrid.GetCellLongitude(lon)) + '\t' + Convert.ToString(currentTimeStep) + 
                                        '\t' + TerrestrialFGsForTracking.Keys.ToArray()[i] + '\t' + TerrestrialFGsForTracking.Keys.ToArray()[j] + 
                                        '\t' + Convert.ToString(FGMassFlows[lat, lon, i, j]));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Close the file that has been written to
        /// </summary>
        /// 
        public override void CloseTrackerFile()
        {
            FGFlowsWriter.Dispose();
        }
    }
}
