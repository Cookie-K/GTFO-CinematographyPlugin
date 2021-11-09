﻿using System.Collections.Generic;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public static class UIUtils
    {
        
        public static Dictionary<UIOption, Option> BuildOptions(GameObject cinemaUI)
        {
            var options = new Dictionary<UIOption, Option>();
            options.Add(UIOption.ToggleUI, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleUI), true, true));
            options.Add(UIOption.ToggleBody, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBody), true, true));
            
            options.Add(UIOption.ToggleFreeCamera, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFreeCamera), false, true));
            options.Add(UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, FreeCameraController.MovementSpeedDefault, FreeCameraController.MovementSpeedMin, FreeCameraController.MovementSpeedMax));
            options.Add(UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, FreeCameraController.SmoothTimeDefault, FreeCameraController.SmoothTimeMin, FreeCameraController.SmoothTimeMax));
            
            options.Add(UIOption.ToggleLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleLookSmoothing), false, true));
            options.Add(UIOption.LookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.LookSmoothingSlider), false, LookSmoothingController.SmoothDefault, LookSmoothingController.SmoothMin, LookSmoothingController.SmoothMax));
            
            options.Add(UIOption.ToggleCameraTilt, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleCameraTilt), false, true));
            options.Add(UIOption.CameraTiltSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.CameraTiltSlider), false, 0, 0, 1));
            
            options.Add(UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true));
            options.Add(UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, 0, 0, 1));

            return options;
        }

        private static GameObject GetOptionObj(GameObject cinemaUI, UIOption option)
        {
            // CinemaUI/Canvas/Window/ViewPort
            var windowViewPort = cinemaUI.transform.GetChild(0).GetChild(0).GetChild(0);
            // ViewPort/Body/ViewPort
            var bodyViewPort = windowViewPort.GetChild(1).GetChild(0);
            var gameObject = bodyViewPort.transform.Find(option.ToString()).gameObject;
            return gameObject;
        }
    }
}