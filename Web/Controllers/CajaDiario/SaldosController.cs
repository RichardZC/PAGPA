using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITB.VENDIX.BL;
using Helper;

namespace VendixWeb.Controllers.CajaDiario
{
    [Autenticado]
    public class SaldosController : Controller
    {
        //
        // GET: /Saldos/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListarSaldoCajaDiario(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = CajaDiarioBL.LstSaldosCajaDiarioJGrid(request, ref totalRecords);
            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.CajaDiarioId,
                            cell = new string[] { 
                                                    item.CajaDiarioId.ToString(),
                                                    item.Caja.Denominacion,
                                                    item.Usuario.NombreUsuario,
                                                    item.SaldoInicial.ToString(),
                                                    item.SaldoFinal.ToString(),
                                                    item.FechaIniOperacion.ToString(),
                                                    item.FechaFinOperacion.ToString(),
                                                    item.IndCierre ? "SI":"NO",
                                                    item.TransBoveda ? "SI":"NO",
                                                    item.CajaDiarioId.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarSaldoCajaDiarioBoveda(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = CajaDiarioBL.LstSaldosBovedaCajaDiarioJGrid(request, ref totalRecords);
            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.CajaDiarioId,
                            cell = new string[] {
                                                    item.CajaDiarioId.ToString(),
                                                    item.Caja,
                                                    item.Usuario,
                                                    item.SaldoInicial.ToString(),
                                                    item.SaldoFinal.ToString(),
                                                    item.FechaIniOperacion.ToString(),
                                                    item.FechaFinOperacion.ToString(),
                                                    item.IndCierre ? "SI":"NO",
                                                    item.TransBoveda ? "SI":"NO",
                                                    item.CajaDiarioId.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarCajasAsignadas(GridDataRequest request)
        {
            var lstGrd = CajaBL.LstCajaDiarioOficina();

            var productsData = new
            {
                total = (int)Math.Ceiling((float)lstGrd.Count / (float)request.rows),
                page = request.page,
                records = lstGrd.Count,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.CajaDiarioId,
                            cell = new string[] { 
                                                    item.CajaDiarioId.ToString(),
                                                    item.NombreCaja,
                                                    item.IndCierre ? "CERRADO":"ABIERTO",
                                                    item.Cajero,
                                                    item.FechaIniOperacion.ToString(),
                                                    item.FechaFinOperacion.HasValue?item.FechaFinOperacion.Value.ToString():string.Empty,
                                                    item.SaldoInicial.ToString(),
                                                    item.Entradas.ToString(),
                                                    item.Salidas.ToString(),
                                                    item.SaldoFinal.ToString()
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtnerListaCajasCombo()
        {
            var olistaCajas = CajaBL.Listar(x => x.Estado  && x.IndAbierto == false).Select(x => new { Id = x.CajaId, Valor = x.Denominacion });
            return Json(olistaCajas, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtnerListaUsuariosCombo()
        {
            var olistaUsuarios = CajaBL.ListaUsuariosNoAsignado();
            return Json(olistaUsuarios, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AsignarCaja(int pCajaId, int pUsuarioAsignadoId, decimal pSaldoInicial)
        {
            return Json(
                CajaDiarioBL.AsignarUsuarioCaja(pCajaId, pUsuarioAsignadoId, pSaldoInicial)
                , JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Transferir()
        {
            return Json(CajaDiarioBL.TransferirCajaDiarioBoveda(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidarCierre()
        {
            var oficinaid = VendixGlobal.GetOficinaId();
            if (CajaDiarioBL.Contar(x => x.IndCierre == false && x.TransBoveda == false && x.Caja.OficinaId == oficinaid, "Caja") > 0)
                return Json("EXISTEN CAJAS ABIERTAS.", JsonRequestBehavior.AllowGet);

            if (CajaDiarioBL.Contar(x => x.IndCierre && x.TransBoveda == false && x.Caja.OficinaId == oficinaid, "Caja") == 0)
                return Json("NO EXISTEN CAJAS POR CERRAR.", JsonRequestBehavior.AllowGet);

            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult MostrarMontoBoveda()
        {
            var ofid = VendixGlobal.GetOficinaId();
            var monto = BovedaBL.Obtener(x => x.OficinaId == ofid && x.IndCierre == false).SaldoFinal;

            return Json(monto, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MostrarDetalleMovCaja(int pMovimientoCajaId)
        {
            var xxx = CajaDiarioBL.MostrarDetalleOvMovCaja(pMovimientoCajaId);
            return Json(xxx, JsonRequestBehavior.AllowGet);
        }
    
    
    }
}
