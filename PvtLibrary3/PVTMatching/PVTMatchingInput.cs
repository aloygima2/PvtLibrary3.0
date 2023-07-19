//using Microsoft.Office.Interop.Excel;
//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Runtime.InteropServices;
//using Excel = Microsoft.Office.Interop.Excel;

//namespace PvtLibrary3
//{
//    public class PVTMatchingInput: List<PvtDataRow>
//    {
//        public Fluid fluid { get; set; }
//        /// <summary>
//        /// Fluid Temperature in degree Fahrenheit
//        /// </summary>
//        public double Temperature { get; set; }
//        /// <summary>
//        /// Fluid Pressure in psia
//        /// </summary>
//        public double Pressure { get; set; }
//        public double LabBubblePoint { get; set; }

//        public double Pb { get; set; }

//        public void Read(string filename, string sheetname)
//        {
//            Application xlApp;
//            Workbook xlWorkBook;
//            Worksheet xlWorkSheet;
//            if (!filename.Contains(":")) filename = Directory.GetCurrentDirectory() + "\\" + filename;
//            xlApp = new Application();
//            xlWorkBook = xlApp.Workbooks.Open(filename, 0, true, 5, "", "", true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
//            xlWorkSheet = (Worksheet)xlWorkBook.Worksheets[sheetname];
//            Range range = xlWorkSheet.UsedRange;
//            range = xlWorkSheet.UsedRange;
//            int Rows = range.Rows.Count, Cols = range.Columns.Count;


//            fluid.Rs1 = (double)(range.Cells[4, 1] as Range).Value2;
//            fluid.Api = (double)(range.Cells[4, 2] as Range).Value2;
//            fluid.GasGravity = (double)(range.Cells[4, 3] as Range).Value2;
//            fluid.Salinity = (double)(range.Cells[4, 4] as Range).Value2;
//            fluid.MoleH2S = (double)(range.Cells[4, 5] as Range).Value2;
//            fluid.MoleCO2 = (double)(range.Cells[4, 6] as Range).Value2;
//            fluid.MoleN2 = (double)(range.Cells[4, 7] as Range).Value2;
//            Temperature = (double)(range.Cells[4, 8] as Range).Value2;
//            LabBubblePoint = (double)(range.Cells[4, 9] as Range).Value2;
           

//            List<string> Head = new List<string>();
//            for (int j = 1; j <= Cols; j++)
//            {
//                string str = (string)(range.Cells[6, j] as Range).Value2;
//                Head.Add(MapHeading(str));
//            }
//            for (int i = 1; i <= Rows - 6; i++)
//            {
//                PvtDataRow data = new PvtDataRow();
//                try
//                {

//                    for (int j = 0; j < Cols; j++)
//                    {
//                        double v = (double)(range.Cells[i + 6, j + 1] as Range).Value2;
//                        try
//                        {
//                            data.SetProperty(Head[j], v);
//                        }
//                        catch { }
//                    }
//                }
//                catch { Add(data); };
//            }

//            xlWorkBook.Close(true, null, null);
//            xlApp.Quit();
//            ReleaseObject(xlWorkSheet);
//            ReleaseObject(xlWorkBook);
//            ReleaseObject(xlApp);
//        }
        
//        static void ReleaseObject(object obj)
//        {
//            try { Marshal.ReleaseComObject(obj); obj = null; }
//            catch (Exception) { obj = null; }
//            finally { GC.Collect(); }
//        }

//        public string MapHeading(string input)
//        {
//            if (input == "GOR") return "GasOilRatio";
//            return input;
//        }
//    }
//}
