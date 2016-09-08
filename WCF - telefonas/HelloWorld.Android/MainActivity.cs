using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using System.ServiceModel;
using HelloWorldWcfHost;
using Android.Provider;
using Java.IO;
using Android.Media;
using Android.Graphics;
using System.IO;

namespace HelloWorld.Droid
{
    public static class App
    {
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Android.Graphics.Bitmap bitmap;
    }

    [Activity(Label = "HelloWorld.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        // public static readonly EndpointAddress EndPoint = new EndpointAddress("http://192.168.1.55:9608/HelloWorldService.svc");
        public static readonly EndpointAddress EndPoint = new EndpointAddress("http://192.168.1.55:9608/Service1.svc");
        private HelloWorldServiceClient _client;
       

        private Button _FotoButton;
        private TextView _FotoView;

        public string dataText;
        public string stringText;
        public string PlateStrings;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            InitializeHelloWorldServiceClient();

            _FotoButton = FindViewById<Button>(Resource.Id.FotoButton);
            _FotoButton.Click += FotoButtonOnClick;
            _FotoView = FindViewById<TextView>(Resource.Id.FotoView);

            App._dir = new Java.IO.File(
               Android.OS.Environment.GetExternalStoragePublicDirectory(
                   Android.OS.Environment.DirectoryPictures), "Recognize");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }

            if (bundle != null) // jei Bundle saugojo kazkokia informacija
            {
           
                PlateStrings = bundle.GetString("PlateStrings", "0");
                _FotoView.Text = PlateStrings;
            }

        }

        protected override void OnSaveInstanceState(Bundle outState) // textview duomenu issaugojimui
        {                                                            // duomenys issaugomi "Bundle", kadangi kiekviena karta vartant telefona
            outState.PutString("dataText", dataText);                // android faktiskai sunaikina gui objektus ir ju saugomus duomenis
            outState.PutString("stringText", stringText);
            outState.PutString("PlateStrings", PlateStrings);                    
            base.OnSaveInstanceState(outState);
        }


        private void InitializeHelloWorldServiceClient()
        {
            BasicHttpBinding binding = CreateBasicHttp();

            _client = new HelloWorldServiceClient(binding, EndPoint);
            _client.SayHelloToCompleted += ClientOnSayHelloToCompleted;
            _client.GetHelloDataCompleted += ClientOnGetHelloDataCompleted;
            _client.RecognizeCompleted += ClientOnRecognizeCompleted;

        }

        private static BasicHttpBinding CreateBasicHttp()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                Name = "basicHttpBinding",
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            };
            TimeSpan timeout = new TimeSpan(0, 0, 30);
            binding.SendTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;
            return binding;
        }


        private void FotoButtonOnClick(object sender, EventArgs eventArgs)
        {
            _FotoView.Text = "Ruošiama...";
            Intent intent = new Intent(MediaStore.ActionImageCapture); // intent yra abstrakti klasė laikanti informacija apie norimą vykdyti operaciją
                                                                       // naudojama "surišti" dvi aplikacijas, šiuo atveju, šita aplikacija ir telefono kamerą
                                                                       // App._file = new Java.IO.File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid())); //Guid (Global unique identifier) sugeneruoja unikalų pavadinimą nuotraukai
            App._file = new Java.IO.File(App._dir, "myPhoto.jpg"); 
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file)); // failo išsaugojimas sa nurodytu pavadinimu
            StartActivityForResult(intent, 0); // vykdo OnActivityResult
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            _FotoView.Text = "Laukiama Serverio...";
          

            var imageFile = new Java.IO.File(App._file.ToString());
            Bitmap bitmap = BitmapFactory.DecodeFile(imageFile.AbsolutePath); //konvertuojamas image i bitmap

            byte[] byteArr;

            var ms = new MemoryStream();

            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 0, ms); //bitmap paverciamas i bitu masyva

            byteArr = ms.ToArray(); 
            ms.Close(); // PAKEITIMAS
            SendToRecognize(byteArr);
            GC.Collect();  // sukurtu bitmap pasalinimas, istisai darant nuotraukas, programa uzlusta nes naudoja per daug atminties
        }

        private void SendToRecognize(byte[] arr)
        {
            _client.RecognizeAsync(arr);
        }


        private void GetHelloWorldDataButtonOnClick(object sender, EventArgs eventArgs)
        {
            HelloWorldData data = new HelloWorldData { Name = "Ponas Vardenis", SayHello = true };
           // _getHelloWorldDataTextView.Text = "Laukiama Serverio...";
            _client.GetHelloDataAsync(data);
          
        }

        private void SayHelloWorldButtonOnClick(object sender, EventArgs eventArgs)
        {
           // _sayHelloWorldTextView.Text = "Laukiama Serverio...";
            _client.SayHelloToAsync("Vardenis");
        }

        private void ClientOnGetHelloDataCompleted(object sender, GetHelloDataCompletedEventArgs getHelloDataCompletedEventArgs)
        {
            string msg = null;

            if (getHelloDataCompletedEventArgs.Error != null)
            {
                msg = getHelloDataCompletedEventArgs.Error.Message;
            }
            else if (getHelloDataCompletedEventArgs.Cancelled)
            {
                msg = "Request was cancelled.";
            }
            else
            {
                msg = getHelloDataCompletedEventArgs.Result.Name;
            }
           // RunOnUiThread(() => _getHelloWorldDataTextView.Text = msg);
            dataText = msg;

        }

        private void ClientOnSayHelloToCompleted(object sender, SayHelloToCompletedEventArgs sayHelloToCompletedEventArgs)
        {
            string msg = null;

            if (sayHelloToCompletedEventArgs.Error != null)
            {
                msg = sayHelloToCompletedEventArgs.Error.Message;
            }
            else if (sayHelloToCompletedEventArgs.Cancelled)
            {
                msg = "Request was cancelled.";
            }
            else
            {
                msg = sayHelloToCompletedEventArgs.Result;
            }
           // RunOnUiThread(() => _sayHelloWorldTextView.Text = msg);
            stringText = msg;
        }


        //ClientOnRecognizeCompleted
        private void ClientOnRecognizeCompleted(object sender, RecognizeCompletedEventArgs RecognizeCompletedEventArgs)
        {

            int ilgis;
            ilgis = RecognizeCompletedEventArgs.Result.Length;

            string result="";

            for (int i = 0; i < ilgis; i++)
            {
                result = result + RecognizeCompletedEventArgs.Result[i] + "\r\n";
            }
            PlateStrings = result;
            RunOnUiThread(() => _FotoView.Text = result);
        }
    }
}

