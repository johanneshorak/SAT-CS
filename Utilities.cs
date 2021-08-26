using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.OleDb;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace Icov
{
    public static class Utilities
    {
        public static String C_SEPARATOR = ";";

        public static DataSet Parse(string fileName)
        {
            string connectionString = string.Format("provider=Microsoft.Jet.OLEDB.4.0; data source={0};Extended Properties=Excel 8.0;", fileName);


            DataSet data = new DataSet();

            foreach (var sheetName in GetExcelSheetNames(connectionString))
            {
                using (OleDbConnection con = new OleDbConnection(connectionString))
                {
                    var dataTable = new DataTable();
                    dataTable.TableName = sheetName;

                    string query = string.Format("SELECT * FROM [{0}]", sheetName);
                    con.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, con);
                    adapter.Fill(dataTable);

                    data.Tables.Add(dataTable);
                }
            }

            return data;
        }

        public static string[] GetExcelSheetNames(string connectionString)
        {
            OleDbConnection con = null;
            DataTable dt = null;
            con = new OleDbConnection(connectionString);
            con.Open();
            dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            if (dt == null)
            {
                return null;
            }

            String[] excelSheetNames = new String[dt.Rows.Count];
            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                excelSheetNames[i] = row["TABLE_NAME"].ToString();
                i++;
            }

            return excelSheetNames;
        }

        public static double minValue(DataTable t, String column)
        {

            DataTable table = new DataView(t).ToTable(false, column);

            double minVal = 1e30;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][column] != DBNull.Value)
                    if (table.Rows[i].Field<double>(column) < minVal) minVal = (double)table.Rows[i].Field<double>(column);
            }

            return (minVal);
        }

        public static double maxValue(DataTable t, String column)
        {

            DataTable table = new DataView(t).ToTable(false, column);

            double maxVal = -1e30;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (table.Rows[i][column] != DBNull.Value)
                    if (table.Rows[i].Field<double>(column) > maxVal) maxVal = (double)table.Rows[i].Field<double>(column);
            }

            return (maxVal);
        }

        public static double minValue(DataPoints dp, int column)
        {
            double minVal = 1e30;

            for (int i = 0; i < dp.series.Points.Count; i++)
            {
                if (column == 0)
                {
                    if (dp.series.Points[i].XValue < minVal) minVal = dp.series.Points[i].XValue;
                }
                else if (column > 0)
                {
                    if (dp.series.Points[i].YValues[column - 1] < minVal) minVal = dp.series.Points[i].YValues[column - 1];
                }
            }

            return (minVal);
        }

        public static double maxValue(DataPoints dp, int column)
        {
            double maxVal = -1e30;

            for (int i = 0; i < dp.series.Points.Count; i++)
            {
                if (column == 0) {
                    if (dp.series.Points[i].XValue > maxVal) maxVal = dp.series.Points[i].XValue;
                }
                else if (column > 0)
                {
                    if (dp.series.Points[i].YValues[column - 1] > maxVal) maxVal = dp.series.Points[i].YValues[column - 1];
                }
            }

            return (maxVal);
        }

        public static double doubleValueAt(DataTable t, int c, int r)
        {
            //Debug.WriteLine("  doubleValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());
            if (r < t.Rows.Count)
            {
                if (t.Rows[r][c] != DBNull.Value)
                    return ((double)t.Rows[r][c]);
                else
                    return (0);
            }
            else
            {
                return (0);
            }
        }

        public static double doubleValueAt(DataTable t, String c, int r)
        {
            //Debug.WriteLine("  doubleValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());

            if (t.Rows[r][c] != DBNull.Value)
            {
                //Debug.WriteLine(t.Rows[r][c]);
                return ((double)t.Rows[r][c]);
            }
            else
                return (0);
        }

        public static DateTime dateValueAt(DataTable t, int c, int r)
        {
            //Debug.WriteLine("  doubleValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());

            if (t.Rows[r][c] != DBNull.Value)
                return ((DateTime)t.Rows[r][c]);
            else
                return (new DateTime());
        }

        public static DateTime dateValueAt(DataTable t, String c, int r)
        {
            //Debug.WriteLine("  ValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());

            if (t.Rows[r][c] != DBNull.Value)
                return ((DateTime)t.Rows[r][c]);
            else
                return (new DateTime());
        }

        public static double doubleValueAtShowNull(DataTable t, int c, int r)
        {
            //Debug.WriteLine("  doubleValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());
            if (r < t.Rows.Count)
            {
                if (t.Rows[r][c] != DBNull.Value)
                    return ((double)t.Rows[r][c]);
                else
                    return (Double.NaN);
            }
            else
                return (Double.NaN);
        }

        public static double doubleValueAtShowNull(DataTable t, String c, int r)
        {
            //Debug.WriteLine("  doubleValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());
            if (r < t.Rows.Count)
            {
                if (t.Rows[r][c] != DBNull.Value)
                    return ((double)t.Rows[r][c]);
                else
                    return (Double.NaN);
            }
            else
                return (Double.NaN);
        }

        public static String stringValueAt(DataTable t, String c, int r)
        {
            //Debug.WriteLine("  stringValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());

            if (t.Rows[r][c] != DBNull.Value)
                return (t.Rows[r][c].ToString());
            else
                return ("-notfound-");
        }

        public static String stringValueAt(DataTable t, int c, int r)
        {
            //Debug.WriteLine("  stringValueAt " + c + "," + r + ": " + t.Rows[r][c].ToString());
            if (r < t.Rows.Count)
            {
                if (t.Rows[r][c] != DBNull.Value)
                    return (t.Rows[r][c].ToString());
                else
                    return ("-notfound-");
            }
            else
                return ("-notfound-");
        }

        public static void populateMeteorologicDebugTable(ref DataGridView dgv, List<DataPoints> dp)
        {
            dgv.ColumnCount = dp.Count + 1;


            //Spalten durchlaufen
            for (int i = 0; i < dp.Count; i++)
            {
                dgv.Columns[i + 1].Name = dp[i].series.Name;

                for (int j = 0; j < dp[i].series.Points.Count; j++)
                {
                    dgv.Rows.Add("");
                }

                //Zeile für Zeile hinzufügen
                for (int j = 0; j < dp[i].series.Points.Count; j++)
                {
                    //if(j>=dgv.Rows.Count)
                    //    dgv.Rows.Add("");

                    dgv.Rows[j].Cells[0].Value = Math.Round(dp[i].series.Points[j].XValue, 2);
                    dgv.Rows[j].Cells[i + 1].Value = Math.Round(dp[i].series.Points[j].YValues[0], 2);
                }
            }

        }

        public static void ExpandToLevel(TreeNodeCollection nodes, int level)
        {
            if (level > 0)
            {
                foreach (TreeNode node in nodes)
                {
                    node.Expand();
                    ExpandToLevel(node.Nodes, level - 1);
                }
            }
        }

        public static void populateGridViewWithDataPoints(ref DataGridView dgv, DataPoints dp)
        {
            dgv.ColumnCount = 2;

            if (dp.series != null)
            {
                if (dp.series.Points.Count > 0)
                {
                    dgv.ColumnCount = 1 + dp.series.Points[0].YValues.Count();
                    dgv.Columns[0].Name = dp.xName;

                    for (int i = 0; i < dp.series.Points.Count; i++)
                    {
                        var c = new List<String>();

                        dgv.Rows.Add("");

                        dgv.Rows[i].Cells[0].Value = Math.Round(dp.series.Points[i].XValue, 2).ToString();

                        c.Add(dp.series.Points[i].XValue.ToString());
                        for (int j = 0; j < dp.series.Points[0].YValues.Count(); j++)
                        {
                            dgv.Columns[j + 1].Name = dp.yName;
                            dgv.Columns[j + 1].Width = 70;

                            dgv.Rows[i].Cells[j + 1].Value = Math.Round(dp.series.Points[i].YValues[j], 2).ToString();
                        }

                    }
                }
            }


        }

        public static String columnHeader(String name, String unit)
        {
            return (name + "(" + unit + ")");
        }

        public static int mod(int a, int n)
        {
            int result = a % n;
            if ((a < 0 && n > 0) || (a > 0 && n < 0))
                result += n;
            return result;
        }

        public static decimal mod(decimal a, decimal n)
        {
            int result = (int)a % (int)n;
            if ((a < 0 && n > 0) || (a > 0 && n < 0))
                result += (int)n;
            return (decimal)result;
        }
        

    }

    public class DataPoints
    {
        public Series series;

        public String xName;
        public String yName;

        public String Name
        {
            get { return this.series.Name;  }
            set { this.series.Name = value; }
        }

        private bool logToFile = false;
        public bool LogToFile
        {
            get { return (logToFile); }
            set { logToFile = value; }
        }

        private System.IO.StreamWriter associatedFile;


        Log eventLog;

        public DataPoints(Log log)
        {
            series = new Series();
            xName = "x";
            yName = "y";

            eventLog = log;
        }

        public void clear()
        {
            series = new Series();
        }

        public int Length {
            get { return series.Points.Count; }
        }

        public double getInterpolatedValueAt(double x) {

            double xFound = 0;
            double y = double.NaN;
            int iLeft = -1;
            int iRight = -1;
            int i = 0;

            if (series.Points.Count == 0)
                return (double.NaN);

            //2015-10-13 1347 - Effizienterer Weg um Position in Tabelle zu finden. Voraussetzung: Equidistante x-Achse!
            double xMin = this.FirstX;
            double xMax = this.LastX;
            double dx = series.Points[1].XValue-xMin;
            if ( (xMin < x)  && (x < xMax) )
            {
                i = (int)Math.Floor((x-xMin) / dx);
                iLeft = i;
                iRight = i+1;
            }
            else if (x <= xMin)
            {
                iLeft = 0;
                iRight = 1;
            }
            else if (x >= xMax)
            {
                iLeft = series.Points.Count - 2;
                iRight = series.Points.Count - 1;
            }

            if ((x - series.Points[i].XValue >= dx) && ((xMin < x) && (x < xMax)))
            {
                //Debug.WriteLine("Fehler beim interpolieren - Schritte in Tabelle nicht äquidistant!"+this.ToString());
                //Debug.WriteLine();
                eventLog.add(new LogEntry("DataPoints.getInterpolatedValueAt(double)", "Interpolated for series " + this.series.Name + ": x-Values not equidistant? x = " + x + "\t i = " + i + "\t x(i) = " + series.Points[i].XValue + "\t y(i) = " + series.Points[i].YValues[0], Log.LOG_ERROR_TYPE));
            }


            if (iLeft == -1) eventLog.add(new LogEntry("DataPoints.getInterpolatedValueAt(double)", "Interpolated for series " + this.series.Name + " at x=" + x + " - value out of bounds LEFT! "+iLeft+","+iRight+"/"+series.Points.Count,Log.LOG_ERROR_TYPE));
            else if (iRight >= series.Points.Count)
            {
                if (series.Points.Count == 1)
                {
                    return (series.Points[0].YValues[0]);
                }
                else
                    eventLog.add(new LogEntry("DataPoints.getInterpolatedValueAt(double)", "Interpolated for series " + this.series.Name + " at x=" + x + " - value out of bounds RIGHT!" + iLeft + "," + iRight + "/" + series.Points.Count, Log.LOG_ERROR_TYPE));
            }
            else
            {
                // for now: linear interpolation - might add other methods too
                double xLeft = series.Points[iLeft].XValue;
                double xRight = series.Points[iRight].XValue;
                double yLeft = series.Points[iLeft].YValues[0];
                double yRight = series.Points[iRight].YValues[0];

                double slope = (yRight - yLeft) / (xRight - xLeft);
                double offset = yLeft;

                y = slope * (x - xLeft) + offset;
            }

            return(y);            
        }

        //Wenn die Werte zB. 0-360° sind, und bei 351° interpoliert werden soll gibts ein Problem wenn de rletzte eingetragene bei 351 liegt. Das wird hier behoben.
        public double getAngularInterpolatedValueAt(double x)
        {

            double xFound = 0;
            double y = double.NaN;
            int iLeft, iRight;
            int i = 0;

            if (x > 360)
            {
                x = x - 360 * Math.Round((x / 360), 0);
            }


            while ((i < series.Points.Count) && (x > series.Points[i].XValue))
            {
                xFound = series.Points[i].XValue;
                i++;
            }

            if (x == series.Points[0].XValue)
            {
                iLeft = 0;
                iRight = 1;
            }
            else if (x == series.Points[series.Points.Count - 1].XValue)
            {
                iLeft = series.Points.Count - 2;
                iRight = series.Points.Count - 1;
            }
            else
            {
                iLeft = i - 1;
                iRight = i;
            }

            if (iLeft == -1) eventLog.add(new LogEntry("DataPoints.getCircularInterpolatedValueAt(double)", "Interpolated for series " + this.series.Name + " at x=" + x + " - value out of bounds LEFT! " + iLeft + "," + iRight + "/" + series.Points.Count, Log.LOG_ERROR_TYPE));
            else
            {
                double yRight = 0;
                double xRight = 0;

                if (series.Points.Count == 1)
                {
                    return (series.Points[0].YValues[0]);
                }
                else if (iRight == series.Points.Count)
                {
                    xRight = 360;
                    yRight = series.Points[0].YValues[0];
                }
                else
                {
                    xRight = series.Points[iRight].XValue;
                    yRight = series.Points[iRight].YValues[0];
                }
                // for now: linear interpolation - might add other methods too
                double xLeft = series.Points[iLeft].XValue;

                double yLeft = series.Points[iLeft].YValues[0];


                double slope = (yRight - yLeft) / (xRight - xLeft);
                double offset = yLeft;

                y = slope * (x - xLeft) + offset;
            }

            return (y);
        }

        public DataPoints populateWithDataTable(DataTable t, String cX, String cY)
        {
            double valX = 0;
            double valY = 0;
            int i = 0;

            xName = cX;
            yName = cY;

            while (!(double.IsNaN(valX)) && !(double.IsNaN(valY)))
            {
                valX = Utilities.doubleValueAtShowNull(t, cX, i);
                valY = Utilities.doubleValueAtShowNull(t, cY, i);

                if (!(double.IsNaN(valX)) && !(double.IsNaN(valY))) { series.Points.AddXY(valX, valY); }
                else if ((double.IsNaN(valX)) && !(double.IsNaN(valY))) { Debug.WriteLine("DataPoints.populateWithDataTable(DataTable, String, String): valX = null while valY != null"); }
                else if (!(double.IsNaN(valX)) && (double.IsNaN(valY))) { Debug.WriteLine("DataPoints.populateWithDataTable(DataTable, String, String): valX != null while valY = null"); }

                i++;
            }
            return (this);

        }

        public DataPoints populateXValuesFromDataTable(DataTable t, String cX)
        {
            double valX = 0;
            
            int i = 0;

            xName = cX;
            

            while (!double.IsNaN(valX))
            {
                valX = Utilities.doubleValueAtShowNull(t, cX, i);
                

                if (!double.IsNaN(valX))         { series.Points.AddXY(valX,0); }
                else if (double.IsNaN(valX))    { Debug.WriteLine("DataPoints.populateXValuesFromDataTable(DataTable, String,): valX = null!"); }
                i++;
            }
            return (this);

        }

        public String ToString()
        {
            String returnString = "";
            for(int i = 0; i < series.Points.Count; i++)
            {
                returnString += "(" + series.Points[i].XValue + ", " + series.Points[i].YValues[0] + ") ";
            }
            return (returnString);
        }

        public void addPoints(double x, double y)
        {
            if (LogToFile == false)
            {
                series.Points.AddXY(x, y);
            }
            else
            {
                associatedFile.WriteLine(x.ToString() + Utilities.C_SEPARATOR + y.ToString());

                series.Points.AddXY(x, y);
                // Wenn in eine Datei gelogged wird behalten wir nur die jeweils letzten 10 Punkte im Speicher.
                if (series.Points.Count > 10)
                {
                    series.Points.RemoveAt(0);
                }
            }
        }

        public void setFileLogging(System.IO.StreamWriter file)
        {
            associatedFile = file;
            LogToFile = true;

            associatedFile.AutoFlush = true;

            //Alle bereits vorhandenen Daten in die Datei schreiben.
            for(int i = 0;i<series.Points.Count;i++)
            {
                file.WriteLine(series.Points.ElementAt(i).XValue.ToString() + Utilities.C_SEPARATOR + series.Points.ElementAt(i).YValues[0].ToString());
            }
        }

        public double FirstX
        {
            get { return (series.Points[0].XValue); }
        }


        public double LastX {
        
            get { return(series.Points[series.Points.Count - 1].XValue); }
        }

        public double FirstY
        {
            get { return (series.Points[0].YValues[0]); }
        }

        public double LastY
        {
            get { return (series.Points[series.Points.Count - 1].YValues[0]); }
        }

        public double SecondToLastX
        {
            get { return (series.Points[series.Points.Count - 2].XValue); }
        }


        public double SecondToLastY
        {
            get { return (series.Points[series.Points.Count - 2].YValues[0]); }
        }


        public double XValueAt(int position)
        {
            return (this.series.Points[position].XValue);
        }

        public double YValueAt(int position)
        {
            return (this.series.Points[position].YValues[0]);
        }

        public DataPoints populateXValuesWithEquidistantValues(double xMin, double xMax, double dX)
        {
            double _time = DateTime.Now.Ticks;

            if (this.series != null)
                this.clear();

            if (xMin > xMax)
            {
                eventLog.add(new LogEntry("DataPoints.populateXValuesWithEquidistantValues(double)", "Tried to populated series " + this.series.Name + " with dX="+dX+" in interval "+xMin+" - "+xMax+" - xMin>xMax!", Log.LOG_ERROR_TYPE));
            }

            if (dX == 0.0)
            {
                eventLog.add(new LogEntry("DataPoints.populateXValuesWithEquidistantValues(double)", "Tried to populated series " + this.series.Name + " with dX=" + dX + " - not possible!", Log.LOG_ERROR_TYPE));
            }
            else
            {
                double x = xMin;

                while (x <= xMax)
                {
                    series.Points.AddXY(x, 0);
                    x += dX;
                }
            }

            eventLog.add(new LogEntry("DataPoints.populateXValuesWithEquidistantValues(double)", "Populated series " + this.series.Name + " with dX=" + dX + " in interval " + xMin + " - " + xMax + " and needed " + (-_time + DateTime.Now.Ticks) / TimeSpan.TicksPerSecond + " seconds", Log.LOG_INFO_TYPE));

            return (this);
        }



    }
}
