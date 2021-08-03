using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealTimeEventLogReader
{
    class LogReader
    {
        public delegate void AddRecordDelegate(EventLogRecord record);

        public AddRecordDelegate AddRecord;

        public int ReadInterval = 1000;

        public int LogLimit = 5000;

        public string LogName = string.Empty;

        private bool Stop = true;

        private Thread readerThread = null;
        private DateTime _lastReadTime = DateTime.UtcNow;
        private const string TimeFormatString = "yyyy-MM-ddTHH:mm:ss.fffffff00K";
        private const string EventReaderQuery = "*[System/TimeCreated/@SystemTime >='{0}']";

        public LogReader(string logName)
        {
            LogName = logName;
            StartReader();
       }

        public void StopReader()
        {
            if(readerThread != null && readerThread.IsAlive && !Stop)
            {
                Stop = true;
                readerThread = null;
            }
        }

        public void StartReader()
        {
            if (Stop)
            {
                Stop = false;
                if (readerThread != null)
                {
                    readerThread = null;
                }
                readerThread = new Thread(ReadLogs);
                readerThread.Start();
            }
        }


        private void ReadLogs()
        {
            while (!Stop)
            {
                // 1. Calculate elapsed time since previous read.
                double elapsedTimeSincePreviousRead = (DateTime.UtcNow - _lastReadTime).TotalSeconds;
                DateTime timeSpanToReadEvents = DateTime.UtcNow.AddSeconds(-elapsedTimeSincePreviousRead);
                string strTimeSpanToReadEvents = timeSpanToReadEvents.ToString(TimeFormatString, CultureInfo.InvariantCulture);
                string query = string.Format(EventReaderQuery, strTimeSpanToReadEvents);
                int readEventCount = 0;

                // 2. Create event log query using elapsed time.
                // 3. Read the record using EventLogReader.
                EventLogQuery eventsQuery = new EventLogQuery(LogName, PathType.LogName, query) { ReverseDirection = true };
                EventLogReader logReader = new EventLogReader(eventsQuery);

                // 4. Set lastReadTime to Date.Now
                _lastReadTime = DateTime.UtcNow;

                for (EventRecord eventdetail = logReader.ReadEvent(); eventdetail != null; eventdetail = logReader.ReadEvent())
                {
                    byte[] bytes = null;
                    //if (eventdetail.Properties.Count >= 2)
                    //{
                        
                    //    MessageBox.Show((string)eventdetail.Properties[eventdetail.Properties.Count - 1].Value);
                    //    //bytes = (byte[])eventdetail.Properties[eventdetail.Properties.Count-1].Value;
                    //}
                    if (eventdetail.Id == 4656 | eventdetail.Id == 4660 | eventdetail.Id == 4663 |  eventdetail.Id == 4658)
                    {
                        string description = eventdetail.FormatDescription();
                        string computernametemp = description.Substring(description.IndexOf("Account Domain:") + ("Account Domain:").Length + 2);
                        string computername = computernametemp.Substring(0, computernametemp.IndexOf("\r"));
                        string usernametemp = description.Substring(description.IndexOf("Account Name:") + ("Account Name:").Length + 2);
                        string username = usernametemp.Substring(0, usernametemp.IndexOf("\r"));
                        int totalobjecttype = description.IndexOf("Object Type:");
                        int totalobjectname = description.IndexOf("Object Name:");
                        int totalaccessmask = description.IndexOf("Access Mask:");
                        int totalaccesstype = description.IndexOf("Accesses:");
                        if (totalobjecttype > 0 && totalobjectname > 0 && totalaccessmask > 0 && totalaccesstype > 0)
                        {
                            string objecttypetemp = description.Substring(description.IndexOf("Object Type:") + ("Object Type:").Length + 2);
                            string objecttype = objecttypetemp.Substring(0, objecttypetemp.IndexOf("\r"));
                            string objectnametemp = description.Substring(description.IndexOf("Object Name:") + ("Object Name:").Length + 2);
                            string objectname = objectnametemp.Substring(0, objectnametemp.IndexOf("\r"));
                            string handleidtemp = description.Substring(description.IndexOf("Handle ID:") + ("Handle ID:").Length + 2);
                            string handleid = handleidtemp.Substring(0, handleidtemp.IndexOf("\r"));
                            string processnametemp = description.Substring(description.IndexOf("Process Name:") + ("Process Name:").Length + 2);
                            string processname = processnametemp.Substring(0, processnametemp.IndexOf("\r"));
                            string accessmasktemp = description.Substring(description.IndexOf("Access Mask:") + ("Access Mask:").Length + 2);
                            string accesstypetemp = description.Substring(description.IndexOf("Accesses:") + ("Accesses:").Length + 2);
                            string accessmask = "";
                            string accesstype = "";
                            if (accessmasktemp.IndexOf("\r") > 0)
                            {
                                 accessmask = accessmasktemp.Substring(0, accessmasktemp.IndexOf("\r"));
                            }
                            else
                            {
                                accessmask = accessmasktemp.Substring(0, accessmasktemp.Length);
                            }

                            if (accesstypetemp.IndexOf("\r") > 0)
                            {
                                accesstype = accesstypetemp.Substring(0, accesstypetemp.IndexOf("\r"));
                            }
                            else
                            {
                                accesstype = accesstypetemp.Substring(0, accesstypetemp.Length);
                            }

                            if (objecttype == "File" && ( objectname.Contains(".pptx") | objectname.Contains(".xlsx") | objectname.Contains(".xls") | objectname.Contains(".iso") | objectname.Contains(".pdf") | objectname.Contains(".ppt") | objectname.Contains(".doc") | objectname.Contains(".docx") | objectname.Contains(".zip") | objectname.Contains(".rar") ))
                            {
                                if( processname == @"C:\Windows\explorer.exe" && objectname.Contains("$RECYCLE.BIN")  == false )
                                {
                                    string namafile = objectname.Replace(":Zone.Identifier", "");
                                    string id = eventdetail.Id.ToString();
                                    string handleid_value = Convert.ToInt32(handleid, 16).ToString();
                                    string process_name = processname;
                                    string computer_name = computername;
                                    string tgl_proses = eventdetail.TimeCreated.Value.ToString();
                                    string access_mask = Convert.ToInt32(accessmask, 16).ToString();
                                    var info = new FileInfo(namafile);
                                    string hasil = DataProcess.InsertFileMonitoring(namafile, info.Name, computer_name, id, handleid_value, process_name, access_mask, username, tgl_proses, accesstype);
                                    EventLogRecord record = new EventLogRecord(eventdetail);
                                    PostDetail(record);
                                }
                                //   MessageBox.Show(username);
                               
                            }
                        }
                        
                        
                    }
                    //EventLogRecord record = new EventLogRecord(eventdetail);

                    // 5. Post record read using event log query.
                    //if (parser.IsValid(temporaryRecord))
                    //{
                        //PostDetail(record);
                    //}
                    // 6. Post only latest InternalLogLimit records, if result of event log query is more than InternalLogLimit.
                    if (++readEventCount >= LogLimit)
                    {
                        break;
                    }
                }
                Thread.Sleep(ReadInterval);
            }
          
        }

        private void PostDetail(params EventLogRecord[] records)
        {
            if (AddRecord != null && records != null)
            {
                //Return each record in the list to the viewer using AddRecord
                foreach (EventLogRecord record in records)
                {
                    if (record != null)
                    {
                        AddRecord.BeginInvoke((EventLogRecord)record.Clone(), null, null);
                    }
                }
            }
        }

    }
}
