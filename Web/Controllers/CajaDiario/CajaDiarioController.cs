using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITB.VENDIX.BE;
using ITB.VENDIX.BL;
using Web.Models;
using Helper;

namespace VendixWeb.Controllers.CajaDiario
{
    [Autenticado]
    public class CajaDiarioController : Controller
    {
        [HttpPost]
        public ActionResult MostrarMontoCaja()
        {
            var ofid = VendixGlobal.GetOficinaId();
            var cajaDiarioId = VendixGlobal.GetCajaDiarioId();

            var monto = CajaDiarioBL.Obtener(x => x.CajaDiarioId == cajaDiarioId && x.IndCierre == false).SaldoFinal;

            return Json(monto, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TransferirBoveda(decimal pMonto, string pDescripcion)
        {
            var boveda = BovedaBL.Obtener(VendixGlobal.GetBovedaId());

            if (boveda.IndCierre == false)
            {
                var oficinaId = VendixGlobal.GetOficinaId();
                var pUsuarRegId = VendixGlobal.GetUsuarioId();
                var pCajaDiarioId = VendixGlobal.GetCajaDiarioId();               

                var rspta = CajaDiarioBL.TransferirSaldosBoveda(pMonto, pDescripcion, pCajaDiarioId, boveda.BovedaId, oficinaId, pUsuarRegId);

                return Json(rspta, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerMovimientoCajaAnular(int pMovimientoCajaId)
        {
            var rpta = new Respuesta() { Error = false };
            var movcaja = MovimientoCajaBL.Obtener(x=>x.MovimientoCajaId== pMovimientoCajaId,includeProperties:"CajaDiario");
            if (movcaja== null)
            {
                rpta.Error = true;
                rpta.Mensaje = "Movimiento no encontrado!";
                return Json(new { rpta = rpta }, JsonRequestBehavior.AllowGet);
            }
            if (movcaja.Estado==false)
            {
                rpta.Error = true;
                rpta.Mensaje = "El movimiento se encuentra Anulado!";
                return Json(new { rpta = rpta }, JsonRequestBehavior.AllowGet);
            }
            if (movcaja.CajaDiario.IndCierre == true)
            {
                rpta.Error = true;
                rpta.Mensaje = "Caja Diario Cerrado, no se puede anular!";
                return Json(new { rpta = rpta }, JsonRequestBehavior.AllowGet);
            }


            rpta.Valor = movcaja.FechaReg.ToShortDateString();
            rpta.Valor1 = UsuarioBL.ObtenerNombre(movcaja.CajaDiario.UsuarioAsignadoId);
            movcaja.CajaDiario = null;
            return Json(new { rpta = rpta, val = movcaja }, JsonRequestBehavior.AllowGet);
        }
    }

}

