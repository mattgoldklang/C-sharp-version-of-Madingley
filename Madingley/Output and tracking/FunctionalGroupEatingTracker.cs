using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// Tracks eating processes relating to the flow of mass between functional groups
    /// </summary>
    public class FunctionalGroupEatingTracker
    {

        string FGFlowsFilename;

        private StreamWriter FGFlowsWriter;

        private TextWriter SyncedFGFlowsWriter;

        /// <summary>
        /// Array to hold flows of mass among functional groups. Order is:
        /// Lat, Lon, From (group), To (group)
        /// </summary>
        private double[,,,] FGMassFlows;

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
        public FunctionalGroupEatingTracker(uint numLats, uint numLons, string fGFlowsFilename, string outputFilesSuffix, string outputPath,
            int cellIndex, MadingleyModelInitialisation initialisation, Boolean MarineCell, FunctionalGroupDefinitions fGDefinitions)
        {
            FGFlowsFilename = fGFlowsFilename;

            FGFlowsWriter = new StreamWriter(outputPath + FGFlowsFilename + outputFilesSuffix + "_Cell" + cellIndex + ".txt");
            SyncedFGFlowsWriter = TextWriter.Synchronized(FGFlowsWriter);
            SyncedFGFlowsWriter.WriteLine("Latitude\tLongitude\ttime_step\tfromIndex\ttoIndex\tmass_eaten_g");


            // Initialise array to hold mass flows among functional groups
            if (MarineCell)
            {
                FGMassFlows = new double[numLats, numLons, fGDefinitions.GetFunctionalGroupIndex("realm", "marine", true).Length, fGDefinitions.GetFunctionalGroupIndex("realm", "marine", true).Length];
                ;
            }
            else
            {
                FGMassFlows = new double[numLats, numLons, fGDefinitions.GetFunctionalGroupIndex("realm", "terrestrial  ", true).Length, fGDefinitions.GetFunctionalGroupIndex("realm", "terrestrial", true).Length];
            }
        }

        /// <summary>
        /// Record the flow of biomass between functional groups during predation
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="fromFunctionalGroup">The index of the functional group that the biomass is flowing from (i.e. the prey)</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the biomass is flowing to (i.e. the predator)</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The total biomass eaten by the predator cohort</param>
        /// <param name="predatorBodyMass">The body mass of the predator doing the eating</param>
        /// <param name="preyBodyMass">The body mass of the prey being eaten</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void RecordPredationFGFlow(uint latIndex, uint lonIndex, int fromFunctionalGroup, int toFunctionalGroup,
            FunctionalGroupDefinitions cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, double preyBodyMass,
            MadingleyModelInitialisation initialisation, Boolean MarineCell)
        {
            int fromIndex = 0;
            int toIndex = 0;
            if (initialisation.TrackMarineSpecifics && MarineCell)
            {
                // Get the trophic level index of the functional group that mass is flowing from
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", fromFunctionalGroup))
                {
                    case "herbivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                        {
                            case "ectotherm":
                                switch(cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        fromIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        fromIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        fromIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 4;
                                        else
                                            fromIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch(cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "allspecial":
                                        fromIndex = 7;
                                        break;
                                    default:
                                        fromIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;   
                        }
                        break;
                    case "omnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                        {
                            case "ectotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        fromIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        fromIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        fromIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 4;
                                        else
                                            fromIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "allspecial":
                                        fromIndex = 7;
                                        break;
                                    default:
                                        fromIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;
                        }
                        break;
                    case "carnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", fromFunctionalGroup))
                        {
                            case "ectotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        fromIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        fromIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        fromIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            fromIndex = 4;
                                        else
                                            fromIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", fromFunctionalGroup))
                                {
                                    case "allspecial":
                                        fromIndex = 7;
                                        break;
                                    default:
                                        fromIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Nutrition source not found in the functional group definition file");
                        break;
                }
                
                // Get the trophic level index of the functional group that mass is flowing to
                switch(cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                        {
                            case "ectotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        toIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        toIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        toIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 4;
                                        else
                                            toIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "allspecial":
                                        toIndex = 7;
                                        break;
                                    default:
                                        toIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;
                        }
                        break;
                    case "omnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                        {
                            case "ectotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        toIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        toIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        toIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 4;
                                        else
                                            toIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "allspecial":
                                        toIndex = 7;
                                        break;
                                    default:
                                        toIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;
                        }
                        break;
                    case "carnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                        {
                            case "ectotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "picophytoplankton":
                                        toIndex = 1;
                                        break;
                                    case "nanophytoplankton":
                                        toIndex = 2;
                                        break;
                                    case "microphytoplankton":
                                        toIndex = 3;
                                        break;
                                    default:
                                        if (preyBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 4;
                                        else
                                            toIndex = 5;
                                        break;
                                }
                                break;
                            case "endotherm":
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                {
                                    case "allspecial":
                                        toIndex = 7;
                                        break;
                                    default:
                                        toIndex = 6;
                                        break;
                                }
                                break;
                            default:
                                Debug.Fail("Thermic trait not found in the functional group definition file.");
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Nutrition source not found in the functional group definition file");
                        break;
                }
            }
            else
            {
                // Get the trophic level index of the functional group that mass is flowing from
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", fromFunctionalGroup))
                {
                    case "herbivore":
                        fromIndex = 1;
                        break;
                    case "omnivore":
                        fromIndex = 2;
                        break;
                    case "carnivore":
                        fromIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }

                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        toIndex = 1;
                        break;
                    case "omnivore":
                        toIndex = 2;
                        break;
                    case "carnivore":
                        toIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }

            // Add the flow of matter to the matrix of functional group mass flows
            FGMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;
        }

        /// <summary>
        /// Record the flow of biomass between functional groups during herbivory
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="toFunctionalGroup">The index of the functional group that the biomass is flowing to (i.e. the herbivore)</param>
        /// <param name="cohortFunctionalGroupDefinitions">The functional group definitions of cohorts in the model</param>
        /// <param name="massEaten">The total biomass eaten by the herbivore cohort</param>
        /// <param name="predatorBodyMass">The mass of the predator doing the eating</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void RecordHerbivoryTrophicFlow(uint latIndex, uint lonIndex, int toFunctionalGroup, FunctionalGroupDefinitions
            cohortFunctionalGroupDefinitions, double massEaten, double predatorBodyMass, MadingleyModelInitialisation initialisation,
            Boolean MarineCell )
        {
            // For herbivory the trophic level index that mass flows from is 0
            int fromIndex = 0;
            // Get the trophic level index of the functional group that mass is flowing to
            int toIndex = 0;

            if (initialisation.TrackMarineSpecifics && MarineCell)
            {
                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 1;
                                                break;
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 1;
                                        break;
                                }
                                break;
                        }
                        break;
                    case "omnivore":
                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("mobility", toFunctionalGroup))
                        {
                            case "planktonic":
                                toIndex = 4;
                                break;
                            default:
                                switch (cohortFunctionalGroupDefinitions.GetTraitNames("endo/ectotherm", toFunctionalGroup))
                                {
                                    case "endotherm":
                                        switch (cohortFunctionalGroupDefinitions.GetTraitNames("diet", toFunctionalGroup))
                                        {
                                            case "allspecial":
                                                toIndex = 6;
                                                break;
                                            default:
                                                toIndex = 2;
                                                break;
                                        }
                                        break;
                                    default:
                                        if (predatorBodyMass <= initialisation.PlanktonDispersalThreshold)
                                            toIndex = 5;
                                        else
                                            toIndex = 2;
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }
            else
            {
                // Get the trophic level index of the functional group that mass is flowing to
                switch (cohortFunctionalGroupDefinitions.GetTraitNames("nutrition source", toFunctionalGroup))
                {
                    case "herbivore":
                        toIndex = 1;
                        break;
                    case "omnivore":
                        toIndex = 2;
                        break;
                    case "carnivore":
                        toIndex = 3;
                        break;
                    default:
                        Debug.Fail("Specified nutrition source is not supported");
                        break;
                }
            }

            // Add the flow of matter to the matrix of mass flows
            FGMassFlows[latIndex, lonIndex, fromIndex, toIndex] += massEaten;
        }

        /// <summary>
        /// Record the flow of biomass into the autotroph trophic level as a result of primary production
        /// </summary>
        /// <param name="latIndex">The latitudinal index of the current grid cell</param>
        /// <param name="lonIndex">The longitudinal index of the current grid cell</param>
        /// <param name="massEaten">The total biomass gained by the autotroph stock</param>
        public void RecordPrimaryProductionTrophicFlow(uint latIndex, uint lonIndex, double massEaten)
        {
            // Add the flow of matter to the matrix of mass flows
            FGMassFlows[latIndex, lonIndex, 0, 0] += massEaten;
        }

        /// <summary>
        /// Write flows of matter among trophic levels to the output file at the end of the time step
        /// </summary>
        /// <param name="currentTimeStep">The current time step</param>
        /// <param name="numLats">The latitudinal dimension of the model grid in number of cells</param>
        /// <param name="numLons">The longitudinal dimension of the model grid in number of cells</param>
        /// <param name="initialisation">The Madingley Model initialisation</param>
        /// <param name="MarineCell">Whether the current cell is a marine cell</param>
        public void WriteTrophicFlows(uint currentTimeStep, uint numLats, uint numLons, MadingleyModelInitialisation initialisation,
            Boolean MarineCell)
        {
            for (int lat = 0; lat < numLats; lat++)
            {
                for (int lon = 0; lon < numLons; lon++)
                {
                    for (int i = 0; i < FGMassFlows.GetLength(2); i++)
                    {
                        for (int j = 0; j < FGMassFlows.GetLength(3); j++)
                        {
                            if (FGMassFlows[lat, lon, i, j] > 0)
                            {
                                SyncedFGFlowsWriter.WriteLine(Convert.ToString(lat) + '\t' + Convert.ToString(lon) + '\t' + Convert.ToString(currentTimeStep) +
                                    '\t' + Convert.ToString(i) + '\t' + Convert.ToString(j) + '\t' + Convert.ToString(FGMassFlows[lat, lon, i, j]));
                            }
                        }
                    }
                }
            }

            // Initialise array to hold mass flows among trophic levels
            if (initialisation.TrackMarineSpecifics && MarineCell)
                FGMassFlows = new double[numLats, numLons, 7, 7];
            else
                FGMassFlows = new double[numLats, numLons, 4, 4];



        }

        /// <summary>
        /// Close the streams for writing eating data
        /// </summary>
        public void CloseStreams()
        {
            FGFlowsWriter.Dispose();
        }
    }
}
