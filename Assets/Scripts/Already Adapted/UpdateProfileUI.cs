using UnityEngine;
using TMPro;

public class UpdateProfileUI : MonoBehaviour
{

    public TMP_Text username;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string name_ = UserDataManager.userInstance.currentUserData.username;
        updateProfile(name_);
    }

    public void updateProfile(string name)
    {
        username.text = name;
    }
}
