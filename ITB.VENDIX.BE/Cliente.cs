
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
    
public partial class Cliente
{

    public int ClienteId { get; set; }

    public int PersonaId { get; set; }

    public Nullable<int> ActividadEconId { get; set; }

    public string Calificacion { get; set; }

    public Nullable<System.DateTime> FechaRegistro { get; set; }

    public bool Estado { get; set; }

    public string Nota { get; set; }

    public string DireccionNegocio { get; set; }

    public string DireccionNegocioRef { get; set; }

    public string Aval { get; set; }

    public string AvalDNI { get; set; }

    public string AvalCelular { get; set; }

    public int UsuarioRegId { get; set; }



    public virtual Persona Persona { get; set; }

}

}
