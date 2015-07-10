using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using SpeechLib;
using System.IO;
using System.Threading;

namespace TalkingHeadCore
{
    public partial class TalkingHead : Form
    {
        string tempFolder = System.IO.Path.GetTempPath();
        StringParser parser;
        string currentEmotion = "normal";
        string previousEmotion = "";
        SpVoice voice = new SpVoice();
        SpVoice FileWriteVoice = new SpVoice();
        public TalkingHead()
        {
            SpeechLib.ISpeechObjectTokens token = voice.GetVoices("", "");
            voice.Voice = voice.GetVoices("gender=female").Item(2);
            voice.Word += wordListener;
            voice.Volume = 0;
            Thread server = new Thread(StartServer);
            server.Start();            
            InitializeComponent();
        }

        private void wordListener(int StreamNumber, object StreamPosition, int CharacterPosition, int Length)
        {
            Console.WriteLine(CharacterPosition);
            string emotion = parser.getEmotion(CharacterPosition);
            if (emotion != null)
            {
                previousEmotion = currentEmotion;
                currentEmotion = emotion;
            }
        }
        
              

        private void button1_Click(object sender, EventArgs e)
        {
            parser = new StringParser(Input.Text);
            int voicenum = 2;
            if (parser.isHaveRussianCharacters())
                voicenum = 0;
            this.SayInFile(voicenum, parser.Result);
            this.Say(voicenum, parser.Result);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void SayInFile(int voiceNum, string input)
        {
            try
            {
                SpFileStream SPFileStream = new SpFileStream();
                SPFileStream.Open(tempFolder + "output.wav", SpeechStreamFileMode.SSFMCreateForWrite, false);
                FileWriteVoice.AudioOutputStream = SPFileStream;
                FileWriteVoice.Voice = voice.GetVoices("gender=female").Item(voiceNum); 
                FileWriteVoice.Speak(input);
                SPFileStream.Close();
            }
            catch (Exception exp)
            {

                MessageBox.Show(exp.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public void Say(int voiceNum, String input)
        {
            try
            {
                voice.Voice = voice.GetVoices("gender=female").Item(voiceNum);
                voice.Speak(input, SpeechVoiceSpeakFlags.SVSFlagsAsync);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        void StartServer()
        {
            TcpListener server = null;
            try
            {
                int MaxThreadsCount = Environment.ProcessorCount * 4;
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);
                Int32 port = 9595;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();

                while (true)
                {
                   ThreadPool.QueueUserWorkItem(Response,server.AcceptTcpClient());                      
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }
        }
        
        void Response(object client_obj)
        {
            Byte[] bytes = new Byte[256];
            String data = null;
            TcpClient client = client_obj as TcpClient;
            data = null;
            NetworkStream stream = client.GetStream();
            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                data = voice.Status.PhonemeId.ToString();
                if (!currentEmotion.Equals(previousEmotion)) { 
                    data = currentEmotion;
                    previousEmotion = currentEmotion;
                }
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                stream.Write(msg, 0, msg.Length);                
            }
            client.Close();
        }

    }
}
