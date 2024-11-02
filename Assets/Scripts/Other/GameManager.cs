using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //--------------------------------------------------------\\
    //Setting the singleton
    //--------------------------------------------------------\\
    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public static GameManager Instance { get; private set; }
    //--------------------------------------------------------\\
    //Singleton has been set
    //--------------------------------------------------------\\
}
