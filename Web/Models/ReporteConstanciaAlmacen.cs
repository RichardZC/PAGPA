using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ITB.VENDIX.BE;
using ITB.VENDIX.BL;

namespace VendixWeb.Models
{
    public class ReporteConstanciaAlmacen
    {
        public MovimientoBL.EntradaSalida Cabecera { get; set; }
        public List<MovimientoDet> Detalle { get; set; }
     }
}