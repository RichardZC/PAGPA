using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITB.VENDIX.BL
{
    public class Constante
    {
        public const decimal IGV = 0.18M;
        public class SerieArticulo {
            public const int SIN_CONFIRMAR = 1;
            public const int EN_ALMACEN = 2;
            public const int PREVENTA = 3;
            public const int VENDIDO = 4;
            public const int ANULADO = 5;               
        }
    }
}
