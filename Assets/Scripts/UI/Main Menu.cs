using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    private Button _newsim;
    private Button _loadsim;
    private Button _exit;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _newsim = root.Q<Button>("New-Sim");
        _loadsim = root.Q<Button>("Load-Sim");
        _exit = root.Q<Button>("Exit");

        _newsim.RegisterCallback<ClickEvent>(NewSim);
        _exit.RegisterCallback<ClickEvent>(ExitGame);
    }
    private void NewSim(ClickEvent evt)
    {
        SceneManager.LoadScene(sceneName: "New Simulation");
    }

    private void ExitGame(ClickEvent evt)
    {
        Debug.Log("Game is exiting");
        Application.Quit();
    }

    void Update()
    {
    }
}
