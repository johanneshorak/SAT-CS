using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Diagnostics;

namespace Icov
{
    class Layer
    {
        public Layer(Log log)
        {
            this.log = log;
        }

        public Layer(Material m, double staerke, Log log)
        {
            Material = m;
            D = staerke;
            this.log = log;
        }

        Log log;

        //Die Schichten die sich aus der räumlichen Diskretisierung deltaX ergeben.
        //In den Datenserien werden die Wärmekapazitäten zu jedem Zeitschritt gespeichert.
        private DataPoints[] subLayers;
        public DataPoints[] SubLayers
        {
            get { return subLayers; }
            set { subLayers = value; }
        }

        private DataPoints[] subLayersT;
        public DataPoints[] SubLayersT
        {
            get { return subLayersT; }
            set { subLayersT = value; }
        }

        public double Volumen
        {
           get { return (Area * DinM); }
        }

        public double Masse
        {
            get { return (Volumen * Material.Rho); }
        }
        
        private double area = 0;
        public double Area
        {
            get { return area; }
            set { area = value; }
        }

        private double q = 0; //Wärmekapazität
        public double Q
        {
            get { return (q); }
            set { q = value; }
        }

        private double d = 0; //Dicke in mm
        public double D
        {
            get { return (d); }
            set { d = value; }
        }

        public double DinM
        {
            get { return (1E-3 * D); }
        }

        private double alpha = 0;
        public double Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }

        private double beta = 0;
        public double Beta
        {
            get { return beta; }
            set { beta = value; }
        }

        private Material material = null;
        public Material Material
        {
            get { return (material); }
            set
            {
                material = value;
            }
        }

        int steps = 1;
        public int Steps
        {
            get { return (steps); }
            set
            {
                steps = value;
            }
        }

        Boolean aussen = false;

        public Boolean Aussen
        {
            get { return aussen; }
            set { aussen = value; }
        }

        Boolean innen = false;

        public Boolean Innen
        {
            get { return innen; }
            set { innen = value; }
        }

        public double ConversionFactorJ2K
        {
            get { return (1 / (area * (DinM / steps) * material.Cv * material.Rho)); }
        }

        public double ConversionFactorK2J
        {
            get { return ( (area * (DinM / steps) * material.Cv * material.Rho)); }
        }

        private List<StreamWriter> heatCurrentLogFile = new List<StreamWriter>();
        public List<StreamWriter> HeatCurrentLogfile
        {
            get { return heatCurrentLogFile; }
            set { heatCurrentLogFile = value; }
        }

        // Diskretisierungsschrittweite entspricht Ausdehnung der linearen Diskretisierungsschritte
        double deltaX = 0;
        double DeltaXinM
        {
            get { return (1E-3*deltaX); }
        }

        public void initialize(double deltax, double T0)
        {
            Q = 0.001*D * Area * Material.Rho * Material.Cv * T0;
            deltaX = deltax;

            //Diskretisierungselemente erstellen und Schrittweite mit Dicke des Layers vergleichen.
            //Die Dicke muss ein ganzzahliges Vielfaches der Schrittweite sein.
            double stepsReal = D / deltaX;
            if ((stepsReal - Math.Round(stepsReal, 0)) > 0)
            {
                log.add(new LogEntry("Layer.initialize()", "Layerdicke ist kein ganzzahliges vielfaches der Diskretisierungsschrittweite! " + ToString(), Log.LOG_ERROR_TYPE));
            }

            steps = (int)Math.Round(D / deltaX, 0);

            if (steps == 0)
            {
                log.add(new LogEntry("Layer.initialize()", "Diskretisierungsschrittweite größer als Layerdicke! " + ToString(), Log.LOG_ERROR_TYPE));
                steps = 1;
            }


            SubLayers = new DataPoints[steps];
            SubLayersT = new DataPoints[steps];

            for (int i = 0; i < SubLayers.Length; i++)
            {

                SubLayers[i] = new DataPoints(log);
                SubLayers[i].addPoints(0, Q / steps);
                SubLayers[i].series.ChartType = SeriesChartType.Line;
                //SubLayers[i].series.MarkerStyle = MarkerStyle.Circle;

                SubLayersT[i] = new DataPoints(log);
                SubLayersT[i].addPoints(0, (Q / steps) * ConversionFactorJ2K);
                SubLayersT[i].series.ChartType = SeriesChartType.Line;
                SubLayersT[i].series.Name = "T(d=" + (i * deltaX)+"mm)";
                SubLayersT[i].series.Color = System.Drawing.Color.FromArgb(255, 128, (int)(i*255 / SubLayers.Length));
                //SubLayersT[i].series.MarkerStyle = MarkerStyle.Circle;

            }

        }

        public String ToString()
        {
            String name = "";
            if (Material != null)
            {
                if (SubLayers != null)
                {
                    name = Material.Name + " (" + D + "mm;" + Area + "m²;" + Volumen + "m³" + Material.Lambda + "W/Km;" + Material.Cv + "J/kgK;" + Material.Rho + "kg/m³;" + Masse + "kg;alphaS="+Material.Surface.AlphaS+ ";tauS=" + Material.Surface.TauS + ";EpsilonL=" + Material.Surface.EpsilonL+") Elemente " + SubLayers.Length;
                }
                else
                {
                    name = Material.Name + " (" + D + "mm;" + Material.Lambda + "W/Km;" + Material.Cv + "J/kgK;" + Masse + "kg) keine Elemente definiert (SubLayers.Length)";
                }
            }
            else
            {
                name = "kein Layer definiert (Material == null)";
            }

            return (name);
        }

        public void heatFluxCalculation(double time, Layer prv, Layer nxt, Wall wall, Physics physics)
        {
            bool debugOut = true;

            if (wall.ID == 0) { debugOut = true; }

            debugOut = false;

            double dQ;

            double kInside,kOutside;
            double c;
            double cL = 0;
            double cR = 0;
            double rL;
            double rS;

            double QAir;
            double QSubLayer;
            double T;

            double TL;
            double TR;
            double TAir;
            double TA;
            double TOpposite=0;

            double t0 = physics.previousTimeStep(time) / 3600; 

            double eta = physics.Eta(t0, Alpha, Beta); ;


            double rS_D = 0;
            double rS_TauEffF = 0;
            double rS_H = 0;
            double rS_R = 0;
            double rS_A = 0;
            double rS_TauEffW = 0;
            double rS_tauW = 0;
            double rS_tauF = 0;
            double tauWArea = 0;
            double tauFArea = 0;

            double dx = 0;
            double dy = 0;
            int wallFloorID = -1;
            int wallOppositeID = -1;

            double rL_HA = 0;
            double rL_HW = 0;
            double rL_HWSO = 0; // Für Glas - Abstrahlung nach außen braucht eigene Variable da innen Näherung als Schwarzkörper, außen aber nicht.

            double rL_HWOpposite = 0;

            double lambda1=0;
            double lambda2=0;
            double aK=0;

            double alphaL=0;


            double dQEffective;
            double dQAirEffective;

            double oppositeTime = 0;

            if (SubLayers == null)
            {
                log.add(new LogEntry("Layer.heatFluxCalculation()", "Keine Sublayer gefunden! SubLayers == null bei " + this.ToString(), Log.LOG_ERROR_TYPE));
            }
            else
            {
                QAir = physics.Cabin.Q.LastY;
                TAir = QAir * physics.Cabin.ConversionFactorJ2K;

                for (int i = 0; i < SubLayers.Length; i++) //Alle räumlichen Diskretisierungselemente des Layers durchlaufen und Wärmeströme berechnen
                {
                    //Debug.WriteLine("      layer.heatFluxCalculation(" + time + ") sublayer no. " + i);
                    dQ = 0;
                    dQEffective = 0;
                    dQAirEffective = 0;

                    kInside = 0;
                    kOutside = 0;
                    c = 0;
                    rL = 0;
                    rS = 0;

                    
                    QSubLayer = SubLayers[i].LastY;
                    T = QSubLayer * ConversionFactorJ2K;

                    TL = 0;
                    TR = 0;
                    
                    if (debugOut) Debug.WriteLine("   -" + this.ToString());
                    if (debugOut) Debug.WriteLine("     T = " + T);

                    //====================================================================================================================================================================================
                    //= Außen
                    //====================================================================================================================================================================================
                    //Noch nötig: Außen && Innen && i==0 - wenns zB. nur eine Blechschicht gibt.
                    if (Aussen && (i >= 0) && !Innen)
                    {
                        
                        rS_D = 0;
                        rS_H = 0;
                        rS_R = 0;
                        rS_A = 0;
                        rL_HA = 0;
                        rL_HW = 0;
                        TA = 0;



                        //if (Math.Cos(eta) > 0) rS_D = Math.Cos(eta) * physics.D0(time);

                        //heatCurrentsOfOuterLayer(double time, double Alpha, double Beta, double T, ref double rS_D, ref double rS_H, ref double rS_R, ref double rS_Am, ref double rL_HA, ref double TA, ref double kOutside)

                        

                        //Am Boden des Fahrzeuges trifft keine Diffuse Strahlung ein



                        //Debug.WriteLine("---------------- i=" + i+"-----------------------");
                        //Debug.WriteLine(" T=" + T);

                        if (i==0)
                        {
                            physics.heatCurrentsOfOuterLayer(time, wall, this, T, ref rS_D, ref rS_H, ref rS_R, ref rL_HA, ref rL_HW, ref TA, ref kOutside);
                            rS = rS_H + rS_R + rS_D;
                            rL = Material.Surface.AlphaL * rL_HA + rL_HW;

                        }

                        if (i == this.SubLayersT.Length - 1) //Achtung - ist dies der letzte Sublayer im aktuellen Layer? Wenn ja ists Zeit Kontaktübergangskoeffizienten zu berechnen
                        {
                            if (i == 0)
                            {
                                if (nxt != null)// Da Außen aber nicht Innen wissen wir eigentlich, dass es einen nächsten Layer gibt. Vorsichtshalber prüfen wir aber dennoch ob null übergeben wurde.
                                {
                                    // Relevant für die Wärmeleitung zwischen Materialschichten ist die Temperatur des ersten Sublayers.
                                    // Hier müsste eigentlich ein Wärmeübergangskoeffizient für den Kontakt zwischen zwei Materialien verwendet werden.
                                    lambda1 = Material.Lambda;
                                    lambda2 = nxt.Material.Lambda;

                                    aK = physics.getThermischenUebergangskoeffizienten(lambda1, lambda2);//Physics.C_AK;

                                    TR = nxt.SubLayersT.ElementAt(0).LastY;//getInterpolatedValueAt(t0);
                                    c = -aK * (T - TR);


                                    //Debug.WriteLine("     TR = " + TR + " c = " + c + " aK = " + aK);
                                }
                            }
                            else
                            {
                                lambda1 = Material.Lambda;
                                lambda2 = nxt.Material.Lambda;




                                aK = physics.getThermischenUebergangskoeffizienten(lambda1, lambda2);//Physics.C_AK;

                                TL = SubLayersT[i - 1].SecondToLastY;
                                TR = nxt.SubLayersT.ElementAt(0).LastY;//getInterpolatedValueAt(t0);
                                cL = (Material.Lambda / DeltaXinM) * (TL - T);
                                c = cL-aK * (T - TR);

                                //Debug.WriteLine("     Lambda = " + Material.Lambda + " dX = " + DeltaXinM + " lambda/deltaXinM = " + (Material.Lambda / DeltaXinM));
                                //Debug.WriteLine("     T = "+T+" TL = " + TL + " TR = " + TR + " (TL-T)="+(TL- T) +" cL = "+cL+" cR = "+ (aK * (T - TR))+" c = " + c + " aK = " + aK);
                            }

                        }
                        else //Wenn nicht berechenn wir die Wärmeleitung. Achtung - ist bei i==0 anders als bei i>0
                        {
                            if (i == 0)
                            {
                                TR = SubLayersT[i + 1].LastY;

                                cR = (Material.Lambda / DeltaXinM) * (T - TR);
                                c = - cR;

                                //Debug.WriteLine("     Lambda = " + Material.Lambda + " dX = " + DeltaXinM);
                                //Debug.WriteLine("     TL = " + TL + " TR = " + TR + " cL = " + cL + " cR = " + cR + " c = " + c);
                            }
                            else
                            {
                                // Heißt wir sind irgendwo inmitten der Sublayer der aktuellen Materialschicht
                                TL = SubLayersT[i - 1].SecondToLastY;// getInterpolatedValueAt(time - physics.Deltat);
                                TR = SubLayersT[i + 1].LastY;

                                cL = (Material.Lambda / DeltaXinM) * (TL - T);
                                cR = (Material.Lambda / DeltaXinM) * (T - TR);
                                c = cL - cR;
                                //Debug.WriteLine("     TL = " + TL + " TR = " + TR + " cL = " + cL + " cR = " + cR + " c = " + c);
                                
                            }
                        }

                        dQ += rL + Material.Surface.AlphaS * rS + c + kOutside;


                        

                        if (debugOut) Debug.WriteLine("     Außen und i=1");
                        if (debugOut) Debug.WriteLine("     alpha = " + this.Alpha + " beta = " + this.Beta);
                        if (debugOut) Debug.WriteLine("     etaSun = " + physics.Eta(time, Alpha, Beta));
                        //Debug.WriteLine("     AlphaL = " + Material.Surface.AlphaL + " AlphaS = " + Material.Surface.AlphaS + " Area = " + Area);
                        //Debug.WriteLine("     rS_D = " + rS_D + " rS_H = " + rS_H + " rS_R = " + rS_R + " rS = " + rS);
                        //Debug.WriteLine("     rL_HA = " + rL_HA + " rL_HW = " + rL_HW + " rL = " + rL);
                        //Debug.WriteLine("     TA = " + TA + " kOutside = " + kOutside);
                        //Debug.WriteLine("  dQ" + i + "=" + dQ);
                        if (log.resultEnvironmentHeatCurrentsFile[wall.ID]!=null)
                        {
                            log.resultEnvironmentHeatCurrentsFile[wall.ID].WriteLine(
                                time + Utilities.C_SEPARATOR +
                                Alpha + Utilities.C_SEPARATOR +
                                Beta + Utilities.C_SEPARATOR +
                                physics.psi(time + physics.tOffsetToMeteorology) + Utilities.C_SEPARATOR +
                                physics.gamma(time + physics.tOffsetToMeteorology) + Utilities.C_SEPARATOR +
                                physics.Eta(time, Alpha, Beta) + Utilities.C_SEPARATOR +
                                rS_D + Utilities.C_SEPARATOR +
                                rS_H + Utilities.C_SEPARATOR +
                                rS_R + Utilities.C_SEPARATOR +
                                rL_HA + Utilities.C_SEPARATOR +
                                rL_HW + Utilities.C_SEPARATOR +
                                TA + Utilities.C_SEPARATOR +
                                kOutside + Utilities.C_SEPARATOR +
                                TR + Utilities.C_SEPARATOR +
                                c + Utilities.C_SEPARATOR +
                                aK + Utilities.C_SEPARATOR +
                                physics.KappaSkyTable.getInterpolatedValueAt(TA + Physics.C_T0) + Utilities.C_SEPARATOR
                                );
                        }
                    }
                    //====================================================================================================================================================================================
                    //= Innen
                    //====================================================================================================================================================================================
                    else if ((Innen) && (i == SubLayers.Length - 1) && !Aussen)
                    {
                        //Debug.WriteLine("     Innen i=" + (i + 1));
                        //Heißt wir haben den letzten Sublayer UND er grenzt an die Luft in der Fahrzeugkabine an.
                        rS_D = 0;

                        TL = SubLayersT[i - 1].SecondToLastY;//getInterpolatedValueAt(time - physics.Deltat);
                        cL = (Material.Lambda / DeltaXinM) * (TL - T);
                        c = cL;

                        physics.heatCurrentsOfInnerLayer(time, wall, this, T, ref TAir, ref alphaL, ref kInside, ref rS_TauEffF, ref rS_TauEffW, ref rS_tauF, ref rS_tauW, ref tauFArea, ref tauWArea, ref dx, ref dy, ref wallFloorID, ref wallOppositeID, ref rL_HW, ref oppositeTime, ref TOpposite, ref rL_HWOpposite);


                        //Hier muss auch berücksichtigt werden, ob direkte Solare Strahlung durch ein Fenster auf den gegenüberliegenden Seiten eindringen kann.

                        if (debugOut) Debug.WriteLine("     rL_HW = " +rL_HW+ " rL_HWOpposite " + rL_HWOpposite);

                        //Debug.WriteLine("     rL_HW = " + rL_HW + " rL_HWOpposite " + rL_HWOpposite);

                        dQ += -kInside + c + rL_HW + rL_HWOpposite;

                        //dQAirEffective und das hinzufügen neuer Datenpunkte darf nicht hier erfolgen da ansonsten
                        //zu jedem Zeitschritt mehrere Einträge in den TAir und QAir Tabellen vorhanden wären. Es gibt immerhin
                        //6 Flächen von denen aus Wärme an die Luft übertragen wird.
                        //Daher wird dieser Schritt nun am Ende des Zeitschrittes durchgeführt, allerdings summieren wir
                        //während eines Zeitschrittes alle kInside auf damit wir nichts verlieren.
                        //    dQAirEffective = kInside * Area * physics.DeltatSeconds;
                        //    physics.Cabin.Q.addPoints(time, QAir + dQAirEffective);
                        //    physics.Cabin.T.addPoints(time, (QAir + dQAirEffective) * physics.Cabin.ConversionFactorJ2K);
                        physics.DQAirEffective += kInside * Area;

                        if (debugOut) Debug.WriteLine("     TAir = " + TAir + " kInside = " + kInside);
                        if (debugOut) Debug.WriteLine("     TL = " + TL + " c = " + c);
                        if (debugOut) Debug.WriteLine("     dQ = " + dQ);
                    }
                    //====================================================================================================================================================================================
                    //= Mitte
                    //====================================================================================================================================================================================
                    else if (!(Aussen && (i == 0)) && !((Innen) && (i == SubLayers.Length - 1)))
                    {
                        //Debug.WriteLine("     Mitte und i="+(i+1));

                        if (i == 0) 
                        {
                            // Heißt wir haben den ersten Sublayer der aktuellen Materialschicht aber diese liegt hinter einer anderen Materialschicht.


                            // THERMISCHER KONTAKTWIDERSTAND - FALLS MODELLIERBAR MUSS DIES AUF JEDEN FALL HIER BERÜCKSICHTIGT WERDEN.
                            // Im Moment ist dies ausreichend, keine Simulation benötigt zur Zeit mehr als zwei aneinander grenzenden Materialschichten.

                            //Berechnung des thermischen Übergangskoeffizienten


                            //lambda1 = prv.Material.Lambda;
                            //lambda2 = Material.Lambda;
                            aK = Physics.C_AK;//;physics.getThermischenUebergangskoeffizienten(lambda1, lambda2);

                            TL = prv.SubLayersT.ElementAt(prv.SubLayersT.Length - 1).SecondToLastY;// getInterpolatedValueAt(time - physics.Deltat);
                            TR = SubLayersT[i + 1].LastY;

                            cL = aK * (TL - T);
                            cR = (Material.Lambda / DeltaXinM) * (T - TR);
                            c =  cL - cR ;
                            //Debug.WriteLine("     Lambda = " + Material.Lambda + " dX = "+DeltaXinM);
                            //Debug.WriteLine("     TL = " + TL + " TR = " + TR + " cL = " + cL + " cR = " + cR + " c = " + c);
                        }
                        else        
                        {
                            // Heißt wir sind irgendwo inmitten der Sublayer der aktuellen Materialschicht
                            TL = SubLayersT[i - 1].SecondToLastY;// getInterpolatedValueAt(time - physics.Deltat);
                            TR = SubLayersT[i + 1].LastY;

                            cL = (Material.Lambda / DeltaXinM) * (TL - T);
                            cR = (Material.Lambda / DeltaXinM) * (T - TR);
                            c = cL - cR;
                            if (debugOut) Debug.WriteLine("     TL = " + TL + " TR = " + TR + " cL = "+cL+" cR = "+cR+" c = " + c);
                        }
                        dQ += c;
                    }
                    //====================================================================================================================================================================================
                    //= Aussen und Innen - also nur ein Layer der nur in einen Sublayer beinhaltet
                    //====================================================================================================================================================================================
                    else if ((Aussen && (i == 0)) && ((Innen) && (i == SubLayers.Length - 1)))
                    {
                        //Debug.WriteLine("Found a Glass Layer...");
                        //Debug.WriteLine("Name: "+this.ToString());

                        rS_D = 0;
                        rS_H = 0;
                        rS_R = 0;
                        rS_A = 0;
                        rL_HA = 0;
                        rL_HW = 0;
                        TA = 0;

                        physics.heatCurrentsOfOuterLayer(time, wall, this, T, ref rS_D, ref rS_H, ref rS_R, ref rL_HA, ref rL_HWSO, ref TA, ref kOutside);
                        physics.heatCurrentsOfInnerLayer(time, wall, this, T, ref TAir, ref alphaL, ref kInside, ref rS_TauEffF, ref rS_TauEffW, ref rS_tauF, ref rS_tauW, ref tauFArea, ref tauWArea, ref dx, ref dy, ref wallFloorID, ref wallOppositeID, ref rL_HW, ref oppositeTime, ref TOpposite, ref rL_HWOpposite);

                        rS = rS_H + rS_R + rS_D;

                        dQ += -kInside + rL_HWSO + rL_HW + rL_HWOpposite + Material.Surface.AlphaL * rL_HA; //Im Prinzip zwei mal L_HW weil eine so dünne Schicht auf beiden Seiten gleichermaßen emittiert.
                        dQ += Material.Surface.AlphaS * rS  + kOutside;

                        //Debug.WriteLine("rS_D " + rS_D + " rS_R " + rS_R + " rS_H " + rS_H);
                        //Debug.WriteLine("rL_HA "+ rL_HA + " rL_HWSO " + rL_HWSO + " rL_HW " + rL_HW + " rL_Opp "+rL_HWOpposite);
                        //Debug.WriteLine("kOutside "+ kOutside + " kInside "+ kInside);
                        //Debug.WriteLine("dQ " + dQ);
                    }


                    dQEffective = dQ * Area * physics.DeltatSeconds + (rS_TauEffW + rS_TauEffF) * physics.DeltatSeconds;
                    
                    SubLayers[i].addPoints(time, QSubLayer + dQEffective);
                    SubLayersT[i].addPoints(time, (QSubLayer + dQEffective) * ConversionFactorJ2K);

                    //Falls die Wärmeströme gelogged werden sollen werden diese hier in eine Datei geschrieben

                    if (HeatCurrentLogfile[i] != null)
                    {
                        HeatCurrentLogfile[i].WriteLine(
                            time + Utilities.C_SEPARATOR +
                            Area + Utilities.C_SEPARATOR +
                            SubLayersT[i].LastY + Utilities.C_SEPARATOR +
                            (SubLayersT[i].LastY - T) + Utilities.C_SEPARATOR +
                            c + Utilities.C_SEPARATOR +
                            cL + Utilities.C_SEPARATOR +
                            cR + Utilities.C_SEPARATOR +
                            kInside + Utilities.C_SEPARATOR +
                            rL_HWOpposite + Utilities.C_SEPARATOR +
                            rL_HW + Utilities.C_SEPARATOR +
                            TOpposite + Utilities.C_SEPARATOR +
                            oppositeTime + Utilities.C_SEPARATOR +
                            wall.ID + Utilities.C_SEPARATOR +
                            physics.Walls[5 - wall.ID].ID + Utilities.C_SEPARATOR +
                            rS_TauEffF + Utilities.C_SEPARATOR +
                            rS_TauEffW + Utilities.C_SEPARATOR +
                            dx + Utilities.C_SEPARATOR +
                            dy + Utilities.C_SEPARATOR +
                            wallFloorID + Utilities.C_SEPARATOR +
                            wallOppositeID + Utilities.C_SEPARATOR +
                            kOutside + Utilities.C_SEPARATOR +
                            rL_HWSO + Utilities.C_SEPARATOR + 
                            dQEffective + Utilities.C_SEPARATOR +
                            rS_tauW + Utilities.C_SEPARATOR +
                            rS_tauF + Utilities.C_SEPARATOR +
                            tauWArea + Utilities.C_SEPARATOR +
                            tauFArea + Utilities.C_SEPARATOR
                            );
                    }

                    //Debug.WriteLine("     sublayer "+i+" has "+SubLayers[i].Length+" datapoints");
                    //if (Aussen && (i == 0))
                    //{
                    if (debugOut) Debug.WriteLine("    Q = "+QSubLayer+" dQ = " + dQ);
                    if (debugOut) Debug.WriteLine("    dQEffective = " + dQEffective);
                    if (debugOut) Debug.WriteLine("    dQAirEffective = " + dQAirEffective + " dQEff/Q+" + (dQEffective / Q));
                    if (debugOut) Debug.WriteLine("    T = "+SubLayersT[i].LastY);
                    if (debugOut) Debug.WriteLine("    dT = " + (SubLayersT[i].LastY - T));
                    if (debugOut) Debug.WriteLine("    TAir = " + physics.Cabin.T.LastY);
                    if (debugOut) Debug.WriteLine("    dTAir = " +( physics.Cabin.T.LastY-TAir));

                    if (debugOut) Debug.WriteLine("    ");

                    //if(i==(SubLayersT.Length-1)) Debug.WriteLine("    TSubLayer"+i+" = " + SubLayersT[i].LastY);
                    //}
                }
            }
        }
    }
}
