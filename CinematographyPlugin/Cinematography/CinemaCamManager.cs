﻿using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Enemies;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaCamManager : MonoBehaviour
    {
        private const float DelayBeforeLocomotionEnable = 0.1f;
        private const float PlayerInvulnerabilityHealth = 999_999f;

        private readonly Dictionary<string, float> _playerPrevMaxHealthByName = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _playerPrevHealthByName = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _playerPrevInfectionByName = new Dictionary<string, float>();

        private bool _freeCamEnabled;
        private bool _locomotionDisabled;
        private float _freeCamDisabledTime;

        private FPSCamera _fpsCamera;
        private GameObject _player;
        private Transform _prevParent;
        private Transform _cinemaCamCtrlHolder;
        private Transform _cinemaCam;
        private Transform _fpsCamHolderSubstitute;
        private PlayerAgent _playerAgent;
        private PlayerLocomotion _playerLocomotion;
        private CinemaCamController _cinemaCamController;

        public CinemaCamManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
	        // Comps reference set up
	        _fpsCamera = FindObjectOfType<FPSCamera>();
			_player = _fpsCamera.m_owner.gameObject;
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();

			// Cinema cam obj set up
			_cinemaCam = new GameObject("CinemaCam").transform;
			var cinemaCamCamRotation = new GameObject("CinemaCamRotation").transform;
			_fpsCamHolderSubstitute = new GameObject("FPSCamHolderSubstitute").transform;
			_cinemaCamCtrlHolder = new GameObject("CinemaCamControl").transform;
			
			_fpsCamHolderSubstitute.parent = cinemaCamCamRotation;
			cinemaCamCamRotation.parent = _cinemaCamCtrlHolder;
			_cinemaCamCtrlHolder.parent = _cinemaCam;
			
			_cinemaCamController = _cinemaCamCtrlHolder.gameObject.AddComponent<CinemaCamController>();
			_cinemaCamController.enabled = false;
        }

        private void Start()
        {
	        CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += OnFreeCameraToggle;

	        CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam += OnOtherPlayerEnterOrExitFreeCam;
        }

        private void Update()
        {
	        if (_freeCamEnabled)
	        {
		        CheckAndForceUiHidden();
		        DivertEnemiesAwayFromCameraMan();
	        }
	        else
	        {
		        CheckAndEnableLocomotionOnExitCinemaCam();
	        }
        }

        private void CheckAndForceUiHidden()
        {
	        if (ScreenClutterManager.GetInstance().IsBodyOrUiVisible())
	        {
		        // force hide all ui when in free cam
		        ScreenClutterManager.GetInstance().ToggleAllScreenClutterExceptWaterMark(false);
	        }
        }

        private void CheckAndEnableLocomotionOnExitCinemaCam()
        {
	        if (!_locomotionDisabled) return;
	        
	        // Update locomotion a frame after to avoid rubber banding 
	        var delayTimeUp = Time.realtimeSinceStartup - _freeCamDisabledTime > DelayBeforeLocomotionEnable / Time.timeScale;
	        if (!delayTimeUp) return;
	        
	        _fpsCamera.m_orgParent.localPosition = Vector3.zero;
	        _playerLocomotion.enabled = true;
	        _locomotionDisabled = false;
        }

        private void OnFreeCameraToggle(bool value)
		{
			EnableOrDisableCinemaCam(value);
			_freeCamEnabled = value;
		}

		private void EnableOrDisableCinemaCam(bool enable)
		{
			SetCameraManHealth(_playerAgent, enable);
			ScreenClutterManager.GetInstance().ToggleAllScreenClutterExceptWaterMark(!enable);

			if (enable)
			{
				_playerLocomotion.enabled = false;
				_locomotionDisabled = true;
				_prevParent = _fpsCamera.m_orgParent.parent;
				
				_cinemaCamController.SyncWithCameraTransform();
				_fpsCamera.m_orgParent.parent = _fpsCamHolderSubstitute;

				_fpsCamera.MouseLookEnabled = false;
				_prevParent.gameObject.active = false;
				_cinemaCamController.enabled = true;
			}
			else
			{
				_cinemaCamController.enabled = false;
				_fpsCamera.MouseLookEnabled = true;
				_prevParent.gameObject.active = true;

				_fpsCamera.m_orgParent.parent = _prevParent;

				// Enable player locomotion later to avoid rubber banding
				_freeCamDisabledTime = Time.realtimeSinceStartup;
			}

			CinematographyCore.log.LogMessage(enable ? "Cinema cam enabled" : "Cinema cam disabled");
		}

		private void SetCameraManHealth(PlayerAgent player, bool enteringFreeCam)
		{
			var playerName = player.Sync.PlayerNick;
			var damage = player.Damage;

			if (enteringFreeCam && _playerPrevMaxHealthByName.ContainsKey(playerName))
			{
				_playerPrevMaxHealthByName.Add(playerName, damage.HealthMax);
				_playerPrevHealthByName.Add(playerName, damage.Health);
				_playerPrevInfectionByName.Add(playerName, damage.Infection);

				damage.HealthMax = PlayerInvulnerabilityHealth;
				damage.Health = PlayerInvulnerabilityHealth;
				damage.Infection = 0;
			}
			else if (!enteringFreeCam && _playerPrevMaxHealthByName.ContainsKey(playerName))
			{
				damage.HealthMax = _playerPrevMaxHealthByName[playerName];
				damage.Health = _playerPrevHealthByName[playerName];
				damage.Infection = _playerPrevInfectionByName[playerName];
				
				_playerPrevMaxHealthByName.Remove(playerName);
				_playerPrevHealthByName.Remove(playerName);
				_playerPrevInfectionByName.Remove(playerName);

				if (player.IsLocallyOwned)
				{
					damage.TryCast<Dam_PlayerDamageLocal>().UpdateHealthGui();
				}
			}
		}

		private void OnOtherPlayerEnterOrExitFreeCam(PlayerAgent playerAgent, bool enteringFreeCam)
		{
			CinematographyCore.log.LogInfo($"{playerAgent.Sync.PlayerNick} entering free cam : {enteringFreeCam}");
			SetCameraManHealth(playerAgent, enteringFreeCam);
			ScreenClutterManager.GetInstance().ToggleClientVisibility(playerAgent, !enteringFreeCam);
		}
		
		private void DivertEnemiesAwayFromCameraMan()
		{
			if (PlayerManager.PlayerAgentsInLevel.Count == 1) return;
			
			foreach (var playerAgent in CinemaNetworkingManager.PlayersInFreeCamByName.Values)
			{
				foreach (var attacker in new List<Agent>(playerAgent.GetAttackers().ToArray()))
				{
					playerAgent.UnregisterAttacker(attacker);
					var delegateAgent = CinemaNetworkingManager.PlayersNotInFreeCamByName.Values.Aggregate(
							(currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
					CinematographyCore.log.LogInfo($"Diverting {attacker.name} to {delegateAgent}");
					attacker.TryCast<EnemyAgent>().AI.SetTarget(delegateAgent);
				}
			}
		}

		private void OnDestroy()
		{
			CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= OnFreeCameraToggle;
	
			CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam -= OnOtherPlayerEnterOrExitFreeCam;
		}
    }
}