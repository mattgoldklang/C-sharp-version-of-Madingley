using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Madingley
{
    /// <summary>
    /// Merges cohorts with similar properties
    /// </summary>
    public class CohortMerge
    {
        /// <summary>
        /// An instance of the simple random number generator
        /// </summary>
        private NonStaticSimpleRNG RandomNumberGenerator = new NonStaticSimpleRNG();

        /// <summary>
        /// Constructor for CohortMerge: sets the seed for the random number generator
        /// </summary>
        /// <param name="DrawRandomly"></param>
        public CohortMerge(Boolean DrawRandomly)
        {
            // Seed the random number generator
            // Set the seed for the random number generator
            RandomNumberGenerator = new NonStaticSimpleRNG();
            if (DrawRandomly)
            {
                RandomNumberGenerator.SetSeedFromSystemTime();
            }
            else
            {
                RandomNumberGenerator.SetSeed(4000);
            }
        }

        /// <summary>
        /// Calculate the distance between two cohorts in multi-dimensional trait space (body mass, adult mass, juvenile mass)
        /// </summary>
        /// <param name="Cohort1">The first cohort to calculate distance to</param>
        /// <param name="Cohort2">The cohort to compare to</param>
        /// <returns>The relative distance in trait space</returns>
        public double CalculateDistance(Cohort Cohort1, Cohort Cohort2)
        {
            double AdultMassDistance = Math.Abs(Cohort1.AdultMass - Cohort2.AdultMass)/Cohort1.AdultMass;
            double JuvenileMassDistance = Math.Abs(Cohort1.JuvenileMass - Cohort2.JuvenileMass)/Cohort1.JuvenileMass;
            double CurrentMassDistance = Math.Abs(Cohort1.IndividualBodyMass - Cohort2.IndividualBodyMass)/Cohort1.IndividualBodyMass;

            return Math.Sqrt((AdultMassDistance * AdultMassDistance) + (JuvenileMassDistance * JuvenileMassDistance) +
                (CurrentMassDistance * CurrentMassDistance));

        }

        /// <summary>
        /// Merge cohorts until below a specified threshold number of cohorts in each grid cell
        /// </summary>
        public int MergeToReachThresholdFast(GridCellCohortHandler gridCellCohorts, int[] TotalNumberOfCohortsPerFG, int[] TargetCohortThresholdPerFG)
        {
            // How many cohorts to remove to reach the threshold
            int[] NumberToRemovePerFG = new int[gridCellCohorts.Count];

            // Work out number to remove per functional group
            for(int i = 0; i < gridCellCohorts.Count; i++)
            {
                // todo(erik): the conversion to int should be handled when imported from definition file
                NumberToRemovePerFG[i] = TotalNumberOfCohortsPerFG[i] - TargetCohortThresholdPerFG[i];
            }

            // Holds the pairwise distances between two cohorts; the cohort IDs of each cohort
            Tuple<double, int[]> PairwiseDistances;

            // Create a list of unique combinations of cohort IDs to compare in each functional group
            List<Tuple<int, int>>[] CohortsToCompareShort = new List<Tuple<int, int>>[gridCellCohorts.Count];

            for(int i = 0; i < gridCellCohorts.Count; i++)
            {
                int[] cohortIDs1 = Enumerable.Range(0, (gridCellCohorts[i].Count - 1) + 1).ToArray();

                var query1 = from item1 in cohortIDs1
                             from item2 in cohortIDs1
                             where item1 < item2
                             select new Tuple<int, int>(item1, item2);

                CohortsToCompareShort[i] = new List<Tuple<int, int>>();
                
                foreach (Tuple<int, int> tuple in query1)
                {
                    CohortsToCompareShort[i].Add(tuple);
                }
            }

            // Calculate distances between all unique cohorts in each functional group
            List<Tuple<double, int[]>>[] DistancesPerFunctionalGroup = new List<Tuple<double, int[]>>[gridCellCohorts.Count];

            for(int i = 0; i < gridCellCohorts.Count; i++)
            {
                // Distances between two cohorts; the functional group of the cohort; the cohort IDs of each cohort
                DistancesPerFunctionalGroup[i] = new List<Tuple<double, int[]>>();

                // Loop through cohorts within functional groups
                for(var j = 0; j < CohortsToCompareShort[i].Count; j++)
                {
                    // Choose which cohort to merge to and from at random
                    if(RandomNumberGenerator.GetUniform() < 0.5)
                    {
                        DistancesPerFunctionalGroup[i].Add(new Tuple<double, int[]>(CalculateDistance(gridCellCohorts[i][CohortsToCompareShort[i][j].Item1],
                            gridCellCohorts[i][CohortsToCompareShort[i][j].Item2]), new int[] { CohortsToCompareShort[i][j].Item1, CohortsToCompareShort[i][j].Item2 }));
                    }
                    else
                    {
                        DistancesPerFunctionalGroup[i].Add(new Tuple<double, int[]>(CalculateDistance(gridCellCohorts[i][CohortsToCompareShort[i][j].Item2],
                            gridCellCohorts[i][CohortsToCompareShort[i][j].Item1]), new int[] { CohortsToCompareShort[i][j].Item1, CohortsToCompareShort[i][j].Item1 }));
                    }
                }
                DistancesPerFunctionalGroup[i] = DistancesPerFunctionalGroup[i].OrderBy(x => x.Item1).ToList();
            }

            // Count the number of merges that have happened for each FG

            int MergeCounter = 0;

            int TotalNumberOfMerges = 0;

            int CurrentListPosition = 0;

            // Perform merge for each functional group
            for(int i = 0; i < DistancesPerFunctionalGroup.Length; i++)
            {
                // Reset merge and current list position counters
                MergeCounter = 0;
                CurrentListPosition = 0;

                while (MergeCounter < NumberToRemovePerFG[i])
                {
                    // Get potential cohort to merge
                    int CohortToMergeFrom = DistancesPerFunctionalGroup[i][CurrentListPosition].Item2[1];
                    int CohortToMergeTo = DistancesPerFunctionalGroup[i][CurrentListPosition].Item2[0];

                    // Only merge if these cohorts have not previously merged this time step
                    if((gridCellCohorts[i][CohortToMergeTo].MergedThisTimeStep == true) || (gridCellCohorts[i][CohortToMergeFrom].MergedThisTimeStep == true))
                    {
                        ;
                    }
                    else
                    {
                        // Add the abundance of the second cohort to that of the first
                        gridCellCohorts[i][CohortToMergeTo].CohortAbundance += (gridCellCohorts[i][CohortToMergeFrom].CohortAbundance
                            * gridCellCohorts[i][CohortToMergeFrom].IndividualBodyMass) / gridCellCohorts[i][CohortToMergeTo].IndividualBodyMass;

                        // Add the reproductive potential mass of the second cohort to that of the first
                        gridCellCohorts[i][CohortToMergeTo].IndividualReproductivePotentialMass += (gridCellCohorts[i][CohortToMergeFrom].IndividualReproductivePotentialMass
                            * gridCellCohorts[i][CohortToMergeFrom].CohortAbundance) / gridCellCohorts[i][CohortToMergeTo].CohortAbundance;

                        // Set the abundance of the second cohort to zero
                        gridCellCohorts[i][CohortToMergeFrom].CohortAbundance = 0.0;

                        // Designate both cohorts as having merged
                        gridCellCohorts[i][CohortToMergeTo].Merged = true;
                        gridCellCohorts[i][CohortToMergeFrom].Merged = true;

                        // Designate both cohorts as having merged this time step
                        gridCellCohorts[i][CohortToMergeTo].MergedThisTimeStep = true;
                        gridCellCohorts[i][CohortToMergeFrom].MergedThisTimeStep = true;

                        // Increase merge counter
                        MergeCounter++;
                    }

                    // Increase current list position counter
                    CurrentListPosition++;

                    if(CurrentListPosition == DistancesPerFunctionalGroup[i].Count)
                    {
                        Console.WriteLine("Merged all possible cohorts.");
                        break;
                    }
                }

                // Reset merged this time step identifier
                for(int j = 0; j < gridCellCohorts[i].Count; j++)
                {
                    gridCellCohorts[i][j].MergedThisTimeStep = false;
                }

                TotalNumberOfMerges += MergeCounter;
            }
            return TotalNumberOfMerges;
        }



        /// <summary>
        /// Merge cohorts for responsive dispersal only; merges identical cohorts, no matter how many times they have been merged before
        /// </summary>
        /// <param name="gridCellCohorts">The cohorts in the current grid cell</param>
        /// <returns>Number of cohorts merged</returns>
        public int MergeForResponsiveDispersalOnly(GridCellCohortHandler gridCellCohorts)
        {
            // Variable to track the total number of cohorts merged
            int NumberCombined = 0;

            //Loop over all functional groups
            for (int i = 0; i < gridCellCohorts.Count; i++)
            {
                // Loop over each cohort in each functional group
                for (int j = 0; j < gridCellCohorts[i].Count; j++)
                {
                    // If that cohort has abundance greater than zero  then check if there are similar cohorts that could be merged with it
                    if (gridCellCohorts[i][j].CohortAbundance > 0)
                    {
                        // Loop over all cohorts above the jth in the cohort list
                        for (int k = j + 1; k < gridCellCohorts[i].Count; k++)
                        {
                            // Check that kth cohort has abunance and that the two cohorts being compared do not represent a juvenile adult pairing
                            if (gridCellCohorts[i][k].CohortAbundance > 0 &&
                                ((gridCellCohorts[i][j].MaturityTimeStep == uint.MaxValue && gridCellCohorts[i][k].MaturityTimeStep == uint.MaxValue) ||
                                 (gridCellCohorts[i][j].MaturityTimeStep < uint.MaxValue && gridCellCohorts[i][k].MaturityTimeStep < uint.MaxValue)))
                            {
                                //Check that the individual masses are widentical
                                if (gridCellCohorts[i][j].IndividualBodyMass == gridCellCohorts[i][k].IndividualBodyMass)
                                {
                                    //Check that the adult masses are similar
                                    if (gridCellCohorts[i][j].AdultMass == gridCellCohorts[i][k].AdultMass)
                                    {
                                        //Check that the juvenile masses are similar
                                        if (gridCellCohorts[i][j].JuvenileMass == gridCellCohorts[i][k].JuvenileMass)
                                        {
                                            //Check that the Maximum achieved mass is similar
                                            if (gridCellCohorts[i][j].MaximumAchievedBodyMass == gridCellCohorts[i][k].MaximumAchievedBodyMass)
                                            {
                                                // In half of cases, add the abundance of the second cohort to that of the first and maintain the properties of the first
                                                if (RandomNumberGenerator.GetUniform() < 0.5)
                                                {
                                                    // Add the abundance of the second cohort to that of the first
                                                    gridCellCohorts[i][j].CohortAbundance += (gridCellCohorts[i][k].CohortAbundance * gridCellCohorts[i][k].IndividualBodyMass) / gridCellCohorts[i][j].IndividualBodyMass;
                                                    // Set the abundance of the second cohort to zero
                                                    gridCellCohorts[i][k].CohortAbundance = 0.0;
                                                    // Add the reproductive potential mass of the second cohort to that of the first
                                                    gridCellCohorts[i][j].IndividualReproductivePotentialMass += (gridCellCohorts[i][k].IndividualReproductivePotentialMass * gridCellCohorts[i][k].CohortAbundance) / gridCellCohorts[i][j].CohortAbundance;
                                                    // Designate both cohorts as having merged
                                                    gridCellCohorts[i][j].Merged = true;
                                                    gridCellCohorts[i][k].Merged = true;
                                                }
                                                // In all other cases, add the abundance of the first cohort to that of the second and maintain the properties of the second
                                                else
                                                {
                                                    // Add the abundance of the first cohort to that of the second
                                                    gridCellCohorts[i][k].CohortAbundance += (gridCellCohorts[i][j].CohortAbundance * gridCellCohorts[i][j].IndividualBodyMass) / gridCellCohorts[i][k].IndividualBodyMass;
                                                    // Set the abundance of the second cohort to zero
                                                    gridCellCohorts[i][j].CohortAbundance = 0.0;
                                                    // Add the reproductive potential mass of the second cohort to that of the first
                                                    gridCellCohorts[i][k].IndividualReproductivePotentialMass += (gridCellCohorts[i][j].IndividualReproductivePotentialMass * gridCellCohorts[i][j].CohortAbundance) / gridCellCohorts[i][k].CohortAbundance;
                                                    // Designate both cohorts as having merged
                                                    gridCellCohorts[i][j].Merged = true;
                                                    gridCellCohorts[i][k].Merged = true;
                                                }
                                                // Increment the number of cohorts combined
                                                NumberCombined += 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return NumberCombined;

        }

    }
}
