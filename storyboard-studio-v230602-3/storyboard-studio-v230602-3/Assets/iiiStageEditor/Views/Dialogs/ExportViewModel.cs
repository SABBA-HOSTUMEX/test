using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.UIControls.Binding;
using Battlehub.Utils;
using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;
//using GLTFast;
//using GLTFast.Export;


namespace ExportFile.ViewModels
{
    [Binding]
    public class ExportViewModel : ViewModel
    {
        protected override void Start()
        {

            //Debug.Log("ExportViewModel Start");
            base.Start();

            


        } 

        protected override void OnDestroy()
        {
            //Debug.Log("ExportViewModel OnDestroy");
            base.OnDestroy();
        }

        protected override void Awake()
        {
            //Debug.Log("ExportViewModel Awake");
            base.Awake();
        }

    }
}


