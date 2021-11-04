using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject resumeUI;


    private void Start() {
        if (resumeUI != null) resumeUI.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (resumeUI != null) {
                if (resumeUI.activeSelf) {
                    resumeUI.SetActive(false);
                    Time.timeScale = 1;
                }else {
                    resumeUI.SetActive(true);
                    Time.timeScale = 0;
                }
            }
        }
    }
    public void Quit(){
        Application.Quit();
        Debug.Log("I quit");
    }

    public void StartGame(){
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void ResumeGame(){
        resumeUI.SetActive(false);
        Time.timeScale = 1;
    }

}
