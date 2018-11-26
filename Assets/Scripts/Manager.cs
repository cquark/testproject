using SupBleLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour {

    public static Manager instance;
    private void Awake()
    {
        instance = this;
    }

    public delegate void BleDeviceConnectedHandler(DeviceInfo device);
    public event BleDeviceConnectedHandler BleDeviceConnectedEvent;
    public delegate void BleCommandReceivedHandler(byte[] bCommand);
    public event BleCommandReceivedHandler bleCommandReceivedEvent;

    public void SetActiveDevice(DeviceInfo device)
    {
        BleDeviceConnectedEvent(device);
    }

    public void BleCommandReceived(byte[] bCommand)
    {
        bleCommandReceivedEvent(bCommand);
    }

}
