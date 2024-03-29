﻿using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class ScreenClutterController : MonoBehaviour
    {
        private static ScreenClutterController _instance;
        private static bool _init;
        private static float _prevPlayerGhost;
        private static bool _prevTeamPlayerInfoVisibility;
        private static bool _uiOn;
        
        private static GameObject _body;
        private static GameObject _fpArms;
        private static GameObject _uiCrosshairLayer;
        
        private static GameObject _uiPlayerLayer;
        private static GameObject _uiInteractionLayer;
        private static GameObject _uiNavMarkerLayer;
        private static GameObject _watermarkLayer;
        private static PE_FPSDamageFeedback _damageFeedback;
        
        private static readonly int PlayerGhostOpacity = Shader.PropertyToID("_PlayerGhostOpacity");

        public static ScreenClutterController GetInstance()
        {
            if (_init)
            {
                return _instance;
            }

            CinematographyCore.log.LogWarning("ScreenClutterManager#GetInstance called before initialized");
            return null;
        }
        
        private void Awake()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleUI].OnValueChanged += ToggleUIElements;
            CinemaUIManager.Current.Toggles[UIOption.ToggleBio].OnValueChanged += ToggleBio;
            CinemaUIManager.Current.Toggles[UIOption.ToggleBody].OnValueChanged += ToggleBody;

            var uiRoot = GuiManager.PlayerLayer.Root;

            _uiPlayerLayer = uiRoot.FindChild("PlayerLayer").gameObject;
            _uiInteractionLayer = uiRoot.FindChild("InteractionLayer").gameObject;
            _watermarkLayer = uiRoot.FindChild("WatermarkLayer").gameObject;
            _uiNavMarkerLayer = uiRoot.FindChild("NavMarkerLayer").gameObject;
            _uiCrosshairLayer = GuiManager.CrosshairLayer.Root.FindChild("CrosshairLayer").gameObject;
            _prevPlayerGhost = Shader.GetGlobalFloat(PlayerGhostOpacity);
            _prevTeamPlayerInfoVisibility = CellSettingsManager.GetBoolValue(eCellSettingID.HUD_Player_AlwaysShowTeammateInfo);

            _body = PlayerManager.GetLocalPlayerAgent().AnimatorBody.transform.parent.gameObject;
            _fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;
            
            var fpsCamera = FindObjectOfType<FPSCamera>();
            _damageFeedback = fpsCamera.gameObject.GetComponent<PE_FPSDamageFeedback>();

            _instance = this;
            _init = true;
        }

        public bool IsBodyOrUiVisible()
        {
            return _body.active || _fpArms.active || _uiPlayerLayer.active;
        }

        public void HideUI()
        {
            ToggleUIElements(false);
        }
        
        public void ToggleAllScreenClutter(bool value)
        {
            ToggleBody(value);
            ToggleUIElements(value);
            ToggleScreenShake(value);
            ToggleScreenLiquids(value);
        }

        public void ToggleClientVisibility(PlayerAgent player, bool value)
        {
            player.AnimatorBody.gameObject.active = value;
            player.NavMarker.m_marker.SetVisible(value);
        }

        private void ToggleBody(bool value)
        {
            _body.active = value;
            _fpArms.active = value;
        }

        private void ToggleUIElements(bool value)
        {
            _uiPlayerLayer.active = value;
            _uiCrosshairLayer.active = value;
            _uiInteractionLayer.active = value;
            _watermarkLayer.active = value;

            CellSettingsManager.SetFloatValue(eCellSettingID.HUD_Player_GhostOpacity, value ? _prevPlayerGhost : 0f);
            CellSettingsManager.SetBoolValue(eCellSettingID.HUD_Player_AlwaysShowTeammateInfo, value && _prevTeamPlayerInfoVisibility);
            CellSettingsManager.ApplyAllSettings();

            _uiOn = value;
        }
        
        private void ToggleBio(bool value)
        {
            if (_uiOn)
            {
                _uiNavMarkerLayer.active = true;
                ToggleUIElements(true);    
            }
            else
            {
                _uiNavMarkerLayer.active = value;
                ToggleUIElements(false);    
            }
        }

        private void ToggleScreenShake(bool value)
        {
            _damageFeedback.enabled = value;
        }

        private void ToggleScreenLiquids(bool value)
        {
            ScreenLiquidManager.hasSystem = value;
        }

        private void OnDestroy()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleUI].OnValueChanged -= ToggleUIElements;
            CinemaUIManager.Current.Toggles[UIOption.ToggleBio].OnValueChanged -= ToggleBio;
            CinemaUIManager.Current.Toggles[UIOption.ToggleBody].OnValueChanged -= ToggleBody;
            CellSettingsApply.ApplyPlayerGhostOpacity(_prevPlayerGhost);
        }
    }
}