using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
//using System.Timers;

namespace SimpleHttpServer
{
    delegate void Function();	// a simple delegate for marshalling calls from event handlers to the GUI thread
    public partial class frmAutenticar : Form, DPFP.Capture.EventHandler
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public string Usuario { get; set; }
        public string RutaImagenes { get; set; }
        public volatile bool Resultado;
        public bool Started { get; set; }
        bool flag = false;
        public volatile bool completado = false;
        bool conectado = false;
        bool first = true;
        List<DPFP.Template> Templates = new List<DPFP.Template>();

        private string path = @"C:\tmp\verificar.txt";
        private DPFP.Capture.Capture Capturer;

        public frmAutenticar()
        {
            InitializeComponent();
          //  CloseFormEvent += CloseFormEventHandler(closeformCallback);
        }
        public volatile bool resultado;
        System.Windows.Forms.Timer t;
        System.Windows.Forms.Timer t2;
        private void Form1_Load(object sender, EventArgs e)
        {

            //Verificator = new CVerificator();
          
            t2 = new System.Windows.Forms.Timer();
            t2.Interval = 500;
            t2.Tick += on_time2;
            t2.Start();
            Init();
            Start();
            Thread.Sleep(100);
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            SetForegroundWindow(proc.MainWindowHandle);
        }

        private void on_time2(Object myObject, EventArgs myEventArgs)
        {
            //if (conectado == false)
            //    Close();
            if (completado)
                Close();
            else
            {
                var proc = System.Diagnostics.Process.GetCurrentProcess();
                SetForegroundWindow(this.Handle);
            }
        }
        //private void on_time(Object myObject, EventArgs myEventArgs)
        //{
        //    t2.Stop();
        //    //t.Stop();
        //    if (flag == false)
        //    {
        //        completado = false;               

        //        //verif = new clsVerificar();
        //        //
        //        if (verif == null)
        //        {
        //            Alive();
        //            //t.Start();
        //            return;
        //        }
        //        CargarMedico(verif.Usuario, "c:\\");
        //        flag = true;
        //        Thread.Sleep(1000);
        //    }
        //    else
        //    {
        //        if (completado)
        //        {
        //            if (verif != null)
        //            {
        //                verif.resultado = resultado;
        //                verif.habilitado = false;
        //                flag = false;
        //                man.actualizar(verif);
        //                Thread.Sleep(1000);
        //            }
        //        }
        //    }
        //    //t2.Start();

        //}


        //private void Alive()
        //{
        //    this.Invoke(new Function(delegate()
        //    {
        //        if (pbAlive.BackColor == Color.Red)
        //            pbAlive.BackColor = Color.White;
        //        else
        //            pbAlive.BackColor = Color.Red;
        //    }));
        //}
        public event CloseHostFormEventHandler CloseFormEvent;
        public delegate void CloseHostFormEventHandler(Object sender, EventArgs e);
        protected void closeformCallback(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, DPFP.Sample Sample)
        {
            this.Invoke(new Function(delegate()
            {
            //t2.Stop();
                resultado = false;
                DPFP.Verification.Verification ver = new DPFP.Verification.Verification();
                DPFP.Verification.Verification.Result res = new DPFP.Verification.Verification.Result();

                DPFP.FeatureSet FeatureSet = ExtractFeatures(Sample, DPFP.Processing.DataPurpose.Verification);

                foreach (var temp in Templates)
                {
                    ver.Verify(FeatureSet, temp, ref res);
                    if (res.Verified)
                    {
                        resultado=true;
                        break;
                    }
                }
                if (!resultado)
                {
                    MakeReport("no Autenticado");
                }
                else
                    MakeReport("autenticado");
                Capturer.StopCapture();
                completado = true;
                EventArgs myargs = new EventArgs(); 
                CloseFormEvent(this, myargs);
                t2.Start();
            }));


            //Application.Exit();
        }

        protected void SetPrompt(string prompt)
        {
            //this.Invoke(new Function(delegate()
            //{
            //    Prompt.Text = prompt;
            //}));
        }
        protected void MakeReport(string message)
        {
            //this.Invoke(new Function(delegate()
            //{
            //    StatusText.AppendText(message + "\r\n");
            //}));
        }

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The finger was removed from the fingerprint reader.");
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
        {
            MakeReport("The fingerprint reader was touched.");
            WriteErrorLog("toco");
            return;
        }
        public void WriteErrorLog(string ErrMensaje)
        {
            StreamWriter strStreamWriter = null;
            try
            {

                //string Archivo = HttpContext.Current.Server.MapPath("/") + clsCtrlApplication.LogErrorFile;
                string Archivo = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\error.log";

                //strStreamWriter = new StreamWriter(strStreamW, System.Text.Encoding.UTF8);                                 

                //Se abre el archivo y si este no existe se crea               
                if (File.Exists(Archivo))
                    strStreamWriter = File.AppendText(Archivo);
                else
                    strStreamWriter = File.CreateText(Archivo);
                string Linea = "< FECHA='" + DateTime.Now.ToString() + "' ERROR='" + ErrMensaje + "' />";
                //Escribimos la línea en el achivo de texto
                strStreamWriter.WriteLine(Linea);
                strStreamWriter.Flush();
            }
            catch
            {
                MessageBox.Show("ERROR DE ESCRITURA EN DISCO (ErrorVerifExe.log): " + ErrMensaje);
            }
            finally
            {
                if (strStreamWriter != null)
                {
                    strStreamWriter.Close();
                }
            }
        }

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            conectado = true;
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            conectado = false;
            Capturer.StopCapture();
            MakeReport("The fingerprint reader was connected.");
        }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber, DPFP.Capture.CaptureFeedback CaptureFeedback)
        {
            if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                MakeReport("The quality of the fingerprint sample is good.");
            else
                MakeReport("The quality of the fingerprint sample is poor.");
        }


        public virtual void Init()
        {
            try
            {
                Capturer = new DPFP.Capture.Capture();              // Create a capture operation.

                if (null != Capturer)
                    Capturer.EventHandler = this;                   // Subscribe for capturing events.
                else
                    throw new Exception("");// SetPrompt("Can't initiate capture operation!");
                CargarMedico(Usuario, "c:\\");
            }
            catch (Exception ex)
            {
                throw ex;  //MessageBox.Show("Can't initiate capture operation!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Start()
        {
            if (null != Capturer)
            {
                try
                {
                    Capturer.StartCapture();
                    Started = true;
                    //label1.Text = "Capturando";
                    SetPrompt("Using the fingerprint reader, scan your fingerprint.");
                }
                catch (Exception ex)
                {
                    throw ex;// SetPrompt("Can't initiate capture!");
                }
            }
        }

        void CargarMedico(string pMedico, string pRutaImagenes)
        {
            Usuario = pMedico;
            RutaImagenes = pRutaImagenes;
            CargarDedos();

        }
        
        protected DPFP.FeatureSet ExtractFeatures(DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
        {
            DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction();  // Create a feature extractor
            DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
            DPFP.FeatureSet features = new DPFP.FeatureSet();
            Extractor.CreateFeatureSet(Sample, Purpose, ref feedback, ref features);            // TODO: return features as a result?
            if (feedback == DPFP.Capture.CaptureFeedback.Good)
                return features;
            else
                return null;
        }

        private bool CargarDedos()
        {
            try
            {
                DPFP.Template template;
                int k;
                byte[] blob;
                string archivo;
                for (k = 1; k <= 10; k++)
                {
                    if (k <= 5)
                        archivo = RutaImagenes + Usuario + "_d" + k.ToString().Trim() + ".ddo";
                    else
                    {
                        archivo = RutaImagenes + Usuario + "_i" + (k - 5).ToString().Trim() + ".ddo";
                    }
                    if (File.Exists(archivo))
                    {
                        var stream = File.Open(archivo, FileMode.Open);
                        blob = new byte[(int)stream.Length];
                        stream.Read(blob, 0, (int)stream.Length);
                        template = new DPFP.Template();
                        template.DeSerialize(blob);
                        Templates.Add(template);
                        stream.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Verificar Huella", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
                return false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Capturer.StopCapture();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            SetForegroundWindow(proc.MainWindowHandle);
        }

       
       

    }
}
