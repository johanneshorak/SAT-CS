using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Icov
{
    class Wall
    {
        private static int BauteilAnzahl = 4;

        String[] BauteilNamen = { "A","B","C","D" };
        Log log;

        private int iD;
        public int ID
        {
            get
            {
                return iD;
            }

            set
            {
                iD = value;
            }
        }

        // areaWall
        // bezeichnet die Gesamtfläche der Wand. Da diese aus mehreren Bauteilen besteht
        // können die Bauteile Bruchteile dieser Fläche einnehmen.
        private double areaWall = 0;
        public double Area
        {
            get { return areaWall; }
            set { areaWall = value; }
        }

        String name = "";
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private double deltax;
        public double Deltax
        {
            get { return deltax;  }
            set { deltax = value;  }
        }

        bool active = false;
        public bool Active
        {
            get { return active;  }
            set { active = value; }
        }

        private double alpha;
        public double Alpha
        {
            get { return alpha;  }
            set { 
                alpha = value;

                foreach (List<Layer> layers in bauteil)
                {
                    foreach (Layer l in layers)
                    {
                        l.Alpha = alpha;
                    }
                }
            }
        }

        private double beta;
        public double Beta
        {
            get { return beta;  }
            set { beta = value;

                foreach (List<Layer> layers in bauteil)
                {
                    foreach (Layer l in layers)
                    {
                        l.Beta = beta;
                    }
                }
            
            }
        }

        /*
        Wall opposing;
        public Wall Opposing
        {
            get { return opposing;  }
            set { opposing = value; }
        }
        */
        List<double> area = new List<double>();
        List<List<Layer>> bauteil = new List<List<Layer>>();
        
        

        public List<List<Layer>> Bauteil
        {
            get { return bauteil;  }
            set { bauteil = value; }
        }

        public List<Layer> getLayers(int teil)
        {
            if (teil < BauteilAnzahl)
                return (bauteil[teil]);
            else
                return null;
        }

        public void addLayer(int teil,Layer layer)
        {
            if (teil < BauteilAnzahl)
            {
                int layerCount = bauteil[teil].Count;
                bauteil[teil].Add(layer);

                layer.Alpha = Alpha;
                layer.Beta = Beta;

                //Entscheiden ob neuer Layer auf außen oder innenseite der Wand
                if (layerCount == 0)
                {
                    bauteil[teil].ElementAt(layerCount).Aussen = true;
                    bauteil[teil].ElementAt(layerCount).Innen = true;
                }
                else if(layerCount > 0 )
                {
                    //Voriger Layer kann nicht mehr innen sein
                    bauteil[teil].ElementAt(layerCount-1).Innen = false;

                    //Aktueller Layer ist auf keinen Fall mehr außen aber vielleicht innen.
                    bauteil[teil].ElementAt(layerCount).Aussen = false;
                    bauteil[teil].ElementAt(layerCount).Innen = true;
                }                          
            }
        }

        // getArea(int)
        // Ermittelt die Fläche die ein bestimmtes Bauteil einnimmt. Diese muss
        // kleiner/gleich der Gesamtfläche areaWall sein!
        public double getArea(int teil)
        {
            return (area[teil]);
        }

        // getArea(int)
        // Setzt die Fläche die ein bestimmtes Bauteil einnimmt. Diese muss
        // kleiner/gleich der Gesamtfläche areaWall sein!
        public void setArea(int teil, double a)
        {
            if (teil < BauteilAnzahl)
            {
                area[teil] = a;

                foreach (Layer l in bauteil[teil])
                {
                    l.Area = a;
                }
            }
        }

        private void initBauteile()
        {
            for (int i = 0; i < BauteilAnzahl; i++)
            {
                bauteil.Add(new List<Layer>());
                area.Add(0);
            }
        }

        public Wall(Log log)
        {
            this.log = log;
            active = true;
            initBauteile();
        }

        public Wall(String n)
        {
            active = true;
            name = n;
            initBauteile();
        }


        public String ToString()
        {
            String txt = "";

            int i = 0;
            foreach(List<Layer> layers in bauteil)
            {
                int j = 0;
                if (area[i] > 0)
                {                   
                    txt += name+" "+Alpha+","+Beta+"_"+BauteilNamen[i]+":L"+(j+1)+"|" + area[i]+"m²[";
                    foreach (Layer l in layers)
                    {
                        if (l.Material != null)
                            txt += l.Material.Name + "(" + l.D + "mm;" + l.Material.Lambda + "W/Km;" + l.Material.Cv + "J/kgK) "+l.SubLayers.Length;
                    }
                    txt = txt + "]\r\n";
                }
                i++;
            }

            
            return(txt);
        }

        public void initialize(double deltax, double T0){

            int i = 0;
            foreach (List<Layer> layers in bauteil)
            {
                int j = 0;
                if (area[i] > 0)
                {
                    foreach (Layer l in layers)
                    {
                        if (l.Material != null)
                            l.initialize(deltax, T0);
                    }
                }
                i++;
            }
        }

        public void heatFluxCalculation(double time, Physics physics)
        {
            Layer layer, prvLayer, nxtLayer;

            for (int j = 0; j < this.Bauteil.Count; j++)
            {
                
                if (area[j] > 0)
                {
                    //Debug.WriteLine("  wall.heatFluxCalculation(" + time + ") bauteil " + this.BauteilNamen[j]);
                    //log.add(new LogEntry("startSimulation", "Berechne Bauteil " + this.BauteilNamen[j] + "...", Log.LOG_INFO_TYPE));

                    for (int k = 0; k < this.Bauteil[j].Count; k++)
                    {
                        
                        layer = this.Bauteil[j].ElementAt(k);
                        prvLayer = null;
                        nxtLayer = null;
                        //Debug.WriteLine("  wall.heatFluxCalculation(" + time + ") layer "+(k+1)+"/"+ this.Bauteil[j].Count);
                        //Der Layer für den der Wärmestrom berechnet wird muss auch seine Nachbarlayer kennen
                        if ((k>=1) && (layer.Aussen== false))     { prvLayer = this.Bauteil[j].ElementAt(k - 1); }
                        if (k<(this.Bauteil[j].Count-1)) { nxtLayer = this.Bauteil[j].ElementAt(k + 1); }
                        
                        //Debug.WriteLine("    wall.heatFluxCalculation(" + time + ") layer " + k + " " + layer.ToString());
                        layer.heatFluxCalculation(time, prvLayer, nxtLayer, this, physics);
                    }
                }
            }
        }

        public void fillVehicleTree(System.Windows.Forms.TreeView treeView,System.Windows.Forms.TreeNode rootNode)
        {
            String wallNodeName = "";

            wallNodeName = Name + ") " + this.areaWall + "m² α=" + Alpha + "° β=" + Beta;
            TreeNode wallNode = new TreeNode(wallNodeName);
            
            //Noden für Tree generieren. Zuerst für sämtliche zu einer Wand definierten Bauteile
            for (int i = 0; i < BauteilAnzahl; i++)
            {
                if (area[i] > 0)
                {
                    String BauteilNodeName = BauteilNamen[i]+": "+area[i]/areaWall;

                    TreeNode bauteilNode = new TreeNode(BauteilNodeName);

                    // Anschließend für alle Layer dieses Bauteils
                    foreach (Layer l in bauteil[i])
                    {
                        if (l.Material != null)
                        {
                            String layerName = l.ToString();

                            if ((l.Aussen) && (l.Innen)) layerName += " grenzfläche innen und außen";
                            if ((l.Aussen) && !(l.Innen)) layerName += " grenzfläche außen";
                            if (!(l.Aussen) && (l.Innen)) layerName += " grenzfläche innen";

                            TreeNode layerNode = new TreeNode(layerName);

                            if (l.SubLayers != null)
                            {
                                int j = 0;
                                foreach (DataPoints dp in l.SubLayersT)
                                {
                                    String dataPointNodeName = "";

                                    dataPointNodeName = (j + 1) + ". ("+Math.Round(dp.LastX,2)+"s, " + (Math.Round(dp.LastY,1) ) + "K) "+dp.Length+" points";

                                    TreeNode dataPointNode = new TreeNode(dataPointNodeName);

                                    layerNode.Nodes.Add(dataPointNode);
                                    j++;
                                }
                            }

                            bauteilNode.Nodes.Add(layerNode);
                        }
                    }


                    wallNode.Nodes.Add(bauteilNode);


                }
            }

            rootNode.Nodes.Add(wallNode);

            
            
            
        }

    }
}
