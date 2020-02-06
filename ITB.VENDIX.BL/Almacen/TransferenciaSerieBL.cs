using ITB.VENDIX.BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITB.VENDIX.BL
{
    public class TransferenciaSerieBL : Repositorio<TransferenciaSerie>
    {
        public static void Eliminar(TransferenciaSerie transferenciaSerie)
        {
            throw new NotImplementedException();
        }


        public class EntradaSalida
        {
            public int TransferenciaSerieId { get; set; }
            public int TransferenciaId { get; set; }
            public string SerieArticuloId { get; set; }

        }
    }
}
