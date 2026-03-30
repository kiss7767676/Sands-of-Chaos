using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;
    private PlayerWeaponController playerWeaponController;


    [Header("Settings")]
    public bool friendlyFire;
    [Space]
    public bool quickStart;

    private void Awake()
    {
        instance = this;
        player = FindObjectOfType<Player>();
        playerWeaponController = player.GetComponent<PlayerWeaponController>();
    }

    public void GameStart()
    {
        //LevelGenerator.instance.InitializeGeneration();
        ControlsManager.instance.SwitchToCharacterControls();
        
    }
    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void GameCompleted()
    {
        UI.instance.ShowVictoryScreenUI();
        ControlsManager.instance.controls.Character.Disable();
        player.health.currentHealth += 99999;
    }

    public void GameOver()
    {
        TimeManager.instance.SlowMotionFor(1);
        UI.instance.ShowGameOverUI();
    }
}