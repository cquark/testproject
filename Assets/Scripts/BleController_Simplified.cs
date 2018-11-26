using SupBleLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BleController_Simplified : MonoBehaviour {

    public MyTCPServer tcpServer;
    public List<DeviceInfo> bleDevices;
    public Dropdown ddBleDevices;
    public string serviceUUID = "00-28";
    public string notifyCharacteristic = "F4-FF";
    public string writeCharacteristic = "F3-FF";

    private void Start()
    {
#if !UNITY_ANDROID
        tcpServer.bleMessageReceivedEvent += OnTCPMessageReceived;
#endif

    }

    public void GetBleDeviceList()
    {
#if !UNITY_ANDROID
        tcpServer.SendData(new PipeData() { appCommand = "Get_Ble_Devices" });
#endif
    }
    public void SendMessageToBleDevice(string message)
    {
        PipeData pipeData = new PipeData() { appCommand = "Message_To_Ble_Device", bleCommand = HexStringToByteArray(message) };
        SendDataToPipe(pipeData);
    }

    public void SendByteMessageToBleDevice(byte[] message)
    {
        PipeData pipeData = new PipeData() { appCommand = "Message_To_Ble_Device", bleCommand = message };
        SendDataToPipe(pipeData);
    }

    public void SendMessageToBleDevice(byte[] message)
    {
        PipeData pipeData = new PipeData() { appCommand = "Message_To_Ble_Device", bleCommand = message };
        SendDataToPipe(pipeData);
    }
    public void ConnectToBleDevice(DeviceInfo device)
    {
        PipeData pipeData = new PipeData() { appCommand = "Set_Connection_Data", data = new string[] { serviceUUID, notifyCharacteristic, writeCharacteristic } };
        Debug.Log("Sending connection data");
        SendDataToPipe(pipeData);

        pipeData = new PipeData() {appCommand= "Connect_To_Ble_Device", data = device};
        Debug.Log("Attempting to connect to " + device.deviceName + Environment.NewLine);
        SendDataToPipe(pipeData);
    }
    public void DisconnectFromBleDevice()
    {
        PipeData pipeData = new PipeData() {appCommand= "Disconnect_From_Ble_Device"};
        Debug.Log("Disconnected. Looking for ble devices" + Environment.NewLine);
        SendDataToPipe(pipeData);
    }
    public void StartBleDevicesDiscovery()
    {
        PipeData pipeData = new PipeData() { appCommand = "Start_Ble_Devices_Discovery" };
        SendDataToPipe(pipeData);
    }
    public void FindBleDongle()
    {
        PipeData pipeData = new PipeData() { appCommand = "Find_Ble_Dongle" };
        SendDataToPipe(pipeData);
    }

    public void SendDataToPipe(PipeData pipeData)
    {
#if !UNITY_ANDROID
        //blePipeClient.SendMessageToServer(pipeData);
        tcpServer.SendData(pipeData);
#endif
        //Debug.Log("Sent: " + pipeData.appCommand);

    }
    public void OnTCPMessageReceived(PipeData message)
    {
        //Debug.Log("Received: " + message.appCommand);
        UnityMainThreadDispatcher.Enqueue(ProcessReceivedPipeData(message));
    }

    IEnumerator ProcessReceivedPipeData(PipeData message)
    {
        switch (message.appCommand)
        {
            case "Ble_Devices":
                {

                    bleDevices = message.data as List<DeviceInfo>;
                    ddBleDevices.ClearOptions();
                    List<string> bleDevicesStr = new List<string>();
                    foreach (var item in bleDevices)
                    {
                        bleDevicesStr.Add(item.deviceName);
                    }
                    ddBleDevices.AddOptions(bleDevicesStr);
                }
                break;
            case "Message_From_Ble_Device":
                //Debug.Log("Received:\t" + ConversionHelper.ByteArrayToHexString(message.bleCommand) + Environment.NewLine);
                Manager.instance.BleCommandReceived(message.bleCommand);
                break;
            case "Connected_To_Ble_Device":
                Debug.Log(message.appCommand + ": " + (message.data as DeviceInfo).deviceName + Environment.NewLine);
                Manager.instance.SetActiveDevice(message.data as DeviceInfo);
                break;
            case "Error":
                Debug.Log(message.appCommand + ": " + message.data.ToString() + Environment.NewLine);
                break;
            case "Ble_Device_Found":
                {
                    DeviceInfo newBleDevice = message.data as DeviceInfo;
                    Debug.Log("Device found:" + newBleDevice.ToString() + Environment.NewLine);
                        bleDevices.Add(newBleDevice);
                    ddBleDevices.ClearOptions();
                    List<string> bleDevicesStr = new List<string>();
                    foreach (var item in bleDevices)
                    {
                        bleDevicesStr.Add(item.deviceName);
                    }
                    ddBleDevices.AddOptions(bleDevicesStr);
                }
                break;
            case "Ble_Dongle_Connected":
                Debug.Log("Ble dongle found" + Environment.NewLine);
                Debug.Log("Looking for ble devices" + Environment.NewLine);
                break;
            case "Ble_Dongles_Searching":
                Debug.Log("Searching for ble dongle" + Environment.NewLine);
                break;
            case "Ble_Dongles_Search_Completed":
                Debug.Log("Searching for ble dongles completed" + Environment.NewLine);
                break;
            case "Pipe_Connection_Established":
                Debug.Log("Pipe connection established" + Environment.NewLine);
                break;
        }
        yield return null;
    }
    public void SendCommandToBle(string command)
    {
        SendMessageToBleDevice(command);
        Debug.Log("Sent:\t\t" + command + Environment.NewLine);
    }
    public void ConnectToSelectedBleDeviceFromDropDownList()
    {
        string selectedBleDeviceName = ddBleDevices.options[ddBleDevices.value].text;
        foreach(var bleDevice in bleDevices)
        {
            if(bleDevice.deviceName==selectedBleDeviceName)
            {
                ConnectToBleDevice(bleDevice);
            }
        }
    }


    public byte[] HexStringToByteArray(string str)
    {
        str = str.Replace(" ", "");
        str = str.Replace("-", "");
        str = str.Replace("$", "");

        if (str.Length % 2 == 1)
            throw new Exception("The binary key cannot have an odd number of digits");

        byte[] arr = new byte[str.Length >> 1];

        for (int i = 0; i < (str.Length >> 1); ++i)
        {
            arr[i] = (byte)((CharToHex(str[i << 1]) << 4) + (CharToHex(str[(i << 1) + 1])));
        }

        return arr;
    }
    public string ByteArrayToHexString(Byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2} ", b);
        return hex.ToString();
    }
    public int CharToHex(char hex)
    {
        int val = (int)hex;
        return val - (val < 58 ? 48 : 55);
    }
}

