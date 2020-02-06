using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITB.VENDIX.BE;
using ITB.VENDIX.BL;
using VendixWeb.Models;
using Microsoft.Reporting.WebForms;
using System.Globalization;
using Helper;

namespace VendixWeb.Controllers
{
    [Autenticado]
    public class ReporteController : Controller
    {

        public ActionResult Almacen()
        {
            ViewBag.cboOficina = new SelectList(OficinaBL.Listar(x => x.Estado), "OficinaId", "Denominacion");

            return View(SerieArticuloBL.ObtenerIndicadoresAlmacen());
        }
        public ActionResult ConstanciaAlmacen(int pMovimientoId)
        {
            var mov = MovimientoBL.ObtenerEntradaSalida(pMovimientoId);
            var det = MovimientoDetBL.Listar(x => x.MovimientoId == pMovimientoId);

            return View(new ReporteConstanciaAlmacen { Cabecera = mov, Detalle = det });
        }
        public ActionResult ConstanciaOrdenPago(int pOrdenId)
        {
            var mov = OrdenVentaBL.Obtener(x=>x.OrdenVentaId== pOrdenId, includeProperties: "OrdenVentaDet,Persona");
            return View(mov);
        }
        public ActionResult CobrosdelDia()
        {
            //var mov = MovimientoBL.ObtenerEntradaSalida(pMovimientoId);
            //var det = MovimientoDetBL.Listar(x => x.MovimientoId == pMovimientoId);

            return View();
        }
        public ActionResult Credito()
        {
            var lstoficina = new SelectList(OficinaBL.Listar(x => x.Estado), "OficinaId", "Denominacion");
            ViewBag.cboOficina = lstoficina;
            ViewBag.cboOficina1 = lstoficina;
            ViewBag.cboOficina2 = lstoficina;
            ViewBag.cboOficina3 = lstoficina;
            ViewBag.cboOficina4 = lstoficina;
            var usuario = new SelectList(UsuarioBL.Listar(x => x.Estado, includeProperties: "Persona"), "UsuarioId", "Persona.NombreCompleto");

            ViewBag.cboUsuario = usuario;
            ViewBag.cboGestor = usuario;
            ViewBag.cboGestor2 = usuario;
            return View();
        }
        public ActionResult Venta()
        {
            ViewBag.cboMarca = new SelectList(MarcaBL.Listar(x => x.Estado, x => x.OrderBy(y => y.Denominacion)), "MarcaId", "Denominacion");
            ViewBag.cboOficina = new SelectList(OficinaBL.Listar(x => x.Estado), "OficinaId", "Denominacion");
            return View();
        }
        #region "ReportViewer"
        public ActionResult ReporteMovimientoCaja(int pMovimientoCajaId)
        {
            var operacion = MovimientoCajaBL.Obtener(pMovimientoCajaId).Operacion;
            if (operacion == "CUO")
            {
                if (PlanPagoBL.Contar(x => x.MovimientoCajaId == pMovimientoCajaId) > 0)
                {
                    var data = MovimientoCajaBL.RptMovCajaCredito(pMovimientoCajaId);
                    var parametros = new List<ReportParameter>
                                         {
                                             new ReportParameter("MovimientoCajaId", data.MovimientoCajaId.ToString()),
                                             new ReportParameter("PersonaId", data.PersonaId.ToString()),
                                             new ReportParameter("Cliente", data.Cliente),
                                             new ReportParameter("User", data.User),
                                             new ReportParameter("FechaReg", data.FechaReg.ToString()),
                                             new ReportParameter("Oficina", data.Oficina),
                                             new ReportParameter("Producto", data.Producto),
                                             new ReportParameter("SaldoAnterior", data.SaldoAnterior.ToString()),
                                             new ReportParameter("PagoDeuda", data.PagoDeuda.ToString()),
                                             new ReportParameter("Interes", data.Interes.ToString()),
                                             new ReportParameter("MoraCargo", data.MoraCargo.ToString()),
                                             new ReportParameter("ImporteLibre", data.ImporteLibre.ToString()),
                                             new ReportParameter("ImportePagado", data.ImportePagado.ToString()),
                                             new ReportParameter("SaldoCapital", data.SaldoCapital.ToString()),
                                             new ReportParameter("CuotasPagadas", data.CuotasPagadas),
                                             new ReportParameter("ProximaCuota", data.ProximaCuota),
                                             new ReportParameter("CuotasAtrazadas", data.CuotasAtrazadas.ToString())
                                         };
                    return Reporte("PDF", "rptMovCajaCuota.rdlc", null, "VoucherCaja", parametros);
                }
                else
                {
                    var data = MovimientoCajaBL.RptMovCajaLibre(pMovimientoCajaId);
                    var parametros = new List<ReportParameter>
                                         {
                                             new ReportParameter("MovimientoCajaId", data.MovimientoCajaId.ToString()),
                                             new ReportParameter("PersonaId", data.PersonaId.ToString()),
                                             new ReportParameter("Cliente", data.Cliente),
                                             new ReportParameter("User", data.User),
                                             new ReportParameter("FechaReg", data.FechaReg.ToString()),
                                             new ReportParameter("Oficina", data.Oficina),
                                             new ReportParameter("Producto", data.Producto),
                                             new ReportParameter("ImportePago", data.ImportePago.ToString()),
                                             new ReportParameter("Articulo", data.Articulo),
                                             new ReportParameter("Concepto", "PAGO LIBRE")
                                         };
                    return Reporte("PDF", "rptMovCaja.rdlc", null, "VoucherCaja", parametros);
                }
            }
            if (operacion == "INI" || operacion == "GAD" || operacion == "CDN")
            {
                var data = MovimientoCajaBL.RptMovCajaInicial(pMovimientoCajaId);
                var concepto = string.Empty;
                switch (operacion)
                {
                    case "INI": concepto = "PAGO INICIAL"; break;
                    case "GAD": concepto = "GASTO ADM ADELANTADO"; break;
                    case "CDN": concepto = "PAGO POR CONDONACION"; break;
                }
                

                var parametros = new List<ReportParameter>
                                     {
                                         new ReportParameter("MovimientoCajaId", data.MovimientoCajaId.ToString()),
                                         new ReportParameter("PersonaId", data.PersonaId.ToString()),
                                         new ReportParameter("Cliente", data.Cliente),
                                         new ReportParameter("User", data.User),
                                         new ReportParameter("FechaReg", data.FechaReg.ToString()),
                                         new ReportParameter("Oficina", data.Oficina),
                                         new ReportParameter("Producto", data.Producto),
                                         new ReportParameter("ImportePago", data.ImportePago.ToString()),
                                         new ReportParameter("Articulo", data.Articulo),
                                         new ReportParameter("Concepto", concepto)
                                     };
                return Reporte("PDF", "rptMovCaja.rdlc", null, "VoucherCaja", parametros);
            }
            if (operacion == "CON")
            {
                var data = MovimientoCajaBL.RptMovCajaContado(pMovimientoCajaId);
                var parametros = new List<ReportParameter>
                                     {
                                         new ReportParameter("MovimientoCajaId", data.MovimientoCajaId.ToString()),
                                         new ReportParameter("PersonaId", data.PersonaId.ToString()),
                                         new ReportParameter("Cliente", data.Cliente),
                                         new ReportParameter("User", data.User),
                                         new ReportParameter("FechaReg", data.FechaReg.ToString()),
                                         new ReportParameter("Oficina", data.Oficina),
                                         new ReportParameter("Producto", data.Producto),
                                         new ReportParameter("ImportePago", data.ImportePago.ToString()),
                                         new ReportParameter("Articulo", data.Articulo),
                                         new ReportParameter("Concepto", "PAGO CONTADO")
                                     };
                return Reporte("PDF", "rptMovCaja.rdlc", null, "VoucherCaja", parametros);
            }

            var dato = MovimientoCajaBL.RptMovCajaOtros(pMovimientoCajaId);
            if (string.IsNullOrEmpty(dato.Articulo)) dato.Articulo = "*";
            var param = new List<ReportParameter>
                                     {
                                         new ReportParameter("MovimientoCajaId", dato.MovimientoCajaId.ToString()),
                                         new ReportParameter("PersonaId", "0"),
                                         new ReportParameter("Cliente", dato.Cliente),
                                         new ReportParameter("User", dato.User),
                                         new ReportParameter("FechaReg", dato.FechaReg.ToString()),
                                         new ReportParameter("Oficina", dato.Oficina),
                                         new ReportParameter("Producto", dato.Producto),
                                         new ReportParameter("ImportePago", dato.ImportePago.ToString()),
                                         new ReportParameter("Articulo", dato.Articulo),
                                         new ReportParameter("Concepto", dato.IndEntrada?"ENTRADA":"SALIDA")
                                     };
            return Reporte("PDF", "rptMovCaja.rdlc", null, "VoucherCaja", param);

        }
        public ActionResult ReporteCodBarras(int pMovimientoId)
        {
            var data = SerieArticuloBL.ListarArticuloCodigoBarras(pMovimientoId);
            var rd = new ReportDataSource("dsCodigo", data);
            return Reporte("PDF", "rptCodigo.rdlc", rd, "CodigoBarras");
        }
        public ActionResult ReporteKardex(int pArticuloId, int pAlmacenId)
        {
            var kardexData = AlmacenBL.GenerarKardex(pArticuloId, pAlmacenId);
            var rd = new ReportDataSource("dsKardex", kardexData);
            return Reporte("PDF", "rptKardex.rdlc", rd, "A4Horizontal0.25");
        }
        public ActionResult ReporteSimuladorPlanPagos(int pProductoId,decimal pMonto, int pCuotas, decimal pInteres, 
            string pFecha, string pModalidad, decimal? pGastosAdm = null, string pGA = "CAP",string pCliente="")
        {
            decimal desemb = pMonto;
            decimal pga = 0;
            if (pGA == "CAP")         
                desemb = pMonto - (pGastosAdm.HasValue ? pGastosAdm.Value : 0);
                        
            if (pGA == "CUO")
                pga = pGastosAdm.Value;


            var oPlanPago = CreditoBL.SimuladorCredito(pMonto, pModalidad,  pCuotas, pInteres, DateTime.Parse(pFecha), pga);
            var rd = new ReportDataSource("dsSimuladorPlanPago", oPlanPago);

                       
            switch (pModalidad)
            {
                case "D": pModalidad = "DIARIO";  break;
                case "S": pModalidad = "SEMANAL"; break;
                case "Q": pModalidad = "QUINCENAL";  break;
                case "M": pModalidad = "MENSUAL";  break;
            }

            //var pTem = CreditoBL.ObtenerTEM(pInteres, pModalidad); // Math.Round(Math.Pow(double.Parse((1 + pInteres/100).ToString()), 1/periodoAnio) - 1, 6);
                      

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Monto", "S/. " + pMonto),
                                     new ReportParameter("Cuotas", pCuotas.ToString()),
                                     new ReportParameter("Producto", ProductoBL.Obtener(pProductoId).Denominacion),
                                     new ReportParameter("Fecha", pFecha),
                                     new ReportParameter("Modalidad", pModalidad),
                                     new ReportParameter("Cliente", pCliente),
                                     new ReportParameter("TEM",pInteres + " %"),
                                     //new ReportParameter("TEM","-"),
                                     new ReportParameter("Desembolso", "S/. " + desemb),
                                     new ReportParameter("GastosAdm", "S/. " + Math.Round( pGastosAdm.Value,2).ToString("0.00",CultureInfo.InvariantCulture))
                                 };

            return Reporte("PDF", "rptSimuladorPlanPago.rdlc", rd, "A4Vertical0.25", parametros);
        }
        public ActionResult ReportePlanPagos(int pCreditoId)
        {
            var credito = CreditoBL.Obtener(x => x.CreditoId == pCreditoId, "Persona");
            var oPlanPago = CreditoBL.ReportePlanPago(pCreditoId);
            var rd = new ReportDataSource("dsSimuladorPlanPago", oPlanPago);

            string pModalidad = string.Empty;
            switch (credito.FormaPago)
            {
                case "D": pModalidad = "DIARIO"; break;
                case "S": pModalidad = "SEMANAL"; break;
                case "Q": pModalidad = "QUINCENAL"; break;
                case "M": pModalidad = "MENSUAL"; break;
            }
            //var pTem = CreditoBL.ObtenerTEM(credito.Interes, pModalidad);

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Monto", credito.MontoCredito.ToString()),
                                     new ReportParameter("Cuotas", credito.NumeroCuotas.ToString()),
                                     new ReportParameter("Producto", ProductoBL.Obtener(credito.ProductoId).Denominacion),
                                     new ReportParameter("Fecha", credito.FechaPrimerPago.ToShortDateString()),
                                     new ReportParameter("Modalidad", pModalidad),
                                     new ReportParameter("Cliente", credito.Persona.NombreCompleto),
                                     new ReportParameter("TEM", credito.Interes.ToString() + "%"),
                                     //new ReportParameter("TEM",Math.Round(pTem,6).ToString() + "%"),
                                     new ReportParameter("Desembolso", credito.MontoDesembolso.ToString()),
                                     new ReportParameter("GastosAdm", credito.MontoGastosAdm.ToString())
                                 };

            return Reporte("PDF", "rptSimuladorPlanPago.rdlc", rd, "A4Vertical0.25", parametros);
        }
        public ActionResult ReporteEstadoCredito(int pCreditoId)
        {
            string pModalidad = string.Empty;
            var credito = CreditoBL.Obtener(x => x.CreditoId == pCreditoId, includeProperties: "Persona,Producto");
            var data = CreditoBL.ListarEstadoPlanPago(pCreditoId);
            var rd = new ReportDataSource("dsEstadoCredito", data);

            switch (credito.FormaPago)
            {
                case "D": pModalidad = "DIARIO"; break;
                case "S": pModalidad = "SEMANAL"; break;
                case "Q": pModalidad = "QUINCENAL"; break;
                case "M": pModalidad = "MENSUAL"; break;
            }
            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Producto", credito.Producto.Denominacion),
                                     new ReportParameter("MontoProducto", credito.MontoProducto.ToString()),
                                     new ReportParameter("MontoInicial", credito.MontoInicial.ToString()),
                                     new ReportParameter("MontoCredito", credito.MontoCredito.ToString()),
                                     new ReportParameter("Modalidad", pModalidad),
                                     new ReportParameter("Cuotas", credito.NumeroCuotas.ToString()),
                                     new ReportParameter("Interes", credito.Interes.ToString()),
                                     new ReportParameter("Estado", credito.Estado),
                                     new ReportParameter("Cliente", credito.Persona.NumeroDocumento + " " + credito.Persona.NombreCompleto),
                                     new ReportParameter("Analista", UsuarioBL.ObtenerNombre(credito.UsuarioRegId)),
                                     new ReportParameter("GastoAdm", credito.MontoGastosAdm.ToString()),
                                     new ReportParameter("Desembolso", credito.MontoDesembolso.ToString())
                                 };
            return Reporte("PDF", "rptEstadoCredito.rdlc", rd, "A4Vertical0.25", parametros);
        }
        public ActionResult ReporteCredito(int? pOficinaId, int? pGestorid, string pEstadoCredito, string pFechaIni, string pFechaFin)
        {
            var data = ReporteBL.ListarReporteCredito(pOficinaId, pGestorid, pEstadoCredito, DateTime.Parse(pFechaIni), DateTime.Parse(pFechaFin));
            var rd = new ReportDataSource("dsCredito", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;
            var ogestor = "TODOS";
            if (pGestorid != null)
                ogestor = UsuarioBL.Obtener(x => x.UsuarioId == pGestorid.Value, includeProperties: "Persona").Persona.NombreCompleto;

            switch (pEstadoCredito)
            {
                case "CRE": pEstadoCredito = "SOLICITUDES DE CREDITO"; break;
                case "PEN": pEstadoCredito = "PENDIENTES"; break;
                case "PAG": pEstadoCredito = "PAGADOS"; break;
                case "DES": pEstadoCredito = "DESEMBOLSADOS"; break;
                case "ANU": pEstadoCredito = "ANULADOS"; break;
                case "REP": pEstadoCredito = "REPROGRAMADOS"; break;
            }

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("Gestor", ogestor),
                                     new ReportParameter("Estado", pEstadoCredito),
                                     new ReportParameter("FechaIni", DateTime.Parse(pFechaIni).ToShortDateString()),
                                     new ReportParameter("FechaFin", DateTime.Parse(pFechaFin).ToShortDateString())
                                 };
            return Reporte("PDF", "rptCredito.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteCreditoRentabilidad(string pFechaIni, string pFechaFin, bool indTodos, int? pOficinaId)
        {
            if (indTodos)
            {
                pFechaIni = "01/10/2013";
                pFechaFin = VendixGlobal.GetFecha().ToShortDateString();
            }
            var data = CreditoBL.ReporteCreditoRentabilidad(pOficinaId, DateTime.Parse(pFechaIni), DateTime.Parse(pFechaFin));
            var rd = new ReportDataSource("dsCreditoRentabilidad", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("FechaIni", indTodos?"----": DateTime.Parse(pFechaIni).ToShortDateString()),
                                     new ReportParameter("FechaFin", DateTime.Parse(pFechaFin).ToShortDateString())
                                 };
            return Reporte("PDF", "rptCreditoRentabilidad.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteCreditoAprobado(string pFecha, int? pUsuarioid, int? pOficinaid)
        {
            
            var data = CreditoBL.ReporteCreditoAprobacion(DateTime.Parse(pFecha), pUsuarioid, pOficinaid);
            var rd = new ReportDataSource("dsCreditoAprobacion", data);

            var oficina = "TODOS";
            if (pOficinaid != null)
                oficina = OficinaBL.Obtener(pOficinaid.Value).Denominacion;
            var usuario = "TODOS";
            if (pUsuarioid != null)
                usuario = UsuarioBL.Obtener(x => x.UsuarioId == pUsuarioid.Value, includeProperties: "Persona").Persona.NombreCompleto;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("Gestor", usuario),
                                     new ReportParameter("Fecha", DateTime.Parse(pFecha).ToShortDateString())
                                 };
            return Reporte("PDF", "rptCreditoAprobacion.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteCreditoMorosidad(int? pOficinaId, string pFechaHasta, int pDiasAtrazoIni, int pDiasAtrazoFin)
        {
            var data = CreditoBL.ReporteCreditoMorosidad(pOficinaId, DateTime.Parse(pFechaHasta), pDiasAtrazoIni, pDiasAtrazoFin);
            var rd = new ReportDataSource("dsCreditoMorosidad", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("HastaFecha",DateTime.Parse(pFechaHasta).ToShortDateString()),
                                     new ReportParameter("DiasAtrazoIni", pDiasAtrazoIni.ToString()),
                                     new ReportParameter("DiasAtrazoFin", pDiasAtrazoFin.ToString())
                                 };
            return Reporte("PDF", "rptCreditoMorosidad.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteCreditoObservado(int? pOficinaId, int? pUsuarioId, string pTipo = "PDF")
        {
            var data = CreditoBL.ReporteCreditoObservado(pOficinaId, pUsuarioId);
            var rd = new ReportDataSource("dsCreditoObservado", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;
            var agente = "TODOS";
            if (pUsuarioId != null)
                agente = UsuarioBL.Obtener(x => x.UsuarioId == pUsuarioId.Value, includeProperties: "Persona").Persona.NombreCompleto;


            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("Agente", agente)
                                 };
            return Reporte(pTipo, "rptCreditoObservado.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteMorosidadGestor(int? pOficinaId, int pUsuarioId)
        {
            return ReporteCobroDiario(pUsuarioId, pOficinaId, true);
        }
        public ActionResult ReporteAvanceVenta(string pFechaIni, string pFechaFin, bool indContado, bool indCredito, int? pOficinaId)
        {
            var data = ReporteBL.ListarReporteRentabilidadVenta(DateTime.Parse(pFechaIni), DateTime.Parse(pFechaFin), indContado, indCredito, pOficinaId);
            var rd = new ReportDataSource("dsRentabilidad", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina),
                                     new ReportParameter("FechaIni", DateTime.Parse(pFechaIni).ToShortDateString()),
                                     new ReportParameter("FechaFin", DateTime.Parse(pFechaFin).ToShortDateString())
                                 };
            return Reporte("PDF", "rptRentabilidad.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteListaPrecio(int? pMarcaId, bool pIndDescuento, bool pIndPuntos, string pTipo = "PDF")
        {
            var data = ReporteBL.ListarReporteListaPrecio(pMarcaId, pIndDescuento, pIndPuntos);
            var rd = new ReportDataSource("dsListaPrecio", data);
            var marca = "TODOS";
            if (pMarcaId != null)
                marca = MarcaBL.Obtener(pMarcaId.Value).Denominacion;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Marca", marca),
                                     new ReportParameter("EsDescuento", pIndDescuento?"SI":"NO"),
                                     new ReportParameter("EsPunto", pIndPuntos?"SI":"NO")
                                 };
            return Reporte(pTipo, "rptListaPrecio.rdlc", rd, "A4Vertical0.25", parametros);
        }
        public ActionResult ReporteStock(int? pOficinaId, string pTipoReporte = "PDF")
        {
            var data = ReporteBL.ListarReporteStockGeneral(pOficinaId);
            var rd = new ReportDataSource("dsStock", data);

            var oficina = "TODOS";
            if (pOficinaId != null)
                oficina = OficinaBL.Obtener(pOficinaId.Value).Denominacion;

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina", oficina)
                                 };
            //return Reporte("Excel", "rptStock.rdlc", rd, "A4Vertical0.25", parametros);
            return Reporte(pTipoReporte, "rptStock.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public ActionResult ReporteStockAnulados(string pTipoReporte = "PDF")
        {
            var data = ReporteBL.ListarReporteStockAnulados();
            var rd = new ReportDataSource("dsStokAnulados", data);
                        
            //var parametros = new List<ReportParameter>
            //                     {
            //                         new ReportParameter("Oficina", oficina)
            //                     };
            //return Reporte("Excel", "rptStock.rdlc", rd, "A4Vertical0.25", parametros);
            return Reporte(pTipoReporte, "rptStockAnulados.rdlc", rd, "A4Horizontal0.25", null);
        }
        public ActionResult ReporteSaldoCajaActual()
        {
            return ReporteSaldoCaja(VendixGlobal.GetCajaDiarioId());
        }
        public ActionResult ReporteSaldoCaja(int pCajaDiarioId)
        {
            var data = CajaDiarioBL.ReporteSaldoCajaDiario(pCajaDiarioId);
            if (data.Count == 0)
                return Content("Reporte Sin Registros!!!!");
            var rd = new ReportDataSource("dsSaldoCaja", data);
            var cab = CajaDiarioBL.ObtenerRptSaldoCajaCab(pCajaDiarioId);

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Oficina",cab.Oficina ),
                                     new ReportParameter("Cajero",cab.Cajero ),
                                     new ReportParameter("Estado",cab.Estado),
                                     new ReportParameter("Fecha",cab.Fecha.ToShortDateString() ),
                                     new ReportParameter("SaldoInicial",cab.SaldoInicial.ToString() ),
                                     new ReportParameter("SaldoFinal",cab.SaldoFinal.ToString()),
                                     new ReportParameter("PorcentajeCobro",cab.PorcentajeCobro.ToString())
                                 };
            return Reporte("PDF", "rptSaldoCaja.rdlc", rd, "A4Vertical0.25", parametros);

        }
        public ActionResult ReporteCreditoMovimiento(int pCreditoId)
        {
            var credito = CreditoBL.Obtener(x => x.CreditoId == pCreditoId, "Persona");
            var oMov = CreditoBL.ReporteCreditoMovimiento(pCreditoId);
            var rd = new ReportDataSource("dsCreditoMov", oMov);

            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("CreditoId", pCreditoId.ToString()),
                                     new ReportParameter("Cliente", credito.Persona.NombreCompleto),
                                     new ReportParameter("Articulo", credito.Descripcion)
                                 };

            return Reporte("PDF", "rptCreditoMov.rdlc", rd, "A4Vertical0.25", parametros);
        }
        public ActionResult ReporteCobroDiario(int? pGestorid, int? pOficinaid,bool indMora=false)
        {
            var usuarioid = pGestorid.HasValue ? pGestorid.Value : VendixGlobal.GetUsuarioId();
            var cajadiarioid = VendixGlobal.GetCajaDiarioId();
            var oAgente = UsuarioBL.Obtener(x => x.UsuarioId == usuarioid, "Persona");
            var sCaja = pGestorid.HasValue ? string.Empty : CajaDiarioBL.Obtener(x => x.CajaDiarioId == cajadiarioid, "Caja").Caja.Denominacion;
            var titulo = "COBRO DIARIO";
            var oCredito = CreditoBL.ReporteCobroDiario(usuarioid, pOficinaid);
            if (indMora) {
                oCredito = oCredito.Where(x => x.Mora > 0).ToList();
                titulo = "REPORTE DE MOROSIDAD";
            }
                

            var saldosininteres = oCredito.Sum(x => x.MontoCredito);
            var saldomora = oCredito.Sum(x => x.Mora);
            var saldoVencido = oCredito.Where(x=>x.DiasAtrazo>0).Sum(x => x.MontoCredito);
            
            var rd = new ReportDataSource("dsCobroDiario", oCredito);
            var parametros = new List<ReportParameter>
                                 {
                                     new ReportParameter("Fecha", VendixGlobal.GetFecha().ToString()),
                                     new ReportParameter("Agente", oAgente.Persona.NombreCompleto),
                                     new ReportParameter("Caja", sCaja),
                                     new ReportParameter("SaldoVencido", saldoVencido.ToString()),
                                     new ReportParameter("SaldoMoroso", saldomora.ToString()),
                                     new ReportParameter("SaldoSinInteres", saldosininteres.ToString()),
                                     new ReportParameter("Titulo", titulo)
                                 };

            return Reporte("PDF", "rptCobroDiario.rdlc", rd, "A4Horizontal0.25", parametros);
        }
        public FileContentResult ReporteCentrarRiegoTXT(int? pOficinaId, int pAnio, int pMes)
        {
            var rpt = ReporteBL.ListarReporteCentralRiesgo(pOficinaId, pAnio, pMes);
            int numeroItems = rpt.Count();
            string vacio = string.Empty;

            var sw = new StringWriter();
            using (sw)
            {
                for (int i = 0; i < numeroItems; i++)
                {                    
                    sw.WriteLine(rpt[i].Periodo + rpt[i].Entidad + rpt[i].TipoDoc.ToString().PadLeft(31,' ') + rpt[i].NumDoc.PadLeft(12,' ') 
                        + rpt[i].RazonSocial.PadLeft(100, ' ') + rpt[i].ApePat.PadRight(20, ' ') + rpt[i].ApeMat.PadRight(20, ' ') + rpt[i].Nombres.PadRight(30, ' ')
                        + rpt[i].TipoPersona + rpt[i].ModalidadCredito + rpt[i].DeudaMenor30.Replace(".","").PadLeft(39,' ') + rpt[i].DeudaMayor30.Replace(".", "").PadLeft(13, ' ')
                        + vacio.PadLeft(65,' ') + vacio.PadLeft(117, ' ')+ rpt[i].Calificacion+ rpt[i].DiasAtrazo.ToString().PadRight(5,' ') 
                        + rpt[i].Direccion.PadRight(80,' ') + vacio.PadLeft(120, ' ') + rpt[i].celular.PadRight(10,' ')) ;
                }
            }

            String NombreArchivo = "DM007898";
            return File(new System.Text.UTF8Encoding().GetBytes(sw.ToString()), "text/txt", NombreArchivo + ".txt");
        }

        public ActionResult Reporte(string pTipoReporte, string rdlc, ReportDataSource rds, string pPapel, List<ReportParameter> pParametros = null)
        {
            var lr = new LocalReport();
            lr.ReportPath = Path.Combine(Server.MapPath("~/Reporte"), rdlc);

            if (rds != null) lr.DataSources.Add(rds);
            if (pParametros != null) lr.SetParameters(pParametros);

            string reportType = pTipoReporte;
            string mimeType;
            string encoding;
            string fileNameExtension;

            var deviceInfo = ObtenerPapel(pPapel).Replace("[TipoReporte]", pTipoReporte);
            Warning[] warnings;
            string[] streams;

            byte[] renderedBytes = lr.Render(reportType, deviceInfo, out mimeType, out encoding,
                                             out fileNameExtension, out streams, out warnings);

            return File(renderedBytes, mimeType);
        }

        private static string ObtenerPapel(string pPapel)
        {
            switch (pPapel)
            {
                case "A4Horizontal":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>11in</PageWidth>" +
                           "  <PageHeight>8.5in</PageHeight>" +
                           "  <MarginTop>0in</MarginTop>" +
                           "  <MarginLeft>0in</MarginLeft>" +
                           "  <MarginRight>0in</MarginRight>" +
                           "  <MarginBottom>0in</MarginBottom>" +
                           "</DeviceInfo>";
                case "A4Vertical":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>8.5in</PageWidth>" +
                           "  <PageHeight>11in</PageHeight>" +
                           "  <MarginTop>0in</MarginTop>" +
                           "  <MarginLeft>0in</MarginLeft>" +
                           "  <MarginRight>0in</MarginRight>" +
                           "  <MarginBottom>0in</MarginBottom>" +
                           "</DeviceInfo>";
                case "A4Horizontal0.25":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>11in</PageWidth>" +
                           "  <PageHeight>8.5in</PageHeight>" +
                           "  <MarginTop>0.25in</MarginTop>" +
                           "  <MarginLeft>0.25in</MarginLeft>" +
                           "  <MarginRight>0.25in</MarginRight>" +
                           "  <MarginBottom>0.25in</MarginBottom>" +
                           "</DeviceInfo>";
                case "A4Vertical0.25":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>8.5in</PageWidth>" +
                           "  <PageHeight>11in</PageHeight>" +
                           "  <MarginTop>0.25in</MarginTop>" +
                           "  <MarginLeft>0.25in</MarginLeft>" +
                           "  <MarginRight>0.25in</MarginRight>" +
                           "  <MarginBottom>0.25in</MarginBottom>" +
                           "</DeviceInfo>";
                case "TicketCaja":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>3.5in</PageWidth>" +
                           "  <PageHeight>5.0in</PageHeight>" +
                           "  <MarginTop>0in</MarginTop>" +
                           "  <MarginLeft>0.1in</MarginLeft>" +
                           "  <MarginRight>0in</MarginRight>" +
                           "  <MarginBottom>0in</MarginBottom>" +
                           "</DeviceInfo>";
                case "VoucherCaja":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>8.5in</PageWidth>" +
                           "  <PageHeight>11in</PageHeight>" +
                           "  <MarginTop>0in</MarginTop>" +
                           "  <MarginLeft>0in</MarginLeft>" +
                           "  <MarginRight>0in</MarginRight>" +
                           "  <MarginBottom>0in</MarginBottom>" +
                           "</DeviceInfo>";
                case "CodigoBarras":
                    return "<DeviceInfo>" +
                           "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                           "  <PageWidth>4.13in</PageWidth>" +
                           "  <PageHeight>2.76in</PageHeight>" +
                           "  <MarginTop>0in</MarginTop>" +
                           "  <MarginLeft>0in</MarginLeft>" +
                           "  <MarginRight>0in</MarginRight>" +
                           "  <MarginBottom>0in</MarginBottom>" +
                           "</DeviceInfo>";

            }

            return "<DeviceInfo>" +
                   "  <OutputFormat>[TipoReporte]</OutputFormat>" +
                   "  <PageWidth>8.5in</PageWidth>" +
                   "  <PageHeight>11in</PageHeight>" +
                   "  <MarginTop>0in</MarginTop>" +
                   "  <MarginLeft>0in</MarginLeft>" +
                   "  <MarginRight>0in</MarginRight>" +
                   "  <MarginBottom>0in</MarginBottom>" +
                   "</DeviceInfo>";
        }
        #endregion

    }
}
