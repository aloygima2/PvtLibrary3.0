using System;
using System.Collections.Generic;
using System.Text;

namespace PvtLibrary3.BlackOil
{
    /// <summary>
    /// Contains all the routines for defining water properties
    /// </summary>
    public class Water
    {
        /// <summary>
        /// Calculates the density of water
        /// </summary>
        /// <param name="watergrav_sc">Water specific gravity</param>
        /// <param name="Bw">Water formation volume factor</param>
        /// <returns>Density of water, in lb/ft^3</returns>
        public static double water_density(double watergrav_sc, double Bw)
        {
            double rho = 62.4 * watergrav_sc / Bw;
            return rho;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="T"></param>
        /// <param name="salinity"></param>
        /// <returns></returns>
        public static double Rsw_craft_hawkins(double p, double T, double salinity)
        {
            // salinity=salinity of water, weight% of NaCl
            double A = 2.12 + (3.45e-3) * T - (3.59e-5) * Math.Pow(T, 2);
            double B = 0.0107 - (5.26e-5) * T + (1.48e-7) * Math.Pow(T, 2);
            double C = -(8.75e-7) + (3.9e-9) * T - (1.02e-11) * Math.Pow(T, 2);
            double Rsw = A + B * p + C * Math.Pow(p, 2);
            double Cs = 1.0 - (0.0753 - 0.000173 * T) * salinity; // Cs=salinity correction factor
            double Rswb = Rsw * Cs;  // Rswb=gas solubility in brine
            return Rswb;
        }

        public static double Bw_gould(double p, double T)
        {
            double Tx = T - 60.0;
            // Bw = 2.0 + 1.2e-4 * Tx + 1e-6 * Tx**2 - 3.33e-6 * p //wrong version given in multiphase flow in wells by brills
            double Bw = 1.0 + 1.2e-4 * Tx + 1e-6 * Math.Pow(Tx, 2) - 3.33e-6 * p;
            return Bw;
        }

        public static double Bw_above_pb(double p, double pb, double Bwb, double cw)
        {
            // Note that this is only applicable for p > pb
            double Bw = Bwb * Math.Exp(-cw * (p - pb));
            return Bw;
        }

        public static double Cwf_meehan(double p, double T)
        {
            // cwf = compressibility of gas-free water, psi**-1
            // p = pressure, psia
            // T = temperature, degF
            double A = 3.8546 - 0.000134 * p;
            double B = -0.01052 + 4.77e-7 * p;
            double C = 3.9267e-5 - 8.8e-10 * p;
            double cwf = 1e-6 * (A + B * T + C * Math.Pow(T, 2));
            return cwf;
        }

        public static double Cwg_meehan(double Rsw, double cwf)
        {
            // cwg = compressibility of gas-saturated water, psi**-1
            double cwg = cwf * (1.0 + 8.9e-3 * Rsw);
            return cwg;
        }

        public static double Cs_salinity_correction(double T, double salinity)
        {
            // Calculates salinity correction factor by Numbere et al
            double S = salinity;
            double Cs = 1.0 + (-0.052 + 2.7e-4 * T - 1.14e-6 * Math.Pow(T, 2) + 1.121e-9 * Math.Pow(T, 3)) * Math.Pow(S, 0.7);
            return Cs;
        }

        public static double Cw_meehan(double p, double T, double pb, double Rsw,
                                       double salinity = 0.0)
        {

            double cwf = Cwf_meehan(p, T); // compressiblity of gas free water
            double cwg = Cwg_meehan(Rsw, cwf);  // compressibility of gas saturated water
            double Cs = Cs_salinity_correction(T, salinity); // 
            double cw = cwg * Cs; // compressibility in brine
            return cw;
        }

        public static double water_viscosity(double T)
        {
            // T = temperature in degF
            double mu_w = Math.Exp(1.003 - 1.479e-2 * T + 1.982e-5 * Math.Pow(T, 2));
            return mu_w;
        }

        public static double gas_water_interfacial_tension(double p, double T)
        {
            if (p > 17569.0)
            {
                throw new Exception("pressure is above 17569 psi");
            }

            double sigmaw74 = 75.0 - 1.108 * Math.Pow(p, 0.349);
            double sigmaw280 = 53.0 - 0.1048 * Math.Pow(p, 0.637);
            double sigmaw;

            if (T <= 74.0)
            {
                sigmaw = sigmaw74;
            }
            else if (T >= 280.0)
            {
                sigmaw = sigmaw280;
            }
            else // 74.0 < T < 280.0
            {
                sigmaw = sigmaw74 - (T - 74.0) * (sigmaw74 - sigmaw280) / 206.0;
            }

            return sigmaw;
        }

    }
}

