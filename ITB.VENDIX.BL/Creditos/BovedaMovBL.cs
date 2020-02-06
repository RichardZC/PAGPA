using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ITB.VENDIX.BE;

namespace ITB.VENDIX.BL
{
    public class BovedaMovBL : Repositorio<BovedaMov>
    {

        public static bool TransferirBovedaCaja(decimal pImporte, string pDescripcion, int pCajaId)
        {
            var pUsuarioRegId = VendixGlobal.GetUsuarioId();
            var oficinaId = VendixGlobal.GetOficinaId();
            
            using (var scope = new TransactionScope())
            {
                try
                {
                    using (var db = new VENDIXEntities())
                    {
                        var bovedaId = db.Boveda.First(x => x.OficinaId == oficinaId && x.IndCierre == false).BovedaId;
                        var oCajaDiario = db.CajaDiario.First(x => x.CajaId == pCajaId && x.IndCierre == false);
                        var personaId = db.Usuario.First(x => x.UsuarioId == pUsuarioRegId).PersonaId;

                        db.BovedaMov.Add(new BovedaMov
                        {
                            BovedaId = bovedaId,
                            CodOperacion = "TRS",
                            Glosa = "TRANS A CAJA: " + pDescripcion.ToUpper(),
                            Importe = pImporte,
                            IndEntrada = false,
                            Estado = true,
                            CajaDiarioId = oCajaDiario.CajaDiarioId,
                            UsuarioRegId = pUsuarioRegId,
                            FechaReg = VendixGlobal.GetFecha()
                        });

                        db.MovimientoCaja.Add(new MovimientoCaja
                                                  {
                                                      CajaDiarioId = oCajaDiario.CajaDiarioId,
                                                      Operacion = "TRE",
                                                      ImportePago = pImporte,
                                                      ImporteRecibido = pImporte,
                                                      MontoVuelto = 0,
                                                      Descripcion = "TRANS DE BOVEDA: " + pDescripcion.ToUpper(),
                                                      IndEntrada = true,
                                                      Estado = true,
                                                      PersonaId = personaId,
                                                      UsuarioRegId = pUsuarioRegId,
                                                      FechaReg = VendixGlobal.GetFecha()
                                                  });
                        db.SaveChanges();

                        var qry = db.MovimientoCaja.Where(z => z.CajaDiarioId == oCajaDiario.CajaDiarioId && z.Estado).Select(x=> new {x.ImportePago,x.IndEntrada});
                        if (qry.Count(x => x.IndEntrada) > 0)
                            oCajaDiario.Entradas = qry.Where(z => z.IndEntrada).Sum(x => x.ImportePago);
                        if (qry.Count(x => x.IndEntrada==false) > 0)
                            oCajaDiario.Salidas = qry.Where(z => z.IndEntrada==false).Sum(x => x.ImportePago);
                        
                        oCajaDiario.SaldoFinal = oCajaDiario.SaldoInicial + oCajaDiario.Entradas - oCajaDiario.Salidas;

                        db.usp_ActualizarSaldosBoveda(bovedaId);

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

        public static bool TransferiraOficina(decimal pImporte, string pDescripcion, int pBovedaInicioId, int pBovedaDestinoId, int pUsuarioRegId)
        {
            //Registra Transaccacion flag =  0
            const int flag = 0;
            const int bovedaMovTempId = 0;

            using (var scope = new TransactionScope())
            {
                try
                {

                    using (var db = new VENDIXEntities())
                    {
                        db.usp_TransferirBoveda(pBovedaInicioId, pBovedaDestinoId, pDescripcion, pImporte, pUsuarioRegId,
                                                flag, bovedaMovTempId);
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

        public static bool TransferiraOficina(int pBovedaMovTempId, int flag)
        {
            using (var scope = new TransactionScope())
            {
                try
                {

                    using (var db = new VENDIXEntities())
                    {
                        db.usp_TransferirBoveda(0, 0, "", 0, 0, flag, pBovedaMovTempId);
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

        public static List<Transferencias> ListarTransferencias()
        {
            var bovedaId = VendixGlobal<int>.Obtener("BovedaId");
            List<BovedaMovTemp> lista;
            var listTransferencias = new List<Transferencias>();
            using (var db = new VENDIXEntities())
            {
                lista = db.BovedaMovTemp.Where(x => x.BovedaDestinoId == bovedaId).ToList();
            }

            foreach (var item in lista)
            {
                var oficinaId = BovedaBL.Obtener(z => z.BovedaId == item.BovedaInicioId).OficinaId;
                var personaId = UsuarioBL.Obtener(z => z.UsuarioId == item.UsuarioRegId).PersonaId;

                listTransferencias.Add(new Transferencias
                {
                    TransferenciaId = item.BovedaMovTempId,
                    Monto = item.Importe,
                    From = OficinaBL.Obtener(x => x.OficinaId == oficinaId).Denominacion,
                    Descripcion = item.Glosa,
                    UsuarioReg = PersonaBL.Obtener(x => x.PersonaId == personaId).NombreCompleto,
                });
            }
            return listTransferencias;
        }
    }

    public class Transferencias
    {
        public int TransferenciaId { get; set; }
        public decimal Monto { get; set; }
        public string From { get; set; }
        public string Descripcion { get; set; }
        public string UsuarioReg { get; set; }
    }
}
