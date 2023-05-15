using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISD_WS.LOG
{
    public class RegistroLog
    {
        string cnxSQL = ConfigurationManager.ConnectionStrings["SqlCnx"].ConnectionString;

        public void RegistraError(string mensaje, string clase, string metodo)
        {
            bool bRegistra = false;
            SqlConnection cnx = new SqlConnection(cnxSQL);

            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_BitacoraError", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@sMensaje", SqlDbType.VarChar).Value = mensaje;
                cmd.Parameters.Add("@sClase", SqlDbType.VarChar).Value = clase;
                cmd.Parameters.Add("@sMetodo", SqlDbType.VarChar).Value = metodo;

                cmd.Parameters.Add("@bRespuesta", SqlDbType.Bit).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                bRegistra = Convert.ToBoolean(cmd.Parameters["@bRespuesta"].Value);
            }
            catch (Exception ex)
            {
                LogError("RegistroLog -- RegistraError(): Error: " + ex.Message);
            }
            finally
            {
                if (cnx.State == ConnectionState.Open)
                    cnx.Close();
            }
        }

        public void LogProceso(string mensaje)
        {
            string nombreArchivo = string.Empty;
            string msg = string.Empty;
            string ruta = string.Empty;

            try
            {
                nombreArchivo = $"LogProceso_{DateTime.Now.ToString("dd-MM-yyyy")}.txt";
                msg = mensaje.Equals("") ? mensaje : string.Format("{0:G}: {1}\r\n", DateTime.Now, mensaje);
                ruta = $"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogProceso\\{nombreArchivo}";

                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogProceso"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogProceso");

                File.AppendAllText(ruta, msg);
            }
            catch (Exception ex)
            {
                LogError("RegistroLog -- LogProceso(): Error: " + ex.Message);
            }
        }

        public void LogError(string mensaje)
        {
            string nombreArchivo = string.Empty;
            string msg = string.Empty;
            string ruta = string.Empty;

            try
            {
                nombreArchivo = $"LogError_{DateTime.Now.ToString("dd-MM-yyyy")}.txt";
                msg = mensaje.Equals("") ? mensaje : string.Format("{0:G}: {1}\r\n", DateTime.Now, mensaje);
                ruta = $"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogError\\{nombreArchivo}";

                if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogError"))
                    Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}Log\\LogError");

                File.AppendAllText(ruta, msg);
            }
            catch (Exception ex)
            {
                //LogError(mensaje);
            }
        }
    }
}
