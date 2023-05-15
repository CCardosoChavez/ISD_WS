using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISD_WS.Entities
{
    public class DatosDeposito
    {
        public decimal MontoBL { get; set; }
        public string Moneda { get; set; }
        public string Descripcion { get; set; }
        public string Orden_Servicio { get; set; }
        public long RecIDCLiente { get; set; }
        public decimal TipoCambio { get; set; }
        public int NumeroLinea { get; set; }
        public long RecIDAX { get; set; }
        public List<object> liDatosDeposito { get; set; }
        public bool Correct { get; set; }
    }
}
