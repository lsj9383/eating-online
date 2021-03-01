using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float intensity = 8.0f;

    int score = 0;
    string id;

    void Awake() {
        id = Random.Range(0, 99999999).ToString();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (intensity == 0.0f) {
            return;
        }

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        float actual_intensity = intensity;
        if (Input.GetKey(KeyCode.LeftShift)) {
            actual_intensity *= 3;
        }

        Rigidbody rg = this.GetComponent<Rigidbody>();
        rg.AddForce(new Vector3(h * actual_intensity, 0, v * actual_intensity));
    }

    void OnTriggerEnter(Collider o) {
        if (intensity == 0.0f) {
            return;
        }

        if (o.gameObject.CompareTag("Food")) {
            Destroy(o.gameObject);

            score += 1;

            // update score show
            GameObject panel = GameObject.FindGameObjectWithTag ("Panel");
            Panel panelsc = panel.GetComponent<Panel>();
            panelsc.UpdateScore(id, score);

            // add new one food
            GameObject root = GameObject.FindGameObjectWithTag ("Root");
            Root rootsc = root.GetComponent<Root>();
            rootsc.InitialRandomFood();
        }
    }

    public int GetScore() {
        return score;
    }

    public Player SetScore(int score) {
        this.score = score;
        return this;
    }

    public string GetID() {
        return id;
    }
}
