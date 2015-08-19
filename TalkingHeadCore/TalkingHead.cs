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
        int count = 0;
        Thread server;
        TcpListener tcpListener = null;
        string tempFolder = "";
        StringParser parser;
        string currentEmotion = "normal";
        string previousEmotion = "";
        SpVoice voice = new SpVoice();
        SpVoice FileWriteVoice = new SpVoice();
        bool shouldStop = false;
        public TalkingHead()
        {
            SpeechLib.ISpeechObjectTokens token = voice.GetVoices("", "");
            voice.Voice = voice.GetVoices("gender=female").Item(2);
            voice.Word += wordListener;
            voice.Volume = 0;
            server = new Thread(StartServer);
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
            count++;
            startCount.Text = count.ToString();
            SaveCount();
        }

      

        private void Form1_Load(object sender, EventArgs e)
        {
            try {
                StreamReader sr = new StreamReader(tempFolder + "startcount.txt");
                String s = sr.ReadLine();
                count = Int32.Parse(s);
                sr.Close();
            } catch(Exception ee)
            {
                count = 0;
            }
            startCount.Text = count.ToString();
        }

        public void SaveCount()
        {
            try
            {
                StreamWriter sw = new StreamWriter(tempFolder+"startcount.txt");
                sw.WriteLine(count);
                sw.Close();
            }
            catch (Exception ee)
            {

            }
        }

        public void SayInFile(int voiceNum, string input)
        {
            try
            {
                SpFileStream SPFileStream = new SpFileStream();
                SPFileStream.Open(tempFolder + "output.wav", SpeechStreamFileMode.SSFMCreateForWrite, false);
 //               SPFileStream.Open("output.wav", SpeechStreamFileMode.SSFMCreateForWrite, false);

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
            try
            {
                int MaxThreadsCount = Environment.ProcessorCount * 4;
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);
                Int32 port = 9595;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                tcpListener = new TcpListener(localAddr, port);
                tcpListener.Start();

                while (!shouldStop)
                {
                    ThreadPool.QueueUserWorkItem(Response, tcpListener.AcceptTcpClient());                      
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                tcpListener.Stop();
            }
        }
        
        void Response(object client_obj)
        {
  //          Console.WriteLine("resp");
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
                if (voice.Status.RunningState == SpeechRunState.SRSEDone)
                    data = "done";
                if (!currentEmotion.Equals(previousEmotion)) { 
                    data = currentEmotion;
                    previousEmotion = currentEmotion;
                }
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                stream.Write(msg, 0, msg.Length);                
            }
            client.Close();
        }

        private void TalkingHead_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcpListener.Stop();
            Console.WriteLine("Stop Server");
            shouldStop = true;
            server.Abort();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            voice.Speak("", SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
        }

        
    }
}
