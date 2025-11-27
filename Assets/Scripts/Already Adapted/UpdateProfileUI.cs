using UnityEngine;
using TMPro;

public class UpdateProfileUI : MonoBehaviour
{

    public TMP_Text username;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void updateProfile(string name)
    {
        username.text = name;
    }
}
