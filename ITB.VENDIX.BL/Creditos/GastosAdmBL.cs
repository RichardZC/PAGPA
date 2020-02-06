using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class GastosAdmBL : Repositorio<GastosAdm>
    {
        public static decimal CalcularGastosAdm(decimal pImporte, bool pIncluyeCentralRiesgo)
        {
            decimal gastos = 0;
            var lstgastos = Listar(x => x.Estado && pImporte >= x.MontoMinimo && pImporte <= x.MontoMaximo);
            if (pIncluyeCentralRiesgo == false)            
                lstgastos = lstgastos.Where(x => x.Ind == false).ToList();
            
            foreach (var item in lstgastos)
            {
                if (item.IndPorcentaje)
                    gastos += pImporte*(item.Valor/100);
                else
                    gastos += item.Valor;
            }
            return Math.Round(gastos, 2);
        }
    }
}
