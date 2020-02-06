
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
    using System.Collections.Generic;
    
public partial class CajaDiario
{

    public CajaDiario()
    {

        this.MovimientoCaja = new HashSet<MovimientoCaja>();

    }


    public int CajaDiarioId { get; set; }

    public int CajaId { get; set; }

    public int UsuarioAsignadoId { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal Entradas { get; set; }

    public decimal Salidas { get; set; }

    public decimal SaldoFinal { get; set; }

    public System.DateTime FechaIniOperacion { get; set; }

    public Nullable<System.DateTime> FechaFinOperacion { get; set; }

    public bool IndCierre { get; set; }

    public bool TransBoveda { get; set; }

    public decimal MontoPorCobrar { get; set; }

    public decimal MontoCobrado { get; set; }



    public virtual Caja Caja { get; set; }

    public virtual Usuario Usuario { get; set; }

    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; }

}

}