
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
    
public partial class TarjetaPuntoDet
{

    public int TarjetaPuntoDetId { get; set; }

    public int TarjetaPuntoId { get; set; }

    public int OrdenVentaId { get; set; }

    public int ValorCanje { get; set; }



    public virtual OrdenVenta OrdenVenta { get; set; }

    public virtual TarjetaPunto TarjetaPunto { get; set; }

}

}
