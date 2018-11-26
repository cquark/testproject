using SupBleLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TerminalController : MonoBehaviour
{

    public BleController_Simplified bleController;
    public InputField ifCommand;
    public InputField ifLog;
    public void Start()
    {
        Manager.instance.bleCommandReceivedEvent += ProcessReceivedCommand;
        Manager.instance.BleDeviceConnectedEvent += OnBleDeviceConnected;
    }

    public void OnBleDeviceConnected(DeviceInfo device)
    {
        ifLog.text += device.deviceName + " connected!\n";
    }

    public void SendCommand()
    {
        byte[] bCommand = HexStringToByteArray(ifCommand.text);
        ifLog.text += "Sent:        " + BitConverter.ToString(bCommand) + "\n";
        bleController.SendByteMessageToBleDevice(bCommand);
    }
    public void ProcessReceivedCommand(byte[] bCommand)
    {
        ifLog.text += "Received: " + BitConverter.ToString(bCommand.ToArray()) + "\n";
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

    public int CharToHex(char hex)
    {
        int val = (int)hex;
        return val - (val < 58 ? 48 : 55);
    }

}
