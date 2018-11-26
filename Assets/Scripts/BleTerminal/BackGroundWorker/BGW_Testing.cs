using UnityEngine;
using System.Collections;
using HoV;
using System;
using System.Threading;

public class BGW_Testing : MonoBehaviour {

    private UnityBackgroundWorker ubwSample;
    private SampleHelper sampleHelper;

    void Start()
    {
        sampleHelper = new SampleHelper() { number = 0 };
        ubwSample = new UnityBackgroundWorker(this, BGW_Sample, BGW_Sample_Progress, BGW_Sample_Done, sampleHelper);
    }

    public void StartSampleBGW()
    {
        if (!ubwSample.IsBusy)
        {
            ubwSample.Run();
        }
    }
    public void AbortSampleBGW()
    {
        if (ubwSample.IsBusy)
        {
            ubwSample.Abort();
        }
    }

    private void BGW_Sample(object customData, UnityBackgroundWorkerArguments e)
    {
        try
        {
            Debug.Log("BGW Sample Starting..");

            while (e.Progress<5)
            {
                Thread.Sleep(1000);
                SampleHelper sampleHelper = (SampleHelper)customData;
                sampleHelper.number++;
                e.Progress++;
            }
        }
        catch (Exception error)
        {
            e.HasError = true;
            e.ErrorMessage = error.Message;
        }
    }
    private void BGW_Sample_Progress(object customData, int Progress)
    {
        try
        {
            SampleHelper sampleHelper = (SampleHelper)customData;
            Debug.Log("BGW Sample Updated. New value: " + sampleHelper.number.ToString());

        }
        catch (Exception error)
        {
            Debug.Log(error.Message);
        }
    }
    private void BGW_Sample_Done(object customData, UnityBackgroundWorkerInformation Information)
    {
        if (Information.Status == UnityBackgroundWorkerStatus.Done)
            Debug.Log("BGW Sample Done");
        else if (Information.Status == UnityBackgroundWorkerStatus.Aborted)
            Debug.Log("BGW Sample Aborted");
        else if (Information.Status == UnityBackgroundWorkerStatus.HasError)
            Debug.Log(Information.ErrorMessage);
    }

    private void OnApplicationQuit()
    {
        AbortSampleBGW();
    }
}

class SampleHelper
{
    public int number;
}
