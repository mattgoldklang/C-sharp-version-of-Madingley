using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Diagnostics;

namespace Madingley
{
    /// <summary>
    /// A revised version of the herbivory process, written November 2011
    /// </summary>
    public partial class RevisedHerbivory: IEatingImplementation
    {
        /// <summary>
        /// The time unit associated with this herbivory implementation and its parameters
        /// </summary>
        private string _TimeUnitImplementation;
        /// <summary>
        /// Get the time unit associated with this herbivory implementation and its parameters
        /// </summary>
        public string TimeUnitImplementation { get { return _TimeUnitImplementation; } }

        /// <summary>
        /// The assimilation efficiency of eaten autotroph mass into herbivore body mass
        /// </summary>
        private double _AssimilationEfficiency;
        /// <summary>
        /// Get and set the assimilation efficiency of eaten autotroph mass into herbivore body mass
        /// </summary>
        public double AssimilationEfficiency
        {
            get { return _AssimilationEfficiency; }
            set { _AssimilationEfficiency = value; }
        }

        /// <summary>
        /// The scalar of the relationship between handling time and the function of herbivore mass for the terrestrial realm
        /// </summary>
        private double _HandlingTimeScalarTerrestrial;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of herbivore mass for the terrestrial realm
        /// </summary>
        public double HandlingTimeScalarTerrestrial
        { get { return _HandlingTimeScalarTerrestrial; } }

        /// <summary>
        /// The scalar of the relationship between handling time and the function of herbivore mass for the marine realm
        /// </summary>
        private double _HandlingTimeScalarMarine;
        /// <summary>
        /// Get the scalar of the relationship between handling time and the function of herbivore mass for the marine realm
        /// </summary>
        public  double HandlingTimeScalarMarine
        { get { return _HandlingTimeScalarMarine; } }
        

        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the terrestrial realm
        /// </summary>
        private double _HandlingTimeExponentTerrestrial;
        /// <summary>
        /// Get the exponent applied to herbivore mass in the handling time relationship for the terrestrial realm
        /// </summary>
        public double HandlingTimeExponentTerrestrial
        { get { return _HandlingTimeExponentTerrestrial; } }

        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the marine realm
        /// </summary>
        private double _HandlingTimeExponentMarine;
        /// <summary>
        /// The exponent applied to herbivore mass in the handling time relationship for the marine realm
        /// </summary>
        public double HandlingTimeExponentMarine
        { get { return _HandlingTimeExponentMarine; } }
        

        /// <summary>
        /// Reference mass of plant matter for calculating handling times
        /// </summary>
        public double _ReferenceMass;
        /// <summary>
        /// Get the reference mass of plant matter for calculating handling times
        /// </summary>
        public double ReferenceMass {  get { return _ReferenceMass; } }
        
        /// <summary>
        /// The maximum herbivory rate for a herbivore of 1 g
        /// </summary>
        private double _HerbivoryRateConstant;
        /// <summary>
        /// Get the maximum herbivory rate for a herbivore of 1 g
        /// </summary>
        public double HerbivoryRateConstant
        { get { return _HerbivoryRateConstant; } }

        /// <summary>
        /// The exponent to apply to body mass in the relationship between body mass and herbivory rate
        /// </summary>
        private double _HerbivoryRateMassExponent;
        /// <summary>
        /// Get and set the exponent to apply to body mass in the relationship between body mass and herbivory rate
        /// </summary>
        public double HerbivoryRateMassExponent
        {   get { return _HerbivoryRateMassExponent; }      }

        /// <summary>
        /// The exponent applied to prey density when calculating attack rates for organisms in the terrestrial realm
        /// </summary>
        private double _AttackRateExponentTerrestrial;
        /// <summary>
        /// Get and set the exponent applied to prey density when calculating attack rates for organisms in the terrestrial realm
        /// </summary>
        public double AttackRateExponentTerrestrial
        {    get { return _AttackRateExponentTerrestrial; }  }

        /// <summary>
        /// The exponent applied to prey density when calculating attack rates for organisms in the marine realm
        /// </summary>
        private double _AttackRateExponentMarine;

        /// <summary>
        /// Get and set the exponent applied to prey density when calculating attack rates for organisms in the marine realm
        /// </summary>
        public double AttackRateExponentMarine
        {  get { return _AttackRateExponentMarine; }  }

        // Variable to hold the instantaneous fraction of the autotroph stock biomass that is eaten
        double InstantFractionEaten;

        //
        private double RelativeFeedingPreferenceHerb;

        /// <summary>
        /// The standard deviation in attack rates around the optimal prey to predator mass ratio
        /// </summary>
        private double _FeedingPreferenceStandardDeviation;

        /// <summary>
        /// Get the standard deviation in attack rates around the optimal prey to predator mass ratio
        /// </summary>
        public double FeedingPreferenceStandardDeviation { get { return _FeedingPreferenceStandardDeviation; } }
        private double FeedingPreferenceHalfStandardDeviation;


        public void InitialiseParametersHerbivory()
        {
            _TimeUnitImplementation =
                EcologicalParameters.TimeUnits[(int)EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.TimeUnitImplementation"]];
            _HandlingTimeScalarTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.HandlingTimeScalar"];
            _HandlingTimeScalarMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.HandlingTimeScalar"];
            _HandlingTimeExponentTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.HandlingTimeExponent"];
            _HandlingTimeExponentMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.HandlingTimeExponent"];
            _HerbivoryRateConstant = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.RateConstant"];
            _HerbivoryRateMassExponent = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.RateMassExponent"];
            _AttackRateExponentTerrestrial = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Terrestrial.AttackRateExponent"];
            _AttackRateExponentMarine = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.Marine.AttackRateExponent"];
            _ReferenceMass = EcologicalParameters.Parameters["Herbivory.RevisedHerbivory.HandlingTimeReferenceMass"];
        }


        /// <summary>
        /// Write out the values of the parameters to an output file
        /// </summary>
        /// <param name="sw">A streamwriter object to write the parameter values to</param>
        public void WriteOutParameterValues(StreamWriter sw)
        {
            // Write out parameters
            sw.WriteLine("Herbivory\t_TimeUnitImplementation\t" + Convert.ToString(_TimeUnitImplementation));
            sw.WriteLine("Herbivory\t_ReferenceMass_g\t" + Convert.ToString(_ReferenceMass));
            sw.WriteLine("Herbivory\t_HandlingTimeScalarTerrestrial\t" + Convert.ToString(_HandlingTimeScalarTerrestrial));
            sw.WriteLine("Herbivory\t_HandlingTimeScalarMarine\t" + Convert.ToString(_HandlingTimeScalarMarine));
            sw.WriteLine("Herbivory\t_HandlingTimeExponentTerrestrial\t" + Convert.ToString(_HandlingTimeExponentTerrestrial));
            sw.WriteLine("Herbivory\t_HandlingTimeExponentMarine\t" + Convert.ToString(_HandlingTimeExponentMarine));
            sw.WriteLine("Herbivory\t_HerbivoryRateConstant\t" + Convert.ToString(_HerbivoryRateConstant));

            sw.WriteLine("Herbivory\t_AttackRateExponentTerrestrial\t" + Convert.ToString(_AttackRateExponentTerrestrial));
            sw.WriteLine("Herbivory\t_AttackRateExponentMarine\t" + Convert.ToString(_AttackRateExponentMarine));
            sw.WriteLine("Herbivory\t_HerbivoryRateMassExponent\t" + Convert.ToString(_HerbivoryRateMassExponent));
        }



        /// <summary>
        /// Calculates the potential biomass of an autotroph stock eaten by a herbivore cohort (terrestrial)
        /// </summary>
        /// <param name="autotrophBiomass">The total biomass of the autotroph stock</param>
        /// <param name="herbivoreIndividualMass">The individual body mass of the acting (herbivore) cohort</param>
        /// <returns>The potential biomass eaten by the herbivore cohort</returns>
        private double CalculatePotentialBiomassEatenTerrestrial(double autotrophBiomass, double herbivoreIndividualMass)
        {
            // Calculate the inidividual herbivory rate per unit autotroph mass-density per hectare
            double IndividualHerbivoryRate = CalculateIndividualHerbivoryRatePerHectareTerrestrial(herbivoreIndividualMass);

            // Calculate autotroph biomass density per hectare
            double AutotrophBiomassDensity = autotrophBiomass / _CellAreaHectares;
            
            // Calculate the expected autotroph biomass eaten
            return IndividualHerbivoryRate * Math.Pow(AutotrophBiomassDensity, _AttackRateExponentTerrestrial);
        }

        /// <summary>
        /// Calculates the potential biomass of an autotroph stock eaten by a herbivore cohort (marine)
        /// </summary>
        /// <param name="autotrophBiomass">The total biomass of the autotroph stock</param>
        /// <param name="herbivoreIndividualMass">The individual body mass of the acting (herbivore) cohort</param>
        /// <returns>The potential biomass eaten by the herbivore cohort</returns>
        private double CalculatePotentialBiomassEatenMarine(double autotrophBiomass, double herbivoreIndividualMass, 
            double stockIndividualMass, double logOptimalPreyPredatorMassRatio)
        {
            // Calculate the individual herbivory rate per unit autotroph mass-density per hectare
            double IndividualHerbivoryRate = CalculateIndividualHerbivoryRatePerHectareMarine(herbivoreIndividualMass, stockIndividualMass,
                logOptimalPreyPredatorMassRatio);

            // Calculate autotroph biomass density per hectare
            double AutotrophBiomassDensity = autotrophBiomass / _CellAreaHectares;

            // Calculate the expected autotroph biomass eaten
            return IndividualHerbivoryRate * Math.Pow(AutotrophBiomassDensity, _AttackRateExponentMarine);
        }


        /// <summary>
        /// Calculate the herbivory rate of an individual herbivore per unit autotroph mass-density per hectare (terrestrial)
        /// </summary>
        /// <param name="herbivoreIndividualMass">Herbivore individual body mass</param>
        /// <returns>The herbivory rate of an individual herbivore per unit autotroph mass-density per hectare</returns>
        private double CalculateIndividualHerbivoryRatePerHectareTerrestrial(double herbivoreIndividualMass)
        {
            // Calculate the individual herbivory rate
            return _HerbivoryRateConstant * Math.Pow(herbivoreIndividualMass, (_HerbivoryRateMassExponent));

        }

        /// <summary>
        /// Calculate the herbivory rate of an individual herbivore per unit autotroph mass-density per hectare (marine)
        /// </summary>
        /// <param name="herbivoreIndividualMass">Herbivore individual body mass</param>
        /// <returns>The herbivory rate of an individual herbivore per unit autotroph mass-density per hectare</returns>
        private double CalculateIndividualHerbivoryRatePerHectareMarine(double herbivoreIndividualMass, double stockIndividualMass,
            double logOptimalPreyPredatorMassRatio)
        {
            // Calculate the relative feeding preference from a log-normal distribution with mean equal to the optimal
            // prey to predator ratio and standard deviation as specified

            RelativeFeedingPreferenceHerb = Math.Exp(-(Math.Pow
                (((Math.Log(stockIndividualMass / herbivoreIndividualMass) - logOptimalPreyPredatorMassRatio) /
                _FeedingPreferenceStandardDeviation), 2)));

            // Calculate the individual herbivory rate
            return _HerbivoryRateConstant * Math.Pow(herbivoreIndividualMass, (_HerbivoryRateMassExponent)) * RelativeFeedingPreferenceHerb;

        }

        /// <summary>
        /// Calculate the time taken for a herbivore in the marine realm to handle unit mass (1 g) of autotroph mass
        /// </summary>
        /// <param name="herbivoreIndividualMass">The body mass of an individual herbivore</param>
        /// <returns>The time taken for a herbivore to handle unit mass (1 g) of autotroph mass</returns>
        private double CalculateHandlingTimeMarine(double herbivoreIndividualMass)
        {
            return _HandlingTimeScalarMarine * Math.Pow((_ReferenceMass / herbivoreIndividualMass), _HandlingTimeExponentMarine);
        }


        /// <summary>
        /// Calculate the time taken for a herbivore in the terrestrial realm to handle unit mass (1 g) of autotroph mass
        /// </summary>
        /// <param name="herbivoreIndividualMass">The body mass of an individual herbivore</param>
        /// <returns>The time taken for a herbivore to handle unit mass (1 g) of autotroph mass</returns>
        private double CalculateHandlingTimeTerrestrial(double herbivoreIndividualMass)
        {
               return _HandlingTimeScalarTerrestrial * Math.Pow((_ReferenceMass / herbivoreIndividualMass), _HandlingTimeExponentTerrestrial);
        }

        /// <summary>
        /// Calculate the actual biomass eaten by a herbivore cohort from an autotroph stock
        /// </summary>
        /// <param name="potentialBiomassEaten">The potential biomass eaten by the herbivore cohort from the autotroph stock given the encounter rate</param>
        /// <param name="totalHandlingTime">The total time that would be taken to handle all encountered autotroph biomass in all autotroph stocks</param>
        /// <param name="herbivoreAbundance">The number of individuals in the acting herbivore cohort</param>
        /// <param name="autotrophBiomass">The total biomass in the autotroph stock</param>
        /// <returns>The biomass eaten by the herbivore cohort from the autotroph stock</returns>
        private double CalculateBiomassesEaten(double potentialBiomassEaten, double totalHandlingTime, double herbivoreAbundance, double autotrophBiomass)
        {
            // Check whether there is any biomass in the autotroph stock
            if (autotrophBiomass > 0.0)
            {
                // Calculate the instantaneous fraction of the autotroph stock eaten
                InstantFractionEaten = herbivoreAbundance * ((potentialBiomassEaten / (1 + totalHandlingTime)) / autotrophBiomass);
            }
            else
            {
                // Set the instantaneous fraction of the autotroph stock eaten to zero
                InstantFractionEaten = 0.0;
            }
            
            // Return the total  biomass of the autotroph stock eaten
            return autotrophBiomass * (1 - Math.Exp(-InstantFractionEaten * _DeltaT * _ProportionOfTimeEating));
        }

    }
}
