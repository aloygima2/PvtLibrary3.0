using System;
using System.Collections.Generic;
using PvtLibrary3.Common;
using static System.Math;

namespace PvtLibrary3.BlackOil
{
    public class Gas
    {
        /// <summary>
        /// this routine estimates the stock tank GOR using Valko,et al correlation.
        /// P.P.Valko, W.D.McCain jr. "Reservoir oil bubblepoint pressures revisited: gas-oil ratios and surface gas specific gravities."
        /// Journal of Petroleum Science and Engineering 37 (2003) 153-169
        /// </summary>
        /// <param name="Psp1"></param>
        /// <param name="Tsp1"></param>
        /// <param name="condensateGravity"></param>
        /// <returns></returns>
        public static double RsTankEstimation(double Psp1, double Tsp1, double condensateGravity)
        {
            double LnPsp = Log(Psp1);
            double LnTsp = Log(Tsp1);
            double api = condensateGravity;
            double Co = -8.005 + 2.7 * LnPsp - 0.161 * Pow(LnPsp, 2);
            double C1 = 1.224 - 0.5 * LnTsp;
            double C2 = -1.587 + 0.0441 * api - 2.29e-5 * Pow(api, 2);
            double zn = Co + C1 + C2;
            double lnRst = 3.955 + 0.83 * zn - 0.024 * Pow(zn, 2) + 0.075 * Pow(zn, 3);
            double Rst = Exp(lnRst);
            return Rst;
        }

        /// <summary>
        /// Calculates the average specific gravity of the surface gas.
        /// It is not implemented for <see cref="SeparatorStage.ThreeStage"/>
        /// </summary>
        /// <param name="separatorTrain"><see cref="SeparatorStage"/></param>
        /// <returns></returns>
        public static double AverageGasGravity(double Psp1, double Tsp1, double condensateGravity, 
                                               double Rs1, double Rs2, double Rs3, double Gg2, double Gg3, 
                                               double GasGravity, SeparatorStage separatorTrain)
        {
            double Rsp = Rs1;
            double ygsp = GasGravity;
            double Rst = 0.0;
            double ygst = 0.0;
            double Rtotal = 0.0;
            double yg = 0.0;
            switch (separatorTrain)
            {
                case SeparatorStage.SingleStage:
                    return GasGravity;
                case SeparatorStage.TwoStages:
                    if (Gg3 == 0.0 || Rs3 == 0.0)
                    {
                        Rst = RsTankEstimation(Psp1, Tsp1, condensateGravity);
                        ygst = ystEst(Psp1, Tsp1, Rs1, GasGravity, condensateGravity);
                        Rtotal = Rsp + Rst;
                        yg = (ygsp * Rsp + ygst * Rst) / Rtotal;
                    }
                    else
                    {
                        double num = GasGravity * Rs1 + Rs3 * Gg3;
                        double denom = Rs1 + Rs3;
                        yg = num / denom;

                    }

                    break;
                case SeparatorStage.ThreeStage:

                    if (Rs2 == 0 || Rs3 == 0 || Gg2 == 0 || Gg3 == 0)
                    {
                        Rtotal = 1.1618 * Rsp;
                        yg = 1.066 * ygsp;
                    }
                    else
                    {
                        double num = GasGravity * Rs1 + Rs2 * Gg2 + Rs3 * Gg3;
                        double denom = Rs1 + Rs2 + Rs3;
                        yg = num / denom;
                    }
                    break;
                default:
                    break;
            }
            return yg;
        }

        /// <summary>
        /// this routine estimates the stock tank gas specific gravity using Valko,et al correlation.
        ///<see cref="RstankEstimation(GasPvtInputs)"/>
        /// </summary>
        /// <param name="Input"><see cref="GasPvtInputs"/></param>
        /// <returns></returns>
        public static double ystEst(double Psp1, double Tsp1, double Rs1, 
                                    double gasGravity, double condensateGravity)
        {
            double LnPsp = Log(Psp1);
            double Tsp = Tsp1;
            double api = condensateGravity;
            double ygsp = gasGravity;
            double lnRsp = Log(Rs1);
            double z1 = -17.275 + 7.9597 * LnPsp - 1.1013 * Pow(LnPsp, 2) + 2.773e-2 * Pow(LnPsp, 3) + 3.2287e-3 * Pow(LnPsp, 4);
            double z2 = -0.3354 - 0.3346 * lnRsp + 0.1956 * Pow(lnRsp, 2) - 3.4374e-2 * Pow(lnRsp, 3) + 2.08e-3 * Pow(lnRsp, 4);
            double z3 = 3.705 - 0.4273 * api + 1.818e-2 * Pow(api, 2) - 3.459e-4 * Pow(api, 3) + 2.505e-6 * Pow(api, 4);
            double z4 = -155.52 + 626.61 * ygsp - 957.38 * Pow(ygsp, 2) + 647.57 * Pow(ygsp, 3) - 163.26 * Pow(ygsp, 4);
            double z5 = 2.085 - 7.097e-2 * Tsp + 9.859e-4 * Pow(Tsp, 2) - 6.312e-6 * Pow(Tsp, 3) + 1.4e-8 * Pow(Tsp, 4);
            double zn = z1 + z2 + z3 + z4 + z5;
            double yst = 1.219 + 0.198 * zn + 0.0845 * Pow(zn, 2) + 0.03 * Pow(zn, 3) + 0.003 * Pow(zn, 4);
            return yst;
        }

        public static double GasDensity(double p, double T, double Z, double gasgrav)
        {
            double T_R = T + 460.0;
            double rhog = 2.7 * gasgrav * p / (Z * T_R);
            return rhog;
        }

        #region Gas Formation volume factor

        public static double BgFromGasLaw(double p, double T, double Z, double Cp1 = 1.0,
                                        double Cp2 = 0.0)
        {
            // [T] = degF
            // [bg] = ft3/scf
            double T_R = T + 460.0;
            double bg = 0.0283 * Z * T_R / p;
            return Cp1 * bg + Cp2;
        }

        #endregion

        public static double GasMolecularWeight(double gasgrav)
        {
            double Mg = 28.97 * gasgrav;
            return Mg;
        }

        #region Gas Viscosity

        //From Prodsys
        public static double GasViscosityLee(double T, double gas_density, double Mg)
        {
            // cannot be used for sour gases
            // Mg = apparent molecular weight of the gas mixture
            // [gas_density] = lbm/ft3, [T] = degF, reservoir temperature
            double T_R = T + 460.0; // convert to degree Rankine
            double K = (9.4 + 0.02 * Mg) * Math.Pow(T_R, 1.5) / (209 + 19 * Mg + T_R);
            double X = 3.5 + (986 / T_R) + 0.01 * Mg;
            double Y = 2.4 - 0.2 * X;
            double mu_g = 1e-4 * K * Math.Exp(X * Math.Pow((gas_density / 62.4), Y));
            return mu_g;
        }

        #endregion

        #region Z factor
        public static double ZFactorBeggsAndBrill(double ppr, double Tpr)
        {
            double A = 1.39 * Math.Pow((Tpr - 0.92), 0.5) - 0.36 * Tpr - 0.101;
            double B = ppr * (0.62 - 0.23 * Tpr) + Math.Pow(ppr, 2) * (0.066 / (Tpr - 0.86) - 0.037) + 0.32 * Math.Pow(ppr, 6) / Math.Exp(20.723 * (Tpr - 1));
            double C = 0.132 - 0.32 * Math.Log10(Tpr);
            double D = Math.Exp(0.715 - 1.128 * Tpr + 0.42 * Math.Pow(Tpr, 2));
            double z = A + (1 - A) * Math.Exp(-B) + C * Math.Pow(ppr, D);

            return z;
        }

        public static double ZFactorHallAndYarborough(double ppr, double Tpr)
        {
            if (Tpr < 1.01) Tpr = 1.0;
            double rT = 1.0 / Tpr;

            // evaluate temperature dependent terms
            double A = 0.06125 * rT * Math.Exp(-1.2 * Math.Pow((1.0 - rT), 2));
            double B = rT * (14.76 - 9.76 * rT + 4.58 * rT * rT);
            double C = rT * (90.7 - 242.2 * rT + 42.4 * rT * rT);
            double D = 2.18 + 2.82 * rT;

            // compute reduced density rhor using the Newton-Raphson method

            // Func<string, string> selector = str => str.ToUpper();

            Func<double, double> f = (x) =>
            {
                var res = -A * ppr + (x + x * x + Math.Pow(x, 3) - Math.Pow(x, 4)) / Math.Pow((1.0 - x), 3) - B * x * x + C * Math.Pow(x, D);
                return res;
            };

            Func<double, double> dfdy = (x) =>
            {
                var df = (1.0 + 4.0 * x + 4.0 * x * x - 4.0 * Math.Pow(x, 3) + Math.Pow(x, 4)) / Math.Pow((1.0 - x), 4) - 2.0 * B * x + D * C * Math.Pow(x, (D - 1.0));
                return df;
            };

            double y = 0.001;  // y = reduced density
            bool converged = false;
            for (int i = 0; i < 25; ++i)
            {
                if (y > 1.0) y = 0.6;
                double f_val = f(y);
                if (Math.Abs(f_val) <= 1e-8)
                {
                    converged = true;
                    //Console.WriteLine("Z factor calculation converged!");
                    break;
                }
                double df_val = dfdy(y);
                y = y - f_val / df_val;
            }

            double z;
            if (converged)
            {
                z = A * ppr / y;
            }
            else // use explicit beggs_brill approximation for z
            {
                Console.WriteLine("Unconverged - using beggs_brill explicit approximation for zfactor");
                z = ZFactorBeggsAndBrill(ppr, Tpr);
            }

            return z;
        }

        #endregion

        public static IEnumerable<double> PseudocriticalPTStanding(double gasgrav, GasType gastype = GasType.Natural, double yCO2 = 0.0, double yH2S = 0.0)
        {
            // Tpc, ppc are in degR abd psia respectively
            double Tpc;
            double ppc;
            if (gastype == GasType.Natural)
            {
                Tpc = 168.0 + 325.0 * gasgrav - 12.5 * Math.Pow(gasgrav, 2);
                ppc = 677.0 + 15.0 * gasgrav - 37.5 * Math.Pow(gasgrav, 2);
            }

            else if (gastype == GasType.Condensate)
            {
                Tpc = 187.0 + 330 * gasgrav - 71.5 * Math.Pow(gasgrav, 2);
                ppc = 706.0 - 51.7 * gasgrav - 11.1 * Math.Pow(gasgrav, 2);
            }
            else
            {
                throw new Exception($"Unknown gastype {gastype}!");
            }

            return CorrectionForNonhydrocarbonWichertAziz(ppc, Tpc, yCO2, yH2S);
        }

        public static IEnumerable<double> PseudocriticalPTSutton(double gasgrav, GasType gastype = GasType.Natural, double yCO2 = 0.0, double yH2S = 0.0)
        {
            // Tpc, ppc are in degR abd psia respectively
            double Tpc;
            double ppc;
            if (gastype == GasType.Natural)
            {
                Tpc = 120.1 + 425.0 * gasgrav - 62.9 * Math.Pow(gasgrav, 2);
                ppc = 671.1 + 14.0 * gasgrav - 34.3 * Math.Pow(gasgrav, 2);
            }
            else if (gastype == GasType.Condensate)
            {
                Tpc = 164.3 + 357.7 * gasgrav - 67.7 * Math.Pow(gasgrav, 2);
                ppc = 706.0 - 51.70 * gasgrav - 11.1 * Math.Pow(gasgrav, 2);
            }
            else
            {
                throw new Exception($"Unknown gastype {gastype}!");
            }

            return CorrectionForNonhydrocarbonWichertAziz(ppc, Tpc, yCO2, yH2S);
        }

        public static IEnumerable<double> CorrectionForNonhydrocarbonWichertAziz(double ppc, double Tpc, double yCO2, double yH2S)
        {
            // [ppc] = psia, [Tpc] = degR
            double ppc_corr = ppc;
            double Tpc_corr = Tpc;
            if (yCO2 != 0.0 && yH2S != 0.0) // only correct when at least one is non zero
            {
                double A = yH2S + yCO2;
                double eps = 120.0 * (Math.Pow(A, 0.9) - Math.Pow(A, 1.6)) + 15.0 * (Math.Pow(yH2S, 0.5) - Math.Pow(yH2S, 0.4));
                Tpc_corr = Tpc - eps;
                ppc_corr = ppc * Tpc_corr / (Tpc + yH2S * (1 - yH2S) * eps);
            }

            yield return ppc_corr;
            yield return Tpc_corr;
        }
    }
}
