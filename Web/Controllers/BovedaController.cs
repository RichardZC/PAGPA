using System;
using System.Linq;
using System.Web.Mvc;
using ITB.VENDIX.BL;

namespace VendixWeb.Controllers
{
    public class BovedaController : Controller
    {
        //
        // GET: /Boveda/

        public ActionResult Index()
        {
            var oficinaid = VendixGlobal.GetOficinaId();

            ViewBag.cboCajas = new SelectList(CajaBL.ListarCajasAbiertas(), "id", "value");

            var oficinaId = VendixGlobal.GetOficinaId();
            ViewBag.cboOficinas = new SelectList(OficinaBL.Listar(x => x.Estado && x.OficinaId != oficinaId), "OficinaId", "Denominacion");

            return View(BovedaBL.Obtener(x => x.OficinaId == oficinaid && x.IndCierre == false));
        }

        public ActionResult ListarBovedaJgrid(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = BovedaBL.LstBovedaJGrid(request, ref totalRecords);
            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.BovedaId,
                            cell = new string[] {
                                                    item.BovedaId.ToString(),
                                                    item.SaldoInicial.ToString(),
                                                    item.Entradas.ToString(),
                                                    item.Salidas.ToString(),
                                                    item.SaldoFinal.ToString(),
                                                    item.FechaIniOperacion.ToString(),
                                                    item.FechaFinOperacion.HasValue?item.FechaFinOperacion.Value.ToString():"",
                                                    item.IndCierre?"SI":"NO"
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListarBovedaMovJgrid(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstItem = BovedaBL.LstBovedaMovJGrid(request, ref totalRecords);
            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstItem
                        select new
                        {
                            id = item.MovimientoBovedaId,
                            cell = new string[] {
                                                    item.MovimientoBovedaId.ToString(),
                                                    item.BovedaId.ToString(),
                                                    item.CajaDiarioId.ToString(),
                                                    item.FechaReg.ToString(),
                                                    item.CodOperacion,
                                                    item.Glosa,
                                                    item.Importe.ToString()
                                                   
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Cerrar()
        {
            return Json(BovedaBL.Cerrar(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TransferirCaja(decimal pImporte, string pDescripcion, int pCboId)
        {
             

            var rspta = BovedaMovBL.TransferirBovedaCaja(pImporte, pDescripcion, pCboId);

            return Json(rspta, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TransferirOficina(decimal pImporte, string pDescripcion, int pCboId)
        {
            var oficinaId = VendixGlobal.GetOficinaId();
            var bovedaInicioId = BovedaBL.Listar(x => x.OficinaId == oficinaId && x.IndCierre == false).FirstOrDefault().BovedaId;
            var pUsuarRegId = VendixGlobal.GetUsuarioId();

            var rpta = BovedaMovBL.TransferiraOficina(pImporte, pDescripcion, bovedaInicioId, pCboId, pUsuarRegId);

            return Json(rpta, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmarTransferencia(int pBovedaMovTempId, int pFlag)
        {
            var rpta = BovedaMovBL.TransferiraOficina(pBovedaMovTempId, pFlag);

            return Json(rpta, JsonRequestBehavior.AllowGet);
        }

    }

}
