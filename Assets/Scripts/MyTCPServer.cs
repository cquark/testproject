using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Threading;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using SupBleLibrary;

public class MyTCPServer : MonoBehaviour {

    public delegate void bleMessageReceivedEventHandler(PipeData message);
    public event bleMessageReceivedEventHandler bleMessageReceivedEvent;

    public int port = 25001;
    public string ipAddress = "127.0.0.1";

    bool isConnected;
    Thread mThread;
    TcpListener server;
    TcpClient client;
    NetworkStream stream;
    BinaryWriter sWriter;
    BinaryReader sReader;

    System.Diagnostics.Process proc;

    private void Start()
    {
        CreateServer();
        CreateClient();
    }

    public void CreateClient()
    {
        string path = Directory.GetParent(Application.dataPath).FullName + @"\BTS\Ble_Terminal_Server.exe";

        proc = new System.Diagnostics.Process();

        //proc.EnableRaisingEvents = false;
        //proc.StartInfo.UseShellExecute = false;
        //proc.StartInfo.RedirectStandardOutput = true;

        proc.StartInfo.FileName = path;

        //comment if don't want to hide 
        proc.StartInfo.CreateNoWindow = true;
        proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;


        proc.Start();
        Thread.Sleep(100);
    }

    void Update()
    {
        //ReadData();
    }

    public void CreateServer()
    {
        isConnected = false;
        ThreadStart ts = new ThreadStart(ServerThread);
        mThread = new Thread(ts);
        mThread.Start();
    }
    void ServerThread()
    {
        server = null;
        try
        {
            IPAddress localAddr = IPAddress.Parse(ipAddress);
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            while (!isConnected)
            {
                Thread.Sleep(10);
                Debug.Log("Waiting for a connection... ");
                client = server.AcceptTcpClient();
                if (client != null)
                {
                    stream = client.GetStream();
                    sWriter = new BinaryWriter(stream);
                    sReader = new BinaryReader(stream);
                    isConnected = true;
                    Debug.Log("Connected!");
                }
            }
            while(true)
            {
                ReadData();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException:" + e);
        }
        finally
        {
            server.Stop();
        }
    }
    public void SendData(PipeData pipeData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, pipeData);
                byte[] convertedClass = ms.ToArray();
                sWriter.BaseStream.Write(convertedClass, 0, convertedClass.Length);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public void ReadData()
    {
        if (stream == null)
            return;
        if (stream.DataAvailable)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            PipeData pipeData = (PipeData)binaryFormatter.Deserialize(stream);
            //Debug.Log(pipeData.appCommand);
            bleMessageReceivedEvent(pipeData);
        }
    }
    void OnApplicationQuit()
    {
        CloseClient();
        CloseServer();
    }

    public void CloseServer()
    {
        isConnected = false;
        mThread.Abort();
        if(server!=null)
        {
            server.Stop();
        }
        if(client!=null)
        {
            sWriter.Close();
            sReader.Close();
            stream.Close();
            client.Close();
        }
    }

    public void CloseClient()
    {
        SendData(new PipeData() { appCommand = "Shutdown" });

        //proc.CloseMainWindow();
    }
}
