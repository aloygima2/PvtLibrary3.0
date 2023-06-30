using System;
using System.Collections.Generic;
using MathematicsLibrary2021;
using PvtLibrary3.Common;
using static System.Math;

namespace PvtLibrary3.BlackOil
{
    public class Oil
    {
        static double Tf;
        static double apiForDegetto;
        public static double gasgrav100VazquezAndBeggs(double API, double gasgrav, double psep = 14.7, double Tsep = 60.0)
        {
            var term = (1.0 + 5.912e-5 * API * Tsep * Math.Log10(psep / 114.7));
            var gasgrav_100 = gasgrav * term;
            return gasgrav_100;
        }
        public static double gasgrav100_kartoatmodjo_and_schmidt(double API, double gasgrav, double psep = 14.7, double Tsep = 60.0)
        {
            var gasgrav_100 = gasgrav * (1.0 + 0.1595 * Math.Pow(API, 0.4078) * Math.Pow(Tsep, (-0.2466)) * Math.Log(psep / 114.7));
            return gasgrav_100;
        }

        #region Rs Correlations

        #region RsAlmarhoun
        /// <summary>
        /// Calculates the solution gas oil ratio using Al-Marhoun correlations.
        /// </summary>
        /// <remarks>
        /// References
        /// see <see cref="BubblePoint.AlMarhounPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb"> bubble point pressure in psia</param>
        /// <returns>Rs in scf/stb</returns>
        public static double RsAlMarhoun(double P, double T, double Pb, double Api,
                                         double GasGravity, double Psp1, double Tsp1,
                                         double condensateGravity, double RsTotal,
                                         double Rs1, double Rs2, double Rs3, double Gg2,
                                         double Gg3, double Tst, SeparatorStage separatorTrain,
                                         double Cp1 = 1, double Cp2 = 0 )
        {
            double Rs;
            if (P < Pb)
            {
                Rs = AlMarhounSaturatedRs(P, T, Api, GasGravity, Psp1, Tsp1, condensateGravity, Rs1,
                                            Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }
            return Cp1 * Rs + Cp2;
        }

        internal static double AlMarhounSaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psp1, double Tsp1, double condensateGravity, 
                                        double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                        double Tst, SeparatorStage separatorTrain)
        {
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3, 
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            if (P < Tst) { return 0.0; }
            double yo = 141.5 / (131.5 + Api);
            double Rs = Pow((185.843208 * Pow(ygave, 1.8774) * Pow(yo, -3.1437) * (Pow((T + 460), -1.32657)) * P), 1.398441);
            return Rs;
        }

        #endregion

        #region RsDeGhetto
        public static double RsDeGhetto(double pressure, double temperature, double Api,
                                         double gasGrav, double Psep, double Tsep, 
                                         double Cp1 = 1.0, double Cp2 = 0)
        {
            double Rs;
            double api = Api;
            if (api < 10)
            {
                Rs = ExtractHeavyOilRs(pressure, temperature, Api, gasGrav);
            }
            else if (api < 22.3 && api >= 10)
            {
                Rs = HeavyOilRs(pressure, temperature, Api, gasGrav, Psep, Tsep);
            }
            else if (api >= 22.3 && api < 31.1)
            {
                Rs = MediumOilRs(pressure, temperature, Api, gasGrav, Psep, Tsep);
            }
            else 
            { 
                Rs = LightOilRs(pressure, temperature, Api, gasGrav, Psep, Tsep); 
            }

            return Cp1 * Rs + Cp2;
        }

        private static double ExtractHeavyOilRs(double pressure, double T, double Api,
                                         double gasGrav)
        {
            if (pressure < 14.7) return 0.0;
            double api = Api;
            double yg = gasGrav;
            double T1 = pressure / 10.7025;
            double T2 = Pow(10, (0.0169 * api - 0.00156 * T));
            double RsH = yg * Pow((T1 * T2), 1.1128);
            return RsH;
        }

        private static double HeavyOilRs(double pressure, double T, double Api,
                                         double gasGrav, double Psep, double Tsep)
        {
            if (pressure < 14.7) return 0.0;
            double api = Api;
            double yg = gasGrav;
            double tsep = Tsep;
            double psep = Psep;
            double ygcorterm1 = 1.0 + 0.00005912 * api * tsep * Log10(psep / 114.7);
            double ygcor = yg * ygcorterm1;
            double rsT1 = (ygcor * Pow(pressure, 1.2057)) / 56.434;
            double rst2 = Pow(10, (10.9267 * api) / (T + 460));
            double RsH = rsT1 * rst2;
            return RsH;
        }

        private static double MediumOilRs(double pressure, double T, double Api,
                                         double gasGrav, double Psep, double Tsep)
        {
            if (pressure < 14.7) return 0.0;
            double api = Api;
            double yg = gasGrav;
            double tsep = Tsep;
            double psep = Psep;
            double ygcorterm1 = 1.0 + 0.1595 * Pow(api, 0.4078) * Pow(tsep, -0.2466) * Log10(psep / 114.7);
            double ygcor = yg * ygcorterm1;
            double rsT1 = 0.10084 * Pow(ygcor, 0.2556) * Pow(pressure, 0.9868);
            double rst2 = Pow(10, (7.4576 * api) / (T + 460));
            double RsH = rsT1 * rst2;
            return RsH;
        }

        private static double LightOilRs(double pressure, double temperature, double Api, 
                                         double gasGrav, double Psep, double Tsep)
        {
            if (pressure < 14.7) return 0.0;
            double T = temperature;
            double api = Api;
            double yg = gasGrav;
            double tsep = Tsep;
            double psep = Psep;
            double ygcorterm1 = 1.0 + 0.1595 * Pow(api, 0.4078) * Pow(tsep, -0.2466) * Log10(psep / 114.7);
            double ygcor = yg * ygcorterm1;
            double rsT1 = 0.01347 * Pow(ygcor, 0.3873) * Pow(pressure, 1.1715);
            double rst2 = Pow(10, (12.753 * api) / (T + 460));
            double RsH = rsT1 * rst2;
            return RsH;
        }
        #endregion

        #region RsGlaso       
        /// <summary>
        /// This routine calculates solution-gas-oil ratio by reversing Glaso bubble correlation.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.GlasoPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">Bubble point pressure in psia</param>
        /// <returns>returns Rs in scf/stb</returns>
        public static double RsGlaso(double P, double T, double Pb, double Api, double GasGravity, 
                                     double Psp1, double Tsp1, double condensateGravity, 
                                     double RsTotal, double Rs1, double Rs2, double Rs3, double Gg2,
                                     double Gg3, double Tst, SeparatorStage separatorTrain, 
                                     double Cp1 = 1.0, double Cp2 = 0.0 )
        {
            double Rs;
            if (P < Pb)
            {
                Rs = GlasoSaturatedRs(P, T, Api, GasGravity, Psp1, Tsp1, condensateGravity,
                                        RsTotal, Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }
            return Cp1 * Rs + Cp2;
        }

        internal static double GlasoSaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psp1, double Tsp1, double condensateGravity, 
                                        double RsTotal, double Rs1, double Rs2, double Rs3, 
                                        double Gg2, double Gg3, double Tst,
                                        SeparatorStage separatorTrain)
        {
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            if (P < Tst) { return 0.0; }
            double x = 2.8869 - Pow((14.1811 - 3.3093 * Log10(P)), 0.5);
            double A = Pow(10, x);
            double Rs;
            if (RsTotal <= 1500)
            {
                Rs = ygave * Pow(((Pow(Api, 0.989) / Pow(T, 0.172)) * A), 1.2255);
            }
            else
            {
                Rs = ygave * Pow(((Pow(Api, 0.989) / Pow(T, 0.1302)) * A), 1.2255);
            }

            return Rs;
        }
        #endregion

        #region RsLasater

        /// <summary>
        /// Calculates the solution-gas-oil ratio by reversing Lasater Bubble point coreelation.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.LasaterPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <returns>returns Rs in scf/stb</returns>
        public static double RsLasater(double P, double T, double Pb, double Api, 
                        double GasGravity, double Psp1, double Tsp1, double condensateGravity,
                        double RsTotal, double Rs1, double Rs2, double Rs3, double Gg2, 
                        double Gg3, double Tst, SeparatorStage separatorTrain, 
                        double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double Rs;
            if (P < Pb)
            {
                Rs = LasaterSaturatedRs(P, T, Api, GasGravity, Psp1, Tsp1, condensateGravity, 
                                          Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }
            return Cp1 * Rs + Cp2;
        }

        internal static double LasaterSaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psp1, double Tsp1, double condensateGravity,
                                        double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                        double Tst, SeparatorStage separatorTrain)
        {
            if (P < Tst) { return 0.0; }
            double yo = 141.5 / (131.5 + Api);
            double Mo;
            double Rs;
            if (Api <= 40)
            {
                Mo = 630 - 10 * Api; ;
            }
            else
            {
                Mo = 73110 / Pow(Api, 1.562);
            }
            //double ygave = AverageGasgravity.ygAve(input);
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double Pf = P * ygave / (T + 460);
            double y;
            //if (Pf < 3.29)
            //{
            //    y = 0.359 * Log(1.473 * Pf + 0.476);
            //}
            //else
            //{
            //    y = Pow((0.121 * Pf - 0.236), 0.281);
            //}
            #region previous implementation
            double ya = 0.6;
            double absy;
            do
            {
                double fy = Pf - 0.11912 - 1.36226 * ya - 3.10526 * Math.Pow(ya, 2) - 5.043 * Math.Pow(ya, 3);
                double dfy = -1.36226 - 6.21052 * ya - 15.129 * Math.Pow(ya, 2);
                double yn = ya - fy / dfy;
                if (yn < 0.0) { break; }
                absy = Math.Abs(yn - ya);
                ya = yn;
            } while (absy >= Math.Pow(10, -6));
            y = ya;
            #endregion
            Rs = 132755 * yo * y / (Mo * (1 - y));
            return Rs;
        }
        #endregion

        #region RsPetroskyFarshad
        
        /// <summary>
        /// Calculates solution-gas-ratio using Petrosky and Farshard correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.PetroskyFarshardPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <returns>returns Rs in scf/stb</returns>
        public static double RsPetroskyFarshad(double P, double T, double Pb, double Api, 
                                               double GasGravity, double Psp1, double Tsp1,
                                               double condensateGravity, double RsTotal, 
                                               double Rs1, double Rs2, double Rs3, double Gg2,
                                               double Gg3, double Tst, SeparatorStage separatorTrain,
                                               double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double Rs;
            if (P < Pb)
            {
                Rs = PetroskySaturatedRs(P, T, Api, GasGravity, Psp1, Tsp1, condensateGravity,
                                          Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }
            return Cp1 * Rs + Cp2; 
        }
        
        internal static double PetroskySaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psp1, double Tsp1, double condensateGravity,
                                        double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                        double Tst, SeparatorStage separatorTrain)
        {
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            if (P < Tst) { return 0.0; }
            double x = 0.0007916 * Pow(Api, 1.541) - 0.00004561 * Pow(T, 1.3911);
            double Rs = Pow(((P / 112.727 + 12.34) * Pow(ygave, 0.8439) * Pow(10, x)), 1.73184);
            return Rs;
        }
        #endregion

        #region RsStanding
        
        /// <summary>
        /// This routine calculates solution-gas-oil ratio using Standing correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.StandingPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <returns>returns Rs in scf/stb</returns>
        public static double RsStanding(double P, double T, double Pb, double Api,
                                        double GasGravity, double Psp1, double Tsp1,
                                        double condensateGravity, double RsTotal,
                                        double Rs1, double Rs2, double Rs3, double Gg2,
                                        double Gg3, double Tst, SeparatorStage separatorTrain,
                                        double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double Rs;
            if (P < Pb)
            {
                Rs = StandingSaturatedRs(P, T, Api, GasGravity, Psp1, Tsp1, condensateGravity, Rs1,
                                            Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }

            return Cp1 * Rs + Cp2;
        }

        internal static double StandingSaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psp1, double Tsp1, double condensateGravity,
                                        double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                        double Tst, SeparatorStage separatorTrain)
        {
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            if (P < Tst) { return 0.0; }
            double x = (0.0125 * Api) - (0.00091 * T);
            double Rs = ygave * Pow((((P / 18.2) + 1.4) * Pow(10, x)), 1.2048);
            return Rs;
        }
        #endregion

        #region RsVasquezBeggs
       
        /// <summary>
        /// This routine calculates solution-gas-oil ratio using Vazquez and Beggs correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.Vazquez_BeggsPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point in psia</param>
        /// <returns>returns Rs in scf/stb</returns>
        public static double RsVazquezBeggs(double P, double T, double Pb, double Api,
                                         double GasGravity, double Psep, double Tsep,
                                         double Psp1, double Tsp1, double condensateGravity,
                                         double RsTotal, double Rs1, double Rs2, double Rs3, 
                                         double Gg2, double Gg3, double Tst, 
                                         SeparatorStage separatorTrain, double Cp1 = 1.0,
                                         double Cp2 = 0.0)
        {
            double Rs;
            if (P < Pb)
            {
                Rs = VazquezSaturatedRs(P, T, Api, GasGravity,
                                        Psep, Tsep, Psp1, Tsp1,
                                        condensateGravity, Rs1, Rs2,
                                        Rs3, Gg2, Gg3, Tst, separatorTrain);
            }
            else
            {
                Rs = RsTotal;
            }
            return Cp1 * Rs + Cp2;
        }

        internal static double VazquezSaturatedRs(double P, double T, double Api, double GasGravity,
                                        double Psep, double Tsep, double Psp1, double Tsp1, 
                                        double condensateGravity, double Rs1, double Rs2, 
                                        double Rs3, double Gg2, double Gg3, double Tst, 
                                        SeparatorStage separatorTrain)
        {
            if (P < Tst) { return 0.0; }
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double y = ygave * (1 + (0.00005912 * Api * Tsep * Log10(Psep / 114.7)));
            //double y = input.Gasgravity * (1 + (0.00005912 * input.Api * input.Tsep * Log10(input.Psep / 114.7)));
            double Rs;
            if (Api <= 30)
            {
                Rs = 0.0362 * y * Pow(P, 1.0937) * Exp(25.724 * (Api / (T + 460)));
            }
            else
            {
                Rs = 0.0178 * y * Pow(P, 1.187) * Exp(23.931 * (Api / (T + 460)));
            }
            return Rs;
        }
        #endregion

        #endregion

        #region Co correlations
        //***** Isothermal Compressibility of Oil *****
        public static double CoVazquezAndBeggs(double p, double T, double API, double gasgrav100, double Rs)
        {
            double Co = (-1433.0 + 5 * Rs + 17.2 * T - 1180 * gasgrav100 + 12.61 * API) / (p * 1e5);
            return Co;
        }
        #region Under saturated Co

        internal static double DeGhettoUndersaturatedCo(double P, double T, double Pb, double Bob,
                                                    double Api, double GasGravity, double RsTotal,
                                                    double Psep, double Tsep)
        {
            double co;
            double api = Api;
            double tsep = Tsep;
            double psep = Psep;
            double yg = GasGravity;
            double ygcor = yg * (1.0 + 0.00005912 * api * tsep * Log10(psep / 114.7));
            if (api < 10)
            {
                co = (-889.6 + 3.1674 * RsTotal + 20 * T - 627.3 * ygcor - 81.4476 * api) / (P * 1e5);
            }
            else if (api < 22.3 && api >= 10)
            {
                co = (-2841.8 + 2.9646 * RsTotal + 25.5439 * T - 1230.5 * ygcor + 41.91 * api) / (P * 1e5);
            }
            else if (api >= 22.3 && api < 31.1)
            {
                co = (-705.288 + 2.2246 * RsTotal + 26.0644 * T - 2080.823 * ygcor - 9.6807 * api) / (P * 1e5);
            }
            else
            {
                double coIni = (-705.288 + 2.2246 * RsTotal + 26.0644 * T - 2080.823 * ygcor - 9.6807 * api) / (P * 1e5);
                Func<double, double> CoFunc = new Func<double, double>(c =>
                {
                    double bo = Bob * (1.0 - c * (P - Pb));
                    double T1 = Pow(10, -6.1646) * Pow(bo, 1.8789) * Pow(api, 0.3646) * Pow(T, 0.1966);
                    double T2 = -1 * (1.0 - (Pb / P)) * (Pow(10, -8.98) * Pow(bo, 3.9392) * Pow(T, 1.349));
                    double res = c - (T1 + T2);
                    return res;
                });
                co = Solvers.fzero(CoFunc, coIni);
            }
            return co;
        }

        internal static double PetroskyUndersaturatedCo(double P, double T, double Api,
                                                double GasGravity, double RsTotal,
                                                double condensateGravity, double Psp1,
                                                double Tsp1, double Rs1, double Rs2,
                                                double Rs3, double Gg2, double Gg3,
                                                SeparatorStage separatorTrain)
        {
            double co;
            double rsb = RsTotal;
            //double ygave = AverageGasgravity.ygAve(input);
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double api = Api;
            co = 1.705e-7 * Pow(rsb, 0.69357) * Pow(ygave, 0.1885) * Pow(api, 0.3272) * Pow(T, 0.6729) * Pow(P, -0.5906);
            return co;
        }

        internal static double StandingUndersaturatedCo(double P, double Pb, double Bo,
                                            double Rs, double Api, double GasGravity,
                                            double condensateGravity, double Psp1,
                                            double Tsp1, double Rs1, double Rs2,
                                            double Rs3, double Gg2, double Gg3,
                                            SeparatorStage separatorTrain)
        {
            //double rhoPb = new OilDensity().Density(input, input.RsTotal, bo);
            double rhoPb = OilDensity(Rs, Bo, Api, GasGravity, condensateGravity, Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);
            double pDiff = P - Pb;
            double co = 1e-6 * Exp((rhoPb + 0.004347 * pDiff - 79.1) / (0.0007141 * pDiff - 12.938));
            return co;
        }

        internal static double VazquezBeggsUnderSaturatedCo(double P, double T, double Api,
                                                        double GasGravity, double RsTotal, 
                                                        double Psep, double Tsep)
        {
            double y, A, co;
            y = GasGravity * (1 + (0.00005912 * Api * Tsep * Log10(Psep / 114.7)));
            A = Pow(10, -5) * (5 * RsTotal + 17.2 * T - 1180 * y + 12.61 * Api - 1433);
            co = A / P;
            return co;
        }


        #endregion
                
        #endregion

        #region Bo correlations
        //***** Oil Formation Volume Factor and Bubblepoint Pressure *****
        #region BoAlMarhoun
        /// <summary>
        /// This routine calculates oil formation volume factor using Almarhoun corrections.
        /// </summary>
        /// <remarks>
        /// Reference:
        /// see <see cref="BubblePoint.AlMarhounPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point in psia</param>
        /// <param name="Rs">solution gas-oil ration in scf/stb</param>
        /// <returns>returns Bo in rbb/stb</returns>
        public static double BoAlMarhoun(double P, double T, double Pb, double Rs, double Api, 
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1, 
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3, 
                                    SeparatorStage separatorTrain, double Cp1 = 1.0,
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = AlMarhounBoFunc(T, Rs, Api, GasGravity, condensateGravity,
                                    Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3,
                                    separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coAve = VazquezBeggsUnderSaturatedCo(P, T, Api, GasGravity, RsTotal, Psep, Tsep);
                bo = bo * (1 - coAve * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double AlMarhounBoFunc(double T, double Rs, double Api,
                                    double GasGravity, double condensateGravity,
                                    double Psp1, double Tsp1, double Rs1, 
                                    double Rs2, double Rs3, double Gg2, double Gg3,
                                    SeparatorStage separatorTrain)
        {
            double yo, f, Bo;
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            yo = 141.5 / (131.5 + Api);
            f = Pow(Rs, 0.74239) * Pow(ygave, 0.323294) * Pow(yo, -1.20204);
            Bo = 0.497069 + (0.000862963 * (T + 460)) + (0.00182594 * f) + (0.00000318099 * Pow(f, 2));
            return Bo;
        }
        #endregion

        #region BoGlaso
        /// <summary>
        /// Calculates Bo using Glaso correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.GlasoPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point in psia</param>
        /// <param name="Rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>Rs in rb/stb</returns>
        public static double BoGlaso(double P, double T, double Pb, double Rs, double Api, 
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1, 
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3, 
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = GlasoBoFunc(Rs, T, Api, GasGravity, condensateGravity, Psp1, 
                                    Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coave = VazquezBeggsUnderSaturatedCo(P, T, Api, GasGravity, RsTotal, Psep, Tsep);
                bo = bo * (1 - coave * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double GlasoBoFunc(double Rs, double T, double Api, double GasGravity,
                                        double condensateGravity, double Psp1, double Tsp1, 
                                        double Rs1, double Rs2, double Rs3, double Gg2, 
                                        double Gg3, SeparatorStage separatorTrain)
        {
            double yo, B, A, Bo;

            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            yo = 141.5 / (131.5 + Api);
            B = Rs * Pow((ygave / yo), 0.526) + (0.968 * T);
            A = -6.58511 + 2.91329 * Log10(B) - 0.27683 * (Pow(Log10(B), 2));
            Bo = 1 + Pow(10, A);
            return Bo;
        }
        #endregion

        #region BoDeGhetto
        /// <summary>
        /// Calculates Bo using De Ghetto correlations.
        /// </summary>
        /// References:
        /// see <see cref="BubblePoint.DeGhettoPb(BlackOilInputs, double)"/>
        /// <remarks>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point ressure in psia</param>
        /// <param name="Rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>Bo in rb/stb</returns>
        public static double BoDeGhetto(double P, double T, double Pb, double Rs, 
                                    double Api, double GasGravity, double RsTotal,
                                    double Psep, double Tsep, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = DeGhettoBoFunc(T, Rs, Api, GasGravity, Psep, Tsep);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coAve = DeGhettoUndersaturatedCo(P, T, Pb, bo, Api, GasGravity, RsTotal,
                                                    Psep, Tsep);
                bo = bo * (1 - coAve * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double DeGhettoBoFunc(double T, double Rs, double Api,
                                    double GasGravity, double Psep, double Tsep)
        {
            double Bo, y;
            y = GasGravity * (1 + (0.00005912 * Api * Tsep * Log10(Psep / 114.7)));
            if (Api <= 30)
            {
                Bo = 1 + 4.677 * Pow(10, -4) * Rs + (T - 60) * (Api / y) * (1.75 * Pow(10, -5) - (1.811 * Pow(10, -8) * Rs));
            }
            else
            {
                Bo = 1 + 4.67 * Pow(10, -4) * Rs + (T - 60) * (Api / y) * (1.1 * Pow(10, -5) + (1.337 * Pow(10, -9) * Rs));
            }

            return Bo;
        }
        #endregion

        #region BoLasater
        /// <summary>
        /// This routine calculates Bo using Standing Correlation, There is no lasater Bo correlation.
        /// </summary>
        /// <remarks>
        /// references:
        /// see <see cref="StandingBo(BlackOilInputs, double, double, double, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="Rs">Rs in scf/stb</param>
        /// <returns>returns Bo in rb/stb</returns>
        public static double BoLasater(double P, double T, double Pb, double Rs, double Api,
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1,
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = LasaterBoFunc(Rs, T, Api, Psp1, Tsp1, condensateGravity,
                                    Rs1, Rs2, Rs3, Gg2, Gg3, GasGravity, separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coave = VazquezBeggsUnderSaturatedCo(P, T, Api, GasGravity, RsTotal, Psep, Tsep);
                bo = bo * (1 - coave * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double LasaterBoFunc(double Rs, double T, double Api, double Psp1, 
                                        double Tsp1, double condensateGravity, double Rs1,
                                        double Rs2, double Rs3, double Gg2, double Gg3,
                                        double GasGravity, SeparatorStage separatorTrain)
        {
            double yo, Bo;
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);

            yo = 141.5 / (131.5 + Api);
            Bo = 0.972 + 0.000147 * Math.Pow((Rs * Math.Pow((ygave / yo), 0.5) + (1.25 * T)), 1.175);
            return Bo;
        }
        #endregion

        #region BoPetroskyFarshard
        /// <summary>
        /// calculates Bo using Petrosky and Farshard correlations.
        /// </summary>Refrences:
        /// see <see cref="BubblePoint.PetroskyFarshardPb(BlackOilInputs, double)"/>
        /// <remarks>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="Rs">Rs in scf/stb</param>
        /// <returns>Bo in rb/stb</returns>
        public static double BoPetroskyFarshard(double P, double T, double Pb, double Rs, double Api,
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1,
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = PetroskyBoFunc(Rs, T, Api, GasGravity, condensateGravity, Psp1, Tsp1,
                                    Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coave = PetroskyUndersaturatedCo(P, T, Api, GasGravity, RsTotal,
                                            condensateGravity, Psp1, Tsp1, Rs1, Rs2,
                                            Rs3, Gg2, Gg3, separatorTrain);
                bo = bo * (1 - coave * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double PetroskyBoFunc(double Rs, double T, double Api, double GasGravity,
                                        double condensateGravity, double Psp1, double Tsp1,
                                        double Rs1, double Rs2, double Rs3, double Gg2,
                                        double Gg3, SeparatorStage separatorTrain)
        {
            double yo, A, Bo;
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            yo = 141.5 / (131.5 + Api);
            A = Pow(((Pow(Rs, 0.3738) * (Pow(ygave, 0.2914) / Pow(yo, 0.6265))) + (0.24626 * Pow(T, 0.5371))), 3.0936);
            Bo = 1.0113 + (0.000072046 * A);
            return Bo;
        }
        #endregion

        #region BoStanding
        /// <summary>
        /// calculates Bo using Standing correlations.
        /// </summary>
        /// <remarks>
        /// references:
        /// see <see cref="BubblePoint.StandingPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point in psia</param>
        /// <param name="Rs">Rs in scf/stb</param>
        /// <returns>returns Bo in rb/stb</returns>
        public static double BoStanding(double P, double T, double Pb, double Rs, double Api,
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1,
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = StandingBoFunc(T, Rs, Api, GasGravity, condensateGravity, Psp1, Tsp1,
                                    Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coave = VazquezBeggsUnderSaturatedCo(P, T, Api, GasGravity, RsTotal, Psep, Tsep);
                bo = bo * (1 - coave * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }
       
        internal static double StandingBoFunc(double T, double Rs, double Api, double GasGravity,
                                        double condensateGravity, double Psp1, double Tsp1,
                                        double Rs1, double Rs2, double Rs3, double Gg2,
                                        double Gg3, SeparatorStage separatorTrain)
        {
            double yo, Bo;
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            yo = 141.5 / (131.5 + Api);
            Bo = 0.972 + 0.000147 * Pow((Rs * Pow((ygave / yo), 0.5) + (1.25 * T)), 1.175);
            return Bo;
        }
        #endregion

        #region BoVazquezBeggs
        /// <summary>
        /// Calculates Bo using Vazquez and Beggs correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="BubblePoint.Vazquez_BeggsPb(BlackOilInputs, double)"/>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="Rs">Rs in scfstb</param>
        /// <returns>Bo in rb/stb</returns>
        public static double BoVazquezBeggs(double P, double T, double Pb, double Rs, double Api,
                                    double RsTotal, double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, double Tsp1,
                                    double Rs1, double Rs2, double Rs3, double Gg2, double Gg3,
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double bo = VazquezBoFunc(T, Rs, Api, GasGravity, condensateGravity, Psep, Tsep,
                                    Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);
            if (P <= Pb) 
            { 
                return Cp1 * bo + Cp2; 
            }
            else
            {
                double coave = VazquezBeggsUnderSaturatedCo(P, T, Api, GasGravity, RsTotal, Psep, Tsep);
                bo = bo * (1 - coave * (P - Pb));
                return Cp3 * bo + Cp4;
            }
        }

        internal static double VazquezBoFunc(double T, double Rs, double Api, double GasGravity,
                                        double condensateGravity, double Psep, double Tsep,
                                        double Psp1, double Tsp1, double Rs1, double Rs2, 
                                        double Rs3, double Gg2, double Gg3, 
                                        SeparatorStage separatorTrain)
        {
            double Bo, y;
            double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            y = yg * (1 + (0.00005912 * Api * Tsep * Log10(Psep / 114.7)));
            //y = input.Gasgravity * (1 + (0.00005912 * input.Api * input.Tsep * Log10(input.Psep / 114.7)));
            if (Api <= 30)
            {
                Bo = 1 + 4.677 * Pow(10, -4) * Rs + (T - 60) * (Api / y) * (1.75 * Pow(10, -5) - (1.811 * Pow(10, -8) * Rs));
            }
            else
            {
                Bo = 1 + 4.67 * Pow(10, -4) * Rs + (T - 60) * (Api / y) * (1.1 * Pow(10, -5) + (1.337 * Pow(10, -9) * Rs));
            }
            return Bo;
        }
        #endregion 
        
        #endregion

        #region Pb Correlations
        // Bubble point
        private static double JacobsonNitrogenCorrectionfactor(double yN2, double T)
        {
            if (yN2 == 0.0) return 1.0;
            else { return 1.1585 + 2.86 * yN2 - 0.00107 * T; }
        }

        #region PbAlMarhoun
        /// <summary>
        /// This routine calculates the bubble point pressure using AlMarhoun Correlations.
        /// <para>Applicable range is 130 to 3573 psia</para>
        /// </summary>
        /// <remarks>
        /// References
        /// <para>Muhammad All Al-Marhuon"PVT Correlations for Middle East Crude Oils." Journals of Petroleum Technology, May 1988</para>
        /// <para>see also <see cref="Reference_Materials"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>returns bubble point pressure in psia</returns>
        public static double PbAlMarhoun(double T, double Api, double RsTotal,
                                        double GasGravity, double condensateGravity,  
                                        double Psp1, double Tsp1, double Rs1, 
                                        double Rs2, double Rs3, double Gg2,
                                        double Gg3, double yN2, bool PbImpurityCorrection,
                                        SeparatorStage separatorTrain, double Cp1 = 1.0,
                                        double Cp2 = 0.0)
        {
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double yo = 141.5 / (131.5 + Api);
            double pb = 0.00538088 * Pow(RsTotal, 0.715082) * Pow(ygave, -1.87784) * Pow(yo, 3.1437) * Pow((T + 460), 1.32657);
            if (PbImpurityCorrection) { return pb * JacobsonNitrogenCorrectionfactor(yN2, T); }
            return Cp1 * pb + Cp2;
        }

        #endregion

        #region PbDeGhetto
        /// <summary>
        /// This routine calculates bubble point pressure using De Ghetto correlations.
        /// <para>The applicable range is from 107.33 to 6613.82 psia </para>
        /// </summary>
        /// <remarks>
        /// References
        /// <para>G. De Ghetto, F.Paone,M.Villa."Reliable Analysis of PVT Correlations." SPE 28904</para>
        /// <para>G.De Ghetto, F.Paone, M. Villa."Pressure-Volume-Temperature Correlations for Heavy and Extra Heavy Oils." SPE 30316</para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>returns bubble point pressure in psia</returns>
        public static double PbDeGhetto(double T, double Api, double RsTotal, 
                                    double GasGravity, double condensateGravity,
                                    double Psep, double Tsep, double Psp1, 
                                    double Tsp1, double Rs1, double Rs2, 
                                    double Rs3, double R3, double Gg2,
                                    double Gg3,  double yN2, bool PbImpurityCorrection,
                                    SeparatorStage separatorTrain, double Cp1 = 1.0, 
                                    double Cp2 = 0.0)
        {
            double api = Api;
            double pb;
            if (api < 10)
            {
                pb = ExtraHeavyOilPb(T, Api, GasGravity, RsTotal, Psp1, Tsp1, condensateGravity,
                                    Rs1, Rs2, Rs3, R3, Gg2, Gg3, separatorTrain);
            }
            else if (api < 22.3 && api >= 10)
            {
                pb = HeavyOilPb(T, Psep, Tsep, Api, GasGravity, RsTotal, Psp1, Tsp1,
                                condensateGravity, Rs1, Rs2, Rs3, R3, Gg2, Gg3,
                                separatorTrain);
            }
            else if (api >= 22.3 && api < 31.1)
            {
                pb = MediumOilPb(T, Psep, Tsep, Api, GasGravity, RsTotal, Psp1, Tsp1,
                                 condensateGravity, Rs1, Rs2, Rs3, R3, Gg2, Gg3, 
                                 separatorTrain);
            }
            else
            {
                pb = LightOilPb(T, Api, GasGravity, RsTotal, Psp1, Tsp1, condensateGravity,
                                Rs1, Rs2, Rs3, R3, Gg2, Gg3, separatorTrain);
            }
            if (PbImpurityCorrection)
            {
                return pb * JacobsonNitrogenCorrectionfactor(yN2, T);
            }
            return Cp1 * pb + Cp2;
        }

        private static double ExtraHeavyOilPb(double T, double Api, double GasGravity,
                                            double RsTotal, double Psp1, double Tsp1, 
                                            double condensateGravity, double Rs1, 
                                            double Rs2, double Rs3, double R3, double Gg2, 
                                            double Gg3, SeparatorStage separatorTrain)
        {
            double api = Api;
            //double yg = AverageGasgravity.ygAve(input);// input.Gasgravity;
            double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double Rs = RsTotal;
            if (separatorTrain == SeparatorStage.TwoStages)
            {
                Rs = RsTotal + R3;
            }
            double T1 = Pow((Rs / yg), (1 / 1.1128));
            double T2 = 10.7025 / Pow(10, (0.0169 * api - 0.0156 * T));
            double PbH = T1 * T2;

            return PbH;
        }

        #region not used
        //private static double HeavyOilPb1(double T, double Api, double GasGravity,
        //                                    double RsTotal, double Psp1, double Tsp1,
        //                                    double condensateGravity, double Rs1,
        //                                    double Rs2, double Rs3, double R3, double Gg2,
        //                                    double Gg3, SeparatorStage separatorTrain)
        //{
        //    double api = Api;
        //    //double yg = AverageGasgravity.ygAve(input);// input.Gasgravity;
        //    double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
        //                                         Gg2, Gg3, GasGravity, separatorTrain);
        //    double Rs = RsTotal;
        //    if (separatorTrain == SeparatorStage.TwoStages)
        //    {
        //        Rs = RsTotal + R3;
        //    }
        //    double T1 = Pow((Rs / yg), 0.7885);
        //    double T2 = Pow(10, 0.002 * T) / Pow(10, 0.0142 * api);
        //    double PbH = 15.7286 * (T1 * T2);

        //    return PbH;
        //}
        #endregion
        private static double HeavyOilPb(double Temperature, double Psep, double Tsep,
                                        double Api, double GasGravity, double RsTotal, 
                                        double Psp1, double Tsp1, double condensateGravity,
                                        double Rs1, double Rs2, double Rs3, double R3, 
                                        double Gg2, double Gg3, SeparatorStage separatorTrain)
        {
            double api = Api;
            //double yg = AverageGasgravity.ygAve(input);// input.Gasgravity;
            double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2,
                                            Rs3, Gg2, Gg3, GasGravity, separatorTrain);
            double Rs = RsTotal;
            if (separatorTrain == SeparatorStage.TwoStages)
            {
                Rs = RsTotal + R3;
            }
            double ygcor = yg * (1.0 + 0.00005912 * api * Tsep * Log10(Psep / 114.7));
            double exponent = (10.9267 * api) / (Temperature + 460);
            double denom = ygcor * Pow(10, exponent);

            double T1 = (56.434 * Rs) / denom;

            double PbH = Pow(T1, (1.0 / 1.2057));

            return PbH;
        }
        private static double MediumOilPb(double T, double Psep, double Tsep, double Api, 
                                        double GasGravity, double RsTotal, double Psp1,
                                        double Tsp1, double condensateGravity, double Rs1,
                                        double Rs2, double Rs3, double R3, double Gg2,
                                        double Gg3, SeparatorStage separatorTrain)
        {
            double api = Api;
            double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double Rs = RsTotal;
            if (separatorTrain == SeparatorStage.TwoStages)
            {
                Rs = RsTotal + R3;
            }
            double Tsp = Tsep;
            double psep = Psep;
            double T1 = 1.0 + 0.1595 * Pow(api, 0.4078) * Pow(Tsp, -0.2466) * Log10(psep / 114.7);
            //double ygcor = yg * psep * T1;
            double ygcor = yg * T1;
            double denom = 0.09902 * Pow(ygcor, 0.2181) * Pow(10, ((7.2153 * api) / (T + 460)));
            double Pbm = Pow((Rs / denom), 0.9997);
            return Pbm;
        }
        private static double LightOilPb(double T, double Api, double GasGravity, 
                                        double RsTotal, double Psp1, double Tsp1,
                                        double condensateGravity, double Rs1,
                                        double Rs2, double Rs3, double R3, double Gg2,
                                        double Gg3, SeparatorStage separatorTrain)
        {
            double api = Api;
            double yg = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            double Rs = RsTotal;
            if (separatorTrain == SeparatorStage.TwoStages)
            {
                Rs = RsTotal + R3;
            }
            double T1 = Pow((Rs / yg), 0.7857);
            double T2 = Pow(10, 0.0009 * T) / Pow(10, 0.0148 * api);
            double PbL = 31.7648 * (T1 * T2);
            return PbL;
        }

        #endregion

        #region PbGlaso
        /// <summary>
        /// This routine calculates bubble point pressure using Glaso correlations.
        /// This implementation does not correct for the paraffinicity of the fluid, but corrects for the effect of non-hydrocarbon inpurities.
        /// If you want to correct for the paraffinicity, please use <see cref="GeneralGlasoPb(BlackOilInputs, double, double, double)"/>, you must provide the residual oil gravity from DLE test, and residual dead oil viscosity from DLE measured at reservoir temperature and atmospheric pressure.
        /// </summary>
        /// <remarks>
        /// Reference
        /// <para>Oistein Glaso "Generalized Pressure-Volume-Temperature Correlations." Journal of Petroleum Technology May 1980</para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>returns bubble point in psia</returns>
        public static double PbGlaso(double T, double Api, double RsTotal, double GasGravity,
                                    double condensateGravity, double Psp1, double Tsp1, 
                                    double Rs1, double Rs2, double Rs3, double Gg2, 
                                    double Gg3, double yCO2, double yH2S, double yN2,
                                    bool PbImpurityCorrection, SeparatorStage separatorTrain,
                                    double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double api = Api;
            double correlatingNumber;
            //double ygAve = AverageGasgravity.ygAve(input);
            double ygAve = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                Gg2, Gg3, GasGravity, separatorTrain);
            if (RsTotal <= 1500)//for typical black oil
            {
                correlatingNumber = Pow((RsTotal / ygAve), 0.816) * (Pow(T, 0.172) / Pow(api, 0.989));
            }
            else//for volatile oils systems
            {
                correlatingNumber = Pow((RsTotal / ygAve), 0.816) * (Pow(T, 0.130) / Pow(api, 0.989));
            }
            double LogPb = (1.7669 + (1.7447 * Log10(correlatingNumber)) - (0.30218 * Pow(Log10(correlatingNumber), 2)));
            double Pb = Pow(10, LogPb);
            if (PbImpurityCorrection)
            {
                double yco2 = yCO2, yH2s = yH2S, yn2 = yN2;
                double C02CorrectionFactor = 1.0 - 693.8 * yco2 * Pow(T, -1.553);
                double H2sCorrectionFactor = 1.0 - (0.9035 + 0.0015 * api) * yH2s + 0.019 * (45 - api) * Pow(yH2s, 2);
                double N2CorrectionFactor = 1.0 + ((-2.65e-4 * api + 5.5e-3) * T + (0.0931 * api - 0.8295)) * yn2 + ((1.954e-11 * Pow(api, 2)) * T + (0.027 * api - 2.366)) * Pow(yn2, 2);
                double correectionfactor = C02CorrectionFactor * H2sCorrectionFactor * N2CorrectionFactor;
                return Pb * correectionfactor;
            }
            return Cp1 * Pb + Cp2;
        }

        #endregion

        #region PbLasater
        /// <summary>
        /// This routine calculates the bubble point using Lasater correlations.
        ///  The routine implements Jacobson correlation to correct for Nitrogen effect on bubble point.
        /// </summary>
        /// <remarks>
        /// References
        /// <para>J.A. Lasater."Bubble Point Pressure Correlation." SPE 957-G. May 1958</para>
        /// <para>Weltest 200. Technical Description 2001A. Schlumberger.</para>
        /// <para>H.A.Jacobson."The Effect of Nitrogen on Reservoir Fluid Saturation Pressure."JCPT67-03-04</para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>Bubble point in Psia</returns>
        public static double PbLasater(double T, double Api, double RsTotal,
                                    double GasGravity, double condensateGravity, 
                                    double Psp1, double Tsp1, double Rs1, 
                                    double Rs2, double Rs3, double Gg2, 
                                    double Gg3, double yN2, bool PbImpurityCorrection, 
                                    SeparatorStage separatorTrain, double Cp1 = 1.0,
                                    double Cp2 = 0.0)
        {
            Func<double, double> ApiFunction = new Func<double, double>(mw => 271.7315 * Math.Exp(-0.0267 * mw) - 0.0955 * mw + 61.8264);
            double Mo = new double();
            Func<double, double> EffectiveMolecularWeightFunc = new Func<double, double>(yy =>
            {
                return Solvers.fzero(x => ApiFunction(x) - yy, new double[] { 100, 500 });
            }
            );

            // Mo = EffectiveMolecularWeightFunc(input.Api);
            //double ygAve = AverageGasgravity.ygAve(input);
            double ygAve = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                Gg2, Gg3, GasGravity, separatorTrain);
            if (Api <= 40)
            {
                Mo = 630 - 10 * Api;
            }
            else
            {
                Mo = 73110 * Math.Pow(Api, -1.562);
            }

            double yo = 141.5 / (131.5 + Api);
            double pb = 0.0;
            double gasMoleFraction = (RsTotal / 379.3) / ((RsTotal / 379.3) + ((350 * yo) / Mo));
            if (gasMoleFraction <= 0.6)
            {
                pb = ((0.679 * Exp(2.786 * gasMoleFraction)) - 0.323) * (T + 460) / ygAve;
            }
            else
            {
                pb = (8.26 * Pow(gasMoleFraction, 3.56) + 1.95) * (T + 460) / ygAve;
            }
            if (PbImpurityCorrection)
            {
                double N2factor = JacobsonNitrogenCorrectionfactor(yN2, T);
                return N2factor * pb;
            }
            return Cp1 * pb + Cp2;
        }

        #endregion

        #region PbPetroskyFashard
        /// <summary>
        /// This routine calculates Bubble point pressure using Petrosky and Farshard correlations
        /// </summary>
        /// <remarks>
        /// References
        /// <para>G.E.Petrosky Jr.,F.Farshard. "Pressure-Volume-Temperature Correlations for Gulf of Mexico Crude Oils." SPE 51395.  </para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>Bubble point in psia</returns>
        public static double PbPetroskyFarshard(double T, double Api, double RsTotal,
                                    double GasGravity, double condensateGravity,  
                                    double Psp1, double Tsp1, double Rs1, double Rs2,
                                    double Rs3, double Gg2, double Gg3, double yN2,
                                    bool PbImpurityCorrection, SeparatorStage separatorTrain,
                                    double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double ygAve = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                Gg2, Gg3, GasGravity, separatorTrain);
            double x = 0.00004561 * Pow(T, 1.3911) - 0.0007916 * Pow(Api, 1.541);
            double pb = 112.727 * (Pow(RsTotal, 0.577421) / Pow(ygAve, 0.8439) * Pow(10, x) - 12.34);
            if (PbImpurityCorrection)
            {
                return pb * JacobsonNitrogenCorrectionfactor(yN2, T);
            }
            return Cp1 * pb + Cp2;
        }

        #endregion

        #region PbStanding
        /// <summary>
        /// This routine calculates bubble point pressure using Standing correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// see <see cref="DryAndWetGasPVTProperties.PseudoReducedProperties.StandingPseudoTcPc(double)"/>
        /// </remarks>
        /// <param name="input"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static double PbStanding(double T, double Api, double RsTotal, 
                                    double GasGravity, double condensateGravity, 
                                    double Psp1, double Tsp1, double Rs1, double Rs2,
                                    double Rs3, double Gg2, double Gg3,  double yN2,
                                    bool PbImpurityCorrection, SeparatorStage separatorTrain,
                                    double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double ygAve = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                Gg2, Gg3, GasGravity, separatorTrain);
            double A = 0.00091 * T - 0.0125 * Api;
            double term1 = Math.Pow((RsTotal / ygAve), 0.83);
            double term2 = term1 * Math.Pow(10, A);
            double pb = 18.2 * (term2 - 1.4);
            if (PbImpurityCorrection)
            {
                double N2Cor = JacobsonNitrogenCorrectionfactor(yN2, T);
                return pb * N2Cor;
            }
            return Cp1 * pb + Cp2;
        }

        #endregion

        #region PbVazquezBeggs
        /// <summary>
        /// This routine calculates the Bubble point pressure from The Vazquez Beggs solution gas correlations
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>A.Vazquez, H.D.Beggs."Correlations for Fluid Physical Property Prediction." SPE 6719.  1980</para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <returns>bubble point in psia</returns>
        public static double PbVazquezBeggs(double T, double Api, double RsTotal,
                                    double GasGravity, double condensateGravity, 
                                    double Psp1, double Tsp1, double Rs1, double Rs2,
                                    double Rs3, double Gg2, double Gg3,  double yN2, 
                                    bool PbImpurityCorrection, SeparatorStage separatorTrain,
                                    double Cp1 = 1.0, double Cp2 = 0.0)
        {
            double ygAve = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                Gg2, Gg3, GasGravity, separatorTrain);
            double Psp = 14.7;// input.Psep;
            double Tsp = 60;// input.Tsep;
            double ygp = ygAve * (1 + (0.00005912 * Api * Tsp * Math.Log10(Psp / 114.7)));//ygp is the corrected gas gravity.
            //double ygp = input.Gasgravity * (1 + (0.00005912 * input.Api * Tsp * Math.Log10(Psp / 114.7)));//ygp is the corrected gas gravity.
            double pb = new double();

            if (Api < 30)
            {
                pb = Pow((27.642 * (RsTotal / ygp)) * Pow(10, (-11.172 * Api / (T + 460))), 0.914328);
            }
            else
            {
                pb = Pow((56.18 * (RsTotal / ygp)) * Pow(10, (-10.393 * Api / (T + 460))), 0.84246);
            }
            if (PbImpurityCorrection)
            {
                return pb * JacobsonNitrogenCorrectionfactor(yN2, T);
            }
            return Cp1 * pb + Cp2;
        }

        #endregion

        #endregion

        #region OilDensity

        /// <summary>
        /// This routine calculates density of oil using the material balance approach.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>Ahmed Tarek."Equations of state and PVT Analysis Application for improved Reservoir Modelling" Elsevier 2016. second Edition</para>
        /// <para>W.D.McCain "Properties of Petroleum Fluids." PennWell 1990. Second Edition</para>
        /// </remarks>
        /// <param name="Rs"></param>
        /// <param name="Bo"></param>
        /// <param name="Api"></param>
        /// <param name="GasGravity"></param>
        /// <param name="condensateGravity"></param>
        /// <param name="Psp1"></param>
        /// <param name="Tsp1"></param>
        /// <param name="Rs1"></param>
        /// <param name="Rs2"></param>
        /// <param name="Rs3"></param>
        /// <param name="Gg2"></param>
        /// <param name="Gg3"></param>
        /// <param name="separatorTrain"></param>
        /// <returns> density in lb/ft3</returns>
        public static double OilDensity(double Rs, double Bo, double Api, double GasGravity, 
                                    double condensateGravity, double Psp1, 
                                    double Tsp1, double Rs1, double Rs2, double Rs3,
                                    double Gg2, double Gg3, SeparatorStage separatorTrain)
        {
            double yo, rhoo;
            double ygave = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, 
                                            Rs3, Gg2, Gg3, GasGravity, separatorTrain);
            yo = 141.5 / (131.5 + Api);
            rhoo = (62.4 * yo + 0.0136 * Rs * ygave) / Bo;
            return rhoo;
        }
        #endregion

        #region Blackoil Correlations

        public static double sanity_check_on_Rs(double Rs, double GOR)
        {
            if (Rs < 0)
            {
                Rs = 0.0;
            }

            if (Rs > GOR)
            {
                Rs = GOR;
            }

            return Rs;
        }

        public static IEnumerable<double> BlackOilAlMarhoun(double P, double T, double API,
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1,
                                        double Tst, double Rs1, double Rs2, double Rs3,
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0, double Cp1 = 1.0,
                                        double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);

            Pb = PbAlMarhoun(T, API, GOR, gasgrav, API, Psp1, Tsp1, Rs1, Rs2, Rs3, 
                            Gg2, Gg3, yN2, pbImpurityCorrection, separatorTrain, Cp1, Cp2);

            Rs = RsAlMarhoun(P, T, Pb, API, gasgrav, Psp1, Tsp1, API, GOR,
                            Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain, Cp1, Cp2);
            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoAlMarhoun(P, T, Pb, Rs, API, GOR, gasgrav, API, Psep, Tsep, Psp1,
                            Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain, Cp1, Cp2,
                            Cp3, Cp4);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }
        public static IEnumerable<double> BlackOilDeGhetto(double P, double T, double API,
                                        double gasgrav,double GOR, double Psep, double Tsep,
                                        double Psp1, double Tsp1, double Rs1, double Rs2, 
                                        double Rs3, double R3, double Gg2, double Gg3,
                                        bool pbImpurityCorrection, SeparatorStage separatorTrain,
                                        double yN2 = 0.0, double Cp1 = 1.0, double Cp2 = 0.0,
                                        double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);

            Pb = PbDeGhetto(T, API, GOR, gasgrav, API, Psep, Tsep, Psp1, Tsp1, Rs1, 
                            Rs2, Rs3, R3, Gg2, Gg3, yN2, pbImpurityCorrection, 
                            separatorTrain, Cp1, Cp2);

            Rs = RsDeGhetto(P, T, API, gasgrav, Psep, Tsep, Cp1, Cp2);
            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoDeGhetto(P, T, Pb, Rs, API, gasgrav, GOR, Psep, Tsep, 
                            Cp1, Cp2, Cp3, Cp4);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }

        public static IEnumerable<double> BlackOilGlaso(double P, double T, double API, 
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1, 
                                        double Tst, double Rs1, double Rs2, double Rs3, 
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0, double Cp1 = 1.0,
                                        double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);
            Pb = PbGlaso(T, API, GOR, gasgrav, condensateGravity, Psp1, Tsp1,
                        Rs1, Rs2, Rs3, Gg2, Gg3, yCO2, yH2S, yN2, 
                        pbImpurityCorrection, separatorTrain, Cp1, Cp2);     
            
            Rs = RsGlaso(P, T, Pb, API, gasgrav, Psp1, Tsp1, condensateGravity, 
                        GOR, Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain, Cp1, Cp2);

            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoGlaso(P, T, Pb, Rs, API, GOR, gasgrav, condensateGravity,
                        Psep, Tsep, Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, 
                        separatorTrain, Cp1, Cp2, Cp3, Cp4);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }

        public static IEnumerable<double> BlackOilLasater(double P, double T, double API,
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1,
                                        double Tst, double Rs1, double Rs2, double Rs3,
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0, double Cp1 = 1.0,
                                        double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);
            Pb = PbLasater(T, API, GOR, gasgrav, API, Psp1, Tsp1, Rs1, Rs2, Rs3, 
                           Gg2, Gg3, yN2, pbImpurityCorrection, separatorTrain, Cp1, Cp2);

            Rs = RsLasater(P, T, Pb, API, gasgrav, Psp1, Tsp1, API, GOR, Rs1, Rs2,
                           Rs3, Gg2, Gg3, Tst, separatorTrain, Cp1, Cp2);

            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoLasater(P, T, Pb, Rs, API, GOR, gasgrav, API, Psep, Tsep, Psp1,
                           Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain, Cp1, 
                           Cp2, Cp3, Cp4);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }

        public static IEnumerable<double> BlackOilPetrosky(double P, double T, double API,
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1,
                                        double Tst, double Rs1, double Rs2, double Rs3,
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0, double Cp1 = 1.0,
                                        double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);
            Pb = PbPetroskyFarshard(T, API, GOR, gasgrav, API, Psp1, Tsp1, Rs1, Rs2, 
                                    Rs3, Gg2, Gg3, yN2, pbImpurityCorrection, 
                                    separatorTrain, Cp1, Cp2);

            Rs = RsPetroskyFarshad(P, T, Pb, API, gasgrav, Psp1, Tsp1, API, GOR, Rs1,
                                    Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain, Cp1, Cp2);

            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoPetroskyFarshard(P, T, Pb, Rs, API, GOR, gasgrav, API, Psep, Tsep,
                                Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain,
                                Cp1, Cp2, Cp3, Cp4);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }

        public static IEnumerable<double> BlackOilStanding(double P, double T, double API,
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1,
                                        double Tst, double Rs1, double Rs2, double Rs3,
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);
            Pb = PbStanding(T, API, GOR, gasgrav, API, Psp1, Tsp1, Rs1, Rs2, Rs3, 
                            Gg2, Gg3, yN2, pbImpurityCorrection, separatorTrain);

            Rs = RsStanding(P, T, Pb, API, gasgrav, Psp1, Tsp1, API, GOR, Rs1, Rs2, 
                            Rs3, Gg2, Gg3, Tst, separatorTrain);

            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoStanding(P, T, Pb, Rs, API, GOR, gasgrav, API, Psep, Tsep, Psp1, 
                            Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }

        public static IEnumerable<double> BlackOilVazquezBeggs(double P, double T, double API,
                                        double gasgrav, double condensateGravity, double GOR,
                                        double Psep, double Tsep, double Psp1, double Tsp1,
                                        double Tst, double Rs1, double Rs2, double Rs3,
                                        double Gg2, double Gg3, bool pbImpurityCorrection,
                                        SeparatorStage separatorTrain, double yN2 = 0.0,
                                        double yCO2 = 0.0, double yH2S = 0.0)
        {
            double Rs, Pb, Bo, Rp, co;
            Rs = Rp = GOR;
            double gasgrav100 = gasgrav100VazquezAndBeggs(API, gasgrav, Psep, Tsep);
            Pb = PbVazquezBeggs(T, API, GOR, gasgrav, API, Psp1, Tsp1, Rs1, Rs2, Rs3,
                                Gg2, Gg3, yN2, pbImpurityCorrection, separatorTrain);

            Rs = RsVazquezBeggs(P, T, Pb, API, gasgrav, Psep, Tsep, Psp1, Tsp1, API, 
                                GOR, Rs1, Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain);

            Rs = sanity_check_on_Rs(Rs, GOR);
            co = CoVazquezAndBeggs(P, T, API, gasgrav100, Rs);
            Bo = BoVazquezBeggs(P, T, Pb, Rs, API, GOR, gasgrav, API, Psep, Tsep, Psp1,
                                Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3, separatorTrain);

            yield return Pb;
            yield return Rs;
            yield return Bo;
            yield return co;
        }
        #endregion

        //=====================================================================
        // PHYSICAL PROPERTIES OF OIL
        //=====================================================================

        public static double gasgrav_dis(double API, double gasgrav_tot, double Rs)
        {
            double grav = (API + 12.5) / 50.0 - 3.57157e-6 * API * Rs;
            if (grav < 0.56) grav = 0.56;
            if (grav < gasgrav_tot) grav = gasgrav_tot;
            return grav;
        }

        public static double gasgrav_free(double gasgrav_tot, double gasgrav_dis,
                                          double Rp, double Rs)
        {
            // Rp = produced gas/oil ratio = calculated as the gas solubility
            // at or above the bubblepoint pressure
            double grav = (Rp * gasgrav_tot - Rs * gasgrav_dis) / (Rp - Rs);
            if (grav < 0.56) grav = 0.56;
            if (grav > gasgrav_tot) grav = gasgrav_tot;

            return grav;
        }

        #region Oil density
        public static double sat_oil_density(double API, double gasgrav_dis,
                                             double Rs, double Bo)
        {
            // This applies to oil below or at bubble point
            double oilgrav = 141.5 / (API + 131.5);
            double rho = (62.4 * oilgrav + 0.0136 * Rs * gasgrav_dis) / Bo;
            return rho;
        }

        public static double sat_oil_density_above_pb(double p, double API,
                                            double gasgrav_tot, double Rp, double pb,
                                            double Bob, double co)
        {
            // gasgrav_tot = total separator gas gravity = gasgrav
            // Rp = Rs at bubble point, which we assume = GOR
            // Bob = Bo at bubble point, pb
            // co = isothermal compressibility
            double rho_pb = sat_oil_density(API, gasgrav_tot, Rp, Bob);
            double rho = rho_pb * Math.Exp(co * (p - pb));
            return rho;
        }

        public static double oil_density(double p, double API, double gasgrav_tot,
                                         double gasgravd, double Rs, double pb,
                                         double Bob, double co)
        {
            // gasgravd = dissolved gas gravity
            // gasgravf = free gas gravity
            double oil_den;
            if (p < pb) // saturated oil
            {
                oil_den = sat_oil_density(API, gasgravd, Rs, Bob);
            }
            else
            {
                oil_den = sat_oil_density_above_pb(p, API, gasgrav_tot, Rs, pb, Bob, co);
            }

            return oil_den;
        }

        #endregion

        #region Oil Viscosity
        
        public static double OilViscosity(double P, double T, double Pb, double rs, double RsTotal, double Api, OilViscosityCorr oilViscCorr, double tp = 0)
        {
            double muO = 0;
            if(oilViscCorr == OilViscosityCorr.Beal_et_al)
            {
                muO = BealViscosity(P, T, Pb, rs, RsTotal, Api);
            }
            else if (oilViscCorr == OilViscosityCorr.BeggsRobinson)
            {
                muO = BeggRobinsonViscosity(P, T, Pb, rs, RsTotal, Api);
            }
            else if(oilViscCorr == OilViscosityCorr.Bergman_Sutton)
            {
                muO = BergemanSuttonViscosity(P, T, Pb, rs, Api, RsTotal);
            }
            else if(oilViscCorr == OilViscosityCorr.DeGhetto)
            {
                muO = DeGhettoViscosity(P, T, Pb, rs, Api, RsTotal);
            }
            else if(oilViscCorr == OilViscosityCorr.Egbogah_et_al)
            {
                muO = EgbogahViscosity(P, T, Pb, rs, Api, RsTotal, tp);
            }
            else if(oilViscCorr == OilViscosityCorr.Petrosky_et_al)
            {
                muO = PetroskyFarshardViscosity(P, T, Pb, rs, Api, RsTotal);
            }

            return muO;
        }

        #region Beal Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Beal's correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>C.Beal."The Viscosity of Air,Water,Natural Gas, Crude Oils and Its Associated Gases at Oil Field Temperatures and Pressures." 1946</para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double BealViscosity(double P, double T, double Pb, double rs,
                                    double RsTotal, double Api, double Cp1 = 1.0,
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double muoD = BealDeadOilViscosity(Api, T);
            if (P < Pb)
            {
                double muoS = BealSaturatedViscosity(muoD, rs);
                return Cp1 * muoS + Cp2;
            }
            else
            {

                double muopb = BealSaturatedViscosity(muoD, RsTotal);
                double muoU = BealUnderSatViscosity(P, Pb, muopb);
                return Cp3 * muoU + Cp4;
            }
        }
        #endregion

        #region Beggs and Robinson Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Beggs and Robinson correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>H.D.Beggs, J.R.Robinson"Estimating the viscosity of Crude Oil Systems." SPE 5434</para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double BeggRobinsonViscosity(double P, double T, double Pb, double rs,
                                            double RsTotal, double Api, double Cp1 = 1.0,
                                            double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            double muoD = BeggRobinsonDeadOilViscosity(Api, T);
            if (P < Pb)
            {
                double muoS = BeggsRobinsonSaturatedViscosity(muoD, rs);
                return Cp1 * muoS + Cp2;
            }
            else
            {
                double muopb = BeggsRobinsonSaturatedViscosity(muoD, RsTotal);
                double muoU = BeggsRobinsonUnderSatViscosity(P, Pb, muopb);
                return Cp3 * muoU + Cp4;
            }
        }
        #endregion

        #region Bergman and Sutton Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Bergman and Sutton correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>D.F.Bergman, R.P.Sutton. "An Update to Viscosity Correlations for Gas-Saturated Crude Oils."a SPE 110195</para>
        /// <para>D.F.Bergman,R.P.Sutton."A Consistent and Accurate Dead-Oil_Viscosity Method" SPE 110194</para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double BergemanSuttonViscosity(double P, double T, double Pb, double rs,
                                                double Api, double RsTotal, double Cp1 = 1.0,
                                                double Cp2 = 0.0, double Cp3 = 1.0, 
                                                double Cp4 = 0.0)
        {
            double muoD = BergManSuttonDeadOilVisc(Api, T);
            if (P < Pb)
            {
                double muoS = BergmanSuttonSaturatedViscosity(muoD, rs);
                return Cp1 * muoS + Cp2;
            }
            else
            {
                double muopb = BergmanSuttonSaturatedViscosity(muoD, RsTotal);
                double muoU = BergmanSuttonUnderSatVisc(P, Pb, muopb);
                return Cp3 * muoU + Cp4;
            }
        }

        #endregion

        #region De Ghetto Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Beal's correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>see <see cref="BubblePoint.DeGhettoPb(BlackOilInputs, double)"/></para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double DeGhettoViscosity(double P, double T, double Pb, double rs, 
                                        double Api, double RsTotal, double Cp1 = 1.0, 
                                        double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            apiForDegetto = Api;
            double muoD = DeGhettoDeadOilViscosity(Api, T);
            if (P < Pb)
            {
                double muoS = DeghettoSaturatedViscosity(muoD, rs, Api);
                return Cp1 * muoS + Cp2;
            }
            else
            {
                double muopb = DeghettoSaturatedViscosity(muoD, RsTotal, Api);
                double muoU = DeGhettoUnderSatViscosity(P, Pb, Api, muopb, muoD);
                return Cp3 * muoU + Cp4;
            }
        }
        #endregion

        #region Egbogah and Ng Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Beal's correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>J.T.H.Ng, E.O.Egbogah."An Improved Temperature-Viscosity Correlations for Crude Oil Systems" 1983.</para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double EgbogahViscosity(double P, double T, double Pb, double rs, 
                                    double Api, double RsTotal, double Tp, double Cp1 = 1.0,
                                    double Cp2 = 0.0, double Cp3 = 1.0, double Cp4 = 0.0)
        {
            Tf = Tp;
            double muoD = EgbogahDeadOilViscosity(Api, T, Tf);
            if (P < Pb)
            {
                double muoS = EgbogahSaturatedViscosity(muoD, rs);
                return Cp1 * muoS + Cp2;
            }
            else
            {

                double muopb = EgbogahSaturatedViscosity(muoD, RsTotal);
                double muoU = EgbogahUnderSatViscosity(P, Pb, muopb);
                return Cp3 * muoU + Cp4;
            }
        }
        #endregion

        #region Petrosky and Farshard Viscosity
        /// <summary>
        /// This routine calculates oil viscosity using Petrosky and Farshard correlations.
        /// </summary>
        /// <remarks>
        /// References:
        /// <para>G.E.Petrosky, F.F.Farshard,"Viscosity Correlations for Gulf of Mexico Crude Oils." SPE 29468</para>
        /// <para>Also see <see cref="OilViscosity"/></para>
        /// </remarks>
        /// <param name="input"><see cref="BlackOilInputs"/></param>
        /// <param name="P">pressure in psia</param>
        /// <param name="T">temperature in fahrenheit</param>
        /// <param name="Pb">bubble point pressure in psia</param>
        /// <param name="rs">solution gas-oil ratio in scf/stb</param>
        /// <returns>returns viscosity in cP</returns>
        public static double PetroskyFarshardViscosity(double P, double T, double Pb, double rs,
                                                double Api, double RsTotal, double Cp1 = 1.0,
                                                double Cp2 = 0.0, double Cp3 = 1.0, 
                                                double Cp4 = 0.0)
        {
            double muoD = PetroskyDeadOilViscosity(Api, T);
            if (P < Pb)
            {
                double muoS = PetroskySaturatedViscosity(muoD, rs);
                return Cp1 * muoS + Cp2;
            }
            else
            {
                double muopb = PetroskySaturatedViscosity(muoD, RsTotal);
                double muoU = PetroskyUnderSatViscosity(P, Pb, muopb);
                return Cp3 * muoU + Cp4;
            }
        }
        #endregion

        #region Dead Oil Viscosity

        internal static double BealDeadOilViscosity(double api, double T)
        {
            double A = Pow(10, (0.43 + (8.33 / api)));
            double muoD = (0.32 + (1.8e7 / Pow(api, 4.53))) * Pow((360 / (T + 200)), A);
            return muoD;
        }

        internal static double BeggRobinsonDeadOilViscosity(double api, double T)
        {
            double A = Pow(10, (3.0324 - 0.02023 * api));
            double muoD = Pow(10, (A * Pow(T, -1.163))) - 1;
            return muoD;
        }
        internal static double BergManSuttonDeadOilVisc(double api, double T)
        {
            double Yo = 141.5 / (131.5 + api);
            double rho60 = 0.999012 * Yo;
            List<double> alpCoefs = new List<double> { 2.5042e-4, 8.302e-5 };
            double alp60 = (alpCoefs[0] + alpCoefs[1] * rho60) / Pow(rho60, 2);
            double Tb = 1748 - 30.05 * api + 0.3451 * Pow(api, 2) - 0.002416 * Pow(api, 3) + 7.397e-6 * Pow(api, 4);
            double Tc1 = 0.533272 + 1.9101e-4 * Tb + 7.79681e-8 * Pow(Tb, 2) - 2.84376e-11 * Pow(Tb, 3) + 9.59468e27 * Pow(Tb, -13);
            double Tc = Tb * Pow(Tc1, -1);
            double a = 1 - (Tb / Tc);
            double v2Term = 2.40219 - 9.59688 * a + 3.45656 * Pow(a, 2) - 143.632 * Pow(a, 4);
            double v2 = Exp(v2Term) + 0.152995;
            double V1Term = 0.701254 + 1.38359 * Log(v2) + 0.103604 * Pow(Log(v2), 2);
            double V1 = Exp(V1Term);
            double YoPrime = 0.843593 - 0.128624 * a - 3.36159 * Pow(a, 3) - 13749.5 * Pow(a, 12);
            double deltaY = Yo - YoPrime;
            double X = Abs(2.68316 - 62.0863 / Sqrt(Tb));
            double f2 = X * deltaY - (47.6033 * Pow(deltaY, 2) / Sqrt(Tb));
            double V210Term = Log(v2 + 232.442 / Tb) * (Pow((1 + 2 * f2) / (1 - 2 * f2), 2));
            double V210 = Exp(V210Term) - 232.442 / Tb;
            double f1 = 0.980633 * X * deltaY - (47.6033 * Pow(deltaY, 2)) / Sqrt(Tb);
            double V100Term = Log(V1 + 232.442 / Tb) * (Pow((1 + 2 * f1) / (1 - 2 * f1), 2));
            double V100 = Exp(V100Term) - 232.442 / Tb;
            double rho100 = rho60 * vcfFunction(100);
            double rho210 = rho60 * vcfFunction(210);
            double muoD100 = V100 * rho100;
            double muoD210 = V210 * rho210;
            double B = (Log(Log(muoD210 + 0.974)) - Log(Log(muoD100 + 0.974))) / (Log(512.7) - Log(402.7));
            double T1 = Exp(Log(Log(muoD100 + 0.974)) + B * (Log(T + 302.7) - Log(402.7)));
            double muoD = Exp(T1) - 0.974;
            double vcfFunction(double Temp)
            {
                double DeltaT = Temp - 60;
                double Vcf = Exp(-alp60 * DeltaT * (1.0 + 0.8 * alp60 * DeltaT));
                return Vcf;
            }
            return muoD;
        }
        internal static double DeGhettoDeadOilViscosity(double api, double T)
        {
            double visc;
            double LogLogTerm;
            double logTerm;
            if (api < 10)
            {
                LogLogTerm = 1.90296 - 0.012619 * api - 0.61748 * Log10(T);
                logTerm = Pow(10, LogLogTerm);
                visc = Pow(10, logTerm) - 1;
            }
            else if (api < 22.3 && api >= 10)
            {
                LogLogTerm = 2.06492 - 0.0179 * api - 0.70226 * Log10(T);
                logTerm = Pow(10, LogLogTerm);
                visc = Pow(10, logTerm) - 1;
            }
            else if (api >= 22.3 && api < 31.1)
            {
                double exponent = 12.5428 * Log10(T) - 45.7874;
                visc = 220.15e9 * Pow(T, -3.5560) * Pow(Log10(api), exponent);
            }
            else
            {
                LogLogTerm = 1.67083 - 0.017628 * api - 0.61304 * Log10(T);
                logTerm = Pow(10, LogLogTerm);
                visc = Pow(10, logTerm) - 1;
            }
            return visc;
        }

        internal static double EgbogahDeadOilViscosity(double api, double T, double Tf)
        {
            double visc;
            if (Tf != 0.0)
            {
                T = (T - 32) / 1.8;
                Tf = (Tf - 32) / 1.8;
                double yo = 141.5 / (api + 131.5);
                double logLogTerm = -1.7095 - 0.0087917 * Tf + 2.7523 * yo + (-1.2943 + 0.0033214 * Tf + 0.958195 * yo) * Log10(T - Tf);
                double logterm = Pow(10, logLogTerm);
                visc = Pow(10, logterm) - 1.0;
            }
            else
            {
                double logMuoTerm = 1.8653 - 0.025086 * api - 0.5644 * Log10(T);
                double muoTerm = Pow(10, logMuoTerm);
                visc = Pow(10, muoTerm) - 1.0;
            }
            return visc;
        }

        internal static double PetroskyDeadOilViscosity(double api, double T)
        {
            double x = 4.59388 * Math.Log10(T) - 22.82792;
            double muoD = 23511000 * Math.Pow(T, -2.10255) * Math.Pow((Math.Log10(api)), x);
            return muoD;
        }

        #endregion
        #region Saturated Viscosity
        internal static double BealSaturatedViscosity(double muoD, double Rs)
        {
            double A1 = Math.Pow(10, (-7.4 * Math.Pow(10, -4) * Rs + 2.2 * Math.Pow(10, -7) * Math.Pow(Rs, 2)));
            double A2 = 0.68 / Math.Pow(10, (8.62 * Math.Pow(10, -5) * Rs)) + 0.25 / Math.Pow(10, (1.1 * Math.Pow(10, -3) * Rs)) + 0.062 / Math.Pow(10, (3.74 * Math.Pow(10, -3) * Rs));
            double muo = A1 * Math.Pow(muoD, A2);
            return muo;
        }

        internal static double BeggsRobinsonSaturatedViscosity(double muoD, double Rs)
        {
            double A1 = 10.715 * Math.Pow((Rs + 100), -0.515);
            double A2 = 5.44 * Math.Pow((Rs + 150), -0.338);
            double muo = A1 * Math.Pow(muoD, A2);
            return muo;
        }

        internal static double EgbogahSaturatedViscosity(double muoD, double Rs)
        {
            return BeggsRobinsonSaturatedViscosity(muoD, Rs);
        }

        internal static double PetroskySaturatedViscosity(double muoD, double Rs)
        {
            double A = 0.1651 + 0.6165 * Math.Pow(10, (-0.00060866 * Rs));
            double B = 0.5131 + 0.5109 * Math.Pow(10, (-0.0011831 * Rs));
            double muo = A * Math.Pow(muoD, B);
            return muo;
        }

        internal static double BergmanSuttonSaturatedViscosity(double muoD, double Rs)
        {
            double a1 = 344.198;
            double a2 = 0.855344;
            double a3 = 0.382322;
            double a4 = 567.953;
            double a5 = 0.819326;
            double A = 1.0 / (1.0 + Pow((Rs / a1), a2));
            double B = a3 + (1.0 - a3) / (1.0 + Pow((Rs / a4), a5));
            double MuoS = A * Pow(muoD, B);
            return MuoS;
        }

        internal static double DeghettoSaturatedViscosity(double muoD, double Rs, double api)
        {
            double y;
            double FT1;
            double FT2;
            double F;
            double muo;
            if (api < 10)
            {
                y = Pow(10, -0.00081 * Rs);
                FT1 = -0.0335 + 1.0785 * Pow(10, -0.000845 * Rs);
                FT2 = Pow(muoD, (0.5798 + 0.3432 * y));
                F = FT1 * FT2;
                muo = 2.3945 + 0.8927 * F + 0.001567 * Pow(F, 2);
            }
            else if (api < 22.3 && api >= 10)
            {
                y = Pow(10, -0.00081 * Rs);
                FT1 = 0.2478 + 0.6114 * Pow(10, -0.000845 * Rs);
                FT2 = Pow(muoD, (0.4731 + 0.5158 * y));
                F = FT1 * FT2;
                muo = -0.6311 + 1.078 * F - 0.003653 * Pow(F, 2);
            }
            else if (api >= 22.3 && api < 31.1)
            {
                y = Pow(10, -0.00081 * Rs);
                FT1 = 0.2038 + 0.8591 * Pow(10, -0.000845 * Rs);
                FT2 = Pow(muoD, (0.3855 + 0.5664 * y));
                F = FT1 * FT2;
                muo = 0.0132 + 0.9821 * F - 0.005215 * Pow(F, 2);
            }
            else
            {
                double muoT1 = 25.1921 * Pow((Rs + 100), -0.6487);
                double muoT2 = Pow(muoD, (2.7516 * Pow((Rs + 100), -0.2135)));
                muo = muoT1 * muoT2;
            }
            return muo;
        }
        #endregion
        #region Under Saturated Viscosity
        private static double BealUnderSatViscosity(double P, double Pb, double muoPb)
        {
            double muo = muoPb + 0.001 * (P - Pb) * (0.024 * Pow(muoPb, 1.6) + 0.038 * Pow(muoPb, 0.56));
            return muo;
        }
        private static double BeggsRobinsonUnderSatViscosity(double P, double Pb, double muoPb)
        {
            double A = 2.6 * Math.Pow(P, 1.187) * Math.Exp(-11.513 - 8.98 * Math.Pow(10, -5) * P);
            double muo = muoPb * Math.Pow((P / Pb), A);
            return muo;
        }
        private static double EgbogahUnderSatViscosity(double P, double Pb, double muoPb)
        {
            return BeggsRobinsonUnderSatViscosity(P, Pb, muoPb);
        }
        private static double BergmanSuttonUnderSatVisc(double P, double Pb, double MuoPb)
        {
            double alp = 6.5698e-7 * Log(Pow(MuoPb, 2)) - 1.48211e-5 * Log(MuoPb) + 2.27877e-4;
            double beta = 2.24623e-2 * Log(MuoPb) + 0.873204;
            double Pdiff = P - Pb;
            double muoUs = MuoPb * Exp(alp * Pow(Pdiff, beta));
            return muoUs;
        }
        private static double DeGhettoUnderSatViscosity(double P, double Pb, double api, double MuoPb, double MuoD)
        {
            double visc;
            if (api < 10)
            {
                visc = MuoPb - ((1.0 * (P / Pb)) * ((Pow(10, -2.19) * Pow(MuoD, 1.055) * Pow(Pb, 0.3132)) / (Pow(10, 0.0099 * api))));
            }
            else if (api < 22.3 && api >= 10)
            {
                visc = 0.9886 * MuoPb + 0.002763 * (P - Pb) * (-0.01153 * Pow(MuoPb, 1.7933) + 0.0316 * Pow(MuoPb, 1.5939));
            }
            else if (api >= 22.3 && api < 31.1)
            {
                visc = MuoPb - ((1.0 * (P / Pb)) * ((Pow(10, -3.8055) * Pow(MuoD, 1.4131) * Pow(Pb, 0.6957)) / (Pow(10, -0.00288 * api))));
            }
            else
            {
                visc = MuoPb - ((1.0 * (P / Pb)) * ((Pow(10, -2.488) * Pow(MuoD, 0.9036) * Pow(Pb, 0.6151)) / (Pow(10, 0.01976 * api))));
            }
            return visc;
        }
        private static double PetroskyUnderSatViscosity(double P, double Pb, double muoB)
        {
            double A = -1.0146 + 1.3322 * Math.Log10(muoB) - 0.4876 * Math.Pow((Math.Log10(muoB)), 2)
              - 1.15036 * Math.Pow((Math.Log10(muoB)), 3);
            double muo = muoB + 0.0013449 * (P - Pb) * Math.Pow(10, A);
            return muo;
        }

        #endregion

        #endregion
        public static double gas_oil_interfacial_tension(double p, double T, double API)
        {
            double sigma68 = 39 - 0.2571 * API;     // dead oil tension at 68 degF, dynes/cm
            double sigma100 = 37.5 - 0.2571 * API;  // dead oil tension at 100 degF, dynes/cm
            double sigma;
            if (T <= 68.0)
            {
                sigma = sigma68;
            }
            else if (T >= 100.0)
            {
                sigma = sigma100;
            }
            else // 68.0 < T < 100.0 degF
            {
                sigma = 68.0 - (T - 68.0) * (sigma68 - sigma100) / 32.0;
            }

            double sigma_corr;
            if (p < 3977.0)
            {
                double C = 1.0 - 0.024 * Math.Pow(p, 0.45);  // correction factor for gas going into solution
                sigma_corr = C * sigma;
            }
            else // p >= 3977.0
            {
                sigma_corr = 1.0;
            }

            return sigma_corr;
        }

    }
}
