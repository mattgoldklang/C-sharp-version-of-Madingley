using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Madingley
{
    public abstract class Tracker
    {
        abstract public void OpenTrackerFile();

        abstract public void WriteToTrackerFile(uint currentTimeStep, ModelGrid madingleyModelGrid, uint numLats, uint numLons,
            MadingleyModelInitialisation initialisation, Boolean MarineCell);

        abstract public void CloseTrackerFile();

        protected int NumberMarineFGsForTracking;
        protected int NumberTerrestrialFGsForTracking;

        protected Dictionary<string, int> MarineFGsForTracking;
        protected Dictionary<string, int> TerrestrialFGsForTracking;

        /// <summary>
        /// Determine the functional group name.
        /// </summary>
        /// <param name="cohortOrStockName">The name of the cohort to lookup the functional group for</param>
        /// <param name="Marine">Whether this is a marine cell</param>
        
        public int DetermineFunctionalGroup(string cohortOrStockName, Boolean Marine)
        {
            if(Marine)
            {
                // Return the FG index
                if(MarineFGsForTracking.ContainsKey(cohortOrStockName))
                {
                    return MarineFGsForTracking[cohortOrStockName];
                }
                else
                {
                    Debug.Fail("Error finding the name of a marine functional group in the tracker");
                    return -1;
                }
            }
            else
            {
                // Return the FG index
                if(TerrestrialFGsForTracking.ContainsKey(cohortOrStockName))
                {
                    return TerrestrialFGsForTracking[cohortOrStockName];
                }
                else
                {
                    Debug.Fail("Error finding the name of a terrestrial functional group in the tracker");
                    return -1;
                }
            }
        }

        /// <summary>
        /// Assign functional group numbers to functional groups (as specified by the tracker, not the model), and keep them in a
        /// dictionary indexed with a string generally corresponding to the FG name
        /// </summary>
        /// <param name="fGDefinitions">Cohort functional group definitions</param>
        /// <param name="stockDefinitions">Stock functional group definitions</param>
        public void AssignFunctionalGroups(FunctionalGroupDefinitions fGDefinitions, FunctionalGroupDefinitions stockDefinitions)
        {
            MarineFGsForTracking = new Dictionary<string, int>();
            TerrestrialFGsForTracking = new Dictionary<string, int>();

            NumberMarineFGsForTracking = 0;
            NumberTerrestrialFGsForTracking = 0;

            int TempMarine = 0;
            int TempTerrestrial = 0;

            // Assign cohort functional groups
            for(int i = 0; i < fGDefinitions.AllFunctionalGroupsIndex.Length; i++)
            {
                if(fGDefinitions.GetTraitNames("realm", i) == "marine")
                {
                    NumberMarineFGsForTracking++;
                    MarineFGsForTracking.Add(fGDefinitions.GetTraitNames("group description", i), TempMarine);
                    TempMarine++;
                }
                else
                {
                    NumberTerrestrialFGsForTracking++;
                    TerrestrialFGsForTracking.Add(fGDefinitions.GetTraitNames("group description", i), TempTerrestrial);
                    TempTerrestrial++;
                }
            }

            // Assign stock functional groups
            for(int i = 0; i < stockDefinitions.AllFunctionalGroupsIndex.Length; i++)
            {
                if(stockDefinitions.GetTraitNames("realm", i) == "marine")
                {
                    NumberMarineFGsForTracking++;
                    MarineFGsForTracking.Add(stockDefinitions.GetTraitNames("stock name", i), TempMarine);
                    TempMarine++;
                }
                else
                {
                    NumberTerrestrialFGsForTracking++;
                    TerrestrialFGsForTracking.Add(stockDefinitions.GetTraitNames("stock name", i), TempTerrestrial);
                    TempTerrestrial++;
                }
            }

            // Add a functional group for adding net autotrophic production as an input (for tracking purposes)
            NumberMarineFGsForTracking++;
            MarineFGsForTracking.Add("autotroph net production", TempMarine);
            TempMarine++;

            NumberTerrestrialFGsForTracking++;
            TerrestrialFGsForTracking.Add("autotroph net production", TempTerrestrial);
            TempTerrestrial++;
        }
    }
}
