using System;
using System.Linq;
using System.Web.Mvc;
using ITB.VENDIX.BE;
using ITB.VENDIX.BL;
using Helper;

namespace Web.Controllers
{
    [Autenticado]
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.cboOcupacion = new SelectList(OcupacionBL.Listar(x => (bool)x.Estado), "OcupacionId", "Denominacion");
            return View();
        }
        public ActionResult ListarCliente(GridDataRequest request)
        {
            int totalRecords = 0;
            var lstGrd = ClienteBL.LstClienteJGrid(request, ref totalRecords);

            var productsData = new
            {
                total = (int)Math.Ceiling((float)totalRecords / (float)request.rows),
                page = request.page,
                records = totalRecords,
                rows = (from item in lstGrd
                        select new
                        {
                            id = item.PersonaId,
                            cell = new string[] {
                                                    item.PersonaId.ToString(),
                                                    item.Codigo,
                                                    item.Cliente,
                                                    item.Documento,
                                                    item.Email,
                                                    item.Celular,
                                                    item.Direccion
                                                }
                        }
                       ).ToArray()
            };
            return Json(productsData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Guardar(Persona p, Cliente c, string chkActivo,string txtOtros)
        {
            p.Estado = true;
            c.Estado = string.IsNullOrEmpty(chkActivo) ? false : true;
            p.Codigo = p.Codigo.Trim().ToUpper();
            try
            {
                if (!string.IsNullOrEmpty(txtOtros))
                {
                    var objNuevaOcupacion = new Ocupacion() { Denominacion = txtOtros, Estado = true };
                    OcupacionBL.Crear(objNuevaOcupacion);
                    c.ActividadEconId = objNuevaOcupacion.OcupacionId; 
                }

                if (p.PersonaId > 0)
                {
                    c.PersonaId = p.PersonaId;
                    var persona = PersonaBL.Obtener(p.PersonaId);
                    p.TipoPersona = persona.TipoPersona;
                    //p.NumeroDocumento = persona.NumeroDocumento;
                }
                if (p.TipoPersona == "N")
                {
                    p.TipoDocumento = "DNI";
                    p.NombreCompleto = p.ApePaterno + " " + p.ApeMaterno + ", " + p.Nombre;
                    p.TipoPersona = "N";
                }
                else
                {
                    p.TipoPersona = "J";
                    p.TipoDocumento = "RUC";
                    p.NombreCompleto = p.Nombre;
                }

                PersonaBL.Guardar(p);

                if (c.ClienteId > 0)
                {
                    var cliente = ClienteBL.Obtener(c.ClienteId);
                    c.FechaRegistro = cliente.FechaRegistro;
                    c.Calificacion = cliente.Calificacion;
                    c.UsuarioRegId = cliente.UsuarioRegId;
                }
                else
                {
                    c.FechaRegistro = VendixGlobal.GetFecha();
                    c.Calificacion = "A";
                    c.UsuarioRegId = VendixGlobal.GetUsuarioId();
                }
                c.ClienteId = p.PersonaId;
                c.PersonaId = p.PersonaId;
                ClienteBL.Guardar(c);
                return RedirectToAction("Index", "Cliente");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GuardarCliente(int pClienteId, string pTipoPersona, string pNombre, string pApePaterno, string pApeMaterno,
                                        string pNumeroDocumento, bool pSexoM, string pEmail, string pCelular1, string pNota,
                                        DateTime? pFechaNacimiento, string pDireccion, DateTime? pFechaCaptacion, int? pOcupacionId,
                                        string pCalificacion, decimal? pMontoInscripcion, bool pActivo)
        {

            var cliente = new Cliente();
            var persona = PersonaBL.Obtener(x => x.NumeroDocumento == pNumeroDocumento);
            if (persona == null)
                persona = new Persona();

            //if (pClienteId > 0)
            //{
            //    cliente = ClienteBL.Obtener(pClienteId);
            //    persona = PersonaBL.Obtener(cliente.PersonaId);
            //}

            pNombre = pNombre.ToUpper();
            if (pTipoPersona == "N")
            {
                pApePaterno = pApePaterno.ToUpper();
                pApeMaterno = pApeMaterno.ToUpper();
                persona.NombreCompleto = pApePaterno + " " + pApeMaterno + ", " + pNombre;
                persona.TipoDocumento = "DNI";

            }
            else
            {
                pApePaterno = string.Empty;
                pApeMaterno = string.Empty;
                persona.NombreCompleto = pNombre;
                persona.TipoDocumento = "RUC";
            }

            persona.TipoPersona = pTipoPersona;
            persona.Nombre = pNombre;
            persona.ApePaterno = pApePaterno;
            persona.ApeMaterno = pApeMaterno;
            persona.NumeroDocumento = pNumeroDocumento;
            persona.Sexo = pSexoM ? "M" : "F";
            persona.EmailPersonal = pEmail;
            persona.Celular1 = pCelular1;
            persona.FechaNacimiento = pFechaNacimiento;
            persona.Direccion = pDireccion;
            persona.Estado = pActivo;

            if (persona.PersonaId == 0)
                PersonaBL.Crear(persona);
            else
                PersonaBL.Actualizar(persona);

            //if (pClienteId == 0)
            //    PersonaBL.Crear(persona);
            //else
            //    PersonaBL.Actualizar(persona);
            if (pClienteId > 0)
                cliente = ClienteBL.Obtener(pClienteId);

            cliente.PersonaId = persona.PersonaId;
            cliente.FechaRegistro = VendixGlobal.GetFecha();
            //cliente.FechaCaptacion = pFechaCaptacion;
            cliente.ActividadEconId = pOcupacionId;
            cliente.Calificacion = pCalificacion;
            //cliente.Inscripcion = pMontoInscripcion.Value;
            //cliente.IndPagoInscripcion = false;
            cliente.Estado = pActivo;
            cliente.Nota = pNota;
            if (pClienteId == 0)
                ClienteBL.Crear(cliente);
            else
                ClienteBL.Actualizar(cliente);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ObtenerClientePersona(int pClienteId)
        {
            var cliente = ClienteBL.Listar(x => x.ClienteId == pClienteId).FirstOrDefault();
            var persona = PersonaBL.Listar(x => x.PersonaId == cliente.PersonaId).FirstOrDefault();

            return Json(new
            {
                Cliente = cliente,
                Persona = persona,
                Sexo = persona.Sexo != "F" ? "true" : "false",
                FNacimiento = persona.FechaNacimiento != null ? persona.FechaNacimiento.Value.ToShortDateString() : string.Empty
                //FCaptacion = cliente.FechaCaptacion != null ? cliente.FechaCaptacion.Value.ToShortDateString() : string.Empty
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExistePersonaDocumento(string pDocumento)
        {

            var persona = PersonaBL.Listar(x => x.NumeroDocumento == pDocumento).FirstOrDefault();
            if (persona == null)
                return Json(false, JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Activar(int pid)
        {
            var oCliente = ClienteBL.Obtener(pid);
            oCliente.Estado = !oCliente.Estado;
            ClienteBL.Actualizar(oCliente);
            return Json(true);
        }

        public JsonResult BuscarCliente(string term)
        {
            return Json(ClienteBL.BuscarCliente(term), JsonRequestBehavior.AllowGet);
        }
        public JsonResult BuscarPersona(string term)
        {
            return Json(ClienteBL.BuscarPersona(term), JsonRequestBehavior.AllowGet);
        }

        //public JsonResult BuscarDemo(string term)
        //{
        //    var results = PersonaBL.Listar(s => term == null || s.NombreCompleto.ToLower().Contains(term.ToLower()))
        //        .Select(x => new { id = x.PersonaId, value = x.NombreCompleto }).Take(5).ToList();

        //    return Json(results.ToList(), JsonRequestBehavior.AllowGet);
        //}


        public ActionResult ObtenerClienteDNI(string pDNI)
        {
            var persona = PersonaBL.Obtener(x => x.NumeroDocumento == pDNI);

            if (persona == null)
                return Json(null, JsonRequestBehavior.AllowGet);


            return Json(new
            {
                Persona = persona,
                FNacimiento = persona.FechaNacimiento != null ? persona.FechaNacimiento.Value.ToShortDateString() : String.Empty,
            }
                , JsonRequestBehavior.AllowGet);
        }
        public ActionResult ValidarClienteDNI(string pDNI)
        {
            var users = ClienteBL.Contar(x => x.Persona.NumeroDocumento == pDNI, includeProperties: "Persona");
            if (users > 0)
                return Json(true, JsonRequestBehavior.AllowGet);

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Mantener(int id = 0)
        {
            var usuarioId = VendixGlobal<int>.Obtener("UsuarioId");
            var oficinaId = VendixGlobal<int>.Obtener("OficinaId");
            ViewBag.cboGiroNegocio = new SelectList(OcupacionBL.Listar(x => (bool)x.Estado), "OcupacionId", "Denominacion");
            ViewBag.cboEstadoCivil = new SelectList(ValorTablaBL.Listar(x => x.TablaId == 11 && x.ItemId > 0), "ItemId", "Denominacion");
            ViewBag.cboTipoVivienda = new SelectList(ValorTablaBL.Listar(x => x.TablaId == 12 && x.ItemId > 0), "ItemId", "Denominacion");
            //ViewBag.Aprobador1 = UsuarioRolBL.Contar(x => x.UsuarioId == usuarioId && x.OficinaId == oficinaId
            //                                                && x.Rol.Denominacion == "APROBADOR 1", includeProperties: "Rol");

            ValorTablaBL.Listar(x => x.TablaId == 2 && x.ItemId > 0).Select(x => new { Id = x.ItemId, Valor = x.Denominacion });

            if (id == 0)
                return View(new Cliente() { Estado = true, Calificacion = "A", Persona = new Persona { Estado = true } });
            else
            {
                var cliente = ClienteBL.Obtener(x => x.ClienteId == id, includeProperties: "Persona");
                if (cliente.Persona.DistritoId.HasValue && cliente.Persona.DistritoId.Value > 0)
                {
                    var distrito = DistritoBL.Obtener(x => x.idDist == cliente.Persona.DistritoId.Value, includeProperties: "Provincia");
                    ViewBag.Distrito = distrito.Denominacion + " - " + distrito.Provincia.Denominacion;
                }
                return View(cliente);
            }
        }
        public JsonResult BuscarDistrito(string term)
        {
            return Json(ClienteBL.BuscarDistrito(term), JsonRequestBehavior.AllowGet);
        }
    }
}
