using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISD_WS.LOG;
using ISD_WS.Entities;

namespace ISD_WS.DAL
{
    public class ISD_DAO
    {
        RegistroLog log = new RegistroLog();
        string cnxSQL = ConfigurationManager.ConnectionStrings["SqlCnx"].ConnectionString;

        public DatosResultado ObtenerDatosParaAplicarRemesa(string sReferencia)
        {
            DatosResultado result = new DatosResultado();
            SqlConnection cnx = new SqlConnection(cnxSQL);

            result.Correct = false;
            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_GET_DATOS_PARA_REMESA_ISD", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@sReferencia", SqlDbType.VarChar, 20).Value = sReferencia;

                log.LogProceso($"ISD_DAO -- ObtenerDatosParaAplicarRemesa() => Procedure: SP_GET_DATOS_PARA_REMESA_ISD - Datos enviados: (@sReferencia: {sReferencia})");

                SqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        result.TipoCambio = (dataReader["TipoCambio"] is DBNull ? 0 : Convert.ToDecimal(dataReader["TipoCambio"]));
                        result.NumeroCompania = (dataReader["NumeroCompania"] is DBNull ? 0 : Convert.ToInt32(dataReader["NumeroCompania"]));
                        result.DataAreaCiaAX = dataReader["sDataAreaCiaAX"].ToString();
                        result.Correct = true;
                    }
                    log.LogProceso($"ISD_DAO -- ObtenerDatosParaAplicarRemesa()--  => Datos obtenidos: (TipoCambio: {result.TipoCambio}, NumeroCompania: {result.NumeroCompania}, DataAreaCiaAX: {result.DataAreaCiaAX})");
                }
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- ObtenerDatosParaAplicarRemesa() => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- ObtenerDatosParaAplicarRemesa() =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return result;
        }

        public DatosResultado GenerarDistribuirRemesa(decimal dMonto, string sReferencia, decimal? dTipoCambio, string sNombreArchivo,
            DateTime dFechaDeposito, long nRecId, string sCuentaBanco, string sMoneda)
        {
            DatosResultado result = new DatosResultado();
            SqlConnection cnx = new SqlConnection(cnxSQL);

            try
            {
                cnx.Open();
                int tipoRemesa = 2;

                SqlCommand cmd = new SqlCommand("SP_DISTRIBUIR_REMESA_POR_BL", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FSREFERENCIA", SqlDbType.VarChar, 20).Value = sReferencia;
                cmd.Parameters.Add("@FIIDTIPO_REMESA", SqlDbType.Int).Value = tipoRemesa;
                cmd.Parameters.Add("@FNMONTO_REMESA", SqlDbType.Float).Value = dMonto;
                cmd.Parameters.Add("@FSMONEDA", SqlDbType.VarChar).Value = sMoneda;
                cmd.Parameters.Add("@FDFECHA_DEPOSITO", SqlDbType.DateTime).Value = dFechaDeposito;
                cmd.Parameters.Add("@RECID_AX", SqlDbType.BigInt).Value = nRecId;
                cmd.Parameters.Add("@FNTIPO_CAMBIO", SqlDbType.Decimal).Value = dTipoCambio;
                cmd.Parameters.Add("@FSNOMBRE_ARCHIVO", SqlDbType.VarChar).Value = sNombreArchivo;
                cmd.Parameters.Add("@FSCUENTA_BANCO", SqlDbType.VarChar).Value = sCuentaBanco;

                cmd.Parameters.Add("@FNNUMERO_REMESA", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@BRESPUESTA", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IDREMESA_AX", SqlDbType.Int).Direction = ParameterDirection.Output;

                log.LogProceso($"ISD_DAO -- GenerarDistribuirRemesa() => Procedure: SP_DISTRIBUIR_REMESA_POR_BL - Datos enviados: (@FSREFERENCIA: {sReferencia}, @FIIDTIPO_REMESA: {tipoRemesa}, @FNMONTO_REMESA: {dMonto}" +
                    $", @FSMONEDA: {sMoneda}, @FDFECHA_DEPOSITO: {dFechaDeposito}, @RECID_AX: {nRecId}, @FNTIPO_CAMBIO: {dTipoCambio}, @FSNOMBRE_ARCHIVO: {sNombreArchivo}, @FSCUENTA_BANCO: {sCuentaBanco} )");

                cmd.ExecuteNonQuery();

                result.Resultado = cmd.Parameters["@BRESPUESTA"].Value is DBNull ? 0 : Convert.ToInt32(cmd.Parameters["@BRESPUESTA"].Value);
                result.NumeroRemesa = cmd.Parameters["@FNNUMERO_REMESA"].Value is DBNull ? 0 : Convert.ToInt32(cmd.Parameters["@FNNUMERO_REMESA"].Value);
                result.IdRemesaAX = cmd.Parameters["@IDREMESA_AX"].Value is DBNull ? 0 : Convert.ToInt32(cmd.Parameters["@IDREMESA_AX"].Value);

                log.LogProceso($"ISD_DAO -- GenerarDistribuirRemesa()--  => Datos obtenidos: (@BRESPUESTA: {result.Resultado}, @FNNUMERO_REMESA: {result.NumeroRemesa}, @IDREMESA_AX: {result.IdRemesaAX})");
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- GenerarDistribuirRemesa() => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- GenerarDistribuirRemesa() =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return result;
        }

        public DatosDeposito ObtenerDatosGenerarDiarioDeposito(string sReferencia, long nRecIdAX)
        {
            SqlConnection cnx = new SqlConnection(cnxSQL);
            DatosDeposito result = new DatosDeposito();
            result.liDatosDeposito = new List<object>();
            result.Correct = false;
            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_GET_DATOS_PARA_GENERAR_DIARIO_DEPOSITO_ISD", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@sReferencia", SqlDbType.VarChar).Value = sReferencia;
                cmd.Parameters.Add("@nRecIdAX", SqlDbType.BigInt).Value = nRecIdAX;

                log.LogProceso($"ISD_DAO -- ObtenerDatosGenerarDiarioDeposito() => Procedure: SP_GET_DATOS_PARA_GENERAR_DIARIO_DEPOSITO_ISD - Datos enviados: (@sReferencia: {sReferencia}, @nRecIdAX: {nRecIdAX} )");

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        result.MontoBL = reader["Monto"] is DBNull ? 0 : decimal.Parse(reader["Monto"].ToString()); //Convert.ToDecimal(reader["Monto"]);
                        result.Moneda = reader["Moneda"].ToString();
                        result.Descripcion = reader["Referencia"].ToString();
                        result.Orden_Servicio = reader["OrdenServicio"].ToString();
                        result.RecIDCLiente = reader["RecIDCliente"] is DBNull ? 0 : Int64.Parse(reader["RecIDCliente"].ToString()); //(long)reader["RecIDCliente"];
                        result.TipoCambio = reader["TipoCambio"] is DBNull ? 0 : decimal.Parse(reader["TipoCambio"].ToString()); //Convert.ToDecimal(reader["TipoCambio"]);
                        result.NumeroLinea = reader["NumeroLineaAX"] is DBNull ? 0 : Int32.Parse(reader["NumeroLineaAX"].ToString()); //(int)reader["NumeroLineaAX"];
                        result.RecIDAX = reader["RecIDAX"] is DBNull ? 0 : Int64.Parse(reader["RecIDAX"].ToString()); //(long)reader["RecIDAX"];
                        result.Correct = true;
                    }
                    log.LogProceso($"ISD_DAO --ObtenerDatosGenerarDiarioDeposito()--  => Datos obtenidos: (Monto: {result.MontoBL}, Moneda: {result.Moneda}, Referencia: {result.Descripcion}" +
                        $", OrdenServicio: {result.Orden_Servicio}, RecIDCliente: {result.RecIDCLiente}, TipoCambio: {result.TipoCambio}, NumeroLineaAX: {result.NumeroLinea}, RecIDAX: {result.RecIDAX})");
                }
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- ObtenerDatosGenerarDiarioDeposito()  => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- ObtenerDatosGenerarDiarioDeposito()  =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
                result.Correct = false;
            }
            finally
            {
                cnx.Close();
            }

            return result;
        }

        public DatosResultado ObtenerBLsPorReferencia(string sReferencia)
        {
            SqlConnection cnx = new SqlConnection(cnxSQL);
            DatosResultado result = new DatosResultado();
            result.liBL = new List<string>();

            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_GET_BL_POR_REFERENCIA", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@sREFERENCIA", SqlDbType.VarChar, 20).Value = sReferencia;
                log.LogProceso($"ISD_DAO -- ObtenerBLsPorReferencia() => Procedure: SP_GET_BL_POR_REFERENCIA - Datos enviados: (@sREFERENCIA: {sReferencia})");

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string numeroBL = "";
                        numeroBL = reader["NumeroBL"].ToString();
                        result.liBL.Add(numeroBL);
                    }
                    log.LogProceso($"ISD_DAO -- ObtenerBLsPorReferencia() => Bls obtenidos: {result.liBL.Count()}");
                }
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- ObtenerBLsPorReferencia()  => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- ObtenerBLsPorReferencia()  =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return result;
        }


        public bool GuardarDiarioAXISD(DatosResultado datos)
        {
            bool bRespuesta = false;
            SqlConnection cnx = new SqlConnection(cnxSQL);

            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_GUARDAR_DIARIO_AX_ISD", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add("@sReferencia", SqlDbType.VarChar,20).Value = sReferencia;
                //cmd.Parameters.Add("@sNumeroBL", SqlDbType.VarChar,20).Value = sNumeroBL;
                cmd.Parameters.Add("@nIdRemesaAX", SqlDbType.Int).Value = datos.IdRemesaAX;
                cmd.Parameters.Add("@sRecIdDiarioAX", SqlDbType.VarChar, 30).Value = datos.DiarioPagoAX;
                cmd.Parameters.Add("@bRespuesta", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log.LogProceso($"ISD_DAO -- GuardarDiarioAXISD() => Procedure: SP_GUARDAR_DIARIO_AX_ISD - Datos enviados: (@nIdRemesaAX: {datos.IdRemesaAX}, @sRecIdDiarioAX: {datos.DiarioPagoAX})");

                cmd.ExecuteNonQuery();

                bRespuesta = Convert.ToBoolean(cmd.Parameters["@bRespuesta"].Value);

                log.LogProceso($"ISD_DAO -- GuardarDiarioAXISD() => Datos obtenidos: (@bRespuesta: {bRespuesta})");
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- GuardarDiarioAXISD()   => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- GuardarDiarioAXISD()  =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return bRespuesta;
        }

        public DatosResultado GenerarReciboISD(DatosResultado datos)
        {
            DatosResultado datosResultado = new DatosResultado();
            SqlConnection cnx = new SqlConnection(cnxSQL);

            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_GENERAR_RECIBO_ISD", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@sReferencia", SqlDbType.VarChar, 20).Value = datos.Referencia;
                cmd.Parameters.Add("@FDFECHA_DEPOSITO", SqlDbType.DateTime, 20).Value = datos.FechaDeposito;
                cmd.Parameters.Add("@FNNUMERO_REMESA", SqlDbType.Int, 30).Value = datos.NumeroRemesa;

                cmd.Parameters.Add("@NUMERO_RECIBO", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@BRESPUESTA", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log.LogProceso($"ISD_DAO -- GenerarReciboISD() => Procedure: SP_GENERAR_RECIBO_ISD - Datos enviados: (@sReferencia: {datos.Referencia}, @FDFECHA_DEPOSITO: {datos.FechaDeposito}), @FNNUMERO_REMESA: {datos.NumeroRemesa}");
                cmd.ExecuteNonQuery();

                datosResultado.Correct = Convert.ToBoolean(cmd.Parameters["@BRESPUESTA"].Value);
                datosResultado.NumeroRecibo =  Convert.ToInt32(cmd.Parameters["@NUMERO_RECIBO"].Value);

                log.LogProceso($"ISD_DAO -- GenerarReciboISD() => Datos obtenidos: (@NUMERO_RECIBO: {datosResultado.NumeroRecibo}, @BRESPUESTA: {datosResultado.Correct})");
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- GenerarReciboISD() => Entro al catch. Error: {ex.Message}");
                log.RegistraError($"{ex.Message} || {ex.Source} || {ex.StackTrace}", "DepositosEnGarantiaDAO", "GenerarReciboISD");

                log.LogProceso($"ISD_DAO -- GenerarReciboISD()  => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- GenerarReciboISD() =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return datosResultado;
        }

        public bool CambiarEstatusRegistrosSinDiarioISD(DatosResultado datos)
        {
            bool bRespuesta = true;
            SqlConnection cnx = new SqlConnection(cnxSQL);

            try
            {
                cnx.Open();

                SqlCommand cmd = new SqlCommand("SP_ACTUALIZAR_ESTATUS_REGISTROS_SIN_DIARIO", cnx);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@FIIDREMESA_AX", SqlDbType.Int).Value = datos.IdRemesaAX;
                //cmd.Parameters.Add("@NUM_CIA", SqlDbType.Int).Value = datos.nNumeroCompania;
                cmd.Parameters.Add("@NUM_REMESA", SqlDbType.Int).Value = datos.NumeroRemesa;
                cmd.Parameters.Add("@FEC_APLICACION_REMESA", SqlDbType.DateTime).Value = datos.FechaDeposito;
                cmd.Parameters.Add("@TIPO_ERROR", SqlDbType.Int).Value = 1; //Tipo de error para cuando no se obtiene el diario de pago desde AX
                cmd.Parameters.Add("@RESPUESTA", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log.LogProceso($"ISD_DAO -- CambiarEstatusRegistrosSinDiarioISD() => Procedure: SP_ACTUALIZAR_ESTATUS_REGISTROS_SIN_DIARIO - Datos enviados: (@FIIDREMESA_AX: {datos.IdRemesaAX}, @NUM_REMESA: {datos.NumeroRemesa}, @FEC_APLICACION_REMESA:{datos.FechaDeposito}, @TIPO_ERROR: {1})");

                cmd.ExecuteNonQuery();

                bRespuesta = Convert.ToBoolean(cmd.Parameters["@RESPUESTA"].Value);
                log.LogProceso($"ISD_DAO -- CambiarEstatusRegistrosSinDiarioISD() => Datos obtenidos: (@RESPUESTA: {bRespuesta})");
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_DAO -- CambiarEstatusRegistrosSinDiarioISD()  => : Entro al catch. Exception: {ex.Message}");
                log.LogError($"ISD_DAO -- CambiarEstatusRegistrosSinDiarioISD() =>  {ex.Message} || {ex.Source} || {ex.StackTrace}");
            }
            finally
            {
                cnx.Close();
            }

            return bRespuesta;
        }

    }
}
