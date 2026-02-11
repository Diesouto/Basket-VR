using UnityEngine;

public class LoaderCallback : MonoBehaviour
{
    bool isFirstUpdate = true;

    void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;

            Loader.LoaderCallback();
        }
    }
}
