using Battlehub.RTEditor.ViewModels;
using UnityWeld.Binding;
using Battlehub.RTCommon;
using UnityEngine;
using FfmpegUnity;
using Battlehub.UIControls.DockPanels;
using UnityEngine.UI;
using iiiStoryEditor.UI.ViewModels;
namespace iiiStoryEditor.UI.ViewModels.ViewModels
{
    [Binding]
    public class StoryExportViewModel : ViewModel
    {

        public StoryboardViewModel storyboardvm;
        public StoryBoardNodeEditor NodeEditor;
        public FfmpegCaptureCommand ffCaptureCmd;
        public Region gameRegin;


        public ExportVideoViewModel exportVideoVM;
        public ExportHtmlViewModel exportHtmlVM;

        public bool isRecording = false;
        public string RecordState = "";

        protected override void Start()
        { 
            base.Start();
            RuntimeWindow window = Editor.GetWindow(RuntimeWindowType.Storyboard);
            storyboardvm = window.GetComponentInChildren<StoryboardViewModel>();
            NodeEditor = window.GetComponentInChildren<StoryBoardNodeEditor>();

            RuntimeWindow gamewin = Editor.GetWindow(RuntimeWindowType.Game);
            gameRegin = gamewin.GetComponentInParent<Region>();
        }

        public void exportMp4()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.ExportVideo.ToString());
            RuntimeWindow ExportVideoWindow = Editor.GetWindow(RuntimeWindowType.ExportVideo);

            if (ExportVideoWindow != null){
                exportVideoVM = ExportVideoWindow.GetComponent<ExportVideoViewModel>();
                if(exportVideoVM != null){
                    exportVideoVM.initByStoryExportViewModel(this);
                }
            }
        }
        public void UpdateCamera(Camera cam)
        {
            if(exportVideoVM != null)
            {
                exportVideoVM.UpdateCamera(cam);
            }
        }

        public void onNodePlayStop()
        {
            if (exportVideoVM == null) {
                RuntimeWindow ExportVideoWindow = Editor.GetWindow(RuntimeWindowType.ExportVideo);

                if(ExportVideoWindow != null)
                {

                    exportVideoVM = ExportVideoWindow.GetComponent<ExportVideoViewModel>();
                }
            }  
            if (exportVideoVM != null)
            {
                exportVideoVM.onNodePlayStop();
            }
        }
        public void exportHtml()
        {
            Editor.CreateOrActivateWindow(RuntimeWindowType.ExportHTml.ToString());
            RuntimeWindow ExportHtmlWindow = Editor.GetWindow(RuntimeWindowType.ExportHTml);

            if (ExportHtmlWindow != null)
            {
                exportHtmlVM = ExportHtmlWindow.GetComponent<ExportHtmlViewModel>();
                if (exportHtmlVM != null)
                {
                    exportHtmlVM.initByStoryExportViewModel(this);
                }
            }
        }


    }
}


