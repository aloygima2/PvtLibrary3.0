//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MathematicsLibrary2021;
//using PvtLibrary3.BlackOil;
//using PvtLibrary3.Common;
//using PrintService;
////using PrintService;

//namespace PvtLibrary3
//{
//    public class PVTMatching
//    {
//        public List<MatchingParameters> BubblePointMatching = new List<MatchingParameters>();
//        public List<MatchingParameters> SolutionGasMatching = new List<MatchingParameters>();
//        public List<MatchingParameters> OilFVFMatching = new List<MatchingParameters>();
//        public List<MatchingParameters> OilViscosityMatching = new List<MatchingParameters>();
//        public List<MatchingParameters> GasFVFMatching = new List<MatchingParameters>();
//        public List<MatchingParameters> GasViscosityMatching = new List<MatchingParameters>();

//        public enum MatchingMethod
//        {
//            EndPoint,
//            Regression
//        }

//        public class MatchingParameters
//        {
//            public string Name { get; set; }
//            public double C1 { get; set; }
//            public double C2 { get; set; }
//            public double C3 { get; set; }
//            public double C4 { get; set; }
//            public double StdDeviation { set; get; }

//            public MatchingParameters()
//            { C1 = 1; C2 = 0; C3 = 1; C4 = 0; StdDeviation = 0; }
//        }

//        public PVTMatching()
//        {
//            for (int i = 0; i < Enum.GetValues(typeof(BlackOilCorr)).Length; i++)
//            {
//                BubblePointMatching.Add(new MatchingParameters());
//                OilFVFMatching.Add(new MatchingParameters());
//                SolutionGasMatching.Add(new MatchingParameters());
//            }
//            for (int i = 0; i < Enum.GetValues(typeof(OilViscosityCorr)).Length; i++)
//                OilViscosityMatching.Add(new MatchingParameters());
//            for (int i = 0; i < 3; i++)
//                GasFVFMatching.Add(new MatchingParameters());
//            for (int i = 0; i < Enum.GetValues(typeof(GasViscosityCorr)).Length; i++)
//                GasViscosityMatching.Add(new MatchingParameters());
//        }

//        public void Match(PVTMatchingInput matchinginput, MatchingMethod matchingMethod = MatchingMethod.EndPoint)
//        {
//            BlackOilCorr oilPbBoRsModel; 
//            OilViscosityCorr oilVisModel;
//            ZfactorCorr gasCompressibilityModel; 
//            GasViscosityCorr gasViscosityModel;
//            double Temperature = matchinginput.Temperature;
//            double Api = matchinginput.fluid.Api;
//            double RsTotal = matchinginput.fluid.RsTotal;
//            double GasGravity = matchinginput.fluid.GasGravity;
//            double condensateGravity = matchinginput.fluid.Api;
//            double Psep = matchinginput.fluid.Psep;
//            double Tsep = matchinginput.fluid.Tsep;
//            double Psp1 = matchinginput.fluid.Psp1;
//            double Tsp1 = matchinginput.fluid.Tsp1;
//            double Rs1 = matchinginput.fluid.Rs1;
//            double Rs2 = matchinginput.fluid.Rs2;
//            double Rs3 = matchinginput.fluid.Rs3;
//            double R3 = matchinginput.fluid.R3;
//            double Gg2 = matchinginput.fluid.Gg2;
//            double Gg3 = matchinginput.fluid.Gg3;
//            double yCO2 = matchinginput.fluid.MoleCO2;
//            double yH2S = matchinginput.fluid.MoleH2S;
//            double yN2 = matchinginput.fluid.MoleN2;
//            double Tst = matchinginput.fluid.Tst;
//            bool PbImpurityCorrection = matchinginput.fluid.PbImpurityCorrection;
//            SeparatorStage separatorTrain = matchinginput.fluid.SepConfig;
//            double LabBubblePoint = matchinginput.LabBubblePoint;


//            Func<double[], double[], (double C1, double C2)> MatchMethod = (E, M) =>
//            matchingMethod == MatchingMethod.EndPoint ? EndPointmatching(E, M) : Regression(E, M);

//            var PbBoRs = Enum.GetValues(typeof(BlackOilCorr));
//            var OilVis = Enum.GetValues(typeof(OilViscosityCorr));
//            var GasCom = Enum.GetValues(typeof(ZfactorCorr));
//            var GasVis = Enum.GetValues(typeof(GasViscosityCorr));

//            //BlackOilInput BlkOilInput = matchinginput.BlkOilInput;
//            //double Temperature = matchinginput.Temperature;

//            //Check if bubble point is included in data row, if not add it and set included to false
//            bool included = true;
//            PvtDataRow BubblePointRow = new PvtDataRow { Pressure = LabBubblePoint };
//            if (!matchinginput.Any(data => data.Pressure == LabBubblePoint))
//            {
//                included = false;
//                matchinginput.Add(BubblePointRow);
//                matchinginput.Sort((a, b) => a.Pressure.CompareTo(b.Pressure));
//            }

//            var Below = included ? matchinginput.Where(data => data.Pressure <= matchinginput.LabBubblePoint)
//                        .ToList() 
//                        :matchinginput.Where(data => data.Pressure < matchinginput.LabBubblePoint).ToList();

//            var Above = matchinginput.Where(data => data.Pressure >= matchinginput.LabBubblePoint)
//                        .ToList();

//            double[] GORest, GORmsd, FVFmsd, FVFest, Viscmsd, Viscest, Pbest = new double[1], Pbmsd = new double[1];

//            //Match Pb, Rs, Bo
//            for (int k = 0; k < PbBoRs.Length; k++)
//            {
//                oilPbBoRsModel = (BlackOilCorr)PbBoRs.GetValue(k);
//                //BubblePointMatching
//                {
//                    BubblePointMatching[k].Name = oilPbBoRsModel.ToString();
//                    Pbest[0] = Oil.ComputePb(oilPbBoRsModel, Temperature, Api, RsTotal, GasGravity,
//                                    condensateGravity, Psep, Tsep, Psp1, Tsp1, Rs1, Rs2, Rs3, R3, 
//                                    Gg2, Gg3, yCO2, yH2S, yN2, PbImpurityCorrection, separatorTrain);
//                    Pbmsd[0] = LabBubblePoint;
//                    (BubblePointMatching[k].C1, BubblePointMatching[k].C2) = MatchMethod(Pbest, Pbmsd);
//                    Pbest[0] = Oil.ComputePb(oilPbBoRsModel, Temperature, Api, RsTotal, GasGravity,
//                                    condensateGravity, Psep, Tsep, Psp1, Tsp1, Rs1, Rs2, Rs3, R3,
//                                    Gg2, Gg3, yCO2, yH2S, yN2, PbImpurityCorrection, separatorTrain,
//                                    BubblePointMatching[k].C1, BubblePointMatching[k].C2);
//                    BubblePointMatching[k].StdDeviation = StandardDeviation(Pbest, Pbmsd);
//                }

//                //Check if bubble point is included in data row, if not add it and set included to false
//                //Solution GOR Matching
//                {
//                    SolutionGasMatching[k].Name = oilPbBoRsModel.ToString();
//                    matchinginput.Pb = Oil.ComputePb(oilPbBoRsModel, Temperature, Api, RsTotal,
//                                    GasGravity, condensateGravity, Psep, Tsep, Psp1, Tsp1, Rs1, Rs2, Rs3,
//                                    R3, Gg2, Gg3, yCO2, yH2S, yN2, PbImpurityCorrection, separatorTrain,
//                                    BubblePointMatching[k].C1, BubblePointMatching[k].C2);
//                    if (!included) BubblePointRow.GasOilRatio = RsTotal;
//                    var Filtered = matchinginput.Where(data => data.Pressure <= LabBubblePoint);
//                    GORmsd = Filtered.Select(data => data.GasOilRatio).ToArray();
//                    GORest = Filtered.Select(data => Oil.ComputeRs(oilPbBoRsModel, data.Pressure,
//                                            Temperature, matchinginput.Pb, Api, GasGravity, 
//                                            Psp1, Tsp1, Psep, Tsep, condensateGravity, RsTotal, Rs1,
//                                            Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain))
//                                            .ToArray();

//                    (SolutionGasMatching[k].C1, SolutionGasMatching[k].C2) = MatchMethod(GORest, GORmsd);

//                    GORest = Filtered.Select(data => Oil.ComputeRs(oilPbBoRsModel, data.Pressure,
//                                            Temperature, matchinginput.Pb, Api, GasGravity,
//                                            Psp1, Tsp1, Psep, Tsep, condensateGravity, RsTotal, Rs1,
//                                            Rs2, Rs3, Gg2, Gg3, Tst, separatorTrain, 
//                                            SolutionGasMatching[k].C1, SolutionGasMatching[k].C2))
//                                            .ToArray();
//                    SolutionGasMatching[k].StdDeviation = StandardDeviation(GORest, GORmsd);
//                }

//                //Oil FVF Matching
//                {
//                    OilFVFMatching[k].Name = oilPbBoRsModel.ToString();

//                    // below bubblepoint
//                    if (Below.Count > 1)
//                    {
//                        FVFmsd = Below.Select(data => data.OilFVF).ToArray();
//                        FVFest = Below.Select(data => Oil.ComputeBo(oilPbBoRsModel, data.Pressure, Temperature,
//                                            matchinginput.Pb, data.GasOilRatio, Api, RsTotal, GasGravity,
//                                            condensateGravity, Psep, Tsep, Psp1, Tsp1, Rs1, Rs2, Rs3, Gg2, Gg3,
//                                            separatorTrain)).ToArray();
//                        (OilFVFMatching[k].C1, OilFVFMatching[k].C2) = MatchMethod(FVFest, FVFmsd);
//                    }

//                    // above bubblepoint
//                    double Cob = Oil.Compressibility.Compute(BlkOilInput, oilPbBoRsModel, 0, BlkOilInput.Pb + 1, Temperature);
//                    if (!included)
//                        BubblePointRow.OilFVF = Oil.FormationVolumeFactor.Compute(BlkOilInput, oilPbBoRsModel, Temperature, BubblePointRow.Pressure,
//                                                BubblePointRow.GasOilRatio, Cob, OilFVFMatching[k].C1, OilFVFMatching[k].C2);
//                    if (Above.Count > 1)
//                    {
//                        FVFmsd = Above.Select(data => data.OilFVF).ToArray();
//                        FVFest = Above.Select(data => Oil.FormationVolumeFactor.Compute(BlkOilInput, oilPbBoRsModel, Temperature, data.Pressure,
//                                 data.GasOilRatio, Cob)).ToArray();
//                        (OilFVFMatching[k].C3, OilFVFMatching[k].C4) = MatchMethod(FVFest, FVFmsd);
//                    }

//                    FVFmsd = matchinginput.Select(data => data.OilFVF).ToArray();
//                    FVFest = matchinginput.Select(data => Oil.FormationVolumeFactor.Compute(BlkOilInput, oilPbBoRsModel, Temperature, data.Pressure,
//                                  data.GasOilRatio, Cob, OilFVFMatching[k].C1, OilFVFMatching[k].C2,
//                                  OilFVFMatching[k].C3, OilFVFMatching[k].C4)).ToArray();
//                    OilFVFMatching[k].StdDeviation = StandardDeviation(FVFest, FVFmsd);
//                }
//            }

//            //Match Oil Visc
//            for (int k = 0; k < OilVis.Length; k++)
//            {
//                oilVisModel = (OilViscosityModel)OilVis.GetValue(k);
//                {
//                    OilViscosityMatching[k].Name = oilVisModel.ToString();
//                    // below bubblepoint
//                    if (Below.Count > 1)
//                    {
//                        Viscmsd = Below.Select(data => data.OilViscosity).ToArray();
//                        Viscest = Below.Select(data => Oil.Viscosity.Compute(BlkOilInput, oilVisModel, data.Pressure,
//                                                       Temperature, data.GasOilRatio)).ToArray();
//                        (OilViscosityMatching[k].C1, OilViscosityMatching[k].C2) = MatchMethod(Viscest, Viscmsd);
//                    }
//                    // above bubblepoint
//                    if (!included)
//                        BubblePointRow.OilViscosity = Oil.Viscosity.Compute(BlkOilInput, oilVisModel, BubblePointRow.Pressure,
//                                             Temperature, BubblePointRow.GasOilRatio, OilViscosityMatching[k].C1, OilViscosityMatching[k].C2);
//                    if (Above.Count > 1)
//                    {
//                        Viscmsd = Above.Select(data => data.OilViscosity).ToArray();
//                        Viscest = Above.Select(data => Oil.Viscosity.Compute(BlkOilInput, oilVisModel, data.Pressure,
//                                  Temperature, data.GasOilRatio)).ToArray();
//                        (OilViscosityMatching[k].C3, OilViscosityMatching[k].C4) = MatchMethod(Viscest, Viscmsd);
//                    }
//                    Viscmsd = matchinginput.Select(data => data.OilViscosity).ToArray();
//                    Viscest = matchinginput.Select(data => Oil.Viscosity.Compute(BlkOilInput, oilVisModel, data.Pressure, Temperature, data.GasOilRatio,
//                              OilViscosityMatching[k].C1, OilViscosityMatching[k].C2, OilViscosityMatching[k].C3, OilViscosityMatching[k].C4)).ToArray();
//                    OilViscosityMatching[k].StdDeviation = StandardDeviation(Viscest, Viscmsd);
//                }
//            }

//            //Match Bg
//            (BlkOilInput.Tpc, BlkOilInput.Ppc) = Gas.PseudoCriticalProperties.Compute(BlkOilInput);
//            for (int k = 0; k < GasCom.Length; k++)
//            {
//                gasCompressibilityModel = (GasCompressibilityModel)GasCom.GetValue(k);
//                {
//                    GasFVFMatching[k].Name = gasCompressibilityModel.ToString();
//                    // below bubblepoint
//                    if (Below.Count > 1)
//                    {
//                        FVFmsd = Below.Select(data => data.GasFVF).ToArray();
//                        FVFest = Below.Select(data => Gas.FormationVolumeFactor.Compute(BlkOilInput, data.Pressure, Temperature,
//                            gasCompressibilityModel)).ToArray();
//                        (GasFVFMatching[k].C1, GasFVFMatching[k].C2) = MatchMethod(FVFest, FVFmsd);
//                        FVFest = Below.Select(data => Gas.FormationVolumeFactor.Compute(BlkOilInput, data.Pressure, Temperature,
//                            gasCompressibilityModel, GasFVFMatching[k].C1, GasFVFMatching[k].C2)).ToArray();
//                        GasFVFMatching[k].StdDeviation = StandardDeviation(FVFest, FVFmsd);
//                    }
//                }
//            }

//            foreach (var data in Below)
//                data.ZFactor = Gas.Zfactor.Compute(BlkOilInput, data.Pressure, Temperature, GasCompressibilityModel.HallYarbourough);

//            for (int k = 0; k < GasVis.Length; k++)
//            {
//                gasViscosityModel = (GasViscosityModel)GasVis.GetValue(k);
//                {
//                    GasViscosityMatching[k].Name = gasViscosityModel.ToString();
//                    // below bubblepoint
//                    if (Below.Count > 1)
//                    {
//                        Viscmsd = Below.Select(data => data.GasViscosity).ToArray();
//                        Viscest = Below.Select(data => Gas.Viscosity.Compute(BlkOilInput, gasViscosityModel, data.Pressure,
//                            Temperature, data.ZFactor)).ToArray();
//                        (GasViscosityMatching[k].C1, GasViscosityMatching[k].C2) = MatchMethod(Viscest, Viscmsd);
//                        Viscest = Below.Select(data => Gas.Viscosity.Compute(BlkOilInput, gasViscosityModel, data.Pressure, Temperature, data.ZFactor, GasViscosityMatching[k].C1, GasViscosityMatching[k].C2)).ToArray();
//                        GasViscosityMatching[k].StdDeviation = StandardDeviation(Viscest, Viscmsd);
//                    }
//                }
//            }
//        }

//        public (double C1, double C2) SinglePointmatching(double Est, double Msd)
//        {
//            double C1, C2;
//            C2 = 0.5 * (Msd - Est);
//            C1 = (Msd - C2) / Est;
//            return (C1, C2);
//        }

//        public (double C1, double C2) EndPointmatching(double[] Est, double[] Msd)
//        {
//            double C1, C2;
//            if (Est.Length == 1)
//                return SinglePointmatching(Est.First(), Msd.First());
//            C1 = (Msd.First() - Msd.Last()) / (Est.First() - Est.Last());
//            C2 = Msd.First() - C1 * Est.First();
//            return (C1, C2);
//        }

//        public (double C1, double C2) Regression(ColVec Est, ColVec Msd)
//        {
//            ColVec One = ColVec.Ones(Est.Numel);
//            if (Est.Numel == 1)
//                return SinglePointmatching(Est.First(), Msd.First());
//            ColVec C = Msd / Matrix.Hcart(Est, One);
//            return (C[0], C[1]);
//        }

//        public double StandardDeviation(double[] E, double[] M)
//        {
//            double[] dev2 = E.Select((e, i) => Math.Pow(e - M[i], 2)).ToArray();
//            double v = dev2.Sum() / dev2.Length;
//            return Math.Sqrt(v);
//        }

//        public (BlackOilCorr pbRsBoModel, OilViscosityCorr oilViscstyModel,
//            ZfactorCorr gasCompressibilityModel, GasViscosityCorr gasViscosityModel) AutoSelect()
//        {
//            var PbBoRs = Enum.GetValues(typeof(BlackOilCorr));
//            var OilVis = Enum.GetValues(typeof(OilViscosityCorr));
//            var GasCom = Enum.GetValues(typeof(ZfactorCorr));
//            var GasVis = Enum.GetValues(typeof(GasViscosityCorr));

//            //selecting the best
//            int k = 0; double std = SolutionGasMatching.First().StdDeviation;
//            for (int i = 0; i < SolutionGasMatching.Count; i++)
//                if (SolutionGasMatching[i].StdDeviation < std)
//                { k = i; std = SolutionGasMatching[i].StdDeviation; }
//            BlackOilCorr pbRsBoModel = (BlackOilCorr)PbBoRs.GetValue(k);

//            k = 0; std = OilViscosityMatching.First().StdDeviation;
//            for (int i = 0; i < OilViscosityMatching.Count; i++)
//                if (OilViscosityMatching[i].StdDeviation < std)
//                { k = i; std = OilViscosityMatching[i].StdDeviation; }
//            OilViscosityCorr oilViscstyModel = (OilViscosityCorr)OilVis.GetValue(k);

//            k = 0; std = GasFVFMatching.First().StdDeviation;
//            for (int i = 0; i < GasFVFMatching.Count; i++)
//                if (GasFVFMatching[i].StdDeviation < std)
//                { k = i; std = GasFVFMatching[i].StdDeviation; }
//            ZfactorCorr gasCompressibilityModel = (ZfactorCorr)GasCom.GetValue(k);

//            k = 0; std = GasViscosityMatching.First().StdDeviation;
//            for (int i = 0; i < GasViscosityMatching.Count; i++)
//                if (GasViscosityMatching[i].StdDeviation < std)
//                { k = i; std = GasViscosityMatching[i].StdDeviation; }
//            GasViscosityCorr gasViscosityModel = (GasViscosityCorr)GasVis.GetValue(k);

//            return (pbRsBoModel, oilViscstyModel, gasCompressibilityModel, gasViscosityModel);
//        }

//        public void PrintResult(string matchingResultfile)
//        {
//            string resulttype = "Bubble Point";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, false);
//            var result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);

//            resulttype = "Solution GOR";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, true);
//            result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);

//            resulttype = "Oil FVF";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, true);
//            result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter3, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter4, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);

//            resulttype = "Oil Viscosity";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, true);
//            result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter3, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter4, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);

//            resulttype = "Gas FVF";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, true);
//            result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);

//            resulttype = "Gas Viscosity";
//            PrintResults.StringsToCSVRow(new string[] { resulttype }, matchingResultfile, true);
//            result = ExtractResult(resulttype);
//            PrintResults.StringsToCSVRow(result.ModelNames, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter1, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.Parameter2, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(result.StdDeviation, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);
//            PrintResults.StringsToCSVRow(new string[] { "" }, matchingResultfile, true);
//        }

//        (string[] ModelNames, string[] Parameter1, string[] Parameter2, string[] Parameter3, string[] Parameter4, string[] StdDeviation) ExtractResult(string resulttype)
//        {
//            string[] ModelNames = null, Parameter1 = null, Parameter2 = null, Parameter3 = null, Parameter4 = null, StdDeviation = null;
//            if (resulttype == "Bubble Point")
//            {
//                ModelNames = new List<string> { "" }.Concat(BubblePointMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(BubblePointMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(BubblePointMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(BubblePointMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }

//            if (resulttype == "Solution GOR")
//            {
//                ModelNames = new List<string> { "" }.Concat(SolutionGasMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(SolutionGasMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(SolutionGasMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(SolutionGasMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }

//            if (resulttype == "Oil FVF")
//            {
//                ModelNames = new List<string> { "" }.Union(OilFVFMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(OilFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(OilFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                Parameter3 = new List<string> { "Parameter 3" }.Concat(OilFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C3))).ToArray();
//                Parameter4 = new List<string> { "Parameter 4" }.Concat(OilFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C4))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(OilFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }

//            if (resulttype == "Oil Viscosity")
//            {
//                ModelNames = new List<string> { "" }.Concat(OilViscosityMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(OilViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(OilViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                Parameter3 = new List<string> { "Parameter 3" }.Concat(OilViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C3))).ToArray();
//                Parameter4 = new List<string> { "Parameter 4" }.Concat(OilViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C4))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(OilViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }


//            if (resulttype == "Gas FVF")
//            {
//                ModelNames = new List<string> { "" }.Concat(GasFVFMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(GasFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(GasFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(GasFVFMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }

//            if (resulttype == "Gas Viscosity")
//            {
//                ModelNames = new List<string> { "" }.Concat(GasViscosityMatching.Where(d => d.Name != null).Select(d => d.Name)).ToArray();
//                Parameter1 = new List<string> { "Parameter 1" }.Concat(GasViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C1))).ToArray();
//                Parameter2 = new List<string> { "Parameter 2" }.Concat(GasViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.C2))).ToArray();
//                StdDeviation = new List<string> { "Std Deviation" }.Concat(GasViscosityMatching.Where(d =>
//                d.Name != null).Select(d => string.Format("{0, 10:0.0000}", d.StdDeviation))).ToArray();
//            }

//            return (ModelNames, Parameter1, Parameter2, Parameter3, Parameter4, StdDeviation);
//        }
//    }
//}
