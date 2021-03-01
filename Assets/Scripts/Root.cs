using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Root : MonoBehaviour
{
    public GameObject player;
    public GameObject ground;
    public GameObject preplayer;
    public GameObject prefood;
    public GameObject endpanel;
    public int foodcount = 150;

    public string ip = "9.134.9.104";
    public int port = 12345;

    float _fadeDuration = 1f;
    float _displayImageDuration = 1f;
    float _endtimer = 0;

    Network _net = new Network(new JsonCodecs());
    Dictionary<string, GameObject> _players = new Dictionary<string, GameObject>();

    public static Color RandomColor(float a = 1.0f) {
        float r = UnityEngine.Random.Range(0, 255) / 255.0f;
        float g = UnityEngine.Random.Range(0, 255) / 255.0f;
        float b = UnityEngine.Random.Range(0, 255) / 255.0f;
        return new Color(r, g, b, a);
    }

    public void InitialRandomFood() {
        float max_ground_x =   ground.GetComponent<Collider>().bounds.size.x / 2.0f;
        float min_ground_x = - ground.GetComponent<Collider>().bounds.size.x / 2.0f;
        float max_ground_z =   ground.GetComponent<Collider>().bounds.size.z / 2.0f;
        float min_ground_z = - ground.GetComponent<Collider>().bounds.size.z / 2.0f;

        Vector3 pos = new Vector3(UnityEngine.Random.Range(min_ground_x, max_ground_x),
                                  0.5f,
                                  UnityEngine.Random.Range(min_ground_z, max_ground_z));
        GameObject o = Instantiate(prefood, pos, Quaternion.identity);
        o.GetComponent<Renderer>().material.color = RandomColor();
    }

    private void Awake() {
        _net.AddCallback("JOIN", PlayerJoinHandler);
        _net.AddCallback("EXIT", PlayerExitHandler);
        _net.AddCallback("MOVE", PlayerMoveHandler);

        Application.targetFrameRate = 60;
        Application.runInBackground = true;

        if (!_net.Connect(ip, port)) {
            Debug.Log("Failed to connect server: " + ip + ":" + port.ToString());
            return;
        }

        Debug.Log("Connect server: " + ip + ":" + port.ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        // init player color
        player.GetComponent<Renderer>().material.color = RandomColor();

        // init food
        for (int i = 0; i < foodcount; ++i) {
            InitialRandomFood();
        }

        JoinProto proto = new JoinProto();
        proto.id = player.GetComponent<Player>().GetID();
        _net.Send<JoinProto>(proto);
    }

    void OnApplicationQuit() {
        ExitProto proto = new ExitProto();
        proto.id = player.GetComponent<Player>().GetID();
        _net.Send<ExitProto>(proto);

        _net.Disconnect();
    }

    void Update() {
        if (_net != null && _net.IsConnected()) {
            _net.Update();
        }

        if (player.transform.position.y < -1.0f) {
            CanvasGroup imageCanvasGroup = endpanel.GetComponent<CanvasGroup>();

            _endtimer += Time.deltaTime;
            imageCanvasGroup.alpha = _endtimer / _fadeDuration;
            if(_endtimer > _fadeDuration + _displayImageDuration)
            {
                Application.Quit ();
            }
        }

        List<string> roomlines = new List<string>();
        roomlines.Add("Room:");
        foreach (var e in _players) {
            string id = e.Key;
            string score = e.Value.GetComponent<Player>().GetScore().ToString();
            roomlines.Add(id + ":" + score);
        }
        string room = String.Join("\n", roomlines.ToArray());

        GameObject panel = GameObject.FindGameObjectWithTag ("Panel");
        Panel panelsc = panel.GetComponent<Panel>();
        panelsc.UpdateRoom(room);
    }

    void LateUpdate() {
        MoveProto proto = new MoveProto();
        proto.id = player.GetComponent<Player>().GetID();
        proto.x = player.transform.position.x;
        proto.z = player.transform.position.z;
        proto.score = player.GetComponent<Player>().GetScore();
        _net.Send<MoveProto>(proto);
    }

    void PlayerJoinHandler(Codecs cs, byte[] data) {
        JoinProto proto = cs.Decode<JoinProto>(data);
        string line = "[" + proto.id + "] join";
        print(line);

        if (proto.id == player.GetComponent<Player>().GetID()) {
            _players.Add(proto.id, player);
            return;
        }

        GameObject e = Instantiate(preplayer, new Vector3(0, 0, 0), Quaternion.identity);
        e.GetComponent<Renderer>().material.color = RandomColor();
        _players.Add(proto.id, e);
        Transform PlayerMapIconTransform = e.transform.GetChild(0);
        PlayerMapIconTransform.GetComponent<Renderer>().material.color = Color.red;
    }

    void PlayerExitHandler(Codecs cs, byte[] data) {
        ExitProto proto = cs.Decode<ExitProto>(data);
        string line = "[" + proto.id + "] exit";
        print(line);

        if (proto.id == player.GetComponent<Player>().GetID()) {
            return;
        }

        if (!_players.ContainsKey(proto.id)) {
            return;
        }

        GameObject e = _players[proto.id];
        _players.Remove(proto.id);
        Destroy(e);
    }

    void PlayerMoveHandler(Codecs cs, byte[] data) {
        MoveProto proto = cs.Decode<MoveProto>(data);

        if (proto.id == player.GetComponent<Player>().GetID()) {
            return;
        }

        string line = "[" + proto.id + "] move " + proto.x.ToString() + ", " + proto.z.ToString();
        print(line);

        if (!_players.ContainsKey(proto.id)) {
            GameObject e = Instantiate(preplayer, new Vector3(proto.x, 0, proto.z), Quaternion.identity);
            e.GetComponent<Renderer>().material.color = RandomColor();
            _players.Add(proto.id, e);
            Transform PlayerMapIconTransform = e.transform.GetChild(0);
            PlayerMapIconTransform.GetComponent<Renderer>().material.color = Color.red;
        } else {
            GameObject e = _players[proto.id];
            Rigidbody rg = e.GetComponent<Rigidbody>();
            rg.MovePosition(new Vector3(proto.x, 0, proto.z));
            e.GetComponent<Player>().SetScore(proto.score);
        }
    }
}
