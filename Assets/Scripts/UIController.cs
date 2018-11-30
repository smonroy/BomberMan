using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {


    [Header("Labels")]
    public GameObject itemUI;
    public Text bombLabel;
    public Text fireLabel;
    public Text speedLabel;
    public GameObject player;

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
        this.player.gameObject.SetActive(true);
        this.itemUI.gameObject.SetActive(true);


    }

    // Update is called once per frame
    void Update () {
		
	}
}
