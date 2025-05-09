using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingController : SingleTon<PathFindingController>
{

    /// <summary>
    /// 为所有未挂载寻路对象的目标挂载
    /// </summary>
    public void EachPlayerAddPathFinding()
    {
        PlayerController[] playerControllers =FindObjectsOfType<PlayerController>();
        foreach(PlayerController playerController in playerControllers)
        {
            if (!playerController.GetComponent<PathFindTarget>())
                playerController.gameObject.AddComponent<PathFindTarget>();
        }

    }
    void Awake()
    {
        CoreController.Instance.InitGameEntity += new CoreController.InitGameHandler(EachPlayerAddPathFinding);
    }

}
