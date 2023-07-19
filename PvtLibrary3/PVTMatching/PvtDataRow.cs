//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace PvtLibrary3
//{
//    public class PvtDataRow
//    {
//        public double Temperature { get; set; }
//        public double Pressure { get; set; }
//        public double BubblePoint { get; set; }
//        public double GasOilRatio { get; set; }
//        public double OilFVF { get; set; }
//        public double OilViscosity { get; set; }
//        public double ZFactor { get; set; }
//        public double GasFVF { get; set; }
//        public double GasViscosity { get; set; }
//        public double OilDensity { get; set; }
//        public double GasDensity { get; set; }
//        public double WaterFVF { get; set; }
//        public double WaterViscosity { get; set; }
//        public double WaterDensity { get; set; }
//        public double WaterCompressibility { get; set; }
//        public double DewPoint { get; set; }
//        public double PseudoPressure { get; set; }
//        public double ReservoirCGR { get; set; }
//        public double VaporizedCGR { get; set; }
//        public double VapourisedWGR { get; set; }
//        public PvtDataRow Copy()
//        {
//            PvtDataRow ans = new PvtDataRow
//            {
//                Temperature = Temperature,
//                Pressure = Pressure,
//                BubblePoint = BubblePoint,
//                GasOilRatio = GasOilRatio,
//                OilFVF = OilFVF,
//                OilViscosity = OilViscosity,
//                ZFactor = ZFactor,
//                GasFVF = GasFVF,
//                GasViscosity = GasViscosity,
//                OilDensity = OilDensity,
//                GasDensity = GasViscosity,
//                WaterFVF = WaterFVF,
//                WaterViscosity = WaterViscosity,
//                WaterDensity = WaterDensity,
//                WaterCompressibility = WaterCompressibility,
//                DewPoint = DewPoint,
//                PseudoPressure = PseudoPressure,
//                ReservoirCGR = ReservoirCGR,
//                VaporizedCGR = VaporizedCGR,
//                VapourisedWGR = VapourisedWGR
//            };
//            return ans;
//        }
//        public void SetProperty(string name, double val)
//        {
//            switch (name)
//            {
//                case "Temperature":
//                    Temperature = val; break;
//                case "Pressure":
//                    Pressure = val; break;
//                case "BubblePoint":
//                    BubblePoint = val; break;
//                case "GasOilRatio":
//                    GasOilRatio = val; break;
//                case "OilFVF":
//                    OilFVF = val; break;
//                case "OilViscosity":
//                    OilViscosity = val; break;
//                case "ZFactor":
//                    ZFactor = val; break;
//                case "GasFVF":
//                    GasFVF = val; break;
//                case "GasViscosity":
//                    GasViscosity = val; break;
//                case "OilDensity":
//                    OilDensity = val; break;
//                case "GasDensity":
//                    GasDensity = val; break;
//                case "WaterFVF":
//                    WaterFVF = val; break;
//                case "WaterViscosity":
//                    WaterViscosity = val; break;
//                case "WaterDensity":
//                    WaterDensity = val; break;
//                case "WaterCompress.":
//                    WaterCompressibility = val; break;
//                case "DewPoint":
//                    DewPoint = val; break;
//                case "PseudoPressure":
//                    PseudoPressure = val; break;
//                case "ReservoirCGR":
//                    ReservoirCGR = val; break;
//                case "VaporizedCGR":
//                    VaporizedCGR = val; break;
//                case "VapourisedWGR":
//                    VapourisedWGR = val; break;
//                default:
//                    break;
//            }
//        }
//    }
//}
