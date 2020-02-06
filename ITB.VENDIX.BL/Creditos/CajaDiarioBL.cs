using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ITB.VENDIX.BE;
using ITB.VENDIX.BL;

namespace ITB.VENDIX.BL
{
    public class CajaDiarioBL : Repositorio<CajaDiario>
    {
        public static List<CajaDiario> LstSaldosCajaDiarioJGrid(GridDataRequest request, ref int pTotalItems)
        {
            //string filterExpression = string.Empty;
           // var bovedaid = int.Parse(request.DataFilters()["BovedaId"]);


            using (var db = new VENDIXEntities())
            {
                IQueryable<CajaDiario> query = db.CajaDiario.Include("Caja").Include("Usuario");
                                
                //if (!String.IsNullOrEmpty(filterExpression))
                //    query = query.Where(filterExpression);

                pTotalItems = query.Count();
                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
            }
        }

        public static List<CajaDiarioBoveda> LstSaldosBovedaCajaDiarioJGrid(GridDataRequest request, ref int pTotalItems)
        {
            //string filterExpression = string.Empty;
            var bovedaid = int.Parse(request.DataFilters()["BovedaId"]);


            using (var db = new VENDIXEntities())
            {
                IQueryable<CajaDiarioBoveda> query = null;
                if (bovedaid > 0)
                {
                    query = from cd in db.CajaDiario.Include("Caja").Include("Usuario")
                            join mb in db.BovedaMov on cd.CajaDiarioId equals mb.CajaDiarioId
                            where mb.BovedaId == bovedaid
                            select new CajaDiarioBoveda
                            {
                                CajaDiarioId = cd.CajaDiarioId,
                                Caja = cd.Caja.Denominacion,
                                Usuario = cd.Usuario.NombreUsuario,
                                SaldoInicial = cd.SaldoInicial,
                                SaldoFinal = cd.SaldoFinal,
                                FechaFinOperacion = cd.FechaFinOperacion,
                                FechaIniOperacion = cd.FechaIniOperacion,
                                IndCierre = cd.IndCierre,
                                TransBoveda = cd.TransBoveda
                            };
                }

                //if (!String.IsNullOrEmpty(filterExpression))
                //    query = query.Where(filterExpression);

                pTotalItems = query.Count();
                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
            }
        }
        public class CajaDiarioBoveda {
            public int CajaDiarioId { get; set; }
            public string Usuario { get; set; }
            public string Caja { get; set; }
            public decimal SaldoInicial { get; set; }
            public decimal SaldoFinal { get; set; }
            public DateTime FechaIniOperacion { get; set; }
            public DateTime? FechaFinOperacion { get; set; }
            public bool IndCierre { get; set; }
            public bool TransBoveda { get; set; }
        }

        public static List<CxcJgrid> LstCuentasxCobrarJGrid(GridDataRequest request, ref int pTotalItems)
        {
            var personaid = int.Parse(request.DataFilters()["PersonaId"]);
            var usuarioid = VendixGlobal.GetUsuarioId();
            IQueryable<CuentaxCobrar> qrycxc;
            using (var db = new VENDIXEntities())
            {
                if (personaid == 0)
                    qrycxc = db.CuentaxCobrar.Where(x => x.Credito.UsuarioRegId == usuarioid && x.Estado == "PEN");
                else
                    qrycxc = db.CuentaxCobrar.Where(x => x.Credito.PersonaId == personaid && x.Credito.UsuarioRegId == usuarioid && x.Estado == "PEN");


                IQueryable<CxcJgrid> query = qrycxc
                    .Select(x => new CxcJgrid
                    {
                        OrdenVentaId = x.Credito.OrdenVentaId == null ? 0 : x.Credito.OrdenVentaId.Value,
                        CuentaxCobrarId = x.CuentaxCobrarId,
                        Codigo = x.Credito.Persona.Codigo,
                        Cliente = x.Credito.Persona.NombreCompleto,
                        Operacion = x.Operacion,
                        Origen = " CREDITO: " + SqlFunctions.StringConvert((decimal)x.CreditoId).Trim(),// + x.Credito.OrdenVentaId == null ? "" : ("ORDEN: " + SqlFunctions.StringConvert((decimal)x.Credito.OrdenVentaId).Trim()),
                        Monto = x.Monto,
                        Estado = x.Estado,
                        FechaReg = x.Credito.FechaAprobacion.Value
                    })
                    .Union(db.OrdenVenta.Where(x => x.PersonaId == personaid && x.TipoVenta == "CON" && x.Estado == "ENV")
                               .Select(x => new CxcJgrid
                               {
                                   OrdenVentaId = x.OrdenVentaId,
                                   CuentaxCobrarId = 0,
                                   Codigo = x.Persona.Codigo,
                                   Cliente = x.Persona.NombreCompleto,
                                   Operacion = "CON",
                                   Origen = "ORDEN: " + SqlFunctions.StringConvert((decimal)x.OrdenVentaId).Trim(),
                                   Monto = x.TotalNeto,
                                   Estado = "PEN",
                                   FechaReg = x.FechaReg
                               }));

                pTotalItems = query.Count();
                var lista = query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1)*request.rows).Take(request.rows).ToList();

                for (var i = 0; i < lista.Count(); i++)
                    lista[i].Id = i;

                return lista;
            }
        }

        public static List<Credito> LstDesembolsoJGrid(GridDataRequest request, ref int pTotalItems)
        {
            var personaid = int.Parse(request.DataFilters()["PersonaId"]);
            var usuarioid = VendixGlobal.GetUsuarioId();
            IQueryable<Credito> query;
            using (var db = new VENDIXEntities())
            {
                if (personaid==0)
                   query = db.Credito.Include("Persona").Where(x => x.UsuarioRegId == usuarioid && x.Estado == "APR");
                else
                   query = db.Credito.Include("Persona").Where(x => x.PersonaId == personaid && x.Estado == "APR");

                pTotalItems = query.Count();
                var lista = query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
                
                return lista;
            }
        }
        public static List<CreditoPendienteJGrid> LstCreditoPendienteJGrid(GridDataRequest request, ref int pTotalItems)
        {
            var usuarioid = VendixGlobal.GetUsuarioId();
            IQueryable<CreditoPendienteJGrid> query;
            using (var db = new VENDIXEntities())
            {
                query = db.Credito.Where(x => x.UsuarioRegId == usuarioid && x.Estado == "DES").
                    Select(x => new CreditoPendienteJGrid
                    {
                        CreditoId = x.CreditoId,
                        Codigo = x.Persona.Codigo,
                        Cliente = x.Persona.NombreCompleto,
                        MontoCredito = x.MontoCredito,
                        PersonaId=x.PersonaId
                    });
               
                pTotalItems = query.Count();
                var lista = query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1) * request.rows).Take(request.rows).ToList();

                return lista;
            }
        }
        public static void ActualizarMontoPorCobrar(int usuarioid)
        {               
            using (var db = new VENDIXEntities())
            {
                db.usp_CalcularMontoPorCobrar(usuarioid);
            }
        }
        public static List<usp_CuotasPendientes_Result> LstCuotasPendientesJGrid(GridDataRequest request,
                                                                                 ref int pTotalItems,
                                                                                 ref string pTotales)
        {
            var indCancelacion = bool.Parse(request.DataFilters()["indCancelacion"]);

            int? creditoId = null;
            if (request.DataFilters()["CreditoId"] != null)
                creditoId = int.Parse(request.DataFilters()["CreditoId"]);

            List<usp_CuotasPendientes_Result> lista;
            using (var db = new VENDIXEntities())
            {
                lista = db.usp_CuotasPendientes(creditoId, VendixGlobal.GetFecha(), indCancelacion).ToList();
            }

            pTotalItems = lista.Count();
            pTotales = string.Empty;
            pTotales += pTotalItems.ToString() + ",";
            pTotales += lista.Sum(x => x.Cuota).ToString() + ",";
            pTotales += lista.Sum(x => x.ImporteMora).ToString() + ",";
            pTotales += lista.Sum(x => x.InteresMora).ToString() + ",";
            pTotales += lista.Sum(x => x.PagoCuota).ToString() + ",";
            pTotales += lista.Sum(x => x.Amortizacion).ToString() ;


            return lista.Skip((request.page - 1) * request.rows).Take(request.rows).ToList();
        }

        
        public static List<MovimientoCajaDiario> LstMovimientosCajaJGrid(GridDataRequest request, ref int pTotalItems)
        {
            string filterExpression = request.DataFilters()["Tipo"] == "E" ? "IndEntrada" : "IndEntrada==false";
            filterExpression += " &&  CajaDiarioId == " + VendixGlobal<int>.Obtener("CajadiarioId").ToString();

            using (var db = new VENDIXEntities())
            {
                IQueryable<MovimientoCajaDiario> query = from mc in db.MovimientoCaja
                                                         join op in db.TipoOperacion on mc.Operacion equals op.Codigo
                                                         select new MovimientoCajaDiario
                                                                    {
                                                                        MovimientoCajaId = mc.MovimientoCajaId,
                                                                        CajaDiarioId = mc.CajaDiarioId,
                                                                        FechaReg = mc.FechaReg,
                                                                        IndEntrada = mc.IndEntrada,
                                                                        Persona =
                                                                            mc.Persona == null
                                                                                ? ""
                                                                                : mc.Persona.NombreCompleto,
                                                                        Operacion = op.Denominacion,
                                                                        ImportePago = mc.ImportePago,
                                                                        Descripcion = mc.Descripcion,
                                                                        Estado = mc.Estado
                                                                    };
                if (!String.IsNullOrEmpty(filterExpression))
                    query = query.Where(filterExpression);

                pTotalItems = query.Count();

                return query.OrderBy(request.sidx + " " + request.sord)
                    .Skip((request.page - 1)*request.rows).Take(request.rows).ToList();
            }
        }

       
        public static int? PagarCuotas(int pCajaDiarioId, int pCreditoId, string pPlanPago, decimal pImporteRecibido)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    int? retid;
                    using (var db = new VENDIXEntities())
                    {

                        retid = db.usp_PagarCuotas(pCajaDiarioId, pCreditoId, pPlanPago, pImporteRecibido,
                                                   VendixGlobal.GetUsuarioId(), VendixGlobal.GetFecha()).ToList()[0];
                       
                    }
                    scope.Complete();
                    return retid;
                    //return 0;
                }
                catch (Exception )
                {
                    scope.Dispose();
                    return -1;
                }
            }


            //using (var scope = new TransactionScope())
            //{
            //    try
            //    {
            //        int? retid;
            //        using (var db = new VENDIXEntities())
            //        {


            //            var retid1 = db.Database.SqlQuery<cal>("CREDITO.usp_PagarCuotas @CajaDiarioId, @CreditoId, @ListaPlanPagoId, @ImporteRecibido, @UsuarioId, @FechaPago",
            //                                                     new SqlParameter("@CajaDiarioId", pCajaDiarioId),
            //                                                     new SqlParameter("@CreditoId", pCreditoId),
            //                                                     new SqlParameter("@ListaPlanPagoId", pPlanPago),
            //                                                     new SqlParameter("@ImporteRecibido", pImporteRecibido),
            //                                                     new SqlParameter("@UsuarioId", VendixGlobal.GetUsuarioId()),
            //                                                     new SqlParameter("@FechaPago", VendixGlobal.GetFecha())
            //                ).ToList();

                       
            //        }
            //        scope.Complete();
            //        //return retid;
            //        return 0;
            //    }
            //    catch (Exception ex)
            //    {
            //        scope.Dispose();
            //        return -1;
            //    }
            //}
        }

        public class cal
        {
            public int Fila { get; set; }
            public decimal PagoCuota { get; set; }
            public int PlanPagoId { get; set; }
        }

        public static int? PagarCuotasCancelacion(int pCajaDiarioId, int pCreditoId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    int? retid;
                    using (var db = new VENDIXEntities())
                    {
                        retid =
                            db.usp_PagarCuotasCancelacion(pCajaDiarioId, pCreditoId, VendixGlobal.GetUsuarioId(),
                                                          VendixGlobal.GetFecha()).ToList()[0];
                    }
                    scope.Complete();
                    return retid;
                }
                catch (Exception )
                {
                    scope.Dispose();
                    return -1;
                }
            }
        }

        public static string EntradaSalida(int pPersonaId, int pTipoOperacionId, string pDescripcion,decimal pImporte)
        {

            if (string.IsNullOrEmpty(pDescripcion))
                return "Ingrese Descripción";

            var cajadiarioid = VendixGlobal.GetCajaDiarioId();
            var tipooperacion = TipoOperacionBL.Obtener(pTipoOperacionId);
            if (!tipooperacion.IndEntrada)
            {
                if (pImporte > Obtener(cajadiarioid).SaldoFinal)
                    return "Saldo Insuficiente!";
            }
            
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        db.usp_EntradaSalidaCajaDiario(cajadiarioid, pPersonaId, pTipoOperacionId, pImporte,
                                                       pDescripcion, VendixGlobal.GetUsuarioId());
                    }
                    scope.Complete();
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return ex.Message;
                }
            }
        }
        
        public static string RealizarDesembolso(int pCreditoId)
        {
            var cajadiarioid = VendixGlobal.GetCajaDiarioId();
            var usuarioid = VendixGlobal.GetUsuarioId();
            int movimientoCajaId = 0;
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        var credito = db.Credito.Find(pCreditoId);
                        credito.FechaDesembolso = VendixGlobal.GetFecha();
                        credito.Estado = "DES";
                        credito.UsuarioModId = usuarioid;
                        credito.FechaMod = VendixGlobal.GetFecha();
                        CreditoBL.Actualizar(db, credito);

                        var mov = new MovimientoCaja()
                        {
                            CajaDiarioId = cajadiarioid,
                            Operacion="DES",
                            ImportePago = credito.MontoDesembolso,
                            ImporteRecibido = credito.MontoDesembolso,
                            MontoVuelto = 0,
                            PersonaId = credito.PersonaId,
                            Descripcion = "DESEMBOLSO CREDITO " + credito.CreditoId.ToString(),
                            IndEntrada = false,
                            Estado = true,
                            UsuarioRegId= usuarioid,
                            FechaReg = VendixGlobal.GetFecha(),
                            OrdenVentaId = null,
                            CreditoId = credito.CreditoId
                        };

                        db.MovimientoCaja.Add(mov);
                        db.SaveChanges();
                        movimientoCajaId = mov.MovimientoCajaId;

                        var oCajaDiario = db.CajaDiario.Find(cajadiarioid);
                        var qry = db.MovimientoCaja.Where(z => z.CajaDiarioId == oCajaDiario.CajaDiarioId && z.Estado).Select(x => new { x.ImportePago, x.IndEntrada });
                        if (qry.Count(x => x.IndEntrada) > 0)
                            oCajaDiario.Entradas = qry.Where(z => z.IndEntrada).Sum(x => x.ImportePago);
                        if (qry.Count(x => x.IndEntrada == false) > 0)
                            oCajaDiario.Salidas = qry.Where(z => z.IndEntrada == false).Sum(x => x.ImportePago);
                        oCajaDiario.SaldoFinal = oCajaDiario.SaldoInicial + oCajaDiario.Entradas - oCajaDiario.Salidas;
                                               
                        db.SaveChanges();

                    }
                    scope.Complete();
                    return movimientoCajaId.ToString();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return ex.Message;
                }                
            }
        }
        public static int? RealizarPagarCuentaxCobrar(int pOrdenVentaId,int pCuentaxCobrarId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    int? retid;
                    using (var db = new VENDIXEntities())
                    {
                        retid = db.usp_PagarCuentaxCobrar(pOrdenVentaId,pCuentaxCobrarId, VendixGlobal.GetCajaDiarioId(),
                                                      VendixGlobal.GetUsuarioId()).ToList()[0];
                    }
                    scope.Complete();
                    return retid;
                }
                catch (Exception )
                {
                    scope.Dispose();
                    return -1;
                }
            }
        }

        public static int CerrarCajaDiario()
        {
            var oCajadiario = Obtener(VendixGlobal.GetCajaDiarioId());
            oCajadiario.IndCierre = true;
            oCajadiario.FechaFinOperacion = VendixGlobal.GetFecha();
            var MontoCobrado = MovimientoCajaBL.Listar(x => x.CajaDiarioId == oCajadiario.CajaDiarioId && x.IndEntrada && x.Operacion == "CUO" && x.Estado)
                            .Sum(x => x.ImportePago);
            oCajadiario.MontoCobrado = MontoCobrado;

            var oCaja = CajaBL.Obtener(oCajadiario.CajaId);
            oCaja.IndAbierto = false;
            oCaja.FechaMod = VendixGlobal.GetFecha();
            oCaja.UsuarioModId = VendixGlobal.GetUsuarioId();

            

            using (var scope = new TransactionScope())
            {
                try
                {
                    Actualizar(oCajadiario);
                    CajaBL.Actualizar(oCaja);

                    scope.Complete();
                    return oCajadiario.CajaDiarioId;
                }
                catch (Exception )
                {
                    scope.Dispose();
                    return 0;
                }
            }
        }

        public static bool AnularMovimientoCaja(int pMovimientoCajaId, string pObservacion)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        db.usp_MovimientoCaja_Del(pMovimientoCajaId, pObservacion, VendixGlobal.GetUsuarioId());
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }

        public static string AsignarUsuarioCaja(int pCajaId, int pUsuarioAsignadoId, decimal pSaldoInicial)
        {
            var idOficina = VendixGlobal.GetOficinaId();
            var cajaDiario = new CajaDiario
            {
                CajaId = pCajaId,
                UsuarioAsignadoId = pUsuarioAsignadoId,
                SaldoInicial = pSaldoInicial,
                Entradas = 0,
                Salidas = 0,
                SaldoFinal = pSaldoInicial,
                FechaIniOperacion = VendixGlobal.GetFecha(),
                IndCierre = false,
                TransBoveda = false,
            };
            var oBoveda = BovedaBL.Obtener(x => x.OficinaId == idOficina && x.IndCierre==false);

            if (cajaDiario.SaldoInicial > oBoveda.SaldoFinal)
                return "Saldo Insuficiente de la boveda.";

            using (var scope = new TransactionScope())
            {
                try
                {
                    Crear(cajaDiario);
                    var oCaja = CajaBL.Obtener(cajaDiario.CajaId);
                    oCaja.IndAbierto = true;
                    CajaBL.Actualizar(oCaja);
                    //ActualizarMontoPorCobrar(pUsuarioAsignadoId);

                    if (pSaldoInicial>0)
                    {
                        BovedaMovBL.Crear(new BovedaMov
                                                     {
                                                         BovedaId = oBoveda.BovedaId,
                                                         CodOperacion = "TRS",
                                                         Glosa = "INICIAL " + oCaja.Denominacion + " " + VendixGlobal.GetFecha().ToShortDateString(),
                                                         Importe = pSaldoInicial,
                                                         IndEntrada = false,
                                                         Estado = true,
                                                         UsuarioRegId = VendixGlobal.GetUsuarioId(),
                                                         FechaReg = VendixGlobal.GetFecha(),
                                                         CajaDiarioId = cajaDiario.CajaDiarioId,
                                                     });
                        var oBovedaMov = BovedaMovBL.Listar(x => x.BovedaId == oBoveda.BovedaId && x.Estado);
                        oBoveda.Entradas = oBovedaMov.Where(x => x.IndEntrada).Sum(x => x.Importe);
                        oBoveda.Salidas = oBovedaMov.Where(x => x.IndEntrada == false).Sum(x => x.Importe);
                        oBoveda.SaldoFinal = oBoveda.SaldoInicial + oBoveda.Entradas - oBoveda.Salidas;
                        BovedaBL.Actualizar(oBoveda);
                    }
                    scope.Complete();
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }

        public static bool TransferirCajaDiarioBoveda()
        {
            var idOficina = VendixGlobal.GetOficinaId();
            using (var scope = new TransactionScope())
            {
                try
                {
                    var cajasDiarios = Listar(x => x.IndCierre && x.TransBoveda == false && x.Caja.OficinaId == idOficina,includeProperties: "Caja");
                    var oBoveda = BovedaBL.Obtener(x => x.OficinaId == idOficina && x.IndCierre == false);

                    foreach (var item in cajasDiarios)
                    {
                        item.TransBoveda = true;
                        Actualizar(item);

                        var movBoveda = new BovedaMov
                                            {
                                                BovedaId = oBoveda.BovedaId,
                                                CodOperacion = "TRE",
                                                Glosa = "CIERRE " + item.Caja.Denominacion + " " + VendixGlobal.GetFecha().ToShortDateString(),
                                                Importe = item.SaldoFinal,
                                                IndEntrada = true,
                                                Estado = true,
                                                UsuarioRegId = VendixGlobal.GetUsuarioId(),
                                                FechaReg = VendixGlobal.GetFecha(),
                                                CajaDiarioId = item.CajaDiarioId,
                                            };
                        BovedaMovBL.Crear(movBoveda);
                    }

                    var oBovedaMov = BovedaMovBL.Listar(x => x.BovedaId == oBoveda.BovedaId && x.Estado);
                    oBoveda.Entradas = oBovedaMov.Where(x => x.IndEntrada).Sum(x => x.Importe);
                    oBoveda.Salidas = oBovedaMov.Where(x => x.IndEntrada == false).Sum(x => x.Importe);
                    oBoveda.SaldoFinal = oBoveda.SaldoInicial + oBoveda.Entradas - oBoveda.Salidas;
                    BovedaBL.Actualizar(oBoveda);

                    using (var db = new VENDIXEntities())
                    {
                        db.usp_CalificarCliente(idOficina);
                        db.SaveChanges();
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    throw;
                }
            }
        }

        public static string MostrarDetalleOvMovCaja(int pMovimientoCajaId)
        {
            string detalleventa = string.Empty;

            using (var db = new VENDIXEntities())
            {
                var operacion = db.MovimientoCaja.Find(pMovimientoCajaId).Operacion;

                switch (operacion)
                {
                    case "INI":
                        detalleventa = string.Join(Environment.NewLine,
                                                   db.CuentaxCobrar.FirstOrDefault(
                                                       x => x.MovimientoCajaId == pMovimientoCajaId)
                                                       .Credito.OrdenVenta.OrdenVentaDet.Select(x => x.Descripcion));

                        break;
                    case "CON":
                        detalleventa = string.Join(Environment.NewLine,
                                                   db.MovimientoCaja.Find(pMovimientoCajaId)
                                                       .OrdenVenta.OrdenVentaDet.Select(x => x.Descripcion));
                        break;
                    case "CUO":
                        detalleventa = string.Join(Environment.NewLine,
                                                   db.PlanPago.FirstOrDefault(
                                                       x => x.MovimientoCajaId == pMovimientoCajaId)
                                                       .Credito.OrdenVenta.OrdenVentaDet.Select(x => x.Descripcion));
                        break;
                }
            }
            return detalleventa;
        }

        public static List<usp_RptSaldosCaja_Result> ReporteSaldoCajaDiario(int pCajaDiarioId)
        {
            using (var bd = new VENDIXEntities())
            {
                return bd.usp_RptSaldosCaja(pCajaDiarioId).ToList();
            }
        }

        public static ReporteSaldoCajaCab ObtenerRptSaldoCajaCab(int pCajaDiarioId)
        {
            
            using (var db = new VENDIXEntities())
            {
                var query = from mc in db.CajaDiario
                            where mc.CajaDiarioId == pCajaDiarioId
                            select new ReporteSaldoCajaCab
                            {
                                Oficina = mc.Caja.Oficina.Denominacion,
                                Cajero = mc.Usuario.NombreUsuario + " - " + mc.Usuario.Persona.NombreCompleto + " - " + mc.Caja.Denominacion,
                                Estado = (mc.IndCierre ? "CERRADO" : "ABIERTO"),
                                Fecha = mc.FechaIniOperacion,
                                SaldoInicial = mc.SaldoInicial,
                                SaldoFinal = mc.SaldoFinal,
                                PorcentajeCobro = mc.MontoPorCobrar > 0 ? (mc.MontoCobrado * 100) / mc.MontoPorCobrar : 0
                            };
                return query.First();
            }
        }

        public static bool TransferirSaldosBoveda(decimal pMonto, string pDescripcion, int pCajaDiarioId, int pBovedaId, int pOficinaId, int pUsuarioRegId)
        {
            using (var scope = new TransactionScope())
            {
                try
                {

                    using (var db = new VENDIXEntities())
                    {
                        var oCajaDiario = db.CajaDiario.First(x => x.CajaDiarioId == pCajaDiarioId);
                        var oficinaPersonaId = db.Oficina.First(x => x.OficinaId == pOficinaId).Usuario.PersonaId;
                        db.MovimientoCaja.Add(new MovimientoCaja
                        {
                            CajaDiarioId = pCajaDiarioId,
                            Operacion = "TRS",
                            ImportePago = pMonto,
                            ImporteRecibido = pMonto,
                            MontoVuelto = 0,
                            Descripcion = "TRANS A BOVEDA: " + pDescripcion,
                            IndEntrada = false,
                            Estado = true,
                            PersonaId = oficinaPersonaId,
                            UsuarioRegId = pUsuarioRegId,
                            FechaReg = VendixGlobal.GetFecha()
                        });

                        db.BovedaMov.Add(new BovedaMov
                        {
                            BovedaId = pBovedaId,
                            CodOperacion = "TRE",
                            Glosa = "TRANS DE CAJA: " + pDescripcion,
                            Importe = pMonto,
                            IndEntrada = true,
                            Estado = true,
                            CajaDiarioId = pCajaDiarioId,
                            UsuarioRegId = pUsuarioRegId,
                            FechaReg = VendixGlobal.GetFecha()
                        });
                        db.SaveChanges();
                       
                        var qry = db.MovimientoCaja.Where(z => z.CajaDiarioId == oCajaDiario.CajaDiarioId && z.Estado).Select(x => new { x.ImportePago, x.IndEntrada });
                        if (qry.Count(x => x.IndEntrada) > 0)
                            oCajaDiario.Entradas = qry.Where(z => z.IndEntrada).Sum(x => x.ImportePago);
                        if (qry.Count(x => x.IndEntrada == false) > 0)
                            oCajaDiario.Salidas = qry.Where(z => z.IndEntrada == false).Sum(x => x.ImportePago);

                        oCajaDiario.SaldoFinal = oCajaDiario.SaldoInicial + oCajaDiario.Entradas - oCajaDiario.Salidas;

                        db.usp_ActualizarSaldosBoveda(pBovedaId);
                        db.SaveChanges();
                    }
                    scope.Complete();
                    return true;
                }
                catch (Exception)
                {
                    scope.Dispose();
                    return false;
                }
            }

        }
    }
    public class CreditoPendienteJGrid
    {
        public int CreditoId { get; set; }
        public string Codigo { get; set; }
        public string Cliente { get; set; }
        public decimal MontoCredito { get; set; }
        public int PersonaId { get; set; }

    }
    public class ReporteSaldoCajaCab
    {
        public string Oficina { get; set; }
        public string Cajero { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public decimal PorcentajeCobro { get; set; }
    }
    public class CxcJgrid
    {
        public int Id { get; set; }
        public int OrdenVentaId { get; set; }
        public int CuentaxCobrarId { get; set; }
        public string Codigo { get; set; }
        public string Cliente { get; set; }
        public string Operacion { get; set; }
        public string Origen { get; set; }
        public string Estado { get; set; }
        public DateTime FechaReg { get; set; }
        public decimal Monto { get; set; }
        
    }
    
    public class MovimientoCajaDiario
    {
        public int MovimientoCajaId { get; set; }
        public int CajaDiarioId { get; set; }
        public DateTime FechaReg { get; set; }
        public string Persona { get; set; }
        public bool IndEntrada { get; set; }
        public string Operacion { get; set; }
        public decimal ImportePago { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
    }
     
}
