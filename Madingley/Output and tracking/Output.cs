using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Madingley
{
    public abstract class Output
    {
        protected int NumberMarineFGsForTracking;
        protected int NumberTerrestrialFGsForTracking;

        protected Dictionary<string, int> MarineFGsForTracking;
        protected Dictionary<string, int> TerrestrialFGsForTracking;

        /// <summary>
        /// Determine the functional group name. Also splits marine functional groups into planktonic and non-planktonic based on size.
        /// </summary>
        /// <param name="madingleyInitialisation">Madingley model initialisation file</param>
        /// <param name="cohortOrStockName">The name of the cohort that you want to find out the functional group for</param>
        /// <param name="cohortOrStockBodyMass">The body mass of the cohrt</param>
        /// <param name="Marine">Whether this is a marine cell</param>
        /// <returns></returns>
        public int DetermineFunctionalGroup(MadingleyModelInitialisation madingleyInitialisation, FunctionalGroupDefinitions stockFunctionalGroupDefinitions, string cohortOrStockName, double cohortOrStockBodyMass, Boolean Marine)
        {
            // In the marine environment, all non-obligate zooplankton are put in the meroplankton group
            if (Marine)
            {
                // Put plankton in the right FG
                if (cohortOrStockBodyMass < madingleyInitialisation.PlanktonDispersalThreshold)
                {
                    if (cohortOrStockName == "obligate unicellular zooplankton")
                    {
                        // If unicellular and below size threshold then nanoplankton
                        if (cohortOrStockBodyMass < 5.00E-08)
                        {
                            cohortOrStockName = "nanozooplankton";
                        }
                        else
                        {
                            cohortOrStockName = "microzooplankton";
                        }
                    }
                    else
                    {
                        if (cohortOrStockName == "obligate multicellular zooplankton")
                        {
                            ;
                        }
                        else
                        {
                            if (!stockFunctionalGroupDefinitions.GetUniqueTraitValues("stock name").Contains(cohortOrStockName))
                                cohortOrStockName = "meroplankton";
                        }
                    }

                }

                // Return the FG index
                if (MarineFGsForTracking.ContainsKey(cohortOrStockName))
                    return MarineFGsForTracking[cohortOrStockName];
                else
                {
                    Debug.Fail("Error finding the name of a marine functional group in the tracker");
                    return -1;
                }
            }
            else
            {
                // Return the FG index
                if (TerrestrialFGsForTracking.ContainsKey(cohortOrStockName))
                    return TerrestrialFGsForTracking[cohortOrStockName];
                else
                {
                    Debug.Fail("Error finding the name of a terrestrial functional group in the tracker");
                    return -1;
                }
            }
        }

        /// <summary>
        /// Assign functional group numbers to functional groups (as specified by the tracker, not the model), and keep them in a dictionary indexed with a string
        /// generally corresponding to the FG name
        /// </summary>
        /// <param name="madingleyInitialisation"></param>
        /// <param name="fGDefinitions"></param>
        /// <param name="stockDefinitions"></param>
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

                    // We create two obligate unicellular zooplankton groups; microzooplankton (here) and nanozooplankton (later)
                    if (fGDefinitions.GetTraitNames("group description", ii) == "obligate unicellular zooplankton")
                        MarineFGsForTracking.Add("microzooplankton", TempMarine);
                    else
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

            // Now add in an FG for adding net autotroph production as an input, again for tracking purposes only
       //     NumberMarineFGsForTracking++;
         //   MarineFGsForTracking.Add("autotroph net production", TempMarine);
           // TempMarine++;
          //  NumberTerrestrialFGsForTracking++;
            //TerrestrialFGsForTracking.Add("autotroph net production", TempTerrestrial);
            //TempTerrestrial++;
        }
    }
}
