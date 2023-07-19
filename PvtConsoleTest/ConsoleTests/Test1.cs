using PvtLibrary3;
using PvtLibrary3.BlackOil;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PvtLibrary3.Common;

namespace PvtConsoleTest
{
    public class Test1
    {
        public static void Run()
        {
            var GOR = 594;     // scf/STB
            var gasgrav = 0.878;
            var API = 36.4;
            var salinity = 1.0;  // weight %
            var WCT = 0.0;
            var waterGravity = 1.0;
            var N2MolPercent = 0.22;
            var C02Percent = 0.17;
            var H2SPercent = 0.0;

            var fluid = new Fluid(API, gasgrav, GOR, WCT, salinity, waterGravity,
                                  N2MolPercent, C02Percent, H2SPercent);
            fluid.SetBlackoilCorr(BlackOilCorr.Glaso);
            //You must set Tp to use Egbogah correlation
            fluid.SetOilViscosityCorr(OilViscosityCorr.BeggsRobinson);

            //var pvt = fluid.LocalGasLiquidProperties(3673 + 14.7, 181);
            var pvt = fluid.LocalGasLiquidProperties(100, 100);
            double[] pvtResults = new double[] { pvt.Pb, pvt.Bo, pvt.Rs, pvt.rhoO, pvt.muO, pvt.Co,
                                                 pvt.rhoG, pvt.muG, pvt.Bg, pvt.ZFactor,
                                                 pvt.rhoW, pvt.muW, pvt.Bw };
            string[] headings = new string[] { "Pb", "Bo", "Rs", "rhoO",
                                                "muO", "Co", "rhoG", "muG", "Bg", 
                                                "Zfactor", "rhoW", "muW", "Bw" };
            for (int i = 0; i < pvtResults.Length; i++)
            {
                Console.WriteLine("{0}          {1}", headings[i], pvtResults[i]);
            }
        }
    }
}
