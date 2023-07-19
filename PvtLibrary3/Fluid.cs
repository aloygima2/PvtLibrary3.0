using System;
using System.Collections.Generic;
using PvtLibrary3.Common;
using System.Text;
using PvtLibrary3.BlackOil;
using System.Linq;

namespace PvtLibrary3
{
    /// <summary>
    /// This class contains all the routines for analysing the fluid instance
    /// </summary>
    public class Fluid
    {
        #region Fields
        /// <summary>
        /// Water gravity at standard conditions
        /// </summary>
        private double watergrav_sc;

        /// <summary>
        /// Gas oil ratio
        /// </summary>
        private double _GOR;

        /// <summary>
        /// Water cut in percent
        /// </summary>
        private double WCT;

        private double WOR;

        /// <summary>
        /// Gas liquid ratio
        /// </summary>
        private double GLR;

        double rsTotal;
        double api;
        double gasgravity;
        double salinity;
        double h2smole;
        double n2mole;
        double co2mole;
        double tf;
        double tsep;
        double psep;
        double tst = 60;
        double pst = 14.7;
        double ygAverage;
        bool pbImpurityCorrection = false;

        /// <summary>
        /// Stock tank oil API gravity
        /// </summary>
        public double Api { get => api; set => api = value; }

        /// <summary>
        /// Producing condensate/gas ratio in (stb/mmScf)
        /// </summary>
        public double CGR { get; set; }

        /// <summary>
        ///stock tank condensate oil gravity in unit of API
        /// </summary>
        public double condensateGravity { get; set; }

        /// <summary>
        /// Gas gravity (air=1).
        /// This refers to the gravity measured at the primary separator, if <see cref="SeparatorStage.SingleStage "/> [is chosen this refers to the stock tank condition.
        /// </summary>
        public double GasGravity { get => gasgravity; set => gasgravity = value; }

        /// <summary>
        /// Secondary separator's gas gravity. (air=1)
        /// </summary>
        public double Gg2 { get; set; }

        /// <summary>
        /// Stock tank gas gravity (air=1)
        /// </summary>
        public double Gg3 { get; set; }

        /// <summary>
        /// Mole composition of Hydrogen Sulphide in %
        /// </summary>
        public double MoleH2S { get => h2smole / 100; set => h2smole = value; }

        /// <summary>
        /// Mole composition of Carbon dioxide in %
        /// </summary>
        public double MoleCO2 { get => co2mole / 100; set => co2mole = value; }

        /// <summary>
        /// Mole composition of Nitrogen in %
        /// </summary>
        public double MoleN2 { get => n2mole / 100; set => n2mole = value; }

        ///// <summary>
        ///// Bubble point of fluid in psia
        ///// </summary>
        //public double Pb { get; set; }

        /// <summary>
        /// if set to true, applies a correction factor to the calculated bubble point pressure due to the presence of non-hydrocarbon impurities.
        /// </summary>
        public bool PbImpurityCorrection
        {
            get => pbImpurityCorrection;
            set
            {
                if (co2mole == 0 && MoleN2 == 0 && MoleH2S == 0)
                {
                    pbImpurityCorrection = false;
                }
                else
                {
                    pbImpurityCorrection = value;
                }
            }
        }

        /// <summary>
        /// Primary separator pressure in psia
        /// </summary>
        /// <value>If separator option is <see cref="SeparatorStage.SingleStage"/>then the <see cref="Tst"/> is used</value>
        public double Psep { get => psep; set => psep = value; }

        /// <summary>
        /// primary separator pressure in Psia
        /// applicable when <see cref="SeparatorStage.TwoStages"/> is chosen
        /// </summary>
        public double Psp1 { get; set; }

        /// <summary>
        /// Stock Tank pressure in psia
        /// </summary>
        /// <value>14.7</value>
        public double Pst { get => pst; set => pst = value; }

        /// <summary>
        /// solution gas/oil ratio at the bubble point in scf/stb
        /// </summary>
        public double RsTotal { get; set; }
                
        /// <summary>
        /// Primary separator Rs in scf/stb
        /// </summary>
        public double R1 { get; set; }
        
        /// <summary>
        /// stock tank Rs in scf/stb
        /// </summary>
        public double R3 { get; set; }

        /// <summary>
        /// GOR at the primary separator.
        /// Necessary for calculating average gas gravity.
        /// </summary>
        public double Rs1 { get; set; }

        /// <summary>
        /// secondary separator gas-oil ratio in scf/stb
        /// </summary>
        public double Rs2 { get; set; }

        /// <summary>
        /// stock tank gas-oil ratio in scf/stb
        /// </summary>
        public double Rs3 { get; set; }
        /// <summary>
        /// salinity is in ppm
        /// </summary>
        public double Salinity { get => salinity; set => salinity = value; }

        /// <summary>
        /// number of surface separator train used.
        /// For Blackoil, the associated gas produce is dry gas, therefore the default separator option used is <see cref="SeparatorStage.SingleStage"/>.
        /// </summary>
        public SeparatorStage SepConfig { get; set; } = SeparatorStage.SingleStage;

        /// <summary>
        /// Pour point temperature in fahreeheit.
        /// used in Egoghah's dead oil viscosity if available
        /// </summary>
        public double Tp { get => tf; set => tf = value; }

        /// <summary>
        ///Primary separator temperature in fahrenheit
        /// </summary>
        /// <value>If separator option is <see cref="SeparatorStage.SingleStage"/>then the <see cref="Tst"/> is used</value>
        public double Tsep { get => tsep; set => tsep = value; }

        /// <summary>
        /// Primary separator temperature in fahrenheit
        /// applicable when <see cref="SeparatorStage.TwoStages"/>is chosen
        /// </summary>
        public double Tsp1 { get; set; }
        /// <summary>
        /// Stock Tank Temperature in fahrenheit
        /// </summary>
        /// <value>60</value>
        public double Tst { get => tst; set => tst = value; }

        /// <summary>
        /// Blackoil correlation
        /// </summary>
        private BlackOilCorr blackoilCorr;

        /// <summary>
        /// Oil viscosity correlation
        /// </summary>
        private OilViscosityCorr oilViscosityCorr;

        /// <summary>
        /// Pseudocritical pressure temperature correlation
        /// </summary>
        private PseudoCritPtCorr pseudoCritPTCorr;

        /// <summary>
        /// Z factor correlation
        /// </summary>
        private ZfactorCorr zfactorCorr;

        private GasType gastype = GasType.Natural;  // the other is condensate
        private double QoSc;
        private double QgSc;
        private double QwSc;

        #endregion

        /// <summary>
        /// Constructs the fluid object
        /// </summary>
        /// <param name="API">API gravity of oil</param>
        /// <param name="gasgrav">Gas gravity</param>
        /// <param name="GOR">Gas oil ratio</param>
        /// <param name="WCT">Water cut</param>
        /// <param name="salinity">Water salinity, in percent</param>
        /// <param name="watergrav_sc">Specific gravity of water at standard conditions</param>
        /// <param name="yN2">Mole fraction of Nitrogen, in percent</param>
        /// <param name="yCO2">Mole fraction of Carbon dioxide, in percent</param>
        /// <param name="yH2S">Mole fraction of Hydrogen sulphide, in percent</param>
        /// <param name="psep">Seperator pressure, in psia</param>
        /// <param name="Tsep">Seperator temperature, in degF</param>
        public Fluid(double API, double gasgrav, double GOR, double WCT, double salinity,
                     double watergrav_sc = 1.0, double yN2 = 0.0, double yCO2 = 0.0,
                     double yH2S = 0.0, double psep = 14.7,
                     SeparatorStage separatorTrain = SeparatorStage.SingleStage) 
        {
            this.Api = API;
            this.condensateGravity = Api;
            this.GasGravity = gasgrav;
            this.watergrav_sc = watergrav_sc;
            this._GOR = GOR;
            this.WCT = WCT;
            this.WOR = WCT / (1 - WCT);
            this.GLR = GOR * (1 - WCT);
            this.salinity = salinity;
            this.MoleN2 = yN2;
            this.MoleCO2 = yCO2;
            this.MoleH2S = yH2S;
            this.psep = psep;
            this.SepConfig = separatorTrain;
            this.ygAverage = Gas.AverageGasGravity(Psp1, Tsp1, condensateGravity, Rs1, Rs2, Rs3,
                                                 Gg2, Gg3, GasGravity, separatorTrain);
            //this.Tsep = Tsep;

            blackoilCorr = BlackOilCorr.Glaso;
            oilViscosityCorr = OilViscosityCorr.BeggsRobinson;
            pseudoCritPTCorr = PseudoCritPtCorr.Sutton;
            zfactorCorr = ZfactorCorr.HallYarborough;

            gastype = GasType.Natural; // the other is condensate
        }
      
        public void OilPhysicalProperties(double p, double T, double pb) { }

        public void GasPhysicalProperties(double p, double T) { }

        public double GetWCT()
        {
            return WCT;
        }

        public double GetGOR()
        {
            return _GOR;
        }

        public double GetOilRate()
        {
            return QoSc;
        }

        public double GetLiquidRate()
        {
            return QoSc + QwSc;
        }

        /// <summary>
        /// Sets the gas lift rate on the fluid
        /// </summary>
        /// <param name="rate">Gas rate, in Scf/Day</param>
        /// <returns>Fluid instance</returns>
        public Fluid SetGasLiftRate(double rate)
        {
            QgSc += rate;
            _GOR = QgSc / QoSc;
            return this;
        }

        /// <summary>
        /// Sets the oil rate at standard conditions on the fluid instance
        /// </summary>
        /// <param name="Q_sc">Fluid rate, in STB/Day</param>
        /// <returns>Fluid instance with oil rate</returns>
        public Fluid SetOilRate(double Q_sc)
        {
            QoSc = Q_sc;
            QgSc = QoSc * _GOR;
            //Qw_sc = Qo_sc * WCT; // Buuuuuuuuuuuuuuuuuuuuuugggggggggggggggggggggggg!!!
            QwSc = QoSc * WCT / (1 - WCT);
            return this;
        }

        /// <summary>
        /// Sets the fluid liquid rate at standard conditions
        /// </summary>
        /// <param name="Q_sc">Liquid rate, in Scf/Day</param>
        /// <returns>Fluid instance</returns>
        public Fluid SetLiquidRate(double Q_sc)
        {
            QoSc = Q_sc * (1 - WCT);
            QgSc = QoSc * _GOR;
            //Qw_sc = Qo_sc * WCT; // Buuuuuuuuuuuuuuuuuuuuggggggggggggggggggggggggg!!!
            QwSc = QoSc * WCT / (1 - WCT);
            return this;
        }

        /// <summary>
        /// Sets the Blackoil correlation on the fluid
        /// </summary>
        /// <param name="corr">Choice of blackoil correlation</param>
        /// <returns>Fluid</returns>
        public Fluid SetBlackoilCorr(BlackOilCorr corr)
        {
            blackoilCorr = corr;
            return this;
        }

        /// <summary>
        /// Sets the oil viscosity correlation for the fluid
        /// </summary>
        /// <param name="corr">Choice of oil viscosity correlation</param>
        /// <returns>Fluid</returns>
        public Fluid SetOilViscosityCorr(OilViscosityCorr corr)
        {
            oilViscosityCorr = corr;
            return this;
        }

        /// <summary>
        /// Sets the Pseudocritical pressure and temperature correlation
        /// </summary>
        /// <param name="corr">Choice of Pseudocritical correlation</param>
        /// <returns>Fluid</returns>
        public Fluid SetPseudoCritPTCorr(PseudoCritPtCorr corr)
        {
            pseudoCritPTCorr = corr;
            return this;
        }

        /// <summary>
        /// Sets the Z-factor correlation
        /// </summary>
        /// <param name="corr">Choice of Z factor correlation</param>
        /// <returns>Fluid object</returns>
        public Fluid SetZFactorCorr(ZfactorCorr corr)
        {
            zfactorCorr = corr;
            return this;
        }

        public Fluid SetGOR(double GOR)
        {
            this._GOR = GOR;
            GLR = GOR * (1 - WCT);
            return this;
        }

        public Fluid SetWCT(double WCT)
        {
            this.WCT = WCT;
            WOR = WCT / (1 - WCT);
            return this;
        }
                
        public FluidProperties LocalGasLiquidProperties(double P, double T, double dia = -99.0, 
                                        double Cp1Bo = 1.0, double Cp2Bo = 0.0, double Cp3Bo = 1.0,
                                        double Cp4Bo = 0.0, double Cp1Pb = 1.0, double Cp2Pb = 0.0,
                                        double Cp1Rs = 1.0, double Cp2Rs = 0.0)
        {
            double API = Api;
            double gasgrav = GasGravity;
            double GOR = _GOR;
            SeparatorStage separatorTrain = SepConfig;
            double yN2 = MoleN2; 
            double yCO2 = MoleCO2;
            double yH2S = MoleH2S;

            var res = BlackOilProperties(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    R3, pbImpurityCorrection, separatorTrain, yN2,
                                    yCO2, yH2S, Cp1Bo, Cp2Bo, Cp3Bo, Cp4Bo, Cp1Pb,
                                    Cp2Pb, Cp1Rs, Cp2Rs).ToList();
            var Pb = res[0];
            var Rs = res[1];
            var Bo = res[2];
            var co = res[3];

            double Rsw;
            double gasgravf;
            double gasgravd;
            if (Rs == GOR) // all gas in oil
            {
                Rsw = 0.0;
                gasgravf = gasgravity; // specific gravity of free gas
                gasgravd = gasgravity; // specific gravity of dissolved gas
            }
            else
            {
                Rsw = Water.Rsw_craft_hawkins(P, T, salinity);
                //Rsw = 0.0;

                // Check to be sure that solution gas in oil and water does not exceed available gas.
                // If it does, Rsw is decreased and gas is preferentially dissolved in oil.
                var Qgs = QoSc * Rs + QwSc * Rsw;   // gas in solution

                if (QgSc < Qgs)
                {
                    Rsw = (QgSc - QoSc * Rs) / QwSc;
                    gasgravf = gasgravity;
                    gasgravd = gasgravity;
                }
                else
                {
                    gasgravd = Oil.gasgrav_dis(API, gasgravity, Rs);
                    gasgravf = Oil.gasgrav_free(gasgravity, gasgravd, GOR, Rs);
                }

            }

            double ppc;
            double Tpc;
            if (pseudoCritPTCorr == PseudoCritPtCorr.Standing)
            {
                var res1 = Gas.PseudocriticalPTStanding(gasgravf, gastype,
                                                        yCO2, yH2S).ToList();
                ppc = res1[0];
                Tpc = res1[1];
            }
            else
            {
                var res1 = Gas.PseudocriticalPTSutton(gasgravf, gastype,
                                                        yCO2, yH2S).ToList();
                ppc = res1[0];
                Tpc = res1[1];
            }

            double ppr = P / ppc;
            double Tpr = (T + 460.0) / Tpc;

            double z;
            if (zfactorCorr == ZfactorCorr.BeggsBrill)
            {
                z = Gas.ZFactorBeggsAndBrill(ppr, Tpr);
            }
            else if (zfactorCorr == ZfactorCorr.HallYarborough)
            {
                z = Gas.ZFactorHallAndYarborough(ppr, Tpr);
            }
            else
            {
                throw new Exception($"Unknown zfactor correlation {zfactorCorr}");
            }

            double Bg = Gas.BgFromGasLaw(P, T, z);

            // water
            var Bw = Water.Bw_gould(P, T);  

            var Qo = QoSc * Bo * 5.614 / 86400.0;
            var Qw = QwSc * Bw * 5.614 / 86400.0;
            var Qg = (QoSc * (GOR - Rs) - QwSc * Rsw) * Bg / 86400.0;
            var Ql = Qo + Qw;

            // local fractions of oil and water in liquid phase
            double Fo = Qo / Ql;
            double Fw = 1.0 - Fo;

            // calculate physical properties
            // density
            //var rho_o = Oil.oil_density(p, API, gasgrav, gasgravd, Rs, Pb, Bo, co);
            double sgo = 141.5 / (API + 131.5);
            var rho_o = (sgo * 62.4 + Rs * gasgravity * 0.0764 / 5.6146) / Bo;
            //var rho_w = watergrav_sc / Bw;     // ======> Blunder! discovered on 02-09-2022 at 04:34 AM
            var rho_w = 62.4 * watergrav_sc / Bw; // correction to above bug
            var rho_g = gasgrav * 0.0764 / Bg;
            //var rho_w = Water.water_density(watergrav_sc, Bw);
            double rho_l = Fo * rho_o + Fw * rho_w;
            //double rho_g = Gas.gas_density(p, T, z, gasgravf);

            // oil viscosity
            var mu_o = Oil.ComputeOilViscosity(P, T, Pb, Rs, GOR, API, oilViscosityCorr, Tp);
            var mu_w = Water.water_viscosity(T);
            double mu_l = Fo * mu_o + Fw * mu_w;
            double Mg = Gas.GasMolecularWeight(gasgravf);
            double mu_g = Gas.GasViscosityLee(T, rho_g, Mg);

            // surface tension
            var surf_o = Oil.gas_oil_interfacial_tension(P, T, API);
            var surf_w = Water.gas_water_interfacial_tension(P, T);
            var surf_l = Fo * surf_o + Fw * surf_w;

            // compute mass flow rate, w
            double sgw = watergrav_sc;
            double glr = GLR;
            double api = API;
            //double sgo = 141.5 / (131.5 + api);
            double m = 350 * sgo * Fo + 350 * sgw * Fw + 0.0764 * glr * gasgravf;  // mass associated with 1 stb of liquid
            double w = m * (QoSc + QwSc);    // mass flow rate, lbm/day

            // Pipe geometry
            double A = -99.0;
            double vsl = -99.0;
            double vsg = -99.0;
            double vm = -99.0;
            if (dia > 0.0)
            {
                A = Math.PI * Math.Pow(dia, 2) / 4.0;
                vsl = Ql / A;
                vsg = Qg / A;
                vm = vsl + vsg;
                if (Math.Abs(vsg) < 1e-17) vsg = 0;
            }

            var fl = new FluidProperties
            {
                vsg = vsg,
                vsl = vsl,

                rhoG = rho_g,
                rhoO = rho_o,
                rhoW = rho_w,
                rhoL = rho_l,

                muO = mu_o,
                muW = mu_w,
                muG = mu_g,
                muL = mu_l,

                sigmaW = surf_w,
                sigmaO = surf_o,
                sigmaL = surf_l,

                Qg = Qg,
                Qo = Qo,
                Qw = Qw,

                Pb = Pb,
                Bg = Bg,
                Bo = Bo,
                Rs = Rs,
                Bw = Bw,
                Co = co,
                ZFactor = z                  
            };

            return fl;
        }

        /// <summary>
        /// Calculates the Bubble point, Oil formation volume factor, Gas oil ratio, 
        /// and Isothermal compressibility of oil
        /// </summary>
        /// <param name="P">Pressure in psia</param>
        /// <param name="T">Temperature in degF</param>
        /// <param name="API">Oil gravity in API</param>
        /// <param name="gasgrav"></param>
        /// <param name="condensateGravity"></param>
        /// <param name="GOR"></param>
        /// <param name="Psep"></param>
        /// <param name="Tsep"></param>
        /// <param name="Psp1"></param>
        /// <param name="Tsp1"></param>
        /// <param name="Tst"></param>
        /// <param name="Rs1"></param>
        /// <param name="Rs2"></param>
        /// <param name="Rs3"></param>
        /// <param name="R3"></param>
        /// <param name="Gg2"></param>
        /// <param name="Gg3"></param>
        /// <param name="pbImpurityCorrection"></param>
        /// <param name="separatorTrain"></param>
        /// <param name="yN2"></param>
        /// <param name="yCO2"></param>
        /// <param name="yH2S"></param>
        /// <returns>IEnumerable containing Pb, Bo, Rs, co</returns>
        public IEnumerable<double> BlackOilProperties(double P, double T, double API,
                                double gasgrav,double GOR, double Psep, double Tsep, 
                                double Tst, double R3, bool pbImpurityCorrection,
                                SeparatorStage separatorTrain, double yN2 = 0.0,
                                double yCO2 = 0.0, double yH2S = 0.0, double Cp1Bo = 1.0,
                                double Cp2Bo = 0.0, double Cp3Bo = 1.0, double Cp4Bo = 0.0,
                                double Cp1Pb = 1.0, double Cp2Pb = 0.0, double Cp1Rs = 1.0,
                                double Cp2Rs = 0.0)
        {
            var blackoil = blackoilCorr;
            if (blackoil == BlackOilCorr.Glaso)
            {
                return Oil.BlackOilGlaso(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    pbImpurityCorrection, ygAverage, yN2, yCO2, yH2S,
                                    Cp1Bo, Cp2Bo, Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.Standing)
            {
                return Oil.BlackOilStanding(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                        pbImpurityCorrection, ygAverage, yN2, Cp1Bo, Cp2Bo,
                                        Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.VazquezBeggs)
            {
                return Oil.BlackOilVazquezBeggs(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    pbImpurityCorrection, ygAverage, yN2, Cp1Bo, Cp2Bo, 
                                    Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.Lasater)
            {
                return Oil.BlackOilLasater(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    pbImpurityCorrection, ygAverage, yN2, Cp1Bo, Cp2Bo,
                                    Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.Petrosky)
            {
                return Oil.BlackOilPetrosky(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    pbImpurityCorrection, ygAverage, yN2, Cp1Bo, Cp2Bo,
                                    Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.AlMarhoun)
            {
                return Oil.BlackOilAlMarhoun(P, T, API, gasgrav, GOR, Psep, Tsep, Tst, 
                                    pbImpurityCorrection, ygAverage, yN2, Cp1Bo, Cp2Bo, 
                                    Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }
            else if (blackoil == BlackOilCorr.DeGhetto)
            {
                return Oil.BlackOilDeGhetto(P, T, API, gasgrav, GOR, Psep, Tsep, R3,
                                ygAverage, pbImpurityCorrection, separatorTrain, yN2, 
                                Cp1Bo, Cp2Bo, Cp3Bo, Cp4Bo, Cp1Pb, Cp2Pb, Cp1Rs, Cp2Rs);
            }                     
            else
            {
                throw new Exception($"Unknown blackoil correlation {blackoil}");
            }

        }

        public Fluid Clone()
        {
            var clone = new Fluid(
                Api,
                GasGravity,
                _GOR,
                WCT,
                salinity,
                watergrav_sc,
                MoleN2,
                MoleCO2,
                MoleH2S,
                psep
            );

            //set_gas_lift_rate()
            clone.SetOilRate(QoSc);
            clone.SetBlackoilCorr(blackoilCorr);
            clone.SetOilViscosityCorr(oilViscosityCorr);
            clone.SetPseudoCritPTCorr(pseudoCritPTCorr);
            clone.SetZFactorCorr(zfactorCorr);

            return clone;
        }
    }
}
