using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIController : MonoBehaviour {


    [Header("Labels")]
    public GameObject itemUI;
    public Text bombLabel;
    public Text fireLabel;
    public Text speedLabel;
    public GameObject player;
    public Button startButton;

    [Header("MusicSFX")]
    public AudioSource backgroundMusic;
    public AudioSource bombUp;
    public AudioSource bombBoom;
    public AudioSource speedUp;
    public AudioSource rangeUp;


    // Use this for initialization
    void Start () {
        backgroundMusic.Play();
        backgroundMusic.loop = true;
//        this.player.gameObject.SetActive(true);
//        this.itemUI.gameObject.SetActive(true);
    }

    //public override void OnStartClient() {
    //    base.OnStartClient();
    //    startButton.gameObject.SetActive(false);
    //}

}
