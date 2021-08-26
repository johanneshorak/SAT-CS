using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Icov
{
    class Physics
    {
        

        public static double C_T0 = 273.15;
        public static double C_SIGMA = 0.0000000567;

        public static double C_AK = 405;
        public static double C_A = 2.8;

        public static double C_RHO_AIR = 1.161;//1.225;     // density of air at 300K kg/m³ source: handbook of chemistry and physics
        public static double C_CP_AIR = 1007;//1005;       // specific heat of air at 300K kg/m³ source handbook of chemistry and physics
        public static double C_LAMBDA_AIR = 0.026;//0.026;  //Wärmeleitfähigkeit Luft bei 300K

        public static double C_RHO_INTERIOR = 55;
        public static double C_CV_INTERIOR = 1300;


        public static int deltaAlpha = 5;
        public static int deltaBeta = 5;

        private List<Wall> walls = new List<Wall>();
        public List<Wall> Walls
        {
            get { return walls;  }
            set { walls = value; }
        }



        private double dQAirEffective = 0;
        public double DQAirEffective
        {
            get { return dQAirEffective;  }
            set { dQAirEffective = value;  }
        }

        private double dQInteriorEffective = 0;
        public double DQInteriorEffective
        {
            get { return dQInteriorEffective; }
            set { dQInteriorEffective = value; }
        }

        private bool sunFixed = false;
        public bool SunFixed
        {
            get { return sunFixed; }
            set { sunFixed = value; }
        }

        private double sonneGammaFixiert;
        private double sonnePsiFixiert;

        private double deltat = 1;
        public double Deltat
        {
            get { return deltat;  }
            set { deltat = value; }
        }
        public double DeltatSeconds
        {
            get { return deltat * 3600; }
        }

        private double deltax = 0.5;
        public double Deltax
        {
            get { return deltax;  }
            set { deltax = value; }
        }

        private double tDuration;
        public double TDuration
        {
            get { return tDuration; }
            set { tDuration = value;  }
        }

        private Compartment compartment;

        public Compartment Cabin
        {
            get { return compartment; }
            set { compartment = value; }
        }


        public int TimeStepsTotal
        {
            get { return (int)Math.Round(TDuration / Deltat, 1); }
        }

        private double initialT;
        public double InitialT
        {
            get { return initialT; }
            set { initialT = value; }
        }

        //ACHTUNG: FÜR VERSUCHSZEITRÄUME BEDEUTEND LÄNGER ALS EINEN TAG MÜSSTEN AUCH DIESE WERTE ALS TABELLEN HINTERLEGT WERDEN!
        public double parSonneX;
        public double parSonneZperMin;
        public double parSonneDeklination;
        //----------------------------------------------------------------------------------------------------------------------

        public double parVehicleLatitude;
        public double parVehicleLongitude;

        private DateTime startMeteorologieDaten;
        public DateTime StartMeteorologieDaten
        {
            get { return startMeteorologieDaten; }
            set { 
                startMeteorologieDaten = value;
                tOffsetToMeteorology = StartZeit.Hour - startMeteorologieDaten.Hour + (StartZeit.Minute - startMeteorologieDaten.Minute) / 60.0;
            }
        }

        private DateTime startZeit;
        public  DateTime StartZeit
        {
            get { return startZeit; }
            set { 
                startZeit = value;
                calcSonnenWOZTable();
                tOffsetToMeteorology = StartZeit.Hour - startMeteorologieDaten.Hour + (StartZeit.Minute - startMeteorologieDaten.Minute) / 60.0;
            }
        }

        public double tOffsetToMeteorology = 0;

        private DateTime startDatum;
        public DateTime StartDatum
        {
            get { return startDatum; }
            set { 
                startDatum = value;
                calcPSonnenParameter();
            }
        }


        private DataPoints timeStepsMeteorology;
        public DataPoints  TimeStepsMeteorology
        {
            get { return timeStepsMeteorology; }
            set { 
                timeStepsMeteorology = value;
                calcSonnenWOZTable();
            }
        }

        private DataPoints timeStepsSimulation;
        public DataPoints  TimeStepsSimulation
        {
            get { return timeStepsSimulation; }
            set {
                    timeStepsSimulation = value;
                    calcSonnenWOZTable();
            }
        }

        private DataPoints meteorologyAmbientTemperature;
        public DataPoints MeteorologyAmbientTemperature
        {
            get { return meteorologyAmbientTemperature; }
            set { meteorologyAmbientTemperature = value; }
        }

        private DataPoints meteorologyGlobalRadiation;
        public DataPoints MeteorologyGlobalRadiation
        {
            get { return meteorologyGlobalRadiation; }
            set { meteorologyGlobalRadiation = value; }
        }

        private DataPoints meteorologyDiffuseRadiation;
        public DataPoints MeteorologyDiffuseRadiation
        {
            get { return meteorologyDiffuseRadiation; }
            set { meteorologyDiffuseRadiation = value; }
        }

        private DataPoints meteorologyWindSpeedv10m;
        public DataPoints MeteorologyWindSpeedv10m
        {
            get { return meteorologyWindSpeedv10m; }
            set { meteorologyWindSpeedv10m = value; }
        }

        private DataPoints meteorologyWindSpeedv1m;
        public DataPoints MeteorologyWindSpeedv1m
        {
            get { return meteorologyWindSpeedv1m; }
            set { meteorologyWindSpeedv1m = value; }
        }

        private DataPoints environmentHorizon;
        public DataPoints EnvironmentHorizon
        {
            get { return environmentHorizon; }
            set { environmentHorizon = value; }
        }


        private DataPoints sonneGammaTable;
        public DataPoints SonneGammaTable
        {
            get { return sonneGammaTable; }
            set { sonneGammaTable = value; }
        }

        private DataPoints sonnePsiTable;
        public DataPoints SonnePsiTable
        {
            get { return sonnePsiTable; }
            set { sonnePsiTable = value; }
        }

        private DataPoints sonneWOZTable;
        public DataPoints SonneWOZTable
        {
            get { return sonneWOZTable; }
            set { sonneWOZTable = value; }
        }

        private DataPoints meteorologyEpsilonSkyTable;
        public DataPoints MeteorologyEpsilonSkyTable
        {
            get { return meteorologyEpsilonSkyTable; }
            set { meteorologyEpsilonSkyTable = value; }
        }

        DataPoints kappaSkyTable;
        public DataPoints KappaSkyTable
        {
            get
            {
                return kappaSkyTable;
            }

            set
            {
                kappaSkyTable = value;
            }
        }

        public void fixSun(double gamma, double psi)
        {
            this.SunFixed = true;
            this.sonnePsiFixiert = psi;
            this.sonneGammaFixiert = gamma;
        }

        //Diese Tabellen werden erst unmittelbar vor Simulationsbegin generiert
        List<DataPoints> meteorologyDebugTable = new List<DataPoints>();
        public List<DataPoints> MeteorologyDebugTable
        {
            get { return meteorologyDebugTable; }
            set { meteorologyDebugTable = value; }
        }


        private double environmentRSEnv;
        public double EnvironmentRSEnv
        {
            get { return environmentRSEnv; }
            set { environmentRSEnv = value; }
        }

        private double environmentELBld;
        public double EnvironmentELBld
        {
            get { return environmentELBld; }
            set { environmentELBld = value; }
        }

        private double environmentELGnd;
        public double EnvironmentELGnd
        {
            get { return environmentELGnd; }
            set { environmentELGnd = value; }
        }

        public static double d2r = Math.PI / 180;
        public static double r2d = 180 / Math.PI;

        

        Log eventLog;

        public Physics(Log log) {
            eventLog = log;

            //Diese Tabellen werden im DataLoader initialisiert
            TimeStepsMeteorology = new DataPoints(log);
            MeteorologyAmbientTemperature = new DataPoints(log);
            MeteorologyGlobalRadiation = new DataPoints(log);
            MeteorologyDiffuseRadiation = new DataPoints(log);
            EnvironmentHorizon = new DataPoints(log);
            EnvironmentHorizon.series.Name = "Horizonthöhe";

            //Vorberechnete KappaSkyTable anlegen. Besteht aus linearer Interpolation der in VDI 3789 in Anhang H angegebenen Funktion und Tabelle
            //und Rückrechnung der so gewonnenen kontinuierlichen Funktion in diskretisierte Tabelle. (kappaTable.nb)
            KappaSkyTable = new DataPoints(log);
                KappaSkyTable.addPoints((double)253, (double)0.62);
                KappaSkyTable.addPoints((double)254, (double)0.68);
                KappaSkyTable.addPoints((double)255, (double)0.74);
                KappaSkyTable.addPoints((double)256, (double)0.8);
                KappaSkyTable.addPoints((double)257, (double)0.87);
                KappaSkyTable.addPoints((double)258, (double)0.95);
                KappaSkyTable.addPoints((double)259, (double)1.03);
                KappaSkyTable.addPoints((double)260, (double)1.1);
                KappaSkyTable.addPoints((double)261, (double)1.19);
                KappaSkyTable.addPoints((double)262, (double)1.28);
                KappaSkyTable.addPoints((double)263, (double)1.37);
                KappaSkyTable.addPoints((double)264, (double)1.47);
                KappaSkyTable.addPoints((double)265, (double)1.58);
                KappaSkyTable.addPoints((double)266, (double)1.7);
                KappaSkyTable.addPoints((double)267, (double)1.82);
                KappaSkyTable.addPoints((double)268, (double)1.95868);
                KappaSkyTable.addPoints((double)269, (double)2.08947);
                KappaSkyTable.addPoints((double)270, (double)2.229);
                KappaSkyTable.addPoints((double)271, (double)2.37784);
                KappaSkyTable.addPoints((double)272, (double)2.53662);
                KappaSkyTable.addPoints((double)273, (double)2.70601);
                KappaSkyTable.addPoints((double)274, (double)2.88671);
                KappaSkyTable.addPoints((double)275, (double)3.07947);
                KappaSkyTable.addPoints((double)276, (double)3.28511);
                KappaSkyTable.addPoints((double)277, (double)3.50447);
                KappaSkyTable.addPoints((double)278, (double)3.73849);
                KappaSkyTable.addPoints((double)279, (double)3.98813);
                KappaSkyTable.addPoints((double)280, (double)4.25444);
                KappaSkyTable.addPoints((double)281, (double)4.53854);
                KappaSkyTable.addPoints((double)282, (double)4.84161);
                KappaSkyTable.addPoints((double)283, (double)5.16491);
                KappaSkyTable.addPoints((double)284, (double)5.5098);
                KappaSkyTable.addPoints((double)285, (double)5.87773);
                KappaSkyTable.addPoints((double)286, (double)6.27022);
                KappaSkyTable.addPoints((double)287, (double)6.68892);
                KappaSkyTable.addPoints((double)288, (double)7.13558);
                KappaSkyTable.addPoints((double)289, (double)7.61207);
                KappaSkyTable.addPoints((double)290, (double)8.12037);
                KappaSkyTable.addPoints((double)291, (double)8.66262);
                KappaSkyTable.addPoints((double)292, (double)9.24108);
                KappaSkyTable.addPoints((double)293, (double)9.85816);
                KappaSkyTable.addPoints((double)294, (double)10.5165);
                KappaSkyTable.addPoints((double)295, (double)11.2187);
                KappaSkyTable.addPoints((double)296, (double)11.9678);
                KappaSkyTable.addPoints((double)297, (double)12.9);
                KappaSkyTable.addPoints((double)298, (double)13.9);
                KappaSkyTable.addPoints((double)299, (double)14.9);
                KappaSkyTable.addPoints((double)300, (double)16.0);
                KappaSkyTable.addPoints((double)301, (double)17.3);
                KappaSkyTable.addPoints((double)302, (double)18.8);
                KappaSkyTable.addPoints((double)303, (double)20.4);
                KappaSkyTable.addPoints((double)304, (double)22.1);
                KappaSkyTable.addPoints((double)305, (double)24.2);
                KappaSkyTable.addPoints((double)306, (double)26.6);
                KappaSkyTable.addPoints((double)307, (double)28.9);
                KappaSkyTable.addPoints((double)308, (double)31.7);
                KappaSkyTable.addPoints((double)309, (double)35.4);
                KappaSkyTable.addPoints((double)310, (double)39.4);
                KappaSkyTable.addPoints((double)311, (double)44.2);
                KappaSkyTable.addPoints((double)312, (double)49.8);
                KappaSkyTable.addPoints((double)313, (double)57.3);

            //Diese Tabellen werden von der Physics Klasse befüllt
            MeteorologyWindSpeedv10m = new DataPoints(log);
            MeteorologyWindSpeedv1m = new DataPoints(log);

            MeteorologyDebugTable.Add(new DataPoints(log)); //D0
            MeteorologyDebugTable.Add(new DataPoints(log)); //H0
            MeteorologyDebugTable.Add(new DataPoints(log)); //R0
            MeteorologyDebugTable.Add(new DataPoints(log)); //A0

            

            SonneWOZTable = new DataPoints(log);
            sonneWOZTable.series.Name = "WOZ";
            sonneWOZTable.xName = "t (h)";
            sonneWOZTable.yName = "WOZ (h)";

            SonneGammaTable = new DataPoints(log);
            sonneGammaTable.series.Name = "SonneGamma";
            sonneGammaTable.xName = "t (h)";
            sonneGammaTable.yName = "γ (°)";

            SonnePsiTable = new DataPoints(log);
            sonnePsiTable.series.Name = "SonnePsi";
            sonnePsiTable.xName = "t (h)";
            sonnePsiTable.yName = "Ψ (°)";

            MeteorologyEpsilonSkyTable = new DataPoints(log);
            MeteorologyEpsilonSkyTable.series.Name = "EpsilonSky";
            MeteorologyEpsilonSkyTable.xName = "t (h)";
            MeteorologyEpsilonSkyTable.yName = "ε_Sky (1)";

            TimeStepsSimulation = new DataPoints(log);

            
        }

        public void calculateWindSpeedForSurfaceRoughness(double z0)
        {
            double nominator;
            double denominator;

            for (int i = 0; i < this.MeteorologyWindSpeedv10m.series.Points.Count; i++)
            {
                nominator = Math.Log(1 / z0);
                denominator = Math.Log(10 / z0);
                if (nominator / denominator < 0) { MeteorologyWindSpeedv1m.series.Points[i].YValues[0] = 0; }
                else { MeteorologyWindSpeedv1m.series.Points[i].YValues[0] = MeteorologyWindSpeedv10m.series.Points[i].YValues[0] * (nominator / denominator); }

                //Debug.WriteLine(v1m.series.Points[i].YValues[0] + " " + nominator + " " + denominator);
            }
        }

        //Eventuell will man entscheiden ob man die meteorologischen Berechnungen auf meteorologische Zeitschritte diskretisiert oder auf Simulationszeitschritte
        //weil den Sonnenstand könnte man zB. ohne Probleme genauer berechnen und müsste dann nicht auf so großem Intervall linear interpolieren.
        public DataPoints getTimeDiscretizationTable()
        {
            return (TimeStepsMeteorology);
        }


        // Parameter
        //      datum               ....Tagesdatum für das Sonnenparameter bestimmt werden sollen
        // Sonstige Abhängigkeiten
        //
        public void calcPSonnenParameter()
        {
            parSonneX = startDatum.DayOfYear * 0.9856 - 2.72;

            Debug.WriteLine("DOYEAR " + startDatum.DayOfYear);

            double sinPar1 = d2r*parSonneX;
            double sinPar2 = d2r*(2*parSonneX+24.99+3.83*Math.Sin(d2r*parSonneX));
            parSonneZperMin = -7.66*Math.Sin(sinPar1)-9.87*Math.Sin(sinPar2);

            double aSinPar = 0.3978*Math.Sin(d2r * (parSonneX - 77.51+1.92*Math.Sin(d2r*parSonneX)));
            parSonneDeklination = r2d * Math.Asin(aSinPar);

            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "========================================================================", Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "Aufgerufen mit " + startDatum, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "parSonneX=" + parSonneX, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "parSonneZperMin=" + parSonneZperMin, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "parSonneDeklination=" + parSonneDeklination, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "    aSinPar=" + aSinPar, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcPSonnenParameter()", "========================================================================", Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));

            calcSonnenWOZTable();
        }

        // Parameter 
        //      t                   ...(h) Ortszeit
        // Pre Abhängigkeiten
        //      parVehicleLatitude  ...(°) Geographische Breite des Versuchsortes
        //      parVehicleLongitude ...(°) Geographische Länge des Versuchsortes
        //      parSonneZperMin     ...
        // Post Abhängigkeiten
        //      sonneGammaTable     ...(°) Tabelle mit Sonnenhöhenwinkel für Simulationszeitraum
        public double calculateSonnenWOZ(double t)
        {
            double woz;

            woz = t - (4.0 / 60.0) * (15.0 - parVehicleLongitude) + parSonneZperMin/60.0;
            //eventLog.add(new LogEntry("physics:calculateWOZ(double)", " woz = " + t + " - " + ((4.0 / 60.0)*(15.0 - parVehicleLongitude))+" + "+parSonneZperMin/60.0, Log.LOG_DEBUG_TYPE));
            return (woz);
        }

       
        public void calcSonnenWOZTable()
        {
            double time,time0;

            time0 = StartZeit.Hour + StartZeit.Minute / 60;

            eventLog.add(new LogEntry("physics:calcSonnenWOZTable()", "WOZ Tabelle neu berechnet für " + getTimeDiscretizationTable().series.Points.Count + " Punkte", Log.LOG_DEBUG_TYPE));
            eventLog.add(new LogEntry("physics:calcSonnenWOZTable()", "    x0 = " + time0, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcSonnenWOZTable()", "    lon = " + parVehicleLongitude, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            eventLog.add(new LogEntry("physics:calcSonnenWOZTable()", "    lat = " + parVehicleLatitude, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            
            if(SonneWOZTable != null)
                sonneWOZTable.clear();

            for (int i = 0; i < getTimeDiscretizationTable().series.Points.Count; i++)
            {
                // ACHTUNG - BERECHNUNG FUNKTIONIERT AUF DIESE WEISE EVT. NUR SOLANGE DER TAG SICH DURCH ADDITION VON EINER STUNDE NICHT ÄNDERT!
                time = time0 + getTimeDiscretizationTable().series.Points[i].XValue;

                sonneWOZTable.series.Points.AddXY(getTimeDiscretizationTable().series.Points[i].XValue, calculateSonnenWOZ(time));

                eventLog.add(new LogEntry("physics:calcSonnenWOZTable()", "    " + getTimeDiscretizationTable().series.Points[i].XValue + ": " + calculateSonnenWOZ(time), Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            }

            calculateSonnenGammaTable();
            
        }

        public double calculateSonnenGamma(double woz)
        {
            double gamma = 0;
            double sinArg1, sinArg2, cosArg1, cosArg2, cosArg3 ;

            sinArg1 = d2r * parVehicleLatitude;
            sinArg2 = d2r * parSonneDeklination;
            cosArg1 = d2r * parVehicleLatitude;
            cosArg2 = d2r * parSonneDeklination;
            cosArg3 = d2r * 15 * (woz - 12);

            gamma = r2d * Math.Asin(Math.Sin(sinArg1) * Math.Sin(sinArg2) + Math.Cos(cosArg1) * Math.Cos(cosArg2) * Math.Cos(cosArg3));

            return (gamma);
        }

        // Pre Abhängigkeiten
        //      woz               ...(h) Wahre Ortszeit
        // Post Abhängigkeiten
        //      sonnePsiTable     ...(°) Tabelle mit Sonnenhöhenwinkel für Simulationszeitraum
        public void calculateSonnenGammaTable()
        {
            double x;
            double woz;
            double gamma;

            eventLog.add(new LogEntry("physics:calculateSonnenGammaTable()", "SonnenGamma Tabelle neu berechnet für " + getTimeDiscretizationTable().series.Points.Count + " Punkte", Log.LOG_DEBUG_TYPE));

            if(SonneGammaTable!=null)
                SonneGammaTable.clear();

            for (int i = 0; i < getTimeDiscretizationTable().series.Points.Count; i++)
            {
                woz = SonneWOZTable.series.Points[i].YValues[0];
                x = getTimeDiscretizationTable().series.Points[i].XValue;
                gamma = calculateSonnenGamma(woz);

                SonneGammaTable.series.Points.AddXY(x, gamma);
                eventLog.add(new LogEntry("physics:calculateSonnenGammaTable()", "    " + x + ": " + gamma, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            }

            calculateSonnenPsiTable();
        }

        public double calculateSonnenPsi(double gamma, double woz)
        {
            double sinPar1, sinPar2, sinPar3, cosPar1, cosPar2;
            double psi = 0;

            sinPar1 = d2r * parVehicleLatitude;
            sinPar2 = d2r * gamma;
            sinPar3 = d2r * parSonneDeklination;
            cosPar1 = d2r * parVehicleLatitude;
            cosPar2 = d2r * gamma;

            double nominator = (Math.Sin(sinPar1) * Math.Sin(sinPar2) - Math.Sin(sinPar3));
            double denominator = Math.Cos(cosPar1) * Math.Cos(cosPar2);

            psi = r2d * Math.Acos
                (
                    nominator / denominator
                );

            if (woz < 12) psi *= -1;

            return (psi);
        }

        // Pre Abhängigkeiten
        //      gamma             ...(°) Sonnenhöhenwinkel
        // Post Abhängigkeiten
        //
        public void calculateSonnenPsiTable()
        {
            double x;
            double psi;
            double gamma, woz;

            eventLog.add(new LogEntry("physics:calculateSonnenGammaTable()", "SonnenPsi Tabelle neu berechnet für " + getTimeDiscretizationTable().series.Points.Count + " Punkte", Log.LOG_DEBUG_TYPE));

            if(SonnePsiTable!=null)
                SonnePsiTable.clear();

            for (int i = 0; i < getTimeDiscretizationTable().series.Points.Count; i++)
            {
                gamma   = SonneGammaTable.series.Points[i].YValues[0];
                woz     = SonneWOZTable.series.Points[i].YValues[0];
              
                //Achtung: +180 anscheinend nötig weil 0° = Norden (lt. VDI)
                psi = calculateSonnenPsi(gamma,woz)+180;
                if (psi >= 360) psi -= 360;

                x = getTimeDiscretizationTable().series.Points[i].XValue;

                SonnePsiTable.series.Points.AddXY(x, psi);
                eventLog.add(new LogEntry("physics:calculateSonnenPsiTable()", "    " + x + ": " + psi, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            }
        }

        public double previousTimeStep(double t)
        {
            return (t - this.Deltat);
        }


        public double calculateEpsilonSky(double TA)
        {
            double epsilon = 0;

            epsilon = 0.0000099 * (TA + C_T0) * (TA + C_T0);
           
            return (epsilon);
        }

        public void calculateEpsilonSkyTable()
        {
            double x;
            double epsilon;
            double TA;

            eventLog.add(new LogEntry("physics:calculateEpsilonSkyTable()", "EpsilonSky Tabelle neu berechnet für " + getTimeDiscretizationTable().series.Points.Count + " Punkte", Log.LOG_DEBUG_TYPE));

            if(MeteorologyEpsilonSkyTable!=null)
                MeteorologyEpsilonSkyTable.clear();

            for (int i = 0; i < getTimeDiscretizationTable().series.Points.Count; i++)
            {
                x = getTimeDiscretizationTable().series.Points[i].XValue;

                TA = MeteorologyAmbientTemperature.series.Points[i].YValues[0];
                epsilon = calculateEpsilonSky(TA);

                MeteorologyEpsilonSkyTable.series.Points.AddXY(x, epsilon);
                eventLog.add(new LogEntry("physics:calculateEpsilonSkyTable()", "    " + x + ": " + epsilon, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            }

        }

        public double vW(double t)
        {
            return(MeteorologyWindSpeedv1m.getInterpolatedValueAt(t + tOffsetToMeteorology));
        }

        public double TA(double t)
        {
                return (MeteorologyAmbientTemperature.getInterpolatedValueAt(t + tOffsetToMeteorology));
        }

        public double gamma(double t)
        {
            if (SunFixed)
            {
                return (sonneGammaFixiert);
            }
            else
            {
                return(SonneGammaTable.getInterpolatedValueAt(t));
            }
        }

        public double psi(double t)
        {
            if (SunFixed)
            {
                return (sonnePsiFixiert);
            }
            else
            {
                return (SonnePsiTable.getInterpolatedValueAt(t));
            }
        }

        public double D0(double t)
        {
            double horizonthoehe = 0;

            double sunAzimuth = 0;
            double sunAltitude = 0;
            double G = 0;
            double H = 0;
            double D0 = 0;


                G = MeteorologyGlobalRadiation.getInterpolatedValueAt(t + tOffsetToMeteorology);
                H = MeteorologyDiffuseRadiation.getInterpolatedValueAt(t + tOffsetToMeteorology);
                sunAzimuth = psi(t + tOffsetToMeteorology); //Utilities.mod((int)Math.Round(SonnePsiTable.getInterpolatedValueAt(t),1),360);
                sunAltitude = gamma(t + tOffsetToMeteorology);
                horizonthoehe = EnvironmentHorizon.getInterpolatedValueAt(sunAzimuth);

                

                if (sunAltitude > horizonthoehe)
                {
                    D0 = G - H;
                }
                else
                    D0 = 0;

                //eventLog.add(new LogEntry("physics:D0(t)", "D0="+G+"-"+H+"="+D0+" G=" + G + " H=" + H + " sunAz=" + sunAzimuth + " sunAlt=" + sunAltitude + " horizont=" + horizonthoehe, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));

                return (D0);
        }

        public double D(double time, double alpha, double beta)
        {
            double eta = Eta(time, alpha, beta);

            //NOCH IMPLEMENTIEREN
            //Wenn gamma < Horizonteinschränkung: return(0) - sollte in D0 implementiert sein.

            if (eta > 90) return (0);
            else
                return (Math.Cos(d2r*eta) * D0(time));
        }

        public double H0(double t)
        {
            return (MeteorologyDiffuseRadiation.getInterpolatedValueAt(t + tOffsetToMeteorology));
        }

        public double R0(double t)
        {
            double H = 0;
            double D = 0;
            double R0 = 0;

            D = D0(t);
            H = H0(t);

            R0 = EnvironmentRSEnv * (D + H);

            return (R0);

        }

        public double A(double t)
        {
            double esky = 0;
            double TA = 0;

            double A = 0;

            esky = MeteorologyEpsilonSkyTable.getInterpolatedValueAt(t);
            TA = C_T0 + MeteorologyAmbientTemperature.getInterpolatedValueAt(t);

            A = C_SIGMA * esky * (TA * TA * TA * TA);

            return (A);

        }
        
        public double getThermischenUebergangskoeffizienten(double l1, double l2)
        {
            double dK = 1E-4;
            double areaK = 0.5;
            double lambdaK = Physics.C_LAMBDA_AIR;
            //return (506);

            return( (1 / dK) * (areaK * (l1* l2) / (l1 + l2) + (1 - areaK) * lambdaK));
        }

        public double Aij(double t, double az, double alt)
        {


            //Annahme für Horizonteinschränkungen:
            //gamma >= 90 ==> Boden von 180 > a > gamma >= 90                            Himmel, ab  gamma > a >= 0 
            //gamma <  90 ==> Boden von 180 > a > 90         Gebäude von 90 > a >= gamma Himmel von  gamma > a >= 0

            double time = t + tOffsetToMeteorology;

            double gamma = 90-EnvironmentHorizon.getAngularInterpolatedValueAt(Utilities.mod((int)az,360));
            double TA    = C_T0 + MeteorologyAmbientTemperature.getInterpolatedValueAt(time);
            double area  = (double)(d2r*d2r*deltaAlpha * deltaBeta);

            double exponent = -0.3 * Math.Sqrt(KappaSkyTable.getInterpolatedValueAt(TA)/ Math.Cos(d2r * alt));
            double eSkyij   = 1.0-0.5*Math.Exp(exponent);

            //eventLog.add(new LogEntry("physics:Aij(t,az,alt)", "time="+time+" gamma="+gamma+" TA="+TA+" area="+area+" exponent="+exponent+" eSkyij="+eSkyij, Log.LOG_DEBUG_CALCULATION_DETAILED_INFO));
            

            int i = 0;

            double aij = 0;

            if      ( gamma > 90 )
            {
                if (alt >= gamma) // Boden
                {
                    // Abstrahlung von Boden zurückgeben
                    aij = area * C_SIGMA * EnvironmentELGnd * TA * TA * TA * TA / Math.PI;
                }
                else if (alt < gamma) //Himmel
                {
                    //Abstrahlung von Himmel zurückgeben
                    aij = area * C_SIGMA * eSkyij * TA * TA * TA * TA / Math.PI;
                    //Debug.WriteLine("time=" + time + " gamma=" + gamma + " TA=" + TA + " area=" + area + " alt = " + alt + " exponent=" + exponent + " eSkyij=" + eSkyij);
                }
            }
            else if ( gamma <= 90 )
            {
                if (alt >= 90) //Boden
                {
                    //Abstrahlung von Boden zurückgeben
                    aij = area * C_SIGMA * EnvironmentELGnd * TA * TA * TA * TA / Math.PI;
                }
                else if ((90 > alt) && (alt >= gamma)) //Gebäude
                {
                    //Abstrahlung von Gebäude zurückgeben
                    aij = area * C_SIGMA * EnvironmentELBld * TA * TA * TA * TA / Math.PI;
                }
                else if (alt < gamma) //Himmel
                {
                    //Abstrahlung von Himmel zurückgeben
                    aij = area * C_SIGMA * eSkyij * TA * TA * TA * TA / Math.PI;
                    //Debug.WriteLine("time=" + time + " gamma=" + gamma + " TA=" + TA + " area=" + area + " alt = " + alt + " exponent=" + exponent + " eSkyij=" + eSkyij);
                }
            }

            return(aij);
        }

        // Alpha ... azimuth
        // Beta ... Zenithwinkel
        // Psi ... Azimuth
        // Gamma ... Horizonthöhenwinkel

        // Hat noch Probleme für Winkel Beta > 90!!
        public double Eta(double alpha, double beta, double psi, double gamma)
        {
            double vector1X, vector1Y, vector1Z;
            double vector2X, vector2Y, vector2Z;

            vector1X = Math.Sin(d2r * beta) * Math.Cos(d2r * alpha);
            vector1Y = Math.Sin(d2r * beta) * Math.Sin(d2r * alpha);
            vector1Z = Math.Cos(d2r * beta);

            vector2X = Math.Sin(d2r * gamma) * Math.Cos(d2r * psi);
            vector2Y = Math.Sin(d2r * gamma) * Math.Sin(d2r * psi);
            vector2Z = Math.Cos(d2r * gamma);

            double scalar_product = vector1X * vector2X + vector1Y * vector2Y + vector1Z * vector2Z;

            double eta = r2d * Math.Acos(scalar_product);

            //double term1 = Math.Sin(d2r * gamma) * Math.Cos(d2r * beta);
            //double term2 = Math.Cos(d2r * gamma) * Math.Sin(d2r * beta) * Math.Cos(d2r*(alpha - psi));

            //double eta = r2d * Math.Acos(term1 + term2);

            return (eta);
        }


        public double Eta(double time, double alpha, double beta)
        {
            double g = 90-gamma(time + tOffsetToMeteorology); //Muss, um korrekten Winkel zu ergeben, noch in Polarwinkel umgerechnet werden. (Gamma wird ja vom Horizont weg gemessen und nicht von Zenith)
            double p = psi(time + tOffsetToMeteorology);         

            return (Eta(alpha, beta, p, g));
        }


        public double horizontNachZenithwinkel(double h) {
            return (90 - h);
        }

        public double AOnOrientedSurface(double t,double alpha, double beta) {
            double lowAlpha;
            double highAlpha;

            double lowBeta;
            double highBeta;

            double sum = 0;

            lowAlpha = Utilities.mod((int)alpha - 90, 360);
            highAlpha = Utilities.mod((int)alpha+ 90,360);


            lowBeta = beta - 90;

            highBeta = beta + 90;
            //Debug.WriteLine("lowBeta=" + lowBeta + " highBeta = " + highBeta);

            double alpha_i;


            for (int i = (int)lowAlpha; Utilities.mod(i,360) != (highAlpha) ;i+=deltaAlpha )
            {
                alpha_i = Utilities.mod(i, 360);
                for (int j = (int)lowBeta; j < highBeta; j+=deltaBeta)
                {
                    double beta_j = j;
                    if (beta_j < 0) beta_j *= -1;
                    else if (beta_j > 180) beta_j = 180-(beta_j-180);
                    sum += Math.Sin(d2r * beta_j) * Math.Cos(d2r * Eta(alpha, beta, alpha_i, beta_j)) * Aij(t, alpha_i, beta_j);
                    //Debug.WriteLine("   AOnOrientedSurface(): Eta=" + Eta(alpha, beta, alpha_i, beta_j) + " Cos(Eta)=" + Math.Cos(d2r * Eta(alpha, beta, alpha_i, beta_j)) + " i=" + i +" alpha_i="+alpha_i+" j="+j+" beta_j="+beta_j);                  
                }

            }


            return (sum);
        }

        public void fillMeteorologyDebugTables()
        {
            for (int i = 0; i < meteorologyDebugTable.Count;i++ )
            {
                meteorologyDebugTable[i].clear();
            }

            MeteorologyDebugTable[0].series.Name = "D0";
            MeteorologyDebugTable[1].series.Name = "H0";
            MeteorologyDebugTable[2].series.Name = "R0";
            MeteorologyDebugTable[3].series.Name = "A0";


            if (TimeStepsMeteorology.series.Points.Count > 0)
            {
                double dT = 0.5 * (TimeStepsMeteorology.series.Points[1].XValue - TimeStepsMeteorology.series.Points[0].XValue);
                double t = 0;

                while (t <= Utilities.maxValue(TimeStepsMeteorology, 0))
                {
                    //Debug.WriteLine(D0(t));

                    MeteorologyDebugTable[0].series.Points.AddXY(t, D0(t));
                    MeteorologyDebugTable[1].series.Points.AddXY(t, H0(t));
                    MeteorologyDebugTable[2].series.Points.AddXY(t, R0(t));
                    MeteorologyDebugTable[3].series.Points.AddXY(t, A(t));

                    t += dT;
                }
            }

        }



        public void heatCurrentsOfOuterLayer(double time, Wall wall, Layer layer, double T, ref double rS_D, ref double rS_H, ref double rS_R, ref double rL_HA, ref double rL_HW, ref double TA, ref double kOutside)
        {
            TA = this.TA(time);
            rS_R = R0(time);
            rS_D = D(time, layer.Alpha, layer.Beta);

            if (wall.ID == 1) 
            {
                //Keine diffuse Himmelsstrahlung kommt am Boden an
                //und - da darüber ein Fahrzeug steht - nehmen wir an, dass dieser in etwa Lufttemperatur hat.
                rS_H = 0;
                rL_HA = EnvironmentELGnd * Physics.C_SIGMA * (TA + Physics.C_T0) * (TA + Physics.C_T0) * (TA + Physics.C_T0) * (TA + Physics.C_T0);
            }
            else
            {
                rS_H = H0(time);
                rL_HA = AOnOrientedSurface(time, layer.Alpha, layer.Beta);
            }


            
            rL_HW = -Physics.C_SIGMA * layer.Material.Surface.EpsilonL * (T + Physics.C_T0) * (T + Physics.C_T0) * (T + Physics.C_T0) * (T + Physics.C_T0);


            kOutside = (Physics.C_A + 3 * vW(time)) * (TA - T); // Wattmuff

        }

        public void heatCurrentsOfInnerLayer(double time, Wall wall, Layer layer, double T, ref double TAir, ref double alphaL, ref double kInside, ref double rS_Tau_EffectiveF, ref double rS_Tau_EffectiveW, ref double rS_TauF, ref double rS_TauW, ref double aF, ref double aW, ref double dx, ref double dy, ref int wallTauFloorID, ref int wallTauOppositeID, ref double rL_HW, ref double oppositeTime, ref double TOpposite, ref double rL_HWOpposite)
        {

            alphaL = Physics.C_A;
            TAir = Cabin.T.LastY;
            double dimA = 0;
            double dimB = 0;

            kInside = alphaL * (T - TAir);

            //Berechnen, welche Strahlung von aktueller Schicht abgegeben wird und welche von gegenüberliegender Wand ankommt
            rL_HW = -Physics.C_SIGMA * /*layer.Material.Surface.EpsilonL **/ (T + Physics.C_T0) * (T + Physics.C_T0) * (T + Physics.C_T0) * (T + Physics.C_T0);

            //Schleife die alle Bauteile durchläuft und abgegebenen radiativen Wärmestrom nach Flächenanteil berücksichtigt.
            double totalArea = 0;

            for (int j = 0; j < Walls[5 - wall.ID].Bauteil.Count; j++)
            {
                int layersInBauteil = Walls[5 - wall.ID].Bauteil[j].Count;

                if (Walls[5 - wall.ID].getArea(j) > 0)
                {
                    int subLayerCount = Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT.Length;
                    //double oppositeTime = time;

                    if (Walls[wall.ID].ID > 2)
                    {
                        TOpposite = Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT[subLayerCount - 1].SecondToLastY; //Hier muss time verwendet werden da sonst nicht klar ist, ob LastY nicht schon time+dt ist.
                        oppositeTime = Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT[subLayerCount - 1].SecondToLastX;

                        //Debug.WriteLine("     *chose SecondToLastY from part "+j+", layer "+layersInBauteil+", sublayer "+subLayerCount);
                    }
                    else
                    {
                        TOpposite = Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT[subLayerCount - 1].LastY; //Hier muss time verwendet werden da sonst nicht klar ist, ob LastY nicht schon time+dt ist.
                        oppositeTime = Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT[subLayerCount - 1].LastX;

                        //Debug.WriteLine("     *chose LastY from part " + j + ", layer " + layersInBauteil + ", sublayer " + subLayerCount);
                    }

                    totalArea += Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Area;
                    rL_HWOpposite += Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Area * Physics.C_SIGMA * /*Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Material.Surface.EpsilonL **/ (TOpposite + Physics.C_T0) * (TOpposite + Physics.C_T0) * (TOpposite + Physics.C_T0) * (TOpposite + Physics.C_T0);

                    /*if (debugOut)*/
                    //Debug.WriteLine("     Wall = " + wall.ID);
                    //Debug.WriteLine("     T = "+T);
                    //Debug.WriteLine("     TOpposite@Bauteil" + j + " = " + TOpposite);
                    //Debug.WriteLine("     Area(" + j + ")=" + physics.Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Area);
                    //Debug.WriteLine("     "+ physics.Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Area + " " + physics.Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].Material.Surface.EpsilonL + " " + TOpposite);
                    //Debug.WriteLine("     " + physics.Walls[5 - wall.ID].Bauteil[j][layersInBauteil - 1].SubLayersT[subLayerCount - 1].ToString());
                }
            }
            rL_HWOpposite /= totalArea;
            rL_HWOpposite *= 1;// 1...Innenseite des Fahrzeuges als Schwarzkörper behandeln, ansonsten: Material.Surface.EpsilonL; - bis sich innen Gleichgewicht ausbildet.

            //Schließlich ermitteln wir noch, falls wir uns auf Wand 1, 2, 3, 4 oder 6 befinden, ob direkte Sonnenstrahlung irgendwo durchtritt. 1,3,4,6 können dabei analog behandelt werden.
            //Voraussetzung, dass etwas durchtritt: Eta(Gegenüberliegende Flächennormale, Sonne) < 90°.
            //Dabei wd. nur gegenüberliegende Flächen berücksichtigt und nicht, dass auch etwas durch 1 durchtretendes auf 3 und 4 auftreffen könnte.
            //Boden
            double area;
            double tau;
            double rS_Tau;

     
            if (wall.ID==1)
            {
                area = 0;
                dimA = 0;
                dimB = 0;
                tau = 0;
                double psiSun = psi(time + tOffsetToMeteorology);

                //Debug.WriteLine("Wall 1: " + (Math.Abs(psi(time + tOffsetToMeteorology) - Walls[0].Alpha)));
                //Debug.WriteLine("Wall 3: " + (Math.Abs(psi(time + tOffsetToMeteorology) - Walls[2].Alpha)));
                //Debug.WriteLine("Wall 4: " + (Math.Abs(psi(time + tOffsetToMeteorology) - Walls[3].Alpha)));
                //Debug.WriteLine("Wall 6: " + (Math.Abs(psi(time + tOffsetToMeteorology) - Walls[5].Alpha)));
                //Debug.WriteLine("limit1 :" + (r2d * Math.Atan(compartment.W / compartment.L)) + " limit2 :" + (r2d * Math.Atan(compartment.L / compartment.W)));

                if ((Math.Abs(psiSun-Walls[0].Alpha)) < (r2d * Math.Atan(compartment.W / compartment.L)))
                {
                    area = Walls[0].getArea(2);
                    dimA = compartment.L;
                    dimB = compartment.W;
                    aF = Walls[5].getArea(2);
                    tau = Walls[0].Bauteil[2][0].Material.Surface.TauS;
                    wallTauFloorID = Walls[0].ID;
                    //Debug.WriteLine("transmission from wall 1");
                }
                else if ((Math.Abs(psiSun - Walls[2].Alpha)) < (r2d * Math.Atan(compartment.L / compartment.W)))
                {
                    area = Walls[2].getArea(2);
                    dimA = compartment.W;
                    dimB = compartment.L;
                    aF = Walls[3].getArea(2);
                    tau = Walls[2].Bauteil[2][0].Material.Surface.TauS;
                    wallTauFloorID = Walls[2].ID;
                    //Debug.WriteLine("transmission from wall 3");
                }
                else if ((Math.Abs(psiSun - Walls[3].Alpha)) < (r2d * Math.Atan(compartment.L / compartment.W)))
                {
                    area = Walls[3].getArea(2);
                    dimA = compartment.W;
                    dimB = compartment.L;
                    aF = Walls[2].getArea(2);
                    tau = Walls[3].Bauteil[2][0].Material.Surface.TauS;
                    wallTauFloorID = Walls[3].ID;
                    //Debug.WriteLine("transmission from wall 4");
                }
                else if ((Math.Abs(psiSun - Walls[5].Alpha)) < (r2d * Math.Atan(compartment.W / compartment.L)))
                {
                    area = Walls[5].getArea(2);
                    dimA = compartment.L;
                    dimB = compartment.W;
                    aF = Walls[0].getArea(2);
                    tau = Walls[5].Bauteil[2][0].Material.Surface.TauS;
                    wallTauFloorID = Walls[5].ID;
                    //Debug.WriteLine("transmission from wall 6");
                }

                double fractionOfTotalArea = area / totalArea;

                double hF = compartment.H * fractionOfTotalArea;
                double hW = compartment.H - hF;

                double g = gamma(time + tOffsetToMeteorology);

                double x1 = hW / Math.Tan(d2r * g);
                double x2 = (hW+ hF) / Math.Tan(d2r * g);

                dx = 0;

                if((x2<= dimA) &&(x1<=dimA))
                {
                    dx = x2 - x1;
                }
                else if((x2 > dimA) && (x1< dimA))
                {
                    dx = dimA - x1;
                }

                
                double etaIncident = Eta(time, layer.Alpha, layer.Beta); //Einfallswinkel d. direkten Strahlung auf momentane Fläche

                rS_Tau = tau * (D0(time) + H0(time));
                rS_TauF = rS_Tau;

                rS_Tau_EffectiveF = Math.Sin(d2r * g) * layer.Material.Surface.AlphaS * rS_Tau * dx * dimB; //hier sinus weil boden horizontal orientiert, dh. bei gamma=0 (sonne am horizont) wird nichts absorbiert
            }

            //Flächen 1,3,4 und 6
            area = Walls[5 - wall.ID].getArea(2);
            if (area > 0)  //Hat gegenüberliegende Wand Fenster?
            {
                if ((wall.ID == 0) || (wall.ID == 2) || (wall.ID == 3) || (wall.ID == 5))
                {
                    dimA = 0;
                    double dimC = 0;
                    double psiSun = psi(time + tOffsetToMeteorology);

                    dimB = this.compartment.H;
                    if ((wall.ID == 0) || (wall.ID == 5))
                    {
                        dimA = this.compartment.L;
                        dimC = this.compartment.W;
                    }
                    else if ((wall.ID == 2) || (wall.ID == 3))
                    {
                        dimA = this.compartment.W;
                        dimC = this.compartment.L;
                    }

                    double hF = 0;
                    double hW =0;

                    double g = 0;

                    double ySun = 0;
                    double yIrradiated = 0;

                    if (Math.Abs(psiSun-Walls[5 - wall.ID].Alpha) < (r2d*Math.Atan(dimC/dimA))) //Einfallswinkel der Sonne noch in Bereich der berücksichtigt wird.
                    {
                        double fractionOfTotalArea = area / totalArea;

                        wallTauOppositeID = Walls[5 - wall.ID].ID;

                        //Extract relevant dimension
                        //wall.ID == 1 => l,h
                        //wall.ID == 3 => b,h
                        //wall.ID == 4 => b,h
                        //wall.ID == 6 => l,h

                        aW = Walls[5 - wall.ID].getArea(2);

                        hF = dimB * fractionOfTotalArea;
                        hW = dimB - hF;

                        g = gamma(time + tOffsetToMeteorology);

                        ySun = 0;
                        yIrradiated = 0;

                        if ((g > 0) && (g < 90))
                        {
                            ySun = dimA * Math.Tan(d2r * g);
                            if ((ySun > hF) && (ySun > 0))
                            {
                                yIrradiated = hF;
                            }
                            else if ((ySun <= hF) && (ySun > 0))
                            {
                                yIrradiated = ySun;
                            }
                            else if ((ySun <= hW + hF) && (ySun + hF > hW + hF))
                            {
                                yIrradiated = (hW + hF) - ySun;
                            }

                            tau = Walls[5 - wall.ID].Bauteil[2][0].Material.Surface.TauS;
                            double etaIncident = Eta(time, layer.Alpha, layer.Beta); //Einfallswinkel d. direkten Strahlung auf momentane Fläche

                            dy = yIrradiated;

                            rS_Tau = tau * (D0(time)+H0(time));
                            rS_TauW = rS_Tau;

                            rS_Tau_EffectiveW = Math.Cos(d2r * g) * layer.Material.Surface.AlphaS * rS_Tau * yIrradiated * dimC;


                        }

                        if ((time >= 0.59) && (time <= 0.70))
                        {
                            if ((wall.ID == 0) && (layer.Material.Surface.AlphaS>0.3))
                            {
                                Debug.WriteLine("Before if");
                                Debug.WriteLine("gamma " + g + "\t psi " + psiSun + "\t dimA " + dimA + "\t dimB " + dimB + "\t dimC " + dimC);
                                Debug.WriteLine("hF " + hF + "\t hW " + hW + "\t ySun " + ySun + "\t alpha " + Walls[5 - wall.ID].Alpha);
                                Debug.WriteLine("diff " + Math.Abs(psiSun - Walls[5 - wall.ID].Alpha) + "\t  minAngle " + (r2d * Math.Atan(dimC / 2 / dimA / 2)));
                                Debug.WriteLine("smaller? " + (Math.Abs(psiSun - Walls[5 - wall.ID].Alpha) < (r2d * Math.Atan(dimC / 2 / dimA / 2))));
                                Debug.WriteLine("rS_Tau " + rS_TauW + "\t rS_Tau_EffectiveW " + rS_Tau_EffectiveW);


                            }
                        }

                    }
                    if ((time >= 0.59) && (time <= 0.70)) {
                        if ((wall.ID == 0) && (layer.Material.Surface.AlphaS > 0.3))
                        {
                            Debug.WriteLine("After if");
                            Debug.WriteLine("gamma " + g + "\t psi " + psiSun + "\t dimA " + dimA + "\t dimB " + dimB + "\t dimC " + dimC);
                            Debug.WriteLine("hF " + hF + "\t hW " + hW + "\t ySun " + ySun + "\t alpha " + Walls[5 - wall.ID].Alpha);
                            Debug.WriteLine("diff " + Math.Abs(psiSun - Walls[5 - wall.ID].Alpha) + "\t  minAngle " + (r2d * Math.Atan(dimC / 2 / dimA / 2)));
                            Debug.WriteLine("smaller? " + (Math.Abs(psiSun - Walls[5 - wall.ID].Alpha) < (r2d * Math.Atan(dimC / 2 / dimA / 2))));
                            Debug.WriteLine("rS_Tau " + rS_TauW + "\t rS_Tau_EffectiveW " + rS_Tau_EffectiveW);

                        }
                    }
                }
            }
        }




    }
}
