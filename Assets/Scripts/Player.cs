﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Player : MonoBehaviour {
    Vector3 mouseScrPos = Vector3.zero;
    [SerializeField]
    float moveSpeed = 1.0f;
    public enum PlayerState { idle, walk, interactive, up, down, offset }
    public PlayerState _playerState;
    Animator ani;
    SpriteRenderer _spriteRender;
    List<string> _holdItems;
    public event EventHandler OnItemAdd;

    private void Awake()
    {
        _playerState = PlayerState.idle;
        ani = GetComponent<Animator>();
        _spriteRender = GetComponentInChildren<SpriteRenderer>();
        _holdItems = new List<string>();
    }
    // Use this for initialization
    void Start() {
        for (int i = 0; i < GameManager.game.Items.Length; i++)
        {
            GameManager.game.Items[i].GetComponent<InteractiveItem>().OnItemClicked += this.OnItemClicked;//監聽
        }
    }

    // Update is called once per frame
    void Update() {
        //check item interactive status
        for (int i = 0; i < GameManager.game.Items.Length; i++)
        {
            if (Vector3.Distance(transform.position, GameManager.game.Items[i].transform.position) < GameManager.game.Items[i].GetComponent<InteractiveItem>().interactiveDistance && (_playerState == PlayerState.idle || _playerState == PlayerState.walk))
                GameManager.game.Items[i].GetComponent<InteractiveItem>().CanInteractive = true; //讓物品變成可點選狀態
            else GameManager.game.Items[i].GetComponent<InteractiveItem>().CanInteractive = false;
        }

        if (Input.GetMouseButtonDown(0))
        {

            //walk
            mouseScrPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseScrPos.z = 0.0f;
            mouseScrPos.y = transform.position.y;
            //set player rotation
            if (_playerState == PlayerState.walk || _playerState == PlayerState.idle)
            {
                if (mouseScrPos.x - transform.position.x < 0) SetPlayerRotation(180);
                else SetPlayerRotation(0);
            }
            //set walk state
            if (_playerState == PlayerState.idle || _playerState == PlayerState.walk) _playerState = PlayerState.walk;
        }

        if (_playerState == PlayerState.walk) {
            _playerState = move();
        }
        else if (_playerState == PlayerState.idle) {

        }
        else if (_playerState == PlayerState.interactive)
        {

        }
        setAnimation(_playerState);
    }
    void OnItemClicked(object sender, EventArgs args) {
        _playerState = PlayerState.interactive;

    }

    public PlayerState Playerstate
    {
        get { return _playerState; }
        set { _playerState = value; }
    }
    PlayerState move() {
        transform.position = Vector3.MoveTowards(transform.position, mouseScrPos, moveSpeed * Time.deltaTime);

        if (transform.position == mouseScrPos) return PlayerState.idle;
        else return PlayerState.walk;
    }

    void setAnimation(PlayerState state) {


        if (state == PlayerState.idle)
        {
            ani.SetBool("isWalk", false);
            ani.SetBool("isUp", false);

        }
        else if (state == PlayerState.walk) ani.SetBool("isWalk", true);
        if (state == PlayerState.offset) ani.SetBool("isWalk", true);
        if (state == PlayerState.up) ani.SetBool("isUp", true);
        if (state == PlayerState.interactive) ani.SetBool("isWalk", false);
    }
    public void SetPlayerRotation(int rot) {
        transform.eulerAngles = new Vector3(0, rot, 0);
    }
    public void SetOrderInLayer(int i) {
        _spriteRender.sortingOrder = i;
    }
    public void AddHoldItem(string name) {


        _holdItems.Add(name);
        OnItemAdd(this, EventArgs.Empty);//分發事件
        for (int i = 0; i < _holdItems.Count; i++)
        {
            Debug.Log(_holdItems[i].ToString());
        }
    }
    public void DeleteHoldItem(string name)
    {
        _holdItems.Remove(name);
    }
    public List<string> HoldItems
    {
        get { return _holdItems; }

    }
}