using System;
using TowerWar.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputControl : MonoBehaviour, IPlayerTurnController
{
    private float _rotationInput;
    private bool _isCharging;
    private double _chargeStartTime;
    private float _rotation;
    private float _charge;
    private bool _turnActive;

    public bool isPlayerOne = true;

    public GameObject directionImage;
    public Image powerSlider;
    public AudioSource powerAudioSource;

    public GameController gameController;
    private Vector2 _currentBlockSelection;

    public Player attachedPlayer;

    private void Update()
    {
        if (!_turnActive || !attachedPlayer || !gameController)
            return;
        
        _rotation -= gameController.RotationSpeed * _rotationInput * Time.deltaTime;

        if (_isCharging)
            _charge = Math.Min((float)(Time.timeAsDouble - _chargeStartTime), 1.0f);
        else
            _charge = 0.0f;

        UpdateUi();
    }

    public void BeginTurn(Player player, GameController controller)
    {
        attachedPlayer = player;
        gameController = controller;
        isPlayerOne = player.PlayerIndex == 0;
        directionImage = player.directionImage;
        powerSlider = player.powerSlider;
        powerAudioSource = player.powerAudioSource;
        _rotation = isPlayerOne ? 0f : 180f;
        _turnActive = true;
        UpdateUi();
    }

    public void EndTurn()
    {
        _turnActive = false;
        _rotationInput = 0f;
        _isCharging = false;
        _charge = 0f;

        if (powerAudioSource)
            powerAudioSource.Stop();
        if (directionImage)
            directionImage.SetActive(false);
        if (powerSlider)
            powerSlider.fillAmount = 0f;
    }

    private void UpdateUi()
    {
        if (!directionImage || !powerSlider || !gameController)
            return;

        if (gameController.gameFinished)
        {
            directionImage.gameObject.SetActive(false);
        }
        else
        {
            if ((gameController.GetCurrentPlayer() == 0 && isPlayerOne) || (gameController.GetCurrentPlayer() == 1 && !isPlayerOne))
            {
                directionImage.gameObject.SetActive(gameController.blockSelected);

                if (!gameController.blockSelected)
                    _rotation = isPlayerOne ? 0 : 180;
            }
            else
            {
                directionImage.gameObject.SetActive(false);
            }
        }

        directionImage.transform.eulerAngles = new Vector3(0, 0, _rotation);
        powerSlider.fillAmount = _charge;
    }

    public void TargetDirection(InputAction.CallbackContext context)
    {
        if (!_turnActive || !attachedPlayer || !gameController.CanPlayerAct(isPlayerOne ? 0 : 1))
            return;
        
        if(!gameController.blockSelected)
            return;

        _rotationInput = context.ReadValue<float>();
    }

    void Fire(bool pressed)
    {
        if (pressed)
        {
            if (!_isCharging)
            {
                _isCharging = true;
                _chargeStartTime = Time.timeAsDouble;
                if (powerAudioSource)
                    powerAudioSource.Play(0);
            }
        }
        else if (_isCharging)
        {
            var velocity = ShotCalculator.CalculateVelocity(
                _rotation,
                _charge,
                gameController.MaximumShotForce,
                gameController.MinimumShotCharge);

            _isCharging = false;
            _charge = 0.0f;
            if (powerAudioSource)
                powerAudioSource.Stop();
            UpdateUi();
            gameController.Fire(velocity);
        }
    }

    public void PrimaryAction(InputAction.CallbackContext context)
    {
        if (!_turnActive || !attachedPlayer || !gameController.CanPlayerAct(isPlayerOne ? 0 : 1))
            return;
        
        if (gameController.gameFinished)
        {
            gameController.ReloadGame();
            return;
        }

        bool pressed = context.ReadValue<float>() != 0;
        if(gameController.blockSelected)
            Fire(pressed);
        else if (!pressed)
        {
            gameController.SelectCurrentBlock();
            if(AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("select1");
        }
    }

    public void BlockSelection(InputAction.CallbackContext context)
    {
        if (!_turnActive || !attachedPlayer || !gameController.CanPlayerAct(isPlayerOne ? 0 : 1))
            return;
        
        if(gameController.blockSelected)
            return;

        Vector2 direction = context.ReadValue<Vector2>();
        if(direction.magnitude < 0.7 && _currentBlockSelection.magnitude > 0.8)
            gameController.BlockSelection(_currentBlockSelection.normalized);

        _currentBlockSelection = direction;
    }
}
