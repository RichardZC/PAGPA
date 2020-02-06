using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class TarjetaPuntoBL : Repositorio<TarjetaPunto>
   {

       public static string CanjearPuntos(int pCodCliente, string pNumSerie)
       {
           using (var db = new VENDIXEntities())
           {
               return db.usp_CanjearPuntos(pCodCliente, pNumSerie).ToList()[0];
           }
       }

      }

 }

