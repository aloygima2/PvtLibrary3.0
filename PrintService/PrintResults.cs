using System;
using System.IO;

namespace PrintService
{
    public class PrintResults
    {
        /// <summary>
        /// Writes an array of strings on a row in csv
        /// </summary>
        /// <param name="description">Array of strings to be written</param>
        /// <param name="filepath">The file to be written to</param>
        /// <param name="write">value is true to append to the file, and false to overwrite the file</param>
        public static void StringsToCSVRow(string[] description, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < description.Length; i++)
                    {
                        if (i < description.Length - 1)
                        {
                            file.Write(description[i] + ",");
                        }
                        else
                        {
                            file.WriteLine(description[i] + ",");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        public static void StringsToCSVCol(string[] description, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < description.Length; i++)
                    {
                        file.WriteLine(description[i] + ",");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        /// <summary>
        /// Writes an array of doubles on a row in csv
        /// </summary>
        /// <param name="value">Array of doubles to be written</param>
        /// <param name="filepath">The file to be written to</param>
        /// <param name="write">>value is true to append to the file, and false to overwrite the file</param>
        public static void ValuesToCSVRow(double[] value, string filepath, bool write)
        {
            string str = "";
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (Math.Abs(value[i]) > 1e-4)
                            str = string.Format("{0, 10:0.0000}", value[i]);
                        else
                            str = string.Format("{0, 10:0.00E00}", value[i]);
                        if (i < value.Length - 1)
                            file.Write(str + ",");
                        else
                            file.WriteLine(str + ",");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        /// <summary>
        /// Writes rows of pairs of strings and values to csv
        /// </summary>
        /// <param name="description">Title of value</param>
        /// <param name="values">number value</param>
        /// <param name="filepath">Path of file to be written to</param>
        /// <param name="write">True to append to file and false to overwrite</param>
        public static void StringsValuesToCSVCol(string[] description, double[] values, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < description.Length; i++)
                    {
                        file.WriteLine(description[i] + "," + values[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        public static void TwoValuesToCSVCol(double[] value1, double[] value2, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < value1.Length; i++)
                    {
                        file.WriteLine(value1[i] + "," + value2[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        public static void TwoStringsToCSVCol(string[] choice, string[] selectedItem, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    for (int i = 0; i < choice.Length; i++)
                    {
                        file.WriteLine(choice[i] + "," + selectedItem[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }

        public static void StringToCSVRow(string description, string filepath, bool write)
        {
            try
            {
                using (StreamWriter file = new StreamWriter(@filepath, write))
                {
                    file.Write(description);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("The program failed to write: ", ex);
            }
        }
    }
}
