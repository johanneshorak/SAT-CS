using System;
using System.Globalization;
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

using System.Threading;
using System.IO;

namespace Icov
{

    public partial class Icov : Form
    {
        String[] simulationsToRun =
{
                "D:\\users\\hosch\\all folders\\work\\vuw\\simulation_batch_test\\V-W-000\\26.08.1000-1145\\",
                "D:\\users\\hosch\\all folders\\work\\vuw\\simulation_batch_test\\V-W-011\\06.09.1153-1305\\",
                "D:\\users\\hosch\\all folders\\work\\vuw\\simulation_batch_test\\V-W-100\\07.08.0656-1156\\",
                "D:\\users\\hosch\\all folders\\work\\vuw\\simulation_batch_test\\V-W-100\\25.08.1156-1700\\"
            };
        int simulationRunning = 0;
        //String InputBasePath = "D:\\users\\hosch\\all folders\\work\\vuw\\simulations\\";
        //String InputSimPath = "A 06.09.1153-06.09.1305-V-W-011";
        String InputPath = "";

        //Versionsgeschichte
        //1.1   ... Es wird nun angenommen, dass langwellige Strahlung die am äußeren Fahrzeugboden eintrifft 
        //          von einer Oberfläche mit Temperatur TA(t) abgegeben wird. Also dass der Boden tatsächlich
        //          nur etwa Lufttemperatur hat. Annahme kommt daher, da ja dort keine direkte solare Strahlung
        //          zur Aufheizung beitragen kann.
        //1.2   ... Variable tauS zu Klasse Surface hinzugefügt. Bei der Transmissionsberechnung wird nun auf
        //          diese zurückgegriffen.
        public static String C_VERSION = "1.2";

        Physics physics;

        Log eventLog;
        DateTime simulationStart;
        DateTime simulationEnd;

        Stopwatch stopwatch = new Stopwatch();

        //List<Wall> walls = new List<Wall>();
        //List<List<Wall>> wallBauteil = new List<List<Wall>>();

        List<Material> materials = new List<Material>();

        DataPoints messwertFahrzeugTiMittelwert;
        DataPoints messwertFahrzeugTiMittelwertUnten;
        DataPoints messwertFahrzeugTiMittelwertMitte;
        DataPoints messwertFahrzeugTiMittelwertOben;

        DataTable allgemeinTable;
        DataTable meteorologyTable;
        DataTable horizonTable;
        DataTable sonnenwegTable;
        DataTable mittelwerteTable;

        DataTable parameterSimulationTable;

        DataTable parameterMaterialdatenTable;
        DataTable parameterFlaechendefTable;
        DataTable parameterBauteildefTable;
        DataTable messdatenFahrzeugTable;

        String sheetAllgmeinID = "allgemein$";
        String sheetMeteorologyID = "meteorologisch$";
        String sheetHorizonID = "heinsch$";
        String sheetMessdatenFahrzeugID = "fahrzeug$";
        String sheetSonnenwegID = "sonnenweg$";
        String sheetMittelWertID = "mittelwerte$";

        String columnZeitInStundenID = "Zeit (h)";
        String columnZeitInStundenString = "Zeit";
        String columnZeitInStundenUnit = "h";
        String shortNameZeitInStunden = "t";

        String columnAmbientTemperatureID = "T_A";
        String columnAmbientTemperatureString = "Umgebungstemperatur";
        String columnAmbientTemperatureUnit = "°C";
        String shortNameAmbientTemperature = "T_A";


        String columnGlobalRadiationID = "G";
        String columnGlobalRadiationString = "Globalstrahlung";
        String columnGlobalRadiationUnit = "W/m²";

        String columnDiffuseRadiationID = "H";
        String columnDiffuseRadiationString = "Diffuse Strahlung";
        String columnDiffuseRadiationUnit = "W/m²";

        String columnWindSpeed10mID = "v_10";
        String columnWindSpeed10mString = "Windgeschw. in 10m";
        String columnWindSpeed10mUnit = "m/s";

        String columnWindSpeed1mID = "v_1";
        String columnWindSpeed1mString = "Windgeschw. in 1m";
        String columnWindSpeed1mUnit = "m/s";

        String columnHorizonAlphaID = "alpha";
        String columnHorizonAlphaString = "α";
        String columnHorizonAlphaUnit = "°";

        String columnHorizonGammaID = "gamma";
        String columnHorizonGammaString = "γ";
        String columnHorizonGammaUnit = "°";

        String columnDatumID = "Datum";
        String columnDatumString = "Datum";
        String columnDatumEinheit = "";

        String columnZeitID = "Zeit";
        String columnZeitString = "Zeit";
        String columnZeitEinheit = "";

        String columnFahrzeugMittelUntenID = "mittel unten";
        String columnFahrzeugMittelUntenString = "Mittel Unten";
        String columnFahrzeugMittelUntenEinheit = "°C";

        String columnFahrzeugMittelMitteID = "mittel mitte";
        String columnFahrzeugMittelMitteString = "Mittel Mitte";
        String columnFahrzeugMittelMitteEinheit = "°C";

        String columnFahrzeugMittelObenID = "mittel oben";
        String columnFahrzeugMittelObenString = "Mittel Oben";
        String columnFahrzeugMittelObenEinheit = "°C";

        String columnFahrzeugMittelID = "mittel gesamt";
        String columnFahrzeugMittelString = "Mittelwert";
        String columnFahrzeugMittelEinheit = "°C";


        String sheetSimulationParameterMaterialdatenID = "materialdaten$";
        String sheetSimulationParameterFlaechendefinitionID = "flaechendefinition$";
        String sheetSimulationParameterBauteildefinitionID = "bauteile$";
        String sheetSimulationParameterID = "allgemein$";

        String columnParamerterWertID = "Wert";
        int rowTOffsetIntID = 7;

        String columnBlechID = "Blech";
        int rowAlphaSBlechIntID = 0;
        int rowTauSBlechIntID = 1;
        int rowAlphaLBlechIntID = 2;
        int rowCVBlechIntID = 4;
        int rowLambdaBlechIntID = 5;
        int rowRhoBlechIntID = 6;

        String columnGlasID = "Glas";
        int rowAlphaSGlasIntID = 0;
        int rowTauSGlasIntID = 1;
        int rowAlphaLGlasIntID = 2;
        int rowCVGlasIntID = 4;
        int rowLambdaGlasIntID = 5;
        int rowRhoGlasIntID = 6;

        String columnLuftID = "Luft";
        int rowAlphaSLuftIntID = 0;
        int rowTauSLuftIntID = 1;
        int rowAlphaLLuftIntID = 2;
        int rowCVLuftIntID = 4;
        int rowLambdaLuftIntID = 5;
        int rowRhoLuftIntID = 6;

        String columnIsolatorID = "Isolator";
        int rowAlphaSIsolatorIntID = 0;
        int rowTauSIsolatorIntID = 1;
        int rowAlphaLIsolatorIntID = 2;
        int rowCVIsolatorIntID = 4;
        int rowLambdaIsolatorIntID = 5;
        int rowRhoIsolatorIntID = 6;

        String columnLaengeID = "Fahrzeuglänge";
        String columnBreiteID = "Fahrzeugbreite";
        String columnHoeheID = "Fahrzeughöhe";

        int rowLaengeIntID = 0;
        int rowBreiteIntID = 0;
        int rowHoeheIntID = 0;

        String columnAlphaID = "α";
        String columnBetaID = "β";

        int rowAlphaIntID = 0;
        int rowBetaIntID = 0;

        String columnAnteilTypA = "A";
        String columnAnteilTypB = "B";
        String columnAnteilTypC = "C";
        String columnAnteilTypD = "D";

        int rowAnteilTypOffset = 0;

        String columnBauteilAMaterial = "MaterialA";
        String columnBauteilBMaterial = "MaterialB";
        String columnBauteilCMaterial = "MaterialC";
        String columnBauteilDMaterial = "MaterialD";

        String columnBauteilAStaerke = "StaerkeA";
        String columnBauteilBStaerke = "StaerkeB";
        String columnBauteilCStaerke = "StaerkeC";
        String columnBauteilDStaerke = "StaerkeD";

        int timeStepsTotal = 0;
        int timeStepNow = 0;

        Boolean initialization = true;
        Boolean batch_mode = false;

        private System.Windows.Forms.Timer timer1;

        double tOffset=0;

        TreeNode rootNode = new TreeNode("Fahrzeug");

        private KeyValuePair<int, string> getKeyValuePair(BindingList<KeyValuePair<int, string>> kvp, String val)
        {
            for (int i = 0; i < kvp.Count(); i++)
            {
                if (kvp[i].Value == val) return kvp[i];
            }

            return (new KeyValuePair<int, string>(-1, "[wählen]"));
        }






        public void regenerate_charts()
        {

            this.chartFahrzeugMessung.ChartAreas.Clear();
            this.chartFahrzeugMessung.Legends.Clear();
            this.chartFahrzeugMessung.Series.Clear();

            this.chartAmbientTemperature.ChartAreas.Clear();
            this.chartAmbientTemperature.Legends.Clear();
            this.chartAmbientTemperature.Series.Clear();

            this.chartWindSpeed.ChartAreas.Clear();
            this.chartWindSpeed.Legends.Clear();
            this.chartWindSpeed.Series.Clear();

            this.chartkwStrahlung.ChartAreas.Clear();
            this.chartkwStrahlung.Legends.Clear();
            this.chartkwStrahlung.Series.Clear();

            this.chartHorizont.ChartAreas.Clear();
            this.chartHorizont.Legends.Clear();
            this.chartHorizont.Series.Clear();


            ChartArea chartArea1 = new ChartArea();
            Legend legend1 = new Legend();
            ChartArea chartArea2 = new ChartArea();
            Legend legend2 = new Legend();
            ChartArea chartArea3 = new ChartArea();
            Legend legend3 = new Legend();
            ChartArea chartArea4 = new ChartArea();
            Legend legend4 = new Legend();
            ChartArea chartArea5 = new ChartArea();
            Legend legend5 = new Legend();
            ChartArea chartArea6 = new ChartArea();
            Legend legend6 = new Legend();
            ChartArea chartArea7 = new ChartArea();
            Legend legend7 = new Legend();
            ChartArea chartArea8 = new ChartArea();
            Legend legend8 = new Legend();

            chartArea5.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea5.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea5.Name = "ChartArea1";
            this.chartFahrzeugMessung.ChartAreas.Add(chartArea5);
            legend5.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend5.Name = "Legend1";

            this.chartFahrzeugMessung.Legends.Add(legend5);
            //this.chartFahrzeugMessung.Location = new System.Drawing.Point(4, 4);
            //this.chartFahrzeugMessung.Margin = new System.Windows.Forms.Padding(4);
            this.chartFahrzeugMessung.Name = "chartFahrzeugMessung";
            //this.chartFahrzeugMessung.Size = new System.Drawing.Size(1219, 926);
            this.chartFahrzeugMessung.TabIndex = 6;
            this.chartFahrzeugMessung.Text = "chartFahrzeugMessung";

            chartArea4.Name = "ChartArea1";
            this.chartAmbientTemperature.ChartAreas.Add(chartArea4);
            legend4.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend4.Name = "Legend1";
            this.chartAmbientTemperature.Legends.Add(legend4);
            //this.chartAmbientTemperature.Location = new System.Drawing.Point(21, 516);
            //this.chartAmbientTemperature.Margin = new System.Windows.Forms.Padding(4);
            this.chartAmbientTemperature.Name = "chartAmbientTemperature";
            //this.chartAmbientTemperature.Size = new System.Drawing.Size(544, 410);
            this.chartAmbientTemperature.TabIndex = 3;
            this.chartAmbientTemperature.Text = "chartAmbientTemperature";
            // 
            // chartkwStrahlung
            // 
            this.chartkwStrahlung.BorderlineWidth = 2;
            this.chartkwStrahlung.BorderSkin.BorderWidth = 5;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.Name = "ChartArea1";
            this.chartkwStrahlung.ChartAreas.Add(chartArea1);
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.chartkwStrahlung.Legends.Add(legend1);
            //this.chartkwStrahlung.Location = new System.Drawing.Point(8, 7);
            //this.chartkwStrahlung.Margin = new System.Windows.Forms.Padding(4);
            this.chartkwStrahlung.Name = "chartkwStrahlung";
            //this.chartkwStrahlung.Size = new System.Drawing.Size(557, 410);
            this.chartkwStrahlung.TabIndex = 2;
            this.chartkwStrahlung.Text = "chart1";
            // 
            // chartWindSpeed
            // 
            chartArea2.Name = "ChartArea1";
            this.chartWindSpeed.ChartAreas.Add(chartArea2);
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Name = "Legend1";
            this.chartWindSpeed.Legends.Add(legend2);
            //this.chartWindSpeed.Location = new System.Drawing.Point(661, 7);
            //this.chartWindSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.chartWindSpeed.Name = "chartWindSpeed";
            //this.chartWindSpeed.Size = new System.Drawing.Size(557, 410);
            this.chartWindSpeed.TabIndex = 2;
            this.chartWindSpeed.Text = "chart3";
            // 
            // chartHorizont
            // 
            chartArea3.Name = "ChartArea1";
            this.chartHorizont.ChartAreas.Add(chartArea3);
            legend3.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend3.Name = "Legend1";
            this.chartHorizont.Legends.Add(legend3);
            //this.chartHorizont.Location = new System.Drawing.Point(661, 516);
            //this.chartHorizont.Margin = new System.Windows.Forms.Padding(4);
            this.chartHorizont.Name = "chartHorizont";
            //this.chartHorizont.Size = new System.Drawing.Size(557, 410);
            this.chartHorizont.TabIndex = 2;
            this.chartHorizont.Text = "chart4";
        }

        public void loadSimulation(String path)
        {
            DataSet dataSetInput = new DataSet();
            DataSet dataSetParameter = new DataSet();
            decimal conversionInputBauteilStaerke = 1000;

            Debug.WriteLine("Loading scenario from " + path);

            physics = new Physics(eventLog);

            dataSetInput = Utilities.Parse(path + "\\input.xls"); //Utilities.Parse(InputBasePath + InputSimPath + "\\input.xls");
            dataSetParameter = Utilities.Parse(path + "\\simulation_parameter.xls");

            int i = 0;
            foreach (DataTable table in dataSetParameter.Tables)
            {
                int j = 0;
                //Debug.WriteLine(i + ". " + table.TableName);
                i++;

                foreach (DataColumn column in table.Columns)
                {
                    //Debug.WriteLine("  " + j + ". " + column.ColumnName);
                    j++;
                }
            }

            if (checkBoxFixSun.Checked)
            {
                physics.fixSun((double)numericUpDownFixedGamma.Value, (double)numericUpDownFixedPsi.Value);
                labelSonnenpositionFixiert.Text = "Achtung! Sonnenposition fixiert in Icov.basicInit() auf " + physics.gamma(2000) + "° und " + physics.psi(2000) + "° !";
            }
            else
            {
                physics.SunFixed = false;
                labelSonnenpositionFixiert.Text = "Sonnenposition variabel!";
            }


            meteorologyTable = dataSetInput.Tables[sheetMeteorologyID];
            horizonTable = dataSetInput.Tables[sheetHorizonID];
            sonnenwegTable = dataSetInput.Tables[sheetSonnenwegID];
            mittelwerteTable = dataSetInput.Tables[sheetMittelWertID];

            parameterSimulationTable = dataSetParameter.Tables[sheetSimulationParameterID];
            parameterMaterialdatenTable = dataSetParameter.Tables[sheetSimulationParameterMaterialdatenID];
            parameterFlaechendefTable = dataSetParameter.Tables[sheetSimulationParameterFlaechendefinitionID];
            parameterBauteildefTable = dataSetParameter.Tables[sheetSimulationParameterBauteildefinitionID];
            messdatenFahrzeugTable = dataSetInput.Tables[sheetMessdatenFahrzeugID];
            messdatenFahrzeugTable = dataSetInput.Tables[sheetMessdatenFahrzeugID];

            regenerate_charts();
            //chartFahrzeugMessung.ChartAreas[0] = new ChartArea();
            //chartAmbientTemperature.ChartAreas[0] = new ChartArea();
            //chartWindSpeed.ChartAreas[0] = new ChartArea();
            //chartkwStrahlung.ChartAreas[0] = new ChartArea();
            //chartHorizont.ChartAreas[0] = new ChartArea();

            if (messdatenFahrzeugTable == null)
            {
                eventLog.add(new LogEntry(this.ToString(), "Tabellenblatt " + sheetMessdatenFahrzeugID + " konnte nicht gefunden werden!\r\nLade keine Fahrzeugmessdaten...", Log.LOG_WARN_TYPE));
            }


            DataView meteorologyView = new DataView(meteorologyTable);

            // ============ Extract Date and Time ============================
            if (messdatenFahrzeugTable != null)
            {
                datePicker.Value = Utilities.dateValueAt(messdatenFahrzeugTable, columnDatumID, 0);
                physics.StartDatum = datePicker.Value;
                Debug.WriteLine("Datepicker Value changed " + datePicker.Value + " DOY: " + datePicker.Value.DayOfYear);
                timePickerFahrzeugdaten.Value = Utilities.dateValueAt(messdatenFahrzeugTable, columnZeitID, 0);
            }
            DateTime simStartZeit = Utilities.dateValueAt(messdatenFahrzeugTable, columnZeitID, 0);
            //Fahrzeug Uhrzeiten sind in Sommerzeit angegeben! Daher simStartZeit.Hour-1!!!!
         
            DateTime metStartZeit = Utilities.dateValueAt(meteorologyTable, columnZeitID, 0);
            DateTime metStartDat = Utilities.dateValueAt(meteorologyTable, columnDatumID, 0);

            tOffset = Utilities.doubleValueAt(parameterSimulationTable, columnParamerterWertID, rowTOffsetIntID);

            int tOffsetHours = (int)Math.Round(tOffset);
            int tOffsetMinutes = (int)Math.Round((tOffset - (double)tOffsetHours) * 60);

            int simStartZeitMinuten = (simStartZeit.Minute + tOffsetMinutes)%60;
            if (simStartZeitMinuten < 0) simStartZeitMinuten += 60;

            int simStartZeitStunden = simStartZeit.Hour - 1;

            if (simStartZeit.Minute+tOffsetMinutes > 59)
            {
                simStartZeitStunden += tOffsetHours + 1;
            } else if (simStartZeit.Minute + tOffsetMinutes < 0)
            {
                simStartZeitStunden += tOffsetHours - 1;
            }
            else
            {
                simStartZeitStunden += tOffsetHours;
            }

            numericUpDownTOffset.Value = (decimal)tOffset;

            timePicker.Value = new DateTime(simStartZeit.Year, simStartZeit.Month, simStartZeit.Day, simStartZeitStunden, simStartZeitMinuten, simStartZeit.Second);
            physics.StartZeit = timePicker.Value;
            dateTimePickerMeasurementStart.Value = new DateTime(simStartZeit.Year, simStartZeit.Month, simStartZeit.Day, simStartZeit.Hour-1, simStartZeit.Minute, simStartZeit.Second);

            physics.StartMeteorologieDaten = new DateTime(metStartDat.Year, metStartDat.Month, metStartDat.Day, metStartZeit.Hour, metStartZeit.Minute, metStartZeit.Second);
            dateTimeMeteorologieDaten.Value = physics.StartMeteorologieDaten;

            // ============ Extract Surface Roughness =================================
            this.parSurfaceRoughness.Value = (decimal)Utilities.doubleValueAt(mittelwerteTable, 1, 23);

            // ============ Extract Simulation Parameters =============================
            physics.Deltat = 1 / 3600;//((double)numericUpDownDT.Value/(double)3600);//0.01 /3600;//Utilities.doubleValueAt(parameterSimulationTable, 1, 0) / 3600;
            physics.Deltax = 1;// (double)numericUpDownDX.Value;

            //this.numericUpDownDT.Value = (decimal)Math.Round(physics.Deltat * 3600, 2);
            //this.numericUpDownDX.Value = (decimal)physics.Deltax;

            
            this.numericUpDownR_SEnv.Value = (decimal)Utilities.doubleValueAt(parameterSimulationTable, 1, 4);
            this.numericUpDownE_LEnv.Value = (decimal)Utilities.doubleValueAt(parameterSimulationTable, 1, 5);
            this.numericUpDownE_LGnd.Value = (decimal)Utilities.doubleValueAt(parameterSimulationTable, 1, 6);
            this.numericUpDownAlpha_I.Value = (decimal)Utilities.doubleValueAt(parameterSimulationTable, 1, 8);
            this.numericUpDownAlpha_Z.Value = (decimal)Utilities.doubleValueAt(parameterSimulationTable, 1, 9);



            // ============ Extract Latitude and Longitude ============================
            numericUpDownLatitude.Value = (Decimal)Utilities.doubleValueAt(sonnenwegTable, 1, 0);
            numericUpDownLongitude.Value = (Decimal)Utilities.doubleValueAt(sonnenwegTable, 1, 1);
            latitudelongitudeValuesChange();


            // ============ EXTRACT MEASURED TEMPERATURES IN VEHICLE IF TABLE IS LOADED =============
            if (messdatenFahrzeugTable != null)
            {
                messwertFahrzeugTiMittelwert = new DataPoints(eventLog);
                messwertFahrzeugTiMittelwertUnten = new DataPoints(eventLog);
                messwertFahrzeugTiMittelwertMitte = new DataPoints(eventLog);
                messwertFahrzeugTiMittelwertOben = new DataPoints(eventLog);

                messwertFahrzeugTiMittelwert.populateWithDataTable(messdatenFahrzeugTable, columnZeitInStundenID, columnFahrzeugMittelID);
                messwertFahrzeugTiMittelwertUnten.populateWithDataTable(messdatenFahrzeugTable, columnZeitInStundenID, columnFahrzeugMittelUntenID);
                messwertFahrzeugTiMittelwertMitte.populateWithDataTable(messdatenFahrzeugTable, columnZeitInStundenID, columnFahrzeugMittelMitteID);
                messwertFahrzeugTiMittelwertOben.populateWithDataTable(messdatenFahrzeugTable, columnZeitInStundenID, columnFahrzeugMittelObenID);

                messwertFahrzeugTiMittelwert.series.Name = columnFahrzeugMittelID;
                messwertFahrzeugTiMittelwert.series.ChartType = SeriesChartType.Line;
                messwertFahrzeugTiMittelwert.series.MarkerStyle = MarkerStyle.Circle;
                messwertFahrzeugTiMittelwert.series.LegendText = columnFahrzeugMittelString;
                messwertFahrzeugTiMittelwert.series.Color = Color.OrangeRed;

                messwertFahrzeugTiMittelwertUnten.series.Name = columnFahrzeugMittelUntenID;
                messwertFahrzeugTiMittelwertUnten.series.ChartType = SeriesChartType.Line;
                messwertFahrzeugTiMittelwertUnten.series.MarkerStyle = MarkerStyle.Circle;
                messwertFahrzeugTiMittelwertUnten.series.LegendText = columnFahrzeugMittelUntenString;
                messwertFahrzeugTiMittelwertUnten.series.Color = Color.DarkRed;

                messwertFahrzeugTiMittelwertMitte.series.Name = columnFahrzeugMittelMitteID;
                messwertFahrzeugTiMittelwertMitte.series.ChartType = SeriesChartType.Line;
                messwertFahrzeugTiMittelwertMitte.series.MarkerStyle = MarkerStyle.Circle;
                messwertFahrzeugTiMittelwertMitte.series.LegendText = columnFahrzeugMittelMitteString;
                messwertFahrzeugTiMittelwertMitte.series.Color = Color.Orange;

                messwertFahrzeugTiMittelwertOben.series.Name = columnFahrzeugMittelObenID;
                messwertFahrzeugTiMittelwertOben.series.ChartType = SeriesChartType.Line;
                messwertFahrzeugTiMittelwertOben.series.MarkerStyle = MarkerStyle.Circle;
                messwertFahrzeugTiMittelwertOben.series.LegendText = columnFahrzeugMittelObenString;
                messwertFahrzeugTiMittelwertOben.series.Color = Color.Gold;


                if (!batch_mode)
                {
                    chartFahrzeugMessung.Series.Add(columnFahrzeugMittelID);
                    chartFahrzeugMessung.Series[columnFahrzeugMittelID] = messwertFahrzeugTiMittelwert.series;

                    chartFahrzeugMessung.Series.Add(columnFahrzeugMittelUntenID);
                    chartFahrzeugMessung.Series[columnFahrzeugMittelUntenID] = messwertFahrzeugTiMittelwertUnten.series;

                    chartFahrzeugMessung.Series.Add(columnFahrzeugMittelMitteID);
                    chartFahrzeugMessung.Series[columnFahrzeugMittelMitteID] = messwertFahrzeugTiMittelwertMitte.series;

                    chartFahrzeugMessung.Series.Add(columnFahrzeugMittelObenID);
                    chartFahrzeugMessung.Series[columnFahrzeugMittelObenID] = messwertFahrzeugTiMittelwertOben.series;
                }

                textBoxCabinTemperaturMeasured.Text = messwertFahrzeugTiMittelwert.FirstY.ToString();

                if (!batch_mode)
                {
                    chartFahrzeugMessung.ChartAreas[0].AxisX.Minimum = Utilities.minValue(messdatenFahrzeugTable, columnZeitInStundenID);
                    chartFahrzeugMessung.ChartAreas[0].AxisX.Maximum = Utilities.maxValue(messdatenFahrzeugTable, columnZeitInStundenID);
                    chartFahrzeugMessung.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
                }

                double timeInterval = Utilities.maxValue(messdatenFahrzeugTable, columnZeitInStundenID) - Utilities.minValue(messdatenFahrzeugTable, columnZeitInStundenID);


                if (!batch_mode)
                {
                    if (timeInterval <= 2)
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 0.125;
                    else if ((timeInterval > 2) && (timeInterval <= 5))
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 0.25;
                    else if ((timeInterval > 5) && (timeInterval <= 12))
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 0.5;
                    else if ((timeInterval > 12) && (timeInterval <= 24))
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 1;
                    else if ((timeInterval > 24) && (timeInterval <= 24 * 20))
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 24;
                    else
                        chartFahrzeugMessung.ChartAreas[0].AxisX.Interval = 24;

                    eventLog.add(new LogEntry(this.ToString(), "Fahrzeugtemperaturen umfassen Zeitraum von " + timeInterval + " h. Intervall der x-Achse auf " + chartFahrzeugMessung.ChartAreas[0].AxisX.Interval + " gesetzt.", Log.LOG_DEBUG_TYPE));

                    chartFahrzeugMessung.ChartAreas[0].AxisY.Title = "Temperatur (°C)";
                    chartFahrzeugMessung.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
                    chartFahrzeugMessung.ChartAreas[0].AxisY.Minimum = Utilities.minValue(messdatenFahrzeugTable, columnFahrzeugMittelUntenID) - 2;
                    chartFahrzeugMessung.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(messdatenFahrzeugTable, columnFahrzeugMittelObenID) + 2;

                    chartFahrzeugMessung.ChartAreas[0].AxisX.Title = columnZeitInStundenString + " (" + columnZeitInStundenUnit + ")";

                    chartFahrzeugMessung.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                    chartFahrzeugMessung.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                    chartFahrzeugMessung.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                    chartFahrzeugMessung.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                }
            }
            else
            {
                tabControl.TabPages.Remove(tabPageMessdatenFahrzeug);
            }


            // ============ EXTRACT PART DEFINITIONS ====================
            bparMaterialA1.SelectedIndex = bparMaterialA1.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilAMaterial, 0));
            bparMaterialA2.SelectedIndex = bparMaterialA2.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilAMaterial, 1));
            bparMaterialA3.SelectedIndex = bparMaterialA3.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilAMaterial, 2));
            bparStaerkeA1.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilAStaerke, 0) * conversionInputBauteilStaerke;
            bparStaerkeA2.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilAStaerke, 1) * conversionInputBauteilStaerke;
            bparStaerkeA3.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilAStaerke, 2) * conversionInputBauteilStaerke;

            bparMaterialB1.SelectedIndex = bparMaterialB1.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilBMaterial, 0));
            bparMaterialB2.SelectedIndex = bparMaterialB2.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilBMaterial, 1));
            bparMaterialB3.SelectedIndex = bparMaterialB3.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilBMaterial, 2));
            bparStaerkeB1.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilBStaerke, 0) * conversionInputBauteilStaerke;
            bparStaerkeB2.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilBStaerke, 1) * conversionInputBauteilStaerke;
            bparStaerkeB3.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilBStaerke, 2) * conversionInputBauteilStaerke;


            bparMaterialC1.SelectedIndex = bparMaterialC1.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilCMaterial, 0));
            bparMaterialC2.SelectedIndex = bparMaterialC2.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilCMaterial, 1));
            bparMaterialC3.SelectedIndex = bparMaterialC3.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilCMaterial, 2));
            bparStaerkeC1.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilCStaerke, 0) * conversionInputBauteilStaerke;
            bparStaerkeC2.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilCStaerke, 1) * conversionInputBauteilStaerke;
            bparStaerkeC3.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilCStaerke, 2) * conversionInputBauteilStaerke;


            bparMaterialD1.SelectedIndex = bparMaterialD1.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilDMaterial, 0));
            bparMaterialD2.SelectedIndex = bparMaterialD2.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilDMaterial, 1));
            bparMaterialD3.SelectedIndex = bparMaterialD3.FindString(Utilities.stringValueAt(parameterBauteildefTable, columnBauteilDMaterial, 2));
            bparStaerkeD1.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilDStaerke, 0) * conversionInputBauteilStaerke;
            bparStaerkeD2.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilDStaerke, 1) * conversionInputBauteilStaerke;
            bparStaerkeD3.Value = (Decimal)Utilities.doubleValueAt(parameterBauteildefTable, columnBauteilDStaerke, 2) * conversionInputBauteilStaerke;


            // ============ EXTRACT MATERIAL CONSTANTS ====================
            materials[0].Cv = Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowCVBlechIntID);
            materials[0].Rho = Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowRhoBlechIntID);
            materials[0].Lambda = Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowLambdaBlechIntID);

            mparAlphaSBlech.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowAlphaSBlechIntID);
            mparTauSBlech.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowTauSBlechIntID);
            mparAlphaLBlech.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnBlechID, rowAlphaLBlechIntID);

            mparCVBlech.Value = (Decimal)materials.Find(m => m.Id == 1).Cv;
            mparLambdaBlech.Value = (Decimal)materials.Find(m => m.Id == 1).Lambda;
            mparRhoBlech.Value = (Decimal)materials.Find(m => m.Id == 1).Rho;


            materials[1].Cv = Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowCVGlasIntID);
            materials[1].Rho = Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowRhoGlasIntID);
            materials[1].Lambda = Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowLambdaGlasIntID);

            mparAlphaSGlas.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowAlphaSGlasIntID);
            mparTauSGlas.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowTauSGlasIntID);
            mparAlphaLGlas.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnGlasID, rowAlphaLGlasIntID);

            mparCVGlas.Value = (Decimal)materials.Find(m => m.Id == 3).Cv;
            mparLambdaGlas.Value = (Decimal)materials.Find(m => m.Id == 3).Lambda;
            mparRhoGlas.Value = (Decimal)materials.Find(m => m.Id == 3).Rho;


            mparAlphaSLuft.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnLuftID, rowAlphaSLuftIntID);
            mparTauSLuft.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnLuftID, rowTauSLuftIntID);
            mparAlphaLLuft.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnLuftID, rowAlphaLLuftIntID);

            mparCVLuft.Value = (Decimal)materials.Find(m => m.Id == 4).Cv;
            mparLambdaLuft.Value = (Decimal)materials.Find(m => m.Id == 4).Lambda;
            mparRhoLuft.Value = (Decimal)materials.Find(m => m.Id == 4).Rho;


            materials[3].Cv = Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowCVIsolatorIntID);
            materials[3].Rho = Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowRhoIsolatorIntID);
            materials[3].Lambda = Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowLambdaIsolatorIntID);

            mparAlphaSIsolator.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowAlphaSIsolatorIntID);
            mparTauSIsolator.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowTauSIsolatorIntID);
            mparAlphaLIsolator.Value = (Decimal)Utilities.doubleValueAt(parameterMaterialdatenTable, columnIsolatorID, rowAlphaLIsolatorIntID);

            mparCVIsolator.Value = (Decimal)materials.Find(m => m.Id == 2).Cv;
            mparLambdaIsolator.Value = (Decimal)materials.Find(m => m.Id == 2).Lambda;
            mparRhoIsolator.Value = (Decimal)materials.Find(m => m.Id == 2).Rho;

            // ============ EXTRACT VEHICLE PARAMETERS ====================
            fparLaenge.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnLaengeID, rowLaengeIntID);
            fparBreite.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnBreiteID, rowBreiteIntID);
            fparHoehe.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnHoeheID, rowHoeheIntID);

            fparAlpha.Value = 1;
            fparBeta.Value = 1;

            fparAlpha.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAlphaID, rowAlphaIntID);
            fparBeta.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnBetaID, rowBetaIntID);
  
            fparAnteil1A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset);
            fparAnteil1B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset);
            fparAnteil1C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset);
            fparAnteil1D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset);

            fparAnteil2A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset + 1);
            fparAnteil2B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset + 1);
            fparAnteil2C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset + 1);
            fparAnteil2D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset + 1);

            fparAnteil3A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset + 2);
            fparAnteil3B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset + 2);
            fparAnteil3C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset + 2);
            fparAnteil3D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset + 2);

            fparAnteil4A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset + 3);
            fparAnteil4B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset + 3);
            fparAnteil4C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset + 3);
            fparAnteil4D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset + 3);

            fparAnteil5A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset + 4);
            fparAnteil5B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset + 4);
            fparAnteil5C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset + 4);
            fparAnteil5D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset + 4);

            fparAnteil6A.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypA, rowAnteilTypOffset + 5);
            fparAnteil6B.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypB, rowAnteilTypOffset + 5);
            fparAnteil6C.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypC, rowAnteilTypOffset + 5);
            fparAnteil6D.Value = (Decimal)Utilities.doubleValueAt(parameterFlaechendefTable, columnAnteilTypD, rowAnteilTypOffset + 5);

            // ============ AMBIENT TEMPERATURE CHART ========================
            physics.MeteorologyAmbientTemperature.populateWithDataTable(meteorologyTable, columnZeitInStundenID, columnAmbientTemperatureID);
            physics.MeteorologyAmbientTemperature.xName = Utilities.columnHeader(shortNameZeitInStunden, columnZeitInStundenUnit);
            physics.MeteorologyAmbientTemperature.yName = Utilities.columnHeader(shortNameAmbientTemperature, columnAmbientTemperatureUnit);

            physics.MeteorologyAmbientTemperature.series.Name = columnAmbientTemperatureID;
            physics.MeteorologyAmbientTemperature.series.ChartType = SeriesChartType.Line;
            physics.MeteorologyAmbientTemperature.series.MarkerStyle = MarkerStyle.Circle;
            physics.MeteorologyAmbientTemperature.series.LegendText = columnAmbientTemperatureString;

            if (!batch_mode)
            {
                chartAmbientTemperature.Series.Add(columnAmbientTemperatureID);
                chartAmbientTemperature.Series[columnAmbientTemperatureID] = physics.MeteorologyAmbientTemperature.series;
                chartAmbientTemperature.Series[columnAmbientTemperatureID].XValueMember = columnZeitInStundenID;
                chartAmbientTemperature.Series[columnAmbientTemperatureID].YValueMembers = columnAmbientTemperatureID;
                chartAmbientTemperature.Series[columnAmbientTemperatureID].LegendText = columnAmbientTemperatureString;

                chartAmbientTemperature.ChartAreas[0].AxisY.Minimum = Utilities.minValue(physics.MeteorologyAmbientTemperature, 1) - 2;
                chartAmbientTemperature.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(physics.MeteorologyAmbientTemperature, 1) + 2;
                chartAmbientTemperature.ChartAreas[0].AxisX.Title = columnZeitInStundenString + " (" + columnZeitInStundenString + ")";
                chartAmbientTemperature.ChartAreas[0].AxisY.Title = "Temperatur (°C)";
                chartAmbientTemperature.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";

                chartAmbientTemperature.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chartAmbientTemperature.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartAmbientTemperature.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chartAmbientTemperature.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartAmbientTemperature.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            }
            // ============ WINDSPEED IN 10m HIGHT CHART ========================
            physics.MeteorologyWindSpeedv10m.populateWithDataTable(meteorologyTable, columnZeitInStundenID, columnWindSpeed10mID);
            physics.MeteorologyWindSpeedv1m.populateWithDataTable(meteorologyTable, columnZeitInStundenID, columnWindSpeed10mID);

            physics.MeteorologyWindSpeedv1m.series.Name = columnWindSpeed1mID;
            physics.MeteorologyWindSpeedv1m.series.ChartType = SeriesChartType.Line;
            physics.MeteorologyWindSpeedv1m.series.MarkerStyle = MarkerStyle.Circle;
            physics.MeteorologyWindSpeedv1m.series.LegendText = columnWindSpeed1mString;

            physics.MeteorologyWindSpeedv10m.series.Name = columnWindSpeed10mID;
            physics.MeteorologyWindSpeedv10m.series.ChartType = SeriesChartType.Line;
            physics.MeteorologyWindSpeedv10m.series.MarkerStyle = MarkerStyle.Circle;
            physics.MeteorologyWindSpeedv10m.series.LegendText = columnWindSpeed10mString;


            if (!batch_mode)
            {
                chartWindSpeed.Series.Add(columnWindSpeed1mID);
                chartWindSpeed.Series[columnWindSpeed1mID] = physics.MeteorologyWindSpeedv1m.series;
                chartWindSpeed.Series.Add(columnWindSpeed10mID);
                chartWindSpeed.Series[columnWindSpeed10mID] = physics.MeteorologyWindSpeedv10m.series;

                chartWindSpeed.ChartAreas[0].AxisY.Minimum = 0;
                chartWindSpeed.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(meteorologyTable, columnWindSpeed10mID) + 2;
                chartWindSpeed.ChartAreas[0].AxisX.Title = columnZeitInStundenString + " (" + columnZeitInStundenString + ")";
                chartWindSpeed.ChartAreas[0].AxisY.Title = "Geschwindigkeit (m/s)";
                chartWindSpeed.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

                chartWindSpeed.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chartWindSpeed.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartWindSpeed.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chartWindSpeed.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartWindSpeed.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
            }
            // ============ SHORT WAVE RADIATION CHART ========================
            physics.MeteorologyGlobalRadiation.populateWithDataTable(meteorologyTable, columnZeitInStundenID, columnGlobalRadiationID);
            physics.MeteorologyDiffuseRadiation.populateWithDataTable(meteorologyTable, columnZeitInStundenID, columnDiffuseRadiationID);

            physics.MeteorologyGlobalRadiation.series.Name = columnGlobalRadiationID;
            physics.MeteorologyDiffuseRadiation.series.Name = columnDiffuseRadiationID;

            physics.MeteorologyGlobalRadiation.series.ChartType = SeriesChartType.Line;
            physics.MeteorologyGlobalRadiation.series.MarkerStyle = MarkerStyle.Circle;
            physics.MeteorologyGlobalRadiation.series.LegendText = columnGlobalRadiationString;

            physics.MeteorologyDiffuseRadiation.series.ChartType = SeriesChartType.Line;
            physics.MeteorologyDiffuseRadiation.series.MarkerStyle = MarkerStyle.Circle;
            physics.MeteorologyDiffuseRadiation.series.LegendText = columnDiffuseRadiationString;

            if (!batch_mode)
            {
                chartkwStrahlung.Series.Add(columnGlobalRadiationID);
                chartkwStrahlung.Series.Add(columnDiffuseRadiationID);
                chartkwStrahlung.Series[columnGlobalRadiationID] = physics.MeteorologyGlobalRadiation.series;
                chartkwStrahlung.Series[columnDiffuseRadiationID] = physics.MeteorologyDiffuseRadiation.series;

                chartkwStrahlung.ChartAreas[0].AxisY.Minimum = Math.Min(Utilities.minValue(physics.MeteorologyGlobalRadiation, 1), Utilities.minValue(physics.MeteorologyDiffuseRadiation, 1));
                chartkwStrahlung.ChartAreas[0].AxisY.Maximum = Math.Max(Utilities.maxValue(physics.MeteorologyGlobalRadiation, 1), Utilities.maxValue(physics.MeteorologyDiffuseRadiation, 1));
                chartkwStrahlung.ChartAreas[0].AxisX.Title = columnZeitInStundenString + " (" + columnZeitInStundenString + ")";
                chartkwStrahlung.ChartAreas[0].AxisY.Title = "Intensität (W/m²)";
                chartkwStrahlung.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

                chartkwStrahlung.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chartkwStrahlung.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartkwStrahlung.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chartkwStrahlung.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartkwStrahlung.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";
            }

            // ============ HORIZON CHART ========================
            physics.EnvironmentHorizon.populateWithDataTable(horizonTable, columnHorizonAlphaID, columnHorizonGammaID);
            physics.EnvironmentHorizon.series.Name = columnHorizonGammaID;

            physics.EnvironmentHorizon.series.ChartType = SeriesChartType.Column;
            physics.EnvironmentHorizon.series["PointWidth"] = "1";
            physics.EnvironmentHorizon.series.LegendText = "Horizonteinschränkung";


            if (!batch_mode)
            {
                chartHorizont.Series.Add(columnHorizonGammaID);
                chartHorizont.Series[columnHorizonGammaID] = physics.EnvironmentHorizon.series;

                chartHorizont.ChartAreas[0].AxisX.Minimum = 0;
                chartHorizont.ChartAreas[0].AxisX.Maximum = 360;
                chartHorizont.ChartAreas[0].AxisY.Minimum = Utilities.minValue(horizonTable, columnHorizonGammaID);
                chartHorizont.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(horizonTable, columnHorizonGammaID);
                chartHorizont.ChartAreas[0].AxisX.Title = columnHorizonAlphaString + " (" + columnHorizonAlphaUnit + ")";
                chartHorizont.ChartAreas[0].AxisY.Title = "Höhenwinkel (°)";

                chartHorizont.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chartHorizont.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
                chartHorizont.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chartHorizont.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.DashDotDot;
            }
            // ============ CALCULATE SOME QUANTITIES ========================
            physics.EnvironmentRSEnv = (double)numericUpDownR_SEnv.Value;
            physics.EnvironmentELBld = (double)numericUpDownE_LEnv.Value;
            physics.EnvironmentELGnd = (double)numericUpDownE_LGnd.Value;
            

            physics.TimeStepsMeteorology = ((new DataPoints(eventLog)).populateXValuesFromDataTable(meteorologyTable, columnZeitInStundenID));

            this.simulationTimeStepSizeChanged();

            physics.TDuration = chartFahrzeugMessung.Series[columnFahrzeugMittelID].Points[chartFahrzeugMessung.Series[columnFahrzeugMittelID].Points.Count-1].XValue;//Utilities.maxValue(physics.TimeStepsMeteorology, 0);
            physics.TDuration -= tOffset;
            //physics.TDuration =0.1;
            numericUpDownSimulationTime.Value = (decimal)physics.TDuration;

            if(checkBoxT0VehicleAsT0Sim.Checked)
            {
                physics.InitialT = messwertFahrzeugTiMittelwert.FirstY;
            }
            else
            {
                physics.InitialT = physics.TA(0);
            }
            
            this.numericUpDownT0.Value = (decimal)physics.InitialT;

            physicsParametersChanged();
            textBoxtOffsetInStunden.Text = physics.tOffsetToMeteorology.ToString();

            initializeWalls();
            thermalInitialization();

            populateTree();

            initialization = false;



            eventLog.add(new LogEntry("main()", "===================================================================================", Log.LOG_DEBUG_TYPE));
            eventLog.add(new LogEntry("main()", "Wände initialisiert...", Log.LOG_DEBUG_TYPE));
            for (int j = 0; j < 6; j++)
            {
                eventLog.add(new LogEntry("main()", physics.Walls[j].ToString(), Log.LOG_DEBUG_TYPE));
            }
            eventLog.add(new LogEntry("main()", "===================================================================================", Log.LOG_DEBUG_TYPE));
        }

        private void initializeWalls()
        {
            physics.Walls.Clear();
            // ============= FILL Wall Classes ===============================
            // Alle sechs Wände anlegen
            for (int j = 0; j < 6; j++)
            {
                physics.Walls.Add(new Wall(eventLog));
                physics.Walls[j].Name = "S" + (j + 1);
                physics.Walls[j].ID = j;
            }

            Material m1A = new Material();
            Material m2A = new Material();
            Material m3A = new Material();

            Material m1B = new Material();
            Material m2B = new Material();
            Material m3B = new Material();

            Material m1C = new Material();
            Material m2C = new Material();
            Material m3C = new Material();

            Material m1D = new Material();
            Material m2D = new Material();
            Material m3D = new Material();



            //Die Oberflächenparameter auslesen und den entsprechenden Materialoberflächen zuordnen.
            materials[0].Surface.AlphaL = (double)mparAlphaLBlech.Value;
            materials[0].Surface.TauS = (double)mparTauSBlech.Value;
            materials[0].Surface.AlphaS = (double)mparAlphaSBlech.Value;
            materials[0].Surface.EpsilonL = (double)mparAlphaLBlech.Value;

            materials[1].Surface.AlphaL = (double)mparAlphaLGlas.Value;
            materials[1].Surface.TauS = (double)mparTauSGlas.Value;
            materials[1].Surface.AlphaS = (double)mparAlphaSGlas.Value;
            materials[1].Surface.EpsilonL = (double)mparAlphaLGlas.Value;

            materials[2].Surface.AlphaL = (double)mparAlphaLLuft.Value;
            materials[2].Surface.TauS = (double)mparTauSLuft.Value;
            materials[2].Surface.AlphaS = (double)mparAlphaSLuft.Value;
            materials[2].Surface.EpsilonL = (double)mparAlphaLLuft.Value;

            materials[3].Surface.AlphaL = (double)mparAlphaLIsolator.Value;
            materials[3].Surface.TauS = (double)mparTauSIsolator.Value;
            materials[3].Surface.AlphaS = (double)mparAlphaSIsolator.Value;
            materials[3].Surface.EpsilonL = (double)mparAlphaLIsolator.Value;


            if (bparMaterialA1.SelectedItem != null) m1A = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialA1.SelectedItem).Key));
            if (bparMaterialA2.SelectedItem != null) m2A = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialA2.SelectedItem).Key));
            if (bparMaterialA3.SelectedItem != null) m3A = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialA3.SelectedItem).Key));

            if (bparMaterialB1.SelectedItem != null) m1B = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialB1.SelectedItem).Key));
            if (bparMaterialB2.SelectedItem != null) m2B = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialB2.SelectedItem).Key));
            if (bparMaterialB3.SelectedItem != null) m3B = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialB3.SelectedItem).Key));

            if (bparMaterialC1.SelectedItem != null) m1C = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialC1.SelectedItem).Key));
            if (bparMaterialC2.SelectedItem != null) m2C = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialC2.SelectedItem).Key));
            if (bparMaterialC3.SelectedItem != null) m3C = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialC3.SelectedItem).Key));

            if (bparMaterialD1.SelectedItem != null) m1D = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialD1.SelectedItem).Key));
            if (bparMaterialD2.SelectedItem != null) m2D = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialD2.SelectedItem).Key));
            if (bparMaterialD3.SelectedItem != null) m3D = materials.Find(m => m.Id == (((KeyValuePair<int, String>)bparMaterialD3.SelectedItem).Key));

            for (int k = 0; k < 6; k++)
            {
                physics.Walls[k].Deltax = physics.Deltax;

                if (bparMaterialA1.SelectedItem != null) physics.Walls[k].addLayer(0, new Layer(m1A, (double)bparStaerkeA1.Value, eventLog));
                if (bparMaterialA2.SelectedItem != null) physics.Walls[k].addLayer(0, new Layer(m2A, (double)bparStaerkeA2.Value, eventLog));
                if (bparMaterialA3.SelectedItem != null) physics.Walls[k].addLayer(0, new Layer(m3A, (double)bparStaerkeA3.Value, eventLog));

                if (bparMaterialB1.SelectedItem != null) physics.Walls[k].addLayer(1, new Layer(m1B, (double)bparStaerkeB1.Value, eventLog));
                if (bparMaterialB2.SelectedItem != null) physics.Walls[k].addLayer(1, new Layer(m2B, (double)bparStaerkeB2.Value, eventLog));
                if (bparMaterialB3.SelectedItem != null) physics.Walls[k].addLayer(1, new Layer(m3B, (double)bparStaerkeB3.Value, eventLog));

                if (bparMaterialC1.SelectedItem != null) physics.Walls[k].addLayer(2, new Layer(m1C, (double)bparStaerkeC1.Value, eventLog));
                if (bparMaterialC2.SelectedItem != null) physics.Walls[k].addLayer(2, new Layer(m2C, (double)bparStaerkeC2.Value, eventLog));
                if (bparMaterialC3.SelectedItem != null) physics.Walls[k].addLayer(2, new Layer(m3C, (double)bparStaerkeC3.Value, eventLog));

                if (bparMaterialD1.SelectedItem != null) physics.Walls[k].addLayer(3, new Layer(m1D, (double)bparStaerkeD1.Value, eventLog));
                if (bparMaterialD2.SelectedItem != null) physics.Walls[k].addLayer(3, new Layer(m2D, (double)bparStaerkeD2.Value, eventLog));
                if (bparMaterialD3.SelectedItem != null) physics.Walls[k].addLayer(3, new Layer(m3D, (double)bparStaerkeD3.Value, eventLog));
            }

            physics.Walls[0].Area = (double)fparFlaeche1.Value;
            physics.Walls[0].setArea(0, (double)(fparFlaeche1.Value * fparAnteil1A.Value));
            physics.Walls[0].setArea(1, (double)(fparFlaeche1.Value * fparAnteil1B.Value));
            physics.Walls[0].setArea(2, (double)(fparFlaeche1.Value * fparAnteil1C.Value));
            physics.Walls[0].setArea(3, (double)(fparFlaeche1.Value * fparAnteil1D.Value));
            physics.Walls[0].Alpha = (double)fparAlpha1.Value;
            physics.Walls[0].Beta = (double)fparBeta1.Value;
            //physics.Walls[0].Opposing = physics.Walls[5];

            physics.Walls[1].Area = (double)fparFlaeche2.Value;
            physics.Walls[1].setArea(0, (double)(fparFlaeche2.Value * fparAnteil2A.Value));
            physics.Walls[1].setArea(1, (double)(fparFlaeche2.Value * fparAnteil2B.Value));
            physics.Walls[1].setArea(2, (double)(fparFlaeche2.Value * fparAnteil2C.Value));
            physics.Walls[1].setArea(3, (double)(fparFlaeche2.Value * fparAnteil2D.Value));
            physics.Walls[1].Alpha = (double)fparAlpha2.Value;
            physics.Walls[1].Beta = (double)fparBeta2.Value;
            //physics.Walls[1].Opposing = physics.Walls[4];

            physics.Walls[2].Area = (double)fparFlaeche3.Value;
            physics.Walls[2].setArea(0, (double)(fparFlaeche3.Value * fparAnteil3A.Value));
            physics.Walls[2].setArea(1, (double)(fparFlaeche3.Value * fparAnteil3B.Value));
            physics.Walls[2].setArea(2, (double)(fparFlaeche3.Value * fparAnteil3C.Value));
            physics.Walls[2].setArea(3, (double)(fparFlaeche3.Value * fparAnteil3D.Value));
            physics.Walls[2].Alpha = (double)fparAlpha3.Value;
            physics.Walls[2].Beta = (double)fparBeta3.Value;
            //physics.Walls[2].Opposing = physics.Walls[3];

            physics.Walls[3].Area = (double)fparFlaeche4.Value;
            physics.Walls[3].setArea(0, (double)(fparFlaeche4.Value * fparAnteil4A.Value));
            physics.Walls[3].setArea(1, (double)(fparFlaeche4.Value * fparAnteil4B.Value));
            physics.Walls[3].setArea(2, (double)(fparFlaeche4.Value * fparAnteil4C.Value));
            physics.Walls[3].setArea(3, (double)(fparFlaeche4.Value * fparAnteil4D.Value));
            physics.Walls[3].Alpha = (double)fparAlpha4.Value;
            physics.Walls[3].Beta = (double)fparBeta4.Value;
            //physics.Walls[3].Opposing = physics.Walls[2];

            physics.Walls[4].Area = (double)fparFlaeche5.Value;
            physics.Walls[4].setArea(0, (double)(fparFlaeche5.Value * fparAnteil5A.Value));
            physics.Walls[4].setArea(1, (double)(fparFlaeche5.Value * fparAnteil5B.Value));
            physics.Walls[4].setArea(2, (double)(fparFlaeche5.Value * fparAnteil5C.Value));
            physics.Walls[4].setArea(3, (double)(fparFlaeche5.Value * fparAnteil5D.Value));
            physics.Walls[4].Alpha = (double)fparAlpha5.Value;
            physics.Walls[4].Beta = (double)fparBeta5.Value;
            //physics.Walls[4].Opposing = physics.Walls[1];

            physics.Walls[5].Area = (double)fparFlaeche6.Value;
            physics.Walls[5].setArea(0, (double)(fparFlaeche6.Value * fparAnteil6A.Value));
            physics.Walls[5].setArea(1, (double)(fparFlaeche6.Value * fparAnteil6B.Value));
            physics.Walls[5].setArea(2, (double)(fparFlaeche6.Value * fparAnteil6C.Value));
            physics.Walls[5].setArea(3, (double)(fparFlaeche6.Value * fparAnteil6D.Value));
            physics.Walls[5].Alpha = (double)fparAlpha6.Value;
            physics.Walls[5].Beta = (double)fparBeta6.Value;
            //physics.Walls[5].Opposing = physics.Walls[0];
        }

        private void basicInit()
        {
            eventLog = new Log(textBoxLog);
            physics = new Physics(eventLog);

            //checkBoxFixSun.Checked = true;

            //physics.fixSun(65.2, 180);

            if(physics.SunFixed)
            {
                labelSonnenpositionFixiert.Text = "Achtung! Sonnenposition fixiert in Icov.basicInit() auf "+physics.gamma(2000)+"° und "+physics.psi(2000)+"° !";
            }

            materials.Add(new Material(1));
            materials[0].Name = "Blech";
            materials[0].Cv = 500;// 924;
            materials[0].Lambda = 80;// 220;
            materials[0].Rho = 7800;// 2700;

            materials.Add(new Material(3));
            materials[1].Name = "Glas";
            materials[1].Cv = 750;
            materials[1].Lambda = 0.95;
            materials[1].Rho = 2500;

            materials.Add(new Material(4));
            materials[2].Name = "Luft";
            materials[2].Cv = Physics.C_CP_AIR;
            materials[2].Lambda = Physics.C_LAMBDA_AIR;
            materials[2].Rho = Physics.C_RHO_AIR;

            materials.Add(new Material(2));
            materials[3].Name = "Isolator";
            materials[3].Cv = 1300;
            materials[3].Lambda = 0.055;
            materials[3].Rho = 270;

            var comboRangeMaterialsA = new BindingList<KeyValuePair<int, string>>();
            KeyValuePair<int, String> emptyKVP = new KeyValuePair<int, String>(0, "--leer--");

            comboRangeMaterialsA.Add(emptyKVP);
            foreach (Material m in materials)
            {
                comboRangeMaterialsA.Add(new KeyValuePair<int, string>(m.Id, m.Name));
            }

            var comboRangeMaterialsB = new BindingList<KeyValuePair<int, string>>();
            comboRangeMaterialsB.Add(emptyKVP);
            foreach (Material m in materials)
            {
                comboRangeMaterialsB.Add(new KeyValuePair<int, string>(m.Id, m.Name));
            }


            bparMaterialA1.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsA);
            bparMaterialA2.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);
            bparMaterialA3.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);


            bparMaterialB1.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsA);
            bparMaterialB2.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);
            bparMaterialB3.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);


            bparMaterialC1.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsA);
            bparMaterialC2.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);
            bparMaterialC3.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);

            bparMaterialD1.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsA);
            bparMaterialD2.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);
            bparMaterialD3.DataSource = new BindingList<KeyValuePair<int, string>>(comboRangeMaterialsB);


            progressBarText.BackColor = Color.Transparent;
        }

        public Icov()
        {
            string sCurrentCulture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
            CultureInfo ci = new CultureInfo(sCurrentCulture);
            ci.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;

            InitializeComponent();
            basicInit();
 


        }
        

        private void thermalInitialization()
        {
            // Setze alle Temperaturen auf Ausgangstemperatur zurück. Neu setzen der Wärmemenge erfolgt automatisch.
            for (int k = 0; k < physics.Walls.Count; k++)
            {
                physics.Walls[k].initialize(physics.Deltax, physics.InitialT);
            }

            physics.Cabin = new Compartment((double)fparLaenge.Value, (double)fparBreite.Value, (double)fparHoehe.Value, physics.InitialT, eventLog);
        }

        private void populateTree()
        {
            
            treeView1.Nodes.Clear();
            rootNode.Nodes.Clear();
            treeView1.Nodes.Add(rootNode);
            // Setze alle Temperaturen auf Ausgangstemperatur zurück. Neu setzen der Wärmemenge erfolgt automatisch.
            for (int k = 0; k < physics.Walls.Count; k++)
            {
                //Befülle den Übersichtsbaum
                physics.Walls[k].fillVehicleTree(treeView1, rootNode);
            }
            Utilities.ExpandToLevel(treeView1.Nodes, 3);
            
        }


        //Werden die Fahrzeugparameter geändert werden hier alle notwendigen Parameter neu erstellt
        private void vehicleParametersChanged()
        {
            if (initialization == false)
            {
                initializeWalls();
                populateTree();
            }
        }

        // Wenn sich bestimmte Parameter ändern muss die Physics Klasse informiert werden UND die entsprechenden Felder in der Oberfläche aktualisiert werden.
        public void physicsParametersChanged()
        {

            Debug.Write("regenerating meteorological tables...\n");

            Debug.Write("  [windspeed at 1m]   \n");
            surfaceRoughnessChanged();

            Debug.Write("  [sunparameters and tables]   \n");
            sonnenParameterValuesChanged();

            Debug.Write("  [sky's emissivity]   \n");
            physics.calculateEpsilonSkyTable();

            Debug.Write("  [meteorology debug table]   \n");
            physics.fillMeteorologyDebugTables();

            Debug.Write("  [populating gridviews]   \n");
            populateGridViewsDebugInfo1();

            Debug.Write("....[Done]\n");
        }


        private void fparBreite6_Validated(object sender, EventArgs e)
        {
            if (((NumericUpDown)sender == fparLaenge1) || ((NumericUpDown)sender == fparBreite1))
                fparFlaeche1.Value = fparLaenge1.Value * fparBreite1.Value;
            else if (((NumericUpDown)sender == fparLaenge2) || ((NumericUpDown)sender == fparBreite2))
                fparFlaeche2.Value = fparLaenge2.Value * fparBreite2.Value;
            else if (((NumericUpDown)sender == fparLaenge3) || ((NumericUpDown)sender == fparBreite3))
                fparFlaeche3.Value = fparLaenge3.Value * fparBreite3.Value;
            else if (((NumericUpDown)sender == fparLaenge4) || ((NumericUpDown)sender == fparBreite4))
                fparFlaeche4.Value = fparLaenge4.Value * fparBreite4.Value;
            else if (((NumericUpDown)sender == fparLaenge5) || ((NumericUpDown)sender == fparBreite5))
                fparFlaeche5.Value = fparLaenge5.Value * fparBreite5.Value;
            else if (((NumericUpDown)sender == fparLaenge6) || ((NumericUpDown)sender == fparBreite6))
                fparFlaeche6.Value = fparLaenge6.Value * fparBreite6.Value;
        }

        private void fparAlpha_ValueChanged(object sender, EventArgs e)
        {
            fparAlpha1.Value = fparAlpha.Value;
            fparAlpha2.Value = fparAlpha.Value;
            fparAlpha3.Value = Math.Abs(Utilities.mod(fparAlpha.Value - 90, 360));
            fparAlpha4.Value = Math.Abs(Utilities.mod(fparAlpha.Value + 90, 360));
            fparAlpha5.Value = fparAlpha.Value;
            fparAlpha6.Value = Math.Abs(Utilities.mod(fparAlpha.Value + 180, 360));

            vehicleParametersChanged();
        }

        private void fparBeta_ValueChanged(object sender, EventArgs e)
        {
            fparBeta1.Value = fparBeta.Value;
            if (fparBeta.Value + 90 < 0) fparBeta2.Value = (fparBeta.Value+90)*(-1);
            else if (fparBeta.Value + 90 > 180) fparBeta2.Value = 180 - (fparBeta.Value - 90);
            else fparBeta2.Value = fparBeta.Value+90;
            //fparBeta2.Value = Math.Abs(Utilities.mod(fparBeta.Value + 90, 180));
            fparBeta3.Value = fparBeta.Value;
            fparBeta4.Value = fparBeta.Value;
            if (fparBeta.Value - 90 < 0) fparBeta5.Value = (fparBeta.Value-90)*(-1);
            else if (fparBeta.Value - 90 > 180) fparBeta5.Value = 180 - (fparBeta.Value - 90 - 180);
            else fparBeta5.Value = fparBeta.Value-90;
            //fparBeta5.Value = Math.Abs(Utilities.mod(fparBeta.Value + 90, 180));
            fparBeta6.Value = fparBeta.Value;

            vehicleParametersChanged();
        }

        private void fparValuesChanges(object sender, EventArgs e)
        {
            vehicleParametersChanged();
        }

        private void fparLaenge_ValueChanged(object sender, EventArgs e)
        {
            fparBreite2.Value = fparLaenge.Value;
            fparBreite3.Value = fparLaenge.Value;
            fparBreite4.Value = fparLaenge.Value;
            fparBreite5.Value = fparLaenge.Value;

            vehicleParametersChanged();

        }

        private void fparBreite_ValueChanged(object sender, EventArgs e)
        {
            fparBreite1.Value = fparBreite.Value;
            fparLaenge2.Value = fparBreite.Value;
            fparLaenge5.Value = fparBreite.Value;
            fparBreite6.Value = fparBreite.Value;

            vehicleParametersChanged();
        }

        private void fparHoehe_ValueChanged(object sender, EventArgs e)
        {
            fparLaenge1.Value = fparHoehe.Value;
            fparLaenge3.Value = fparHoehe.Value;
            fparLaenge4.Value = fparHoehe.Value;
            fparLaenge6.Value = fparHoehe.Value;

            vehicleParametersChanged();
        }

        private void fParAmpelValidation(decimal sum, TextBox ampel)
        {
            if (sum < 0) { ampel.BackColor = Color.OrangeRed; ampel.Text = "<0"; }
            else if (sum < 1) { ampel.BackColor = Color.Orange; ampel.Text = "<1"; }
            else if (sum > 1) { ampel.BackColor = Color.OrangeRed; ampel.Text = ">1"; }
            else if (sum == 1) { ampel.BackColor = Color.YellowGreen; ampel.Text = "=1"; }
        }

        private void fparAnteil1A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil1A.Value + fparAnteil1B.Value + fparAnteil1C.Value + fparAnteil1D.Value;
            fParAmpelValidation(sum, tbAmpel1);


            vehicleParametersChanged();
        }

        private void fparAnteil2A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil2A.Value + fparAnteil2B.Value + fparAnteil2C.Value + fparAnteil2D.Value;
            fParAmpelValidation(sum, tbAmpel2);

            vehicleParametersChanged();
        }

        private void fparAnteil3A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil3A.Value + fparAnteil3B.Value + fparAnteil3C.Value + fparAnteil3D.Value;
            fParAmpelValidation(sum, tbAmpel3);

            vehicleParametersChanged();
        }

        private void fparAnteil4A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil4A.Value + fparAnteil4B.Value + fparAnteil4C.Value + fparAnteil4D.Value;
            fParAmpelValidation(sum, tbAmpel4);

            vehicleParametersChanged();
        }

        private void fparAnteil5A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil5A.Value + fparAnteil5B.Value + fparAnteil5C.Value + fparAnteil5D.Value;
            fParAmpelValidation(sum, tbAmpel5);

            vehicleParametersChanged();
        }

        private void fparAnteil6A_ValueChanged(object sender, EventArgs e)
        {
            decimal sum = 0;

            sum = fparAnteil6A.Value + fparAnteil6B.Value + fparAnteil6C.Value + fparAnteil6D.Value;
            fParAmpelValidation(sum, tbAmpel6);

            vehicleParametersChanged();
        }


        private void numericUpDownDT_TimeStepSizeChanged(object sender, EventArgs e)
        {
            simulationTimeStepSizeChanged();
        }

        private void numericUpDownSurfaceRoughness_ValueChanged(object sender, EventArgs e)
        {
            surfaceRoughnessChanged();
        }

        private void datePicker_ValueChanged(object sender, EventArgs e)
        {
            physics.StartDatum = datePicker.Value;
            physicsParametersChanged();
        }


        private void timePicker_ValueChanged(object sender, EventArgs e)
        {
            physics.StartZeit = timePicker.Value;
            textBoxtOffsetInStunden.Text = physics.tOffsetToMeteorology.ToString();
            if (physics.MeteorologyAmbientTemperature.series.Points.Count > 0)
            {
                numericUpDownT0.Value = (decimal)physics.TA(0);
            }
            physicsParametersChanged();
        }

        private void numericUpDownLongitude_ValueChanged(object sender, EventArgs e)
        {
            latitudelongitudeValuesChange();
        }


        public void populateGridViewsDebugInfo1()
        {
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
            dataGridView5.Rows.Clear();
            dataGridView6.Rows.Clear();
            dataGridView7.Rows.Clear();
            dataGridView8.Rows.Clear();
            dataGridView9.Rows.Clear();

            Utilities.populateGridViewWithDataPoints(ref dataGridView1, physics.SonneWOZTable);
            Utilities.populateGridViewWithDataPoints(ref dataGridView2, physics.SonneGammaTable);
            Utilities.populateGridViewWithDataPoints(ref dataGridView3, physics.SonnePsiTable);
            Utilities.populateGridViewWithDataPoints(ref dataGridView4, physics.MeteorologyAmbientTemperature);
            Utilities.populateGridViewWithDataPoints(ref dataGridView5, physics.MeteorologyEpsilonSkyTable);
            Utilities.populateGridViewWithDataPoints(ref dataGridView6, physics.MeteorologyGlobalRadiation);
            Utilities.populateGridViewWithDataPoints(ref dataGridView7, physics.MeteorologyDiffuseRadiation);
            Utilities.populateGridViewWithDataPoints(ref dataGridView8, physics.EnvironmentHorizon);
            Utilities.populateMeteorologicDebugTable(ref dataGridView9, physics.MeteorologyDebugTable);

            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].Width = 50;

            dataGridView2.Columns[0].Width = 50;
            dataGridView2.Columns[1].Width = 50;

            dataGridView3.Columns[0].Width = 50;
            dataGridView3.Columns[1].Width = 50;

            dataGridView4.Columns[0].Width = 50;
            dataGridView4.Columns[1].Width = 50;

            dataGridView5.Columns[0].Width = 50;
            dataGridView5.Columns[1].Width = 50;

            dataGridView6.Columns[0].Width = 50;
            dataGridView6.Columns[1].Width = 50;

            dataGridView7.Columns[0].Width = 50;
            dataGridView7.Columns[1].Width = 50;

            // dataGridView8.Columns[0].Width = 100;
            //dataGridView8.Columns[1].Width = 0;
        }


        public void populateGridViewsDebugInfo2()
        {
            dataGridView9.Rows.Clear();

            Utilities.populateMeteorologicDebugTable(ref dataGridView9, physics.MeteorologyDebugTable);
            dataGridView1.Columns[0].Width = 100;
            dataGridView1.Columns[1].Width = 50;


        }

        private void surfaceRoughnessChanged()
        {
            physics.calculateWindSpeedForSurfaceRoughness((double)parSurfaceRoughness.Value);
            chartWindSpeed.Invalidate();

            eventLog.add(new LogEntry("main:parRecalculateWindSpeed1m()", "Neuberechnung der Windgeschwindigkeit auf 1m Höhe mit z0=" + (double)parSurfaceRoughness.Value + " m", Log.LOG_DEBUG_TYPE));
        }

        private void sonnenParameterValuesChanged()
        {
            physics.calcPSonnenParameter();

            textBoxSonneX.Text = physics.parSonneX.ToString();
            textBoxSonneZperMin.Text = physics.parSonneZperMin.ToString();
            textBoxSonneDeklination.Text = physics.parSonneDeklination.ToString();
        }

        private void latitudelongitudeValuesChange()
        {
            physics.parVehicleLatitude = (double)numericUpDownLatitude.Value;
            physics.parVehicleLongitude = (double)numericUpDownLongitude.Value;

            physicsParametersChanged();
        }

        private void simulationTimeStepSizeChanged()
        {
            physics.Deltat = (double)numericUpDownDT.Value / 3600;
            physics.TimeStepsSimulation.populateXValuesWithEquidistantValues(0, 3600.0 * Utilities.maxValue(physics.TimeStepsMeteorology, 0), (double)numericUpDownDT.Value);
            populateGridViewsDebugInfo1();
        }

        private void numericUpDownR_SEnv_ValueChanged(object sender, EventArgs e)
        {
            physics.EnvironmentRSEnv = (double)numericUpDownR_SEnv.Value;
        }

        private void numericUpDownE_LEnv_ValueChanged(object sender, EventArgs e)
        {
            physics.EnvironmentELBld = (double)numericUpDownE_LEnv.Value;
        }

        private void numericUpDownE_LGnd_ValueChanged(object sender, EventArgs e)
        {
            physics.EnvironmentELGnd = (double)numericUpDownE_LGnd.Value;
        }

        private void numericUpDownV_ValueChanged(object sender, EventArgs e)
        {
            
        }


        private void numericUpDownDX_SpatialStepSizeChanged(object sender, EventArgs e)
        {
            physics.Deltax = (double)numericUpDownDX.Value;

            for (int k = 0; k < physics.Walls.Count; k++)
            {
                physics.Walls[k].Deltax = physics.Deltax;
            }

            vehicleParametersChanged();
        }

        private void gparInitialTemperatureChanged(object sender, EventArgs e)
        {
            physics.InitialT = (double)numericUpDownT0.Value;
            vehicleParametersChanged();
        }

        private void prepareResultOutputFiles()
        {
            //Die im Log beheimateten Streamwriter öffnen
            eventLog.OutputTCabinFile = new StreamWriter(eventLog.resultOutputPath + Log.resultTCabinFileName);
            eventLog.OutputQCabinFile = new StreamWriter(eventLog.resultOutputPath + Log.resultQCabinFileName);
            eventLog.OutputGeneralFile = new StreamWriter(eventLog.resultOutputPath + Log.resultGeneralFileName);
            //eventLog.OutputLogFile = new StreamWriter(eventLog.resultOutputPath + Log.logFileName);
            //eventLog.OutputLogFile.WriteLine("Simulation started: " + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + " at " + DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes + ":" + DateTime.Now.TimeOfDay.Seconds);
            

            //Mit den jeweiligen Datenserien assoziieren
            physics.Cabin.T.setFileLogging(eventLog.OutputTCabinFile);
            physics.Cabin.Q.setFileLogging(eventLog.OutputQCabinFile);

            eventLog.resultOutputStreamWriterList = new List<StreamWriter>();
            eventLog.resultEnvironmentHeatCurrentsFile = new List<StreamWriter>();

            //Selbiges für sämtliche Sublayer durchführen
            for (int i = 0; i < physics.Walls.Count; i++)
            {
                StreamWriter eHCFile = new StreamWriter(eventLog.resultOutputPath + Log.resultEnvironmentHeatCurrentsFileName.Replace("$$", i.ToString()));
                eHCFile.AutoFlush = true;
                eventLog.resultEnvironmentHeatCurrentsFile.Add(eHCFile);

                for (int j = 0; j < physics.Walls[i].Bauteil.Count; j++)
                {
                    if (physics.Walls[i].getArea(j) > 0)
                    {
                        for (int k = 0; k < physics.Walls[i].Bauteil[j].Count; k++)
                        {
                            for(int l = 0;l< physics.Walls[i].Bauteil[j][k].SubLayers.Length;l++)
                            {
                                StreamWriter QFile = new StreamWriter(eventLog.resultOutputPath + "wall-" + i + "_part-" + j + "_layer-" + k + "_sub-" + l + "_Q.csv");
                                StreamWriter TFile = new StreamWriter(eventLog.resultOutputPath + "wall-" + i + "_part-" + j + "_layer-" + k + "_sub-" + l + "_T.csv");
                                
                                eventLog.resultOutputStreamWriterList.Add(QFile);
                                eventLog.resultOutputStreamWriterList.Add(TFile);

                                physics.Walls[i].Bauteil[j][k].SubLayers[l].setFileLogging(QFile);
                                physics.Walls[i].Bauteil[j][k].SubLayersT[l].setFileLogging(TFile);
                                if ((checkBoxLogAllHCs.Checked) || (l== physics.Walls[i].Bauteil[j][k].SubLayers.Length-1)) //Nur innerster Sublayer werden auch sonst gelogged - sonst wissen wir nichts über die Transmittierte Strahlung
                                {
                                    StreamWriter qFile = new StreamWriter(eventLog.resultOutputPath + "wall-" + i + "_part-" + j + "_layer-" + k + "_sub-" + l + "_hc.csv");
                                    qFile.AutoFlush = true;

                                    qFile.WriteLine(
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        physics.InitialT + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR +
                                        "0" + Utilities.C_SEPARATOR
                                        );

                                    physics.Walls[i].Bauteil[j][k].HeatCurrentLogfile.Add(qFile);
                                }
                                else
                                {
                                    physics.Walls[i].Bauteil[j][k].HeatCurrentLogfile.Add(null);
                                }
                            }
                        }
                    }
                    
                }
            }
        }

        private void closeResultOutputfiles()
        {
            //Streamwriter schließen
            eventLog.OutputTCabinFile.Close();
            eventLog.OutputQCabinFile.Close();
            eventLog.OutputGeneralFile.Close();

            //eventLog.OutputLogFile.WriteLine("Simulation ended: " + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + " at " + DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes + ":" + DateTime.Now.TimeOfDay.Seconds);
            //eventLog.OutputLogFile.Close();

            for (int i = 0; i < eventLog.resultOutputStreamWriterList.Count; i++)
            {
                eventLog.resultOutputStreamWriterList[i].Close();
            }

            for (int i = 0; i < eventLog.resultEnvironmentHeatCurrentsFile.Count; i++)
            {
                eventLog.resultOutputStreamWriterList[i].Close();
            }
        }

        public void InitTimer()
        {
            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(run_new_batch_sim);
            timer1.Interval = 2000; // in miliseconds
            timer1.Start();
        }

        private void run_new_batch_sim(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy) { 

                labelManuellerModus.Text = "Batch Modus für " + simulationsToRun[simulationRunning]+ "\nJob "+(simulationRunning+1)+" von "+simulationsToRun.Length;

                InputPath = simulationsToRun[simulationRunning];
                basicInit();
                loadSimulation(simulationsToRun[simulationRunning]);
                prepareSimulation(sender, e);
                if((simulationRunning+1)<simulationsToRun.Length)
                {
                    simulationRunning++;
                }
                else
                {
                    timer1.Stop();
                    simulationRunning = 0;
                }
            }
        }

        private void prepareSimulation_Batch(object sender, EventArgs e)
        {
            simulationsToRun = textBoxBatchDirectories.Text.Replace("\r", "").Split('\n');
            InitTimer();
        }

        private void prepareSimulation(object sender, EventArgs e)
        {
            eventLog.clear();
            eventLog.add(new LogEntry("startSimulation()", physics.TDuration / physics.Deltat + " timesteps dt=" + physics.Deltat / 3600 + "s duration=" + physics.TDuration + "h", Log.LOG_INFO_TYPE));

            thermalInitialization();

            eventLog.resultOutputBasePath = InputPath;
            eventLog.createOutPutPath();


            
            timeStepsTotal = physics.TimeStepsTotal;
            timeStepNow = 1;

            progressBar1.Maximum = physics.TimeStepsTotal;

            // Ergebnis Graphen befüllen
            // Wall[x] ... Fahrzeugwand x
            // Bauteil[y][z] ... Bauteiltyp y und davon die Schicht z
            Layer l = physics.Walls[0].Bauteil[3][0];

            chartWallTvtLayer1.Series.Clear();
            chartWallTvtLayer2.Series.Clear();
            chartWallTAirvt.Series.Clear();

            /*
            for (int i = 0; i < l.SubLayers.Length; i++)
            {
                chartWallTvtLayer1.Series.Add(i.ToString());
                chartWallTvtLayer1.Series[i.ToString()] = l.SubLayersT[i].series;
            }
            */
            //l = physics.Walls[0].Bauteil[3][1];
            /*
            int modGraphAdd = 1;

            if (l.SubLayers.Length == 100) { modGraphAdd = 10; }
            */
            /*
                for (int i = 0; i < l.SubLayers.Length; i++)
            {
                if (i % 10 == 0)
                {
                    chartWallTvtLayer2.Series.Add(i.ToString());
                    chartWallTvtLayer2.Series[i.ToString()] = l.SubLayersT[i].series;
                }
            }
            */
            chartWallTAirvt.Series.Add("CabinAir");
            chartWallTAirvt.Series["CabinAir"] = physics.Cabin.T.series;
            chartWallTAirvt.Series.Add(messwertFahrzeugTiMittelwert.series);

            textBoxRechenzeit.Text = "-messe-";

            buttonStartSimulation.Enabled = false;

            backgroundWorker.RunWorkerAsync();
        }


        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Aus irgendwelchen Gründen ists manchmal um eins zu hoch?
            if (e.ProgressPercentage > progressBar1.Maximum)
                progressBar1.Value = progressBar1.Maximum;
            else progressBar1.Value = e.ProgressPercentage;

            double percentage = ((double)(e.ProgressPercentage) / ((double)physics.TimeStepsTotal));
            double timeNow = stopwatch.ElapsedMilliseconds / 1000;
            double timeToFinish = Math.Round(((timeNow / (percentage * 100)) * 100) / 60 - timeNow / 60, 1);

            labelVerbleibendeZeit.Text = /*(timeNow).ToString() + "s / " +*/ timeToFinish.ToString() + " m";

            progressBarText.Text = (Math.Round(percentage * 100, 1)).ToString() + "%";
            // Set the text.
            this.Text = e.ProgressPercentage.ToString();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            prepareResultOutputFiles();
            // Zeit mitstoppen
            simulationStart = DateTime.Now;

            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(3);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1200)
            {
                Math.Sqrt(10E9);
            }
            stopwatch.Stop();

            stopwatch.Reset();
            stopwatch.Start();

            for (double time = physics.Deltat; time < physics.TDuration; time += physics.Deltat)
            {

                timeStepNow += 1;

                Debug.WriteLine("");
                Debug.WriteLine("======================================================= TIMESTEP " + timeStepNow + "/" + (timeStepsTotal) + "=======================================================");
                Debug.WriteLine("t=" + time + " TAir = " + physics.Cabin.T.LastY + " °C");
                for (int i = 0; i < physics.Walls.Count; i++)
                {
                    //Debug.WriteLine("------------------------------------------------------------------------------");
                    //Debug.WriteLine("wall " + i);
                    physics.Walls[i].heatFluxCalculation(time, physics);
                }

                physics.Cabin.Q.addPoints(time, physics.Cabin.Q.LastY + physics.DQAirEffective * physics.DeltatSeconds);
                physics.Cabin.T.addPoints(time, (physics.Cabin.Q.LastY + physics.DQAirEffective * physics.DeltatSeconds) * physics.Cabin.ConversionFactorJ2K);
                physics.DQAirEffective = 0;


                backgroundWorker.ReportProgress(timeStepNow);
            }

            //Ermittle wieviel Energie nach Abschluss der Simulationen innerhalb der Schichten steckt
            double deltaQTotal = 0;

            for (int i = 0; i < physics.Walls.Count; i++)
            {
                Wall wall = physics.Walls[i];

                for (int j = 0; j < wall.Bauteil.Count; j++)
                {

                    if (wall.getArea(j) > 0)
                    {
                        //Debug.WriteLine("  wall.heatFluxCalculation(" + time + ") bauteil " + this.BauteilNamen[j]);
                        //log.add(new LogEntry("startSimulation", "Berechne Bauteil " + this.BauteilNamen[j] + "...", Log.LOG_INFO_TYPE));

                        for (int k = 0; k < wall.Bauteil[j].Count; k++)
                        {
                            Layer layer = wall.Bauteil[j].ElementAt(k);

                            for (int l = 0; l < layer.SubLayers.Length; l++)
                            {
                                deltaQTotal += layer.SubLayers[l].LastY - physics.InitialT*layer.ConversionFactorK2J;
                            }
                        }
                    }
                }
            }

            //Erhöhung der thermischen Energie der Kabinenluft
            deltaQTotal+=physics.Cabin.Q.LastY - physics.InitialT*physics.Cabin.ConversionFactorK2J;

            simulationEnd = DateTime.Now;

            eventLog.OutputGeneralFile.WriteLine("ΔQTotal;" + deltaQTotal);
            eventLog.OutputGeneralFile.WriteLine("tOffset;" + tOffset);
            eventLog.OutputGeneralFile.WriteLine("Version;" + Icov.C_VERSION);
            eventLog.OutputGeneralFile.WriteLine("Started;" + simulationStart.ToString());
            eventLog.OutputGeneralFile.WriteLine("Finished;" + simulationEnd.ToString());
            eventLog.OutputGeneralFile.WriteLine("Δx;"+physics.Deltax+";mm");
            eventLog.OutputGeneralFile.WriteLine("Δt;" + (physics.Deltat*3600)+";s");
            eventLog.OutputGeneralFile.WriteLine("cPAir;"+Physics.C_CP_AIR+";J/kgK");
            eventLog.OutputGeneralFile.WriteLine("cRhoAir;" + Physics.C_RHO_AIR + ";kg/m³");
            eventLog.OutputGeneralFile.WriteLine("cLambdaAir;" + Physics.C_LAMBDA_AIR + ";W/Km");
            eventLog.OutputGeneralFile.WriteLine("List of Layers and their thermal properties:");

            //Schreibe eine Liste aller Layer in general.csv - dann ist klar welche Parameter verwendet wurden in der Simulation
            for (int i = 0; i < physics.Walls.Count; i++)
            {
                Wall wall = physics.Walls[i];

                for (int j = 0; j < wall.Bauteil.Count; j++)
                {

                    if (wall.getArea(j) > 0)
                    {
                        //Debug.WriteLine("  wall.heatFluxCalculation(" + time + ") bauteil " + this.BauteilNamen[j]);
                        //log.add(new LogEntry("startSimulation", "Berechne Bauteil " + this.BauteilNamen[j] + "...", Log.LOG_INFO_TYPE));

                        for (int k = 0; k < wall.Bauteil[j].Count; k++)
                        {
                            Layer layer = wall.Bauteil[j].ElementAt(k);

                            eventLog.OutputGeneralFile.WriteLine("w"+wall.ID+"-p"+j+"-l"+k+"-"+layer.ToString());
                        }
                    }
                }
            }

            stopwatch.Stop();
            

            this.populateTree();


        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            textBoxRechenzeit.Text = (stopwatch.ElapsedMilliseconds / 1000).ToString();

            //Achsenskalierung setzen
            chartWallTvtLayer1.ChartAreas[0].AxisX.Minimum = Utilities.minValue(physics.Walls[0].Bauteil[3][0].SubLayersT[0], 0);
            chartWallTvtLayer1.ChartAreas[0].AxisX.Maximum = Utilities.maxValue(physics.Walls[0].Bauteil[3][0].SubLayersT[0], 0);
            chartWallTvtLayer1.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            chartWallTvtLayer1.ChartAreas[0].AxisY.Minimum = Utilities.minValue(physics.Walls[0].Bauteil[3][0].SubLayersT[0], 1);
            chartWallTvtLayer1.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(physics.Walls[0].Bauteil[3][0].SubLayersT[0], 1);
            chartWallTvtLayer1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";

            
            chartWallTAirvt.ChartAreas[0].AxisX.Minimum = Utilities.minValue(physics.Cabin.T, 0);
            chartWallTAirvt.ChartAreas[0].AxisX.Maximum = Utilities.maxValue(physics.Cabin.T, 0);
            chartWallTAirvt.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            chartWallTAirvt.ChartAreas[0].AxisY.Minimum = Utilities.minValue(physics.Cabin.T, 1) - 2;
            chartWallTAirvt.ChartAreas[0].AxisY.Maximum = Utilities.maxValue(physics.Cabin.T, 1) + 2;
            chartWallTAirvt.ChartAreas[0].AxisY.LabelStyle.Format = "{0:0.00}";

            closeResultOutputfiles();
            

            buttonStartSimulation.Enabled = true;

        }

        private void gparTDurationChanged(object sender, EventArgs e)
        {
            physics.TDuration = (double)numericUpDownSimulationTime.Value;
        }

        private void label101_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDownAijTest_ValueChanged(object sender, EventArgs e)
        {
            double result = physics.AOnOrientedSurface(0, (double)numericUpDownAlphaAijTest.Value, (double)numericUpDownBetaAijTest.Value);
            textBoxAijTestResult.Text = String.Format("{0:0.0000}", result);

            System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\temp\\aij.csv");
            double alpha = (double)numericUpDownAlphaAijTest.Value;
            double beta = (double)numericUpDownBetaAijTest.Value;

            double lowAlpha;
            double highAlpha;

            double lowBeta;
            double highBeta;

            lowAlpha = Utilities.mod((int)alpha - 90, 360);
            highAlpha = Utilities.mod((int)alpha + 90, 360);


            lowBeta = beta - 90;
            highBeta = beta + 90;
            /*
            double alpha_i;
            int lasti = 0;*/

            for (int i = (int)lowAlpha; Utilities.mod(i, 360) != (highAlpha); i += Physics.deltaAlpha)
            {
                double alpha_i = Utilities.mod(i, 360);
                for (int j = (int)lowBeta; j <= highBeta; j += Physics.deltaBeta)
                {
                    double beta_j = j;
                    if (beta_j < 0) beta_j *= -1;
                    else if (beta_j > 180) beta_j = 180 - (beta_j - 180);

                    file.WriteLine(i + "," +j + "," +"," + physics.Aij(0, alpha_i, beta_j) + "," + Math.Sin(Physics.d2r * beta_j) * Math.Cos(Physics.d2r * physics.Eta(alpha, beta, alpha_i, beta_j)) * physics.Aij(0, alpha_i, beta_j));

                }
            }
            file.Close();
        }

        private void Icov_Load(object sender, EventArgs e)
        {

        }

        private void textBoxSimulationFolder_Click(object sender, EventArgs e)
        {
            
        }

        private void folderBrowserDialogSimulation_HelpRequest(object sender, EventArgs e)
        {

        }

        private void buttonLoadSimParameters_Click(object sender, EventArgs e)
        {
            labelStatus.Text = "Auswahl des Simulationspfades...";
            DialogResult result = folderBrowserDialogSimulation.ShowDialog();
        
            if (result == DialogResult.OK)
            {
                
                //
                // The user selected a folder and pressed the OK button.
                // We print the number of files found.
                //
                textBoxSimulationFolder.Text = folderBrowserDialogSimulation.SelectedPath;

                InputPath = folderBrowserDialogSimulation.SelectedPath;

                loadSimulation(folderBrowserDialogSimulation.SelectedPath);

                labelStatus.Text = "Daten geladen, bereit";
                buttonStartSimulation.Enabled = true;
            }
        }

        private void checkBoxFixedSun_checkstateChanged(object sender, EventArgs e)
        {
            if (!physics.SunFixed)
            {
                physics.fixSun((double)numericUpDownFixedGamma.Value, (double)numericUpDownFixedPsi.Value);
                labelSonnenpositionFixiert.Text = "Achtung! Sonnenposition fixiert in Icov.basicInit() auf " + physics.gamma(2000) + "° und " + physics.psi(2000) + "° !";
            }
            else
            {
                physics.SunFixed = false;
                labelSonnenpositionFixiert.Text = "Sonnenposition variabel!";
            }

        }

        private void checkBoxT0VehicleAsT0Sim_CheckedChanged(object sender, EventArgs e)
        {
            if (buttonStartSimulation.Enabled == true)
            {
                if (checkBoxT0VehicleAsT0Sim.Checked)
                {
                    physics.InitialT = messwertFahrzeugTiMittelwert.FirstY;
                }
                else
                {
                    physics.InitialT = physics.TA(0);
                }
                numericUpDownT0.Value = (decimal)physics.InitialT;
            }

        }

        private void checkBoxLogAllHCs_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}