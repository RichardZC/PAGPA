using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class CreditoBL : Repositorio<Credito>
    {
        public static bool CrearSolicitudCredito(int pPersonaId)
        {
            var oCredito = new Credito
            {
                OficinaId = VendixGlobal.GetOficinaId(),
                PersonaId = pPersonaId,
                TipoCuota = "F",
                Descripcion = "",
                MontoProducto = 0,
                MontoInicial = 0,
                MontoCredito = 500,
                ProductoId = 1,
                MontoGastosAdm = 0,
                TipoGastoAdm = "CAP",
                Estado = "CRE",
                FormaPago = "D",
                NumeroCuotas = 26,
                Interes = 8,
                Observacion = string.Empty,
                FechaPrimerPago = VendixGlobal.GetFecha(),
                FechaVencimiento = VendixGlobal.GetFecha(),
                FechaReg = VendixGlobal.GetFecha(),
                UsuarioRegId = VendixGlobal.GetUsuarioId(),
                Calificacion = "A"
            };
            oCredito.MontoGastosAdm = GastosAdmBL.CalcularGastosAdm(oCredito.MontoCredito,true);
            CreditoBL.Crear(oCredito);
            return true;
        }

        public static DatoCredito ObtenerDatoCredito(int pCreditoId)
        {
            
            using (var db = new VENDIXEntities())
            {
                var qry = from c in db.Credito
                          where c.CreditoId == pCreditoId
                          select new DatoCredito()
                          {
                              CreditoId = c.CreditoId,
                              Descripcion = c.Descripcion,
                              MontoProducto = c.MontoProducto,
                              MontoInicial = c.MontoInicial,
                              MontoCredito = c.MontoCredito,
                              MontoGastosAdm = c.MontoGastosAdm,
                              MontoDesembolso = c.MontoDesembolso,
                              TipoGastoAdm = c.TipoGastoAdm,
                              FormaPago = c.FormaPago,
                              NumeroCuotas = c.NumeroCuotas,
                              Interes = c.Interes,
                              Estado = c.Estado,
                              Observacion = c.Observacion,
                              FechaDesembolso = c.FechaDesembolso,
                              FechaAprobacion = c.FechaAprobacion,
                              FechaVencimiento = c.FechaVencimiento,
                              Analista = c.Usuario.Persona.NombreCompleto,
                              ProductoCre = c.Producto.Denominacion
                          };
                var data = qry.First();
                data.Desembolso = data.FechaDesembolso.HasValue ? data.FechaDesembolso.Value.ToShortDateString() : string.Empty;
                data.FAprobacion = data.FechaAprobacion.HasValue ? data.FechaAprobacion.Value.ToShortDateString() : string.Empty;
                data.Vencimiento = data.FechaVencimiento.ToShortDateString();
                if (data.Estado == "DES")
                    data.SaldoCancelacion = ObtenerSaldoCancelacion(pCreditoId);
                return data;
            }
        }

        public static List<usp_SimuladorCredito_Result> SimuladorCredito
            (decimal pMonto, string pFormaPago, int pNumerocuotas, decimal pInteres, DateTime pFechaPrimerPago,
             Decimal? pGastosAdm)
        {

            using (var db = new VENDIXEntities())
            {
                return db.usp_SimuladorCredito(pMonto, pFormaPago, pNumerocuotas, pInteres,
                        pFechaPrimerPago, pGastosAdm).ToList();
            }

        }

        public static List<RptPlanPago> ReportePlanPago(int pCreditoId)
        {
            using (var db = new VENDIXEntities())
            {
                return db.PlanPago.Where(x => x.CreditoId == pCreditoId)
                .Select(x => new RptPlanPago
                {
                    Numero = x.Numero,
                    Capital = x.Capital,
                    FechaPago = x.FechaVencimiento,
                    Amortizacion = x.Amortizacion,
                    Interes = x.Interes,
                    GastosAdm = x.GastosAdm,
                    Cuota = x.Cuota
                }).ToList();
            }
        }
        public static List<RptCreditoObservado> ReporteCreditoObservado(int? pOficinaId, int? pUsuarioId)
        {
            using (var db = new VENDIXEntities())
            {
                var qry = from c in db.Credito
                          where c.Observacion.Length > 0 && (c.Estado == "PEN" || c.Estado == "DES")
                          select new RptCreditoObservado
                          {
                              OficinaId = c.OficinaId,
                              Oficina = c.Oficina.Denominacion,
                              CreditoId = c.CreditoId,
                              Cliente = c.Persona.NombreCompleto,
                              FechaPrimerPago = c.FechaPrimerPago,
                              FechaVencimiento = c.FechaVencimiento,
                              MontoCredito = c.MontoCredito,
                              Interes = c.Interes,
                              AgenteId = c.UsuarioRegId,
                              Agente = c.Usuario.Persona.NombreCompleto,
                              Observacion = c.Observacion
                          };

                if (pOficinaId.HasValue)
                    qry = qry.Where(x => x.OficinaId == pOficinaId.Value);

                if (pUsuarioId.HasValue)
                    qry = qry.Where(x => x.AgenteId == pUsuarioId.Value);

                return qry.ToList();
            }
        }

        public static string CrearCredito(int pSolicitudCreditoId, int pProductoId, string pTipoCuota,
                                          decimal pMontoInicial, decimal pMontoGastosAdm, string pIndGastosAdm, decimal pMontoCredito,
                                          string pModalidad, int pNumerocuotas, decimal pInteresMensual, DateTime pFechaPrimerPago, string pObservacion)
        {

            string retorno;
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        retorno =
                            db.usp_Credito_Ins(pSolicitudCreditoId, pProductoId, pTipoCuota, pMontoInicial, pMontoCredito,
                                               pMontoGastosAdm, pIndGastosAdm, pModalidad, pNumerocuotas, pInteresMensual,
                                               pFechaPrimerPago, pObservacion, VendixGlobal.GetUsuarioId()).ToList()[0];
                    }
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    retorno = ex.InnerException.Message;
                }
            }
            return retorno;
        }
        public static bool AprobarCredito(int pCreditoId, int pOpcion)
        {
            var usuario = VendixGlobal<int>.Obtener("UsuarioId");
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        db.usp_Credito_Upd(pOpcion, pCreditoId, usuario);
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }
        public static bool RechazarCredito(int pCreditoId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        var oCredito = db.Credito.Find(pCreditoId);
                        if (oCredito.OrdenVentaId.HasValue)
                            db.usp_OrdenVenta_Del(oCredito.OrdenVentaId, 0);
                        else
                            db.usp_SolicitudCredito_Del(pCreditoId);
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }
        public static bool AnularCredito(int pCreditoId, string pObservacion)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        db.usp_Credito_Del(pCreditoId, pObservacion, VendixGlobal.GetUsuarioId());
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }
        public static bool ReprogramarCredito(int pCreditoId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        db.usp_ReprogramarCredito(pCreditoId, VendixGlobal.GetUsuarioId());
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }
        public static List<Credito> ListarCreditosGrd(GridDataRequest request, ref int pTotalItems)
        {
            string filterExpression = string.Empty;

            if (request.DataFilters()["Buscar"] != string.Empty)
                filterExpression = "PersonaId=" + request.DataFilters()["Buscar"];

            if (request.DataFilters()["Estado"] == "DES")
                filterExpression += " && (Estado=\"PEN\" || Estado=\"APR\" || Estado=\"DES\")";
            else
                filterExpression += " && (Estado=\"ANU\" || Estado=\"PAG\" || Estado=\"REP\")";

            using (var db = new VENDIXEntities())
            {
                IQueryable<Credito> query = db.Credito;
                if (!String.IsNullOrEmpty(filterExpression))
                    query = query.Where(filterExpression);

                pTotalItems = query.Count();

                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();

            }
        }
        public static List<usp_EstadoPlanPago_Result> ListarEstadoPlanPago(int pCreditoId)
        {
            using (var db = new VENDIXEntities())
            {
                return db.usp_EstadoPlanPago(pCreditoId).ToList();
            }
        }
        public static List<usp_RptCreditoRentabilidad_Result> ReporteCreditoRentabilidad(int? pOficinaId, DateTime pFechaIni, DateTime pFechaFin)
        {
            using (var db = new VENDIXEntities())
            {
                return db.usp_RptCreditoRentabilidad(pOficinaId, pFechaIni, pFechaFin).ToList();
            }
        }
        public static List<usp_RptCreditoAprobacion_Result> ReporteCreditoAprobacion(DateTime pFecha, int? pUsuarioid, int? pOficinaid)
        {
            using (var db = new VENDIXEntities())
            {
                return db.usp_RptCreditoAprobacion(pFecha, pUsuarioid, pOficinaid).ToList();
            }
        }
        public static List<usp_RptCreditoMorosidad_Result> ReporteCreditoMorosidad(int? pOficinaId, DateTime pFechaHasta, int pDiasAtrazoIni, int pDiasAtrazoFin)
        {
            using (var db = new VENDIXEntities())
            {
                return db.usp_RptCreditoMorosidad(pOficinaId, pFechaHasta, pDiasAtrazoIni, pDiasAtrazoFin).ToList();
            }
        }
        public static List<usp_RptCobroDiario_Result> ReporteCobroDiario(int pGestorid, int? pOficinaId)
        {         
            using (var db = new VENDIXEntities())
            {
                return db.usp_RptCobroDiario(pGestorid, pOficinaId).ToList();
            }
        }
        public static List<RptCreditoMov> ReporteCreditoMovimiento(int pCreditoId)
        {
            using (var db = new VENDIXEntities())
            {
                var qry = from mc in db.MovimientoCaja
                          where mc.CreditoId == pCreditoId && mc.Estado
                          orderby mc.FechaReg
                          select new RptCreditoMov
                          {
                              MovimientoCajaId = mc.MovimientoCajaId,
                              Operacion = mc.Operacion,
                              Fecha = mc.FechaReg,
                              Glosa = mc.Descripcion,
                              ImportePago = mc.ImportePago
                          };
                return qry.ToList();
            }
        }
        public static decimal ObtenerSaldoCancelacion(int pCreditoId)
        {
            using (var db = new VENDIXEntities())
            {
                var cancel = db.usp_CuotasPendientes(pCreditoId, VendixGlobal.GetFecha(), true).Sum(x => x.PagoCuota);
                return (decimal)cancel;
            }
        }
        public static decimal ObtenerTEM(decimal pTEA,string pFormaPago)
        {
            
            using (var db = new VENDIXEntities())
            {
                decimal tem = db.usp_CalcularTEM(pTEA, pFormaPago).First().Value;
                return tem;
            }
        }

        public static List<CreditoxAprobar> LstCreditoAprobarJGrid(GridDataRequest request, ref int pTotalItems)
        {
            string filterExpression = string.Empty;

            if (request.DataFilters()["Buscar"] != string.Empty)
                filterExpression = "Cliente.Contains( \"" + request.DataFilters()["Buscar"] + "\")";

            using (var db = new VENDIXEntities())
            {
                IQueryable<CreditoxAprobar> query = db.Credito.Where(x => x.Estado == "PEN")
                    .Select(x => new CreditoxAprobar
                    {
                        CreditoId = x.CreditoId,
                        PersonaId = x.PersonaId,
                        Codigo = x.Persona.Codigo,
                        Cliente = x.Persona.NombreCompleto,
                        Monto = x.MontoCredito,
                        Interes = x.Interes,
                        Agente = x.Usuario.Persona.NombreCompleto
                    });
                if (!String.IsNullOrEmpty(filterExpression))
                    query = query.Where(filterExpression);

                pTotalItems = query.Count();

                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();

            }
        }
    }

    public class CreditoxAprobar
    {
        public int CreditoId { get; set; }
        public int PersonaId { get; set; }
        public string Codigo { get; set; }
        public string Cliente { get; set; }
        public decimal Monto { get; set; }
        public decimal Interes { get; set; }
        public string Agente { get; set; }
    }

    public class DatoCredito : Credito
    {
        public string ProductoCre { get; set; }
        public string FAprobacion { get; set; }
        public string Desembolso { get; set; }
        public string Vencimiento { get; set; }
        public string Analista { get; set; }
        public decimal SaldoCancelacion { get; set; }
    }


    public class RptPlanPago
    {
        public int Numero { get; set; }
        public decimal Capital { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Amortizacion { get; set; }
        public decimal Interes { get; set; }
        public decimal GastosAdm { get; set; }
        public decimal Cuota { get; set; }
    }
    public class RptCreditoMov
    {
        public int MovimientoCajaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Operacion { get; set; }
        public string Glosa { get; set; }
        public decimal ImportePago { get; set; }
    }
    public class RptCreditoObservado
    {
        public int OficinaId { get; set; }
        public string Oficina { get; set; }
        public int CreditoId { get; set; }
        public string Cliente { get; set; }
        public DateTime FechaPrimerPago { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoCredito { get; set; }
        public decimal Interes { get; set; }
        public int AgenteId { get; set; }
        public string Agente { get; set; }
        public string Observacion { get; set; }
    }
}
