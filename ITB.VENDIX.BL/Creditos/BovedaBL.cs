using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class BovedaBL: Repositorio<Boveda>
    {
        public static List<Boveda> LstBovedaJGrid(GridDataRequest request, ref int pTotalItems)
        {
            var oficinaid = VendixGlobal.GetOficinaId();
            using (var db = new VENDIXEntities())
            {
                IQueryable<Boveda> query = db.Boveda.Where(x=>x.OficinaId== oficinaid);             

                pTotalItems = query.Count();
                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
            }
        }
        public static List<BovedaMov> LstBovedaMovJGrid(GridDataRequest request, ref int pTotalItems)
        {
            string filterExpression = string.Empty;

            if (request.DataFilters()["BovedaId"] != string.Empty)
                filterExpression = "BovedaId == " + request.DataFilters()["BovedaId"];
            
            using (var db = new VENDIXEntities())
            {                
                IQueryable<BovedaMov> query = db.BovedaMov;
                if (!String.IsNullOrEmpty(filterExpression))
                    query = query.Where(filterExpression);

                pTotalItems = query.Count();
                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
            }
        }

        public  static bool Cerrar()
        {
            var oficinaid = VendixGlobal.GetOficinaId();
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        var boveda = db.Boveda.First(x => x.OficinaId == oficinaid && x.IndCierre==false);
                        boveda.IndCierre = true;
                        boveda.FechaFinOperacion = VendixGlobal.GetFecha();
                        Actualizar(db,boveda);

                        Crear(db,new Boveda()
                                  {
                                      OficinaId = oficinaid,
                                      SaldoInicial = boveda.SaldoFinal,
                                      Entradas = 0,
                                      Salidas = 0,
                                      SaldoFinal = boveda.SaldoFinal,
                                      FechaIniOperacion = VendixGlobal.GetFecha(),
                                      IndCierre = false
                                  });
                        db.SaveChanges();
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    return false;
                }
            }
        }

        
 
    }
}
