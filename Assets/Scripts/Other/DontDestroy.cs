using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    [SerializeField]
    private bool DontDestroyOnLoad = true;
    private void Awake()
    {
        if (DontDestroyOnLoad == false) return;
        DontDestroyOnLoad(this);
    }
}
