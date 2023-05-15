using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using ISD_WS.BL;

namespace ISD_WS
{
    /// <summary>
    /// Descripción breve de ISD
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente. 
    // [System.Web.Script.Services.ScriptService]
    public class ISD : System.Web.Services.WebService
    {
        ISD_BL bl = new ISD_BL();
        [WebMethod]
        public string RegistrarDepositoISD(decimal dMonto, string sReferencia, decimal dTipoCambio, string sNombreArchivo,
            DateTime dFechaDeposito, long RecId, string sCuentaBanco, string sMoneda)
        {
            return bl.RegistrarPagoISD(dMonto, sReferencia, sNombreArchivo, dFechaDeposito,
                RecId, sCuentaBanco, sMoneda);
        }

    }
}
