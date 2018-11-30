using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour {


    [Header("Labels")]
    public Text bombLabel;
    public Text fireLabel;
    public Text speedLabel;
    public GameObject player;
    public GameObject itemUI;

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
        this.bombLabel.gameObject.SetActive(true);
        this.fireLabel.gameObject.SetActive(true);
        this.speedLabel.gameObject.SetActive(true);
        this.player.gameObject.SetActive(true);


    }

    // Update is called once per frame
    void Update () {
		
	}
}
