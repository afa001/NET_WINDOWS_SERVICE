using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WindowsService1
{
    partial class Archivos : ServiceBase
    {

        bool blBandera = false;
        public Archivos()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: agregar código aquí para iniciar el servicio.
            stLapso.Start(); //Se Inicia automaticamente el Control Timer
        }

        protected override void OnStop()
        {
            // TODO: agregar código aquí para rea lizar cualquier anulación necesaria para detener el servicio.
            stLapso.Stop(); //Se detiente el Control Timer
        }

        private void stLapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Evento del Control Timer que se ejecuta cada dos minutos de acuerdo al intervalo que establecimos
            if (blBandera) return;

            try
            {
                blBandera = true;

                EventLog.WriteEntry("Se Inicio proceso de copia", EventLogEntryType.Information);

                //Obtenemos las rutas 
                string stRutaOrigen = ConfigurationSettings.AppSettings["stRutaOrigen"].ToString();
                string stRutaDestino = ConfigurationSettings.AppSettings["stRutaDestino"].ToString();

                DirectoryInfo di = new DirectoryInfo(stRutaOrigen);

                //Iterammos todos los archivos que se encuentran en el directorio origen
                foreach (var archivo in di.GetFiles("*",SearchOption.AllDirectories))
                {
                    //Validamos si el archivo ya existe
                    if (File.Exists(stRutaDestino + archivo.Name))
                    {
                        File.SetAttributes(stRutaDestino + archivo.Name,FileAttributes.Normal); //establecemos su estado,propiedades en Normal
                        File.Delete(stRutaDestino + archivo.Name);//Eliminamos el archivo a copiar
                    }
                    
                    File.Copy(stRutaOrigen + archivo.Name,stRutaDestino + archivo.Name); //Copiamos el archivo en la ruta de destino
                    File.SetAttributes(stRutaDestino + archivo.Name, FileAttributes.Normal); //establecemos su estado,propiedades en Normal

                    //Validamos si el archivo existe y se copio correctamente
                    if (File.Exists(stRutaDestino + archivo.Name))
                    {
                        EventLog.WriteEntry("Se copio el archivo "+archivo.Name+" con exito",EventLogEntryType.Information);
                    }
                    else
                    {
                        EventLog.WriteEntry("No se copio el archivo "+archivo.Name+" con exito", EventLogEntryType.Information);
                    }

                    EventLog.WriteEntry("Se finalizo el proceso de copiado",EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Message,EventLogEntryType.Error);
            }

            blBandera = false;
        }
    }
}
