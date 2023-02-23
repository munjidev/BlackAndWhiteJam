using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI portalsPlacedUI;
    public TextMeshProUGUI deathsUI;
    private int _portalsPlacedText;
    private int _deathsText;

    private void Update()
    {
        // Get portalsplacedtext from portalplacement script
        _portalsPlacedText = GameObject.Find("Player").GetComponent<PortalPlacement>().portalsPlaced;
        
        // Get palyer deaths from playermovement script
        _deathsText = GameObject.Find("Player").GetComponent<PlayerMovement>().deaths;
        
        portalsPlacedUI.text = "Holes placed: " + _portalsPlacedText;
        deathsUI.text = "Deaths: " + _deathsText;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Scenes/PortalDemo");
    }

}
