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
    }
}
