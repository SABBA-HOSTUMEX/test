using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FfmpegUnity.Sample
{
    public class BackButton : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(quitCoroutine());
            }
        }

        IEnumerator quitCoroutine()
        {
            FfmpegCommand[] commands = FindObjectsOfType<FfmpegCommand>();
            foreach (var command in commands)
            {
                command.StopFfmpeg();
            }

            bool loopFlag;
            do
            {
                yield return null;
                loopFlag = false;

                foreach (var command in commands)
                {
                    if (command.IsRunning)
                    {
                        loopFlag = true;
                        break;
                    }
                }
            } while (loopFlag);

            Application.Quit();
        }
    }
}
