using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace Icov
{
    public class Log
    {

        public static int LOG_INFO_TYPE = 0;
        public static int LOG_WARN_TYPE = 1;
        public static int LOG_ERROR_TYPE = 2;
        public static int LOG_DEBUG_TYPE = 50;
        public static int LOG_DEBUG_CALCULATION_DETAILED_INFO = 51;

        List<LogEntry> logEntries = new List<LogEntry>();
        TextBox tbLog;

        public static String resultOutputSimPath = "output";
        public String resultOutputBasePath;
        public String resultOutputPath;
        public static String resultTCabinFileName = "T_cabin.csv";
        public static String resultQCabinFileName = "Q_cabin.csv";
        public static String resultGeneralFileName = "general.csv";
        public static String logFileName = "log.txt";
        public static String resultEnvironmentHeatCurrentsFileName = "q_environment_$$.csv";
        public static String resultConductiveHeatCurrentsFileName = "q_conduction_$$.csv";
        public static String resultInsideHeatCurrentsFileName = "q_inside_w-$1_l-$2_sl-$3.csv";

        public List<StreamWriter> resultOutputStreamWriterList;
        public List<StreamWriter> resultEnvironmentHeatCurrentsFile;
        public List<StreamWriter> resultConductiveHeatCurrentsFile;
        public List<StreamWriter> resultInsideHeatCurrentsFile;


        System.IO.StreamWriter outputLogFile;
        public StreamWriter OutputLogFile
        {
            get { return outputLogFile; }
            set { outputLogFile = value; }
        }

        System.IO.StreamWriter outputGeneralFile;
        public StreamWriter OutputGeneralFile
        {
            get { return outputGeneralFile;  }
            set { outputGeneralFile = value; }
        }

        System.IO.StreamWriter outputTCabinFile;
        public StreamWriter OutputTCabinFile
        {
            get { return outputTCabinFile; }
            set { outputTCabinFile = value; }
        }

        System.IO.StreamWriter outputQCabinFile;
        public StreamWriter OutputQCabinFile
        {
            get { return outputQCabinFile; }
            set { outputQCabinFile = value; }
        }

        public void createOutPutPath()
        {
            Directory.CreateDirectory(resultOutputBasePath + "\\" + resultOutputSimPath);
            resultOutputPath = resultOutputBasePath + "\\" + resultOutputSimPath + "\\";
        }

        public Log()
        {
        }

        public Log(TextBox logBox)
        {
            tbLog = logBox;
         }

        public void clear()
        {
            tbLog.Text = String.Empty;
            tbLog.Invalidate();
        }

        public void add(LogEntry le)
        {
            logEntries.Add(le);
            if(OutputLogFile!=null)
            {
                OutputLogFile.WriteLine(le.dateTime.TimeOfDay.Hours + ":" + le.dateTime.TimeOfDay.Minutes + ":" + le.dateTime.TimeOfDay.Seconds + "[" + le.sender + "]>> " + le.message);
            }
            //tbLog.Text = tbLog.Text + "\r\n"+le.dateTime.TimeOfDay.Hours+":"+le.dateTime.TimeOfDay.Minutes+":"+le.dateTime.TimeOfDay.Seconds+"["+le.sender+"]>> "+le.message;
            //tbLog.Text = tbLog.Text + "\r\n\n" + le.message;
            //tbLog.Invalidate();

            //Debug.WriteLine(le.message);
        }

        public void add(String msg)
        {
            add(new LogEntry("", msg, LOG_DEBUG_TYPE));
        }

    }


    public class LogEntry
    {
        public DateTime dateTime;
        public String message;
        public String sender;
        public int type;


        public LogEntry()
        {
            dateTime = DateTime.Now;
            type = Log.LOG_INFO_TYPE;
        }

        public LogEntry(String s, String msg, int t)
        {
            sender = s;
            dateTime = DateTime.Now;
            message = msg;
            type = t;
        }

    }
}
