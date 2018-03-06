﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class InteractiveItem : MonoBehaviour {
    //item tag要設置
    public string itemName;
    Player player;
    public float interactiveDistance;
    bool _canInteractive;
    public event EventHandler OnItemClicked;
    public float walkSpeed = 0.2f;
    [SerializeField]
    bool isHideSprite;
    SpriteRenderer _spriteRender;
    [SerializeField]
    bool canPick;
    [SerializeField]
    bool canTalk;
    private void Start()
    {
        player = GameManager.game.Player;
        //TODO: to be improved
        
        _spriteRender = GetComponent<SpriteRenderer>();
        if (isHideSprite)
        {
            _spriteRender.enabled = false;
        }
    }
    private void Update()
    {
        if(isHideSprite) displaySprite();
    }

    public bool CanInteractive {
        get { return _canInteractive; }
        set { _canInteractive = value; }
    }

    private void OnMouseDown()
    {
       
        if (_canInteractive) {
            if (OnItemClicked != null)
            {
                OnItemClicked(this, EventArgs.Empty);//分發事件
            }
            if (itemName == "2to3" || itemName =="3to2" || itemName =="2to1" || itemName =="1to2")
            {
                //矯正誤差，讓玩家移動到定點再開始播動畫
                Vector3 pointPos = transform.GetChild(0).position; pointPos.y = player.transform.position.y; pointPos.z = 0;
                StartCoroutine(gotoPoint(Player.PlayerState.offset, pointPos, itemName, walkSpeed));         
            }
            if(itemName == "Smain" || itemName =="Sbalcony" || itemName == "SredRoom" || itemName =="SblueRoom")
            {
                //TODO: go to door animation 

                //save player hold item
                SaveData._data.player.itemName.Clear();//清空
                foreach (string _item in GameManager.game.Player.HoldItems)
                {
                    SaveData._data.player.itemName.Add(_item) ;            
                }
                foreach (string _item in SaveData._data.player.itemName)//Debug
                {
                    Debug.Log(_item);
                }
                //save scene item    
                SaveData.SceneInfo roomInfo = SaveData._data.getRoomInfo(SaveData._data.nowScene);
                roomInfo.itemName = new String[GameObject.FindGameObjectsWithTag("item").Length];      
                for (int i = 0; i < GameObject.FindGameObjectsWithTag("item").Length; i++) roomInfo.itemName[i] = GameObject.FindGameObjectsWithTag("item")[i].name;
                SaveData._data.setRoomInfo(SaveData._data.nowScene, roomInfo);
                GameManager.game.changeSceneWithFade(itemName);
            }
            if(itemName == "SstorageRoom")
            {
                //TODO: go to door ani
                FindObjectOfType<Storage>().goIn(true);
            }
            else if(itemName == "StorageInDoor")
            {
                GameManager.game.Player.SetPlayerState(2);
                GetComponent<Storage>().goOut(false);
            }
            if (itemName == "Sout")
            {//結局
                GameManager.game.changeSceneWithFade(itemName);
            }
            if (itemName == "mouse") {//特例
                GetComponent<SpriteRenderer>().enabled = true;
                Destroy(GameObject.Find("cheese_02_target"));
            }
            if (canPick)
            {
                GameManager.game.Player.Playerstate = Player.PlayerState.pick;
                SoundManager.sound.playOne(SoundManager.sound.playerse.pick);              
                Vector3 pointPos = Camera.main.transform.position; pointPos.z = 0.0f;
                StartCoroutine(itemGotoPoint(pointPos)); 
            
            }
            if (canTalk) {
                 SetTalk();
            }
        }
    }
    public void SetSpecialTalk()
    {
        player.Playerstate = Player.PlayerState.talk;
        SaveData.CharsInfo _charInfo = SaveData._data.getCharInfo(itemName);
        if (_charInfo.talkStatus == SaveData.CharsInfo.TalkStatus.firstTalk) { GameManager.game.SetTalk("yellow", 51); }
        else if(_charInfo.talkStatus == SaveData.CharsInfo.TalkStatus.premissionNotComplete) { GameManager.game.SetTalk("yellow", 52); }
        else if(_charInfo.talkStatus == SaveData.CharsInfo.TalkStatus.missionComplete) { GameManager.game.SetTalk("yellow", 53); }
        GameManager.game.Setactive(GameManager.game.TalkUI, true);
    }
    public void SetTalk() {
        player.Playerstate = Player.PlayerState.talk;
        //load talk data
        SaveData.CharsInfo _charInfo = new SaveData.CharsInfo();
        Debug.Log(_charInfo.talkStatus.ToString());
        if (SaveData._data.tutorialEnd) {
            _charInfo = SaveData._data.getCharInfo(itemName);
        }else
        {
            _charInfo.talkStatus = SaveData.CharsInfo.TalkStatus.firstTalk;
            _charInfo.talkNum = 1;
            _charInfo.charTalkFirst = true;
        }
        if (_charInfo.talkStatus == SaveData.CharsInfo.TalkStatus.premissionNotComplete) { SetSpecialTalk(); }
        else
        {
            Debug.Log(_charInfo.name + " , " + _charInfo.talkNum + ", " + _charInfo.talkStatus.ToString() + ", " + _charInfo.charTalkFirst);
            if (itemName == "tutorial" || !_charInfo.charTalkFirst) { GameManager.game.SetTalk("yellow", _charInfo.talkNum); }//player talk first
            else GameManager.game.SetTalk(itemName, _charInfo.talkNum);//item talk first

            GameManager.game.Setactive(GameManager.game.TalkUI, true);
        }
    }
    //item move to point
    IEnumerator itemGotoPoint(Vector3 point)
    {
        _canInteractive = false;
        interactiveDistance = 0;
        _spriteRender.sortingOrder = 9;

        if (canPick)
        {
            player.AddHoldItem(itemName);
            StartCoroutine(GameManager.game.fadeInOut(Camera.main.GetComponentInChildren<SpriteRenderer>(), 0.08f));
        }
        else StartCoroutine(GameManager.game.fadeInOut(Camera.main.GetComponentInChildren<SpriteRenderer>(), -0.18f));
        while (transform.position != point) {
            transform.position = Vector3.MoveTowards(transform.position, point, 10.0f * Time.deltaTime);
            yield return null;
        }
       if(canPick) yield return new WaitForSeconds(1.0f);
        afterItemGotoPoint();
    }
    void afterItemGotoPoint()
    {
        if (canPick)
        {
            canPick = false;        
            Vector3 pointPos = FindObjectOfType<BagUI>().transform.GetChild(player.HoldItems.Count-1).position;
            pointPos = Camera.main.ScreenToWorldPoint(pointPos); pointPos.z = 0.0f;        
            StartCoroutine(itemGotoPoint(pointPos));
        }
        else
        {
            player.Playerstate = Player.PlayerState.idle;
            player.OnItemChanged();
            Destroy(this.gameObject);
          
        }
      
    }
    private void OnDestroy()
    {
        GameManager.game.refindItem();//重新統物品數量
    }
    //player go to special point
    IEnumerator gotoPoint(Player.PlayerState state,Vector3 point,string name, float speed) {
        _canInteractive = false;
        player.Playerstate = state;
        if (point.x - player.transform.position.x < 0) player.SetPlayerRotation(180);
        else player.SetPlayerRotation(0);
        while (player.transform.position.x != point.x)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, point, speed * Time.deltaTime);
            yield return null;   
        }
        //
        afterGoToPoint(name);
       
    }
    void afterGoToPoint(string name) {
        if (name == "2to3" || name == "3to2" || name == "1to2" || name == "2to1")
        {
            if (name == "2to3" || name == "1to2") player.Playerstate = Player.PlayerState.up;//變換狀態
            else player.Playerstate = Player.PlayerState.down;
            if (name == "2to3" || name =="2to1") player.SetPlayerRotation(0);//變換腳色上樓移動方向動畫(朝右)
            else player.SetPlayerRotation(180);//朝左
            player.SetOrderInLayer(0);//把腳色Layer變輕
            //走樓梯
            StartCoroutine(gotoPoint(player.Playerstate, transform.GetChild(1).transform.position, transform.GetChild(1).name, walkSpeed));
        }
        else if (name == "2to3pointEnd" || name == "3to2pointEnd" || name =="1to2pointEnd" || name =="2to1pointEnd")
        {
            player.Playerstate = Player.PlayerState.idle;
            if(name == "2to1pointEnd" || name == "3to2pointEnd")player.SetOrderInLayer(2);
        }
       
    }
    //
    void changeSpriteColor() {
        _spriteRender.color = Color.gray;
    }
    //
    void displaySprite() {
        if (_canInteractive) _spriteRender.enabled = true;
        else _spriteRender.enabled = false;
    }
    //debug line
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactiveDistance);
    }

}
