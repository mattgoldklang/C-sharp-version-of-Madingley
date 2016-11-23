using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    public abstract class Tracker
    {
        abstract public void OpenTrackerFile();

        abstract public void WriteToTrackerFile();

        abstract public void CloseTrackerFile();

        protected int NumberMarineFGsForTracking;
        protected int NumberTerrestrialFGsForTracking;

        protected Dictionary<string, int> MarineFGsForTracking;
        protected Dictionary<string, int> TerrestrialFGsForTracking;

        /// <summary>
        /// Determine the functional group name. Also splits marine functional groups into planktonic and non-planktonic based on size.
        /// </summary>
        /// <param name="madingleyInitialisation">Madingley model initialisation file</param>
        /// <param name="cohortToClassify">The cohort that you want to find out the functional group for</param>
        /// <param name="fGDefinitions">Definitions of all functional groups</param>
        /// <param name="Marine">Whether this is a marine cell</param>
        /// <returns></returns>
        public string DetermineFunctionalGroup(MadingleyModelInitialisation madingleyInitialisation, Cohort cohortToClassify, FunctionalGroupDefinitions fGDefinitions, Boolean Marine)
        {
            string TempString = fGDefinitions.GetTraitNames("group description", cohortToClassify.FunctionalGroupIndex);

            if (Marine)
            {
                if (cohortToClassify.IndividualBodyMass < madingleyInitialisation.PlanktonDispersalThreshold)
                    TempString = TempString + " planktonic";
            }

            return (TempString);

        }

        public void AssignFunctionalGroups(MadingleyModelInitialisation madingleyInitialisation, FunctionalGroupDefinitions fGDefinitions, FunctionalGroupDefinitions stockDefinitions)
        {
            MarineFGsForTracking = new Dictionary<string, int>();
            TerrestrialFGsForTracking = new Dictionary<string, int>();

            NumberMarineFGsForTracking = 0;
            NumberTerrestrialFGsForTracking = 0;

            int TempMarine = 0;
            int TempTerrestrial = 0;

            for (int ii = 0; ii < fGDefinitions.AllFunctionalGroupsIndex.Length; ii++)
            {
                if (fGDefinitions.GetTraitNames("realm", ii) == "marine")
                {
                    NumberMarineFGsForTracking++;
                    MarineFGsForTracking.Add(fGDefinitions.GetTraitNames("group description", ii), TempMarine);
                    TempMarine++;
                }
                else
                {
                    NumberTerrestrialFGsForTracking++;
                    TerrestrialFGsForTracking.Add(fGDefinitions.GetTraitNames("group description", ii), TempTerrestrial);
                    TempTerrestrial++;
                }
            }
            // Add in two extra marine FGs: one for meroplankton, and another for separating the nanozooplankton and the microzooplankton, which are one FG in the model input file
            // These aren't strictly FGs as defined by the model, but are separated for output purposes
            MarineFGsForTracking.Add("meroplankton", TempMarine);
            TempMarine++;
            MarineFGsForTracking.Add("nanozooplankton", TempMarine);
            TempMarine++;
            NumberMarineFGsForTracking = NumberMarineFGsForTracking + 2;

            // Now add the stocks

            for (int ii = 0; ii < stockDefinitions.AllFunctionalGroupsIndex.Length; ii++)
            {
                if (stockDefinitions.GetTraitNames("realm", ii) == "marine")
                {
                    NumberMarineFGsForTracking++;
                    MarineFGsForTracking.Add(stockDefinitions.GetTraitNames("stock name", ii), TempMarine);
                    TempMarine++;
                }
                else
                {
                    NumberTerrestrialFGsForTracking++;
                    TerrestrialFGsForTracking.Add(stockDefinitions.GetTraitNames("stock name", ii), TempTerrestrial);
                    TempTerrestrial++;
                }
            }
        }
    }
}
