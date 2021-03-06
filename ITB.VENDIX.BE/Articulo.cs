
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
    
public partial class Articulo
{

    public Articulo()
    {

        this.MovimientoDet = new HashSet<MovimientoDet>();

        this.OrdenVentaDet = new HashSet<OrdenVentaDet>();

        this.ListaPrecio = new HashSet<ListaPrecio>();

        this.SerieArticulo = new HashSet<SerieArticulo>();

    }


    public int ArticuloId { get; set; }

    public Nullable<int> ModeloId { get; set; }

    public Nullable<int> TipoArticuloId { get; set; }

    public string CodArticulo { get; set; }

    public string Denominacion { get; set; }

    public string Descripcion { get; set; }

    public string Imagen { get; set; }

    public Nullable<bool> IndPerecible { get; set; }

    public Nullable<bool> IndImportado { get; set; }

    public Nullable<bool> IndCanjeable { get; set; }

    public bool Estado { get; set; }



    public virtual Modelo Modelo { get; set; }

    public virtual TipoArticulo TipoArticulo { get; set; }

    public virtual ICollection<MovimientoDet> MovimientoDet { get; set; }

    public virtual ICollection<OrdenVentaDet> OrdenVentaDet { get; set; }

    public virtual ICollection<ListaPrecio> ListaPrecio { get; set; }

    public virtual ICollection<SerieArticulo> SerieArticulo { get; set; }

}

}
