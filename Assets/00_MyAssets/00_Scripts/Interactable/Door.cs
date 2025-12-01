using UnityEngine;

public class Door : MonoBehaviour
{
    public static Door Instance;
    public ObjectForeground objectForeground;

    private void Awake()
    {
        Instance = this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectForeground.textToShow1 = "Door";
            objectForeground.textToShow2 = "Door";
            objectForeground.showImage = true;
            objectForeground.imageType = 2;
            objectForeground.spriteKey = "Door";
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectForeground.textToShow1 = "Boat";
            objectForeground.textToShow2 = "Boat";
            objectForeground.imageType = 2;
            objectForeground.showImage = false;
        }
    }
}