
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
    
public partial class OrdenVenta
{

    public OrdenVenta()
    {

        this.MovimientoCaja = new HashSet<MovimientoCaja>();

        this.TarjetaPuntoDet = new HashSet<TarjetaPuntoDet>();

        this.OrdenVentaDet = new HashSet<OrdenVentaDet>();

        this.Credito = new HashSet<Credito>();

    }


    public int OrdenVentaId { get; set; }

    public int OficinaId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TotalImpuesto { get; set; }

    public decimal TotalNeto { get; set; }

    public decimal TotalDescuento { get; set; }

    public string Estado { get; set; }

    public int UsuarioRegId { get; set; }

    public System.DateTime FechaReg { get; set; }

    public Nullable<int> UsuarioModId { get; set; }

    public Nullable<System.DateTime> FechaMod { get; set; }

    public int PersonaId { get; set; }

    public Nullable<int> MovimientoAlmacenId { get; set; }

    public string TipoVenta { get; set; }



    public virtual ICollection<MovimientoCaja> MovimientoCaja { get; set; }

    public virtual Oficina Oficina { get; set; }

    public virtual Usuario Usuario { get; set; }

    public virtual Usuario Usuario1 { get; set; }

    public virtual ICollection<TarjetaPuntoDet> TarjetaPuntoDet { get; set; }

    public virtual ICollection<OrdenVentaDet> OrdenVentaDet { get; set; }

    public virtual ICollection<Credito> Credito { get; set; }

    public virtual Persona Persona { get; set; }

}

}
