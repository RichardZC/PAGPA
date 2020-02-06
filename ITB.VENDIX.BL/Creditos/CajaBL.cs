using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class CajaBL : Repositorio<Caja>
    {

        public static List<Caja> LstCajaJGrid(GridDataRequest request, ref int pTotalItems)
        {
            string filterExpression = string.Empty;

            if (request.DataFilters()["Buscar"] != string.Empty)
                filterExpression = "Denominacion.Contains( \"" + request.DataFilters()["Buscar"] + "\")";

            using (var db = new VENDIXEntities())
            {
                IQueryable<Caja> query = db.Caja.Include("Oficina");
                if (!String.IsNullOrEmpty(filterExpression))
                    query = query.Where(filterExpression);

                pTotalItems = query.Count();
                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
            }
        }
        public static List<CajaDiarioOficina> LstCajaDiarioOficina()
        {
            var oficina = VendixGlobal.GetOficinaId();
            using (var db = new VENDIXEntities())
            {
                var query = from c in db.CajaDiario
                            where c.Caja.OficinaId == oficina && c.TransBoveda == false
                            select new CajaDiarioOficina
                            {
                                CajaDiarioId = c.CajaId,
                                NombreCaja = c.Caja.Denominacion,
                                IndCierre = c.IndCierre,
                                Cajero = c.Usuario.Persona.NombreCompleto,
                                FechaIniOperacion = c.FechaIniOperacion,
                                FechaFinOperacion = c.FechaFinOperacion,
                                SaldoInicial = c.SaldoInicial,
                                Entradas = c.Entradas,
                                Salidas = c.Salidas,
                                SaldoFinal = c.SaldoFinal
                            };
                return query.ToList();
            }
        }

        public static List<usp_UsuariosNoAsignadosCaja_Result> ListaUsuariosNoAsignado()
        {
            using (var db = new VENDIXEntities())
            {
                return db.usp_UsuariosNoAsignadosCaja(VendixGlobal.GetOficinaId()).ToList();
            }
        }

        public static List<ItemCombo> ListarCajasAbiertas()
        {
            var oficinaId = VendixGlobal.GetOficinaId();
            using (var db = new VENDIXEntities())
            {
                var query = from c in db.CajaDiario
                            where c.Caja.OficinaId == oficinaId && c.IndCierre == false
                            select new ItemCombo
                            {
                                id = c.CajaId,
                                value = c.Caja.Denominacion + " - " + c.Usuario.Persona.NombreCompleto
                            };
                return query.ToList();
            }
        }

    }

    public class CajaDiarioOficina : CajaDiario
    {
        public string NombreCaja { get; set; }
        public string Cajero { get; set; }
    }
}
