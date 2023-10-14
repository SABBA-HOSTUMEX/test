using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FfmpegUnity
{
    public class FfmpegGetTexturePerFrameCommand : FfmpegPlayerCommand
    {
        void Awake()
        {
            SyncFrameRate = false;
            StopPerFrame = true;
            BufferTime = -1f;
        }

        protected override void Build()
        {
            Time = 0f;

            base.Build();
        }

        /*
        protected override void Update()
        {
            ExecuteStdErrLine();
        }
        */

        public IEnumerator GetNextFrame()
        {
            while (!WriteNextTexture())
            {
                yield return null;
            }
        }
    }
}