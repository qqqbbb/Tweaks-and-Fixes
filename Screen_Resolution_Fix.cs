using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;
using UnityEngine;
using static OVRHaptics;

namespace Tweaks_Fixes
{
    public class Screen_Resolution_Fix
    {
        public struct ScreenRes
        {
            public int width;
            public int height;
            public bool fullscreen;
            public ScreenRes(int w, int h, bool f)
            {
                width = w;
                height = h;
                fullscreen = f;
            }
        }

        [HarmonyPatch(typeof(DisplayManager))]
        class DisplayManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            static void InitializePostfix(DisplayManager __instance)
            {
                if (Main.config == null)
                {
                    Main.logger.LogMessage("DisplayManager Initialize configOld == null");
                    return;
                }
                if (Main.config.screenRes.width == 0)
                    return;

                if (Screen.currentResolution.width != Main.config.screenRes.width || Screen.currentResolution.height != Main.config.screenRes.height)
                {
                    Resolution[] resolutions = Screen.resolutions;
                    for (int i = 0; i < resolutions.Length; i++)
                    {
                        if (resolutions[i].width == Main.config.screenRes.width && resolutions[i].height == Main.config.screenRes.height)
                        {
                            Screen.SetResolution(Main.config.screenRes.width, Main.config.screenRes.height, Main.config.screenRes.fullscreen);
                            break;
                        }
                    }
                    Main.logger.LogMessage("Resolution fixed " + Screen.currentResolution.width);
                }
            }
        }

    }
}
