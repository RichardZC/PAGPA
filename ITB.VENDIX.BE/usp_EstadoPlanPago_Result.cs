
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
    
public partial class usp_EstadoPlanPago_Result
{

    public int PlanPagoId { get; set; }

    public int Numero { get; set; }

    public decimal Capital { get; set; }

    public System.DateTime FechaVencimiento { get; set; }

    public decimal Amortizacion { get; set; }

    public decimal Interes { get; set; }

    public decimal GastosAdm { get; set; }

    public decimal Cuota { get; set; }

    public string Estado { get; set; }

    public Nullable<int> DiasAtrazo { get; set; }

    public Nullable<decimal> ImporteMora { get; set; }

    public Nullable<decimal> InteresMora { get; set; }

    public Nullable<decimal> Cargo { get; set; }

    public Nullable<decimal> PagoLibre { get; set; }

    public Nullable<System.DateTime> FechaPagoCuota { get; set; }

    public Nullable<decimal> PagoCuota { get; set; }

    public Nullable<int> MovimientoCajaId { get; set; }

}

}