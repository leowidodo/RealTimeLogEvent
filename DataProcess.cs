using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBHelperForNet;

namespace RealTimeEventLogReader
{
   public class DataProcess
    {
        public static string InsertFileMonitoring(string pathfile, string namafile, string computer_name, string event_id, string handle_id, string Process_name, string Access_mask, string User, string tgl_proses, string access_type)
        {

            DBParams dbprocess = new DBParams();
            dbprocess.AddParameter("@PATH_FILE", pathfile);
            dbprocess.AddParameter("@NAMA_FILE", namafile);
            dbprocess.AddParameter("@COMPUTER_NAME", computer_name);
            dbprocess.AddParameter("@EVENT_ID", event_id);
            dbprocess.AddParameter("@HANDLE_ID", handle_id);
            dbprocess.AddParameter("@PROCESS_NAME", Process_name);
            dbprocess.AddParameter("@ACCESS_MASK", Access_mask);
            dbprocess.AddParameter("@USER", User);
            dbprocess.AddParameter("@TGL_PROSES", tgl_proses);
            dbprocess.AddParameter("@ACCESS_TYPE", access_type);

            // DBConn.Conn.InsertUpdateDeleteProcedure("INSERT_TR_FILE_MONITORING", dbprocess);

            int prhasil = DBConn.Conn.InsertUpdateDeleteProcedure("INSERT_TR_LOG_MONITORING", dbprocess);
            if (prhasil > 0)
            {
                return "OK";
            }
            else
            {
                return "NG";
            }
        }
    }
}
