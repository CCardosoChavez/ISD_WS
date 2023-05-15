using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISD_WS.LOG;
using ISD_WS.DAL;
using ISD_WS.Entities;
using System.Configuration;

namespace ISD_WS.BL
{
    public class ISD_BL
    {
        private string _DominioAX { get => ConfigurationManager.AppSettings["DynDomain"]; }
        private string _UserNameAX { get => ConfigurationManager.AppSettings["DynUserName"]; }
        private string _PasswordAX { get => ConfigurationManager.AppSettings["DynPassword"]; }
        private string _CompanyAX { get => ConfigurationManager.AppSettings["DynCompany"]; }


        RegistroLog log = new RegistroLog();
        ISD_DAO da = new ISD_DAO();
        public string RegistrarPagoISD(decimal dMonto, string sReferencia, string sNombreArchivo,
           DateTime dFechaDeposito, long RecId, string sCuentaBanco, string sMoneda)
        {
            log.LogProceso($"ISD_BL -- RegistrarPagoISD() => Datos recibidos en el servicio: Monto: {dMonto}, Referencia: {sReferencia}, Nombre Archivo: {sNombreArchivo}, Fecha Deposito: {dFechaDeposito}, RecIdAX: {RecId}, Cuenta Banco: {sCuentaBanco}, Moneda: {sMoneda} ");
            string respuesta = "";

            try
            {
                //Obtener el tipo de cambio guardado en la tabla Referencia al igual que la compania, debido a esto retorna una lista que parcea con un entero para el IDCOMPAÑIA
                DatosResultado datos = da.ObtenerDatosParaAplicarRemesa(sReferencia);
                if (datos.Correct)
                {
                    //El tipo de cambio debe obtenerce de la tabla referencias. 
                    log.LogProceso($"ISD_BL -- RegistrarPagoISD => Monto: {dMonto}, Referencia: {sReferencia}, Tipo cambio: {datos.TipoCambio}, " +
                        $"Nombre archivo: {sNombreArchivo}, Fecha depósito: {dFechaDeposito}, RecIdAX: {RecId}, Cuenta banco: {sCuentaBanco}, " +
                        $"Moneda: {sMoneda}, Numero Compania: {datos.NumeroCompania}");

                    DatosResultado datosResultado = da.GenerarDistribuirRemesa(dMonto, sReferencia, datos.TipoCambio, sNombreArchivo, dFechaDeposito,
                        RecId, sCuentaBanco, sMoneda);

                    switch (datosResultado.Resultado)
                    {
                        case 0:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. Se generó un error al hacer la distribución del deposito en alguna transacción");
                            break;
                        case 1:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. Se ralizo el pago completo");
                            break;
                        case 2:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. La referencia no se reconcoce");
                            break;
                        case 3:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. El pago no cubre la referencia");
                            break;
                        case 4:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. El pago no cubre la referencia aun con montos almacenados");
                            break;
                        case 5:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. Es un saldo a favor");
                            break;
                        default:
                            log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- GenerarDistribuirRemesa() => Respuesta: {datosResultado.Resultado}. Se generó un error al hacer la distribución del deposito");
                            break;
                    }


                    //Si el registro fue exitoso se consume el WS de AX para establecer el recid del cliente al que pertenece el depósito realizado
                    if (datosResultado.Resultado != 0 || datosResultado.Resultado != 2)
                    {

                        datos.RecIdAX = RecId;
                        datos.FechaDeposito = dFechaDeposito;
                        datos.NumeroRemesa = datosResultado.NumeroRemesa;
                        datos.IdRemesaAX = datosResultado.IdRemesaAX;
                        datos.Referencia = sReferencia;
                        DatosResultado respuestaDiarioGenerado = EstablecerClienteDePagoISD(datos);

                        switch (respuestaDiarioGenerado.Resultado)
                        {

                            case 0:
                                respuesta = $"Entro a la excepcion de  EstablecerClienteDePagoISD(). Error: {respuestaDiarioGenerado.MensajeError}";
                                break;
                            case 1:
                                //Si se generó el numero de diario y la información se guardó correctamente se genera un recibo
                                DatosResultado resultadoRecibo = da.GenerarReciboISD(datos);
                                if (resultadoRecibo.Correct)
                                {
                                    log.LogProceso($"RegistrarPagoISD -- GenerarReciboISD() => Se generó el recibo con exito en liner: {resultadoRecibo.NumeroRecibo}");
                                    respuesta = "OK"; //Se realizo el proceso completo con exito
                                }
                                else
                                {
                                    log.LogProceso($"RegistrarPagoISD -- GenerarReciboISD() => No se pudo generar el recibo");
                                    respuesta = "No se pudo generar el recibo en liner";
                                }
                                break;
                            case 2:
                                respuesta = "No se pudieron obtener los datos de la base para generar el diario de pago en AX";
                                break;
                            case 3:
                                respuesta = $"No se pudo obtener el diario de pago para el deposito de ISD del servicio de AX pr_GenerateDepositISD Error: {respuestaDiarioGenerado.MensajeError}";
                                break;
                            case 4:
                                respuesta = "No se pudo guardar el Diario del deposito generado en la Base de datos";
                                break;
                            default:
                                respuesta = "Error.";
                                break;
                        }
                    }
                    else
                    {
                        respuesta = "Error: Se generó un error al hacer la distribución del deposito en alguna transacción en la base de datos";
                    }
                        
                }
                else
                {
                    log.LogProceso($"ISD_BL -- RegistrarPagoISD() -- ObtenerDatosParaAplicarRemesa() => No se pudo obtener el tipo de cambio ni la compania del la referencia {sReferencia}");
                    respuesta = $"Error: No se pudo obtener el tipo de cambio ni la compania del la referencia {sReferencia}";
                }

                
            }
            catch (Exception ex)
            {
                log.LogProceso($" ISD_BL -- RegistrarPagoISD() =>  Entro al chatch. Exception: {ex.Message}");
                log.LogError($"ISD_BL -- RegistrarPagoISD()  => {ex.Message} || {ex.Source} || {ex.StackTrace}");
                respuesta = "Error: "+ ex.Message;
            }

            return respuesta;
        }

        private DatosResultado EstablecerClienteDePagoISD(DatosResultado datos)
        {
            DatosResultado result = new DatosResultado();
            log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() => Referencia: {datos.Referencia}, RecIdAX: {datos.RecIdAX}, IdCompania: {datos.NumeroCompania}");

            try
            {
                //Se obtienen los datos para el registro del depósito en garantía en AX
                DatosDeposito datosDeposito = da.ObtenerDatosGenerarDiarioDeposito(datos.Referencia, datos.RecIdAX);

                if (datosDeposito.Correct)
                {
                    //Distribuir los datos para generar el diario de deposito por BL
                    //Identificar que recID del cliente se utilizará ya que al existir multiples BLS se tiene un recId por cliente

                    log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD()-- ObtenerDatosGenerarDiarioDeposito() => Se obtuvieron los datos: Monto: {datosDeposito.MontoBL}, RecIdAX: {datos.RecIdAX}, Descripción: {datosDeposito.Descripcion}, " +
                        $"Orden de servicio: {datosDeposito.Orden_Servicio}, RecIdCliente: {datosDeposito.RecIDCLiente}, Tipo de cambio: {datosDeposito.TipoCambio}, " +
                        $"Número de línea: {datosDeposito.NumeroLinea}, RecIdAX: {datosDeposito.RecIDAX}");

                    #region datos
                    string Domain = _DominioAX;
                    string User = _UserNameAX;
                    string Password = _PasswordAX;
                    #endregion datos


                    WS_ISD.CallContext ctx = new WS_ISD.CallContext();
                    ctx.Company = datos.DataAreaCiaAX.ToLower();
                    WS_ISD.LinerInvoicesServiceClient client = new WS_ISD.LinerInvoicesServiceClient();
                    client.ClientCredentials.Windows.ClientCredential.Domain = Domain;
                    client.ClientCredentials.Windows.ClientCredential.UserName = User;
                    client.ClientCredentials.Windows.ClientCredential.Password = Password;

                    log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() => Conexión al WSDynamicsAX - LinerInvoicesService. Datos enviados: (Monto: {datosDeposito.MontoBL}, Descripcion: {datosDeposito.Descripcion}, RecIdCliente: {0}, " +
                        $" TipoCambio: {datosDeposito.TipoCambio}, Numero Linea: {datosDeposito.NumeroLinea}, RecIdAX: {datosDeposito.RecIDAX})");
                    
                    int recIdCliente = 0;
                    
                    var respDiarioAX = client.pr_GenerateDepositISD(ctx, datosDeposito.MontoBL, datosDeposito.Moneda, datosDeposito.Descripcion, recIdCliente,
                        datosDeposito.TipoCambio, datosDeposito.NumeroLinea, datosDeposito.RecIDAX);

                    //var respDiarioAX = "000003";

                    if (respDiarioAX.ToUpper().Contains("ERROR"))
                    {
                        log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD()  -- pr_GenerateDepositISD() => No se pudo obtener el diario de pago para el deposito de ISD. Error: {respDiarioAX}");
                        CambiarEstatusRegistrosPorErroresISD(datos);
                        result.MensajeError = respDiarioAX;
                        result.Resultado = 3;
                    }
                    else
                    {
                        log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD()  -- pr_GenerateDepositISD() => Se genró el diario de pago para el deposito de ISD. Voucher: {respDiarioAX}");
                        datos.DiarioPagoAX = respDiarioAX;
                        bool bGuardado = da.GuardarDiarioAXISD(datos);

                        if (bGuardado == false)
                        {
                            CambiarEstatusRegistrosPorErroresISD(datos);
                            log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() -- GuardarDiarioAXISD() => No se pudo guardar el Diario del deposito en la Base de datos.  Respuesta: {bGuardado}");
                            result.Resultado = 4;
                        }
                        else
                        {
                            log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() -- GuardarDiarioAXISD() => Se guardó el Diario del deposito en la Base de datos.  Respuesta: {bGuardado}");
                            result.Resultado = 1;
                        }   
                    }

                    //resp corresponde al voucher (alfanumérico) que se deberá de enviar al realizar la solicitud de devolución de saldo
                    //Al realizar la solicitud de devolución de saldo la respuesta es el voucher que identifica la solicitud
                    //Cuando ya se realiza la devolución AX consume el WS y regresa el mismo voucher de identificación de la solicitud acompañado de la fecha de devolución
                }
                else
                {
                    log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() -- ObtenerDatosGenerarDiarioDeposito() => No se pudieron obtener los datos para generar el diario de pago en AX");
                    result.Resultado = 2;
                }

                
            }
            catch (Exception ex)
            {
                log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() =>  Entro al catch, Error: {ex.Message}");
                log.LogProceso($"ISD_BL -- EstablecerClienteDePagoISD() -- pr_GenerateDepositISD() => No se pudo obtener el diario de pago");
                log.LogError($"ISD_BL -- EstablecerClienteDePagoISD() => {ex.Message} || {ex.Source} || {ex.StackTrace}");
                CambiarEstatusRegistrosPorErroresISD(datos);
                result.MensajeError = ex.Message;
                result.Resultado = 0;
            }
            return result;
        }

        private void CambiarEstatusRegistrosPorErroresISD(DatosResultado datosReferencia)
        {
            bool bEliminado = da.CambiarEstatusRegistrosSinDiarioISD(datosReferencia);

            if (bEliminado)
                log.LogProceso($"CambiarEstatusRegistrosSinDiarioISD => Se cambio el estatus a inactivo (2)  de los registros y la remesa ");
            else
                log.LogProceso($"CambiarEstatusRegistrosSinDiarioISD => No se pudo cambiar el estatus a inactivo (2)  de los registros ni la remesa");
        }
    }
}
