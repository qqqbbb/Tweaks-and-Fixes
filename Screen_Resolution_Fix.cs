using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

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

        [HarmonyPatch(typeof(GameSettings))]
        class GameSettings_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("LoadAsync", new Type[] { })]
            static void InitializePostfix(GameSettings __instance)
            {
                //Main.logger.LogMessage("GameSettings LoadAsync");
                if (!ConfigToEdit.fixScreenResolution.Value || Main.configMain.screenRes.width == 0)
                    return;

                if (Screen.currentResolution.width == Main.configMain.screenRes.width && Screen.currentResolution.height == Main.configMain.screenRes.height)
                    return;

                Resolution[] resolutions = Screen.resolutions;
                for (int i = 0; i < resolutions.Length; i++)
                {
                    if (resolutions[i].width == Main.configMain.screenRes.width && resolutions[i].height == Main.configMain.screenRes.height)
                    {
                        Screen.SetResolution(Main.configMain.screenRes.width, Main.configMain.screenRes.height, Main.configMain.screenRes.fullscreen);
                        break;
                    }
                }
                Main.logger.LogMessage("Resolution fixed " + Screen.currentResolution.width);
            }
        }

    }
}
