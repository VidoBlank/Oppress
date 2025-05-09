using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreController : SingleTon<CoreController>
{
    [Header("场景坐标")]
    public Transform scene;

    //游戏正式开始时的委托
    public delegate void InitGameHandler();
    //游戏正式开始时调用的事件，实体加载与配置加载
    public event InitGameHandler InitGameEntity;
    public event InitGameHandler InitGameConfig;

    private void OnInitGame()
    {
        //对游戏的实体进行实例化
        if (InitGameEntity != null)
            InitGameEntity();
        else
            Debug.Log("事件不存在");
        //获取基本的游戏实例，并进行后续的UI等配置
        if (InitGameConfig != null)
            InitGameConfig();
        else
            Debug.Log("事件不存在");
    }


    void Start()
    {
        scene = transform.Find("场景");
        OnInitGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
