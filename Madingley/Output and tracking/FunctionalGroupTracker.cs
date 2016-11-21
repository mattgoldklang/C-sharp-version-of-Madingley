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

        private int NumMarineFGsIncStocks;
        private int NumTerrestrialFGsIncStocks;
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

            NumMarineFGsIncStocks = fGDefinitions.GetFunctionalGroupIndex("realm", "marine", true).Length + stockFGDefinitions.GetFunctionalGroupIndex("realm", "marine", true).Length;
            NumTerrestrialFGsIncStocks = fGDefinitions.GetFunctionalGroupIndex("realm", "terrestrial", true).Length + stockFGDefinitions.GetFunctionalGroupIndex("realm", "terrestrial", true).Length;

            // Initialise array to hold mass flows among functional groups
            MaxNumberFunctionalGroups = Math.Max(NumMarineFGsIncStocks, NumTerrestrialFGsIncStocks);
            FGMassFlows = new double[latitudes.Length, longitudes.Length, MaxNumberFunctionalGroups, MaxNumberFunctionalGroups];

        }

        /// <summary>
        /// Open the tracking file for writing to
        /// </summary>
        /// <param name="outputPath">The output directory</param>
        /// <param name="outputFilesSuffix"></param>
        override public void OpenTrackerFile()
        {
            FGFlowsWriter = new StreamWriter(OutputPath + FileName + FileSuffix + ".txt");
            SyncedFGFlowsWriter = TextWriter.Synchronized(FGFlowsWriter);
            SyncedFGFlowsWriter.WriteLine("Latitude\tLongitude\ttime_step\tfromIndex\ttoIndex\tmass_eaten_g");

        }

        override public void WriteToTrackerFile()
        { }

        override public void CloseTrackerFile()
        {
            FGFlowsWriter.Dispose();
        }


    }
}
