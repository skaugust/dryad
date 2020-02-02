using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public GameObject tutorial1;
    public GameObject tutorial2;
    public GameObject tutorial3;
    public Camera camera;

    private int currentIndex = 0;
    private List<GameObject> tutorialList = new List<GameObject>();

    void Start()
    {
        tutorialList.Add(tutorial1);
        tutorialList.Add(tutorial2);
        tutorialList.Add(tutorial3);
    }

    void OnMouseDown()
    {
        tutorialList[currentIndex].SetActive(false);
        currentIndex++;
        if (currentIndex < tutorialList.Count)
        {
            tutorialList[currentIndex].SetActive(true);
        }
        else
        {
            // Black out the screen to make it clear to the player we're loading.
            this.gameObject.SetActive(false);
            camera.backgroundColor = Color.black;

            SceneManager.LoadScene("GameScene");
        }
    }
}
