
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
    
public partial class usp_RptCreditoRentabilidad_Result
{

    public int CreditoId { get; set; }

    public string Oficina { get; set; }

    public string Cliente { get; set; }

    public Nullable<System.DateTime> FechaDesembolso { get; set; }

    public int NumeroCuotas { get; set; }

    public decimal Interes { get; set; }

    public string FormaPago { get; set; }

    public string Estado { get; set; }

    public decimal MontoProducto { get; set; }

    public decimal MontoInicial { get; set; }

    public decimal MontoCredito { get; set; }

    public decimal MontoGastosAdm { get; set; }

    public Nullable<int> CuotasPagadas { get; set; }

    public Nullable<decimal> SumAmortizacion { get; set; }

    public Nullable<decimal> SumGastosAdm { get; set; }

    public Nullable<decimal> SumInteres { get; set; }

    public Nullable<decimal> SumCuota { get; set; }

    public Nullable<decimal> SumMora { get; set; }

    public Nullable<decimal> SumPago { get; set; }

}

}
