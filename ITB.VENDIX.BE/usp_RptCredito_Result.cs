
//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ITB.VENDIX.BE
{

using System;
    
public partial class usp_RptCredito_Result
{

    public string Producto { get; set; }

    public string Cliente { get; set; }

    public int CreditoId { get; set; }

    public Nullable<System.DateTime> FechaDesembolso { get; set; }

    public Nullable<System.DateTime> FechaVcto { get; set; }

    public string FormaPago { get; set; }

    public int NumeroCuotas { get; set; }

    public decimal Interes { get; set; }

    public string Estado { get; set; }

    public decimal MontoProducto { get; set; }

    public decimal MontoInicial { get; set; }

    public decimal MontoCredito { get; set; }

    public string TipoGastoAdm { get; set; }

    public decimal MontoGastosAdm { get; set; }

    public decimal MontoDesembolso { get; set; }

}

}