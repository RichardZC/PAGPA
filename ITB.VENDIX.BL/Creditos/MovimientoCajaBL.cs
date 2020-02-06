using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITB.VENDIX.BE;
using System.Data.Objects.SqlClient;

namespace ITB.VENDIX.BL
{
    public class MovimientoCajaBL : Repositorio<MovimientoCaja>
    {
        public static List<decimal> ResumenEntradaSalida(int pCajadiarioId)
        {
            var resumen = new List<decimal>();

            using (var db = new VENDIXEntities())
            {
                if (db.MovimientoCaja.Count(x => x.CajaDiarioId == pCajadiarioId && x.Estado && x.IndEntrada) > 0)
                {
                    resumen.Add(db.MovimientoCaja
                                    .Where(x => x.CajaDiarioId == pCajadiarioId && x.Estado && x.IndEntrada)
                                    .Sum(x => x.ImportePago));
                }
                else
                {
                    resumen.Add(0);
                }

                if (db.MovimientoCaja.Count(x => x.CajaDiarioId == pCajadiarioId && x.Estado && x.IndEntrada == false) >
                    0)
                {
                    resumen.Add(db.MovimientoCaja
                                    .Where(x => x.CajaDiarioId == pCajadiarioId && x.Estado && x.IndEntrada == false)
                                    .Sum(x => x.ImportePago));
                }
                else
                {
                    resumen.Add(0);
                }
            }
            return resumen;
        }

        public static MovCajaCredito RptMovCajaCredito(int pMovimientoCajaId)
        {
            using (var db = new VENDIXEntities())
            {
                var plan = db.PlanPago.Where(x => x.MovimientoCajaId == pMovimientoCajaId).ToList();
                var creditoid = plan.First().CreditoId;
                var saldoAnt = plan.OrderBy(x => x.Numero).First().Capital;
                var cuotaspagadas = plan.Max(x => x.Numero);
                var pagodeuda = plan.Sum(x => x.Amortizacion);
                var interes = plan.Sum(x => x.Interes + x.GastosAdm);
                var importelibre = plan.Sum(x => x.PagoLibre);
                var cargosmora = plan.Sum(x => x.ImporteMora + x.InteresMora + x.Cargo);
                var pagocuota = plan.Sum(x => x.PagoCuota.Value);
                var proxcuota = string.Empty;
                var proxcuotapen = db.PlanPago
                                    .Where(x => x.CreditoId == creditoid && x.Estado == "PEN")
                                    .OrderBy(x => x.Numero).FirstOrDefault();
                if (proxcuotapen != null)
                    proxcuota = proxcuotapen.FechaVencimiento.ToShortDateString();
                
                var cuotasAtrazadas = db.PlanPago
                    .Count(x => x.CreditoId == creditoid && x.Estado == "PEN" && x.FechaVencimiento < VendixGlobal.GetFecha());

                var qrycre = from mc in db.MovimientoCaja
                             where mc.MovimientoCajaId == pMovimientoCajaId
                             select new MovCajaCredito
                             {
                                 MovimientoCajaId = mc.MovimientoCajaId,
                                 PersonaId = mc.PersonaId,
                                 Cliente = mc.Persona.NombreCompleto,
                                 User = mc.Usuario.NombreUsuario,
                                 FechaReg = mc.FechaReg,
                                 Oficina = mc.CajaDiario.Caja.Oficina.Denominacion,
                                 Producto = mc.Credito.Producto.Denominacion,
                                 SaldoAnterior = saldoAnt,
                                 PagoDeuda = pagodeuda,
                                 Interes = interes,
                                 MoraCargo = cargosmora,
                                 ImporteLibre = mc.ImportePago - pagocuota - importelibre,
                                 ImportePagado = mc.ImportePago,
                                 SaldoCapital = saldoAnt - pagodeuda,
                                 CuotasPagadas = SqlFunctions.StringConvert((double)cuotaspagadas).Trim() + " de " + SqlFunctions.StringConvert((double)mc.Credito.NumeroCuotas).Trim(),
                                 ProximaCuota = proxcuota,
                                 CuotasAtrazadas = cuotasAtrazadas
                             };
                return qrycre.ToList()[0];
            }
        }

        public static MovCajaBase RptMovCajaInicial(int pMovimientoCajaId)
        {
            using (var db = new VENDIXEntities())
            {
                var credito = db.MovimientoCaja.First(x => x.MovimientoCajaId == pMovimientoCajaId).Credito;
                
                var qrycre = from mc in db.MovimientoCaja
                             where mc.MovimientoCajaId == pMovimientoCajaId
                             select new MovCajaBase
                             {
                                 MovimientoCajaId = mc.MovimientoCajaId,
                                 PersonaId = mc.PersonaId,
                                 Cliente = mc.Persona.NombreCompleto,
                                 User = mc.Usuario.NombreUsuario,
                                 FechaReg = mc.FechaReg,
                                 Oficina = mc.CajaDiario.Caja.Oficina.Denominacion,
                                 Producto = credito.Producto.Denominacion,
                                 ImportePago = mc.ImportePago,
                                 Articulo = credito.Descripcion
                             };
                return qrycre.ToList()[0];
            }
        }

        public static MovCajaBase RptMovCajaLibre(int pMovimientoCajaId)
        {
            using (var db = new VENDIXEntities())
            {
                var credito = db.PlanPagoLibre.First(x => x.MovimientoCajaId == pMovimientoCajaId).PlanPago.Credito;

                var qrycre = from mc in db.MovimientoCaja
                             where mc.MovimientoCajaId == pMovimientoCajaId
                             select new MovCajaBase
                             {
                                 MovimientoCajaId = mc.MovimientoCajaId,
                                 PersonaId = mc.PersonaId,
                                 Cliente = mc.Persona.NombreCompleto,
                                 User = mc.Usuario.NombreUsuario,
                                 FechaReg = mc.FechaReg,
                                 Oficina = mc.CajaDiario.Caja.Oficina.Denominacion,
                                 Producto = credito.Producto.Denominacion,
                                 ImportePago = mc.ImportePago,
                                 Articulo = mc.Descripcion
                             };
                return qrycre.ToList()[0];
            }
        }

        public static MovCajaBase RptMovCajaContado(int pMovimientoCajaId)
        {
            using (var db = new VENDIXEntities())
            {
                var listaart = string.Join(Environment.NewLine,db.MovimientoCaja.Find(pMovimientoCajaId)
                                                       .OrdenVenta.OrdenVentaDet.Select(x => x.Descripcion));
                
                var qrycre = from mc in db.MovimientoCaja
                             where mc.MovimientoCajaId == pMovimientoCajaId
                             select new MovCajaBase
                             {
                                 MovimientoCajaId = mc.MovimientoCajaId,
                                 PersonaId = mc.PersonaId,
                                 Cliente = mc.Persona.NombreCompleto,
                                 User = mc.Usuario.NombreUsuario,
                                 FechaReg = mc.FechaReg,
                                 Oficina = mc.CajaDiario.Caja.Oficina.Denominacion,
                                 Producto = mc.Descripcion,
                                 ImportePago = mc.ImportePago,
                                 Articulo = listaart
                             };
                return qrycre.ToList()[0];
            }
        }
        public static MovCajaBase RptMovCajaOtros(int pMovimientoCajaId)
        {
            using (var db = new VENDIXEntities())
            {
                var qrycre = from mc in db.MovimientoCaja
                             where mc.MovimientoCajaId == pMovimientoCajaId
                             select new MovCajaBase
                             {
                                 MovimientoCajaId = mc.MovimientoCajaId,
                                 PersonaId = mc.PersonaId,
                                 Cliente = mc.Persona.NombreCompleto,
                                 User = mc.Usuario.NombreUsuario,
                                 FechaReg = mc.FechaReg,
                                 Oficina = mc.CajaDiario.Caja.Oficina.Denominacion,
                                 Producto = "CAJA DIARIO",
                                 ImportePago = mc.ImportePago,
                                 Articulo = mc.Descripcion,
                                 IndEntrada = mc.IndEntrada
                             };
                return qrycre.ToList()[0];
            }
        }
    }

    public class MovCajaBase : MovimientoCaja
    {
        public string Cliente { get; set; }
        public string User { get; set; }
        public string Oficina { get; set; }
        public string Producto { get; set; }
        public string Articulo { get; set; }
    }

    public class MovCajaCredito : MovCajaBase
    {
        public decimal SaldoAnterior { get; set; }
        public decimal PagoDeuda { get; set; }
        public decimal Interes { get; set; }
        public decimal MoraCargo { get; set; }
        public decimal ImporteLibre { get; set; }
        public decimal ImportePagado { get; set; }
        public decimal SaldoCapital { get; set; }
        public string CuotasPagadas { get; set; }
        public string ProximaCuota { get; set; }
        public int CuotasAtrazadas { get; set; }
    }

}
