using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISD_WS.Entities
{
    public class DatosResultado
    {
        //Datos resultados al generar y distribuir remesa
        public int Resultado { get; set; }
        public int NumeroRemesa { get; set; }

        //Datos para aplicar la remesa
        public decimal TipoCambio { get; set; }
        public int NumeroCompania { get; set; }

        public string DataAreaCiaAX { get; set; }
        public bool Correct { get; set; }

        //Datos para lista de BLs de la referencia
        public List<string> liBL { get; set; }

        //Datos del deposito
        public long RecIdAX { get; set; }
        public string Referencia { get; set; }
        public int IdRemesaAX { get; set; }
        public DateTime? FechaDeposito { get; set; }
        public string DiarioPagoAX { get; set; }
        public string MensajeError { get; set; }
        public int NumeroRecibo { get; set; }
    }
}
