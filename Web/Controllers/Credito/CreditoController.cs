using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITB.VENDIX.BL;
using ITB.VENDIX.BE;
using System.Transactions;
using Helper;

namespace VendixWeb.Controllers
{
    [Autenticado]
    public class CreditoController : Controller
    {
        //
        // GET: /Credito/
        public ActionResult Creditos(int pPersonaId = 0)
        {
            if (pPersonaId > 0)
            {
                var usuarioId = VendixGlobal<int>.Obtener("UsuarioId");
                var oficinaId = VendixGlobal<int>.Obtener("OficinaId");
                var datos = new DatosCredito
                {
                    Persona = PersonaBL.Obtener(pPersonaId),
                    Cliente = ClienteBL.Obtener(x => x.PersonaId == pPersonaId),
                    SolicitudCredito = CreditoBL.Listar(x => x.Estado == "CRE" && x.PersonaId == pPersonaId,
                                        y => y.OrderByDescending(z => z.FechaReg), "Producto").FirstOrDefault(),
                    Producto = ProductoBL.Listar(x => x.Estado).FirstOrDefault(),
                    Creditos = CreditoBL.Listar(x => (x.Estado == "PEN" || x.Estado == "AP1" || x.Estado == "APR" || x.Estado == "DES") && x.PersonaId == pPersonaId, includeProperties: "PlanPago,Producto").ToList()

                };
                datos.EstadoCliente = datos.Cliente.Estado ? "ACTIVO" : "INACTIVO";
                datos.TotalCreditos = CreditoBL.Contar(x => x.PersonaId == pPersonaId && x.Estado != "CRE");

                switch (datos.Cliente.Calificacion)
                {
                    case "A": datos.CalificacionCliente = "BUENO"; break;
                    case "B": datos.CalificacionCliente = "REGULAR"; break;
                    case "C": datos.CalificacionCliente = "MALO"; break;
                    default: datos.CalificacionCliente = "NO TIENE"; break;
                }

                ViewBag.PersonaId = pPersonaId;
                ViewBag.Cliente = datos.Persona.NombreCompleto;
                ViewBag.cboProducto = new SelectList(ProductoBL.Listar(x => x.Estado), "ProductoId", "Denominacion");
                ViewBag.Aprobador1 = UsuarioRolBL.Contar(x => x.UsuarioId == usuarioId
                                                            && x.OficinaId == oficinaId
                                                            && x.Rol.Denominacion == "APROBADOR 1", includeProperties: "Rol");
                //ViewBag.Aprobador2 = UsuarioRolBL.Contar(x => x.UsuarioId == usuarioId
                //                                            && x.OficinaId == oficinaId
                //                                            && x.Rol.Denominacion == "APROBADOR 2", includeProperties: "Rol");

                if (datos.SolicitudCredito != null)
                    VendixGlobal<int>.Crear("SolicitudCreditoId", datos.SolicitudCredito.CreditoId);

                return View(datos);
            }
            return View();
        }

        public ActionResult ParametrosSimulador()
        {
            ViewBag.FactorVariable = ValorTablaBL.Obtener(x => x.TablaId == 3 && x.ItemId == 1).Valor;
            ViewBag.FactorFijo = ValorTablaBL.Obtener(x => x.TablaId == 3 && x.ItemId == 2).Valor;

            return View();
        }

        [HttpPost]
        public ActionResult ActualizarParametrosSimulador(string pFactorVariable, string pFactorFijo)
        {
            var fvar = ValorTablaBL.Obtener(x => x.TablaId == 3 && x.ItemId == 1);
            fvar.Valor = pFactorVariable;
            ValorTablaBL.Actualizar(fvar);

            var ffijo = ValorTablaBL.Obtener(x => x.TablaId == 3 && x.ItemId == 2);
            ffijo.Valor = pFactorFijo;
            ValorTablaBL.Actualizar(ffijo);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Simulador(int pProductoId = 1, string pTipo = "V", decimal pMonto = 0, int pCuotas = 0, decimal pInteres = 0,
            string pFecha = "", string pModalidad = "", decimal? pGastosAdm = null, string pGA = "CAP")
        {
            ViewBag.pMonto = pMonto;
            ViewBag.pCuotas = pCuotas;
            ViewBag.pInteres = pInteres;
            ViewBag.pProductoId = pProductoId;
            ViewBag.pProducto = ProductoBL.Obtener(pProductoId).Denominacion;
            ViewBag.pFecha = pFecha;
            ViewBag.pModalidadVal = pModalidad;

            var periodoAnio = 0.0;
            switch (pModalidad)
            {
                case "D": ViewBag.pModalidad = "DIARIO"; periodoAnio = 360.0; break;
                case "S": ViewBag.pModalidad = "SEMANAL"; periodoAnio = 52.0; break;
                case "Q": ViewBag.pModalidad = "QUINCENAL"; periodoAnio = 24.0; break;
                case "M": ViewBag.pModalidad = "MENSUAL"; periodoAnio = 12.0; break;
            }


            var pTem = pMonto > 0 ? Math.Pow(double.Parse((1 + pInteres / 100).ToString()), 1 / periodoAnio) - 1 : 0;
            ViewBag.TEM = Math.Round(pTem, 6);

            List<usp_SimuladorCredito_Result> oPlanPago = pMonto > 0
                    ? CreditoBL.SimuladorCredito(pMonto, pModalidad, pCuotas, pInteres, DateTime.Parse(pFecha), pGA == "CUO" ? pGastosAdm : 0)
                    : new List<usp_SimuladorCredito_Result>();

            return View(oPlanPago);
        }

        public ActionResult CrearSolicitudCredito(int pPersonaId)
        {
            return Json(CreditoBL.CrearSolicitudCredito(pPersonaId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarCredito(int pProductoId, string pTipoCuota, decimal pMontoInicial, decimal pMontoGastosAdm, string pIndGastosAdm,
                            decimal pMontoCredito, string pModalidad, int pNumerocuotas, decimal pInteresMensual, string pFecha, string pObservacion)
        {
            var rpta = CreditoBL.CrearCredito(VendixGlobal<int>.Obtener("SolicitudCreditoId"), pProductoId, pTipoCuota, pMontoInicial, pMontoGastosAdm, pIndGastosAdm,
                pMontoCredito, pModalidad, pNumerocuotas, pInteresMensual, DateTime.Parse(pFecha), pObservacion);
            return Json(rpta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AprobarCredito1ra(int pCreditoId)
        {
            var rpta = CreditoBL.AprobarCredito(pCreditoId, 0);
            return Json(rpta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AprobarCredito(int pCreditoId)
        {
            var rpta = CreditoBL.AprobarCredito(pCreditoId, 1);
            return Json(rpta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RechazarCredito(int pCreditoId)
        {
            return Json(CreditoBL.RechazarCredito(pCreditoId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReprogramarCredito(int pCreditoId)
        {
            return Json(CreditoBL.ReprogramarCredito(pCreditoId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GuardarCargo(int pCreditoId, int pTipoCargoId, decimal pMonto, string pDescripcion, bool pFinal)
        {
            var rpta = CargoBL.CrearCargo(pCreditoId, pTipoCargoId, pMonto, pDescripcion, pFinal);
            return Json(rpta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidarAnularCredito(int pCreditoId)
        {
            if (PlanPagoBL.Contar(x => x.Estado == "PAG" && x.CreditoId == pCreditoId) > 0)
                return Json(false, JsonRequestBehavior.AllowGet);

            var cant = CuentaxCobrarBL.Contar(x => x.CreditoId == pCreditoId);
            var cant1 = CuentaxCobrarBL.Contar(x => x.CreditoId == pCreditoId && (x.Estado == "ANU" || x.Estado == "PEN"));

            if (cant == cant1)
                return Json(true, JsonRequestBehavior.AllowGet);

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AnularCredito(int pCreditoId, string pObservacion)
        {
            return Json(CreditoBL.AnularCredito(pCreditoId, pObservacion), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerCredito(int pCreditoId)
        {
            return Json(CreditoBL.ObtenerDatoCredito(pCreditoId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerProducto(int pProductoId)
        {
            var rpta = ProductoBL.Obtener(pProductoId);
            return Json(rpta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarProductos()
        {
            var prod = ProductoBL.Listar(x => x.Estado).Select(x => new { Id = x.ProductoId, Valor = x.Denominacion });
            return Json(prod, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarTipoCargo()
        {
            var dta = ValorTablaBL.Listar(x => x.TablaId == 2 && x.ItemId > 0).Select(x => new { Id = x.ItemId, Valor = x.Denominacion });
            return Json(dta, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarCargoGrd(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstGrd = CargoBL.ListarCargoJGrid(request, ref totalRecords);

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.CargoId,
                            cell = new string[] {
                                                    item.CargoId.ToString(),
                                                    item.UsuarioCargo,
                                                    item.TipoCargo,
                                                    item.Importe.ToString(),
                                                    item.Descripcion,
                                                    item.NumCuota.ToString(),
                                                    item.Estado
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarCreditoPendienteGrd(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstGrd = CajaDiarioBL.LstCreditoPendienteJGrid(request, ref totalRecords);

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.CreditoId,
                            cell = new string[] {
                                                    item.CreditoId.ToString(),
                                                    item.Codigo,
                                                    item.Cliente,
                                                    item.MontoCredito.ToString(),
                                                    item.PersonaId.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CalcularGastosAdm(int pProductoId, decimal pMonto, bool pIncluyeCentralRiesgo)
        {
            return Json(GastosAdmBL.CalcularGastosAdm(pMonto, pIncluyeCentralRiesgo), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerFechaPrimerPago(string pModalidad)
        {
            var fecha = VendixGlobal.GetFecha();
            switch (pModalidad)
            {
                case "D": fecha = fecha.AddDays(1); break;
                case "S": fecha = fecha.AddDays(7); break;
                case "Q": fecha = fecha.AddDays(15); break;
                case "M": fecha = fecha.AddMonths(1); break;
            }
            return Json(fecha.ToString("dd/MM/yyyy"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarCreditosGrd(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstGrd = CreditoBL.ListarCreditosGrd(request, ref totalRecords);

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.CreditoId,
                            cell = new string[] {
                                                    item.CreditoId.ToString(),
                                                    item.MontoProducto.ToString(),
                                                    item.MontoInicial.ToString(),
                                                    item.MontoCredito.ToString(),
                                                    item.FormaPago,
                                                    item.NumeroCuotas.ToString(),
                                                    item.Estado=="PAG"?"CANCELADO":item.Estado
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarPlanPagoHistGrd(GridDataRequest request, int pCreditoId)
        {
            var lstGrd = PlanPagoBL.Listar(x => x.CreditoId == pCreditoId);
            int totalRecords = lstGrd.Count;
            lstGrd.Add(new PlanPago()
            {
                PlanPagoId = 0,
                Amortizacion = lstGrd.Sum(x => x.Amortizacion),
                Interes = lstGrd.Sum(x => x.Interes),
                GastosAdm = lstGrd.Sum(x => x.GastosAdm),
                ImporteMora = lstGrd.Sum(x => x.ImporteMora),
                InteresMora = lstGrd.Sum(x => x.InteresMora)
            });

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.PlanPagoId,
                            cell = new string[] {
                                                   item.PlanPagoId==0?String.Empty:item.Numero.ToString(),
                                                    item.PlanPagoId==0?String.Empty:item.Capital.ToString(),
                                                    item.PlanPagoId==0?"TOTAL:":item.FechaVencimiento.ToShortDateString(),
                                                    item.Amortizacion.ToString(),
                                                    item.Interes.ToString(),
                                                    item.GastosAdm.ToString(),
                                                    item.PlanPagoId==0?String.Empty:item.Cuota.ToString(),
                                                    item.ImporteMora.ToString(),
                                                    item.InteresMora.ToString(),
                                                    item.PlanPagoId==0?String.Empty:item.PagoCuota.ToString(),
                                                    item.Estado,
                                                    item.FechaPagoCuota.HasValue?item.FechaPagoCuota.Value.ToShortDateString():string.Empty
                                                }
                        }
                       ).ToArray()
            };

            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerCreditoPendiente(int pCreditoId)
        {
            var lstGrd = CreditoBL.ListarEstadoPlanPago(pCreditoId);
            var lstGrdPen = lstGrd.Where(x => x.Estado == "PEN").ToList();

            var pendiente = new usp_EstadoPlanPago_Result()
            {
                Amortizacion = lstGrdPen.Sum(x => x.Amortizacion),
                Interes = lstGrdPen.Sum(x => x.Interes) + lstGrdPen.Sum(x => x.GastosAdm),
                ImporteMora = lstGrdPen.Sum(x => x.ImporteMora) + lstGrdPen.Sum(x => x.InteresMora),
                Cargo = lstGrdPen.Sum(x => x.Cargo)
            };

            return Json(pendiente, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarPlanPagoActGrd(GridDataRequest request)
        {
            var lstGrd = CreditoBL.ListarEstadoPlanPago(int.Parse(request.DataFilters()["pCreditoId"]));
            int totalRecords = lstGrd.Count;

            var lstGrdPen = lstGrd.Where(x => x.Estado == "PAG").ToList();
            lstGrd.Add(new usp_EstadoPlanPago_Result()
            {
                PlanPagoId = -1,
                Amortizacion = lstGrdPen.Sum(x => x.Amortizacion),
                Interes = lstGrdPen.Sum(x => x.Interes),
                GastosAdm = lstGrdPen.Sum(x => x.GastosAdm),
                Cuota = lstGrdPen.Sum(x => x.Cuota),
                ImporteMora = lstGrdPen.Sum(x => x.ImporteMora),
                InteresMora = lstGrdPen.Sum(x => x.InteresMora),
                PagoCuota = lstGrdPen.Sum(x => x.PagoCuota)
            });
            lstGrdPen = lstGrd.Where(x => x.Estado == "PEN").ToList();
            lstGrd.Add(new usp_EstadoPlanPago_Result()
            {
                PlanPagoId = -2,
                Amortizacion = lstGrdPen.Sum(x => x.Amortizacion),
                Interes = lstGrdPen.Sum(x => x.Interes),
                GastosAdm = lstGrdPen.Sum(x => x.GastosAdm),
                Cuota = lstGrdPen.Sum(x => x.Cuota),
                ImporteMora = lstGrdPen.Sum(x => x.ImporteMora),
                InteresMora = lstGrdPen.Sum(x => x.InteresMora),
                PagoCuota = lstGrdPen.Sum(x => x.PagoCuota)
            });
            lstGrdPen = lstGrd.Where(x => x.Estado == "PEN" || x.Estado == "PAG" || x.Estado == "CRE").ToList();
            lstGrd.Add(new usp_EstadoPlanPago_Result()
            {
                PlanPagoId = 0,
                Amortizacion = lstGrdPen.Sum(x => x.Amortizacion),
                Interes = lstGrdPen.Sum(x => x.Interes),
                GastosAdm = lstGrdPen.Sum(x => x.GastosAdm),
                Cuota = lstGrdPen.Sum(x => x.Cuota),
                ImporteMora = lstGrdPen.Sum(x => x.ImporteMora),
                InteresMora = lstGrdPen.Sum(x => x.InteresMora),
                PagoCuota = lstGrdPen.Sum(x => x.PagoCuota)
            });

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.PlanPagoId,
                            cell = new string[] {
                                                   item.PlanPagoId<=0?String.Empty:item.Numero.ToString(),
                                                    item.PlanPagoId<=0?String.Empty:item.Capital.ToString(),
                                                    item.PlanPagoId>0?item.FechaVencimiento.ToShortDateString():(item.PlanPagoId==0?"TOTAL":(item.PlanPagoId==-1?"PAGADO":"PENDIENTE")),
                                                    item.Amortizacion.ToString(),
                                                    item.Interes.ToString(),
                                                    item.GastosAdm.ToString(),
                                                    item.Cuota.ToString(),
                                                    item.DiasAtrazo.ToString(),
                                                    item.ImporteMora.ToString(),
                                                    item.InteresMora.ToString(),
                                                    item.Cargo.ToString(),
                                                    item.PagoLibre.ToString(),
                                                    item.PagoCuota.ToString(),
                                                    item.Estado + "," + item.MovimientoCajaId.ToString(),
                                                    item.FechaPagoCuota.HasValue?item.FechaPagoCuota.Value.ToShortDateString():string.Empty
                                                }
                        }
                       ).ToArray()
            };

            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CondonarCredito(int pCreditoId, decimal pMontocxc, string pObs)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var cxc = CuentaxCobrarBL.Obtener(x => x.CreditoId == pCreditoId);
                    if (cxc == null)
                    {
                        CuentaxCobrarBL.Crear(new CuentaxCobrar
                        {
                            Operacion = "CDN",
                            Monto = pMontocxc,
                            Estado = "PEN",
                            CreditoId = pCreditoId
                        });
                    }
                    else
                    {
                        cxc.CreditoId = pCreditoId;
                        cxc.Operacion = "CDN";
                        cxc.Monto = pMontocxc;
                        cxc.Estado = "PEN";
                        CuentaxCobrarBL.Actualizar(cxc);
                    }

                    var c = CreditoBL.Obtener(pCreditoId);
                    c.Observacion = VendixGlobal.GetFecha().ToString() + " " + pObs;
                    CreditoBL.Actualizar(c);
                    scope.Complete();
                    return Json(true);
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return Json(ex.InnerException.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult ObservarCredito(int pCreditoId, string pObs)
        {
            var c = CreditoBL.Obtener(pCreditoId);
            c.Observacion = pObs;
            CreditoBL.Actualizar(c);
            return Json(true);
            //using (var scope = new TransactionScope())
            //{
            //    try
            //    {
            //        CreditoBL.ActualizarParcial(new Credito { CreditoId = pCreditoId, Observacion = pObs }, x => x.Observacion);
            //        scope.Complete();
            //        return Json(true);
            //    }
            //    catch (Exception ex)
            //    {
            //        scope.Dispose();
            //        return Json(ex.InnerException.Message);
            //    }
            //}
        }

        #region Cajadiario
        public ActionResult CajaDiario()
        {
            var oficinaId = VendixGlobal.GetOficinaId();

            // ViewBag.cboBovedas = new SelectList(BovedaBL.ListaBovedasXOficina(oficinaId), "BovedaId", "Denominacion");

            var usuarioId = VendixGlobal.GetUsuarioId();
            var cajadiario = CajaDiarioBL.Obtener(x => x.UsuarioAsignadoId == usuarioId && x.IndCierre == false, includeProperties: "Caja");
            if (cajadiario != null)
                VendixGlobal<int>.Crear("CajadiarioId", cajadiario.CajaDiarioId);

            ViewBag.cboTipoOperacion = new SelectList(TipoOperacionBL.Listar(x => x.IndCajaDiario), "TipoOperacionId", "Denominacion");

            return View(cajadiario);

            //return View(new DatosCajaDiario() { CajaDiario = cajadiario, Entradas = resumen[0], Salidas = resumen[1] });
        }

        public ActionResult ExisteCxCPendientes(int pPersonaId)
        {
            var usuarioid = VendixGlobal.GetUsuarioId();
            var nro = OrdenVentaBL.Contar(x => x.PersonaId == pPersonaId && x.TipoVenta == "CON" && x.Estado == "ENV");
            if (pPersonaId == 0)
                nro = nro + CuentaxCobrarBL.Contar(x => x.Credito.UsuarioRegId == usuarioid && x.Estado == "PEN");
            else
                nro = nro + CuentaxCobrarBL.Contar(x => x.Credito.PersonaId == pPersonaId && x.Estado == "PEN");


            return Json(nro, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarCuentasxCobrarJGrid(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = CajaDiarioBL.LstCuentasxCobrarJGrid(request, ref totalRecords);

            var data = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.Id,
                            cell = new string[] {
                                                    item.OrdenVentaId.ToString(),
                                                    item.CuentaxCobrarId.ToString(),
                                                    item.Codigo,
                                                    item.Cliente,
                                                    ObtenerOp(item.Operacion),
                                                    item.Origen,
                                                    item.Monto.ToString(),
                                                    item.Estado,
                                                    item.FechaReg.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarDesembolsoJGrid(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = CajaDiarioBL.LstDesembolsoJGrid(request, ref totalRecords);

            var data = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.CreditoId,
                            cell = new string[] {
                                                    item.CreditoId.ToString(),
                                                    item.Persona.Codigo,
                                                    item.Persona.NombreCompleto,
                                                    item.MontoCredito.ToString(),
                                                    item.MontoGastosAdm.ToString(),
                                                    item.MontoDesembolso.ToString(),
                                                    item.Estado
                                                }
                        }
                       ).ToArray()
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        private String ObtenerOp(string pOp)
        {
            switch (pOp)
            {
                case "CON": return "PAGO AL CONTADO";
                case "INI": return "PAGO INICIAL";
            }
            return pOp;
        }

        public ActionResult ListarCuotasPendientesJGrid(GridDataRequest request)
        {
            int totalRecords = 0; string totales = string.Empty;
            var lstItem = CajaDiarioBL.LstCuotasPendientesJGrid(request, ref totalRecords, ref totales);
            VendixGlobal<string>.Crear("TotalesCuotasPendientes", totales);
            var data = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.PlanPagoId,
                            cell = new string[] {
                                                    item.PlanPagoId.ToString(),
                                                    item.Glosa,
                                                    item.FechaVencimiento.ToShortDateString(),
                                                    item.Amortizacion.ToString(),
                                                    item.Interes.ToString(),
                                                    item.GastosAdm.ToString(),
                                                    item.Cuota.ToString(),
                                                    item.DiasAtrazo.ToString(),
                                                    item.ImporteMora.ToString(),
                                                    item.InteresMora.ToString(),
                                                    item.PagoLibre.ToString(),
                                                    item.PagoCuota.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerTotalesCuotasPendientes()
        {
            return Json(VendixGlobal<string>.Obtener("TotalesCuotasPendientes"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PagarCuotas(int pCreditoId, string pPlanPago, decimal pImporteRecibido)
        {
            return Json(CajaDiarioBL.PagarCuotas(VendixGlobal.GetCajaDiarioId(), pCreditoId, pPlanPago, pImporteRecibido),
                     JsonRequestBehavior.AllowGet);
        }

        public ActionResult TieneCxcPendiente(int pCreditoId)
        {
            var nro = CuentaxCobrarBL.Contar(x => x.CreditoId == pCreditoId && x.Estado == "PEN");
            return Json(nro > 0, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PagarCuotasImporteLibre(int pCreditoId, decimal pImporteLibre)
        {
            return Json(CajaDiarioBL.PagarCuotas(VendixGlobal.GetCajaDiarioId(), pCreditoId, string.Empty, pImporteLibre),
                     JsonRequestBehavior.AllowGet);
        }
        public ActionResult PagarCuotasCancelacion(int pCreditoId)
        {
            return Json(CajaDiarioBL.PagarCuotasCancelacion(VendixGlobal.GetCajaDiarioId(), pCreditoId),
                     JsonRequestBehavior.AllowGet);
        }
        public ActionResult ObtenerCajaDiario()
        {
            var cajadiarioid = VendixGlobal<int>.Obtener("CajadiarioId");
            return Json(CajaDiarioBL.Obtener(cajadiarioid), JsonRequestBehavior.AllowGet);
        }
        public ActionResult RealizarEntradaSalidaCajaDiario(int pPersonaId, int pTipoOperacionId, string pDescripcion, decimal pImporte)
        {
            return Json(CajaDiarioBL.EntradaSalida(pPersonaId, pTipoOperacionId, pDescripcion, pImporte)
                    , JsonRequestBehavior.AllowGet);
        }
        public ActionResult RealizarDesembolso(int pCreditoId)
        {
            return Json(CajaDiarioBL.RealizarDesembolso(pCreditoId)
                    , JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidarDesembolso(int pCreditoId)
        {
            if (CuentaxCobrarBL.Contar(x => x.CreditoId == pCreditoId && x.Estado == "PEN") > 0)
                return Json(new { error = true, mensaje = "Tiene Cuentas por cobrar Pendientes!" }, JsonRequestBehavior.AllowGet);

            var pImporte = CreditoBL.Obtener(pCreditoId).MontoDesembolso;
            if (pImporte > CajaDiarioBL.Obtener(VendixGlobal.GetCajaDiarioId()).SaldoFinal)
                return Json(new { error = true, mensaje = "Saldo Insuficiente!" }, JsonRequestBehavior.AllowGet);

            return Json(new { error = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RealizarPagarCuentaxCobrar(int pOrdenVentaId, int pCuentaxCobrarId)
        {
            return Json(CajaDiarioBL.RealizarPagarCuentaxCobrar(pOrdenVentaId, pCuentaxCobrarId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CerrarCajaDiario()
        {
            return Json(CajaDiarioBL.CerrarCajaDiario(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarCreditosPendientesCombo(int pPersonaId)
        {
            var userid = VendixGlobal.GetUsuarioId();
            var olstCredito = CreditoBL
                .Listar(x => x.Estado == "DES" && x.PersonaId == pPersonaId && x.UsuarioRegId == userid)
                .Select(x => new
                {
                    Id = x.CreditoId,
                    Valor = "Credito " + x.CreditoId.ToString() + " - " + x.Descripcion.Replace("\"", ""),
                    MontoCredito = x.MontoCredito
                });

            return Json(olstCredito, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarMovimientosCajaJGrid(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = CajaDiarioBL.LstMovimientosCajaJGrid(request, ref totalRecords);
            var data = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.MovimientoCajaId,
                            cell = new string[] {
                                                    item.MovimientoCajaId.ToString(),
                                                    item.Estado?item.MovimientoCajaId.ToString():"",
                                                    item.CajaDiarioId.ToString(),
                                                    item.FechaReg.ToString(),
                                                    item.IndEntrada?"ENTRADA":"SALIDA",
                                                    item.Persona,
                                                    item.Operacion,
                                                    item.Descripcion,
                                                    item.ImportePago.ToString(),
                                                    item.Estado?"ACTIVO":"ANULADO",
                                                    item.Estado?item.MovimientoCajaId.ToString():""
                                                }
                        }
                       ).ToArray()
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidarAnularMovimientoCaja(int pMovimientoCajaId)
        {
            var operacion = MovimientoCajaBL.Obtener(pMovimientoCajaId).Operacion;
            if (operacion == "INI")
            {
                int pCreditoId = CuentaxCobrarBL.Obtener(x => x.MovimientoCajaId == pMovimientoCajaId).CreditoId.Value;
                if (PlanPagoBL.Contar(x => x.Estado == "PAG" && x.CreditoId == pCreditoId) > 0)
                    return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AnularMovimientoCaja(int pMovimientoCajaId, string pObservacion)
        {
            return Json(CajaDiarioBL.AnularMovimientoCaja(pMovimientoCajaId, pObservacion), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExisteDesembolso(int pPersonaId)
        {
            return Json(CreditoBL.Contar(x => x.PersonaId == pPersonaId && x.Estado == "APR"), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class DatosCredito
    {
        public Persona Persona { get; set; }
        public Cliente Cliente { get; set; }
        public int TotalCreditos { get; set; }
        public string EstadoCliente { get; set; }
        public string Analista { get; set; }
        public string CalificacionCliente { get; set; }
        public Credito SolicitudCredito { get; set; }
        public Producto Producto { get; set; }
        public List<Credito> Creditos { get; set; }
    }




}
